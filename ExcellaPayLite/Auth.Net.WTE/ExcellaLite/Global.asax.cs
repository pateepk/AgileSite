using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using ExcellaLite;
using PaymentProcessor.Web.Applications;

namespace ExcellaLite
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AuthConfig.RegisterOpenAuth();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            AppManager.ApplicationError();

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            AppManager.RequestStart();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            AppManager.RequestEnd();

        }

    }
}
