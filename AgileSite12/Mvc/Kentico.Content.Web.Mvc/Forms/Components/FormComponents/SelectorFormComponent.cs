using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides basic component support for selectors.
    /// </summary>
    /// <typeparam name="TProperties">Properties type of the form component.</typeparam>
    /// <remarks>
    /// Selected value is of <see cref="string"/> type.
    /// </remarks>
    public abstract class SelectorFormComponent<TProperties> : FormComponent<TProperties, string>
        where TProperties : SelectorProperties, new()
    {
        private IEnumerable<SelectListItem> mItems;


        /// <summary>
        /// Gets the collection of items populated in component.
        /// </summary>
        public IEnumerable<SelectListItem> Items
        {
            get => mItems ?? (mItems = GetItems());
            set => mItems = value;
        }


        /// <summary>
        /// Gets or sets value selected by component.
        /// </summary>
        [BindableProperty]
        public string SelectedValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets name of the <see cref="SelectedValue"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(SelectedValue);


        /// <summary>
        /// Gets the <see cref="SelectedValue"/> property.
        /// </summary>
        public override string GetValue()
        {
            return SelectedValue;
        }


        /// <summary>
        /// Sets the <see cref="SelectedValue"/> property.
        /// </summary>
        public override void SetValue(string value)
        {
            var values = Items.Select(i => i.Value);
            if (values.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                SelectedValue = value;
            }
        }


        /// <summary>
        /// Returns collection of items populated in component.
        /// </summary>
        protected virtual IEnumerable<SelectListItem> GetItems()
        {
            return SelectorDataSourceHelper.ParseDataSource(Properties.DataSource);
        }


        /// <summary>
        /// Performs validation on <see cref="SelectedValue"/> property checking whether such value is present in <see cref="Items"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>Collection holding validation results.</returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = base.Validate(validationContext).ToList();

            var values = Items.Select(i => i.Value);
            if (!string.IsNullOrEmpty(SelectedValue) && !values.Contains(SelectedValue, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(new ValidationResult(ResHelper.GetString("kentico.formbuilder.component.selector.validation.selectedvaluenotpresent")));
            }

            return result;
        }
    }
}
