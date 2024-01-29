using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;


namespace ExcellaLite.Account
{
    public partial class Manage : BasePage
    {
        protected string SuccessMessage
        {
            get;
            private set;
        }

        protected bool CanRemoveExternalLogins
        {
            get;
            private set;
        }

        protected void Page_Load()
        {
            if (user.isLogin)
            {
                if (!IsPostBack)
                {

                }
            } else
            {
                Utils.responseRedirect("Default.aspx");
            }
        }

        protected void setPassword_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                string ThePassword = password.Text;
                DRspUser_UpdatePassword update = SQLData.spUser_UpdatePassword(user.userID, EncryptionManager.EncryptExternalPassword(ThePassword));
                if ((update.Count > 0)&&(update.RowUpdated(0) > 0))
                {
                    newPasswordMessage.Text = "Password has been changed.";
                }
                else
                {
                    newPasswordMessage.Text = update.ErrorMessage;
                }
            }
        }


        protected void externalLoginsList_ItemDeleting(object sender, ListViewDeleteEventArgs e)
        {

        }

        protected T Item<T>() where T : class
        {
            return GetDataItem() as T ?? default(T);
        }


        protected static string ConvertToDisplayDateTime(DateTime? utcDateTime)
        {
            // You can change this method to convert the UTC date time into the desired display
            // offset and format. Here we're converting it to the server timezone and formatting
            // as a short date and a long time string, using the current thread culture.
            return utcDateTime.HasValue ? utcDateTime.Value.ToLocalTime().ToString("G") : "[never]";
        }
    }
}