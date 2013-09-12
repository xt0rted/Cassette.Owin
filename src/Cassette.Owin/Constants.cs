using System.Threading.Tasks;

namespace Cassette.Owin
{
    internal static class Constants
    {
        internal const string CacheControl = "Cache-Control";
        internal const string Expires = "Expires";
        internal const string Pragma = "Pragma";
        internal const string IfNoneMatch = "If-None-Match";

        internal const string HostappDisposingKey = "host.OnAppDisposing";
        internal const string TimestampKey = "cassette.owin.timestamp";

        internal const string TextHtml = "text/html";
        internal const string ApplicationXhtmlXml = "application/xhtml+xml";

        internal static readonly Task CompletedTask = CreateCompletedTask();

        private static Task CreateCompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}