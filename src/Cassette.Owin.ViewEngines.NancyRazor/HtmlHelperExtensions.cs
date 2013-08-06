using System.Collections.Generic;

using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

using Nancy.ViewEngines.Razor;

namespace Cassette.Owin.ViewEngines.NancyRazor
{
    public static class HtmlHelperExtensions
    {
        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            if (env.TryGetValue(key, out value))
            {
                return (T)value;
            }

            return default(T);
        }

        private static IReferenceBuilder GetReferenceBuilder<T>(HtmlHelpers<T> helper)
        {
            // we're not using the const from Nancy.Owin just to reduce the project dependencies
            var environment = Get<IDictionary<string, object>>(helper.RenderContext.Context.Items, "OWIN_REQUEST_ENVIRONMENT");

            return Get<IReferenceBuilder>(environment, "cassette.requestReferenceBuilder"); // ToDo: move to a const in Cassette.Owin
        }

        public static void Reference<T>(this HtmlHelpers<T> helper, string bundle, string name = null)
        {
            var referenceBuilder = GetReferenceBuilder(helper);
            referenceBuilder.Reference(bundle, name);
        }

        public static IHtmlString Render<TBundle, TModel>(this HtmlHelpers<TModel> helper, string pageLocation) where TBundle : Bundle
        {
            var referenceBuilder = GetReferenceBuilder(helper);
            return new NonEncodedHtmlString(referenceBuilder.Render<TBundle>(pageLocation));
        }

        public static IHtmlString RenderScripts<T>(this HtmlHelpers<T> helper, string pageLocation = null)
        {
            return Render<ScriptBundle, T>(helper, pageLocation);
        }

        public static IHtmlString RenderStylesheets<T>(this HtmlHelpers<T> helper, string pageLocation = null)
        {
            return Render<StylesheetBundle, T>(helper, pageLocation);
        }

        public static IHtmlString RenderHtmlTemplates<T>(this HtmlHelpers<T> helper, string pageLocation = null)
        {
            return Render<HtmlTemplateBundle, T>(helper, pageLocation);
        }
    }
}