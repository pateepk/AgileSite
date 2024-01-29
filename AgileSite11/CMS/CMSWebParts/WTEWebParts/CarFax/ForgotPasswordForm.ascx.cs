using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;

using CMS.Helpers;
using CMS.Base;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.URLRewritingEngine;
using CMS.WebAnalytics;
using CMS.MacroEngine;

namespace CMSApp.CMSWebParts.WTEWebParts.CarFax
{
    public partial class ForgotPasswordForm : CMSAbstractWebPart
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets the sender e-mail (from).
        /// </summary>
        public string SendEmailFrom
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("SendEmailFrom"), GetStringValue(SiteContext.CurrentSiteName + ".CMSSendPasswordEmailsFrom", ""));
            }
            set
            {
                SetValue("SendEmailFrom", value);
            }
        }

        /// <summary>
        /// Gets or sets reset password url - this url is sent to user in e-mail when he wants to reset his password.
        /// </summary>
        public string ResetPasswordURL
        {
            get
            {
                return DataHelper.GetNotEmpty(URLHelper.GetAbsoluteUrl(GetValue("ResetPasswordURL").ToString()), AuthenticationHelper.GetResetPasswordUrl(SiteContext.CurrentSiteName));
            }
            set
            {
                SetValue("ResetPasswordURL", value);
            }
        }

        #endregion "Public properties"

        /// <summary>
        /// OnLoad override (show hide password retrieval).
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
			lblPasswdRetrieval.Text = GetString("LogonForm.lblPasswordRetrieval");
			btnPasswdRetrieval.Text = GetString("LogonForm.btnPasswordRetrieval");
            btnPasswdRetrieval.Click += new EventHandler(btnPasswdRetrieval_Click);
        }

        /// <summary>
        /// Retrieve the user password.
        /// </summary>
        private void btnPasswdRetrieval_Click(object sender, EventArgs e)
        {
            string value = txtPasswordRetrieval.Text.Trim();

            if (value != String.Empty)
            {
                // Prepare return URL
                string returnUrl = RequestContext.CurrentURL;

                AuthenticationHelper.ForgottenEmailRequest(value, SiteContext.CurrentSiteName, "LOGONFORM", SendEmailFrom, MacroContext.CurrentResolver, ResetPasswordURL, returnUrl);

                lblResult.Text = "Password reset email sent.";
                lblResult.Visible = true;
                pnlPasswdRetrieval.Visible = true;
            }
        }

    }
}