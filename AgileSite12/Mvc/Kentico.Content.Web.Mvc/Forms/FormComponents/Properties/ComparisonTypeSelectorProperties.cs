using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="ComparisonTypeSelectorComponent{TComparisonEnum}"/>.
    /// </summary>
    public class ComparisonTypeSelectorProperties<TComparisonEnum> : FormComponentProperties<TComparisonEnum>
    {
        /// <summary>
        /// Gets or sets the default value of the form component.
        /// </summary>
        public override TComparisonEnum DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonTypeSelectorProperties{TComparisonEnum}"/> class.
        /// </summary>
        public ComparisonTypeSelectorProperties()
            : base(FieldDataType.Unknown)
        {
        }
    }
}