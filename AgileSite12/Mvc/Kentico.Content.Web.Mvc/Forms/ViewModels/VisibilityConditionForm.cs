using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// View model for rendering a <see cref="VisibilityCondition"/> configuration.
    /// </summary>
    public class VisibilityConditionForm
    {
        /// <summary>
        /// Gets or sets ID of a biz form whose fields are being edited.
        /// </summary>
        public int FormId { get; set; }


        /// <summary>
        /// Gets or sets <see cref="Mvc.VisibilityConditionConfiguration"/> for displaying the form used for <see cref="VisibilityCondition"/>'s configuration.
        /// </summary>
        public VisibilityConditionConfiguration VisibilityConditionConfiguration { get; set; }


        /// <summary>
        /// Gets or sets name of a field the visibility condition belongs to.
        /// </summary>
        public string FormFieldName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the currently edited instance of <see cref="FormComponent{TProperties, TValue}"/>.
        /// </summary>
        public Guid FormComponentInstanceIdentifier { get; set; }


        /// <summary>
        /// Collection of form components used for configuring <see cref="VisibilityCondition"/>.
        /// </summary>
        public IEnumerable<FormComponent> FormComponents { get; set; }

        /// <summary>
        /// Represents localized label for the selected visibility condition dropdown.
        /// </summary>
        /// <seealso cref="SelectedVisibilityConditionIdentifier"/>
        public string SelectedVisibilityConditionLocalizedLabel { get; set; }

        /// <summary>
        /// Contains value of the selected item in the list represented by <see cref="AvailableVisibilityConditions"/>.
        /// </summary>
        /// <see cref="AvailableVisibilityConditions"/>
        public string SelectedVisibilityConditionIdentifier { get; set; }


        /// <summary>
        /// Represents list of all available visibility conditions.
        /// </summary>
        /// <seealso cref="SelectedVisibilityConditionIdentifier"/>
        public IEnumerable<SelectListItem> AvailableVisibilityConditions { get; set; }


        /// <summary>
        /// Decides whether Form builder should be notified about validation of <see cref="VisibilityCondition"/> configuration.
        /// </summary>
        public bool NotifyFormBuilder { get; set; }


        /// <summary>
        /// Returns true if current visibility condition is valid for current form component.
        /// </summary>
        /// <remarks>
        /// Visibility condition is invalid e.g. when visibility condition implements <see cref="IAnotherFieldVisibilityCondition"/>
        /// and depending field is in order after current form component.
        /// </remarks>
        public bool IsVisibilityConditionValid { get; set; }
    }
}
