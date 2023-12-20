using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.SiteProvider;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(RecaptchaComponent.IDENTIFIER, typeof(RecaptchaComponent), "{$kentico.formbuilder.component.recaptcha.name$}", Description = "{$kentico.formbuilder.component.recaptcha.description$}", IconClass = "icon-recaptcha", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a reCAPTCHA form component.
    /// </summary>
    public class RecaptchaComponent : FormComponent<RecaptchaProperties, string>
    {
        /// <summary>
        /// Represents the <see cref="RecaptchaComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.Recaptcha";


        private string mPublicKey;
        private string mPrivateKey;
        private string mLanguage;
        private bool? mSkipRecaptcha;


        /// <summary>
        /// Holds nothing and is here just because it is required.
        /// </summary>
        public string Value
        {
            get;
            set;
        }


        /// <summary>
        /// reCAPTCHA site key from https://www.google.com/recaptcha/admin.
        /// </summary>
        public string PublicKey
        {
            get
            {
                if (String.IsNullOrEmpty(mPublicKey))
                {
                    mPublicKey = SettingsKeyInfoProvider.GetValue("CMSRecaptchaPublicKey", SiteContext.CurrentSiteID);
                }

                return mPublicKey;
            }
            set
            {
                mPublicKey = value;
            }
        }


        /// <summary>
        /// reCAPTCHA private key from https://www.google.com/recaptcha/admin.
        /// </summary>
        public string PrivateKey
        {
            get
            {
                if (String.IsNullOrEmpty(mPrivateKey))
                {
                    mPrivateKey = SettingsKeyInfoProvider.GetValue("CMSRecaptchaPrivateKey", SiteContext.CurrentSiteID);
                }

                return mPrivateKey;
            }
            set
            {
                mPrivateKey = value;
            }
        }


        /// <summary>
        /// Optional. Forces the reCAPTCHA to render in a specific language.
        /// Auto-detects the user's language if unspecified.
        /// Currently supported values are listed at https://developers.google.com/recaptcha/docs/language.
        /// </summary>
        public string Language
        {
            get
            {
                if (String.IsNullOrEmpty(mLanguage))
                {
                    mLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }

                return mLanguage;
            }
            set
            {
                mLanguage = value;
            }
        }


        /// <summary>
        /// Determines whether the component is configured and allowed to be displayed.
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return AreKeysConfigured && !SkipRecaptcha;
            }
        }


        /// <summary>
        /// Indicates whether to skip the reCAPTCHA validation.
        /// Useful for testing platform. Can be set using RecaptchaSkipValidation in AppSettings.
        /// </summary>
        private bool SkipRecaptcha
        {
            get
            {
                if (!mSkipRecaptcha.HasValue)
                {
                    mSkipRecaptcha = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["RecaptchaSkipValidation"], false);
                }

                return mSkipRecaptcha.Value;
            }
        }


        /// <summary>
        /// Indicates whether both required keys are configured in the Settings application.
        /// </summary>
        private bool AreKeysConfigured => !String.IsNullOrEmpty(PublicKey) && !String.IsNullOrEmpty(PrivateKey);


        /// <summary>
        /// Label "for" cannot be used for this component. 
        /// </summary>
        public override string LabelForPropertyName => null;


        /// <summary>
        /// Returns empty string since the <see cref="Value"/> does not hold anything.
        /// </summary>
        /// <returns>Returns the value of the form component.</returns>
        public override string GetValue()
        {
            return String.Empty;
        }


        /// <summary>
        /// Does nothing since the <see cref="Value"/> does not need to hold anything.
        /// </summary>
        /// <param name="value">Value to be set.</param>
        public override void SetValue(string value)
        {
            // the Value does not need to hold anything
        }


        /// <summary>
        /// Performs validation of the reCAPTCHA component.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {       
            var errors = new List<ValidationResult>();
            errors.AddRange(base.Validate(validationContext));

            var isRenderedInAdminUI = VirtualContext.IsInitialized;

            if (!IsConfigured || isRenderedInAdminUI)
            {
                return errors;
            }

            RecaptchaValidator validator = new RecaptchaValidator
            {
                PrivateKey = PrivateKey,
                RemoteIP = RequestContext.UserHostAddress,
                Response = CMSHttpContext.Current.Request.Form.Get("g-recaptcha-response")
            };

            var validationResult = validator.Validate();

            if (validationResult != null)
            {
                if (!String.IsNullOrEmpty(validationResult.ErrorMessage))
                {
                    errors.Add(new ValidationResult(validationResult.ErrorMessage));
                }
            }
            else
            {
                errors.Add(new ValidationResult(ResHelper.GetString("recaptcha.error.serverunavailable")));
            }

            return errors;
        }
    }
}