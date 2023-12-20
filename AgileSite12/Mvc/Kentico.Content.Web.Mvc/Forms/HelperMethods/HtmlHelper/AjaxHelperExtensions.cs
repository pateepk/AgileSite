using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides system extension methods for <see cref="AjaxHelper"/>.
    /// </summary>
    [Obsolete("This extension methods are obsolete. They were not intended for public use.")]
    public static class AjaxHelperExtensions
    {
        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="instance">AjaxHelper extension point.</param>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="ajaxOptions">An object that provides options for the asynchronous request.</param>
        /// <param name="updateTargetId">
        /// Gets or sets the ID of the DOM element to update by using the response from the server when form needs to be re-rendered e.g. due to the application of visibility condition.
        /// If null then 'ajaxOptions.UpdateTargetId' is used.
        /// </param>
        /// <param name="routeName">The name of the route to use to obtain the form post-URL.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>Returns an object representing updatable form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty or both <paramref name="updateTargetId"/> and <see cref="AjaxOptions.UpdateTargetId"/> are null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        public static UpdatableMvcForm BeginRouteForm(this ExtensionPoint<AjaxHelper> instance, string formId, AjaxOptions ajaxOptions, string updateTargetId = null, string routeName = null, RouteValueDictionary routeValues = null, IDictionary<string, object> htmlAttributes = null)
        {
            ValidateParameters(instance, formId, ajaxOptions, updateTargetId);

            var updateTargetElementId = String.IsNullOrEmpty(updateTargetId) ? ajaxOptions?.UpdateTargetId : updateTargetId;
            var attributes = GetHtmlAttributes(formId, updateTargetElementId, htmlAttributes);
            var form = instance.Target.BeginRouteForm(routeName, routeValues, ajaxOptions, attributes);
            var updatableForm = new UpdatableMvcForm(formId, form, instance.Target.ViewContext, instance.Target.ViewDataContainer, instance.Target.RouteCollection);
            updatableForm.StoreInViewData(instance.Target.ViewData);

            return updatableForm;
        }


        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="instance">AjaxHelper extension point.</param>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="ajaxOptions">An object that provides options for the asynchronous request.</param>
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
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty or both <paramref name="updateTargetId"/> and <see cref="AjaxOptions.UpdateTargetId"/> are null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        public static UpdatableMvcForm BeginForm(this ExtensionPoint<AjaxHelper> instance, string formId, AjaxOptions ajaxOptions, string updateTargetId = null, string actionName = null, string controllerName = null, RouteValueDictionary routeValues = null, IDictionary<string, object> htmlAttributes = null)
        {
            ValidateParameters(instance, formId, ajaxOptions, updateTargetId);

            var updateTargetElementId = String.IsNullOrEmpty(updateTargetId) ? ajaxOptions?.UpdateTargetId : updateTargetId;
            var attributes = GetHtmlAttributes(formId, updateTargetElementId, htmlAttributes);
            var form = instance.Target.BeginForm(actionName, controllerName, routeValues, ajaxOptions, attributes);
            var updatableForm = new UpdatableMvcForm(formId, form, instance.Target.ViewContext, instance.Target.ViewDataContainer, instance.Target.RouteCollection);
            updatableForm.StoreInViewData(instance.Target.ViewData);

            return updatableForm;
        }


        /// <summary>
        /// Ends the form and disposes of all form resources.
        /// </summary>
        /// <param name="instance">AjaxHelper extension point.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="BeginForm(ExtensionPoint{AjaxHelper}, string, AjaxOptions, string, string, string, RouteValueDictionary, IDictionary{string, object})"/> was not called before.
        /// </exception>
        public static void EndForm(this ExtensionPoint<AjaxHelper> instance)
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
        /// <param name="instance">AjaxHelper extension point.</param>
        /// <param name="formId">Identifier of the form in the DOM.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> is null or empty.</exception>
        public static string RegisterUpdatableFormScript(this ExtensionPoint<AjaxHelper> instance, string formId)
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
        private static void ValidateParameters(this ExtensionPoint<AjaxHelper> instance, string formId, AjaxOptions ajaxOptions, string updateTargetId)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (String.IsNullOrEmpty(formId))
            {
                throw new ArgumentException("Argument 'formId' is null or empty.");
            }

            if (String.IsNullOrEmpty(ajaxOptions?.UpdateTargetId) && String.IsNullOrEmpty(updateTargetId))
            {
                throw new ArgumentException($"ID of the DOM element to update when form needs to be re-rendered is not set. Both arguments 'ajaxOptions.UpdateTargetId' and '{updateTargetId}' cannot be null or empty.");
            }
        }


        /// <summary>
        /// Creates HTML attributes for the form.
        /// </summary>
        private static Dictionary<string, object> GetHtmlAttributes(string formId, string updateTargetId, IDictionary<string, object> htmlAttributes)
        {
            return new Dictionary<string, object>(htmlAttributes ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase)
            {
                { "id", formId },
                { UpdatableMvcForm.UPDATE_TARGET_ID, "#" + updateTargetId }
            };
        }
    }
}

