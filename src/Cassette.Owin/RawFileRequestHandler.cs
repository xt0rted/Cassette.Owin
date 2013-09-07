using System.Threading.Tasks;

using Cassette.Utilities;

using Microsoft.Owin;

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
            var givenETag = context.Request.Headers["If-None-Match"];

            if (originalPath.Contains(hash))
            {
                // ToDo: set cache headers
                context.Response.ETag = actualETag;
            }
            else
            {
                // ToDo: set no cache headers
            }

            if (givenETag == actualETag)
            {
                return context.NotModifiedResult();
            }

            // to serve up the actual file from disk we rewrite the path and pass it off to the
            // next piece of middleware in hopes of the static file being handled down the line
            context.Request.Path = new PathString(path);
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