using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.WTE.DPC
{
    public partial class WTE_Custom_DPC_JWT_SSO : CMSAbstractWebPart
    {
        #region "Properties"
        public string UrlWithQS
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("UrlWithQS"), null);
            }
            set
            {
                SetValue("UrlWithQS", value);

            }
        }
        #endregion

        #region "Methods"

        /// <summary>
        /// Content loaded event handler.
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }


        /// <summary>
        /// Initializes the control properties.
        /// </summary>
        protected void SetupControl()
        {
            if (this.StopProcessing)
            {
                // Do not process
            }
            else
            {
                hplLogin.NavigateUrl = GetUrl();
                btnLogin.Click += btnLogin_Click;
            }
        }


        /// <summary>
        /// Reloads the control data.
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();

            SetupControl();
        }

        #endregion

        private string GetUrl()
        {
            UserInfo p_ui = UserInfoProvider.GetFullUserInfo(MembershipContext.AuthenticatedUser.UserID);
            string uEmail = p_ui.Email;
            string uName = "";
            string uRole = "r";

            if (String.IsNullOrEmpty(p_ui.LastName) || String.IsNullOrEmpty(p_ui.FirstName))
            {
                uName = "Anonymous";
            }
            else
            {
                uName = p_ui.FirstName + p_ui.LastName.Substring(0, 1);
            }


            if (p_ui.IsInRole("CMSDeskAdmin", "DarkPoolCrypto", true, true))
            {
                uRole = "a";
            }

            string URL = UrlWithQS + "?e=" + uEmail + "&u=" + uName + "&p=" + uRole;

            try
            {
                WebClient client = new WebClient();
                string downloadString = client.DownloadString(URL);

                if (String.IsNullOrEmpty(downloadString))
                {
                    lblError.Text = "There was a problem with your login. Please try again. (1)";
                }
                else
                {
                    return downloadString;
                }
            }
            catch (WebException exp)
            {
                lblError.Text = "There was a problem with your login. Please try again. (2)<br>" + exp.ToString();
            }

            return "";
        }

        /// <summary>
        /// The login button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            UserInfo p_ui = UserInfoProvider.GetFullUserInfo(MembershipContext.AuthenticatedUser.UserID);
            string uEmail = p_ui.Email;
            string uName = p_ui.UserName;
            string uRole = "r";

            if (p_ui.IsInRole("CMSDeskAdmin", "TheStockWhisperer", true, true))
            {
                uRole = "a";
            }

            string URL = "/EnterTheJPTR/sso_entry_point_DPC.aspx?e=" + uEmail + "&u=" + uName + "&p=" + uRole;

            try
            {
                WebClient client = new WebClient();
                string downloadString = client.DownloadString(URL);

                if (String.IsNullOrEmpty(downloadString))
                {
                    lblError.Text = "There was a problem with your login. Please try again. (1)";
                }
                else
                {

                    System.Web.HttpContext.Current.Response.Write("<script>");
                    System.Web.HttpContext.Current.Response.Write("window.open('" + downloadString + "','_blank')");
                    System.Web.HttpContext.Current.Response.Write("</script>");

                    //System.Web.HttpContext.Current.Response.Redirect(downloadString);
                    //System.Web.HttpContext.Current.Response.End();
                }
            }
            catch (WebException exp)
            {
                lblError.Text = "There was a problem with your login. Please try again. (2)<br>" + exp.ToString();
            }
        }
    }
}