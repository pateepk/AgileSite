using System;
using System.Collections.Generic;
using System.Web.Mvc;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Selector component for enums specifying comparison type. The selected value is represented by its corresponding underlying type value.
    /// </summary>
    /// <typeparam name="TComparisonEnum">Type of enum whose values the component offers.</typeparam>
    public abstract class ComparisonTypeSelectorComponent<TComparisonEnum> : FormComponent<ComparisonTypeSelectorProperties<TComparisonEnum>, TComparisonEnum>
    {
        /// <summary>
        /// Represents the selector value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public TComparisonEnum ComparisonType { get; set; }


        /// <summary>
        /// Gets enumeration of available comparison types to populate the selector.
        /// </summary>
        public IEnumerable<SelectListItem> ComparisonTypes
        {
            get
            {
                return GetSelectListItems();
            }
        }


        /// <summary>
        /// Gets name of the <see cref="ComparisonType"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(ComparisonType);


        /// <summary>
        /// Gets enumeration of select list items whose value is the underlying type's value and text is a localized enum value. Item whose value
        /// corresponds to <see cref="ComparisonType"/> has its <see cref="SelectListItem.Selected"/> flag set to true.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<SelectListItem> GetSelectListItems()
        {
            var comparisonEnumType = typeof(TComparisonEnum);

            var prefix = GetResourceStringNamePrefix(comparisonEnumType.Name);
            var selectedComparisonTypeName = Enum.GetName(comparisonEnumType, ComparisonType);

            foreach (string enumValue in Enum.GetNames(comparisonEnumType))
            {
                yield return new SelectListItem { Value = Convert.ChangeType(Enum.Parse(comparisonEnumType, enumValue, false), comparisonEnumType.GetEnumUnderlyingType()).ToString(), Text = ResHelper.GetString(prefix + enumValue), Selected = (enumValue == selectedComparisonTypeName) };
            }
        }


        /// <summary>
        /// Gets resource string name prefix for enum named <paramref name="enumName"/>.
        /// </summary>
        protected virtual string GetResourceStringNamePrefix(string enumName)
        {
            return $"kentico.formbuilder.{enumName}.";
        }


        /// <summary>
        /// Gets value representing the currently selected enum value.
        /// </summary>
        /// <returns></returns>
        public override TComparisonEnum GetValue()
        {
            return ComparisonType;
        }


        /// <summary>
        /// Sets the currently selected enum value.
        /// </summary>
        /// <param name="value">Enum value.</param>
        public override void SetValue(TComparisonEnum value)
        {
            ComparisonType = value;
        }
    }
}