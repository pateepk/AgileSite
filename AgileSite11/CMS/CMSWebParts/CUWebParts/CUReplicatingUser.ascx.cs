using System;
using System.Text;
using System.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class CUReplicatingUser : CMSAbstractWebPart
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("RedirectURL"), "");
            }
            set
            {
                this.SetValue("RedirectURL", value);
            }
        }

        public int ReplicatingAdmin
        {
            get
            {
                int ret = 0;

                object o = SessionHelper.GetValue("ReplicatingAdmin");
                if (o != null)
                {
                    ret = (int)o;
                }
                return ret;
            }
        }

        #endregion "Properties"

        protected void Page_Load(object sender, EventArgs e)
        {
            //RedirectURL = "~/Secure/sadmin";

            if (!Page.IsPostBack)
            {
                if (ReplicatingAdmin != 0)
                {
                    btnReturnToAdmin.Visible = true;
                }
            }
        }

        protected void ReturnToAdmin_Click(Object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            int adminId = ReplicatingAdmin;

            if (adminId != 0)
            {
                //get user to replicate
                UserInfo userInfo = UserInfoProvider.GetUserInfo(adminId);

                if (userInfo != null)
                {
                    //log out replicated user
                    //AuthenticationHelper.LogoutUser();

                    SessionHelper.Remove("ReplicatingAdmin");

                    //log in admin
                    AuthenticationHelper.AuthenticateUser(userInfo.UserName, false);

                    //redirect to replicated user initial page

                    string replicatingFrom = SessionHelper.GetValue("ReplicatingFrom") as string;

                    if (!string.IsNullOrWhiteSpace(replicatingFrom))
                    {
                        SessionHelper.Remove("ReplicatingFrom");
                        Response.Redirect(replicatingFrom);
                    }
                    else
                    {
                        Response.Redirect(RedirectURL);
                    }
                }
                else
                {
                    lblMsg.Text = "Unable to get user info to redirect to admin.";
                }
            }
        }
    }
}