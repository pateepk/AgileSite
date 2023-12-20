using System;

using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="CompareToFieldSelectorComponent"/>.
    /// </summary>
    public class CompareToFieldSelectorProperties : FormComponentProperties<Guid>
    {
        /// <summary>
        /// Gets or sets the default value of the form component.
        /// </summary>
        public override Guid DefaultValue { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompareToFieldSelectorProperties"/> class.
        /// </summary>
        public CompareToFieldSelectorProperties()
            : base(FieldDataType.Unknown)
        {
        }

    }
}