using System;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUAcceptDisclosure : CMSAbstractWebPart
    {
        #region "Properties"
        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
		public string AcceptedURL
		{
			get
			{
				return ValidationHelper.GetString(this.GetValue("AcceptedURL"), "");
			}
			set
			{
				this.SetValue("AcceptedURL", value);
			}
		}

		public bool LockEmail
		{
			get
			{
				return ValidationHelper.GetBoolean(this.GetValue("LockEmail"), false);
			}
			set
			{
				this.SetValue("LockEmail", value);
			}
		}

        #endregion

        protected void Page_Load(object sender, EventArgs e)
		{
			if (LockEmail)
			{
				txtEmail.Enabled = false;
			}
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

        protected void cvAcceptTerms_ServerValidate(Object source, ServerValidateEventArgs args)
        {
            args.IsValid = (cbAcceptTerms.Checked == true);
        }

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
                Response.Redirect(AcceptedURL);
            }
        }
    }
}