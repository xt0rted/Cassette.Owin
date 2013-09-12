using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Cassette.Owin.Infrastructure;

using Microsoft.Owin;

namespace Cassette.Owin
{
    internal class CassetteMiddlewareContext
    {
        private static readonly Func<InterceptMode> InterceptDetaching = () => InterceptMode.DoingNothing;

        private readonly IOwinContext _context;
        private readonly IPlaceholderTracker _placeholderTracker;
        private Stream _originalResponseBody;

        private InterceptMode _intercept;
        private bool _interceptInitialized;
        private object _interceptLock = new object();

        private Stream _rewritingStream;

        public CassetteMiddlewareContext(IOwinContext context, IPlaceholderTracker placeholderTracker)
        {
            _context = context;
            _placeholderTracker = placeholderTracker;
        }

        internal enum InterceptMode
        {
            Uninitialized,
            DoingNothing,
            RewritingStream
        }

        public void Attach()
        {
            _originalResponseBody = _context.Response.Body;
            _context.Response.Body = new SwitchingStream(this, _originalResponseBody);
        }

        public InterceptMode Intercept(bool detaching = false)
        {
            return LazyInitializer.EnsureInitialized(
                ref _intercept,
                ref _interceptInitialized,
                ref _interceptLock,
                detaching ? InterceptDetaching : InterceptOnce);
        }

        public InterceptMode InterceptOnce()
        {
            var contentType = _context.Response.ContentType;
            if (contentType != "text/html" && contentType != "application/xhtml+xml")
            {
                _rewritingStream = _originalResponseBody;
                return InterceptMode.RewritingStream;
            }

            _rewritingStream = new PlaceholderReplacingResponseStream(_originalResponseBody, _placeholderTracker);
            return InterceptMode.RewritingStream;
        }

        public void Detach()
        {
            Intercept(detaching: true);
            _context.Response.Body = _originalResponseBody;
        }

        public Stream GetTargetStream()
        {
            switch (Intercept())
            {
                case InterceptMode.DoingNothing:
                    return _originalResponseBody;

                case InterceptMode.RewritingStream:
                    return _rewritingStream;
            }

            throw new NotImplementedException();
        }

        public Task Complete()
        {
            InterceptMode interceptMode = Intercept();
            Detach();

            switch (interceptMode)
            {
                case InterceptMode.DoingNothing:
                    return Constants.CompletedTask;

                case InterceptMode.RewritingStream:
                    _rewritingStream.Close();
                    return Constants.CompletedTask;
            }

            throw new NotImplementedException();
        }

        public CatchInfoBase<Task>.CatchResult Complete(CatchInfo catchInfo)
        {
            Detach();
            return catchInfo.Throw();
        }
    }
}