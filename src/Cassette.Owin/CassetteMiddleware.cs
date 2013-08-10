using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Owin;

using Owin;

using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Owin
{
    public class CassetteMiddleware : OwinMiddleware
    {
        internal static string StartUpTrace;

        private static readonly object Lock = new object();

        private static WebHost _host;

        private readonly ThreadLocal<IOwinContext> _contextLocal = new ThreadLocal<IOwinContext>(() => null);
        private readonly CassetteOptions _options;

        public CassetteMiddleware(OwinMiddleware next, IAppBuilder builder, CassetteOptions options)
            : base(next)
        {
            _options = options;

            var context = new OwinContext(builder.Properties);
            var token = context.Get<CancellationToken>("host.OnAppDisposing"); // ToDo: move out to a const
            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    lock (Lock)
                    {
                        if (_host != null)
                        {
                            try
                            {
                                _host.Dispose();
                            }
                            catch (Exception ex)
                            {
                                // nothing to do here...
                            }
                        }
                    }
                });
            }
        }

        public override Task Invoke(IOwinContext context)
        {
            _contextLocal.Value = context;
            lock (Lock)
            {
                if (_host == null)
                {
                    var startupTimer = Stopwatch.StartNew();
                    using (var recorder = new StartUpTraceRecorder())
                    {
                        _host = new WebHost(_options, () => _contextLocal.Value);
                        _host.Initialize();

                        Trace.Source.TraceInformation("Total time elapsed: {0}ms", startupTimer.ElapsedMilliseconds);
                        StartUpTrace = recorder.TraceOutput;
                    }
                }
            }

            _host.StoreRequestContainerInOwinContext();

            if (!context.Request.Path.StartsWith(_options.RouteRoot))
            {
                return _host.ProcessRewriteRequest(context, Next);
            }

            return _host.ProcessRequest(context, Next);
        }
    }
}