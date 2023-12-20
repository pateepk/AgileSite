using System.Net;
using System.Web.Http.Filters;

using CMS.Core;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Attribute handling errors in the Page and the Form builders.
    /// </summary>
    internal class LogExceptionApiFilterAttribute : ExceptionFilterAttribute
    {
        private readonly string exceptionSource;
        private readonly IEventLogService eventLogService;


        /// <summary>
        /// Initializes a new instance of the <see cref="LogExceptionApiFilterAttribute"/> class.
        /// </summary>
        /// <param name="exceptionSource">The exception source.</param>
        public LogExceptionApiFilterAttribute(string exceptionSource)
            : this(exceptionSource, Service.Resolve<IEventLogService>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LogExceptionApiFilterAttribute"/> class.
        /// </summary>
        /// <param name="exceptionSource">The exception source.</param>
        /// <param name="eventLogService">The event log service that writes data into an appropriate output.</param>
        internal LogExceptionApiFilterAttribute(string exceptionSource, IEventLogService eventLogService)
        {
            this.exceptionSource = exceptionSource;
            this.eventLogService = eventLogService;
        }



        /// <summary>
        /// Logs exception to the event log and response with 500 status code.
        /// </summary>
        /// <param name="actionExecutedContext">Represents the action of the HTTP executed context.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            string eventCode = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName + "_" + actionExecutedContext.ActionContext.ActionDescriptor.ActionName;

            eventLogService.LogException(exceptionSource, eventCode, actionExecutedContext.Exception);

            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.StatusCode = HttpStatusCode.InternalServerError;
            }
        }
    }
}
