using System.Web;

namespace CMS.Base
{
    /// <summary>
    /// Holds events that allow performing of custom logic at specific points within the request processing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These request events allow you to handle most usable events from <see cref="HttpApplication"/> without the need to create a custom HTTP module or edit global.asax.
    /// </para>
    /// <para>
    /// The events are represented by fields of the <see cref="SimpleHandler"/> type. Handler methods need to be assigned to the Execute event of individual fields.
    /// </para>
    /// <para>
    /// The events are not raised out of web-based applications without the CMSApplication module (when using the Kentico API externally).
    /// </para>    
    /// </remarks>
    public sealed class RequestEvents
    {
        // NOTE - The events are placed in the order of their execution. Please maintain this order.

        /// <summary>
        /// Occurs before the <see cref="Begin"/> event.
        /// </summary>
        /// <remarks>        
        /// This event is related to the <see cref="RequestEvents"/> lifecycle only. The standard .NET <see cref="HttpApplication.BeginRequest"/> event can be invoked before this event.
        /// </remarks>
        public static SimpleHandler Prepare = new SimpleHandler
            {
                Name = "RequestEvents.Prepare", 
                Debug = false
            };


        /// <summary>
        /// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// </summary>
        /// <seealso cref="HttpApplication.BeginRequest"/>
        public static SimpleHandler Begin = new SimpleHandler
            {
                Name = "RequestEvents.Begin"
            };


        /// <summary>
        /// Occurs when a security module has established the identity of a user.
        /// </summary>
        /// <seealso cref="HttpApplication.AuthenticateRequest"/>
        public static SimpleHandler Authenticate = new SimpleHandler
            {
                Name = "RequestEvents.Authenticate"
            };


        /// <summary>
        /// Occurs when a security module has verified user authorization.
        /// </summary>
        /// <seealso cref="HttpApplication.AuthorizeRequest"/>
        public static SimpleHandler Authorize = new SimpleHandler
            {
                Name = "RequestEvents.Authorize"
            };


        /// <summary>
        /// Occurs when the user for the current request has been authorized.
        /// </summary>
        /// <seealso cref="HttpApplication.PostAuthorizeRequest"/>
        public static SimpleHandler PostAuthorize = new SimpleHandler
            {
                Name = "RequestEvents.PostAuthorize"
            };


        /// <summary>
        /// Occurs when ASP.NET bypasses execution of the current event handler and allows a caching module to serve a request from the cache.
        /// </summary>
        /// <seealso cref="HttpApplication.PostResolveRequestCache"/>
        public static SimpleHandler PostResolveRequestCache = new SimpleHandler
        {
            Name = "RequestEvents.PostResolveRequestCache"
        };


        /// <summary>
        /// Occurs when a handler is selected to respond to the request.
        /// </summary>
        /// <seealso cref="HttpApplication.MapRequestHandler"/>
        public static SimpleHandler MapRequestHandler = new SimpleHandler
            {
                Name = "RequestEvents.MapRequestHandler"
            };


        /// <summary>
        /// Occurs after ASP.NET has mapped the current request to the appropriate event handler.
        /// </summary>
        /// <seealso cref="HttpApplication.PostMapRequestHandler"/>
        public static SimpleHandler PostMapRequestHandler = new SimpleHandler
            {
                Name = "RequestEvents.PostMapRequestHandler"
            };


        /// <summary>
        /// Occurs when ASP.NET acquires the current state (for example, session state) that is associated with the current request.
        /// </summary>
        /// <seealso cref="HttpApplication.AcquireRequestState"/>
        public static SimpleHandler AcquireRequestState = new SimpleHandler
            {
                Name = "RequestEvents.AcquireRequestState"
            };


        /// <summary>
        /// Occurs after the request state (for example, session state) that is associated with the current request has been obtained.
        /// </summary>
        /// <seealso cref="HttpApplication.PostAcquireRequestState"/>
        public static SimpleHandler PostAcquireRequestState = new SimpleHandler
        {
            Name = "RequestEvents.PostAcquireRequestState"
        };


        /// <summary>
        /// Occurs as the last event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// </summary>
        /// <seealso cref="HttpApplication.EndRequest"/>
        public static SimpleHandler End = new SimpleHandler
            {
                Name = "RequestEvents.End"
            };


        /// <summary>
        /// Occurs after the <see cref="End"/> event has finished.
        /// </summary>
        /// <remarks>
        /// Response time can be affected by long running operations.
        /// </remarks>
        public static SimpleHandler RunEndRequestTasks = new SimpleHandler
            {
                Name = "RequestEvents.RunEndRequestTasks"
            };


        /// <summary>
        /// Occurs as the last event in the pipeline and can be used to clean up and release resources used by the request.
        /// </summary>
        /// <remarks>
        /// Response time can be affected by long running operations.
        /// </remarks>
        public static SimpleHandler Finalize = new SimpleHandler
            {
                Name = "RequestEvents.Finalize"
            };



        #region "Constructor"

        /// <summary>
        /// The <see cref="RequestEvents"/> class is not intended to be used as instance type. 
        /// Static modifier is not used due to possibility of using extension methods.
        /// </summary>
        private RequestEvents()
        {
        }

        #endregion
    }
}