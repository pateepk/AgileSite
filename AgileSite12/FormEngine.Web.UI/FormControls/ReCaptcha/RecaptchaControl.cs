using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// This class encapsulates reCAPTCHA UI and logic into a form engine user control.
    /// </summary>
    public class RecaptchaControl : FormEngineUserControl
    {
        #region "Private Fields"

        private const string RECAPTCHA_RESPONSE_FIELD = "g-recaptcha-response";
        private const string RECAPTCHA_JS_URL = "https://www.google.com/recaptcha/api.js";
        private const string RECAPTCHA_NOSCRIPT_URL = "https://www.google.com/recaptcha/api/fallback";

        private string mPublicKey;
        private string mPrivateKey;
        private string mLanguage;
        private bool? mSkipRecaptcha;
        private Panel mMainPanel;
        private bool KeysConfigured => !String.IsNullOrEmpty(PublicKey) && !String.IsNullOrEmpty(PrivateKey);

        #endregion


        #region "Public Properties"

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
                    mLanguage = GetCurrentCulture();
                }

                return mLanguage;
            }
            set
            {
                mLanguage = value;
            }
        }


        /// <summary>
        /// Optional. The color theme of the reCAPTCHA widget. Currently supported values are 'light' and 'dark'.
        /// </summary>
        public string Theme
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Theme"), "light");
            }
            set
            {
                SetValue("Theme", value);
            }
        }


        /// <summary>
        /// Optional. The type of CAPTCHA to serve. Currently supported values are 'image' and 'audio'.
        /// </summary>
        public string Type
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Type"), "image");
            }
            set
            {
                SetValue("Type", value);
            }
        }


        /// <summary>
        /// Optional. The size of the reCAPTCHA widget. Currently supported values are 'normal' and 'compact'.
        /// </summary>
        public string Size
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Size"), "normal");
            }
            set
            {
                SetValue("Size", value);
            }
        }


        /// <summary>
        /// Indicates whether to skip the reCAPTCHA validation.
        /// Useful for testing platform. Can be set using RecaptchaSkipValidation in AppSettings.
        /// </summary>
        public bool SkipRecaptcha
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
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;

                if (FieldInfo != null)
                {
                    FieldInfo.Visible = value;
                }
            }
        }


        /// <summary>
        /// Get or sets control value
        /// </summary>
        public override object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Indicates that field should be checked on emptiness in validation step by BasicForm. Default TRUE.
        /// It is preferable to set to FALSE for controls with complex input such as file uploaders. Field emptiness
        /// validation then must be placed in custom form control in IsValid() method.
        /// </summary>
        public override bool CheckFieldEmptiness => false;

        #endregion


        #region "Overridden Methods"

        /// <summary>
        /// OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            mMainPanel = new Panel { ID = "pnlRecaptchaControl" };
            Controls.Add(mMainPanel);

            base.OnInit(e);
        }


        /// <summary>
        /// OnPreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible && Enabled && !StopProcessing)
            {
                if (SkipRecaptcha)
                {
                    RegisterRecaptchaSkippedMessage();
                }
                else if (!KeysConfigured)
                {
                    RegisterInvalidConfigurationMessage();
                }
                else
                {
                    RegisterRecaptchaContainer();
                    RegisterRecaptchaScripts();
                    RegisterNoscriptHTML();
                }
            }
        }


        /// <summary>
        /// Indicates if validation of form control was successful
        /// </summary>
        public override bool IsValid()
        {
            var validationResult = Validate();

            if (!String.IsNullOrEmpty(ErrorMessage))
            {
                ValidationError = ErrorMessage;
            }

            return validationResult;
        }

        #endregion


        #region "Helper methods for rendering"

        private void RegisterRecaptchaSkippedMessage()
        {
            mMainPanel.Controls.Add(new LiteralControl
            {
                Text = "reCAPTCHA validation is skipped. Set the RecaptchaSkipValidation AppSettings value to false to enable validation.",
                EnableViewState = false
            });
        }


        private void RegisterInvalidConfigurationMessage()
        {
            mMainPanel.Controls.Add(new LiteralControl
            {
                Text = ResHelper.GetString("recaptcha.error.invalidconfiguration"),
                EnableViewState = false
            });
        }


        /// <summary>
        /// Adds a container where the reCAPTCHA will be explicitly rendered with javascript
        /// </summary>
        private void RegisterRecaptchaContainer()
        {

            var captchaPanelWrap = new Panel
            {
                ID = "pnlCaptchaWrap",
                CssClass = "cms-recaptcha-wrap"
            };
            captchaPanelWrap.Attributes.Add("data-rendersettings", GetRenderSettingsJson());
            mMainPanel.Controls.Add(captchaPanelWrap);
        }


        /// <summary>
        /// Gets render settings for explicit javascript rendering.
        /// </summary>
        private string GetRenderSettingsJson()
        {
            var renderSettings = new
            {
                sitekey = PublicKey,
                theme = Theme,
                type = Type,
                size = Size
            };

            return ScriptHelper.SerializeModuleParametersToJson(renderSettings);
        }


        /// <summary>
        /// Registers client scripts to render all reCAPTCHA controls within a page.
        /// </summary>
        private void RegisterRecaptchaScripts()
        {
            ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "RecaptchaScript", $"<script src=\"{GenerateScriptUrl()}\" async=\"async\" defer=\"defer\"></script>");
            ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "RecaptchaExplicitRender", GetExplicitRenderCode(), true);

            if (ControlsHelper.IsInAsyncPostback(Page))
            {
                ScriptHelper.RegisterStartupScript(Page, typeof(string), "RecaptchaReload", "RenderRecaptchas();", true);
            }
        }


        /// <summary>
        /// Generates reCAPTCHA js url with query parameters.
        /// </summary>
        private string GenerateScriptUrl()
        {
            var url = RECAPTCHA_JS_URL;

            url = URLHelper.AddParameterToUrl(url, "onload", "RenderRecaptchas");
            url = URLHelper.AddParameterToUrl(url, "render", "explicit");

            if (Language != null)
            {
                url = URLHelper.AddParameterToUrl(url, "hl", HTMLHelper.HTMLEncode(Language));
            }

            return url;
        }


        /// <summary>
        /// Gets a client script which renders the reCAPTCHA controls.
        /// </summary>
        private string GetExplicitRenderCode()
        {
            return @"var RenderRecaptchas = function() {
    var captchas = document.getElementsByClassName('cms-recaptcha-wrap');
    if (captchas.length > 0 && grecaptcha) {
        Array.from(captchas).forEach(function(item) {
            var renderSettings = item.getAttribute('data-rendersettings');
            if (item.id && renderSettings && item.children.length == 0) {
                grecaptcha.render(item.id, JSON.parse(renderSettings));
            };
        });
    };
};";
        }


        /// <summary>
        /// Adds a reCAPTCHA noscript HTML code for visitors with disabled javascript.
        /// </summary>
        private void RegisterNoscriptHTML()
        {
            mMainPanel.Controls.Add(new LiteralControl
            {
                Text = GenerateNoscriptHtml(),
                EnableViewState = false
            });
        }


        /// <summary>
        /// Generates reCAPTCHA noscript HTML code for visitors with disabled javascript.
        /// </summary>
        private string GenerateNoscriptHtml()
        {
            return $@"<noscript>
  <div>
    <div style=""width: 302px; height: 422px; position: relative;"">
      <div style=""width: 302px; height: 422px; position: absolute;"">
        <iframe src=""{GenerateInlineFrameUrl()}""
                frameborder=""0"" scrolling=""no""
                style=""width: 302px; height:422px; border-style: none;"">
        </iframe>
      </div>
    </div>
    <div style=""width: 300px; height: 60px; border-style: none;
                   bottom: 12px; left: 25px; margin: 0px; padding: 0px; right: 25px;
                   background: #f9f9f9; border: 1px solid #c1c1c1; border-radius: 3px;"">
      <textarea id=""g-recaptcha-response"" name=""g-recaptcha-response""
                   class=""g-recaptcha-response""
                   style=""width: 250px; height: 40px; border: 1px solid #c1c1c1;
                          margin: 10px 25px; padding: 0px; resize: none;"" >
      </textarea>
    </div>
  </div>
</noscript>";
        }


        /// <summary>
        /// Generates the reCAPTCHA iframe URL for visitors with disabled javascript.
        /// </summary>
        private string GenerateInlineFrameUrl()
        {
            var url = RECAPTCHA_NOSCRIPT_URL;

            url = URLHelper.AddParameterToUrl(url, "k", HTMLHelper.HTMLEncode(PublicKey));

            if (Language != null)
            {
                url = URLHelper.AddParameterToUrl(url, "hl", HTMLHelper.HTMLEncode(Language));
            }

            return url;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Performs validation of reCAPTCHA.
        /// </summary>
        private bool Validate()
        {
            if (!Enabled || SkipRecaptcha || Context?.Request == null || !KeysConfigured)
            {
                return true;
            }

            // Validate reCAPTCHA
            RecaptchaValidator validator = new RecaptchaValidator
            {
                PrivateKey = PrivateKey,
                RemoteIP = RequestContext.UserHostAddress,
                Response = GetCoupledRequestFormValue(RECAPTCHA_RESPONSE_FIELD)
            };

            var validationResult = validator.Validate();

            if (validationResult != null)
            {
                ErrorMessage = validationResult.ErrorMessage;
                return validationResult.IsValid;
            }

            ErrorMessage = ResHelper.GetString("recaptcha.error.serverunavailable");
            return false;
        }


        /// <summary>
        /// Gets form field value
        /// </summary>
        /// <param name="formField">Field name to get its value from</param>
        private string GetCoupledRequestFormValue(string formField)
        {
            // Get form value
            string fieldValue = Context.Request.Form[formField];
            if (!String.IsNullOrEmpty(fieldValue))
            {
                string[] fieldValuesArray = fieldValue.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                fieldValue = fieldValuesArray.FirstOrDefault() ?? String.Empty;
            }

            return fieldValue;
        }


        /// <summary>
        /// Gets current short culture code.
        /// </summary>
        private string GetCurrentCulture()
        {
            var culture = IsLiveSite ? LocalizationContext.CurrentCulture : LocalizationContext.CurrentUICulture;

            return CultureHelper.GetShortCultureCode(culture.CultureCode).ToLowerInvariant();
        }

        #endregion
    }
}