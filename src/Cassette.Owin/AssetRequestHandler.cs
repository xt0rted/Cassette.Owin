using System.Threading.Tasks;

using Cassette.Utilities;

using Microsoft.Owin;

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
            var originalPath = path;

            // ToDo: move the path out to a const
            path = "~" + path.Substring("/asset".Length);

            using (_bundles.GetReadLock())
            {
                Bundle bundle;
                IAsset asset;

                if (!_bundles.TryGetAssetByPath(path, out asset, out bundle))
                {
                    return context.NotFoundResult();
                }

                context.Response.ContentType = bundle.ContentType;

                var actualETag = "\"" + asset.Hash.ToHexString() + "\"";
                var givenETag = context.Request.Headers["If-None-Match"];

                if (givenETag == actualETag)
                {
                    return context.NotModifiedResult();
                }

                if (originalPath.Contains(bundle.Hash.ToHexString()))
                {
                    // ToDo: add cache headers
                    context.Response.ETag = actualETag;
                }

                return context.ReturnStream(asset.OpenStream());
            }
        }
    }
}