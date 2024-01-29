using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
//using CMS.PortalControls;

namespace CMSApp.CMSWebParts.CUFWebParts.FirstCitizens
{
    public partial class CUFAcceptDisclosure : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string AcceptedURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("AcceptedURL"), "~/Secure/Member/My-Profile/Delivery-Preferences?da=1");
            }
            set
            {
                this.SetValue("AcceptedURL", value);
            }
        }

        /// <summary>
        /// URL to the security question page
        /// </summary>
        public string SecurityQuestionAnswerURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("SecurityQuestionAnswerURL"), "/Secure/Member/My-Profile/Security-Questions");
            }
            set
            {
                this.SetValue("SecurityQuestionAnswerURL", value);
            }
        }

        /// <summary>
        /// Does the site has security question and answer enabled
        /// </summary>
        public bool SecurityQuestionEnabled
        {
            get
            {
                return GetSafeBoolValue("SecurityQuestionEnabled", false);
            }
            set
            {
                this.SetValue("SecurityQuestionEnabled", value);
            }
        }

        #endregion "Properties"

        #region page load event

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                cvAcceptTerms.Attributes["cbClientID"] = cbAcceptTerms.ClientID;

                if (PortalContext.ViewMode.IsLiveSite() || PortalContext.ViewMode.IsPreview())
                {
                    DateTime acceptanceDate = CurrentUser.UserSettings.GetDateTimeValue("CUDisclosureDate", DateTime.Now.AddYears(1));
                    String email = CurrentUser.Email;

                    if (acceptanceDate > DateTime.Now)
                    {
                        rowEmail.Visible = true;
                        txtEmail.Text = email.Trim();
                    }
                    else
                    {
                        //already accepted
                        Response.Redirect(AcceptedURL);
                    }
                }
                else
                {
                    cvAcceptTerms.Enabled = false;
                    rfvTxtEmail.Enabled = false;
                    revTxtEmail.Enabled = false;
                }
            }
        }

        #endregion page load event

        #region general events

        /// <summary>
        /// Validate the check box
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void cvAcceptTerms_ServerValidate(Object source, ServerValidateEventArgs args)
        {
            args.IsValid = (cbAcceptTerms.Checked == true);
        }

        /// <summary>
        /// Continue button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Continue_Click(Object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (PortalContext.ViewMode.IsLiveSite() || PortalContext.ViewMode.IsPreview())
                {
                    if (rowEmail.Visible == true)
                    {
                        if (CurrentUser.Email.Trim() != txtEmail.Text)
                        {
                            CurrentUser.UserSettings.SetValue("OldEmail", CurrentUser.Email.Trim());
                            CurrentUser.UserSettings.SetValue("NewEmailDate", DateTime.Now);
                        }
                        CurrentUser.Email = txtEmail.Text;
                    }

                    CurrentUser.UserSettings.SetValue("CUDisclosureDate", DateTime.Now);

                    //commit updates
                    UserInfoProvider.SetUserInfo(CurrentUser);
                }

                // check security question and answer
                //if (SecurityQuestionEnabled && !CUFSecurityQuestionAnswers.CheckUserSecurityQuestionAnswer())
                //{
                //    RedirectToURL(SecurityQuestionAnswerURL);
                //}
                //else
                //{
                //    RedirectToURL(AcceptedURL);
                //}
            }
        }

        #endregion general events

        #region text helper

        /// <summary>
        /// Get string property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected string GetSafeStringValue(string p_key, string p_default)
        {
            string value = p_default;

            object obj = GetValue(p_key);

            if (obj != null)
            {
                value = obj.ToString();
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                value = p_default;
            }

            return value;
        }

        /// <summary>
        /// Get Bool property value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected bool GetSafeBoolValue(string p_key, bool p_default)
        {
            bool value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!bool.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        /// <summary>
        /// Get int value
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected int GetSafeIntValue(string p_key, int p_default)
        {
            int value = p_default;
            object obj = GetValue(p_key);
            if (obj != null)
            {
                if (!int.TryParse(obj.ToString(), out value))
                {
                    value = p_default;
                }
            }
            return value;
        }

        #endregion text helper

        #region redirection

        /// <summary>
        /// Redirect to a page
        /// </summary>
        /// <param name="p_url"></param>
        protected void RedirectToURL(string p_url)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                URLHelper.Redirect(ResolveUrl(p_url));
            }
        }

        #endregion redirection
    }
}