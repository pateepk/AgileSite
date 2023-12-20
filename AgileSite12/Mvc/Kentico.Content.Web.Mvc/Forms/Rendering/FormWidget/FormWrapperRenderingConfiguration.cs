using System.Collections.Generic;
using System.Web;

namespace Kentico.Forms.Web.Mvc.Widgets
{
    /// <summary>
    /// Configuration describing rendering of an optional wrapping HTML element around Form builder's form.
    /// </summary>
    public class FormWrapperRenderingConfiguration
    {
        /// <summary>
        /// Defines a placeholder for wrapped elements if <see cref="CustomHtmlEnvelope"/> is used.
        /// </summary>
        /// <seealso cref="CustomHtmlEnvelope"/>
        public static string CONTENT_PLACEHOLDER => "__kentico-formrenderingconfiguration-content-placeholder";


        internal ElementRenderingConfiguration Configuration
        {
            get;
            private set;
        } = new ElementRenderingConfiguration();


        /// <summary>
        /// Gets or sets element name to be rendered.
        /// </summary>
        public string ElementName
        {
            get => Configuration.ElementName;
            set => Configuration.ElementName = value;
        }


        /// <summary>
        /// Gets or sets optional attributes that should be added to the element. An empty dictionary by default.
        /// </summary>
        public IDictionary<string, object> HtmlAttributes
        {
            get => Configuration.HtmlAttributes;
            set => Configuration.HtmlAttributes = value;
        }


        /// <summary>
        /// Gets or sets child element rendering configuration. Use this property to nest multiple wrapping elements.
        /// </summary>
        public ElementRenderingConfiguration ChildConfiguration
        {
            get => Configuration.ChildConfiguration;
            set => Configuration.ChildConfiguration = value;
        }


        /// <summary>
        /// Get or sets custom HTML envelope. Within a custom HTML code must be used <see cref="CONTENT_PLACEHOLDER"/> value.
        /// </summary>
        /// <remarks>Defined HTML in is static and is not updated after form submit.</remarks>
        /// <seealso cref="CONTENT_PLACEHOLDER"/>
        public IHtmlString CustomHtmlEnvelope { get; set; }


        /// <summary>
        /// Returns a deep copy of the configuration. All <see cref="FormWrapperRenderingConfiguration"/> based properties
        /// are recursively copied using <see cref="Copy"/>, all <see cref="IDictionary{TKey, TValue}"/>
        /// based properties are shallow copied to a new dictionary (i.e. individual dictionary items are not re-created).
        /// </summary>
        /// <returns>Returns a copy of this configuration.</returns>
        internal FormWrapperRenderingConfiguration Copy()
        {
            var result = new FormWrapperRenderingConfiguration
            {
                Configuration = Configuration?.Copy(),
                CustomHtmlEnvelope = new HtmlString(CustomHtmlEnvelope?.ToHtmlString())
            };

            return result;
        }
    }
}
