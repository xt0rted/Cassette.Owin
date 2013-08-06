using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

using Microsoft.Owin;

namespace Cassette.Owin
{
    public class DiagnosticRequestHandler : ICassetteRequestHandler
    {
        private static readonly ConcurrentDictionary<string, string> ResourceCache = new ConcurrentDictionary<string, string>();

        private readonly BundleCollection _bundles;
        private readonly CassetteSettings _settings;
        private readonly IBundleCacheRebuilder _bundleCacheRebuilder;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Func<IReferenceBuilder> _getReferenceBuilder;
        private readonly IUrlGenerator _urlGenerator;

        public DiagnosticRequestHandler(BundleCollection bundles, CassetteSettings settings, IBundleCacheRebuilder bundleCacheRebuilder, IJsonSerializer jsonSerializer, Func<IReferenceBuilder> getReferenceBuilder, IUrlGenerator urlGenerator)
        {
            _bundles = bundles;
            _settings = settings;
            _bundleCacheRebuilder = bundleCacheRebuilder;
            _jsonSerializer = jsonSerializer;
            _getReferenceBuilder = getReferenceBuilder;
            _urlGenerator = urlGenerator;
        }

        private static string GetResource(string filename)
        {
            filename = filename.ToLowerInvariant();
            string result;

            if (!ResourceCache.TryGetValue(filename, out result))
            {
                using (var stream = typeof(CassetteMiddleware).Assembly.GetManifestResourceStream("Cassette.Owin.Resources." + filename))
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                ResourceCache[filename] = result;
            }

            return result;
        }

        public Task ProcessRequest(IOwinContext context, string path)
        {
            if (!context.IsLocal() && !_settings.AllowRemoteDiagnostics)
            {
                return context.NotFoundResult();
            }

            if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                var form = context.Request.Form();
                if (form["action"] == "rebuild-cache")
                {
                    _bundleCacheRebuilder.RebuildCache();
                    return TaskHelpers.Completed();
                }
            }

            _getReferenceBuilder().Reference(new PageDataScriptBundle("data", PageData(), _jsonSerializer));
            _getReferenceBuilder().Reference("/Cassette.Owin.Resources");

            var html = GetResource("hud.htm");
            var scripts = _getReferenceBuilder().Render<ScriptBundle>(null);

            html = html.Replace("$scripts$", scripts);
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(html);
        }

        private object PageData()
        {
            using (_bundles.GetReadLock())
            {
                var scripts = _bundles.OfType<ScriptBundle>();
                var stylesheets = _bundles.OfType<StylesheetBundle>();
                var htmlTemplates = _bundles.OfType<HtmlTemplateBundle>();

                var data = new
                {
                    Scripts = scripts.Select(ScriptData),
                    Stylesheets = stylesheets.Select(StylesheetData),
                    HtmlTemplates = htmlTemplates.Select(HtmlTemplateData),
                    StartupTrace = CassetteMiddleware.StartUpTrace,
                    Cassette = new
                    {
                        Version = new AssemblyName(typeof(Bundle).Assembly.FullName).Version.ToString(),
                        CacheDirectory = GetCacheDirectory(_settings),
                        SourceDirectory = GetSourceDirectory(_settings),
                        IsHtmlRewritingEnabled = _settings.IsHtmlRewritingEnabled,
                        IsDebuggingEnabled = _settings.IsDebuggingEnabled
                    }
                };
                return data;
            }
        }

        private static string GetSourceDirectory(CassetteSettings settings)
        {
            if (settings.SourceDirectory == null)
            {
                return "(none)";
            }

            return settings.SourceDirectory.FullPath + "(" + settings.SourceDirectory.GetType().FullName + ")";
        }

        private static string GetCacheDirectory(CassetteSettings settings)
        {
            if (settings.CacheDirectory == null)
            {
                return "(none)";
            }

            return settings.CacheDirectory.FullPath + "(" + settings.CacheDirectory.GetType().FullName + ")";
        }

        private object HtmlTemplateData(HtmlTemplateBundle htmlTemplate)
        {
            return new
            {
                Path = htmlTemplate.Path,
                Url = BundleUrl(htmlTemplate),
                Assets = AssetPaths(htmlTemplate),
                References = htmlTemplate.References,
                Size = BundleSize(htmlTemplate)
            };
        }

        private object StylesheetData(StylesheetBundle stylesheet)
        {
            return new
            {
                Path = stylesheet.Path,
                Url = BundleUrl(stylesheet),
                Media = stylesheet.Media,
                Condition = stylesheet.Condition,
                Assets = AssetPaths(stylesheet),
                References = stylesheet.References,
                Size = BundleSize(stylesheet)
            };
        }

        private object ScriptData(ScriptBundle script)
        {
            return new
            {
                Path = script.Path,
                Url = BundleUrl(script),
                Condition = script.Condition,
                Assets = AssetPaths(script),
                References = script.References,
                Size = BundleSize(script)
            };
        }

        private string BundleUrl(Bundle bundle)
        {
            var external = bundle as IExternalBundle;
            if (external == null)
            {
                return _urlGenerator.CreateBundleUrl(bundle);
            }

            return external.ExternalUrl;
        }

        private long BundleSize(Bundle bundle)
        {
            if (_settings.IsDebuggingEnabled)
            {
                return -1;
            }

            if (bundle.Assets.Any())
            {
                using (var s = bundle.OpenStream())
                {
                    return s.Length;
                }
            }

            return -1;
        }

        private IEnumerable<AssetLink> AssetPaths(Bundle bundle)
        {
            var generateUrls = _settings.IsDebuggingEnabled;
            var visitor = generateUrls ? new AssetLinkCreator(_urlGenerator) : new AssetLinkCreator();
            bundle.Accept(visitor);
            return visitor.AssetLinks;
        }

        private class AssetLink
        {
            public string Path { get; set; }
            public string Url { get; set; }
        }

        private class AssetLinkCreator : IBundleVisitor
        {
            private readonly List<AssetLink> _assetLinks = new List<AssetLink>();
            private readonly IUrlGenerator _urlGenerator;

            private string _bundlePath;

            public AssetLinkCreator()
            {
            }

            public AssetLinkCreator(IUrlGenerator urlGenerator)
            {
                _urlGenerator = urlGenerator;
            }

            public List<AssetLink> AssetLinks
            {
                get { return _assetLinks; }
            }

            public void Visit(Bundle bundle)
            {
                _bundlePath = bundle.Path;
            }

            public void Visit(IAsset asset)
            {
                var path = asset.Path;
                if (path.Length > _bundlePath.Length && path.StartsWith(_bundlePath, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(_bundlePath.Length + 1);
                }

                var url = _urlGenerator != null ? _urlGenerator.CreateAssetUrl(asset) : null;
                AssetLinks.Add(new AssetLink { Path = path, Url = url });
            }
        }
    }
}