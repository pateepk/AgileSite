using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

using CMS.Core;

namespace CMS.WebApi
{
    /// <summary>
    /// Handles exceptions thrown when processing controller's actions, ensures that internal exceptions are properly logged to the Event log.
    /// </summary>
    /// <example>
    /// This example shows how to handle unauthorized request.
    /// <code>
    /// [HandleExceptions]
    /// public class MyController : ApiController
    /// {
    ///     public HttpResponseMessage GetValue()
    ///     {
    ///         ...
    ///         if(!IsAuthorized(MembershipContext.AuthenticatedUser))
    ///         {
    ///             // 401 status code Unauthorized is returned to the caller, so browser can handle the response properly
    ///             throw new UnauthorizedAccessException();
    ///         }
    ///         ...
    ///     }
    /// }
    /// </code>
    /// 
    /// This example shows how <see cref="HttpResponseException"/> behaves.
    /// <code>
    /// [HandleExceptions]
    /// public class MyController : ApiController
    /// {
    ///     public HttpResponseMessage GetValue()
    ///     {
    ///         ...
    ///         // 400 status code Bad request is returned to the caller together with the error message
    ///         throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Given property is in invalid format"))
    ///     }
    /// }
    /// </code>
    ///
    /// This example shows how other types of exceptions behave.
    /// <code>
    /// [HandleExceptions]
    /// public class MyController : ApiController
    /// {
    ///     public HttpResponseMessage GetValue()
    ///     {
    ///         ...
    ///         // 500 status code Internal server error is returned to the caller. All sensitive data like error message or stack trace are omitted
    ///         // Exception is logged to the Event log
    ///         throw new Exception()
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </para>
    /// <para>
    /// If exception is of type <see cref="UnauthorizedAccessException"/>, response is returned with the status code 
    /// <see cref="HttpStatusCode.Unauthorized"/>, so the browser can invoke login dialog.
    /// 
    /// All other exceptions are logged to the Event log and empty response with status code <see cref="HttpStatusCode.InternalServerError"/>
    /// is returned, except for the <see cref="HttpResponseException"/>. 
    /// This exception is considered as valid result and therefore is returned to the caller, including the error message.
    /// </para>
    /// </remarks>
    public class HandleExceptionsAttribute : ExceptionFilterAttribute
    {
        private readonly IEventLogService mEventLogService;

        /// <summary>
        /// Creates new instance of <see cref="HandleExceptionsAttribute"/>. 
        /// </summary>
        /// <remarks>
        /// For event logging uses default implementation of <see cref="IEventLogService"/>.
        /// </remarks>
        public HandleExceptionsAttribute()
        {
            mEventLogService = Service.Resolve<IEventLogService>();
        }


        /// <summary>
        /// Creates new instance of <see cref="HandleExceptionsAttribute"/>
        /// </summary>
        /// <param name="eventLogService">Event log service used for logging handled exceptions</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventLogService"/> is null</exception>
        public HandleExceptionsAttribute(IEventLogService eventLogService)
        {
            if (eventLogService == null)
            {
                throw new ArgumentNullException("eventLogService");
            }

            mEventLogService = eventLogService;
        }


        /// <summary>
        /// Raises the exception event.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is UnauthorizedAccessException)
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            else
            {
                // HttpResponseException is valid result from the controller,
                // therefore it should be returned unaltered to the caller.
                // OperationCanceledException is caused by client being disconnected, which doesn't need to be handled.
                if (actionExecutedContext.Exception is HttpResponseException || 
                    actionExecutedContext.Exception is OperationCanceledException)
                {
                    return;
                }

                // Other exceptions needs to be logged
                var controllerName = actionExecutedContext.ActionContext.ControllerContext.Controller.GetType().FullName;
                var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
                var exception = new Exception(string.Format("Unexpected error occurred in API call to Controller: {0}, Action: {1}", controllerName, actionName), actionExecutedContext.Exception);

                mEventLogService.LogException("Web API", "ApiMethod", exception);

                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
