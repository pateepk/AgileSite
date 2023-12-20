using System;
using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// View model for rendering a properties panel of a <see cref="FormComponent"/>.
    /// </summary>
    public class PropertiesPanel
    {
        /// <summary>
        /// Gets or sets identifier of the currently edited <see cref="FormComponent{TProperties, TValue}"/>.
        /// </summary>
        /// <seealso cref="FormComponentDefinition"/>
        public Guid InstanceIdentifier { get; set; }


        /// <summary>
        /// Gets or sets identifier of the currently edited BizFormInfo.
        /// </summary>
        public int FormId { get; set; }


        /// <summary>
        /// Gets or sets the name of currently edited form field.
        /// </summary>
        public string FormFieldName { get; set; }


        /// <summary>
        /// Gets or sets type identifier of the currently edited <see cref="FormComponent{TProperties, TValue}"/>.
        /// </summary>
        /// <seealso cref="InstanceIdentifier"/>
        public string TypeIdentifier { get; set; }


        /// <summary>
        /// Decides whether Form builder should be notified about properties validation.
        /// </summary>
        /// <seealso cref="UpdatedProperties"/>
        public bool NotifyFormBuilder { get; set; }


        /// <summary>
        /// Gets or sets collection of <see cref="FormComponent"/>s for editing properties in class of type <see cref="FormComponentProperties"/>
        /// assigned to a <see cref="FormComponent{TProperties, TValue}"/>. Edited <see cref="FormComponent{TProperties, TValue}"/> is defined via <see cref="InstanceIdentifier"/>.
        /// </summary>
        public IEnumerable<FormComponent> PropertiesFormComponents { get; set; }


        /// <summary>
        /// Gets or sets collection of key-value pairs representing <see cref="FormComponentProperties"/> property name with it's value.
        /// </summary>
        public Dictionary<string, object> UpdatedProperties { get; set; }
    }
}
