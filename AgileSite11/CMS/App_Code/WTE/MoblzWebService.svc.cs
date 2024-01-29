using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using System.Web;
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
using WTE.SVC;


// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "MoblzWebService" in code, svc and config file together.
public class MoblzWebServiceWCF : IMoblzWebService
{
    //public void DoWork()
    //{
    //}

    //public string GetData(int value)
    //{
    //    return string.Format("You entered: {0}", value);
    //}

    //public CompositeType GetDataUsingDataContract(CompositeType composite)
    //{
    //    if (composite == null)
    //    {
    //        throw new ArgumentNullException("composite");
    //    }
    //    if (composite.BoolValue)
    //    {
    //        composite.StringValue += "Suffix";
    //    }
    //    return composite;
    //}

    /// <summary>
    /// Hello world!
    /// </summary>
    /// <returns></returns>
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
    public bool Login(string p_userName, string p_password)
    {
        return MoblzAuthentication.Login(p_userName, p_password);
    }

    /// <summary>
    /// Authenicate a user with full info.
    /// </summary>
    /// <param name="p_loginInfo"></param>
    /// <returns></returns>
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
    public MOBLZAuthenticationInfo RegisterUser(string p_userName, string p_password, string p_email, string p_firstName, string p_lastname, string p_id)
    {
        return MoblzAuthentication.RegisterUser(p_userName, p_password, p_email, p_firstName, p_lastname, p_id);
    }

    /// <summary>
    /// Register a user account
    /// </summary>
    /// <param name="p_registrationinfo"></param>
    /// <returns></returns>
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
    public MOBLZAuthenticationInfo GetAuthenticatedAccount(string p_userName, string p_password)
    {
        return MoblzAuthentication.GetAuthenticatedAccount(p_userName, p_password);
    }
}

