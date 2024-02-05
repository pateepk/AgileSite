using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ExcellaLite.Account
{
    public partial class Logout : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            user.Logout();
            Utils.responseRedirect("Default.aspx");
        }
    }
}