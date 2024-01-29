using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// This class encapsulates reCAPTCHA UI and logic into an ASP.NET server control.
    /// </summary>
    [ToolboxData("<{0}:RecaptchaControl runat=\"server\" />")]
    [Designer(typeof(RecaptchaControlDesigner))]
    public class RecaptchaControl : WebControl, IValidator, INamingContainer
    {
        #region "Private Fields"

        private const string RECAPTCHA_RESPONSE_FIELD = "g-recaptcha-response";
        private const string RECAPTCHA_JS_URL = "https://www.google.com/recaptcha/api.js";
        private const string RECAPTCHA_NOSCRIPT_URL = "https://www.google.com/recaptcha/api/fallback";

        private bool? mSkipRecaptcha;
        private Panel mMainPanel;
        private bool KeysConfigured => !String.IsNullOrEmpty(PublicKey) && !String.IsNullOrEmpty(PrivateKey);

        #endregion


        #region "Public Properties"

        /// <summary>
        /// reCAPTCHA public API key from https://www.google.com/recaptcha/admin. 
        /// </summary>
        [Category("Settings")]
        [Description("The public key from https://www.google.com/recaptcha/admin.")]
        public string PublicKey
        {
            get;
            set;
        }


        /// <summary>
        /// reCAPTCHA private API key from https://www.google.com/recaptcha/admin. 
        /// </summary>
        [Category("Settings")]
        [Description("The private key from https://www.google.com/recaptcha/admin.")]
        public string PrivateKey
        {
            get;
            set;
        }


        /// <summary>
        /// reCAPTCHA theme to be used.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("light")]
        [Description("The theme for the reCAPTCHA control. Currently supported values are 'light' and 'dark'.")]
        public string Theme
        {
            get;
            set;
        }


        /// <summary>
        /// Optional. The type of CAPTCHA to serve. Currently supported values are 'image' and 'audio'.
        /// </summary>
        public string Type
        {
            get;
            set;
        }


        /// <summary>
        /// Optional. The size of the reCAPTCHA widget. Currently supported values are 'normal' and 'compact'.
        /// </summary>
        public string Size
        {
            get;
            set;
        }


        /// <summary>
        /// Optional. Forces the reCAPTCHA to render in a specific language. 
        /// Auto-detects the user's language if unspecified. 
        /// Currently supported values are listed at https://developers.google.com/recaptcha/docs/language.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [Description("UI language for the reCAPTCHA control. Currently supported values are listed at https://developers.google.com/recaptcha/docs/language.")]
        public string Language
        {
            get;
            set;
        }


        /// <summary>
        /// reCAPTCHA custom translations.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [Obsolete("This property is not used and will be removed in the next version. Translate the reCAPTCHA error codes as resource strings with the following key: 'recaptcha.error.[error-code]'.")]
        public Dictionary<string, string> CustomTranslations
        {
            get;
            set;
        }


        /// <summary>
        /// reCAPTCHA custom theme widget.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [Description("When using custom theming, this is a div element which contains the widget. ")]
        [Obsolete("This property is not used and will be removed in the next version without replacement.")]
        public string CustomThemeWidget
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether to skip the reCAPTCHA validation.
        /// Useful for testing platform. Can also be set using RecaptchaSkipValidation in AppSettings.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(false)]
        [Description("Set this to true to stop reCAPTCHA validation. Useful for testing platform. Can also be set using RecaptchaSkipValidation in AppSettings.")]
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
            set
            {
                mSkipRecaptcha = value;
            }
        }


        /// <summary>
        /// Allow multiple instances.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(false)]
        [Description("Set this to true to enable multiple reCAPTCHA on a single page. There may be complication between controls when this is enabled.")]
        [Obsolete("This property is not used and will be removed in the next version without replacement.")]
        public bool AllowMultipleInstances
        {
            get;
            set;
        }


        /// <summary>
        /// Override reCAPTCHA secure API.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(false)]
        [Description("Set this to true to override reCAPTCHA usage of Secure API.")]
        [Obsolete("This property is not used and will be removed. HTTPS is used by default.")]
        public bool OverrideSecureMode
        {
            get;
            set;
        }


        /// <summary>
        /// reCAPTCHA proxy used to validate.
        /// </summary>
        [Category("Settings")]
        [Description("Set this to override proxy used to validate reCAPTCHA.")]
        [Obsolete("This property is not used and will be removed in the next version without replacement.")]
        public IWebProxy Proxy
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether this control takes advantage of other control's reCaptcha or has its own.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This property is not used and will be removed in the next version without replacement.")]
        public bool IsClonnedInstance
        {
            get;
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Constructor, ensures the correct HTML tag.
        /// </summary>
        public RecaptchaControl() : base(HtmlTextWriterTag.Div)
        {
        }

        #endregion


        #region Overridden Methods

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

            if (Visible && Enabled)
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

        #endregion


        #region IValidator Members

        /// <summary>
        /// Validation error message.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if validation was successful.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsValid
        {
            get;
            set;
        }


        /// <summary>
        /// Perform validation of reCAPTCHA.
        /// </summary>
        public void Validate()
        {
            if (!Enabled || SkipRecaptcha || Context?.Request == null || !KeysConfigured)
            {
                IsValid = true;
                return;
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
                IsValid = validationResult.IsValid;
                return;
            }

            ErrorMessage = ResHelper.GetString("recaptcha.error.serverunavailable");
            IsValid = false;
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


        #region "Helper methods"

        /// <summary>
        /// Get form field value
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

        #endregion
    }
}