using System;

using Cassette.TinyIoC;

using Microsoft.Owin;

namespace Cassette.Owin
{
    public class OwinContextLifetimeProvider : RequestLifetimeProviderBase
    {
        private readonly Func<IOwinContext> _getOwinContext;
        // ToDo: look into changing this to something like cassette.owin.{0}
        private readonly string _keyName = string.Format("TinyIoC.OwinContext.{0}", Guid.NewGuid());

        public OwinContextLifetimeProvider(Func<IOwinContext> getOwinContext)
        {
            _getOwinContext = getOwinContext;
        }

        public override object GetObject()
        {
            var context = _getOwinContext();
            return context.Get<object>(_keyName);
        }

        public override void SetObject(object value)
        {
            var context = _getOwinContext();
            context.Environment[_keyName] = value;
        }
    }
}