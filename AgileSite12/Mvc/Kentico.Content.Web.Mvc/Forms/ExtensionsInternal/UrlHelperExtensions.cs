using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using CMS.FormEngine;

using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides system extension methods for <see cref="UrlHelper"/>.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Gets URL for validating properties assigned to <see cref="FormComponent{TProperties, TValue}"/>.
        /// </summary>
        /// <param name="instance">UrlHelper extension point.</param>
        /// <param name="formComponentInstanceIdentifier">Identifier of <see cref="FormComponent{TProperties, TValue}"/> instance for which to validate properties assigned to it.</param>
        /// <param name="formComponentTypeIdentifier">Type identifier of <see cref="FormComponent{TProperties, TValue}"/> given in <paramref name="formComponentInstanceIdentifier"/>.</param>
        /// <param name="formId">Identifier of biz form being edited.</param>
        /// <param name="formFieldName">Name of form field the properties belong to.</param>
        public static string GetValidatePropertiesUrl(this ExtensionPoint<UrlHelper> instance, Guid formComponentInstanceIdentifier, string formComponentTypeIdentifier, int formId, string formFieldName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var routeValueDictionary = new RouteValueDictionary(new
            {
                formComponentInstanceIdentifier,
                formComponentTypeIdentifier,
                formId,
                formFieldName
            });

            var url = UrlHelper.GenerateUrl(FormBuilderRoutes.VALIDATE_FORMCOMPONENT_PROPERTIES_ROUTE_NAME, nameof(KenticoFormBuilderPropertiesPanelController.ValidateProperties),
                                           "KenticoFormBuilderPropertiesPanel", routeValueDictionary, RouteTable.Routes, instance.Target.RequestContext, false);

            return new FormBuilderPathDecorator().Decorate(url);
        }


        /// <summary>
        /// Gets URL for validating values used for configuiring <see cref="ValidationRule"/>.
        /// </summary>
        /// <param name="instance">UrlHelper extension point.</param>
        /// <param name="formComponentInstanceIdentifier">Identifier of <see cref="FormComponent{TProperties, TValue}"/> instance for which is edited.</param>
        /// <param name="validationRuleIdentifier">Identifier of <see cref="ValidationRule"/>.</param>
        /// <param name="formId">Identifier of biz form being edited.</param>
        /// <param name="formFieldName">Name of form field the properties belong to.</param>
        public static string GetValidateValidationRuleConfigurationUrl(this ExtensionPoint<UrlHelper> instance, Guid formComponentInstanceIdentifier, string validationRuleIdentifier, int formId, string formFieldName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (String.IsNullOrEmpty(validationRuleIdentifier))
            {
                throw new ArgumentException(nameof(validationRuleIdentifier));
            }
            if (String.IsNullOrEmpty(formFieldName))
            {
                throw new ArgumentException(nameof(formFieldName));
            }

            var routeValueDictionary = new RouteValueDictionary(new
            {
                formComponentInstanceIdentifier,
                validationRuleIdentifier,
                formId,
                formFieldName
            });

            var url = UrlHelper.GenerateUrl(FormBuilderRoutes.FORMBUILDER_VALIDATE_VALIDATION_RULE_CONFIGURATION_ROUTE_NAME, nameof(KenticoFormBuilderPropertiesPanelController.ValidateValidationRuleConfiguration),
                                           "KenticoFormBuilderPropertiesPanel", routeValueDictionary, RouteTable.Routes, instance.Target.RequestContext, false);

            return new FormBuilderPathDecorator().Decorate(url);
        }


        /// <summary>
        /// Gets URL for validating values used for configuiring <see cref="VisibilityCondition"/>.
        /// </summary>
        /// <param name="instance">UrlHelper extension point.</param>
        /// <param name="formId">Identifier of biz form being edited.</param>
        /// <param name="formFieldName">Name of the form field the properties belong to.</param>
        /// <param name="formComponentInstanceIdentifier">Identifier of biz form being edited.</param>
        public static string GetValidateVisibilityConditionConfigurationUrl(this ExtensionPoint<UrlHelper> instance, int formId, string formFieldName, Guid formComponentInstanceIdentifier)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var routeValueDictionary = new RouteValueDictionary(new
            {
                formId,
                formFieldName,
                formComponentInstanceIdentifier,
            });

            var url = UrlHelper.GenerateUrl(FormBuilderRoutes.FORMBUILDER_VALIDATE_VISIBILITY_CONDITION_CONFIGURATION_ROUTE_NAME, "ValidateVisibilityConditionConfiguration",
                                           "KenticoFormBuilderPropertiesPanel", routeValueDictionary, RouteTable.Routes, instance.Target.RequestContext, false);

            return new FormBuilderPathDecorator().Decorate(url);
        }


        /// <summary>
        /// Gets URL for accessing a form file.
        /// </summary>
        /// <param name="instance">UrlHelper extension point.</param>
        /// <param name="fileName">File name as returned by the <see cref="FormHelper.GetGuidFileName"/> method.</param>
        /// <param name="originalFileName">File name as returned by the <see cref="FormHelper.GetOriginalFileName"/> method.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <returns>Returns URL for accessing a form file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileName"/> is null or an empty string.</exception>
        public static string GetFormFileUrl(this ExtensionPoint<UrlHelper> instance, string fileName, string originalFileName, string siteName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("", nameof(fileName));
            }

            var contentPath = $"~/{FormBuilderRoutes.GET_FILE_ROUTE_TEMPLATE}?filename={HttpUtility.UrlEncode(fileName)}&originalfilename={HttpUtility.UrlEncode(originalFileName)}&sitename={HttpUtility.UrlEncode(siteName)}";
            var url = UrlHelper.GenerateContentUrl(contentPath, instance.Target.RequestContext.HttpContext);

            return new FormBuilderPathDecorator().Decorate(url);
        }
    }
}
