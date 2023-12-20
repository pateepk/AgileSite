using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains helper methods for adding and getting Form builder related configuration data to and from <see cref="ViewDataDictionary"/>.
    /// </summary>
    public static class ViewDataDictionaryExtensions
    {
        /// <summary>
        /// Key under which is <see cref="FormFieldRenderingConfiguration"/> instance stored in <see cref="ViewDataDictionary"/>.
        /// </summary>
        public const string FORM_FIELD_RENDERING_CONFIGURATION_KEY = "FormFieldRenderingConfiguration";


        /// <summary>
        /// Key under which is <see cref="IDictionary{String, Object}"/> containing editor HTML attributes stored in <see cref="ViewDataDictionary"/>.
        /// </summary>
        public const string EDITOR_HTML_ATTRIBUTES_KEY = "EditorHtmlAttributes";


        /// <summary>
        /// Gets <see cref="FormFieldRenderingConfiguration"/> from <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="viewData">View data to get the configuration from.</param>
        /// <returns>Instance of <see cref="FormFieldRenderingConfiguration"/> or <c>null</c> if no data are found or <see cref="ViewDataDictionary"/> is not initialized.</returns>
        public static FormFieldRenderingConfiguration GetFormFieldRenderingConfiguration(this ViewDataDictionary viewData)
        {
            return viewData?[FORM_FIELD_RENDERING_CONFIGURATION_KEY] as FormFieldRenderingConfiguration;
        }


        /// <summary>
        /// Adds instance of <see cref="FormFieldRenderingConfiguration"/> into <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="viewData">View data to add the configuration to.</param>
        /// <param name="formFieldRenderingConfiguration">Form field rendering configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewData"/> is null.</exception>
        public static void AddFormFieldRenderingConfiguration(this ViewDataDictionary viewData, FormFieldRenderingConfiguration formFieldRenderingConfiguration)
        {
            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            viewData.Add(FORM_FIELD_RENDERING_CONFIGURATION_KEY, formFieldRenderingConfiguration);
        }


        /// <summary>
        /// Gets <see cref="IDictionary{String, Object}"/> containing editor HTML attributes from <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="viewData">View data to get the attributes from.</param>
        /// <returns>Instance of <see cref="IDictionary{String, Object}"/> or <c>null</c> if no data are found or <see cref="ViewDataDictionary"/> is not initialized.</returns>
        public static IDictionary<string, object> GetEditorHtmlAttributes(this ViewDataDictionary viewData)
        {
            var attributes = viewData?[EDITOR_HTML_ATTRIBUTES_KEY] as IDictionary<string, object>;

            // Creates a new dictionary to prevent any modifications across multiple views
            return RenderingConfigurationUtils.CreateAttributeDictionary(attributes);
        }


        /// <summary>
        /// Adds instance of <see cref="IDictionary{String, Object}"/> containing editor HTML attributes into <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="viewData">View data to add the attributes to.</param>
        /// <param name="editorHtmlAttributes">Editor HTML attributes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewData"/> is null.</exception>
        public static void AddEditorHtmlAttributes(this ViewDataDictionary viewData, IDictionary<string, object> editorHtmlAttributes)
        {
            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            viewData.Add(EDITOR_HTML_ATTRIBUTES_KEY, editorHtmlAttributes);
        }
    }
}
