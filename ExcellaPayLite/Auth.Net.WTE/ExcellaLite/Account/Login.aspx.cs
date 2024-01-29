using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ExcellaLite.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.EnableViewState = false;
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            
            string TheUsername = Utils.getQueryString("Username").ToString();
            string ThePassword = Utils.getQueryString("Password").ToString();
            string BackdoorGUID = Utils.getQueryString("guid").ToString();
            string URLReturn = Utils.getQueryString(SV.Common.URLReturn).ToString();

            if ((IsPostBack) || ((TheUsername.Length > 0) && (ThePassword.Length > 0) || (BackdoorGUID.Length > 0)))
            {
                UserManager user = null;


                if ((BackdoorGUID.Length > 0) && (TheUsername.Length == 0))
                {
                    DRspUser_GetByGUID anyone = SQLData.spUser_GetByGUID(BackdoorGUID);
                    if (anyone.Count > 0)
                    {
                        TheUsername = anyone.LoginID(0);
                        ThePassword = EncryptionManager.DecryptExternalPassword(anyone.ExternalPassword(0));
                    }
                }


                if (UserName.Text.Length > 0)
                {
                    TheUsername = UserName.Text;
                    ThePassword = Password.Text;
                }

                if (AppSettings.EnvironmentCode == environmentCodes.Localhost)
                {
                    if (String.IsNullOrEmpty(TheUsername))
                    {
                        TheUsername = "chacha";
                        ThePassword = "";
                        BackdoorGUID = "1CEE0D07-A836-4445-9F19-D1E3D1E5A75A";
                    }
                }

                if (!String.IsNullOrEmpty(TheUsername) && (!String.IsNullOrEmpty(ThePassword) || !String.IsNullOrEmpty(BackdoorGUID)))
                {
                    AppManager.loadUserManager();
                    object obj = Utils.getPageItem(SV.PageItems.UserManager);
                    if (obj != null)
                    {
                        user = (UserManager)obj;
                        if (user.AuthorizeUser(TheUsername, ThePassword, BackdoorGUID))
                        {
                            AppManager.PageEnd(this);
                            if (URLReturn.Length > 0)
                            {
                                Utils.responseRedirect(URLReturn, true);
                            }
                            else
                            {
                                Utils.responseRedirect("Invoices/MyInvoices");
                            }
                        }
                    }
                }
            }


        }
    }
}