using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Contains helper methods for rendering HTML for forms built using Form builder.
    /// </summary>
    public static class FormExtensions
    {
        private const string FORM_CONTEXT_DATA = "kentico_end_form_context_data";

        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="instance">>HtmlHelper extension point.</param>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="updateTargetId">Gets or sets the ID of the DOM element to update by using the response from the server when form needs to be re-rendered e.g. due to the application of visibility condition.</param>
        /// <param name="routeName">The name of the route to use to obtain the form post-URL.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="formMethod">Enumeration of the HTTP request types for a form.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>Returns an object representing updatable form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> or <paramref name="updateTargetId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        public static UpdatableMvcForm BeginRouteForm(this ExtensionPoint<HtmlHelper> instance, string formId, string updateTargetId, string routeName = null, RouteValueDictionary routeValues = null, FormMethod formMethod = FormMethod.Post, IDictionary<string, object> htmlAttributes = null)
        {
            ValidateParameters(instance, formId, updateTargetId);

            var attributes = GetHtmlAttributes(formId, updateTargetId, htmlAttributes);
            MvcForm form = instance.Target.BeginRouteForm(routeName, routeValues, formMethod, attributes);
            var updatableForm = new UpdatableMvcForm(formId, form, instance.Target.ViewContext, instance.Target.ViewDataContainer, instance.Target.RouteCollection);

            StoreBeginFormFata(instance, updatableForm);

            return updatableForm;
        }


        /// <summary>
        /// Writes an opening form tag to the response.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="formId">Gets or sets element's ID for the form.</param>
        /// <param name="updateTargetId">
        /// Gets or sets the ID of the DOM element to update by using the response from the server when form needs to be re-rendered e.g. due to the application of visibility condition.
        /// </param>
        /// <param name="actionName">The name of the action method that will handle the request.</param>
        /// <param name="controllerName">The name of the controller that will handle the request.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="formMethod">Enumeration of the HTTP request types for a form.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>Returns an object representing updatable form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formId"/> or <paramref name="updateTargetId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="UpdatableMvcForm"/> are nested.</exception>
        public static UpdatableMvcForm BeginForm(this ExtensionPoint<HtmlHelper> instance, string formId, string updateTargetId, string actionName = null, string controllerName = null, RouteValueDictionary routeValues = null, FormMethod formMethod = FormMethod.Post, IDictionary<string, object> htmlAttributes = null)
        {
            ValidateParameters(instance, formId, updateTargetId);

            var attributes = GetHtmlAttributes(formId, updateTargetId, htmlAttributes);
            MvcForm form = instance.Target.BeginForm(actionName, controllerName, routeValues, formMethod, attributes);
            var updatableForm = new UpdatableMvcForm(formId, form, instance.Target.ViewContext, instance.Target.ViewDataContainer, instance.Target.RouteCollection);

            StoreBeginFormFata(instance, updatableForm);

            return updatableForm;
        }


        /// <summary>
        /// Ends the form and disposes of all form resources.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="BeginForm(ExtensionPoint{HtmlHelper}, string, string, string, string, RouteValueDictionary, FormMethod, IDictionary{string, object})" />
        /// OR <see cref="BeginRouteForm(ExtensionPoint{HtmlHelper}, string, string, string, RouteValueDictionary, FormMethod, IDictionary{string, object})"/> was not called before.
        /// </exception>
        public static void EndForm(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var updatableForm = instance.Target.ViewContext.ViewData[FORM_CONTEXT_DATA] as UpdatableMvcForm ?? throw new InvalidOperationException($"Method '{nameof(BeginForm)}' or '{nameof(BeginRouteForm)}' has to be called before.");
            instance.Target.ViewContext.ViewData.Remove(FORM_CONTEXT_DATA);
            updatableForm.Dispose();
        }


        /// <summary>
        /// Passes data required in EndForm via ViewData.
        /// </summary>
        private static void StoreBeginFormFata(this ExtensionPoint<HtmlHelper> instance, UpdatableMvcForm updatableForm)
        {
            // Pass data required in EndForm method via ViewData
            if (instance.Target.ViewContext.ViewData.ContainsKey(FORM_CONTEXT_DATA))
            {
                throw new InvalidOperationException("Forms cannot be nested.");
            }

            instance.Target.ViewContext.ViewData.Add(FORM_CONTEXT_DATA, updatableForm);
        }


        /// <summary>
        /// Checks whether given parameters are in valid state. Otherwise throws exception.
        /// </summary>
        private static void ValidateParameters(this ExtensionPoint<HtmlHelper> instance, string formId, string updateTargetId)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (String.IsNullOrEmpty(formId))
            {
                throw new ArgumentException($"Argument {nameof(formId)} is null or empty.");
            }

            if (String.IsNullOrEmpty(updateTargetId))
            {
                throw new ArgumentException($"Argument {nameof(updateTargetId)} is null or empty.");
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


        /// <summary>
        /// Renders validation messages for a form component.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="component">Component to render validation messages for.</param>
        /// <param name="supressValidationMessages">If true no validation messages are rendered.</param>
        /// <returns>Validation messages markup.</returns>
        public static MvcHtmlString ValidationMessage(this ExtensionPoint<HtmlHelper> htmlHelper, FormComponent component, bool supressValidationMessages = false)
        {
            if (supressValidationMessages)
            {
                return MvcHtmlString.Empty;
            }

            var errors = new List<string>();
            var props = component.GetType()
                                 .GetProperties()
                                 .Where(p => p.GetCustomAttributes<BindablePropertyAttribute>(false).Any())
                                 .Select(p => p.Name);

            if (component.ShowPartialValidationMessages)
            {
                var partialErrors = props.Select(p => htmlHelper.Target.ValidationMessage($"{component.Name}.{p}", new { }, "div").ToString());

                errors.AddRange(partialErrors);
            }

            errors.Add(htmlHelper.Target.ValidationMessage(component.Name, new { }, "div").ToString());

            return MvcHtmlString.Create(string.Join("", errors));
        }


        /// <summary>
        /// Returns a file upload input element.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper extension point.</param>
        /// <param name="htmlAttributes">An object containing additional HTML attributes for the label.</param>
        /// <returns>A file upload input element.</returns>
        public static MvcHtmlString File(this ExtensionPoint<HtmlHelper> htmlHelper, IDictionary<string, object> htmlAttributes)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", "file");

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
        }
    }
}
