using Owin;

namespace Cassette.Owin
{
    public static class CassetteExtensions
    {
        public static IAppBuilder UseCassette(this IAppBuilder app)
        {
            return UseCassette(app, new CassetteOptions());
        }

        public static IAppBuilder UseCassette(this IAppBuilder app, CassetteOptions options)
        {
            return app.Use(typeof(CassetteMiddleware), app, options);
        }
    }
}