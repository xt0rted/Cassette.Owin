using Owin;

namespace Cassette.Owin.Samples.NancyRazor
{
    public class Startup
    {
        public static void Configuration(IAppBuilder app)
        {
            app.UseCassette(new CassetteOptions
            {
                RouteRoot = "/asset-route"
            });
            app.UseNancy();
        }
    }
}