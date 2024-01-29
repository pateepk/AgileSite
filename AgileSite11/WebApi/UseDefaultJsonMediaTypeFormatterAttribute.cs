using System;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;

namespace CMS.WebApi
{
    /// <summary>
    /// Controller decorated with this attribute uses <see cref="JsonMediaTypeFormatter"/> to handle JSON formatting.
    /// </summary>
    public sealed class UseDefaultJsonMediaTypeFormatterAttribute : Attribute, IControllerConfiguration
    {
        /// <summary>
        /// Initializes controller with <see cref="JsonMediaTypeFormatter"/> to handle JSON formatting.
        /// </summary>
        /// <param name="settings">The controller settings to initialize.</param>
        /// <param name="descriptor">The controller descriptor.</param>
        public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor descriptor)
        {
            settings.Formatters.Insert(0, new JsonMediaTypeFormatter());
        }
    }
}
