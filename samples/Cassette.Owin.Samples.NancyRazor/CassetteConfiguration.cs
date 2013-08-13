using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Owin.Samples.NancyRazor
{
    public class CassetteConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<ScriptBundle>("Scripts");
            bundles.Add<StylesheetBundle>("Content");
        }
    }
}