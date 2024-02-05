using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using System;
using System.Net;

namespace CMSApp.CMSWebParts.WTE.TSW
{
    public partial class WTE_Custom_JWT_SSO : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// The URL
        /// </summary>
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

        /// <summary>
        /// The button CSS Class
        /// </summary>
        public string ButtonCSSClass
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ButtonCSSClass"), "btn btn-primary");
            }
            set
            {
                SetValue("ButtonCSSClass", value);
            }
        }

        /// <summary>
        /// The text on the primary "button"
        /// </summary>
        public string ButtonText
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ButtonText"), "Enter the Chat Room");
            }
            set
            {
                SetValue("ButtonText", value);
            }
        }

        /// <summary>
        /// The room id
        /// </summary>
        public string RoomID
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("RoomID"), "5f9a63b5af6dc75d2d3d7030");
            }
            set
            {
                SetValue("RoomID", value);
            }
        }

        /// <summary>
        /// The secret key
        /// </summary>
        public string Secret
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("Secret"), "");
            }
            set
            {
                SetValue("Secret", value);
            }
        }

        /// <summary>
        /// The Site domain
        /// </summary>
        public string SiteDomain
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("SiteDomain"), "thedarkpools");
            }
            set
            {
                SetValue("SiteDomain", value);
            }
        }

        #endregion "Properties"

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
                hplLogin.CssClass = ButtonCSSClass;
                hplLogin.Text = ButtonText;

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

        #endregion "Methods"

        /// <summary>
        /// Encode string to base 64 bytes
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string Base64Encode(string plainText)
        {
            string ret = String.Empty;
            if (!String.IsNullOrWhiteSpace(plainText))
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                ret = System.Convert.ToBase64String(plainTextBytes);
            }
            return ret;
        }


        private string GetUrl()
        {
            UserInfo p_ui = UserInfoProvider.GetFullUserInfo(MembershipContext.AuthenticatedUser.UserID);
            string uEmail = p_ui.Email;
            string uName = "";
            string uRole = "r";
            string secret = Base64Encode(Secret);

            if (String.IsNullOrEmpty(p_ui.LastName) || String.IsNullOrEmpty(p_ui.FirstName))
            {
                uName = "Anonymous";
            }
            else
            {
                uName = p_ui.FirstName + p_ui.LastName.Substring(0, 1);
            }

            if (p_ui.IsInRole("CMSDeskAdmin", CurrentSiteName, true, true))
            {
                uRole = "a";
            }

            string URL = UrlWithQS + "?e=" + uEmail + "&u=" + uName + "&p=" + uRole + "&r=" + RoomID + "&sn=" + SiteDomain;

            if (!String.IsNullOrWhiteSpace(secret))
            {
                URL += "&s=" + secret;
            }

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