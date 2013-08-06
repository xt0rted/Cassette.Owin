using System.Reflection;

using Cassette.Scripts;

namespace Cassette.Owin
{
    public class DiagnosticBundleConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.AddEmbeddedResources<ScriptBundle>(
                "/Cassette.Owin.Resources",
                Assembly.GetExecutingAssembly(),
                "Cassette.Owin.Resources",

                "jquery.js",
                "knockout.js",
                "diagnostic-page.js");
        }
    }
}