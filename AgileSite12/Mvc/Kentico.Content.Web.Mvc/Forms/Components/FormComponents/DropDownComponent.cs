using System;
using System.Linq;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(DropDownComponent.IDENTIFIER, typeof(DropDownComponent), "{$kentico.formbuilder.component.dropdown.name$}", Description = "{$kentico.formbuilder.component.dropdown.description$}", IconClass = "icon-menu", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents component displaying drown down list.
    /// </summary>
    public class DropDownComponent : SelectorFormComponent<DropDownProperties>
    {
        /// <summary>
        /// Represents the <see cref="DropDownComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.DropDown";


        /// <summary>
        /// Gets the <see cref="SelectorFormComponent{TProperties}.SelectedValue"/> property.
        /// </summary>
        public override string GetValue()
        {
            return SelectedValue ?? (String.IsNullOrEmpty(Properties.OptionLabel) ? GetItems().FirstOrDefault()?.Value : null);
        }
    }
}
