using System;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using Microsoft.Ajax.Utilities;

namespace CMSApp.CMSWebParts.WTE.Custom.Modules
{
    public partial class WTE_UserNameControl : FormEngineUserControl
    {
        private bool mUseDefaultValidationGroup = true;

        /// <summary>
        /// The disabled CSS style
        /// </summary>
        public string DisabledStyle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DisabledStyle"), "background-color: dimgrey;color: linen;opacity: 0.5;filter: alpha(opacity = 50);");
            }
            set
            {
                SetValue("DisabledStyle", value);
            }
        }

        /// <summary>
        ///  Do we need to lock down the username field.
        /// </summary>
        public bool LockUserNameText
        {
            get
            {
                bool ret = false;

                try
                {
                    if (!String.IsNullOrWhiteSpace(Request.QueryString["un"]))
                    {

                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    // eat it.. something is null here, probably the request?
                }

                return ret;
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
                txtUserName.Enabled = value;
                if (!txtUserName.Enabled)
                {
                    // override the CSS to make it look disabled
                    txtUserName.Attributes["Style"] = "background-color: dimgrey;color: linen;opacity: 0.6;filter: alpha(opacity = 60);";
                }
                //if (LockUserNameText)
                //{
                //    txtUserName.Enabled = value;
                //}
                //else
                //{
                //    txtUserName.Enabled = false;
                //}
            }
        }

        /// <summary>
        /// Gets or sets field value.
        /// </summary>
        public override object Value
        {
            get
            {
                return txtUserName.Text;
            }
            set
            {
                bool disableUserName = false;
                string setval = ValidationHelper.GetString(value, "");
                if (String.IsNullOrWhiteSpace(setval))
                {
                    if (!String.IsNullOrWhiteSpace(Request.QueryString["un"]))
                    {
                        setval = Request.QueryString["un"];
                        disableUserName = true;
                    }
                }
                txtUserName.Text = setval;
                txtUserName.Enabled = !disableUserName;
            }
        }

        /// <summary>
        /// If true validator has default validation group set.
        /// </summary>
        public bool UseDefaultValidationGroup
        {
            get
            {
                return mUseDefaultValidationGroup;
            }
            set
            {
                mUseDefaultValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets inner textbox.
        /// </summary>
        public CMSTextBox TextBox
        {
            get
            {
                return txtUserName;
            }
        }

        /// <summary>
        /// Clears current value.
        /// </summary>
        public void Clear()
        {
            txtUserName.Text = "";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Apply CSS styles
            if (!String.IsNullOrEmpty(CssClass))
            {
                txtUserName.CssClass = CssClass;
                CssClass = null;
            }

            if (!String.IsNullOrEmpty(ControlStyle))
            {
                txtUserName.Attributes.Add("style", ControlStyle);
                ControlStyle = null;
            }

            txtUserName.Enabled = !LockUserNameText;

            if (!txtUserName.Enabled)
            {
                // override the CSS to make it look disabled
                txtUserName.Attributes["style"] = DisabledStyle;
            }

            CheckRegularExpression = true;
            CheckMinMaxLength = true;
        }

        /// <summary>
        /// Returns true if user control is valid.
        /// </summary>
        public override bool IsValid()
        {
            // Get appropriate validation message
            string userValidationString = ValidationHelper.UseSafeUserName ? "general.safeusernamevalidation" : "general.usernamevalidation";

            // For custom regular expression use general validation message
            if (ValidationHelper.CustomUsernameRegExpString != null)
            {
                userValidationString = "general.customusernamevalidation";
            }

            if ((FieldInfo == null) || !FieldInfo.AllowEmpty || !(txtUserName.Text.Length == 0))
            {
                string result = new Validator().NotEmpty(txtUserName.Text, GetString("Administration-User_New.RequiresUserName")).IsUserName(txtUserName.Text, GetString(userValidationString)).Result;

                if (!String.IsNullOrEmpty(result))
                {
                    ValidationError = result;
                    return false;
                }
            }
            return true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RequiredFieldValidatorUserName.ErrorMessage = GetString("Administration-User_New.RequiresUserName");
            txtUserName.Enabled = !LockUserNameText;
            if (UseDefaultValidationGroup)
            {
                RequiredFieldValidatorUserName.ValidationGroup = "ConfirmRegForm";
            }
        }
    }
}