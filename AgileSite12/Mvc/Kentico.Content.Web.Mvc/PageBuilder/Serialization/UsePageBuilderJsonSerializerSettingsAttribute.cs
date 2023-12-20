using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;

using Kentico.Builder.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Controller decorated with this attribute uses <see cref="CamelCasePropertyNamesContractResolver"/> to handle JSON serialization.
    /// </summary>
    internal sealed class UsePageBuilderJsonSerializerSettingsAttribute : Attribute, IControllerConfiguration
    {
        /// <summary>
        /// Initializes controller with <see cref="JsonMediaTypeFormatter"/> to handle JSON formatting.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var currentFormatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>().Single();
            controllerSettings.Formatters.Remove(currentFormatter);

            var newFormatter = new JsonMediaTypeFormatter
            {
                UseDataContractJsonSerializer = false,
                SerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = SerializerHelper.GetDefaultContractResolver()
                }
            };
            controllerSettings.Formatters.Add(newFormatter);
        }
    }
}
