using System.Linq;
using System.Threading.Tasks;

using Cassette.Utilities;

using Microsoft.Owin;

namespace Cassette.Owin
{
    public class BundleRequestHandler<TBundle> : ICassetteRequestHandler where TBundle : Bundle
    {
        private readonly BundleCollection _bundles;

        public BundleRequestHandler(BundleCollection bundles)
        {
            _bundles = bundles;
        }

        public Task ProcessRequest(IOwinContext context, string path)
        {
            var originalPath = path;

            // path == "/00000000000000000000000000000000/the/asset/bundle"
            path = "~" + path.Substring(path.IndexOf('/', 32));

            using (_bundles.GetReadLock())
            {
                var bundle = _bundles.FindBundlesContainingPath(path)
                                     .OfType<TBundle>()
                                     .FirstOrDefault();

                if (bundle == null)
                {
                    return context.NotFoundResult();
                }

                context.Response.ContentType = bundle.ContentType;

                var actualETag = "\"" + bundle.Hash.ToHexString() + "\"";
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

                return context.ReturnStream(bundle.OpenStream());
            }
        }
    }
}