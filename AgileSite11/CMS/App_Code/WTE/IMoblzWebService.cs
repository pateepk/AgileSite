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

/// <summary>
/// MOBLZ login service
/// </summary>
// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMoblzWebService" in both code and config file together.
[ServiceContract]
public interface IMoblzWebService
{
    //[OperationContract]
    //void DoWork();

    //[OperationContract]
    //string GetData(int value);

    //[OperationContract]
    //CompositeType GetDataUsingDataContract(CompositeType composite);

    // TODO: Add your service operations here

    [OperationContract]
    string HelloWorld();

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <returns></returns>
    [OperationContract]
    bool Login(string p_userName, string p_password);

    /// <summary>
    /// Authenicate a user with full info.
    /// </summary>
    /// <param name="p_loginInfo"></param>
    /// <returns></returns>
    [OperationContract]
    bool AuthenticateUser(LoginData p_loginInfo);

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
    [OperationContract]
    MOBLZAuthenticationInfo RegisterUser(string p_userName, string p_password, string p_email, string p_firstName, string p_lastname, string p_id);

    /// <summary>
    /// Register a user
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <param name="p_email"></param>
    /// <param name="p_firstName"></param>
    /// <param name="p_lastName"></param>
    /// <returns></returns>
    [OperationContract]
    MOBLZAuthenticationInfo RegisterUserAccount(MOBLZAuthenticationInfo p_registrationinfo);

    /// <summary>
    /// Get authenticated account info
    /// </summary>
    /// <param name="p_userName"></param>
    /// <param name="p_password"></param>
    /// <returns></returns>
    [OperationContract]
    MOBLZAuthenticationInfo GetAuthenticatedAccount(string p_userName, string p_password);
}

// Use a data contract as illustrated in the sample below to add composite types to service operations.
[DataContract]
public class CompositeType
{
    bool boolValue = true;
    string stringValue = "Hello ";

    [DataMember]
    public bool BoolValue
    {
        get { return boolValue; }
        set { boolValue = value; }
    }

    [DataMember]
    public string StringValue
    {
        get { return stringValue; }
        set { stringValue = value; }
    }
}

/// <summary>
/// User data
/// </summary>
[DataContract]
[Serializable()]
public class UserData
{
    private int? _userid = null;
    private string _firstName = String.Empty;
    private string _lastName = String.Empty;
    private string _emailAddress = String.Empty;

    [DataMember]
    public int? UserID
    {
        get
        {
            return _userid;
        }
        set
        {
            _userid = value;
        }
    }

    [DataMember]
    public string FirstName
    {
        get
        {
            return _firstName;
        }
        set
        {
            _firstName = value;
        }
    }

    [DataMember]
    public String LastName
    {
        get
        {
            return _lastName;
        }
        set
        {
            _lastName = value;
        }
    }

    [DataMember]
    public string EmailAddress
    {
        get
        {
            return _emailAddress;
        }
        set
        {
            _emailAddress = value;
        }
    }
}

/// <summary>
/// User login info
/// </summary>
[DataContract]
[Serializable()]
public class LoginData
{
    private string _token = String.Empty;
    private bool _loginValid = false;
    private int _loginStatusCode = 0;
    private string _errorMessage = String.Empty;

    private string _username = String.Empty;
    private string _password = String.Empty;
    private string _mobileDeviceID = String.Empty;
    private string _assignRole = String.Empty;

    [DataMember]
    public string Token
    {
        get
        {
            return _token;
        }

        set
        {
            _token = value;
        }
    }

    [DataMember]
    public bool LoginValid
    {
        get
        {
            return _loginValid;
        }
        set
        {
            _loginValid = value;
        }
    }

    [DataMember]
    public int LoginStatusCode
    {
        get
        {
            return _loginStatusCode;
        }
        set
        {
            _loginStatusCode = value;
        }
    }

    [DataMember]
    public string ErrorMessage
    {
        get
        {
            return _errorMessage;
        }
        set
        {
            _errorMessage = value;
        }
    }

    [DataMember]
    public string UserName
    {
        get
        {
            return _username;
        }
        set
        {
            _username = value;
        }
    }

    [DataMember]
    public string Password
    {
        get
        {
            return _password;
        }
        set
        {
            _password = value;
        }
    }

    [DataMember]
    public string MobileDeviceID
    {
        get
        {
            return _mobileDeviceID;
        }
        set
        {
            _mobileDeviceID = value;
        }
    }

    /// <summary>
    /// Role name separated by ";"
    /// </summary>
    [DataMember]
    public string AssignRoles
    {
        get
        {
            return _assignRole;
        }
        set
        {
            _assignRole = value;
        }
    }
}

/// <summary>
/// Reponse for authentication
/// </summary>
[DataContract]
[Serializable()]
public class MOBLZAuthenticationInfo
{
    private LoginData _loginInfo = new LoginData();
    private UserData _userInfo = new UserData();

    [DataMember]
    public LoginData LoginInfo
    {
        get
        {
            return _loginInfo;
        }
        set
        {
            _loginInfo = value;
        }
    }

    [DataMember]
    public UserData UserInfo
    {
        get
        {
            return _userInfo;
        }
        set
        {
            _userInfo = value;
        }
    }
}