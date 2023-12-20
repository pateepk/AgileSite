namespace CMS.Base
{
    /// <summary>
    /// Holds events that allow performing of custom logic at specific points within the request processing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These request events allow you to handle most usable events from application without the need to create a custom HTTP module or edit global.asax.
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
        /// This event is related to the <see cref="RequestEvents"/> lifecycle only.
        /// </remarks>
        public static readonly SimpleHandler Prepare = new SimpleHandler
            {
                Name = "RequestEvents.Prepare", 
                Debug = false
            };


        /// <summary>
        /// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// </summary>
        public static readonly SimpleHandler Begin = new SimpleHandler
            {
                Name = "RequestEvents.Begin"
            };


        /// <summary>
        /// Occurs when a security module has established the identity of a user.
        /// </summary>
        public static readonly SimpleHandler Authenticate = new SimpleHandler
            {
                Name = "RequestEvents.Authenticate"
            };


        /// <summary>
        /// Occurs when a security module has established the identity of the user.
        /// </summary>
        public static readonly SimpleHandler PostAuthenticate = new SimpleHandler
        {
            Name = "RequestEvents.PostAuthenticate"
        };


        /// <summary>
        /// Occurs when a security module has verified user authorization.
        /// </summary>
        public static readonly SimpleHandler Authorize = new SimpleHandler
            {
                Name = "RequestEvents.Authorize"
            };


        /// <summary>
        /// Occurs when the user for the current request has been authorized.
        /// </summary>
        public static readonly SimpleHandler PostAuthorize = new SimpleHandler
            {
                Name = "RequestEvents.PostAuthorize"
            };


        /// <summary>
        /// Occurs when ASP.NET finishes an authorization event to let the caching modules serve requests from the cache, bypassing execution of the event handler (for example, a page or an XML Web service).
        /// </summary>
        public static readonly SimpleHandler ResolveRequestCache = new SimpleHandler
        {
            Name = "RequestEvents.ResolveRequestCache"
        };


        /// <summary>
        /// Occurs when ASP.NET bypasses execution of the current event handler and allows a caching module to serve a request from the cache.
        /// </summary>
        public static readonly SimpleHandler PostResolveRequestCache = new SimpleHandler
        {
            Name = "RequestEvents.PostResolveRequestCache"
        };


        /// <summary>
        /// Occurs when a handler is selected to respond to the request.
        /// </summary>
        public static readonly SimpleHandler MapRequestHandler = new SimpleHandler
            {
                Name = "RequestEvents.MapRequestHandler"
            };


        /// <summary>
        /// Occurs after ASP.NET has mapped the current request to the appropriate event handler.
        /// </summary>
        public static readonly SimpleHandler PostMapRequestHandler = new SimpleHandler
            {
                Name = "RequestEvents.PostMapRequestHandler"
            };


        /// <summary>
        /// Occurs when ASP.NET acquires the current state (for example, session state) that is associated with the current request.
        /// </summary>
        public static readonly SimpleHandler AcquireRequestState = new SimpleHandler
            {
                Name = "RequestEvents.AcquireRequestState"
            };


        /// <summary>
        /// Occurs after the request state (for example, session state) that is associated with the current request has been obtained.
        /// </summary>
        public static readonly SimpleHandler PostAcquireRequestState = new SimpleHandler
        {
            Name = "RequestEvents.PostAcquireRequestState"
        };


        /// <summary>
        /// Occurs just before ASP.NET starts executing an event handler.
        /// </summary>
        public static readonly SimpleHandler PreRequestHandlerExecute = new SimpleHandler
        {
            Name = "RequestEvents.PreRequestHandlerExecute"
        };


        /// <summary>
        /// Occurs when the ASP.NET event handler (for example, a page or an XML Web service) finishes execution.
        /// </summary>
        public static readonly SimpleHandler PostRequestHandlerExecute = new SimpleHandler
        {
            Name = "RequestEvents.PostRequestHandlerExecute"
        };


        /// <summary>
        /// Occurs after ASP.NET finishes executing all request event handlers. This event causes state modules to save the current state data
        /// </summary>
        public static readonly SimpleHandler ReleaseRequestState = new SimpleHandler
        {
            Name = "RequestEvents.ReleaseRequestState"
        };


        /// <summary>
        /// Occurs when ASP.NET has completed executing all request event handlers and the request state data has been stored.
        /// </summary>
        public static readonly SimpleHandler PostReleaseRequestState = new SimpleHandler
        {
            Name = "RequestEvents.PostReleaseRequestState"
        };


        /// <summary>
        /// Occurs when ASP.NET finishes executing an event handler in order to let caching modules store responses that will be used to serve subsequent requests from the cache.
        /// </summary>
        public static readonly SimpleHandler UpdateRequestCache = new SimpleHandler
        {
            Name = "RequestEvents.UpdateRequestCache"
        };


        /// <summary>
        /// Occurs when ASP.NET finishes updating caching modules and storing responses that are used to serve subsequent requests from the cache.
        /// </summary>
        public static readonly SimpleHandler PostUpdateRequestCache = new SimpleHandler
        {
            Name = "RequestEvents.PostUpdateRequestCache"
        };


        /// <summary>
        /// Occurs just before ASP.NET performs any logging for the current request.
        /// </summary>
        public static readonly SimpleHandler LogRequest = new SimpleHandler
        {
            Name = "RequestEvents.LogRequest"
        };


        /// <summary>
        /// Occurs when ASP.NET has completed processing all the event handlers for the <see cref="LogRequest"/> event.
        /// </summary>
        public static readonly SimpleHandler PostLogRequest = new SimpleHandler
        {
            Name = "RequestEvents.PostLogRequest"
        };


        /// <summary>
        /// Occurs as the last event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// </summary>
        public static readonly SimpleHandler End = new SimpleHandler
            {
                Name = "RequestEvents.End"
            };


        /// <summary>
        /// Occurs after the <see cref="End"/> event has finished.
        /// </summary>
        /// <remarks>
        /// Response time can be affected by long running operations.
        /// </remarks>
        public static readonly SimpleHandler RunEndRequestTasks = new SimpleHandler
            {
                Name = "RequestEvents.RunEndRequestTasks"
            };


        /// <summary>
        /// Occurs as the last event in the pipeline and can be used to clean up and release resources used by the request.
        /// </summary>
        /// <remarks>
        /// Response time can be affected by long running operations.
        /// </remarks>
        public static readonly SimpleHandler Finalize = new SimpleHandler
            {
                Name = "RequestEvents.Finalize"
            };


        #region "Unordered events"

        /// <summary>
        /// Occurs when the managed objects that are associated with the request have been released.
        /// </summary>
        public static readonly SimpleHandler RequestCompleted = new SimpleHandler
        {
            Name = "RequestEvents.RequestCompleted"
        };


        /// <summary>
        /// Occurs just before ASP.NET sends HTTP headers to the client.
        /// </summary>
        public static readonly SimpleHandler PreSendRequestHeaders = new SimpleHandler
        {
            Name = "RequestEvents.PreSendRequestHeaders"
        };


        /// <summary>
        /// Occurs just before ASP.NET sends content to the client.
        /// </summary>
        public static readonly SimpleHandler PreSendRequestContent = new SimpleHandler
        {
            Name = "RequestEvents.PreSendRequestContent"
        };

        #endregion


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