using System;

using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="HiddenGuidInputProperties"/>.
    /// </summary>
    public class HiddenGuidInputProperties : FormComponentProperties<Guid>
    {
        /// <summary>
        /// Gets or sets default value for <see cref="HiddenGuidInputProperties"/>.
        /// </summary>
        public override Guid DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Returns an empty string.
        /// </summary>
        public override string Label
        {
            get { return String.Empty; }
            set {  }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenGuidInputProperties"/> class.
        /// </summary>
        public HiddenGuidInputProperties()
            : base(FieldDataType.Guid)
        {
        }
    }
}