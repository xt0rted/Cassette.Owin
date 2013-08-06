using System.Diagnostics;
using System.Text;

namespace Cassette.Owin
{
    public class StringBuilderTraceListener : TraceListener
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public override void Write(string message)
        {
            _builder.Append(message);
        }

        public override void WriteLine(string message)
        {
            _builder.AppendLine(message);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}