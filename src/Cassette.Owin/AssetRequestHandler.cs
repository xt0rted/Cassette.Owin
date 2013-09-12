using System.Threading.Tasks;

using Cassette.Utilities;

using Microsoft.Owin;

using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Owin
{
    public class AssetRequestHandler : ICassetteRequestHandler
    {
        private readonly BundleCollection _bundles;

        public AssetRequestHandler(BundleCollection bundles)
        {
            _bundles = bundles;
        }

        public Task ProcessRequest(IOwinContext context, string path)
        {
            Trace.Source.TraceInformation("Handling asset request for path \"{0}\"", path);

            var request = context.Request;
            var response = context.Response;
            var originalPath = path;

            // ToDo: move the path out to a const
            path = "~" + path.Substring("/asset".Length);

            using (_bundles.GetReadLock())
            {
                Bundle bundle;
                IAsset asset;

                if (!_bundles.TryGetAssetByPath(path, out asset, out bundle))
                {
                    Trace.Source.TraceInformation("Bundle asset not found with path \"{0}\"", path);
                    return context.NotFoundResult();
                }

                response.ContentType = bundle.ContentType;

                var actualETag = "\"" + asset.Hash.ToHexString() + "\"";

                if (originalPath.Contains(bundle.Hash.ToHexString()))
                {
                    response.CacheForOneYear(actualETag);
                }
                else
                {
                    response.DoNotCache();
                }

                var givenETag = request.Headers[Constants.IfNoneMatch];
                if (givenETag == actualETag)
                {
                    return context.NotModifiedResult();
                }

                return context.ReturnStream(asset.OpenStream());
            }
        }
    }
}