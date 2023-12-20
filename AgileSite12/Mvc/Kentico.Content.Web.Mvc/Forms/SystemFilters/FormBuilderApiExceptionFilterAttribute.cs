using System.Net;
using System.Web.Http.Filters;

using CMS.EventLog;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Attribute handling errors in Form builder.
    /// </summary>
    internal class FormBuilderApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Logs exception to the event log and response with 500 status code.
        /// </summary>
        /// <param name="actionExecutedContext">Represents the action of the HTTP executed context.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, actionExecutedContext.ActionContext.ActionDescriptor.ActionName, actionExecutedContext.Exception);

            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.StatusCode = HttpStatusCode.InternalServerError;
            }
        }
    }
}
