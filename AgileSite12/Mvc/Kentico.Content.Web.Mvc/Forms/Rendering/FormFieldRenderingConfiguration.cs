using System;
using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Configuration describing rendering of optional wrapping HTML elements for Form builder's form field.
    /// </summary>
    /// <seealso cref="FormExtensions.FormFields(Kentico.Web.Mvc.ExtensionPoint{System.Web.Mvc.HtmlHelper}, IEnumerable{FormComponent}, FormBuilderConfiguration, FormFieldRenderingConfiguration)"/>
    /// <seealso cref="FormExtensions.FormFields(Kentico.Web.Mvc.ExtensionPoint{System.Web.Mvc.HtmlHelper}, IEnumerable{FormComponent}, FormFieldRenderingConfiguration)"/>
    /// <seealso cref="FormExtensions.FormField(Kentico.Web.Mvc.ExtensionPoint{System.Web.Mvc.HtmlHelper}, FormComponent, FormFieldRenderingConfiguration)"/>
    public class FormFieldRenderingConfiguration
    {
        /// <summary>
        /// An event raised upon getting rendering configuration of a form field on live site. Allows for modification of <see cref="GetFormFieldRenderingConfigurationEventArgs.Configuration"/>
        /// for rendering the specified <see cref="GetFormFieldRenderingConfigurationEventArgs.FormComponent"/>.
        /// </summary>
        public static readonly GetFormFieldRenderingConfigurationHandler GetConfiguration = new GetFormFieldRenderingConfigurationHandler { Name = "FormFieldRenderingConfiguration.GetConfiguration" };


        /// <summary>
        /// Gets or sets the default form rendering configuration. The default configuration wraps the form field in a 'div' element with no attributes and renders a colon after the label.
        /// The configuration can be adjusted for individual form components using the <see cref="GetConfiguration"/> event.
        /// </summary>
        /// <seealso cref="FormExtensions.FormFields(Kentico.Web.Mvc.ExtensionPoint{System.Web.Mvc.HtmlHelper}, IEnumerable{FormComponent}, FormBuilderConfiguration, FormFieldRenderingConfiguration)"/>
        /// <seealso cref="FormExtensions.FormFields(Kentico.Web.Mvc.ExtensionPoint{System.Web.Mvc.HtmlHelper}, IEnumerable{FormComponent}, FormFieldRenderingConfiguration)"/>
        /// <seealso cref="FormExtensions.FormField(Kentico.Web.Mvc.ExtensionPoint{System.Web.Mvc.HtmlHelper}, FormComponent, FormFieldRenderingConfiguration)"/>
        public static FormFieldRenderingConfiguration Default { get; set; } = new FormFieldRenderingConfiguration
        {
            RootConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = null
            },
            LabelWrapperConfiguration = null,
            ColonAfterLabel = true,
            EditorWrapperConfiguration = null,
            ComponentWrapperConfiguration = null,
            ExplanationTextWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "explanation-text" } }
            }
        };


        /// <summary>
        /// Gets rendering configuration for a widget field.
        /// The configuration can be adjusted for individual form components using the <see cref="GetConfiguration"/> event.
        /// </summary>
        public static FormFieldRenderingConfiguration Widget { get; set; } = new FormFieldRenderingConfiguration
        {
            RootConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "form-field" } }
            },

            LabelWrapperConfiguration = new ElementRenderingConfiguration()
            {
                HtmlAttributes = { { "class", "label-property" } }
            },
            LabelHtmlAttributes = { { "class", "control-label" } },
            ColonAfterLabel = false,

            EditorWrapperConfiguration = new ElementRenderingConfiguration()
            {
                HtmlAttributes = { { "class", "field-property" } }
            },
            EditorHtmlAttributes = { { "class", "form-control" } },

            ComponentWrapperConfiguration = new ElementRenderingConfiguration()
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "editing-form-control-nested-control" } },
            },

            ExplanationTextWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "explanation-text" } }
            }
        };

        /// <summary>
        /// Gets or sets the rendering configuration of an element wrapping the form's field.
        /// A field consists of a label and an editor.
        /// </summary>
        public ElementRenderingConfiguration RootConfiguration { get; set; }


        /// <summary>
        /// Gets or sets the rendering configuration of an element wrapping the form's 'label' element.
        /// </summary>
        public ElementRenderingConfiguration LabelWrapperConfiguration { get; set; }


        /// <summary>
        /// Gets or sets the HTML attributes for the form's 'label' element. An empty dictionary by default.
        /// </summary>
        public IDictionary<string, object> LabelHtmlAttributes { get; set; } = RenderingConfigurationUtils.CreateAttributeDictionary();


        /// <summary>
        /// Gets or sets a value indicating whether a colon is rendered after 'label' element. True by default.
        /// </summary>
        public bool ColonAfterLabel { get; set; } = true;


        /// <summary>
        /// Gets or sets the rendering configuration of an element wrapping the smart icon element.
        /// </summary>
        public ElementRenderingConfiguration SmartFieldIconWrapperConfiguration { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether a smart field icon is rendered for smart fields.
        /// </summary>
        public bool ShowSmartFieldIcon { get; set; }


        /// <summary>
        /// Gets or sets HTML attributes for the smart field icon. An empty dictionary by default.
        /// </summary>
        public IDictionary<string, object> SmartFieldIconHtmlAttributes { get; set; } = RenderingConfigurationUtils.CreateAttributeDictionary();


        /// <summary>
        /// Gets or sets the rendering configuration of an element wrapping the form's 'editor' element.
        /// </summary>
        public ElementRenderingConfiguration ComponentWrapperConfiguration { get; set; }


        /// <summary>
        /// Gets or sets the rendering configuration of an element wrapped in <see cref="ComponentWrapperConfiguration"/> and the corresponding 'explanation text'.
        /// </summary>
        public ElementRenderingConfiguration EditorWrapperConfiguration { get; set; }


        /// <summary>
        /// Gets or sets HTML attributes for the form's 'editor' element. An empty dictionary by default.
        /// </summary>
        public IDictionary<string, object> EditorHtmlAttributes { get; set; } = RenderingConfigurationUtils.CreateAttributeDictionary();


        /// <summary>
        /// Gets or sets the rendering configuration to the explanation text of a form field.
        /// </summary>
        public ElementRenderingConfiguration ExplanationTextWrapperConfiguration { get; set; }


        /// <summary>
        /// When true no validation messages are rendered for the fields.
        /// This way validation messages may be handled from outside of the form field.
        /// </summary>
        public bool SuppressValidationMessages { get; set; }


        /// <summary>
        /// Gets rendering configuration for a form component. The <paramref name="sourceConfiguration"/> passed
        /// can be modified using the <see cref="GetConfiguration"/> event which is invoked.
        /// </summary>
        /// <param name="formComponent">Form component to return rendering configuration for.</param>
        /// <param name="sourceConfiguration">Initial rendering configuration which is passed to the <see cref="GetConfiguration"/> event.</param>
        /// <returns>Returns rendering configuration based on <paramref name="formComponent"/> and optionally modified by <see cref="GetConfiguration"/> event handlers.</returns>
        internal static FormFieldRenderingConfiguration GetConfigurationInternal(FormComponent formComponent, FormFieldRenderingConfiguration sourceConfiguration)
        {
            if (formComponent == null)
            {
                throw new ArgumentNullException(nameof(formComponent));
            }
            if (sourceConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sourceConfiguration));
            }

            var eventArgs = new GetFormFieldRenderingConfigurationEventArgs
            {
                SourceConfiguration = sourceConfiguration,
                FormComponent = formComponent
            };

            GetConfiguration.StartEvent(eventArgs);

            return eventArgs.TargetConfiguration;
        }


        /// <summary>
        /// Returns a deep copy of the configuration. All <see cref="ElementRenderingConfiguration"/> based properties
        /// are recursively copied using <see cref="Copy"/>, all <see cref="IDictionary{TKey, TValue}"/>
        /// based properties are shallow copied to a new dictionary (i.e. individual dictionary items are not re-created).
        /// </summary>
        /// <returns>Returns a copy of this configuration.</returns>
        internal FormFieldRenderingConfiguration Copy()
        {
            var result = new FormFieldRenderingConfiguration
            {
                ColonAfterLabel = ColonAfterLabel,
                ComponentWrapperConfiguration = ComponentWrapperConfiguration?.Copy(),
                EditorHtmlAttributes = RenderingConfigurationUtils.CreateAttributeDictionary(EditorHtmlAttributes),
                EditorWrapperConfiguration = EditorWrapperConfiguration?.Copy(),
                ExplanationTextWrapperConfiguration = ExplanationTextWrapperConfiguration?.Copy(),
                LabelHtmlAttributes = RenderingConfigurationUtils.CreateAttributeDictionary(LabelHtmlAttributes),
                LabelWrapperConfiguration = LabelWrapperConfiguration?.Copy(),
                RootConfiguration = RootConfiguration?.Copy(),
                ShowSmartFieldIcon = ShowSmartFieldIcon,
                SmartFieldIconHtmlAttributes = RenderingConfigurationUtils.CreateAttributeDictionary(SmartFieldIconHtmlAttributes),
                SmartFieldIconWrapperConfiguration = SmartFieldIconWrapperConfiguration?.Copy(),
                SuppressValidationMessages = SuppressValidationMessages
            };

            return result;
        }
    }
}
