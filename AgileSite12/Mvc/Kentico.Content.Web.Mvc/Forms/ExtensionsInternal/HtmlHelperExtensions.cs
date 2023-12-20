using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Optimization;
using System.Web.Routing;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

using Kentico.Builder.Web.Mvc;
using Kentico.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides system extension methods for <see cref="Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(HtmlHelper)"/> extension point.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders necessary stylesheet link tags for Form builder in context of administration interface.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns HTML markup with link tags for stylesheets or returns <c>null</c> if edit mode is not initialized.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString FormBuilderStyles(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var assetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(instance.Target.ViewContext.RequestContext);

            return new HtmlString(
                assetsProvider.GetAdminStylesheetLinkTag() +
                assetsProvider.GetFormComponentsAdminStylesheetLinkTag()
            );
        }


        /// <summary>
        /// Renders necessary stylesheet link tags for Form builder components in context of administration interface.
        /// If no form component specific stylesheet is available, an empty string is returned.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns stylesheet link tags for form components in administration interface, or an empty string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString FormComponentsAdminStyles(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var assetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(instance.Target.ViewContext.RequestContext);

            return new HtmlString(
                assetsProvider.GetFormComponentsAdminStylesheetLinkTag()
            );
        }


        /// <summary>
        /// Renders forms scripts bundle link tag.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <returns>Returns script tag with source pointed to forms JavaScript bundle,
        /// which includes scripts for form components, sections and additional forms scripts.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString FormsScripts(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var assetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(instance.Target.ViewContext.RequestContext);

            return new HtmlString(
                assetsProvider.GetFormsScriptBundleTag()
            );
        }


        /// <summary>
        /// Renders necessary scripts for Form builder feature.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">Identifier of the form for which the Form builder will be initialized.</param>
        /// <param name="propertiesEditorClientId">Client ID of the properties panel to render the properties editor into.</param>
        /// <param name="saveMessageClientId">Client ID of the element to render the save message into.</param>
        /// <returns>Returns HTML markup with script tags for scripts.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString FormBuilderScripts(this ExtensionPoint<HtmlHelper> instance, int formId, string propertiesEditorClientId, string saveMessageClientId)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return instance.FormBuilderScriptsInternal(formId, propertiesEditorClientId, saveMessageClientId, new AllowedDomainsRetriever(SiteContext.CurrentSite));
        }


        /// <summary>
        /// Renders jQuery and unobtrusive jQuery script tags. Non-customized versions are used.
        /// </summary>
        /// <returns>Script tags for jQuery.</returns>
        public static IHtmlString RenderJQueryScriptTags()
        {
            return new HtmlString(
                Scripts.Render(ScriptsBundleCollectionUtils.JQUERY_SCRIPTS_BUNDLE_VIRTUAL_PATH).ToString() +
                Scripts.Render(ScriptsBundleCollectionUtils.JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_BUNDLE_VIRTUAL_PATH).ToString()
            );
        }


        /// <summary>
        /// Renders necessary startup scripts for Form builder feature.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">>Identifier of the form for which the Form builder will be initialized.</param>
        /// <param name="propertiesEditorClientId">Client ID of the properties panel to render the properties editor into.</param>
        /// <param name="saveMessageClientId">Client ID of the element to render the save message into.</param>
        /// <param name="allowedDomainsRetriever">Retriever of allowed domains to check origin of post messages on client.</param>
        /// <returns>Returns HTML markup with script tags for startup scripts.</returns>
        internal static IHtmlString FormBuilderScriptsInternal(this ExtensionPoint<HtmlHelper> instance, int formId, string propertiesEditorClientId, string saveMessageClientId, IAllowedDomainsRetriever allowedDomainsRetriever)
        {
            var httpContext = instance.Target.ViewContext.HttpContext;
            var applicationPath = httpContext.Request.ApplicationPath;

            if (formId == 0)
            {
                throw new ArgumentException(nameof(formId));
            }

            var builderAssetsProviderFactory = Service.Resolve<IBuilderAssetsProviderFactory>();
            var requestContext = instance.Target.ViewContext.RequestContext;
            var assetsProvider = builderAssetsProviderFactory.Get<FormBuilderAssetsProvider, IFormBuilderAssetsProvider>(requestContext);


            var scriptBlock = assetsProvider.GetScriptTags()
                + assetsProvider.GetLocalizationScriptTags(MembershipContext.AuthenticatedUser.PreferredUICultureCode, CultureHelper.DefaultUICultureCode);

            var startupScript = assetsProvider.GetStartupScriptTag(new FormBuilderScriptConfiguration
            {
                FormIdentifier = formId,
                ApplicationPath = applicationPath,
                ConfigurationLoadEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.CONFIGURATION_LOAD_ROUTE_TEMPLATE.Replace("{formId}", formId.ToString())}",
                ConfigurationStoreEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.CONFIGURATION_STORE_ROUTE_TEMPLATE}",
                MetadataEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.METADATA_ROUTE_TEMPLATE.Replace("{formId}", formId.ToString())}",
                AllowedOrigins = allowedDomainsRetriever.Retrieve(),
                PropertiesEditorClientId = propertiesEditorClientId,
                SaveMessageClientId = saveMessageClientId,
                PropertiesEditorEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.FORMBUILDER_PROPERTIES_TAB_ROUTE_TEMPLATE.Replace("{formId}", formId.ToString())}",
                ValidationRuleMetadataEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.VALIDATION_RULE_METADATA_ROUTE_TEMPLATE}",
                ValidationRuleMarkupEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.FORMBUILDER_GET_VALIDATION_RULE_CONFIGURATION_MARKUP_ROUTE_TEMPLATE.Replace("{formId}", formId.ToString())}",
                VisibilityConditionMarkupEndpoint = $"{applicationPath?.TrimEnd('/')}/{FormBuilderRoutes.FORMBUILDER_GET_VISIBILITY_CONDITION_CONFIGURATION_MARKUP_ROUTE_TEMPLATE.Replace("{formId}", formId.ToString())}",
                DevelopmentMode = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDebugBuilderScripts"], false),
            }, new FormBuilderPathDecorator());

            return new HtmlString(
                scriptBlock +
                assetsProvider.GetJQueryScriptBundleTags(true) +
                assetsProvider.GetFormsScriptBundleTag() +
                startupScript
            );
        }


        /// <summary>
        /// Renders the <paramref name="explanationText"/> to a form field.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="explanationText">Explanation text to be displayed.</param>
        /// <param name="configuration">Configuration object for the wrapping element.</param>
        /// <remarks>If the <paramref name="configuration"/> is null, the method returns raw <paramref name="explanationText"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> is null.</exception>
        public static MvcHtmlString WrappedExplanationText(this ExtensionPoint<HtmlHelper> htmlHelper, string explanationText, ElementRenderingConfiguration configuration = null)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (String.IsNullOrEmpty(explanationText))
            {
                return MvcHtmlString.Empty;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(htmlHelper.BeginWrappingElement(configuration));
            stringBuilder.Append(HttpUtility.HtmlEncode(ResHelper.LocalizeString(explanationText)));
            stringBuilder.Append(htmlHelper.EndWrappingElement(configuration));

            return new MvcHtmlString(stringBuilder.ToString());
        }


        /// <summary>
        /// Returns immediately executed JavaScript block that notifies Form builder that properties have been validated.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="propertiesPanel">Instance representing Form builder's properties panel.</param>
        /// <param name="areValid">True, if properties were validated successfully, false otherwise.</param>
        public static string NotifyPropertiesValidated(this ExtensionPoint<HtmlHelper> instance, PropertiesPanel propertiesPanel, bool areValid)
        {
            if (propertiesPanel == null)
            {
                throw new ArgumentNullException(nameof(propertiesPanel));
            }

            if (propertiesPanel.InstanceIdentifier == default(Guid))
            {
                throw new ArgumentException($"Argument '{nameof(propertiesPanel)}' has no '{propertiesPanel.InstanceIdentifier}' set.");
            }

            if (!propertiesPanel.NotifyFormBuilder)
            {
                return String.Empty;
            }

            var data = new
            {
                detail = new
                {
                    identifier = propertiesPanel.InstanceIdentifier,
                    properties = propertiesPanel.UpdatedProperties
                }
            };


            return areValid ? GetNotifyFormBuilderScript("kenticoPropertiesStateChanged", data) : NotifyFormBuilderToUnfreezeFormComponentSelection();
        }


        /// <summary>
        /// Returns immediately executed JavaScript block that notifies Form builder that <see cref="ValidationRule"/> configuration has been validated.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="validationRuleForm">Instance representing <see cref="FormComponent{TProperties, TValue}"/>'s validation rule form.</param>
        /// <param name="areValid">True, if validation rule configuration is validated successfully, false otherwise.</param>
        public static string NotifyValidationRuleConfigurationValidated(this ExtensionPoint<HtmlHelper> instance, ValidationRuleForm validationRuleForm, bool areValid)
        {
            if (validationRuleForm == null)
            {
                throw new ArgumentNullException(nameof(validationRuleForm));
            }

            if (validationRuleForm.ValidationRuleConfiguration == null)
            {
                throw new ArgumentException($"Argument '{nameof(validationRuleForm)}' has no '{nameof(validationRuleForm.ValidationRuleConfiguration)}' set.");
            }

            if (!validationRuleForm.NotifyFormBuilder)
            {
                return String.Empty;
            }

            var data = new
            {
                detail = new
                {
                    identifier = validationRuleForm.FormComponentInstanceIdentifier,
                    validationRuleConfiguration = validationRuleForm.ValidationRuleConfiguration
                }
            };

            return areValid ? GetNotifyFormBuilderScript("kenticoValidationRuleStateChanged", data) : NotifyFormBuilderToUnfreezeFormComponentSelection();
        }


        /// <summary>
        /// Returns immediately executed JavaScript block that notifies Form builder that <see cref="ValidationRule"/> configuration has been validated.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="visibilityConditionForm">Instance representing <see cref="FormComponent{TProperties, TValue}"/>'s visibility condition form.</param>
        /// <param name="areValid">True, if visibility condition configuration is validated successfully, false otherwise.</param>
        public static string NotifyVisibilityConditionValidated(this ExtensionPoint<HtmlHelper> instance, VisibilityConditionForm visibilityConditionForm, bool areValid)
        {
            if (visibilityConditionForm == null)
            {
                throw new ArgumentNullException(nameof(visibilityConditionForm));
            }

            if (!visibilityConditionForm.NotifyFormBuilder)
            {
                return String.Empty;
            }

            if (visibilityConditionForm.NotifyFormBuilder && visibilityConditionForm.VisibilityConditionConfiguration == null)
            {
                throw new InvalidOperationException("Notifying form builder without visibility condition configuration is invalid.");
            }

            var data = new
            {
                detail = new
                {
                    identifier = visibilityConditionForm.FormComponentInstanceIdentifier,
                    visibilityConditionConfiguration = visibilityConditionForm.VisibilityConditionConfiguration
                }
            };

            return areValid ? GetNotifyFormBuilderScript("kenticoVisibilityConditionStateChanged", data) : NotifyFormBuilderToUnfreezeFormComponentSelection();
        }


        /// <summary>
        /// Returns submit button to submit a form with <paramref name="formId"/>.
        /// Pressing the submit button causes Form builder UI to freeze as well as disables the button to prevent multiple submissions.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">Identifier of the form in the DOM on which submit the render button should be disabled.</param>
        /// <param name="submitButtonValue">Value of the submit button.</param>
        /// <param name="submitButtonId">Identifier of the submit button element in the DOM.</param>
        /// <returns></returns>
        public static string FormBuilderSubmitButton(this ExtensionPoint<HtmlHelper> instance, string formId, string submitButtonValue, Guid submitButtonId)
        {
            var scriptBuilder = new TagBuilder("script");
            scriptBuilder.InnerHtml += @"document.getElementById('" + formId + @"').addEventListener('submit', function () {
                document.getElementById('" + submitButtonId + @"').setAttribute('disabled', 'disabled');
            });";

            var inputBuilder = new TagBuilder("input");
            inputBuilder.MergeAttribute("id", submitButtonId.ToString());
            inputBuilder.MergeAttribute("type", "submit");
            inputBuilder.MergeAttribute("onclick", "document.dispatchEvent(new CustomEvent('kenticoFormBuilderFreezeUI'));");
            inputBuilder.MergeAttribute("value", submitButtonValue);
            inputBuilder.AddCssClass("ktc-btn ktc-btn-primary");

            return scriptBuilder.ToString() + inputBuilder.ToString();
        }


        /// <summary>
        /// Returns an input element of type <c>image</c> whose <c>src</c> attribute is set to given <paramref name="src"/>.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="src">Source of the image.</param>
        /// <param name="htmlAttributes">An object containing additional HTML attributes for the input.</param>
        /// <returns>An HTML input element of type </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public static MvcHtmlString ImageInput(this ExtensionPoint<HtmlHelper> instance, string src, IDictionary<string, object> htmlAttributes = null)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.Attributes.Add("type", "image");
            var urlHelper = new UrlHelper(instance.Target.ViewContext.RequestContext);
            tagBuilder.Attributes.Add("src", urlHelper.Content(src));
            tagBuilder.MergeAttributes(htmlAttributes, true);

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }


        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="updateTargetId">
        /// Gets or sets the ID of the DOM element to update by using the response from the server when form needs to be re-rendered e.g. due to the application of visibility condition.
        /// If null then 'ajaxOptions.UpdateTargetId' is used.
        /// </param>
        /// <param name="routeName">The name of the route to use to obtain the form post-URL.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>Returns an object representing updatable form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty or both <paramref name="updateTargetId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        public static UpdatableMvcForm BeginRouteUpdatableForm(this ExtensionPoint<HtmlHelper> instance, string formId, string updateTargetId, string routeName = null, RouteValueDictionary routeValues = null, IDictionary<string, object> htmlAttributes = null)
        {
            ValidateUpdatableFormParameters(instance, formId, updateTargetId);

            var updateTargetElementId = updateTargetId;
            var attributes = GetUpdatableFormHtmlAttributes(formId, updateTargetElementId, htmlAttributes);
            attributes.Add("onsubmit", "window.kentico.updatableFormHelper.submitForm(event);");

            var form = instance.Target.BeginRouteForm(routeName, routeValues, FormMethod.Post, attributes);
            var updatableForm = new UpdatableMvcForm(formId, form, instance.Target.ViewContext, instance.Target.ViewDataContainer, instance.Target.RouteCollection);
            updatableForm.StoreInViewData(instance.Target.ViewData);

            return updatableForm;
        }


        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="updateTargetId">
        /// Gets or sets the ID of the DOM element to update by using the response from the server when form needs to be re-rendered e.g. due to the application of visibility condition.
        /// If null then 'ajaxOptions.UpdateTargetId' is used.
        /// </param>
        /// <param name="actionName">The name of the action method that will handle the request.</param>
        /// <param name="controllerName">The name of the controller that will handle the request.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>Returns an object representing updatable form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty or both <paramref name="updateTargetId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        public static UpdatableMvcForm BeginUpdatableForm(this ExtensionPoint<HtmlHelper> instance, string formId, string updateTargetId, string actionName = null, string controllerName = null, RouteValueDictionary routeValues = null, IDictionary<string, object> htmlAttributes = null)
        {
            ValidateUpdatableFormParameters(instance, formId, updateTargetId);

            var updateTargetElementId = updateTargetId;
            var attributes = GetUpdatableFormHtmlAttributes(formId, updateTargetElementId, htmlAttributes);
            attributes.Add("onsubmit", "window.kentico.updatableFormHelper.submitForm(event);");

            var form = instance.Target.BeginForm(actionName, controllerName, routeValues, FormMethod.Post, attributes);
            var updatableForm = new UpdatableMvcForm(formId, form, instance.Target.ViewContext, instance.Target.ViewDataContainer, instance.Target.RouteCollection);
            updatableForm.StoreInViewData(instance.Target.ViewData);

            return updatableForm;
        }


        /// <summary>
        /// Ends the form and disposes of all form resources.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="BeginUpdatableForm(ExtensionPoint{HtmlHelper}, string, string, string, string, RouteValueDictionary, IDictionary{string, object})"/> was not called before.
        /// </exception>
        public static void EndUpdatableForm(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var updatableForm = UpdatableMvcForm.GetUpdatableMvcFormFromViewData(instance.Target.ViewData) ?? throw new InvalidOperationException("Method 'Kentico.Forms.Web.Mvc.BeginForm' has to be called before.");
            updatableForm?.Dispose();
        }


        /// <summary>
        /// Returns immediately executed JavaScript block which enables form identified with <paramref name="formId"/> to be updatable upon changes of the form data.
        /// Script block must be positioned under the form element identified with <paramref name="formId"/>.
        /// Form has to contain attribute 'ktc-data-ajax-update' or 'data-ajax-update' to identify element where the form is to be re-rendered.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">Identifier of the form in the DOM.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty.</exception>
        public static string RegisterUpdatableFormScript(this ExtensionPoint<HtmlHelper> instance, string formId)
        {
            if (String.IsNullOrEmpty(formId))
            {
                throw new ArgumentException(nameof(formId));
            }

            var scriptBuilder = new TagBuilder("script");
            scriptBuilder.InnerHtml = GetUpdatableFormScript(formId);
            return scriptBuilder.ToString();
        }


        /// <summary>
        /// Returns JavaScript block which enables form identified with <paramref name="formId"/> to be updatable upon changes of the form data.
        /// </summary>
        internal static string GetUpdatableFormScript(string formId)
        {
            var config = new
            {
                FormId = formId,
                TargetAttributeName = UpdatableMvcForm.UPDATE_TARGET_ID,
                UnobservedAttributeName = UpdatableMvcForm.NOT_OBSERVED_ELEMENT_ATTRIBUTE_NAME
            };

            var configJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            // If the form is loaded asynchronously the DOMContentLoaded event has already fired,
            // in that case we need to check the document's ready state and execute the script right away.

            return $@"
if (document.readyState === 'complete') {{
  window.kentico.updatableFormHelper.registerEventListeners({configJson});
}} else {{
    document.addEventListener('DOMContentLoaded', function(event) {{
      window.kentico.updatableFormHelper.registerEventListeners({configJson});
    }});
}}";
        }


        /// <summary>
        /// Checks whether given parameters are in valid state. Otherwise throws exception.
        /// </summary>
        private static void ValidateUpdatableFormParameters(this ExtensionPoint<HtmlHelper> instance, string formId, string updateTargetId)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (String.IsNullOrEmpty(formId))
            {
                throw new ArgumentException("Argument 'formId' is null or empty.");
            }

            if (String.IsNullOrEmpty(updateTargetId))
            {
                throw new ArgumentException($"ID of the DOM element to update when form needs to be re-rendered is not set. Argument 'updateTargetId' cannot be null or empty.");
            }
        }


        /// <summary>
        /// Creates HTML attributes for the form.
        /// </summary>
        private static Dictionary<string, object> GetUpdatableFormHtmlAttributes(string formId, string updateTargetId, IDictionary<string, object> htmlAttributes)
        {
            return new Dictionary<string, object>(htmlAttributes ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase)
            {
                { "id", formId },
                { UpdatableMvcForm.UPDATE_TARGET_ID, "#" + updateTargetId }
            };
        }


        /// <summary>
        /// Returns immediately executed JavaScript block that dispatches an <paramref name="eventName"/> with serialized <paramref name="data"/>.
        /// </summary>
        private static string GetNotifyFormBuilderScript(string eventName, object data = null)
        {
            var updatedPropertiesJson = data == null ? "{}" : JsonConvert.SerializeObject(data, FormBuilderConfigurationSerializer.GetSettings());
            var dispatchSaveFormBuilderStateEvent = $"document.dispatchEvent(new CustomEvent(\"{eventName}\", {updatedPropertiesJson}));";

            var tagBuilder = new TagBuilder("script");
            tagBuilder.InnerHtml = dispatchSaveFormBuilderStateEvent;

            return tagBuilder.ToString();
        }


        /// <summary>
        /// Returns immediately executed JavaScript block that notifies Form builder to unfreeze.
        /// </summary>
        private static string NotifyFormBuilderToUnfreezeFormComponentSelection()
        {
            return GetNotifyFormBuilderScript("kenticoFormBuilderUnfreezeUI");
        }
    }
}
