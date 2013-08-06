using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Owin;

namespace Cassette.Owin
{
    internal static class OwinRequestHelpers
    {
        private const string FormDictionaryKey = "Microsoft.Owin.Form#dictionary";

        public static IFormCollection Form(this IOwinRequest request)
        {
            var formDictionary = request.Get<IDictionary<string, string[]>>(FormDictionaryKey);
            if (formDictionary == null)
            {
                IFormCollection formCollection = request.ReadForm();

                formDictionary = formCollection.ToDictionary(f => f.Key, f => f.Value);

                request.Set(FormDictionaryKey, formDictionary);

                return formCollection;
            }

            return new FormCollection(formDictionary);
        }

        private static IFormCollection ReadForm(this IOwinRequest request)
        {
            using (var reader = new StreamReader(request.Body))
            {
                var text = reader.ReadToEnd();
                return Microsoft.Owin.Helpers.WebHelpers.ParseForm(text);
            }
        }
    }
}