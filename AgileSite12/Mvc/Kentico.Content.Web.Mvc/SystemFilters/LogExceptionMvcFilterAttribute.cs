using System.Web.Mvc;

using CMS.Core;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Attribute handling errors in Form builder.
    /// </summary>
    internal class LogExceptionMvcFilterAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly string exceptionSource;
        private readonly IEventLogService eventLogService;


        /// <summary>
        /// Initializes a new instance of the <see cref="LogExceptionMvcFilterAttribute"/> class.
        /// </summary>
        /// <param name="exceptionSource">The exception source.</param>
        public LogExceptionMvcFilterAttribute(string exceptionSource)
            : this(exceptionSource, Service.Resolve<IEventLogService>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LogExceptionMvcFilterAttribute"/> class.
        /// </summary>
        /// <param name="exceptionSource">The exception source.</param>
        /// <param name="eventLogService">The event log service that writes data into an appropriate output.</param>
        internal LogExceptionMvcFilterAttribute(string exceptionSource, IEventLogService eventLogService)
        {
            this.exceptionSource = exceptionSource;
            this.eventLogService = eventLogService;
        }


        /// <summary>
        /// Logs exception to the event log and response with 500 status code.
        /// </summary>
        /// <param name="filterContext">Provides the exception context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                string eventCode = filterContext.RouteData.Values["controller"]?.ToString() + "_" + filterContext.RouteData.Values["action"]?.ToString();

                eventLogService.LogException(exceptionSource, eventCode, filterContext.Exception);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 500;
            }
        }
    }
}
