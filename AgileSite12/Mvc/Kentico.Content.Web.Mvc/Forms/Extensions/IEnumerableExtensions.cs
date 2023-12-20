using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Extensions methods for <see cref="IEnumerable{FormComponent}"/> interface.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns filtered components which will be displayed in form based on given parameters.
        /// </summary>
        /// <param name="components">Original collection of components.</param>
        /// <param name="contact">Contact for which the filter is performed, can be null.</param>
        /// <param name="formInfo">Form representation.</param>
        /// <param name="formItem">Existing form item, can be null.</param>
        /// <param name="visibilityEvaluator">Form component visibility evaluator.</param>
        /// <returns>Filtered components.</returns>
        public static IEnumerable<FormComponent> GetDisplayedComponents(this IEnumerable<FormComponent> components, ContactInfo contact, BizFormInfo formInfo, BizFormItem formItem, IFormComponentVisibilityEvaluator visibilityEvaluator)
        {
            var fields = formInfo.Form.GetFields(true, false, false);
            var containsSmartFields = fields.Any(i => i.IsSmartField());

            if (!containsSmartFields || contact == null || !SmartFieldLicenseHelper.HasLicense())
            {
                return GetVisibleFormComponents(components, visibilityEvaluator);
            }

            var limit = fields.Count(i => !i.IsSmartField());
            limit = Math.Max(limit, SmartFieldConstants.MINIMUM_DISPLAYED_FIELD_COUNT);

            if (formItem == null)
            {
                return GetVisibleFormComponents(components, visibilityEvaluator).OrderBy(x => x.BaseProperties.SmartField).Take(limit);
            }
       
            return GetVisibleFormComponents(components, visibilityEvaluator, formItem).Where(component => formItem.GetValue(component.Name) == null).Take(limit);
        }


        private static IEnumerable<FormComponent> GetVisibleFormComponents(IEnumerable<FormComponent> components, IFormComponentVisibilityEvaluator visibilityEvaluator, BizFormItem formItem = null)
        {
            if (visibilityEvaluator == null)
            {
                return components;
            }
       
            var visibleComponents = new List<FormComponent>();
            foreach (var component in components)
            {
                var value = formItem?.GetValue(component.Name);
                if (value != null)
                {
                    component.SetObjectValue(value);
                }

                if (visibilityEvaluator.IsComponentVisible(component, visibleComponents))
                {
                    visibleComponents.Add(component);
                }
            }

            return visibleComponents;
        }
    }
}
