using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc.Internal;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;


namespace Kentico.Forms.Web.Mvc.Widgets.Internal
{
    /// <summary>
    /// Form widget extension methods.
    /// </summary>
    public static class FormWidgetExtensions
    {
        private const string FORM_WIDGET_RENDERING_CONFIGURATION_KEY = "KenticoFormWidgetRenderingConfiguration";
        private const string FORM_WIDGET_RENDERING_IS_FORM_SUBMIT = "KenticoFormWidgetFormSubmitFlag";

        /// <summary>
        /// Indicates whether form submit action is executed.
        /// </summary>
        internal static bool IsFormSubmit(this ViewDataDictionary viewData)
        {
            return viewData?[FORM_WIDGET_RENDERING_IS_FORM_SUBMIT] != null;
        }


        /// <summary>
        /// Sets form submit flag used within <see cref="IsFormSubmit"/>.
        /// </summary>
        internal static void SetFormSubmit(this ViewDataDictionary viewData)
        {
            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            viewData.Add(FORM_WIDGET_RENDERING_IS_FORM_SUBMIT, new object());
        }


        /// <summary>
        /// Gets <see cref="FormWidgetRenderingConfiguration"/> from <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="viewData">View data to get the configuration from.</param>
        /// <returns>Instance of <see cref="FormFieldRenderingConfiguration"/> or <c>null</c> if no data are found or <see cref="ViewDataDictionary"/> is not initialized.</returns>
        public static FormWidgetRenderingConfiguration GetFormWidgetRenderingConfiguration(this ViewDataDictionary viewData)
        {
            return viewData?[FORM_WIDGET_RENDERING_CONFIGURATION_KEY] as FormWidgetRenderingConfiguration;
        }


        /// <summary>
        /// Adds instance of <see cref="FormWidgetRenderingConfiguration"/> into <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="viewData">View data to add the configuration to.</param>
        /// <param name="formWidgetRenderingConfiguration">Form widget rendering configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewData"/> is null.</exception>
        public static void AddFormWidgetRenderingConfiguration(this ViewDataDictionary viewData, FormWidgetRenderingConfiguration formWidgetRenderingConfiguration)
        {
            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            viewData.Add(FORM_WIDGET_RENDERING_CONFIGURATION_KEY, formWidgetRenderingConfiguration);
        }


        /// <summary>
        /// Writes an opening &lt;form&gt; tag to the response.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="model">Form widget model.</param>
        public static FormWidgetForm BeginForm(this ExtensionPoint<HtmlHelper> htmlHelper, FormWidgetViewModel model)
        {
            return new FormWidgetForm(htmlHelper, model);
        }


        /// <summary>
        /// Renders form submit button based on <paramref name="model"/> values.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="model">Form widget model.</param>
        public static MvcHtmlString FormSubmitButton(this ExtensionPoint<HtmlHelper> htmlHelper, FormWidgetViewModel model)
        {
            if (model?.FormComponents?.Any() != true)
            {
                return MvcHtmlString.Empty;
            }
            
            var renderingConfiguration = htmlHelper.Target.ViewData.GetFormWidgetRenderingConfiguration();

            if (!String.IsNullOrEmpty(model.SubmitButtonImage))
            {
                var htmlAttributes = new Dictionary<string, object>(renderingConfiguration.SubmitButtonHtmlAttributes, StringComparer.Ordinal);

                if (!String.IsNullOrEmpty(model.SubmitButtonText))
                {
                    if (!htmlAttributes.ContainsKey("alt"))
                    {
                        htmlAttributes.Add("alt", model.SubmitButtonText);
                    }

                    if (!htmlAttributes.ContainsKey("title"))
                    {
                        htmlAttributes.Add("title", model.SubmitButtonText);
                    }
                }

                if (!model.IsFormSubmittable)
                {
                    htmlAttributes["onclick"] = "return false;";
                }

                return htmlHelper.Target.Kentico().ImageInput(model.SubmitButtonImage, htmlAttributes);
            }
            else
            {
                var submitInput = new TagBuilder("input");
                submitInput.Attributes.Add("type", "submit");
                submitInput.Attributes.Add("value", model.SubmitButtonText);

                submitInput.MergeAttributes(renderingConfiguration.SubmitButtonHtmlAttributes, true);

                if (!model.IsFormSubmittable)
                {
                    submitInput.Attributes["onclick"] = "return false;";
                }

                return new MvcHtmlString(submitInput.ToString(TagRenderMode.SelfClosing));
            }
        }


        /// <summary>
        /// Renders form inline selector in edit mode.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="model">Form widget model.</param>
        public static MvcHtmlString FormInlineSelector(this ExtensionPoint<HtmlHelper> htmlHelper, FormWidgetViewModel model)
        {
            return FormInlineSelectorInternal(htmlHelper, model, renderPartial: htmlHelper.Target.RenderPartial);
        }


        /// <summary>
        /// Renders form inline selector in edit mode with optional render partial method for testing purposes.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="model">Form widget model.</param>
        /// <param name="renderPartial">Render partial method</param>
        internal static MvcHtmlString FormInlineSelectorInternal(ExtensionPoint<HtmlHelper> htmlHelper, FormWidgetViewModel model, Action<string, object> renderPartial)
        {
            if (!CMSHttpContext.Current.Kentico().PageBuilder().EditMode)
            {
                return MvcHtmlString.Empty;
            }

            renderPartial("~/Views/Shared/Kentico/InlineEditors/_DropdownEditor.cshtml", new DropdownEditorViewModel
            {
                PropertyName = nameof(FormWidgetProperties.SelectedForm),
                Options = model.SiteForms,
                Selected = model.FormName,
                LabelKey = "kentico.formbuilder.widget.selectform",
                NoOptionsMessageKey = "kentico.formbuilder.widget.noforms"
            });

            if (!String.IsNullOrEmpty(model.FormName) && model.FormComponents == null)
            {
                var errorSpan = new TagBuilder("span");
                errorSpan.AddCssClass("message-error");
                errorSpan.SetInnerText(ResHelper.GetString("kentico.formbuilder.widget.formdeleted"));
                return new MvcHtmlString(errorSpan.ToString());
            }

            return MvcHtmlString.Empty;
        }


        /// <summary>
        /// Renders form fields based on <paramref name="model"/> values.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="model">Form widget model.</param>
        public static MvcHtmlString FormFields(this ExtensionPoint<HtmlHelper> htmlHelper, FormWidgetViewModel model)
        {
            if (model?.FormComponents?.Any() != true)
            {
                return MvcHtmlString.Empty;
            }

            return htmlHelper.FormFields(model.FormComponents, model.FormConfiguration, FormFieldRenderingConfiguration.Widget);
        }
    }
}
