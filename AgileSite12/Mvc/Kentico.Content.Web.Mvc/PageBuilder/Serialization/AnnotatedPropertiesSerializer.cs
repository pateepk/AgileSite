using System;
using System.Linq;

using Kentico.Builder.Web.Mvc;
using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.AnnotationExtensions;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Class responsible for annotated properties serialization.
    /// </summary>
    internal sealed class AnnotatedPropertiesSerializer : IAnnotatedPropertiesSerializer
    {
        /// <summary>
        /// Serializes object properties that are decorated with the <see cref="EditingComponentAttribute"/> attribute to a JSON string.
        /// Properties that are not decorated are not included in the serialized JSON.
        /// </summary>
        /// <param name="obj">Object to be serialized.</param>
        /// <returns>JSON string representing annotated object properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> is <c>null</c>.</exception>
        public string Serialize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            try
            {
                var updatedProperties = obj.GetAnnotatedProperties<EditingComponentAttribute>(false)
                                           .ToDictionary(p => p.Name, p => p.GetValue(obj, null), StringComparer.OrdinalIgnoreCase);

                var data = new
                {
                    properties = updatedProperties
                };

                return JsonConvert.SerializeObject(data, GetSettings());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Object annotated properties cannot be serialized.", ex);
            }
        }


        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = SerializerHelper.GetDefaultContractResolver()
            };
        }
    }
}
