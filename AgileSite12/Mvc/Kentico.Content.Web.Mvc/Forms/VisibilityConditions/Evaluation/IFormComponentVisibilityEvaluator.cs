using System;
using System.Collections.Generic;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentVisibilityEvaluator), typeof(FormComponentVisibilityEvaluator), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines interface for evaluator of form component visibility conditions.
    /// </summary>
    public interface IFormComponentVisibilityEvaluator
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
        bool IsComponentVisible(FormComponent component, IEnumerable<FormComponent> visibleComponents);
    }
}