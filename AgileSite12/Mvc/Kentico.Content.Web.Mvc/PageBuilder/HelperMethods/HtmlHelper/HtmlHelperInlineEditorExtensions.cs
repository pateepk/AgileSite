using System;
using System.Web.Mvc;

using Newtonsoft.Json.Serialization;

using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for extension point <see cref="Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(HtmlHelper)"/> to simplify inline editor creation.
    /// </summary>
    public static class HtmlHelperInlineEditorExtensions
    {
        /// <summary>
        /// Renders beginning tag of inline editor wrapper element.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="inlineEditorName">The name of the inline editor used for editing the widget property.</param>
        /// <param name="propertyName">The name of the widget property managed by the inline editor.</param>
        /// <param name="htmlAttributes">Additional inline editor element attributes.</param>
        /// <param name="wrapperElement">Element in which the inline editor will be wrapped.</param>
        /// <returns>Returns an object representing inline editor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <see name="inlineEditorName"/> or <see name="propertyName"/> is null or an empty string.</exception>
        public static MvcInlineEditor BeginInlineEditor(this ExtensionPoint<HtmlHelper> instance, string inlineEditorName, string propertyName, object htmlAttributes = null, string wrapperElement = "div")
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (string.IsNullOrEmpty(inlineEditorName))
            {
                throw new ArgumentException("The inline editor name cannot be null or an empty string.", nameof(inlineEditorName));
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("The property name cannot be null or an empty string.", nameof(propertyName));
            }

            var htmlHelper = instance.Target;

            var editorElement = new TagBuilder(wrapperElement);
            editorElement.Attributes.Add("data-inline-editor", inlineEditorName);
            editorElement.Attributes.Add("data-property-name", new CamelCaseConvertor().Convert(propertyName));

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            editorElement.MergeAttributes(attributes);

            htmlHelper.ViewContext.Writer.Write(editorElement.ToString(TagRenderMode.StartTag));

            return new MvcInlineEditor(htmlHelper.ViewContext, editorElement);
        }


        /// <summary>
        /// Renders ending tag of inline editor wrapper element.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="wrapperElement">Element in which the inline editor will be wrapped.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public static void EndInlineEditor(this ExtensionPoint<HtmlHelper> instance, string wrapperElement = "div")
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var htmlHelper = instance.Target;

            var editorElement = new TagBuilder(wrapperElement);
            htmlHelper.ViewContext.Writer.Write(editorElement.ToString(TagRenderMode.EndTag));
        }


        private sealed class CamelCaseConvertor : CamelCaseNamingStrategy
        {
            public string Convert(string name)
            {
                return ResolvePropertyName(name);
            }
        }
    }
}
