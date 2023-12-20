using System.Collections.Generic;

using CMS.DataEngine;

using Kentico.Forms.Web.Mvc;

namespace Kentico.Components.Web.Mvc.FormComponents.Internal
{
    /// <summary>
    /// Represents properties of the <see cref="PageSelector"/>.
    /// </summary>
    public abstract class PageSelectorPropertiesBase<TValue> : FormComponentProperties<IList<TValue>>
    {
        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        public override IList<TValue> DefaultValue { get; set; }


        /// <summary>
        /// Dialog root path.
        /// </summary>
        /// <remarks>
        /// If provided, the page selector dialog root will start from the given root path.
        /// If the property value not provided, site root is used.
        /// </remarks>
        public string RootPath { get; set; } = "/";


        /// <summary>
        /// Initializes a new instance of the <see cref="PageSelectorPropertiesBase{TValue}"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Unknown"/>.
        /// </remarks>
        protected PageSelectorPropertiesBase()
            : base(FieldDataType.Unknown)
        {
        }
    }
}
