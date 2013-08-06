using System;
using System.Diagnostics;

using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Owin
{
    public class StartUpTraceRecorder : IDisposable
    {
        private readonly TraceListener _traceListener;

        public StartUpTraceRecorder()
        {
            _traceListener = CreateTraceListener();
            Trace.Source.Listeners.Add(_traceListener);
        }

        public string TraceOutput
        {
            get
            {
                Trace.Source.Flush();
                _traceListener.Flush();
                return _traceListener.ToString();
            }
        }

        public void Dispose()
        {
            Trace.Source.Listeners.Remove(_traceListener);
        }

        private StringBuilderTraceListener CreateTraceListener()
        {
            return new StringBuilderTraceListener
            {
                Filter = new EventTypeFilter(SourceLevels.All)
            };
        }
    }
}