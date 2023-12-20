using System;

using System.Text;
using System.Web;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Globalization;


namespace CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl
{
    public partial class WTE_PasswordStrength : FormEngineUserControl
    {
        #region "Variables"

        private string mSiteName;
        private int mPreferedLength = 12;
        private int mPreferedNonAlphaNumChars = 2;
        private string mClassPrefix = "password-strength-";
        private string mValidationGroup = string.Empty;
        private string mTextBoxClass = string.Empty;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Returns current site name.
        /// </summary>
        private string SiteName
        {
            get
            {
                if (mSiteName == null)
                {
                    mSiteName = SiteContext.CurrentSiteName;
                }

                return mSiteName;
            }
        }


        /// <summary>
        /// Returns whether password policy is used.
        /// </summary>
        private bool UsePasswordPolicy
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSUsePasswordPolicy");
            }
        }


        /// <summary>
        /// Returns password minimal length.
        /// </summary>
        private int MinLength
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteName + ".CMSPolicyMinimalLength");
            }
        }


        /// <summary>
        /// Returns number of non alpha numeric characters
        /// </summary>
        private int MinNonAlphaNumChars
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(SiteName + ".CMSPolicyNumberOfNonAlphaNumChars");
            }
        }


        /// <summary>
        /// Returns password regular expression.
        /// </summary>
        private string RegularExpression
        {
            get
            {
                return SettingsKeyInfoProvider.GetValue(SiteName + ".CMSPolicyRegularExpression");
            }
        }

        #endregion


        #region "Public properties"


        protected string SessionKeyCurrentPasswordText
        {
            get
            {
                string ret = InputClientID + "_WTEPasswordString";
                return ret;
            }
        }

        public string SessionPasswordText
        {
            get
            {
                string ret = GetSession(SessionKeyCurrentPasswordText);
                return ret;
            }
            set
            {
                SetSession(SessionKeyCurrentPasswordText, value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether inline styles should be used for strength indicator
        /// </summary>
        public bool UseStylesForStrengthIndicator
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value of from control.
        /// </summary>
        public override object Value
        {
            get
            {
                //return txtPassword.Text;
                object ret = null;
                if (!String.IsNullOrWhiteSpace(SessionPasswordText))
                {
                    ret = SessionPasswordText;
                    txtPassword.Text = SessionPasswordText;
                }
                else
                {
                    ret = txtPassword.Text;
                }
                return ret;
            }
            set
            {
                //txtPassword.Text = ValidationHelper.GetString(value, string.Empty);
                if (String.IsNullOrWhiteSpace(SessionPasswordText))
                {
                    SessionPasswordText = ValidationHelper.GetString(value, string.Empty);
                }
                if (!String.IsNullOrWhiteSpace(SessionPasswordText))
                {
                    txtPassword.Text = SessionPasswordText;
                    txtPassword.Attributes["value"] = SessionPasswordText;
                }
            }
        }


        /// <summary>
        /// Returns textbox client ID.
        /// </summary>
        public override string ValueElementID
        {
            get
            {
                return txtPassword.ClientID;
            }
        }


        /// <summary>
        /// Gets or sets preferred length.
        /// </summary>
        public int PreferedLength
        {
            get
            {
                return mPreferedLength;
            }
            set
            {
                mPreferedLength = value;
            }
        }


        /// <summary>
        /// Gets or sets preferred number of non alpha numeric characters.
        /// </summary>
        public int PreferedNonAlphaNumChars
        {
            get
            {
                return mPreferedNonAlphaNumChars;
            }
            set
            {
                mPreferedNonAlphaNumChars = value;
            }
        }


        /// <summary>
        /// Class prefix for labels.
        /// </summary>
        public string ClassPrefix
        {
            get
            {
                return mClassPrefix;
            }
            set
            {
                mClassPrefix = value;
            }
        }


        /// <summary>
        /// Gets or sets value of from control in string type.
        /// </summary>
        public override string Text
        {
            get
            {
                return txtPassword.Text;
            }
            set
            {
                txtPassword.Text = value;
            }
        }


        /// <summary>
        /// Gets or sets whether password could be empty.
        /// </summary>
        public bool AllowEmpty
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether validation control is shown under the control.
        /// </summary>
        public bool ShowValidationOnNewLine
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets validation group.
        /// </summary>
        public string ValidationGroup
        {
            get
            {
                return mValidationGroup;
            }
            set
            {
                mValidationGroup = value;
            }
        }


        /// <summary>
        /// Gets or sets class of textbox.
        /// </summary>
        public string TextBoxClass
        {
            get
            {
                return mTextBoxClass;
            }
            set
            {
                mTextBoxClass = value;
            }
        }


        /// <summary>
        /// Returns textbox attributes.
        /// </summary>
        public AttributeCollection TextBoxAttributes
        {
            get
            {
                return txtPassword.Attributes;
            }
        }


        /// <summary>
        /// Gets or sets maximal length of password. 
        /// </summary>
        public int MaxLength
        {
            get
            {
                return txtPassword.MaxLength;
            }
            set
            {
                txtPassword.MaxLength = value;
            }
        }


        /// <summary>
        /// Gets or sets HTML that is displayed next to password input and indicates password as required field.
        /// </summary>
        public string RequiredFieldMark
        {
            get
            {
                return lblRequiredFieldMark.Text;
            }
            set
            {
                lblRequiredFieldMark.Text = value;
            }
        }


        /// <summary>
        /// Gets or sets whether strength indicator is shown.
        /// </summary>
        public bool ShowStrengthIndicator
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowStrengthIndicator"), true);
            }
            set
            {
                SetValue("ShowStrengthIndicator", value);
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

        /// <summary>
        /// Gets or sets placeholder attribute of textbox. Inserted resource string will be localized. Default value is empty.
        /// </summary>
        public string PlaceholderText
        {
            get;
            set;
        }

        #endregion

        #region "Event Handlers"

        public event EventHandler ShowPasswordClicked;


        protected void RaiseShowPasswordClicked(object sender, EventArgs e)
        {
            if (ShowPasswordClicked != null)
            {
                ShowPasswordClicked(sender, e);
            }
        }

        #endregion

        #region "Page events"

        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPasswordText = txtPassword.Text;

            // Add CSS text box class
            txtPassword.AddCssClass(TextBoxClass);

            if (!String.IsNullOrEmpty(PlaceholderText))
            {
                txtPassword.Attributes.Add("placeholder", GetString(PlaceholderText));
            }

            if (this.IsPostBack)
            {
                // try to save the password text
                txtPassword.Text = currentPasswordText;
            }

            if (ShowStrengthIndicator)
            {
                string tooltipMessage;

                StringBuilder sb = new StringBuilder();
                if (UsePasswordPolicy)
                {
                    string notacceptableString = GetString("passwordstrength.notacceptable");
                    if (!String.IsNullOrWhiteSpace(PasswordHintText))
                    {
                        notacceptableString += " : " + PasswordHintText;
                    }
                    sb.Append(notacceptableString, ";", GetString("passwordstrength.weak"));
                    //sb.Append(GetString("passwordstrength.notacceptable"), ";", GetString("passwordstrength.weak"));
                    tooltipMessage = string.Format(GetString("passwordstrength.hint"), MinLength, MinNonAlphaNumChars, PreferedLength, PreferedNonAlphaNumChars);
                }
                else
                {
                    sb.Append(GetString("passwordstrength.weak"), ";", GetString("passwordstrength.weak"));
                    tooltipMessage = string.Format(GetString("passwordstrength.recommend"), PreferedLength, PreferedNonAlphaNumChars);
                }

                if (!String.IsNullOrWhiteSpace(PasswordHintText))
                {
                    tooltipMessage = PasswordHintText;
                }

                // Register jQuery and registration of script which shows password strength        
                ScriptHelper.RegisterJQuery(Page);
                ScriptHelper.RegisterScriptFile(Page, "~/CMSScripts/membership.js");

                sb.Append(";", GetString("passwordstrength.acceptable"), ";", GetString("passwordstrength.average"), ";", GetString("passwordstrength.strong"), ";", GetString("passwordstrength.excellent"));

                
                string regex = "''";
                if (!string.IsNullOrEmpty(RegularExpression))
                {
                    regex = "/" + RegularExpression + "/";
                }

                // Javascript for calling js function on keyup of textbox
                string txtVar = "txtSearch_" + txtPassword.ClientID;
                string script =
                    txtVar + " = $cmsj('#" + txtPassword.ClientID + @"');
        if (" + txtVar + @" ) {                    
           " + txtVar + @".keyup(function(event){
                ShowStrength('" + txtPassword.ClientID + "', '" + MinLength + "', '" + PreferedLength + "', '" + MinNonAlphaNumChars + "', '"
                    + PreferedNonAlphaNumChars + "', " + regex + ", '" + lblEvaluation.ClientID + "', '" + sb + "', '" + ClassPrefix + "', '" + UsePasswordPolicy + "', '" + pnlPasswIndicator.ClientID + "', '" + UseStylesForStrengthIndicator + @"');                               
            });                   
        }";

                ScriptHelper.RegisterStartupScript(this, typeof(string), "PasswordStrength_" + txtPassword.ClientID, ScriptHelper.GetScript(script));

                if (UseStylesForStrengthIndicator)
                {
                    pnlPasswStrengthIndicator.Style.Add("height", "5px");
                    pnlPasswStrengthIndicator.Style.Add("background-color", "#dddddd");

                    pnlPasswIndicator.Style.Add("height", "5px");
                }

                ScriptHelper.RegisterTooltip(Page);
                ScriptHelper.AppendTooltip(lblPasswStregth, tooltipMessage, "help");
            }
            else
            {
                pnlPasswStrengthIndicator.Visible = false;
                lblEvaluation.Visible = false;
                lblPasswStregth.Visible = false;
            }

            // Set up required field validator
            if (AllowEmpty)
            {
                rfvPassword.Enabled = false;
            }
            else
            {
                rfvPassword.Text = GetString("general.requirespassword");
                rfvPassword.ValidationGroup = ValidationGroup;
                if (ShowValidationOnNewLine)
                {
                    rfvPassword.Text += "<br />";
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            lblRequiredFieldMark.Visible = !String.IsNullOrEmpty(lblRequiredFieldMark.Text);
        }

        #endregion

        #region "General Event"

        /// <summary>
        /// On show password clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbtnShowPassword_Clicked(Object sender, EventArgs e)
        {
            SessionPasswordText = txtPassword.Text;
            if (txtPassword.TextMode == System.Web.UI.WebControls.TextBoxMode.Password)
            {
                //txtPassword.Text = currentText; // set back the text if possible.
                txtPassword.TextMode = System.Web.UI.WebControls.TextBoxMode.SingleLine;
                //lbtnShowPassword.CssClass = "fa-solid fa-eye-slash";
                lbtnShowPassword.Text = "<i class=\"fa-solid fa-eye-slash\"></i>";
            }
            else
            {
                //txtPassword.Text = currentText; // set back the text if possible.
                txtPassword.TextMode = System.Web.UI.WebControls.TextBoxMode.Password;
                //lbtnShowPassword.CssClass = "fa-solid fa-eye";
                lbtnShowPassword.Text = "<i class=\"fa-solid fa-eye\"></i>";
            }
            RaiseShowPasswordClicked(sender, e);
        }

        /// <summary>
        /// On show password clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void txtPassword_TextChanged(Object sender, EventArgs e)
        {
            SessionPasswordText = txtPassword.Text;
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Returns whether 
        /// </summary>
        public override bool IsValid()
        {
            ValidationError = AuthenticationHelper.GetPolicyViolationMessage(SiteContext.CurrentSiteName);
            return SecurityHelper.CheckPasswordPolicy(txtPassword.Text, SiteName);
        }


        /// <summary>
        /// Returns textual information about the current password policy along with recommended safe password properties.
        /// </summary>
        public string GetPasswordPolicyHint()
        {
            string message = string.Empty;

            if (UsePasswordPolicy)
            {
                // Both minimum length and number of special characters are set
                if ((MinLength > 0) && (MinNonAlphaNumChars > 0))
                {
                    message = string.Format(GetString("passwordstrength.hint"), MinLength, MinNonAlphaNumChars);
                }
                // Only minimum length is set
                else if (MinLength > 0)
                {
                    message = string.Format(GetString("passwordstrength.hintlength"), MinLength);
                }
                // Only number of special characters is set
                else if (MinNonAlphaNumChars > 0)
                {
                    message = string.Format(GetString("passwordstrength.hintnonalphanum"), MinNonAlphaNumChars);
                }

                // Append recommended password parameters info
                if (message != string.Empty)
                {
                    message += " ";
                }
                message += string.Format(GetString("passwordstrength.recommend"), PreferedLength, PreferedNonAlphaNumChars);
            }

            return message;
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