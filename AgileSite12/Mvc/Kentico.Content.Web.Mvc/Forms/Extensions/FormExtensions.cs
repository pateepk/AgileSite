using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;

using Kentico.Forms.Web.Mvc.Internal;
using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains helper methods for rendering HTML for forms built using Form builder.
    /// </summary>
    public static class FormExtensions
    {
        /// <summary>
        /// Returns HTML markup for given enumeration of form components organized by given <paramref name="formConfiguration"/>.
        /// The markup contains a label and an editing element representing each of the form components.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponents">The form components to render the markup for.</param>
        /// <param name="formConfiguration">Configuration defining how to render form components.</param>
        /// <param name="renderingConfiguration">Configuration for the form fields rendering. If null, <see cref="FormFieldRenderingConfiguration.Default"/> is used.</param>
        /// <returns>HTML markup for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponents"/> or <paramref name="formConfiguration"/> is null.</exception>
        public static MvcHtmlString FormFields(this ExtensionPoint<HtmlHelper> htmlHelper, IEnumerable<FormComponent> formComponents, FormBuilderConfiguration formConfiguration, FormFieldRenderingConfiguration renderingConfiguration = null)
        {
            return htmlHelper.FormFields(formComponents, formConfiguration, renderingConfiguration, (action, controller) => htmlHelper.Target.RenderAction(action, controller));
        }


        /// <summary>
        /// Returns HTML markup for given enumeration of form components organized by given <paramref name="formConfiguration"/>.
        /// The markup contains a label and an editing element representing each of the form components.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponents">The form components to render the markup for.</param>
        /// <param name="formConfiguration">Configuration defining how to render form components.</param>
        /// <param name="renderingConfiguration">Configuration for the form fields rendering. If null, <see cref="FormFieldRenderingConfiguration.Default"/> is used.</param>
        /// <param name="renderSection">Action for rendering sections.</param>
        /// <returns>HTML markup for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponents"/> or <paramref name="formConfiguration"/> is null.</exception>
        internal static MvcHtmlString FormFields(this ExtensionPoint<HtmlHelper> htmlHelper, IEnumerable<FormComponent> formComponents, FormBuilderConfiguration formConfiguration, FormFieldRenderingConfiguration renderingConfiguration, Action<string, string> renderSection = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }
            if (formComponents == null)
            {
                throw new ArgumentNullException(nameof(formComponents));
            }
            if (formConfiguration == null)
            {
                throw new ArgumentNullException(nameof(formConfiguration));
            }

            var editableArea = formConfiguration.EditableAreas.FirstOrDefault();
            if (editableArea == null)
            {
                return MvcHtmlString.Empty;
            }

            var registeredSections = Service.Resolve<ISectionDefinitionProvider>().GetAll().ToDictionary(s => s.Identifier, StringComparer.InvariantCultureIgnoreCase);

            foreach (var section in editableArea.Sections)
            {
                if (!registeredSections.TryGetValue(section.TypeIdentifier, out SectionDefinition renderedSection))
                {
                    Service.Resolve<IEventLogService>().LogEvent(EventType.ERROR, FormBuilderConstants.EVENT_LOG_SOURCE, "RenderSection", $"The '{section.TypeIdentifier}' form section type is not registered.");

                    continue;
                }

                var sectionModel = CreateSectionModel(section, formComponents.ToDictionary(f => f.BaseProperties.Guid), renderingConfiguration);
                StoreSectionModel(htmlHelper.Target.ViewContext.ViewData, sectionModel);

                renderSection?.Invoke(FormBuilderRoutes.DEFAULT_ACTION_NAME, renderedSection.ControllerName);
            }

            return MvcHtmlString.Empty;
        }


        /// <summary>
        /// Returns HTML markup for given enumeration of form components. The markup contains
        /// a label and an editing element representing each of the form components.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponents">The form components to render the markup for.</param>
        /// <param name="renderingConfiguration">Configuration for the form fields rendering. If null, <see cref="FormFieldRenderingConfiguration.Default"/> is used.</param>
        /// <returns>HTML markup for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponents"/> is null.</exception>
        public static MvcHtmlString FormFields(this ExtensionPoint<HtmlHelper> htmlHelper, IEnumerable<FormComponent> formComponents, FormFieldRenderingConfiguration renderingConfiguration = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (formComponents == null)
            {
                throw new ArgumentNullException(nameof(formComponents));
            }

            StringBuilder html = new StringBuilder();

            foreach (var formComponent in formComponents)
            {
                var formFieldHtml = htmlHelper.FormField(formComponent, htmlHelper.Target.ViewData, renderingConfiguration, false);

                html.Append(formFieldHtml.ToHtmlString());
            }

            return new MvcHtmlString(html.ToString());
        }


        private static void StoreSectionModel(ViewDataDictionary viewData, SectionModel sectionModel)
        {
            viewData[FormBuilderConstants.SECTION_MODEL_DATA_KEY] = sectionModel;
        }


        private static SectionModel CreateSectionModel(SectionConfiguration sectionConfiguration, IDictionary<Guid, FormComponent> formComponents, FormFieldRenderingConfiguration renderingConfiguration)
        {
            var sectionModel = new SectionModel
            {
                RenderingConfiguration = renderingConfiguration,
            };


            foreach (var zone in sectionConfiguration.Zones)
            {
                var zoneContent = new List<FormComponent>();

                foreach (var formComponent in zone.FormComponents)
                {
                    if (formComponents.TryGetValue(formComponent.Identifier, out FormComponent renderedComponent))
                    {
                        zoneContent.Add(renderedComponent);
                    }
                }

                sectionModel.ZonesContent.Add(zoneContent);
            }

            return sectionModel;
        }


        /// <summary>
        /// Returns HTML markup for given <paramref name="formComponent"/>. The markup contains
        /// a label and an editing element representing the form component given.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponent">The form component to render the markup for.</param>
        /// <param name="renderingConfiguration">Configuration for the form field rendering. If null, <see cref="FormFieldRenderingConfiguration.Default"/> is used.</param>
        /// <returns>HTML markup for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponent"/> is null.</exception>
        public static MvcHtmlString FormField(this ExtensionPoint<HtmlHelper> htmlHelper, FormComponent formComponent, FormFieldRenderingConfiguration renderingConfiguration = null)
        {
            return htmlHelper.FormField(formComponent, htmlHelper.Target.ViewData, renderingConfiguration, false);
        }


        /// <summary>
        /// Returns HTML markup for given <paramref name="formComponent"/>. The markup contains
        /// a label and an editing element representing the form component given.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponent">The form component to render the markup for.</param>
        /// <param name="viewData">View data used for rendering <paramref name="formComponent"/>.</param>
        /// <param name="renderingConfiguration">Configuration for the form field rendering. If null, <see cref="FormFieldRenderingConfiguration.Default"/> is used.</param>
        /// <param name="invokeGetConfiguration">A value indicating whether to invoke the <see cref="FormFieldRenderingConfiguration.GetConfiguration"/> event.</param>
        /// <returns>HTML markup for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponent"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an exception occurred during component rendering.</exception>
        internal static MvcHtmlString FormField(this ExtensionPoint<HtmlHelper> htmlHelper, FormComponent formComponent, ViewDataDictionary viewData, FormFieldRenderingConfiguration renderingConfiguration, bool invokeGetConfiguration)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }
            if (formComponent == null)
            {
                throw new ArgumentNullException(nameof(formComponent));
            }

            var componentData = new ViewDataDictionary(viewData);

            var sourceRenderingConfiguration = renderingConfiguration ?? FormFieldRenderingConfiguration.Default;
            var appliedRenderingConfiguration = invokeGetConfiguration ? FormFieldRenderingConfiguration.GetConfigurationInternal(formComponent, sourceRenderingConfiguration) : sourceRenderingConfiguration;
            componentData.AddFormFieldRenderingConfiguration(appliedRenderingConfiguration);

            try
            {
                return htmlHelper.Target.Partial("~/Views/Shared/Kentico/FormBuilder/_FormField.cshtml", formComponent, componentData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error rendering view for '{formComponent.Definition.Identifier}' component.", ex);
            }
        }


        /// <summary>
        /// Returns an HTML editing element for given <paramref name="formComponent"/>.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponent">The form component to render the editing element for.</param>
        /// <param name="htmlAttributes">An object containing additional HTML attributes for the editor.</param>
        /// <returns>An HTML editing element for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponent"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="FormComponent.Name"/> of the form component passed is null or an empty string.</exception>
        public static MvcHtmlString Editor(this ExtensionPoint<HtmlHelper> htmlHelper, FormComponent formComponent, IDictionary<string, object> htmlAttributes = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }
            if (formComponent == null)
            {
                throw new ArgumentNullException(nameof(formComponent));
            }
            if (String.IsNullOrEmpty(formComponent.Name))
            {
                throw new ArgumentException($"The form component must have its '{nameof(formComponent.Name)}' property set in order to render an editor.");
            }

            var viewData = htmlHelper.Target.ViewData != null
                ? new ViewDataDictionary(htmlHelper.Target.ViewData)
                : new ViewDataDictionary();

            string tooltip = null;
            string placeholder = null;

            IDictionary<string, object> attributes = RenderingConfigurationUtils.CreateAttributeDictionary(htmlAttributes);

            if (!attributes.ContainsKey("title") && !String.IsNullOrEmpty(tooltip = ResHelper.LocalizeString(formComponent.BaseProperties?.Tooltip)))
            {
                attributes["title"] = tooltip;
            }

            if (!attributes.ContainsKey("placeholder") && !String.IsNullOrEmpty(placeholder = ResHelper.LocalizeString(formComponent.BaseProperties?.Placeholder)))
            {
                attributes["placeholder"] = placeholder;
            }

            if (!formComponent.HasDependingFields || formComponent.CustomAutopostHandling)
            {
                attributes[UpdatableMvcForm.NOT_OBSERVED_ELEMENT_ATTRIBUTE_NAME] = null;
            }

            viewData.AddEditorHtmlAttributes(attributes);

            var partialViewName = String.IsNullOrEmpty(formComponent.Definition.ViewName) ? $"FormComponents/_{formComponent.Definition.Identifier}" : formComponent.Definition.ViewName;

            return htmlHelper.Target.Partial(partialViewName, formComponent, new ViewDataDictionary(viewData)
            {
                TemplateInfo = new TemplateInfo
                {
                    HtmlFieldPrefix = viewData.TemplateInfo.GetFullHtmlFieldName(formComponent.Name)
                }
            });
        }


        /// <summary>
        /// Returns a smart field icon if provided <paramref name="formComponent"/> is a smart field.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponent">The form component to render the icon for.</param>
        /// <param name="htmlAttributes">An object containing additional HTML attributes for the icon.</param>
        /// <returns>An HTML editing element for smart field icon.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponent"/> is null.</exception>
        public static MvcHtmlString SmartFieldIcon(this ExtensionPoint<HtmlHelper> htmlHelper, FormComponent formComponent, IDictionary<string, object> htmlAttributes = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (formComponent == null)
            {
                throw new ArgumentNullException(nameof(formComponent));
            }

            if (!formComponent.BaseProperties.SmartField)
            {
                return null;
            }

            var localizedTitle = ResHelper.GetString("kentico.formbuilder.smartfield.label");

            var iconBuilder = new TagBuilder("i");
            iconBuilder.Attributes.Add("aria-hidden", "true");
            iconBuilder.Attributes.Add("title", localizedTitle);
            iconBuilder.MergeAttributes(htmlAttributes, true);

            return new MvcHtmlString(iconBuilder.ToString(TagRenderMode.Normal));
        }


        /// <summary>
        /// Returns an HTML label element for given <paramref name="formComponent"/>.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponent">The form component to render the label for.</param>
        /// <param name="htmlAttributes">An object containing additional HTML attributes for the label.</param>
        /// <param name="renderColon">Indicates whether a colon is to be rendered after the label text. If a label text is missing then colon is not rendered.</param>
        /// <returns>An HTML label element for form component.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> or <paramref name="formComponent"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="FormComponent.Name"/> of the form component passed is null or an empty string.</exception>
        public static MvcHtmlString Label(this ExtensionPoint<HtmlHelper> htmlHelper, FormComponent formComponent, IDictionary<string, object> htmlAttributes = null, bool renderColon = true)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }
            if (formComponent == null)
            {
                throw new ArgumentNullException(nameof(formComponent));
            }
            if (String.IsNullOrEmpty(formComponent.Name))
            {
                throw new ArgumentException($"The form component must have its '{nameof(formComponent.Name)}' property set in order to render a label.");
            }

            var labelText = ResHelper.LocalizeString(formComponent.BaseProperties.Label);
            if (String.IsNullOrEmpty(labelText))
            {
                return new MvcHtmlString(String.Empty);
            }

            if (renderColon)
            {
                labelText += ":";
            }

            var templateInfo = htmlHelper.Target.ViewData?.TemplateInfo ?? new TemplateInfo();

            TagBuilder tagBuilder = new TagBuilder("label");
            tagBuilder.SetInnerText(labelText);
            tagBuilder.MergeAttributes(htmlAttributes, true);

            if (!string.IsNullOrEmpty(formComponent.LabelForPropertyName) && (htmlAttributes == null || !htmlAttributes.ContainsKey("for")))
            {
                var labelForPartial = TagBuilder.CreateSanitizedId($"{formComponent.Name}.{formComponent.LabelForPropertyName}");
                tagBuilder.Attributes.Add("for", templateInfo.GetFullHtmlFieldId(labelForPartial));
            }

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }


        /// <summary>
        /// Renders the start tag of wrapping HTML element based on <paramref name="configuration"/>.
        /// An empty string is returned, when configuration is null or does not specify an <see cref="ElementRenderingConfiguration.ElementName"/>.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="configuration">Configuration object for the wrapping element.</param>
        /// <returns>Returns start tag of wrapping HTML element or an empty string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ElementRenderingConfiguration.CustomHtml"/> is used but <see cref="ElementRenderingConfiguration.CONTENT_PLACEHOLDER" /> is missing.</exception>
        public static MvcHtmlString BeginWrappingElement(this ExtensionPoint<HtmlHelper> htmlHelper, ElementRenderingConfiguration configuration = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var stringBuilder = new StringBuilder();

            BeginWrappingElementCore(configuration, stringBuilder);

            return stringBuilder.Length > 0 ? new MvcHtmlString(stringBuilder.ToString()) : MvcHtmlString.Empty;
        }



        private static bool TryBeginCustomHtml(ElementRenderingConfiguration configuration, StringBuilder stringBuilder)
        {
            if (CanRenderCustomHtml(configuration, out var customHtml, out var placeholderIndex))
            {
                stringBuilder.Append(customHtml.Substring(0, placeholderIndex));

                return true;
            }

            return false;
        }


        private static bool TryEndCustomHtml(ElementRenderingConfiguration configuration, StringBuilder stringBuilder)
        {
            if (CanRenderCustomHtml(configuration, out var customHtml, out var placeholderIndex))
            {
                stringBuilder.Append(customHtml.Substring(placeholderIndex + ElementRenderingConfiguration.CONTENT_PLACEHOLDER.Length));

                return true;
            }

            return false;
        }


        private static bool CanRenderCustomHtml(ElementRenderingConfiguration configuration, out string customHtml, out int placeholderIndex)
        {
            placeholderIndex = -1;
            customHtml = configuration?.CustomHtml?.ToHtmlString();
            if (String.IsNullOrWhiteSpace(customHtml))
            {
                return false;
            }

            placeholderIndex = customHtml.IndexOf(ElementRenderingConfiguration.CONTENT_PLACEHOLDER, StringComparison.Ordinal);
            if (placeholderIndex < 0)
            {
                throw new InvalidOperationException($"{typeof(ElementRenderingConfiguration).FullName}.{nameof(ElementRenderingConfiguration.CONTENT_PLACEHOLDER)} is missing from the {nameof(ElementRenderingConfiguration.CustomHtml)} property.");
            }

            return true;
        }


        private static void BeginWrappingElementCore(ElementRenderingConfiguration configuration, StringBuilder stringBuilder)
        {
            if (TryBeginCustomHtml(configuration, stringBuilder))
            {
                return;
            }

            if (String.IsNullOrEmpty(configuration?.ElementName))
            {
                return;
            }

            var tagBuilder = new TagBuilder(configuration.ElementName);

            tagBuilder.MergeAttributes(configuration.HtmlAttributes, true);

            stringBuilder.Append(tagBuilder.ToString(TagRenderMode.StartTag));

            BeginWrappingElementCore(configuration.ChildConfiguration, stringBuilder);
        }


        /// <summary>
        /// Renders the end tag of wrapping HTML element based on <paramref name="configuration"/>.
        /// An empty string is returned, when configuration is null or does not specify an <see cref="ElementRenderingConfiguration.ElementName"/>.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="configuration">Configuration object for the wrapping element.</param>
        /// <returns>Returns end tag of wrapping HTML element or an empty string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ElementRenderingConfiguration.CustomHtml"/> is used but <see cref="ElementRenderingConfiguration.CONTENT_PLACEHOLDER" /> is missing.</exception>
        public static MvcHtmlString EndWrappingElement(this ExtensionPoint<HtmlHelper> htmlHelper, ElementRenderingConfiguration configuration = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var stringBuilder = new StringBuilder();

            EndWrappingElementCore(configuration, stringBuilder);

            return stringBuilder.Length > 0 ? new MvcHtmlString(stringBuilder.ToString()) : MvcHtmlString.Empty;
        }


        private static void EndWrappingElementCore(ElementRenderingConfiguration configuration, StringBuilder stringBuilder)
        {
            if (TryEndCustomHtml(configuration, stringBuilder))
            {
                return;
            }

            if (String.IsNullOrEmpty(configuration?.ElementName))
            {
                return;
            }

            var tagBuilder = new TagBuilder(configuration.ElementName);

            EndWrappingElementCore(configuration.ChildConfiguration, stringBuilder);

            stringBuilder.Append(tagBuilder.ToString(TagRenderMode.EndTag));
        }


        /// <summary>
        /// Renders a validation summary for a form with clickable labels.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="formComponents">Components contained a form for which the summary is rendered.</param>
        /// <returns>Validation summary markup.</returns>
        public static MvcHtmlString ValidationSummary(this ExtensionPoint<HtmlHelper> htmlHelper, IEnumerable<FormComponent> formComponents)
        {
            if (formComponents == null)
            {
                throw new ArgumentNullException(nameof(formComponents));
            }

            formComponents = formComponents.ToList();

            var summary = new List<string>();
            var summaryWithoutComponents = new List<string>();

            var states = htmlHelper.Target.ViewData.ModelState
                .Where(state => state.Value.Errors.Any());

            foreach (var state in states)
            {
                var key = state.Key;
                var prefix = htmlHelper.Target.ViewData.TemplateInfo.HtmlFieldPrefix;

                if (!String.IsNullOrEmpty(prefix))
                {
                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        key = key.Substring(prefix.Length).TrimStart('.');
                    }
                }

                var dotIndex = key.IndexOf('.');
                var componentName = dotIndex >= 0 ? key.Substring(0, dotIndex) : key;
                var component = formComponents.FirstOrDefault(c => c.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase));

                foreach (var error in state.Value.Errors)
                {
                    if (component == null)
                    {
                        summaryWithoutComponents.Add($@"<span class=""field-validation-error"">{error.ErrorMessage}</span>");
                    }
                    else if (String.Equals(key, component.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var labelString = component.GetDisplayName();

                        var label = htmlHelper.Target.Label($"{key}.{component.LabelForPropertyName}", $"{ResHelper.LocalizeString(labelString)}:", new { @class = "ktc-Anchor" });

                        summary.Add($@"{label} <span class=""field-validation-error"">{error.ErrorMessage}</span>");
                    }
                    else
                    {
                        // Note: we cannot use _ValidationMessage(key)_ method as it will return only first error message
                        // and this is incorrect in case of multiple errors for same component and same property
                        ////var validation = htmlHelper.Target.ValidationMessage(key).ToString();

                        string extraAttributes = null;
                        if (htmlHelper.Target.ViewContext.ClientValidationEnabled && htmlHelper.Target.ViewContext.UnobtrusiveJavaScriptEnabled)
                        {
                            extraAttributes = $@" data-valmsg-for=""{state.Key}"" data-valmsg-replace=""true""";
                        }

                        var validation = $@"<span class=""field-validation-error""{extraAttributes}>{error.ErrorMessage}</span>";

                        var labelString = component.GetDisplayName();

                        var label = htmlHelper.Target.Label(key, $"{ResHelper.LocalizeString(labelString)}:", new { @class = "ktc-Anchor" });

                        summary.Add($"{label} {validation}");
                    }
                }
            }

            summary.AddRange(summaryWithoutComponents);

            return MvcHtmlString.Create(String.Join("<br />", summary));
        }
    }
}
