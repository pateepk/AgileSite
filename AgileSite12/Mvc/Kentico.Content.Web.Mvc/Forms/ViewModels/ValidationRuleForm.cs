using System;
using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// View model for rendering a <see cref="ValidationRule"/> configuration.
    /// </summary>
    public class ValidationRuleForm
    {
        /// <summary>
        /// Gets or sets identifier of the currently edited instance of <see cref="FormComponent{TProperties, TValue}"/>.
        /// </summary>
        public Guid FormComponentInstanceIdentifier { get; set; }


        /// <summary>
        /// Gets or sets ID of a biz form whose fields are being edited.
        /// </summary>
        public int FormId { get; set; }


        /// <summary>
        /// Gets or sets name of a field the validation rule belongs to.
        /// </summary>
        public string FieldName { get; set; }


        /// <summary>
        /// Decides whether Form builder should be notified about validation of <see cref="ValidationRule"/> configuration.
        /// </summary>
        /// <seealso cref="ValidationRuleConfiguration"/>
        public bool NotifyFormBuilder { get; set; }


        /// <summary>
        /// Collection of form components used for configuring <see cref="ValidationRule"/>.
        /// </summary>
        public IEnumerable<FormComponent> FormComponents { get; set; }


        /// <summary>
        /// Gets or sets <see cref="ValidationRuleConfiguration"/> for displaying the form used for <see cref="ValidationRule"/>'s configuration.
        /// </summary>
        public ValidationRuleConfiguration ValidationRuleConfiguration { get; set; }
    }
}
