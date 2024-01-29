using System;
using System.Threading;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for CMS Application
    /// </summary>
    public class CMSApplication
    {
        #region "Application start recovery constants"

        // Defines the interval in minutes after which the system tries to recover itself in case the initialization fails.
        private const int RECOVERY_INTERVAL = 5;

        // Defines the interval in seconds after which the system tries to re-init in case the initialization fails in early stages.
        private const int RETRY_INIT_INTERVAL = 5;

        #endregion


        #region "Variables"

        // Date and time when the application started
        private static DateTime mApplicationStart = DateTime.Now;

        // Date and time when the application start finished
        private static DateTime mApplicationStartFinished = DateTime.MinValue;

        // If true, the system executes first request after the system start
        private static bool mPostStartRaised;

        // Defines the state whether the application is initialized. If true, the application was initialized correctly. If false, the application was initialized with an error. If null, the initialization is to be done.
        private static readonly CMSStatic<bool?> mApplicationInitialized = new CMSStatic<bool?>();

        // If true, the application is pre-initialized
        private static bool mApplicationPreInitialized;

        // If true, the application end actions were already called
        private static bool mApplicationEnded;

        // Object for locking the context
        private static readonly object lockObject = new object();

        // Connection error message
        private static readonly CMSStatic<string> mApplicationErrorMessage = new CMSStatic<string>();


        /// <summary>
        /// If true, the process without the database available waits for the database instead of attempting to redirect to DB installation wizard. Use this for background applications such as services, that start automatically prior to the installation.
        /// </summary>
        public static readonly BoolAppSetting WaitForDatabaseAvailable = new BoolAppSetting("CMSWaitForDatabaseAvailable");

        /// <summary>
        /// If true, the application ensures the default schema for the user upon its initialization. Use this setting for Windows authentication on DB server with automatic default schema.
        /// </summary>
        public static BoolAppSetting EnsureDefaultDBSchema = new BoolAppSetting("CMSEnsureDefaultDBSchema");

        #endregion


        #region "Properties"

        /// <summary>
        /// Date and time of the application start.
        /// </summary>
        public static DateTime ApplicationStart
        {
            get
            {
                return mApplicationStart;
            }
        }


        /// <summary>
        /// Date and time when the application start (initialization) finished its execution.
        /// </summary>
        public static DateTime ApplicationStartFinished
        {
            get
            {
                return mApplicationStartFinished;
            }
        }


        /// <summary>
        /// Defines the state whether the application is initialized. If true, the application was initialized correctly. If false, the application was initialized with an error. If null, the initialization is to be done.
        /// </summary>
        public static bool? ApplicationInitialized
        {
            get
            {
                return mApplicationInitialized.Value;
            }
            internal set
            {
                // Set the time when recovery should be performed
                if (value.HasValue && (value.Value == false) && SystemHelper.RestartApplicationIfInitFails)
                {
                    AllowRecovery();
                }

                mApplicationInitialized.Value = value;
            }
        }


        /// <summary>
        /// Connection error message.
        /// </summary>
        public static string ApplicationErrorMessage
        {
            get
            {
                return mApplicationErrorMessage;
            }
            set
            {
                mApplicationErrorMessage.Value = value;
            }
        }

        #endregion


        #region "Application start recovery properties"

        /// <summary>
        /// The time when the application should try to recover itself from initialization failure.
        /// </summary>
        private static DateTime? RecoveryTime
        {
            get;
            set;
        }


        /// <summary>
        /// The time when the application attempts to retry the initialization to recover itself from initialization failure in early stages.
        /// </summary>
        private static DateTime? RetryInitTime
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor - Initialization code
        /// </summary>
        static CMSApplication()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }


        /// <summary>
        /// Runs the actions when the application exits
        /// </summary>
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            // End the application properly
            ApplicationEnd();
        }


        /// <summary>
        /// Performs the application pre-initialization.
        /// </summary>
        /// <returns>Returns true, if the application was pre-initialized during this call.</returns>
        public static bool PreInit()
        {
            // PreInitialize the application
            if (!mApplicationPreInitialized)
            {
                lock (lockObject)
                {
                    if (!mApplicationPreInitialized)
                    {
                        // Remember date and time of the application start
                        mApplicationStart = DateTime.Now;

                        // Pre-init core
                        AppCore.Setup(CreateSetup());
                        AppCore.PreInit();

                        ApplicationEvents.PreInitialized.StartEvent(new EventArgs());

                        mApplicationPreInitialized = true;

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Performs the application initialization on the first request.
        /// </summary>
        /// <returns>Returns true, if the application was initialized during this call.</returns>
        public static bool Init()
        {
            // Check the initialized status
            switch (ApplicationInitialized)
            {
                // If application was initialized properly, do not initialize
                case true:
                    return false;

                case null:
                    {
                        // Ensure PreInitialization if not run yet
                        PreInit();

                        // Install if necessary
                        if (InstallRedirect(false))
                        {
                            return false;
                        }
                    }
                    break;
            }

            // Initialize application in a locked context
            lock (lockObject)
            {
                // Check again to make sure the application wasn't initialized by another thread
                switch (ApplicationInitialized)
                {
                    // If application was initialized properly, do not initialize
                    case true:
                        return false;

                    // Report the application error in case of failed initialization
                    case false:
                        if (!RecoverFromFailedInitialization())
                        {
                            ReportApplicationInitError();
                            return false;
                        }
                        break;
                }

                using (var scope = new CMSConnectionScope { DisableDebug = true })
                {
                    // Use single open connection for the application start
                    bool opened = false;
                    bool install = false;
                    bool allowRetry = true;

                    try
                    {
                        scope.Open();
                        opened = true;

                        // Ensure default schema for the user
                        if (EnsureDefaultDBSchema)
                        {
                            SqlInstallationHelper.EnsureDefaultSchema(scope.Connection);
                        }

                        // Check if the database is available
                        if (!DatabaseHelper.IsDatabaseAvailable)
                        {
                            install = true;
                            return false;
                        }

                        // Check database version
                        if (!DatabaseHelper.CheckDatabaseVersion())
                        {
                            AllowRetryInit();
                            ReportApplicationInitError();
                            return false;
                        }

                        // There is no going back from this point, until this point, application can retry initialization without the full restart
                        allowRetry = false;

                        DebugHelper.SetApplicationStartDebug();

                        // Enable debug of connection after debugs are set up
                        scope.DisableDebug = false;

                        // Initialize the environment
                        AppCore.Init();

                        // Update the system data !! IMPORTANT - must be first
                        ApplicationEvents.UpdateData.StartEvent(new EventArgs());

                        // Check database separation
                        if (!DatabaseSeparationHelper.CheckDBSeparation())
                        {
                            ReportApplicationInitError();
                            return false;
                        }

                        ApplicationEvents.Initialized.StartEvent(new EventArgs());

                        if (SystemContext.IsWebSite)
                        {
                            // Attach the post start event to the end of the first request in a web site scenario
                            RequestEvents.RunEndRequestTasks.Execute += PostStart;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (allowRetry)
                        {
                            AllowRetryInit();
                        }

                        // Unexpected error during app initialization
                        ApplicationInitialized = false;

                        // Only log the error if not already logged to not hide an original error message
                        if (ApplicationErrorMessage == null)
                        {
                            ApplicationErrorMessage = ex.Message;
                        }

                        if (opened)
                        {
                            // Server connected successfully but something else went wrong
                            CoreServices.EventLog.LogException("Application", "INITIALIZATION", ex);

                            throw;
                        }
                        else
                        {
                            ReportApplicationInitError();
                        }
                    }
                    finally
                    {
                        // Redirect to installation if necessary
                        if (install)
                        {
                            InstallRedirect(true);
                        }
                    }
                }

                // Log when the overall application start finished its execution
                mApplicationStartFinished = DateTime.Now;
                ApplicationInitialized = true;

                RequestDebug.LogRequestOperation("AfterApplicationStart", null, 0);

                return true;
            }
        }


        /// <summary>
        /// Reinitializes the application state
        /// </summary>
        private static void ReInitAppState()
        {
            mApplicationStartFinished = DateTime.MinValue;

            mPostStartRaised = false;
            mApplicationEnded = false;

            mApplicationInitialized.Value = null;

            ApplicationErrorMessage = null;

            DatabaseHelper.Clear();
        }


        /// <summary>
        /// Runs the application post start event in case it wasn't run already
        /// </summary>
        private static void PostStart(object sender, EventArgs e)
        {
            PostStart();
        }


        /// <summary>
        /// Runs the application post start event in case it wasn't run already
        /// </summary>
        public static void PostStart()
        {
            LockHelper.ExecuteOnceInLifetime(RaisePostStart, lockObject, ref mPostStartRaised);
        }


        /// <summary>
        /// Raises the application post start event
        /// </summary>
        private static void RaisePostStart()
        {
            ApplicationEvents.PostStart.StartEvent(new EventArgs());
        }


        /// <summary>
        /// Raises the application error event
        /// </summary>
        public static void ApplicationError()
        {
            ApplicationEvents.Error.StartEvent(new EventArgs());
        }


        /// <summary>
        /// Raises the application end event to properly end the application
        /// </summary>
        public static void ApplicationEnd()
        {
            if ((ApplicationInitialized == true) && !mApplicationEnded)
            {
                mApplicationEnded = true;

                // Allow empty context - this thread does never originates from a request
                CMSThread.AllowEmptyContext();

                ApplicationEvents.End.StartEvent(new EventArgs());

                ApplicationEvents.Finalize.StartEvent(new EventArgs());
            }
        }


        /// <summary>
        /// Returns the  
        /// </summary>
        /// <param name="e">Event arguments</param>
        public static string GetVaryByCustomString(GetVaryByCustomStringEventArgs e)
        {
            // Run the event
            ApplicationEvents.GetVaryByCustomString.StartEvent(e);

            return e.Result;
        }


        /// <summary>
        /// Reports the application error
        /// </summary>
        /// <param name="message">Error message to report</param>
        private static void ReportApplicationInitError(string message = null)
        {
            ApplicationInitialized = false;

            // Set the error message
            if (message != null)
            {
                ApplicationErrorMessage = message;
            }

            // Redirect to error or throw exception
            if (SystemContext.IsCMSRunningAsMainApplication && (HttpContext.Current != null))
            {
                HttpContext.Current.Server.Transfer("~/CMSMessages/error.aspx");
            }
            else
            {
                throw new ApplicationInitException(ApplicationErrorMessage);
            }
        }


        /// <summary>
        /// Creates application startup parameters and returns it.
        /// </summary>
        /// <returns>Application startup parameters.</returns>
        private static AppCoreSetup CreateSetup()
        {
            var dependenciesFolderPath = Path.Combine(SystemContext.WebApplicationPhysicalPath, "CMSDependencies");
            var builder = new AppCoreSetup.Builder().WithDependenciesFolderPath(dependenciesFolderPath);

            return builder.Build();
        }

        #endregion


        #region "Application start recovery methods"

        /// <summary>
        /// When called, the application is allowed to recover by application restart
        /// </summary>
        private static void AllowRecovery()
        {
            if (RecoveryTime == null)
            {
                RecoveryTime = DateTime.Now.AddMinutes(RECOVERY_INTERVAL);
            }
        }


        /// <summary>
        /// When called, the application is allowed to retry the initialization
        /// </summary>
        private static void AllowRetryInit()
        {
            if (RetryInitTime == null)
            {
                RetryInitTime = DateTime.Now.AddSeconds(RETRY_INIT_INTERVAL);
            }
        }


        /// <summary>
        /// Attempts to perform the application recovery from a failed initialization
        /// </summary>
        private static bool RecoverFromFailedInitialization()
        {
            // Retry the whole initialization for early failures
            if ((RetryInitTime != null) && (DateTime.Now > RetryInitTime))
            {
                // Disable further attempts to init retry until the application tries to start again
                RetryInitTime = null;

                ReInitAppState();

                return true;
            }

            // Try to restart the application if enabled for problems during initialization itself
            if ((RecoveryTime != null) && (HttpContext.Current != null) && (DateTime.Now > RecoveryTime))
            {
                // Disable further attempts for recovery
                RecoveryTime = null;

                SystemHelper.RestartApplication(HttpContext.Current.Request.PhysicalApplicationPath);

                // Redirect to current page to re-init the application
                URLHelper.ResponseRedirect(RequestContext.URL.ToString(), true);
                
                return true;
            }

            return false;
        }

        #endregion


        #region "Installer methods"

        /// <summary>
        /// Redirects user to the installation page if connectionString not set.
        /// </summary>
        /// <param name="forceRedirect">If true, the redirect is forced</param>
        public static bool InstallRedirect(bool forceRedirect)
        {
            // Check if the connection string is initialized
            if (forceRedirect || !ConnectionHelper.IsConnectionStringInitialized)
            {
                // Wait for the database being available if necessary
                if (WaitForDatabaseAvailable)
                {
                    // Let only one thread do the checking
                    lock (lockObject)
                    {
                        Console.Write("Waiting for database to be ready ");

                        while (!DatabaseHelper.IsCorrectDatabaseVersion)
                        {
                            // Check whether the database is ready every second
                            Thread.Sleep(1000);

                            Console.Write(".");
                        }

                        Console.WriteLine();
                    }

                    return false;
                }

                bool install = true;

                // Check relative path to figure out whether to install or not
                var relativePath = RequestContext.CurrentRelativePath;
                if (relativePath != null)
                {
                    string currentFile = Path.GetFileName(relativePath);

                    // Do not redirect if already on install page
                    if (CMSString.Compare(currentFile, "install.aspx", true) == 0)
                    {
                        return true;
                    }

                    // Redirect in case of request to ASPX file
                    string fileExtension = Path.GetExtension(currentFile);

                    install =
                        (String.IsNullOrEmpty(currentFile) || (currentFile == "/") || (CMSString.Compare(fileExtension, ".aspx", true) == 0)) &&
                        !IsInstallerExcluded(relativePath);
                }

                if (install)
                {
                    // If the request is unable to redirect, report
                    if ((HttpContext.Current == null) || !SystemContext.IsCMSRunningAsMainApplication)
                    {
                        ReportApplicationInitError(
@"Cannot access the database specified by the 'CMSConnectionString' connection string. Please install the database externally and set a correct connection string.

Alternatively, you can use web.config key CMSWaitForDatabaseAvailable or set property CMSApplication.WaitForDatabaseAvailable prior to the API initialization to let the application intentionally wait until the database is ready.
");
                        return true;
                    }

                    // Redirect to install page
                    URLHelper.Redirect("~/cmsinstall/install.aspx");
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Returns true if the path is excluded for the installer process.
        /// </summary>
        /// <param name="path">Path to check</param>
        private static bool IsInstallerExcluded(string path)
        {
            if (path.StartsWithCSafe("/cmsmessages") || path.StartsWithCSafe("/cmspages/getresource.ashx"))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}