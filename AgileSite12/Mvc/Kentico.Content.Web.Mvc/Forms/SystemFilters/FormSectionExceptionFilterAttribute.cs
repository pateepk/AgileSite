using System.Web.Mvc;

using CMS.EventLog;

using Kentico.Forms.Web.Mvc;

namespace Kentico.Forms.Mvc
{
    /// <summary>
    /// Attribute handling errors occurring during form sections rendering.
    /// </summary>
    public sealed class FormSectionExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        /// <summary>
        /// Logs exception to the event log. Responds with 200 status code and no markup.
        /// </summary>
        /// <param name="filterContext">Provides the exception context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {                
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 200;

                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, filterContext.RouteData.Values["action"]?.ToString(), filterContext.Exception);
            }
        }
    }
}
