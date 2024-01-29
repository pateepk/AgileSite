using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Any exception thrown by ContactImport should be passed to client translated and with correct HTTP status code. 
    /// This class ensure such a behavior.
    /// </summary>
    internal class ContactImportExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            // OperationCanceledException is caused by client being disconnected, which doesn't need to be handled.
            if (context.Exception is OperationCanceledException)
            {
                return;
            }

            if (context.Exception is UnauthorizedAccessException)
            {
                // If windows authentication is used, status code 401: Unauthorized has to be set to response in order to force client to send Authentication token in next request.
                // Otherwise the Membership context will be flushed and server would fail to recognize user.
                //
                // If using forms authentication, ASP.NET automatically redirects after sending status 401, client caller would never get the response thus cannot handle it properly.
                // Instead of the status code 401, the status code 403: Forbidden is used in a such scenario. Redirecting to the given URL will handle the client side of the application.
                bool isWindowsAuthentication = (HttpContext.Current != null) && (HttpContext.Current.User != null) && (HttpContext.Current.User.Identity is WindowsIdentity);
                var responseCode = isWindowsAuthentication ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden;

                EventLogProvider.LogException("ContactImportController", "CONTACT_IMPORT_EXCEPTION", context.Exception);

                throw new HttpResponseException(responseCode);
            }

            if (context.Exception is ContactImportException)
            {
                var contactImportException = context.Exception as ContactImportException;

                EventLogProvider.LogException("ContactImportController", "CONTACT_IMPORT_EXCEPTION", contactImportException);

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ResHelper.GetString(contactImportException.UIMessage)),
                });
            }

            // Is Exception
            EventLogProvider.LogException("ContactImportController", "CONTACT_IMPORT_EXCEPTION", context.Exception);

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ResHelper.GetString("om.contact.importcsv.unknownerror")),
            });
        }
    }
}
