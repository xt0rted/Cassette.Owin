using System.IO;
using System.Threading;

namespace Cassette.Owin.Infrastructure
{
    public class SwitchingStream : DelegatingStream
    {
        private readonly CassetteMiddlewareContext _cassetteMiddlewareContext;
        private readonly Stream _originalBody;

        private Stream _targetStream;
        private bool _targetStreamInitialized;
        private object _targetStreamLock = new object();

        internal SwitchingStream(CassetteMiddlewareContext cassetteMiddlewareContext, Stream originalBody)
        {
            _cassetteMiddlewareContext = cassetteMiddlewareContext;
            _originalBody = originalBody;
        }

        protected override Stream TargetStream
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _targetStream,
                    ref _targetStreamInitialized,
                    ref _targetStreamLock,
                    _cassetteMiddlewareContext.GetTargetStream);
            }
        }
    }
}