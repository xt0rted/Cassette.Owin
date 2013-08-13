using System;

namespace Cassette.Owin.Samples.NancyRazor
{
    public class CassetteFileAuthorizationConfiguration : IConfiguration<IFileAccessAuthorization>
    {
        public void Configure(IFileAccessAuthorization authorization)
        {
            authorization.AllowAccess(path => path.StartsWith("/Content", StringComparison.OrdinalIgnoreCase));
        }
    }
}