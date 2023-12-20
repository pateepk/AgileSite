using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(USPhoneComponent.IDENTIFIER, typeof(USPhoneComponent), "{$kentico.formbuilder.component.usphone.name$}", Description = "{$kentico.formbuilder.component.usphone.description$}", IconClass = "icon-smartphone", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents an US phone number input component.
    /// </summary>
    public class USPhoneComponent : FormComponent<USPhoneProperties, string>
    {
        /// <summary>
        /// Represents the <see cref="USPhoneComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.USPhone";


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public string PhoneNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the <see cref="PhoneNumber"/>.
        /// </summary>
        public override string GetValue()
        {
            return PhoneNumber;
        }


        /// <summary>
        /// Sets the <see cref="PhoneNumber"/>.
        /// </summary>
        public override void SetValue(string value)
        {
            PhoneNumber = value;
        }


        /// <summary>
        /// Determines whether given input is a valid US phone numbers.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            errors.AddRange(base.Validate(validationContext));

            var value = GetValue();

            if (!String.IsNullOrWhiteSpace(value) && !ValidationHelper.IsUsPhoneNumber(value))
            {
                errors.Add(new ValidationResult(ResHelper.GetString("USPhone.ValidationError"), new [] { nameof(PhoneNumber) }));
            }

            return errors;
        }
    }
}
