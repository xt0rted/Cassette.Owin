using Nancy;

namespace Cassette.Owin.Samples.NancyRazor
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = _ => View["Index"];

            Get["/about"] = _ => View["About"];
        }
    }
}