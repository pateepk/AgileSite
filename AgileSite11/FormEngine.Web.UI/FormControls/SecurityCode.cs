using System;

using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Base form control for CAPTCHA controls
    /// </summary>
    public class SecurityCode : FormEngineUserControl
    {
        private FormEngineUserControl mCaptchaControl;


        /// <summary>
        /// Gets or sets enabled state of CAPTCHA controls.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                CaptchaControl.Enabled = value;
                base.Enabled = value;
            }
        }


        /// <summary>
        /// Gets CAPTCHA control.
        /// </summary>
        protected FormEngineUserControl CaptchaControl
        {
            get
            {
                if (mCaptchaControl == null)
                {
                    var selectedCaptcha = CaptchaSetting(SiteContext.CurrentSiteName);

                    string captchaControlName = null;
                    switch (selectedCaptcha)
                    {
                        case CaptchaEnum.Logic:
                            captchaControlName = "LogicCaptcha";
                            break;

                        case CaptchaEnum.Text:
                            captchaControlName = "TextCaptcha";
                            break;

                        case CaptchaEnum.ReCaptcha:
                            captchaControlName = "ReCaptcha";
                            break;

                        case CaptchaEnum.Default:
                        default:
                            captchaControlName = "SimpleCaptcha";
                            break;
                    }

                    mCaptchaControl = FormUserControlLoader.LoadFormControl(Page, captchaControlName, "", loadDefaultProperties: false);
                    mCaptchaControl.Form = Form;
                    mCaptchaControl.ID = "captchaControl";

                    // Load the default properties of the form control
                    var ctrl = FormUserControlInfoProvider.GetFormUserControlInfo(captchaControlName);
                    var properties = FormHelper.GetFormControlParameters(captchaControlName, ctrl.UserControlParameters, false);
                    mCaptchaControl.LoadDefaultProperties(properties);
                }

                return mCaptchaControl;
            }
        }


        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public override object Value
        {
            get
            {
                return CaptchaControl.Value;
            }
            set
            {
                CaptchaControl.Value = value;
            }
        }


        /// <summary>
        /// Sets whether the CAPTCHA control should display info label.
        /// </summary>
        public bool ShowInfoLabel
        {
            set
            {
                CaptchaControl.SetValue("ShowInfoLabel", value);
            }
        }


        /// <summary>
        /// OnInit event
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            Controls.Add(CaptchaControl);
            base.OnInit(e);
        }


        /// <summary>
        /// Gets selected CAPTCHA from settings.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected CaptchaEnum CaptchaSetting(string siteName)
        {
            return(CaptchaEnum)SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSCaptchaControl");
        }


        /// <summary>
        /// Returns true if entered data is valid.
        /// </summary>
        public override bool IsValid()
        {
            bool isValid = CaptchaControl.IsValid();
            if (!isValid)
            {
                ValidationError = GetString("SecurityCode.ValidationError");
            }

            return isValid;
        }
    }
}