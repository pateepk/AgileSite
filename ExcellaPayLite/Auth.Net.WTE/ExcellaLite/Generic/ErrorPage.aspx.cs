using PaymentProcessor.Web.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ExcellaLite.Generic
{
    public partial class ErrorPage : Page
    {

        protected int ErrorNumber = 0;

        protected void Page_Load(object sender, EventArgs e)
        {

            ErrorNumber = Utils.getQueryString("ErrorNumber").ToInt();

        }
    }
}