using CMS.OnlineForms;
using System;
using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc.Widgets
{
    /// <summary>
    /// Configuration describing rendering of form elements.
    /// </summary>
    public class FormWidgetRenderingConfiguration
    {
        /// <summary>
        /// An event raised upon getting rendering configuration. Allows for modification of <see cref="GetFormWidgetRenderingConfigurationEventArgs.Configuration"/>
        /// for rendering the specified <see cref="GetFormWidgetRenderingConfigurationEventArgs.Form"/>.
        /// </summary>
        public static readonly GetFormWidgetRenderingConfigurationHandler GetConfiguration = new GetFormWidgetRenderingConfigurationHandler { Name = "FormWidgetRenderingConfiguration.GetConfiguration" };


        /// <summary>
        /// Gets rendering configuration for a widget form.
        /// </summary>
        public static FormWidgetRenderingConfiguration Default { get; set; } = new FormWidgetRenderingConfiguration()
        {
            FormWrapperConfiguration = new FormWrapperRenderingConfiguration
            {
                ElementName = "div"
            }
        };


        /// <summary>
        /// HTML attributes used for form element.
        /// </summary>
        public IDictionary<string, object> FormHtmlAttributes { get; set; } = RenderingConfigurationUtils.CreateAttributeDictionary();


        /// <summary>
        /// HTML attributes used for submit button element.
        /// </summary>
        public IDictionary<string, object> SubmitButtonHtmlAttributes { get; set; } = RenderingConfigurationUtils.CreateAttributeDictionary();


        /// <summary>
        /// Gets or sets the rendering configuration of an element wrapping the form.
        /// </summary>
        public FormWrapperRenderingConfiguration FormWrapperConfiguration { get; set; }


        /// <summary>
        /// Gets rendering configuration for a form. The <paramref name="sourceConfiguration"/> passed
        /// can be modified using the <see cref="GetConfiguration"/> event which is invoked.
        /// </summary>
        /// <param name="form">Form to return rendering configuration for.</param>
        /// <param name="formComponents">Collection of actually rendered form components.</param>
        /// <param name="formWidgetProperties">Widget properties used for currently displayed form.</param>
        /// <param name="sourceConfiguration">Initial rendering configuration which is passed to the <see cref="GetConfiguration"/> event.</param>
        /// <returns>Returns rendering configuration based on <paramref name="form"/> and optionally modified by <see cref="GetConfiguration"/> event handlers.</returns>
        internal static FormWidgetRenderingConfiguration GetConfigurationInternal(BizFormInfo form, IEnumerable<FormComponent> formComponents, FormWidgetProperties formWidgetProperties,  FormWidgetRenderingConfiguration sourceConfiguration)
        {
            if (sourceConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sourceConfiguration));
            }

            var eventArgs = new GetFormWidgetRenderingConfigurationEventArgs
            {
                SourceConfiguration = sourceConfiguration,
                Form = form,
                FormComponents = formComponents,
                FormWidgetProperties = formWidgetProperties
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
        internal FormWidgetRenderingConfiguration Copy()
        {
            var result = new FormWidgetRenderingConfiguration
            {
                FormHtmlAttributes = RenderingConfigurationUtils.CreateAttributeDictionary(FormHtmlAttributes),
                SubmitButtonHtmlAttributes = RenderingConfigurationUtils.CreateAttributeDictionary(SubmitButtonHtmlAttributes),
                FormWrapperConfiguration = FormWrapperConfiguration?.Copy()
            };

            return result;
        }
    }
}
