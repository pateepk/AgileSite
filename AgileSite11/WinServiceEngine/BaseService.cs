using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.WebFarmSync;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Base class for windows service.
    /// </summary>
    public class BaseService : ServiceBase
    {
        #region "Constants"

        /// <summary>
        /// Default sleep interval in seconds. Default value is 30 seconds.
        /// </summary>
        public const int DEFAULT_SLEEP_INTERVAL = 30;

        #endregion


        #region "Variables"

        private FileSystemWatcher mWatcher = null;
        private int mMaxInterval = int.MaxValue;
        private bool mRestart = false;
        private WinServiceItem mServiceDefinition = null;
        private string mEventLogSourceName = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// File watcher to indicate to clear the cache.
        /// </summary>
        public FileSystemWatcher Watcher
        {
            get
            {
                if (mWatcher == null)
                {
                    string filePath = null;
                    try
                    {
                        // Get file path
                        filePath = WinServiceHelper.GetServiceWatcherFilePath(ServiceName);
                        // Ensure directory path
                        EnsureDiskPath(filePath, WebApplicationPath);
                        // Ensure file
                        File.WriteAllText(filePath, DateTime.Now.ToString());

                        mWatcher = new FileSystemWatcher(WinServiceHelper.ServicesDataPath, WinServiceHelper.GetServiceWatcherFileName(ServiceName));
                        mWatcher.EnableRaisingEvents = true;
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        ServiceHelper.LogException(EventLogSourceName, ex, "Service watcher could not be initialized. Original message:", false);
                    }
                }

                return mWatcher;
            }
        }


        /// <summary>
        /// Web application path.
        /// </summary>
        public string WebApplicationPath
        {
            get;
            set;
        }


        /// <summary>
        /// Service base name.
        /// </summary>
        public string BaseName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that there is an service execution error.
        /// </summary>
        protected bool Error
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if service should be executed.
        /// </summary>
        protected virtual bool Enabled
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates that the service thread is running.
        /// </summary>
        protected bool Running
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that the service thread should be cancelled
        /// </summary>
        protected bool Cancel
        {
            get;
            set;
        }


        /// <summary>
        /// Service interval.
        /// </summary>
        protected virtual int Interval
        {
            get;
            private set;
        }


        /// <summary>
        /// Minimum service interval (in seconds). Default value is 0 second.
        /// </summary>
        protected virtual int MinInterval
        {
            get;
            private set;
        }


        /// <summary>
        /// Maximum service interval (in seconds). Default value is maximum value of type Int32 in seconds.
        /// </summary>
        protected virtual int MaxInterval
        {
            get
            {
                return mMaxInterval;
            }
            private set
            {
                mMaxInterval = value;
            }
        }


        /// <summary>
        /// Default sleep interval (in seconds).
        /// </summary>
        protected int SleepInterval
        {
            get
            {
                int interval = Interval;

                // Check max interval
                if (interval > MaxInterval)
                {
                    interval = MaxInterval;
                }

                // Check min interval
                if (interval < MinInterval)
                {
                    interval = MinInterval;
                }

                // Service stop -> set default sleep interval
                if (interval <= 0)
                {
                    interval = DEFAULT_SLEEP_INTERVAL;
                }

                return interval;
            }
        }


        /// <summary>
        /// Service thread.
        /// </summary>
        private Thread Thread
        {
            get;
            set;
        }


        /// <summary>
        /// Service definition.
        /// </summary>
        protected WinServiceItem ServiceDefinition
        {
            get
            {
                if (mServiceDefinition == null)
                {
                    mServiceDefinition = WinServiceHelper.GetServiceDefinition(BaseName);
                    if (mServiceDefinition == null)
                    {
                        // Missing service definition
                        throw new Exception(string.Format("Missing '{0}' service definition.", BaseName));
                    }
                }

                return mServiceDefinition;
            }
        }


        /// <summary>
        /// Event log source name for current service.
        /// </summary>
        protected string EventLogSourceName
        {
            get
            {
                if (mEventLogSourceName == null)
                {
                    mEventLogSourceName = ServiceDefinition.GetServiceName();
                }

                return mEventLogSourceName;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="webAppPath">Web application path</param>
        /// <param name="baseServiceName">Service base name.</param>
        public BaseService(string webAppPath, string baseServiceName)
            : base()
        {
            try
            {
                //Debugger.Launch();
                BaseName = baseServiceName;
                WebApplicationPath = webAppPath;

                // Check service settings
                CheckServiceSettings();

                // Set web application physical path
                SystemContext.WebApplicationPhysicalPath = webAppPath;

                // Use web application settings
                SystemContext.UseWebApplicationConfiguration = true;

                // Use web farm tasks for web application synchronization
                WebFarmContext.UseTasksForExternalApplication = true;

                // Set service name
                ServiceName = ServiceDefinition.GetServiceName();

                // Set default properties
                CanHandlePowerEvent = false;
                CanHandleSessionChangeEvent = false;
                CanPauseAndContinue = false;
                CanShutdown = false;
                CanStop = true;

                // Set event log
                EventLog.Source = EventLogSourceName;
                EventLog.Log = ServiceHelper.EVENT_LOG_NAME;

                // Create event source
                if (!EventLog.SourceExists(EventLogSourceName))
                {
                    EventLog.CreateEventSource(EventLogSourceName, ServiceHelper.EVENT_LOG_NAME);
                }

                // Init service watcher
                if (Watcher != null)
                {
                    Watcher.Changed += new FileSystemEventHandler(Watcher_Event);
                }
            }
            catch (Exception ex)
            {
                // Log exception to the windows event log
                ServiceHelper.LogException(EventLogSourceName, ex, "Service could not be started. Original message:", false);
                Error = true;
            }
        }

        #endregion


        #region "Service methods"

        /// <summary>
        /// Starts service.
        /// </summary>
        /// <param name="args">Start arguments</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                if (!Error)
                {
                    base.OnStart(args);

                    if (!Running)
                    {
                        Thread = new Thread(new ThreadStart(ExecuteInternal));
                        Thread.Start();
                    }
                }
                else
                {
                    Stop();
                }
            }
            catch (Exception ex)
            {
                // Log exception to the windows event log
                ServiceHelper.LogException(EventLogSourceName, ex, "Service could not be started. Original message:", false);
                Error = true;
            }
        }


        /// <summary>
        /// Stops service.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                // Stop the thread
                if (Thread != null)
                {
                    base.OnStop();

                    Cancel = true;
                    Running = false;

                    try
                    {
                        Thread.Abort(CMSThread.ABORT_REASON_STOP);
                    }
                    catch (ThreadAbortException abortEx)
                    {
                        // Throw exception only for another reason than STOP
                        if (!CMSThread.Stopped(abortEx))
                        {
                            throw;
                        }
                    }

                    Thread = null;
                }
            }
            catch (Exception ex)
            {
                // Log exception to the windows event log
                ServiceHelper.LogException(EventLogSourceName, ex, "Service could not be stopped. Original message:", false);
                Error = true;
            }
        }


        /// <summary>
        /// Method which is executed by the service.
        /// </summary>
        protected virtual void Execute()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Executes service logging.
        /// </summary>
        private void ExecuteInternal()
        {
            try
            {
                Running = true;
                Cancel = false;

                while (!Cancel)
                {
                    if (Enabled && (Interval > 0))
                    {
                        // Execute
                        Execute();
                    }

                    // Restart service
                    if (mRestart)
                    {
                        // Restart service
                        ServiceHelper.RestartService(ServiceName);
                        mRestart = false;
                        Cancel = true;
                    }
                    else
                    {
                        Thread.Sleep(SleepInterval * 1000);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                // Log exception
                ServiceHelper.LogException(EventLogSourceName, ex, "Error executing service. Original message:", false);
                Error = true;
            }
            finally
            {
                // Stop service
                Stop();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks service settings
        /// </summary>
        private void CheckServiceSettings()
        {
            // Check arguments
            if (string.IsNullOrEmpty(WebApplicationPath))
            {
                throw new Exception(string.Format("Service executable path doesn't have parameter '{0}'.", ServiceHelper.WEB_PATH_PREFIX));
            }

            // Check web application directory
            ServiceHelper.CheckWebApplicationPath(WebApplicationPath);
        }


        /// <summary>
        /// Handles changed event of file system watcher.
        /// </summary>
        /// <param name="sender">File system watcher</param>
        /// <param name="e">File system event argument</param>
        private void Watcher_Event(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Temporarily disable raising events because event OnChange is called twice when file is changed
                Watcher.EnableRaisingEvents = false;

                ServiceHelper.LogMessage(EventLogSourceName, "Service restart requested.");

                // Require service restart
                mRestart = true;
            }
            catch (Exception ex)
            {
                // Log exception
                ServiceHelper.LogException(EventLogSourceName, ex);
                Error = true;
            }
            finally
            {
                // Enable raising events
                Watcher.EnableRaisingEvents = true;
            }
        }


        /// <summary>
        /// Checks if all folders of the given path exist and if not, it creates them.
        /// </summary>
        /// <param name="path">Full disk path of the file including file name</param>
        /// <param name="startingPath">Starting path where folders can be checked and created</param>
        private void EnsureDiskPath(string path, string startingPath)
        {
            string folderPath = null;
            int folderIndex = 0;
            string currentPath = null;
            string[] pathArray = null;
            int startingIndex = 0;

            // Prepare the starting path
            if (startingPath == null)
            {
                startingPath = "";
            }
            if (startingPath.EndsWithCSafe("\\"))
            {
                startingPath = startingPath.Substring(0, startingPath.Length - 1);
            }
            // If path outside of the application folder, ignore the starting path
            if (!path.StartsWithCSafe(startingPath, true))
            {
                startingPath = "";
            }

            bool networkDirectory = path.StartsWithCSafe("\\\\");

            // Remove file name from the path
            folderPath = path.Substring(0, path.LastIndexOfCSafe(@"\"));
            pathArray = folderPath.Split('\\');
            currentPath = pathArray[0];

            // If starting path available, get starting index
            if ((startingPath != "") && folderPath.ToLowerCSafe().Trim().StartsWithCSafe(startingPath.ToLowerCSafe().Trim()))
            {
                startingIndex = startingPath.Split('\\').GetUpperBound(0);
            }

            for (folderIndex = 1; folderIndex <= pathArray.GetUpperBound(0); folderIndex++)
            {
                currentPath += @"\" + pathArray[folderIndex];
                if ((startingIndex < folderIndex) && (!networkDirectory || (folderIndex > 2)))
                {
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }
        }

        #endregion
    }
}