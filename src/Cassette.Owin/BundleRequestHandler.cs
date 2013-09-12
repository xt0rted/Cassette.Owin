using System.Linq;
using System.Threading.Tasks;

using Cassette.Utilities;

using Microsoft.Owin;

using Trace = Cassette.Diagnostics.Trace;

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
            Trace.Source.TraceInformation("Handling bundle request for path \"{0}\"", path);

            var request = context.Request;
            var response = context.Response;
            var originalPath = path;

            // path == "/00000000000000000000000000000000/the/asset/bundle"
            path = "~" + path.Substring(path.IndexOf('/', 1));

            using (_bundles.GetReadLock())
            {
                var bundle = _bundles.FindBundlesContainingPath(path)
                                     .OfType<TBundle>()
                                     .FirstOrDefault();

                if (bundle == null)
                {
                    return context.NotFoundResult();
                }

                response.ContentType = bundle.ContentType;

                var actualETag = "\"" + bundle.Hash.ToHexString() + "\"";

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

                return context.ReturnStream(bundle.OpenStream());
            }
        }
    }
}