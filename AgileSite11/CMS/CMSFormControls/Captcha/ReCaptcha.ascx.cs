using System;

using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;


public partial class CMSFormControls_Captcha_ReCaptcha : FormEngineUserControl
{
    #region "Variables"

    /// <summary>
    ///  reCAPTCHA primary API key
    /// </summary>
    private string mPublicKey;

    /// reCAPTCHA secondary API key
    private string mPrivateKey;

    #endregion


    #region "Properties"

    /// <summary>
    /// reCAPTCHA public API key
    /// </summary>
    public string PublicKey
    {
        get
        {
            if (string.IsNullOrEmpty(mPublicKey))
            {
                mPublicKey = SettingsKeyInfoProvider.GetValue("CMSReCaptchaPublicKey", SiteContext.CurrentSiteID);
            }

            return mPublicKey;
        }
        set
        {
            mPublicKey = value;
        }
    }


    /// <summary>
    /// reCAPTCHA private API key
    /// </summary>
    public string PrivateKey
    {
        get
        {
            if (string.IsNullOrEmpty(mPrivateKey))
            {
                mPrivateKey = SettingsKeyInfoProvider.GetValue("CMSReCaptchaPrivateKey", SiteContext.CurrentSiteID);
            }

            return mPrivateKey;
        }
        set
        {
            mPrivateKey = value;
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

            // Disable CAPTCHA control
            captcha.Enabled = value;
            captcha.Visible = value;

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
    /// Indicates if validation of form control was successful
    /// </summary>
    public override bool IsValid()
    {
        captcha.Validate();

        if (!String.IsNullOrEmpty(ErrorMessage))
        {
            ValidationError = ErrorMessage;
        }

        return captcha.IsValid;
    }


    /// <summary>
    /// Error message displayed when validation fails
    /// </summary>
    public override string ErrorMessage
    {
        get
        {
            return captcha.ErrorMessage;
        }
        set
        {
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

    #endregion


    #region "Control methods"

    /// <summary>
    /// Page load event
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        CheckFieldEmptiness = false;

        string culture = GetCurrentCulture();

        captcha.PublicKey = PublicKey;
        captcha.PrivateKey = PrivateKey;
        captcha.Language = culture;
        captcha.Theme = Theme;
        captcha.Type = Type;
        captcha.Size = Size;
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