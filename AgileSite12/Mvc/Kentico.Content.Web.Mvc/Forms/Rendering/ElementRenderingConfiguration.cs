using System.Collections.Generic;
using System.Web;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Configuration describing rendering of an optional wrapping HTML element around Form builder's element or custom HTML code.
    /// </summary>
    public class ElementRenderingConfiguration
    {
        /// <summary>
        /// Defines a placeholder for wrapped elements if <see cref="CustomHtml"/> is used.
        /// </summary>
        /// <seealso cref="CustomHtml"/>
        public static string CONTENT_PLACEHOLDER => "__kentico-elementrenderingconfiguration-content-placeholder";


        /// <summary>
        /// Gets or sets element name to be rendered.
        /// </summary>
        /// <remarks>Property value is ignored if <see cref="CustomHtml"/> value is set.</remarks>
        /// <seealso cref="CustomHtml"/>
        public string ElementName { get; set; }


        /// <summary>
        /// Gets or sets optional attributes that should be added to the element. An empty dictionary by default.
        /// </summary>
        /// <remarks>Property value is ignored if <see cref="CustomHtml"/> value is set.</remarks>
        public IDictionary<string, object> HtmlAttributes { get; set; } = RenderingConfigurationUtils.CreateAttributeDictionary();


        /// <summary>
        /// Gets or sets child element rendering configuration. Use this property to nest multiple wrapping elements.
        /// </summary>
        /// <remarks>Property value is ignored if <see cref="CustomHtml"/> value is set.</remarks>
        public ElementRenderingConfiguration ChildConfiguration { get; set; }


        /// <summary>
        /// Get or sets custom HTML string used instead of wrapping element defined in <see cref="ElementName"/>. Within a custom HTML code must be used <see cref="CONTENT_PLACEHOLDER"/> value.
        /// </summary>
        /// <remarks>Takes precedence over <see cref="ElementName"/>, <see cref="HtmlAttributes"/> and <see cref="ChildConfiguration"/>.</remarks>
        /// <seealso cref="CONTENT_PLACEHOLDER"/>
        public IHtmlString CustomHtml { get; set; }


        /// <summary>
        /// Returns a deep copy of the configuration. All <see cref="ElementRenderingConfiguration"/> based properties
        /// are recursively copied using <see cref="Copy"/>, all <see cref="IDictionary{TKey, TValue}"/>
        /// based properties are shallow copied to a new dictionary (i.e. individual dictionary items are not re-created).
        /// </summary>
        /// <returns>Returns a copy of this configuration.</returns>
        internal ElementRenderingConfiguration Copy()
        {
            var result = new ElementRenderingConfiguration
            {
                ElementName = ElementName,
                HtmlAttributes = RenderingConfigurationUtils.CreateAttributeDictionary(HtmlAttributes),
                ChildConfiguration = ChildConfiguration?.Copy(),
                CustomHtml = new HtmlString(CustomHtml?.ToHtmlString())
            };

            return result;
        }
    }
}
