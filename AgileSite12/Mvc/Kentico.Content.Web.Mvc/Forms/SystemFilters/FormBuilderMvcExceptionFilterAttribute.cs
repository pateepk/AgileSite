using System.Web.Mvc;

using CMS.EventLog;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Attribute handling errors in Form builder.
    /// </summary>
    internal class FormBuilderMvcExceptionFilterAttribute: FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// Logs exception to the event log and response with 500 status code.
        /// </summary>
        /// <param name="filterContext">Provides the exception context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, filterContext.RouteData.Values["action"]?.ToString(), filterContext.Exception);
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 500;
            }
        }
    }
}
