using System;

using System.Text;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl
{
    public partial class WTE_PasswordConfirmator : FormEngineUserControl
    {
        #region "Properties"

        #region form control properties

        public bool ShowStrengthIndicator
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("showstrength"), true);
            }
            set
            {
                SetValue("showstrength", value);
            }
        }

        public string PasswordHintText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PasswordHintText"), String.Empty);
            }
            set
            {
                SetValue("PasswordHintText", value);
            }
        }


        #endregion

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
                txtConfirmPassword.Enabled = value;
                passStrength.Enabled = value;
            }
        }


        /// <summary>
        /// Returns inserted password.
        /// </summary>
        public override object Value
        {
            get
            {
                // Check if text is set
                if (string.IsNullOrEmpty(passStrength.Text))
                {
                    return string.Empty;
                }

                return passStrength.Text;
            }
            set
            {
                passStrength.Text = ValidationHelper.GetString(value, string.Empty);
                //if (String.IsNullOrWhiteSpace(passStrength.Text))
                //{ 
                //    passStrength.SessionPasswordText = ValidationHelper.GetString(value, string.Empty);
                //    passStrength.Text = passStrength.SessionPasswordText;
                //}
            }
        }


        /// <summary>
        /// Client ID of primary input control.
        /// </summary>
        public override string InputClientID
        {
            get
            {
                return passStrength.ValueElementID;
            }
        }

        #endregion


        #region "Page events"

        protected void Page_Load(object sender, EventArgs e)
        {
            passStrength.ShowStrengthIndicator = ShowStrengthIndicator;
            passStrength.PasswordHintText = PasswordHintText;
            //passStrength.ShowPasswordClicked += txtPassword_ShowPasswordClicked;
            //if (IsPostBack)
            //{
            //    Value = passStrength.SessionPasswordText;
            //}
        }

        #endregion

        #region "General Events"

        /// <summary>
        /// Txt Password show/hide password was clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void txtPassword_ShowPasswordClicked(Object sender, EventArgs e)
        {
            string currentPassword = passStrength.SessionPasswordText;
            Value = currentPassword;
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Returns true if user control is valid.
        /// </summary>
        public override bool IsValid()
        {
            // Check if passwords match
            if (passStrength.Text != txtConfirmPassword.Text)
            {
                ValidationError = GetString("PassConfirmator.PasswordDoNotMatch");
            }

            // Check regular expression
            string regularExpression = FieldInfo.RegularExpression;
            if ((!String.IsNullOrEmpty(regularExpression)) && (new Validator().IsRegularExp(passStrength.Text, regularExpression, "error").Result.EqualsCSafe("error", true)))
            {
                ValidationError = GetString("PassConfirmator.InvalidPassword");
            }

            // Check min length
            int minLength = ValidationHelper.GetInteger(FieldInfo.MinValue, -1);
            if ((minLength > 0) && (passStrength.Text.Length < minLength))
            {
                ValidationError = string.Format(GetString("PassConfirmator.PasswordLength"), minLength);
            }

            // Check password policy
            if (!passStrength.IsValid())
            {
                ValidationError = AuthenticationHelper.GetPolicyViolationMessage(SiteContext.CurrentSiteName);
            }

            if (!String.IsNullOrEmpty(ValidationError))
            {
                Value = string.Empty;
                return false;
            }

            return true;
        }

        #endregion

        #region "session helper"

        /// <summary>
        /// Get Session data
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetSession(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        ret = (string)HttpContext.Current.Session[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void SetSession(string p_key, object p_value)
        {
            try
            {
                if (p_value == null)
                {
                    ClearSession(p_key);
                }
                else
                {
                    if (HttpContext.Current != null)
                    {
                        if (HttpContext.Current.Session != null)
                        {
                            HttpContext.Current.Session[p_key] = p_value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void ClearSession(string p_key)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session.Remove(p_key);
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #endregion "session helper"
    }
}