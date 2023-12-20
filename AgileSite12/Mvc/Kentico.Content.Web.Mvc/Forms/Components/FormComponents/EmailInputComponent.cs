using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(EmailInputComponent.IDENTIFIER, typeof(EmailInputComponent), "{$kentico.formbuilder.component.emailinput.name$}", Description = "{$kentico.formbuilder.component.emailinput.description$}", IconClass = "icon-message", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents an email input component.
    /// </summary>
    public class EmailInputComponent : FormComponent<EmailInputProperties, string>
    {
        /// <summary>
        /// Represents the <see cref="EmailInputComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.EmailInput";


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public string Email { get; set; }

               
        /// <summary>
        /// Gets the <see cref="Email"/> property.
        /// </summary>
        public override string GetValue()
        {
            return Email;
        }

        
        /// <summary>
        /// Sets the <see cref="Email"/>.
        /// </summary>
        public override void SetValue(string value)
        {
            Email = value;
        }
        

        /// <summary>
        /// Determines whether given input is a valid email address.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            errors.AddRange(base.Validate(validationContext));
            
            var value = GetValue();

            if (!String.IsNullOrWhiteSpace(value) && !ValidationHelper.AreEmails(value))
            {
                errors.Add(new ValidationResult(ResHelper.GetString("EmailInput.ValidationError"), new [] { nameof(Email) }));
            }

            return errors;
        }
    }
}
