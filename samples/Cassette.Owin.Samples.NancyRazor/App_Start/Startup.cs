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

            // in some scenarios cassette.owin offloads serving up static files to the static file middleware instead of streaming them directly
            app.UseStaticFiles();

            app.UseNancy();
        }
    }
}