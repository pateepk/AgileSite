using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;


namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Controller decorated with this attribute uses settings returned by <see cref="FormBuilderConfigurationSerializer.GetSettings()"/> to handle JSON serialization.
    /// </summary>
    internal sealed class UseFormBuilderJsonSerializerSettingsAttribute : Attribute, IControllerConfiguration
    {
        /// <summary>
        /// Initializes controller with settings returned by <see cref="FormBuilderConfigurationSerializer.GetSettings()"/> to handle JSON serialization.
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
                SerializerSettings = FormBuilderConfigurationSerializer.GetSettings()
            };
            controllerSettings.Formatters.Add(newFormatter);
        }
    }
}
