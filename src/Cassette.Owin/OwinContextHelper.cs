using System.IO;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;

namespace Cassette.Owin
{
    internal static class OwinContextHelper
    {
        public static bool IsLocal(this IOwinContext context)
        {
            return context.Get<bool>("server.IsLocal");
        }

        public static Task NotFoundResult(this IOwinContext context)
        {
            context.Response.StatusCode = 404;
            return TaskHelpers.Completed();
        }

        public static Task NotModifiedResult(this IOwinContext context)
        {
            context.Response.StatusCode = 304;
            return TaskHelpers.Completed();
        }

        public static Task ReturnStream(this IOwinContext context, Stream stream)
        {
            var copyOperation = new StreamCopyOperation(stream, context.Response.Body, null, context.Request.CallCancelled);
            Task task = copyOperation.Start();
            task.ContinueWith(resultTask => stream.Close(), TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }
    }
}