using System;

namespace Cassette.Owin
{
    public class CassetteOptions
    {
        private string _routeRoot;

        public CassetteOptions()
        {
            RouteRoot = "/cassette";
        }

        public string RouteRoot
        {
            get { return _routeRoot; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (!value.StartsWith("/", StringComparison.Ordinal) || value.Length == 1)
                {
                    throw new ArgumentException("The path must start with a '/' followed by one or more characters.");
                }

                if (value.EndsWith("/", StringComparison.Ordinal))
                {
                    value = value.Substring(0, value.Length - 1);
                }

                _routeRoot = value;
            }
        }
    }
}