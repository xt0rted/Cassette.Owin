using System;

using Microsoft.Owin;

namespace Cassette.Owin
{
    internal static class Helpers
    {
        internal static void CacheForOneYear(this IOwinResponse response, string eTag)
        {
            response.Headers.Set(Constants.CacheControl, "public");
            response.Headers.Append(Constants.CacheControl, "max-age=" + (long)TimeSpan.FromDays(365).TotalSeconds);
            response.Expires = response.Get<DateTime>(Constants.TimestampKey).AddYears(1);
            response.ETag = eTag;
        }

        internal static void DoNotCache(this IOwinResponse response)
        {
            response.Headers.Set(Constants.Pragma, "no-cache");
            response.Headers.Set(Constants.CacheControl, "no-cache");
            response.Headers.Set(Constants.Expires, "-1");
        }

        internal static void AddCacheControlNoStore(this IHeaderDictionary headers)
        {
            headers.Append(Constants.CacheControl, "no-store");
        }
    }
}