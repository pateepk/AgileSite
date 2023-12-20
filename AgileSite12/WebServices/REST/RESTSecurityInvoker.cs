using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.WebServices
{
    /// <summary>
    /// Security handler over the REST requests
    /// </summary>
    public class RESTSecurityInvoker : Attribute, IOperationBehavior, IOperationInvoker
    {
        #region "Constants"

        /// <summary>
        /// Constant to identify the hash authentication requirement 
        /// </summary>
        public const string HASH_AUTHENTICATED_USERNAME = "##hashauth##";

        /// <summary>
        /// Basic authentication
        /// </summary>
        public const string BASIC_AUTHENTICATION_TYPE = "basic";

        /// <summary>
        /// Current user name key
        /// </summary>
        private const string USER_NAME_KEY = "RESTCurrentUserName";

        /// <summary>
        /// Is hash authenticated key
        /// </summary>
        private const string HASH_AUTH_KEY = "RESTHashAuthenticated";

        #endregion


        #region "Variables"

        private static RESTSecurityInvoker mInternalImplementation;

        private IOperationInvoker invoker;
        private string mCurrentSiteName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Singleton instance used to access override-able non-static fields. 
        /// </summary>
        private static RESTSecurityInvoker InternalImplementation
        {
            get
            {
                return mInternalImplementation ?? (mInternalImplementation = new RESTSecurityInvoker());
            }
        }


        /// <summary>
        /// Incoming request
        /// </summary>
        private static IncomingWebRequestContext IncomingRequest
        {
            get
            {
                return WebOperationContext.IncomingRequest;
            }
        }


        /// <summary>
        /// Web operation context
        /// </summary>
        private static WebOperationContext WebOperationContext
        {
            get
            {
                return WebOperationContext.Current;
            }
        }


        /// <summary>
        /// Gets current site name (retrieved from domain accessed).
        /// </summary>
        private string CurrentSiteName
        {
            get
            {
                // Check the application context first
                if (!string.IsNullOrEmpty(SiteContext.CurrentSiteName))
                {
                    return SiteContext.CurrentSiteName;
                }

                // Get the site from domain
                if (string.IsNullOrEmpty(mCurrentSiteName))
                {
                    mCurrentSiteName = GetRunningSiteName();
                }

                return mCurrentSiteName;
            }
        }


        /// <summary>
        /// Indicates whether the query is translation query.
        /// </summary>
        public bool IsTranslation
        {
            get
            {
                return ValidationHelper.GetBoolean(GetQueryParam("translate"), false);
            }
        }


        /// <summary>
        /// Indicates whether the REST service is enabled (in settings).
        /// </summary>
        public bool RESTServiceEnabled
        {
            get
            {
                var settingKey = IsTranslation ? ".CMSEnableTranlsationRESTService" : ".CMSRESTServiceEnabled";
                return SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + settingKey);
            }
        }


        /// <summary>
        /// Indicates whether the access to the documents is read-only. If true, only GET requests are allowed.
        /// </summary>
        public bool DocumentAccessReadOnly
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSRESTDocumentsReadOnly");
            }
        }


        /// <summary>
        /// Indicates whether the access to the objects is read-only. If true, only GET requests are allowed.
        /// </summary>
        public bool ObjectAccessReadOnly
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(CurrentSiteName + ".CMSRESTObjectsReadOnly");
            }
        }


        /// <summary>
        /// Gets the type of the service which is enabled (0 = Everything, 1 = Only objects, 2 = Only documents).
        /// </summary>
        public int ServiceTypeEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetIntValue(CurrentSiteName + ".CMSRESTServiceTypeEnabled");
            }
        }

        #endregion


        #region "IOperationBehavior Members"

        /// <summary>
        /// Initializes the invoker.
        /// </summary>
        /// <param name="operationDescription">Operation description</param>
        /// <param name="dispatchOperation">Dispatch operation</param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            invoker = dispatchOperation.Invoker;
            dispatchOperation.Invoker = this;
        }


        /// <summary>
        /// Not needed, remains empty, does nothing.
        /// </summary>
        /// <param name="operationDescription">Operation description</param>
        /// <param name="clientOperation">Client operation</param>
        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }


        /// <summary>
        /// Not needed, remains empty, does nothing.
        /// </summary>
        /// <param name="operationDescription">Operation description</param>
        /// <param name="bindingParameters">Binding parameters</param>
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }


        /// <summary>
        /// Not needed, remains empty, does nothing.
        /// </summary>
        /// <param name="operationDescription">Operation description</param>
        public void Validate(OperationDescription operationDescription)
        {
        }

        #endregion


        #region "IOperationInvoker Members"

        /// <summary>
        /// Invokes the operation only when authentication went well.
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <param name="inputs">List of inputs</param>
        /// <param name="outputs">List of outputs</param>
        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            // Force the site to reload according to current domain
            mCurrentSiteName = null;

            // Check whether the service is allowed and that it was accessed via ~/rest URL (using rewriter)
            if (!RESTServiceEnabled || String.IsNullOrEmpty(GetQueryParam(BaseRESTService.DOMAIN_QUERY_KEY)))
            {
                WebOperationContext.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
                outputs = null;
                return null;
            }

            object result = null;

            // Set the URL context for debug purposes
            RequestStockHelper.Add("ContextRawUrl", OperationContext.Current.IncomingMessageHeaders.To.PathAndQuery, true);

            // Ensure the connection scope
            var connectionScopeCreated = ConnectionContext.EnsureThreadScope(null);

            try
            {

                // Check other security settings (read-only settings)
                if (!IsTranslation && !OperationIsAllowed())
                {
                    WebOperationContext.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
                    outputs = null;
                }
                else
                {
                    // First try if there is an HTTP context of the application if not authenticate user in a standard way (hash / basic / forms)
                    if (!String.IsNullOrEmpty(AuthenticateUser()))
                    {
                        // Set the correct culture
                        string cultureName = SettingsHelper.AppSettings[BaseRESTService.CULTURE_KEY];
                        if (!String.IsNullOrEmpty(cultureName))
                        {
                            CultureInfo culture = new CultureInfo(cultureName);
                            Thread.CurrentThread.CurrentCulture = culture;
                            Thread.CurrentThread.CurrentUICulture = culture;
                        }

                        RequestStockHelper.Add("RequestActionContextUser", BaseRESTService.GetCurrentUser(), true);
                        result = invoker.Invoke(instance, inputs, out outputs);
                    }
                    else
                    {
                        // Set custom status code to 306 (non-existing status code) which will be handled by AuthenticationHandler and changed to 
                        // 401 Unauthorized (that's because 401 is automatically handled by ASP.NET and redirected to logon page, this way we can achieve to return 401)
                        WebOperationContext.OutgoingResponse.StatusCode = HttpStatusCode.Unused;
                        outputs = null;
                    }
                }
            }
            finally
            {
                if (connectionScopeCreated)
                {
                    ConnectionContext.DisposeThreadScope();
                }
            }

            return result;
        }


        /// <summary>
        /// Calls AllocateInputs on the invoker.
        /// </summary>
        public object[] AllocateInputs()
        {
            return invoker.AllocateInputs();
        }


        /// <summary>
        /// Not implemented method.
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <param name="inputs">List of inputs</param>
        /// <param name="callback">Callback object</param>
        /// <param name="state">State object</param>
        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Not implemented method.
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <param name="outputs">List of outputs</param>
        /// <param name="result">Result object</param>
        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Always true - we need synchronous behavior for authentication.
        /// </summary>
        public bool IsSynchronous
        {
            get
            {
                return true;
            }
        }

        #endregion


        #region "Authentication methods"

        /// <summary>
        /// Handles necessary actions to provide proper REST authentication response
        /// </summary>
        public static void HandleRESTAuthentication()
        {
            // Check status code 306 (code which is returned only when authentication failed in REST Service), change it to classical 401 - Unauthorized.
            // That's because 401 is automatically handled by ASP.NET and redirected to logon page, this way we can achieve to return 401 without ASP.NET to interfere.
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 306)
            {
                response.Headers["WWW-Authenticate"] = "Basic realm=\"CMS REST Service\"";
                response.StatusCode = 401;
            }
        }


        /// <summary>
        /// Checks hash parameter. Returns true if the parameter matches the requested URL. For request different from GET returns always false.
        /// </summary>
        public static bool CheckHash()
        {
            return InternalImplementation.CheckHashInternal();
        }


        /// <summary>
        /// Returns username of authenticated user.
        /// </summary>
        public static string GetUserName()
        {
            string userName = (string)RequestStockHelper.GetItem(USER_NAME_KEY);
            if (String.IsNullOrEmpty(userName))
            {
                userName = AuthenticateUser(true);
            }
            return userName;
        }


        /// <summary>
        /// Authenticates the user according to authentication type setting.
        /// </summary>
        public string AuthenticateUser()
        {
            return AuthenticateUser(true);
        }


        /// <summary>
        /// Authenticates the user according to authentication type setting.
        /// </summary>
        /// <param name="checkHash">If true, hash is checked and has higher priority than authentication</param>
        internal static string AuthenticateUser(bool checkHash)
        {
            var userName = InternalImplementation.GetAuthenticatedUserNameInternal(checkHash);

            RequestStockHelper.Add(USER_NAME_KEY, userName);

            return userName;
        }


        /// <summary>
        /// Authenticates the user using basic authentication.
        /// </summary>
        public static string AuthenticateUserBasic()
        {
            return InternalImplementation.AuthenticateUserBasicInternal();
        }


        /// <summary>
        /// Returns query string parameter.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        private static string GetQueryParam(string name)
        {
            NameValueCollection query = IncomingRequest.UriTemplateMatch.QueryParameters;
            return query[name];
        }


        /// <summary>
        /// Returns name of authenticated user.
        /// </summary>
        /// <param name="checkHash">If true, hash is checked and has higher priority than authentication</param>
        private string GetAuthenticatedUserNameInternal(bool checkHash)
        {
            if (checkHash && IsHashAuthenticated())
            {
                return HASH_AUTHENTICATED_USERNAME;
            }

            // Allow a white-listed AD user to access REST
            if (AuthenticationMode.IsWindowsAuthentication())
            {
                var allowedUserName = CoreServices.AppSettings["CMSRestWindowsAuthenticationUserName"];
                var actualUser = MembershipContext.AuthenticatedUser;

                if (!String.IsNullOrEmpty(allowedUserName) && allowedUserName.Equals(actualUser.UserName, StringComparison.InvariantCultureIgnoreCase))
                {
                    RequestStockHelper.Add(BaseRESTService.USER_KEY, actualUser, true);

                    return actualUser.UserName;
                }
            }

            return AuthenticateUserBasicInternal();
        }


        /// <summary>
        /// Checks hash parameter. Returns true if the parameter matches the requested URL. For request different from GET returns always false.
        /// </summary>
        private bool CheckHashInternal()
        {
            // Only read methods are allowed with hash
            if (!IncomingRequest.Method.Equals("get", StringComparison.OrdinalIgnoreCase))
            {
                RequestStockHelper.Add(HASH_AUTH_KEY, false);
                return false;
            }

            // If hash is not present in the query return false
            var hash = IncomingRequest.UriTemplateMatch.QueryParameters["hash"];
            if (string.IsNullOrEmpty(hash))
            {
                RequestStockHelper.Add(HASH_AUTH_KEY, false);
                return false;
            }

            // Check the hash
            var url = OperationContext.Current.IncomingMessageHeaders.To.PathAndQuery;

            url = URLHelper.RemoveParameterFromUrl(url, "hash");
            url = HttpUtility.UrlDecode(url);

            var hashValidationResult = RESTServiceHelper.IsUrlPathAndQueryHashValid(url, hash);
            RequestStockHelper.Add(HASH_AUTH_KEY, hashValidationResult);
            return hashValidationResult;
        }


        /// <summary>
        /// Authenticates the user using basic authentication.
        /// </summary>
        private string AuthenticateUserBasicInternal()
        {
            string username;
            string password;

            if (SecurityHelper.TryParseBasicAuthorizationHeader(IncomingRequest.Headers["Authorization"], out username, out password))
            {
                var siteName = GetRunningSiteName();

                var user = GetAuthenticatedUser(username, password, siteName);
                if (user != null)
                {
                    RequestStockHelper.Add(BaseRESTService.USER_KEY, user, true);
                    return user.UserName;
                }
            }

            return null;
        }


        private static string GetRunningSiteName()
        {
            var site = SiteInfoProvider.GetRunningSiteInfo(GetQueryParam(BaseRESTService.DOMAIN_QUERY_KEY), null);
            return site != null ? site.SiteName : string.Empty;
        }


        private static CurrentUserInfo GetAuthenticatedUser(string username, string password, string siteName)
        {
            var user = AuthenticationHelper.AuthenticateUser(username, password, siteName, false, AuthenticationSourceEnum.ExternalOrAPI);
            return user != null
                ? new CurrentUserInfo(user, true)
                : null;
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Check the read-only settings for object/document operations.
        /// </summary>
        private bool OperationIsAllowed()
        {
            string servicePath = IncomingRequest.UriTemplateMatch.Template.ToString();
            // Allow root service document
            if ((servicePath != "") && (servicePath != "/"))
            {
                // If the path is not root of the service, do the check of settings
                if (servicePath.StartsWith("/content/", StringComparison.OrdinalIgnoreCase))
                {
                    // Document request, check document settings
                    if ((ServiceTypeEnabled == 1) || (DocumentAccessReadOnly && (!IncomingRequest.Method.Equals("get", StringComparison.OrdinalIgnoreCase))))
                    {
                        return false;
                    }
                }
                else
                {
                    // Object request, check object settings
                    if ((ServiceTypeEnabled == 2) || (ObjectAccessReadOnly && (!IncomingRequest.Method.Equals("get", StringComparison.OrdinalIgnoreCase))))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the request is GET and the hash parameter is present and matches the requested URL.
        /// </summary>
        public static bool IsHashAuthenticated()
        {
            return RequestStockHelper.Contains(HASH_AUTH_KEY) ? (bool)RequestStockHelper.GetItem(HASH_AUTH_KEY) : InternalImplementation.CheckHashInternal();
        }

        #endregion
    }
}