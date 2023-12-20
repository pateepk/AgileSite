using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(HiddenGuidInputComponent.IDENTIFIER, typeof(HiddenGuidInputComponent), "Hidden GUID input component", IsAvailableInFormBuilderEditor = false, ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a hidden input form component.
    /// </summary>
    public class HiddenGuidInputComponent : FormComponent<HiddenGuidInputProperties, Guid>
    {
        /// <summary>
        /// Represents the <see cref="HiddenGuidInputComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.HiddenGuidInput";


        /// <summary>
        /// Gets or set value of the <see cref="HiddenGuidInputComponent"/>.
        /// </summary>
        [BindableProperty]
        public Guid Value { get; set; }


        /// <summary>
        /// Returns an empty string.
        /// </summary>
        public override string LabelForPropertyName => String.Empty;


        /// <summary>
        /// Returns the <see cref="Value"/>.
        /// </summary>
        public override Guid GetValue()
        {
            return Value;
        }


        /// <summary>
        /// Sets the <see cref="Value"/>.
        /// </summary>
        public override void SetValue(Guid value)
        {
            Value = value;
        }
    }
}
