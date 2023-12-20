namespace CMS.Base
{
    /// <summary>
    /// Holds events that allow performing of custom logic at specific points within the application's life cycle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The events are represented by fields of the <see cref="SimpleHandler"/> type. Handler methods need to be assigned to the Execute event of individual fields.
    /// </para>
    /// <para>
    /// Some of the ApplicationEvents are not raised out of web-based applications without the CMSApplication module (when using the Kentico API externally).
    /// </para>
    /// </remarks>
    public sealed class ApplicationEvents
    {
        #region "Life-cycle events"

        // NOTE - Events are placed in the order of their execution. Please maintain this order.

        /// <summary>
        /// Occurs after the application pre-initialization.
        /// </summary>
        /// <remarks>
        /// Fires only once during the application lifetime. Handlers need to be attached in the PreInit of modules.
        /// </remarks>
        public static SimpleHandler PreInitialized = new SimpleHandler
            {
                OneTime = true,
                Name = "ApplicationEvents.PreInitialized",
                Debug = false
            };


        /// <summary>
        /// Occurs when the application is ready to perform an upgrade routine.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is not intended to be used in custom code.
        /// </para>
        /// <para>
        /// This event fires on every application start.
        /// </para>
        /// </remarks>
        public static SimpleHandler UpdateData = new SimpleHandler
            {
                OneTime = true,
                Name = "ApplicationEvents.UpdateData",
                Debug = false
            };


        /// <summary>
        /// Occurs after the application finishes initialization.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this event occurs, the database connection is available and all modules are initialized.
        /// </para>
        /// <para>
        /// Fires only once during the application lifetime. Handlers need to be attached in the PreInit or Init of modules.
        /// </para>
        /// </remarks>
        public static SimpleHandler Initialized = new SimpleHandler
            {
                OneTime = true,
                Name = "ApplicationEvents.Initialized"
            };


        /// <summary>
        /// Occurs after the end of the first application request after successful initialization.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Fires only once during the application lifetime. Use to perform additional async tasks needed for application startup.
        /// </para>
        /// <para>
        /// Response time can be affected by long running operations.
        /// </para>
        /// <para>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </para>
        /// </remarks>
        public static SimpleHandler PostStart = new SimpleHandler
            {
                OneTime = true,
                Name = "ApplicationEvents.PostStart"
            };


        /// <summary>
        /// Occurs just before the application shuts down.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Fires only once during the application lifetime.
        /// </para>
        /// <para>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </para>
        /// <para>
        /// Do not use this event for performing critical actions. Code execution can be forcibly interrupted after a short period of time.
        /// </para>
        /// </remarks>
        public static SimpleHandler End = new SimpleHandler
            {
                OneTime = true,
                Name = "ApplicationEvents.End",
                Debug = false
            };


        /// <summary>
        /// Occurs just before the application shuts down, after the <see cref="End"/> application event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is not intended to be used in custom code. Use <see cref="End"/> instead.
        /// </para>
        /// <para>
        /// The purpose of the event is to call actions after custom code execution within the <see cref="End"/> event.
        /// </para>
        /// <para>
        /// Fires only once during the application lifetime.
        /// </para>
        /// <para>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </para>
        /// <para>
        /// Do not use this event for performing critical actions. Code execution can be forcibly interrupted after a short period of time.
        /// </para>        
        /// </remarks>
        public static SimpleHandler Finalize = new SimpleHandler
        {
            OneTime = true,
            Name = "ApplicationEvents.Finalize",
            Debug = false
        };


        #endregion


        #region "General events"

        /// <summary>
        /// Occurs when an unhandled exception is thrown.
        /// </summary>
        /// <remarks>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </remarks>
        public static SimpleHandler Error = new SimpleHandler
            {
                Name = "ApplicationEvents.Error",
                Debug = false
            };


        /// <summary>
        /// Occurs when a new HTTP session is created for a client.
        /// </summary>
        /// <remarks>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </remarks>
        public static SimpleHandler SessionStart = new SimpleHandler
            {
                Name = "ApplicationEvents.SessionStart"
            };


        /// <summary>
        /// Occurs when an HTTP session is destroyed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </para>
        /// <para>
        /// May not be raised if the application is configured to use a session state mode different than InProc.
        /// </para>
        /// </remarks>
        public static SimpleHandler SessionEnd = new SimpleHandler
            {
                Name = "ApplicationEvents.SessionEnd"
            };

#if NETFULLFRAMEWORK
        /// <summary>
        /// Occurs when the application evaluates variables in custom names of output cache keys.
        /// </summary>
        /// <remarks>
        /// This event only occurs for web-based applications. For example, does not occur when using the Kentico API in an external console application.
        /// </remarks>
        public static GetVaryByCustomStringHandler GetVaryByCustomString = new GetVaryByCustomStringHandler
            {
                Name = "ApplicationEvents.GetVaryByCustomString"
            };
#endif

        #endregion


        #region "Constructor"

        /// <summary>
        /// The <see cref="ApplicationEvents"/> class is not intended to be used as instance type. 
        /// Static modifier is not used due to possibility of using extension methods.
        /// </summary>
        private ApplicationEvents()
        {
        }

        #endregion
    }
}