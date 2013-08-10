using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

using Microsoft.Owin;

namespace Cassette.Owin
{
    public class WebHost : HostBase
    {
        private readonly CassetteOptions _options;
        private readonly Func<IOwinContext> _getContext;

        public WebHost(CassetteOptions options, Func<IOwinContext> getContext)
        {
            _options = options;
            _getContext = getContext;
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return true; }
        }

        protected override void ConfigureContainer()
        {
            Container.Register((c, p) => _getContext());

            Container.Register<ICassetteRequestHandler, DiagnosticRequestHandler>("DiagnosticRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, AssetRequestHandler>("AssetRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, BundleRequestHandler<ScriptBundle>>("ScriptBundleRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, BundleRequestHandler<StylesheetBundle>>("StylesheetBundleRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, BundleRequestHandler<HtmlTemplateBundle>>("HtmlTemplateBundleRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());
            Container.Register<ICassetteRequestHandler, RawFileRequestHandler>("RawFileRequestHandler").AsPerRequestSingleton(CreateRequestLifetimeProvider());

            base.ConfigureContainer();

            Container.Register<IUrlGenerator>((c, n) => new UrlGenerator(c.Resolve<IUrlModifier>(), c.Resolve<CassetteSettings>().SourceDirectory, _options.RouteRoot + "/"));
            Container.Register<IUrlModifier>((c, n) => new VirtualDirectoryPrepender("/"));
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new WebHostSettingsConfiguration();
        }

        protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider CreateRequestLifetimeProvider()
        {
            return new OwinContextLifetimeProvider(() => Container.Resolve<IOwinContext>());
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            // ToDo: look into letting the options class supply an override method for this
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public Task ProcessRequest(IOwinContext context, OwinMiddleware next)
        {
            var path = context.Request.Path.Substring(_options.RouteRoot.Length);

            if (string.IsNullOrWhiteSpace(path))
            {
                var handler = Container.Resolve<ICassetteRequestHandler>("DiagnosticRequestHandler");

                var placeholderTracker = Container.Resolve<IPlaceholderTracker>();

                var cmc = new CassetteMiddlewareContext(context, placeholderTracker);
                cmc.Attach();

                return handler.ProcessRequest(context, path)
                              .Then((Func<Task>)cmc.Complete)
                              .Catch(cmc.Complete);
            }

            // ToDo: move path to const
            if (path.StartsWith("/asset"))
            {
                var handler = Container.Resolve<ICassetteRequestHandler>("AssetRequestHandler");
                return handler.ProcessRequest(context, path);
            }

            // ToDo: move path to const
            if (path.StartsWith("/script", StringComparison.OrdinalIgnoreCase))
            {
                var handler = Container.Resolve<ICassetteRequestHandler>("ScriptBundleRequestHandler");
                return handler.ProcessRequest(context, path.Substring("/script".Length));
            }

            // ToDo: move path to const
            if (path.StartsWith("/stylesheet", StringComparison.OrdinalIgnoreCase))
            {
                var handler = Container.Resolve<ICassetteRequestHandler>("StylesheetBundleRequestHandler");
                return handler.ProcessRequest(context, path.Substring("/stylesheet".Length));
            }

            // ToDo: move path to const
            if (path.StartsWith("/htmltemplate", StringComparison.OrdinalIgnoreCase))
            {
                var handler = Container.Resolve<ICassetteRequestHandler>("HtmlTemplateBundleRequestHandler");
                return handler.ProcessRequest(context, path.Substring("/htmltemplate".Length));
            }

            // ToDo: move path to const
            if (path.StartsWith("/file", StringComparison.OrdinalIgnoreCase))
            {
                var handler = Container.Resolve<ICassetteRequestHandler>("RawFileRequestHandler");
                var resultTask = handler.ProcessRequest(context, path.Substring("/file".Length));
                return resultTask ?? next.Invoke(context);
            }

            return context.NotFoundResult();
        }

        public Task ProcessRewriteRequest(IOwinContext context, OwinMiddleware next)
        {
            var placeholderTracker = Container.Resolve<IPlaceholderTracker>();

            var cmc = new CassetteMiddlewareContext(context, placeholderTracker);
            cmc.Attach();

            return next.Invoke(context)
                       .Then((Func<Task>)cmc.Complete)
                       .Catch(cmc.Complete);
        }

        public void StoreRequestContainerInOwinContext()
        {
            var context = _getContext();
            // ToDo: move these values out into consts
            context.Set("cassette.requestContainer", Container.GetChildContainer());
            context.Set("cassette.requestReferenceBuilder", Container.Resolve<IReferenceBuilder>());
        }

    }
}