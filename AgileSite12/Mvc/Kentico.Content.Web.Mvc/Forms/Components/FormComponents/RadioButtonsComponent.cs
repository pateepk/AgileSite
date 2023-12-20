using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(RadioButtonsComponent.IDENTIFIER, typeof(RadioButtonsComponent), "{$kentico.formbuilder.component.radiobuttons.name$}", Description = "{$kentico.formbuilder.component.radiobuttons.description$}", IconClass = "icon-list-bullets", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents component displaying radio buttons selector.
    /// </summary>
    public class RadioButtonsComponent : SelectorFormComponent<RadioButtonsProperties>
    {
        /// <summary>
        /// Represents the <see cref="RadioButtonsComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.RadioButtons";


        /// <summary>
        /// Label "for" cannot be specified for radio buttons.
        /// </summary>
        public override string LabelForPropertyName => null;
    }
}
