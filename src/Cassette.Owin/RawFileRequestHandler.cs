using System.Threading.Tasks;

using Cassette.Utilities;

using Microsoft.Owin;

using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Owin
{
    public class RawFileRequestHandler : ICassetteRequestHandler
    {
        private readonly IFileAccessAuthorization _fileAccessAuthorization;
        private readonly IFileContentHasher _fileContentHasher;

        public RawFileRequestHandler(IFileAccessAuthorization fileAccessAuthorization, IFileContentHasher fileContentHasher)
        {
            _fileAccessAuthorization = fileAccessAuthorization;
            _fileContentHasher = fileContentHasher;
        }

        public Task ProcessRequest(IOwinContext context, string path)
        {
            Trace.Source.TraceInformation("Handling asset request for path \"{0}\"", path);

            var request = context.Request;
            var response = context.Response;
            var originalPath = path;

            path = RemoveHashFromPath(path);

            if (!_fileAccessAuthorization.CanAccess(path))
            {
                return context.NotFoundResult();
            }

            // if there's a leading slash the underlying code combines the paths wrong and
            // instead of d:\sites\awesome-site\content\logo.png you get d:\content\logo.png
            var pathToHash = path.Substring(1);
            var hash = _fileContentHasher.Hash(pathToHash).ToHexString();

            var actualETag = "\"" + hash + "\"";

            if (originalPath.Contains(hash))
            {
                response.CacheForOneYear(actualETag);
                // ToDo: add sliding expiration support?
            }
            else
            {
                response.DoNotCache();
                response.Headers.AddCacheControlNoStore();
            }

            var givenETag = request.Headers[Constants.IfNoneMatch];
            if (givenETag == actualETag)
            {
                return context.NotModifiedResult();
            }

            // to serve up the actual file from disk we rewrite the path and pass it off to the
            // next piece of middleware in hopes of the static file being handled down the line
            request.Path = new PathString(path);
            return null;
        }

        private string RemoveHashFromPath(string path)
        {
            var periodIndex = path.LastIndexOf('.');
            if (periodIndex >= 0)
            {
                var extension = path.Substring(periodIndex);
                var name = path.Substring(0, periodIndex);
                var hyphenIndex = name.LastIndexOf('-');
                if (hyphenIndex >= 0)
                {
                    name = name.Substring(0, hyphenIndex);
                    return name + extension;
                }

                return path;
            }
            else
            {
                var hyphenIndex = path.LastIndexOf('-');
                if (hyphenIndex >= 0)
                {
                    return path.Substring(0, hyphenIndex);
                }

                return path;
            }
        }
    }
}