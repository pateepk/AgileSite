using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Evaluator of form component visibility conditions.
    /// </summary>
    public class FormComponentVisibilityEvaluator : IFormComponentVisibilityEvaluator
    {
        /// <summary>
        /// Evaluates <see cref="VisibilityCondition"/> of given <paramref name="component"/>. 
        /// If the component depends on another fields, the visibility condition is evaluated against provided <paramref name="visibleComponents"/>.
        /// </summary>
        /// <param name="component">Form component which visibility condition is evaluated.</param>
        /// <param name="visibleComponents">Visible form components.</param>
        /// <returns>
        /// <c>true</c> if the component is visible or visibility condition is <c>null</c>, otherwise <c>false</c>.
        /// <c>false</c> is also returned when visibility condition depends on another field which is not present in <paramref name="visibleComponents"/> enumeration.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> or <paramref name="visibleComponents"/> is <c>null</c>.</exception>
        public bool IsComponentVisible(FormComponent component, IEnumerable<FormComponent> visibleComponents)
        {

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (visibleComponents == null)
            {
                throw new ArgumentNullException(nameof(visibleComponents));
            }

            var visibilityCondition = component.BaseProperties.VisibilityConditionConfiguration?.VisibilityCondition;
            if (visibilityCondition == null)
            {
                return true;
            }
                        
            if (visibilityCondition is IAnotherFieldVisibilityCondition visibilityDependingOnAnotherField)
            {
                var dependeeComponent = visibleComponents.FirstOrDefault(c => c.BaseProperties.Guid == visibilityDependingOnAnotherField.DependeeFieldGuid);
                    
                if (dependeeComponent != null)
                {
                    visibilityDependingOnAnotherField.SetDependeeFieldValue(dependeeComponent.GetObjectValue());
                }
                else
                {
                    return false;
                }
            }

            return visibilityCondition.IsVisible();
        }
    }
}