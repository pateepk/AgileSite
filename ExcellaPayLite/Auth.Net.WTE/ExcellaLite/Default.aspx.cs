using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PaymentProcessor.Web.Applications;
using System.Net;
using System.Text;
using System.IO;
using System.ServiceModel.Channels;
using WTE.Communication;

namespace ExcellaLite
{
    public partial class _Default : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.EnableViewState = false;
            string URLReturn = Utils.getQueryString(SV.Common.URLReturn).ToString();
            if (URLReturn.Length > 0)
            {
                if (user.isLogin)
                {
                    Utils.responseRedirect(URLReturn, true);
                }
                else
                {
                    Utils.setSessionObject(SV.Sessions.URLReturn, URLReturn);
                }
            }
        }
    }
}