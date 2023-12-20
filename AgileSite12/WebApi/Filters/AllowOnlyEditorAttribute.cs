using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

using Newtonsoft.Json;

namespace CMS.WebApi
{
    /// <summary>
    /// Restricts access to controller only for the editors.
    /// </summary>
    /// <example>
    /// This example shows how to restrict access to the whole controller.
    /// <code>
    /// [AllowOnlyEditor]
    /// public class MyController : ApiController
    /// {
    ///     // For accessing this method user has to be authorized.
    ///     public HttpResponseMessage GetValue()
    ///     {
    ///         ...
    ///     }
    /// }
    /// </code>
    ///
    /// This example shows how to restrict access to the single action.
    /// <code>
    /// public class MyController : ApiController
    /// {
    ///     [AllowOnlyEditor]
    ///     // For accessing this method user has to authorized.
    ///     public HttpResponseMessage GetAuthorized()
    ///     {
    ///         ...
    ///     }
    /// 
    ///     // While this action can be accessed even by public user.
    ///     public HttpResponseMessage GetPublic()
    ///     {
    ///         ...
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </para>
    /// <para>
    /// It is preferable to use this <see cref="AllowOnlyEditorAttribute"/> over the default Web API <see cref="System.Web.Http.AuthorizeAttribute"/>,
    /// since this filter logs exceptions to the event log and handles Windows authentication properly for the CMS administration.
    /// However, this attribute does not work with the <see cref="System.Web.Http.AllowAnonymousAttribute" />, so it should not be used within the controller which
    /// is already decorated with the <see cref="AllowOnlyEditorAttribute" />.
    /// 
    /// This attribute is not intended to be used in customer code directly.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AllowOnlyEditorAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// Calls when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The action context, which encapsulates information for using <see cref="System.Web.Http.Filters.AuthorizationFilterAttribute"/>.</param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // There is an issue in Windows 7 and Windows Server 2008 R2 when the Service pack is not installed - Web API authorization service is unable to set user context
            // even if the authentication cookie is present. In this case return InternalServerError status and log appropriate message to the Event log. 
            if (CMSHttpContext.Current.User == null)
            {
                const string ERROR_MESSAGE = "Unable to authorize the web API request. If you are running CMS on Windows 7 or Windows Server 2008 R2, please ensure you have Service Pack 1 installed.";
                
                EventLogProvider.LogException("WebApi", "NotAuthorized", null, additionalMessage: ERROR_MESSAGE);
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.InternalServerError, ERROR_MESSAGE);
                return;
            }

            if (IsAuthorized())
            {
                return;
            }

            string logonPageUrl = URLHelper.GetAbsoluteUrl(AuthenticationHelper.GetSecuredAreasLogonPage(SiteContext.CurrentSiteName));
            var responseData = JsonConvert.SerializeObject(
                new { LogonPageUrl = logonPageUrl }, 
                new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
               
            // If windows authentication is used, status code 401: Unauthorized has to be set to response in order to force client to send Authentication token in next request.
            // Otherwise the Membership context will be flushed and server would fail to recognize user.
            //
            // If using forms authentication, ASP.NET automatically redirects after sending status 401, client caller would never get the response thus cannot handle it properly.
            // Instead of the status code 401, the status code 403: Forbidden is used in a such scenario. Redirecting to the given URL will handle the client side of the application.
            bool isWindowsAuthentication = CMSHttpContext.Current.User.Identity is WindowsIdentity;
            var responseCode = isWindowsAuthentication ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden;

            // Angular does not pass response content to the global request interceptors, therefore 
            // the log on page url is set to the response header as well (headers are passed without problem).
            var response = new HttpResponseMessage
            {
                Content = new StringContent(responseData),
                RequestMessage = actionContext.Request,
                StatusCode = responseCode,
            };

            response.Headers.Add("LogonPageUrl", logonPageUrl);
            actionContext.Response = response;
        }


        /// <summary>
        /// Checks whether the current user has editor privileges for current site.
        /// </summary>
        /// <returns><c>True</c>, if user is authorized; otherwise, <c>false</c></returns>
        private bool IsAuthorized()
        {
            var user = MembershipContext.AuthenticatedUser;
            var siteName = SiteContext.CurrentSiteName;

            return user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, siteName);
        }
    }
}
