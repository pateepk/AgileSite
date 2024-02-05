using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;


using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Activities.Loggers;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Protection;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace WTE.SVC
{
    /// <summary>
    /// Summary description for MoblzService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class MoblzWebServiceASMX : System.Web.Services.WebService
    {

        public MoblzWebServiceASMX()
        {

            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
        }

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="p_userName"></param>
        /// <param name="p_password"></param>
        /// <returns></returns>
        [WebMethod]
        public bool Login(string p_userName, string p_password)
        {
            return MoblzAuthentication.Login(p_userName, p_password);
        }

        /// <summary>
        /// Authenicate a user with full info.
        /// </summary>
        /// <param name="p_loginInfo"></param>
        /// <returns></returns>
        [WebMethod]
        public bool AuthenticateUser(LoginData p_loginInfo)
        {
            return MoblzAuthentication.AuthenticateUser(p_loginInfo);
        }

        /// <summary>
        /// Register a user
        /// </summary>
        /// <param name="p_userName"></param>
        /// <param name="p_password"></param>
        /// <param name="p_email"></param>
        /// <param name="p_firstName"></param>
        /// <param name="p_lastname"></param>
        /// <param name="p_id"></param>
        /// <returns></returns>
        [WebMethod]
        public MOBLZAuthenticationInfo RegisterUser(string p_userName, string p_password, string p_email, string p_firstName, string p_lastname, string p_id)
        {
            return MoblzAuthentication.RegisterUser(p_userName, p_password, p_email, p_firstName, p_lastname, p_id);
        }


        /// <summary>
        /// Register a user account
        /// </summary>
        /// <param name="p_registrationinfo"></param>
        /// <returns></returns>
        [WebMethod]
        public MOBLZAuthenticationInfo RegisterUserAccount(MOBLZAuthenticationInfo p_registrationinfo)
        {
            return MoblzAuthentication.RegisterUserAccount(p_registrationinfo);
        }

        /// <summary>
        /// Get authenticated account info
        /// </summary>
        /// <param name="p_userName"></param>
        /// <param name="p_password"></param>
        /// <returns></returns>
        [WebMethod]
        public MOBLZAuthenticationInfo GetAuthenticatedAccount(string p_userName, string p_password)
        {
            return MoblzAuthentication.GetAuthenticatedAccount(p_userName, p_password);
        }
    }
}
