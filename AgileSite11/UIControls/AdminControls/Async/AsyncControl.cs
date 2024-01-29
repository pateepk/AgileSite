using System;
using System.Security.Principal;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Provides an asynchronous display of the background thread worker log.
    /// </summary>
    public class AsyncControl : CMSUserControl, ICallbackEventHandler, IPostBackEventHandler
    {
        #region "Private variables & constants"

        /// <summary>
        /// Table of the worker processes.
        /// </summary>
        private static readonly SafeDictionary<Guid, AsyncWorker> mWorkers = new SafeDictionary<Guid, AsyncWorker>();


        private AsyncWorker mWorker;

        private string mCallbackResult;

        private bool mPostbackOnError = true;
        private bool mPostbackOnFinish = true;

        private bool mReversedLog = true;

        private string mLog;
        private ILogContext mLogContext;
        private Panel mLogPanel;
        private Button mCancelButton;
        private bool mContinueOnAnyServer = true;
        private AsyncProcessData mProcessData;
        private LogContext mCurrentLog;

        // Constants
        private const string SEPARATOR = "|";

        private const string RESULT_FINISHED = "finished";
        private const string RESULT_RUNNING = "running";
        private const string RESULT_ERROR = "error";
        private const string RESULT_STOPPED = "stopped";
        private const string RESULT_THREADLOST = "threadlost";

        #endregion


        #region "Properties"

        /// <summary>
        /// Action performed if Cancel button is pressed. If not specified then Cancel stops the worker.
        /// </summary>
        public Action CancelAction
        {
            get;
            set;
        }


        /// <summary>
        /// Current log context.
        /// </summary>
        public LogContext CurrentLog
        {
            get
            {
                return EnsureLog();
            }
        }


        
        /// <summary>
        /// List of log context names separated by semicolon that should be received by this async control log from a general API call. 
        /// Use empty string for context which logs messages that do not provide context.
        /// Direct logging to context always logs the message without checking the context.
        /// </summary>
        public string LogContextNames
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the async process provides a log context 
        /// </summary>
        public bool ProvideLogContext
        {
            get;
            set;
        }


        /// <summary>
        /// Process messages
        /// </summary>
        public AsyncProcessData ProcessData
        {
            get
            {
                return mProcessData ?? (mProcessData = AsyncProcessData.GetDataForProcess(ProcessGUID));
            }
        }


        /// <summary>
        /// If true, the async process and it's subsequent callbacks can finish on other server that executed the action. 
        /// Disable with caution as there is no guarantee that non-sticky web farm will be switching between servers, and if it doesn't, the process continuation may hang.
        /// If possible, leverage ProcessData with serializable content (Data property) so that the process can continue on other server rather than using static variables to pass the data to the next request.
        /// </summary>
        public bool ContinueOnAnyServer
        {
            get
            {
                return mContinueOnAnyServer;
            }
            set
            {
                mContinueOnAnyServer = value;
            }
        }


        /// <summary>
        /// Offset of the given log in the entire log
        /// </summary>
        private int LogOffset
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current log.
        /// </summary>
        public string Log
        {
            get
            {
                EnsureLogData();

                return mLog;
            }
            set
            {
                mLog = value;
            }
        }


        /// <summary>
        /// Gets the current log.
        /// </summary>
        public ILogContext LogContext
        {
            get
            {
                EnsureLogData();

                return mLogContext;
            }
            set
            {
                mLogContext = value;

                mLog = value.Log;
                LogOffset = value.Offset;
            }
        }


        /// <summary>
        /// Process GUID.
        /// </summary>
        public Guid ProcessGUID
        {
            get
            {
                if (ViewState["ProcessGUID"] == null)
                {
                    ViewState["ProcessGUID"] = Guid.NewGuid();
                }

                return ValidationHelper.GetGuid(ViewState["ProcessGUID"], Guid.NewGuid());
            }
            set
            {
                ViewState["ProcessGUID"] = value;
            }
        }


        /// <summary>
        /// Asynchronous worker.
        /// </summary>
        public AsyncWorker Worker
        {
            get
            {
                EnsureWorker();

                return mWorker;
            }
        }


        /// <summary>
        /// True if the postback should occur after finished.
        /// </summary>
        public bool PostbackOnFinish
        {
            get
            {
                return mPostbackOnFinish;
            }
            set
            {
                mPostbackOnFinish = value;
            }
        }


        /// <summary>
        /// Name of the JS function called on client when the process finishes either successfully or with error
        /// </summary>
        public string FinishClientCallback
        {
            get;
            set;
        }


        /// <summary>
        /// True if the postback should occur after error.
        /// </summary>
        public bool PostbackOnError
        {
            get
            {
                return mPostbackOnError;
            }
            set
            {
                mPostbackOnError = value;
            }
        }


        /// <summary>
        /// Process parameter.
        /// </summary>
        public new string Parameter
        {
            get
            {
                return Worker.Parameter;
            }
            set
            {
                Worker.Parameter = value;
            }
        }


        /// <summary>
        /// Indicates if the logging is reversed. Default is true.
        /// </summary>
        public bool ReversedLog
        {
            get
            {
                return mReversedLog;
            }
            set
            {
                mReversedLog = value;
            }
        }


        /// <summary>
        /// Indicates if the control should use the string from resource file.
        /// </summary>
        public bool UseFileStrings
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the worker status.
        /// </summary>
        public AsyncWorkerStatusEnum Status
        {
            get
            {
                return Worker.Status;
            }
        }


        /// <summary>
        /// Maximum log length. (0 = unlimited)
        /// </summary>
        public int MaxLogLines
        {
            get;
            set;
        }


        /// <summary>
        /// If set, control tries to find already running thread to attach to on load
        /// </summary>
        public bool AttachToRunningThread
        {
            get;
            set;
        }


        /// <summary>
        /// Message panel where log is being displayed.
        /// </summary>
        public Panel LogPanel
        {
            get
            {
                EnsureChildControls();

                return mLogPanel;
            }
            protected set
            {
                mLogPanel = value;
            }
        }


        /// <summary>
        /// Cancel button
        /// </summary>
        public Button CancelButton
        {
            get
            {
                EnsureChildControls();

                return mCancelButton;
            }
            private set
            {
                mCancelButton = value;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// OnLoad event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            // Attach to worker running thread to handle events
            if (AttachToRunningThread)
            {
                if (!RequestHelper.IsCallback() && !RequestHelper.IsPostBack())
                {
                    if (WorkerExists())
                    {
                        var thread = Worker.Thread;
                        if (thread != null)
                        {
                            AttachToThread(thread);
                        }
                    }
                }
            }

            base.OnLoad(e);
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register full postbacks
            CancelButton.Attributes.Add("style", "display: none;");

            // Register full postbacks
            ControlsHelper.RegisterPostbackControl(this);
            ControlsHelper.RegisterPostbackControl(CancelButton);

            RenderScripts(false);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds the log information.
        /// </summary>
        /// <param name="newLog">New log information</param>
        /// <param name="newLine">If true, the ne log information is added on new line</param>
        public void AddLog(string newLog, bool newLine = true)
        {
            CurrentLog.AppendText(newLog, newLine);
        }


        /// <summary>
        /// Ensures the logging context.
        /// </summary>
        public LogContext EnsureLog()
        {
            if (mCurrentLog == null)
            {
                // Create the log
                var log = EventLog.LogContext.EnsureLog(ProcessGUID);

                // If configured, only receive logs from specific contexts
                if (LogContextNames != null)
                {
                    log.SetAllowedContexts(LogContextNames.Split(';'));
                }

                log.LogAlways = true;

                mCurrentLog = log;
            }

            return mCurrentLog;
        }

        
        /// <summary>
        /// Creates the child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Cancel button
            CancelButton = new CMSButton
            {
                ID = "btnCancel",
                ButtonStyle = ButtonStyle.Primary
            };

            CancelButton.Click += btnCancel_Click;

            Controls.Add(CancelButton);

            // Log panel
            if (mLogPanel == null)
            {
                LogPanel = new Panel
                {
                    ID = "pnlAsync",
                    CssClass = "AsyncPanel"
                };

                Controls.Add(LogPanel);
            }
        }


        /// <summary>
        /// Ensures the background worker for the async action or retrieves it from the storage
        /// </summary>
        private void EnsureWorker()
        {
            if (mWorker == null)
            {
                var processGuid = ProcessGUID;

                mWorker = mWorkers[processGuid];

                if (mWorker == null)
                {
                    mWorker = new AsyncWorker
                    {
                        // Ensure process guid
                        ProcessGUID = processGuid
                    };

                    mWorkers[processGuid] = mWorker;
                }
            }
        }


        /// <summary>
        /// Ensures that the log data is available
        /// </summary>
        private void EnsureLogData()
        {
            if (mLog == null)
            {
                // Provide it's own log if configured
                if (ProvideLogContext)
                {
                    LogContext = CurrentLog;
                }

                // Let external process provide the log
                if (OnRequestLog != null)
                {

                    OnRequestLog(this, new EventArgs());
                }
            }
        }


        /// <summary>
        /// Returns localized string.
        /// </summary>
        /// <param name="stringName">String to localize</param>
        /// <param name="culture">Culture</param>
        public override string GetString(string stringName, string culture = null)
        {
            if (UseFileStrings)
            {
                return ResHelper.GetFileString(stringName, culture);
            }

            return base.GetString(stringName, culture);
        }


        /// <summary>
        /// Returns true if the worker for current control exists.
        /// </summary>
        protected bool WorkerExists()
        {
            if (ProcessGUID != Guid.Empty)
            {
                return (mWorkers[ProcessGUID] != null);
            }

            return false;
        }


        /// <summary>
        /// Attaches to the given asynchronous process.
        /// </summary>
        public void AttachToThread(CMSThread thread)
        {
            RenderScripts(true);

            Worker.AttachToThread(thread);
        }


        /// <summary>
        /// Runs the asynchronous action.
        /// </summary>
        /// <param name="action">Action to run</param>
        /// <param name="wi">Windows identity (windows user)</param>
        public void RunAsync(AsyncAction action, WindowsIdentity wi)
        {
            RenderScripts(true);

            LogContext log = null;

            // Ensure the log context
            if (ProvideLogContext)
            {
                log = EnsureLog();
            }

            var worker = Worker;

            worker.Stop();
            worker.Reset();

            worker.RunAsync(p =>
            {
                if (log != null)
                {
                    // Ensure that created log is distributed to the async action context
                    EventLog.LogContext.Current = log;
                }

                action(p);
            }, wi);
        }


        /// <summary>
        /// Stops the worker.
        /// </summary>
        public void Stop()
        {
            Worker.Stop();
        }


        /// <summary>
        /// Registers scripts necessary.
        /// </summary>
        /// <param name="force">Determines whether to register scrips in any case</param>
        private void RenderScripts(bool force)
        {
            if (!RequestHelper.IsCallback() || force)
            {
                var status = Worker.Status;
                if (((status == AsyncWorkerStatusEnum.Running) || (status == AsyncWorkerStatusEnum.WaitForFinish)) || force)
                {
                    // Setup the module
                    object config = new
                    {
                        id = ClientID,
                        logId = LogPanel.ClientID,
                        uniqueId = UniqueID,
                        machineName = SystemContext.InstanceName.ToLowerCSafe(),
                        reversed = ReversedLog,
                        maxLogLines = MaxLogLines,
                        postbackOnError = PostbackOnError,
                        postbackOnFinish = PostbackOnFinish,
                        finishClientCallback = FinishClientCallback,
                        cancelButtonUniqueId = CancelButton.UniqueID,
                        closeText = GetString("General.Close")
                    };

                    // This must be called in order to make the WebFormCaller module methods available
                    ScriptHelper.EnsurePostbackMethods(this);

                    ScriptHelper.RegisterModule(this, "CMS/AsyncControl", config);
                }
            }
        }


        /// <summary>
        /// Gets the script which cancels the execution of the worker
        /// </summary>
        /// <param name="withPostback">If true, the cancel action should raise postback</param>
        public string GetCancelScript(bool withPostback)
        {
            return "window.CMS.AC_" + ClientID + ".cancel(" + withPostback.ToString().ToLowerCSafe() + ");";
        }


        /// <summary>
        /// Retries control post back request. Used when post back request lands on other server then the one the worker is running on.
        /// </summary>
        /// <param name="eventArgument"></param>
        private void RenderRefreshPostbackScript(string eventArgument)
        {
            var script = Page.ClientScript.GetPostBackEventReference(this, eventArgument) + ";";

            ScriptManager.RegisterStartupScript(this, typeof(string), "RefreshAsyncControlPostBack", script, true);
        }

        #endregion


        #region "Callback handling"

        /// <summary>
        /// Raises the callback event.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            mCallbackResult = string.Empty;

            var args = eventArgument.Split('|');

            // Return result only if the call is being made to the original instance that ran the async action
            if (IsOriginalInstance(args[1]))
            {
                if (WorkerExists())
                {
                    var status = Worker.Status;

                    mCallbackResult = GetResultCode(status);

                    // Provide log lines
                    string log = Log;

                    if (!String.IsNullOrEmpty(log))
                    {
                        int requestedLength = ValidationHelper.GetInteger(args[0], 0);

                        log = GetLogLines(log, requestedLength);

                        mCallbackResult += log;
                    }
                }
                else
                {
                    mCallbackResult = RESULT_THREADLOST + SEPARATOR + SEPARATOR + GetString("AsyncControl.ThreadLost");
                }
            }
            else
            {
                // Try to get status from persistent storage
                var data = ProcessData;

                data.UpdateStatusFromPersistentMedium();

                var status = data.Status;
                if (status != AsyncWorkerStatusEnum.Unknown)
                {
                    mCallbackResult = GetResultCode(status);
                }
                else
                {
                    // Unknown status - Probably another instance or error, treat as still running to not interrupt the operation
                    mCallbackResult = RESULT_RUNNING + SEPARATOR + SEPARATOR;
                }
            }
        }


        private string GetLogLines(string log, int requestedLength)
        {
            int logLength = LogOffset + log.Length;
            int trimStart;
            int trimLength;

            if (MaxLogLines > 0)
            {
                // Get position of the specified occurrence of new line tag
                int index = log.NthIndexOf(Environment.NewLine, MaxLogLines, false);
                bool indexExists = (index > -1);

                // Select max. number of lines from the end of the log
                trimStart = indexExists ? index : 0;
                trimLength = indexExists ? (logLength - index) : logLength;
            }
            else
            {
                // Correct the length in case of invalid value
                if (requestedLength > logLength)
                {
                    requestedLength = logLength;
                }

                // Send only the part that is not present on the client machine
                trimStart = requestedLength;
                trimLength = (logLength - requestedLength);
            }

            string output = null;

            // Adjust the values by the log offset
            if (LogOffset > 0)
            {
                trimStart -= LogOffset;
                if (trimStart < 0)
                {
                    trimStart = 0;

                    // Some of the required data is missing, add trailing dots
                    output += "...\r\n";
                }

                if (log.Length < trimStart + trimLength)
                {
                    trimLength = log.Length - trimStart;
                }
            }

            // Get the message within the specified bounds
            output += log.Substring(trimStart, trimLength);

            // Send the message to client
            log = SEPARATOR + logLength + SEPARATOR + output;

            return log;
        }


        /// <summary>
        /// Returns true if the given instance is an original instance on which the async process runs
        /// </summary>
        /// <param name="instanceName">Instance name</param>
        private static bool IsOriginalInstance(string instanceName)
        {
            return SystemContext.InstanceName.EqualsCSafe(instanceName, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Sets the result code to the
        /// </summary>
        /// <param name="status">Worker status</param>
        private string GetResultCode(AsyncWorkerStatusEnum status)
        {
            switch (status)
            {
                case AsyncWorkerStatusEnum.Finished:
                case AsyncWorkerStatusEnum.WaitForFinish:
                    // Allow worker to finish
                    return RESULT_FINISHED;

                case AsyncWorkerStatusEnum.Running:
                    return RESULT_RUNNING;

                case AsyncWorkerStatusEnum.Error:
                    // Allow worker to finish
                    return RESULT_ERROR;

                case AsyncWorkerStatusEnum.Stopped:
                    return RESULT_STOPPED;
            }

            return null;
        }


        /// <summary>
        /// Returns the result of a callback.
        /// </summary>
        public string GetCallbackResult()
        {
            return mCallbackResult;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Finished event handler.
        /// </summary>
        public event EventHandler OnFinished;


        /// <summary>
        /// Error event handler.
        /// </summary>
        public new event EventHandler OnError;


        /// <summary>
        /// Cancel event handler.
        /// </summary>
        public event EventHandler OnCancel;


        /// <summary>
        /// Request log event handler.
        /// </summary>
        public event EventHandler OnRequestLog;


        /// <summary>
        /// Raises the finished event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void RaiseFinished(object sender, EventArgs e)
        {
            if (OnFinished != null)
            {
                OnFinished(this, e);
            }

            CloseLogContext();
        }


        /// <summary>
        /// Raises the Error event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void RaiseError(object sender, EventArgs e)
        {
            if (OnError != null)
            {
                OnError(this, e);
            }

            CloseLogContext();
        }


        /// <summary>
        /// Raises the Cancel event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void RaiseCancel(object sender, EventArgs e)
        {
            if (OnCancel != null)
            {
                OnCancel(this, e);
            }

            CloseLogContext();
        }


        private void CloseLogContext()
        {
            if (ProvideLogContext)
            {
                CurrentLog.Close();
            }
        }


        /// <summary>
        /// Cancel button click event
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (WorkerExists())
            {
                if (CancelAction != null)
                {
                    CancelAction();
                }
                else if (Worker.Status == AsyncWorkerStatusEnum.Running)
                {
                    // Stop worker
                    Worker.Stop();
                }
            }

            RaiseCancel(this, e);
        }

        #endregion


        #region "IPostbackEventHandler"

        /// <summary>
        /// Processes the postback event
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            string[] args = eventArgument.Split('|');

            var originalInstance = IsOriginalInstance(args[1]);
            if (originalInstance || ContinueOnAnyServer)
            {
                // For other than original instance load the process data to keep it updated with the original instance state
                if (!originalInstance)
                {
                    ProcessData.UpdateFromPersistentMedium();
                }

                // Fire postback locally
                var e = new EventArgs();

                switch (args[0])
                {
                    case "finished":
                        RaiseFinished(this, e);
                        break;

                    case "error":
                        RaiseError(this, e);
                        break;
                }
            }
            else
            {
                // Retry the postback to hit another instance. 
                // There is a chance that next postbacks will be hitting the same wrong server, but this is at least the best effort to let the process continue.
                RenderRefreshPostbackScript(eventArgument);
            }
        }

        #endregion
    }
}
