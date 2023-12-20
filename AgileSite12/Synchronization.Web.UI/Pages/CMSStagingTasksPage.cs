using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using System.Threading;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Synchronization.Web.UI
{
    /// <summary>
    /// Base page for staging tasks management
    /// </summary>
    public abstract class CMSStagingTasksPage : CMSStagingPage
    {
        #region "Variables"

        /// <summary>
        /// Message storage for async control
        /// </summary>
        private static readonly Hashtable mInfos = new Hashtable();
        private StagingTaskRunner mStagingTaskRunner;

        #endregion


        #region "Properties"

        /// <summary>
        /// Selected server ID to synchronize
        /// </summary>
        protected int SelectedServerID
        {
            get;
            set;
        }


        /// <summary>
        /// Event log event code
        /// </summary>
        protected string EventCode
        {
            get;
            set;
        }


        /// <summary>
        /// Event log event type
        /// </summary>
        protected string EventType
        {
            get;
            set;
        }


        /// <summary>
        /// Currently synchronized site ID
        /// </summary>
        protected int CurrentSiteID
        {
            get;
            set;
        }
        

        /// <summary>
        /// Event code suffix for task event names
        /// </summary>
        protected abstract string EventCodeSuffix
        {
            get;
        }


        /// <summary>
        /// Grid with the task listing
        /// </summary>
        protected abstract UniGrid GridTasks
        {
            get;
        }


        /// <summary>
        /// Async control
        /// </summary>
        protected abstract AsyncControl AsyncControl
        {
            get;
        }


        /// <summary>
        /// Staging task runner instance
        /// </summary>
        protected StagingTaskRunner StagingTaskRunner
        {
            get
            {
                return mStagingTaskRunner ?? (mStagingTaskRunner = new StagingTaskRunner(SelectedServerID, CurrentSiteID, AddLog));
            }
        }


        /// <summary>
        /// Current Error.
        /// </summary>
        public string CurrentError
        {
            get
            {
                return ValidationHelper.GetString(mInfos["SyncError_" + AsyncControl.ProcessGUID], string.Empty);
            }
            set
            {
                mInfos["SyncError_" + AsyncControl.ProcessGUID] = value;
            }
        }


        /// <summary>
        /// Current Info.
        /// </summary>
        public string CurrentInfo
        {
            get
            {
                return ValidationHelper.GetString(mInfos["SyncInfo_" + AsyncControl.ProcessGUID], string.Empty);
            }
            set
            {
                mInfos["SyncInfo_" + AsyncControl.ProcessGUID] = value;
            }
        }


        /// <summary>
        /// Gets or sets the cancel string.
        /// </summary>
        public string CanceledString
        {
            get
            {
                return ValidationHelper.GetString(mInfos["SyncCancel_" + AsyncControl.ProcessGUID], string.Empty);
            }
            set
            {
                mInfos["SyncCancel_" + AsyncControl.ProcessGUID] = value;
            }
        }


        /// <summary>
        /// Staging task types to be shown. Should be set with semicolon.
        /// </summary>
        public string TaskTypeCategories
        {
            get;
            set;
        }


        /// <summary>
        /// Decides whether to show task group selector.
        /// </summary>
        public bool TaskGroupSelectorEnabled
        {
            get;
            set;
        }

        #endregion


        #region "Life-cycle methods"

        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AttachEvents();

            CurrentSiteID = SiteContext.CurrentSiteID;
            TaskGroupSelectorEnabled = true;
            GridTasks.FilterLimit = 1;
        }


        private void AttachEvents()
        {
            var ctl = AsyncControl;

            ctl.OnFinished += ctlAsyncLog_OnFinished;
            ctl.OnError += ctlAsyncLog_OnError;
            ctl.OnCancel += ctlAsyncLog_OnCancel;

            GridTasks.OnExternalDataBound += OnExternalDataBound;
            GridTasks.OnAction += gridTasks_OnAction;
            GridTasks.OnFilterFieldCreated += GridTasks_OnFilterFieldCreated;
        }


        /// <summary>
        /// Handles the grid external data bound
        /// </summary>
        protected virtual object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName.ToLowerCSafe())
            {
                case "taskresult":
                    return GetTaskResultLink(parameter);
                    

                case "view":
                    return GetTaskViewAction(sender);

                case "tasktype":
                    return GetString(TaskHelper.GetTaskTypeEnum(parameter.ToString()).ToLocalizedString<TaskTypeEnum>(TaskHelper.TASK_TYPE_RESOURCE_STRING_PREFIX));
            }

            return parameter;
        }


        private static object GetTaskViewAction(object sender)
        {
            var btnView = sender as CMSGridActionButton;
            if (btnView == null)
            {
                return String.Empty;
            }

            // Add view JavaScript
            var drv = UniGridFunctions.GetDataRowView((DataControlFieldCell)btnView.Parent);
            var taskId = ValidationHelper.GetInteger(drv["TaskID"], 0);
            string url = ScriptHelper.ResolveUrl(String.Format("~/CMSModules/Staging/Tools/View.aspx?taskid={0}&tasktype=All&hash={1}", taskId, QueryHelper.GetHash("?taskid=" + taskId + "&tasktype=All")));
            btnView.OnClientClick = "window.open('" + url + "');return false;";

            return btnView;
        }


        private object GetTaskResultLink(object parameter)
        {
            var drv = parameter as DataRowView;
            if (drv == null)
            {
                return parameter;
            }

            int failedCount = ValidationHelper.GetInteger(drv["FailedCount"], 0);
            var taskId = ValidationHelper.GetInteger(drv["TaskID"], 0);

            return GetResultLink(failedCount, taskId);
        }


        /// <summary>
        /// Returns the result link for the synchronization log.
        /// </summary>
        /// <param name="failedCount">Failed items count</param>
        /// <param name="taskId">Task ID</param>
        protected string GetResultLink(object failedCount, object taskId)
        {
            int count = ValidationHelper.GetInteger(failedCount, 0);
            if (count > 0)
            {
                string logUrl = ResolveUrl(String.Format("~/CMSModules/Staging/Tools/log.aspx?taskid={0}&serverId={1}&tasktype=All", taskId, SelectedServerID));
                logUrl = URLHelper.AddParameterToUrl(logUrl, "hash", QueryHelper.GetHash(logUrl));
                return String.Format("<a target=\"_blank\" href=\"{0}\" onclick=\"modalDialog('{0}', 'tasklog', 700, 500); return false;\">{1}</a>", logUrl, GetString("Tasks.ResultFailed"));
            }

            return string.Empty;
        }


        private void GridTasks_OnFilterFieldCreated(string columnName, UniGridFilterField filterDefinition)
        {
            var filter = (StagingTasksFilterBase)GridTasks.CustomFilter;
            filter.TaskTypeCategories = TaskTypeCategories;
            filter.TaskGroupSelectorEnabled = TaskGroupSelectorEnabled;
        }

        #endregion


        #region "Synchronize methods"

        private void gridTasks_OnAction(string actionName, object actionArgument)
        {
            // Parse action argument
            int taskId = ValidationHelper.GetInteger(actionArgument, 0);
            EventType = EventLog.EventType.INFORMATION;

            if (taskId <= 0)
            {
                return;
            }

            StagingTaskInfo task = StagingTaskInfoProvider.GetTaskInfo(taskId);
            if (task == null)
            {
                return;
            }

            switch (actionName.ToLowerCSafe())
            {
                case "delete":
                    DeleteTask("DELETESELECTED" + EventCodeSuffix, task, taskId);
                    break;

                case "synchronize":
                    SynchronizeTask("SYNCSELECTED" + EventCodeSuffix, task);
                    break;
            }
        }


        private void DeleteTask(string deleteEventCode, StagingTaskInfo task, int taskId)
        {
            // Delete task
            EventCode = deleteEventCode;
            AddEventLog(String.Format(ResHelper.GetAPIString("deletion.running", "Deleting '{0}' task"), HTMLHelper.HTMLEncode(task.TaskTitle)));

            SynchronizationInfoProvider.DeleteSynchronizationInfo(taskId, SelectedServerID, CurrentSiteID);
        }


        private void SynchronizeTask(string taskEventCode, StagingTaskInfo task)
        {
            string result;
            try
            {
                // Run task synchronization
                EventCode = taskEventCode;
                result = StagingTaskRunner.RunSynchronization(task.TaskID);

                if (string.IsNullOrEmpty(result))
                {
                    ShowConfirmation(GetString("Tasks.SynchronizationOK"));
                }
                else
                {
                    ShowError(GetString("Tasks.SynchronizationFailed"));
                    EventType = EventLog.EventType.ERROR;
                }
            }
            catch (Exception ex)
            {
                result = EventLogProvider.GetExceptionLogMessage(ex);
                ShowError(GetString("Tasks.SynchronizationFailed"));
                EventType = EventLog.EventType.ERROR;
            }

            // Log message
            AddEventLog(result + String.Format(ResHelper.GetAPIString("synchronization.running", "Processing '{0}' task"), HTMLHelper.HTMLEncode(task.TaskTitle)));
        }

        #endregion


        #region "Async processing"

        /// <summary>
        /// Runs the asynchronous action
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="evCode">Event code</param>
        /// <param name="action">Action to run</param>
        protected void RunAction(string actionName, string evCode, Func<string> action)
        {
            string result = null;

            EventCode = evCode;
            CanceledString = GetString("Tasks." + actionName + "Canceled");

            try
            {
                result = action();

                // Log possible errors
                if (!string.IsNullOrEmpty(result))
                {
                    CurrentError = GetString("Tasks." + actionName + "Failed");
                    AddErrorLog(CurrentError, null);
                }
                else
                {
                    CurrentInfo = GetString("Tasks." + actionName + "OK");
                    AddLog(CurrentInfo);
                }
            }
            catch (ThreadAbortException ex)
            {
                if (CMSThread.Stopped(ex))
                {
                    // Canceled by user
                    CurrentInfo = CanceledString;
                    AddLog(CurrentInfo);
                }
                else
                {
                    CurrentError = GetString("Tasks." + actionName + "Failed");
                    AddErrorLog(CurrentError, result);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Staging", EventCode, ex);

                CurrentError = GetString("Tasks." + actionName + "Failed") + ": " + ex.Message;
                AddErrorLog(CurrentError);
            }
        }


        private void ctlAsyncLog_OnError(object sender, EventArgs e)
        {
            // Handle error
            GridTasks.ResetSelection();

            if (!String.IsNullOrEmpty(CurrentError))
            {
                ShowError(CurrentError);
            }

            if (!String.IsNullOrEmpty(CurrentInfo))
            {
                ShowConfirmation(CurrentInfo);
            }
        }


        private void ctlAsyncLog_OnFinished(object sender, EventArgs e)
        {
            GridTasks.ResetSelection();

            if (!String.IsNullOrEmpty(CurrentError))
            {
                ShowError(CurrentError);
            }

            if (!String.IsNullOrEmpty(CurrentInfo))
            {
                ShowConfirmation(CurrentInfo);
            }
        }


        private void ctlAsyncLog_OnCancel(object sender, EventArgs e)
        {
            CurrentInfo = CanceledString;
            GridTasks.ResetSelection();

            if (!String.IsNullOrEmpty(CurrentError))
            {
                ShowError(CurrentError);
            }

            if (!String.IsNullOrEmpty(CurrentInfo))
            {
                ShowConfirmation(CurrentInfo);
            }
        }


        /// <summary>
        /// Executes given action asynchronously
        /// </summary>
        /// <param name="action">Action to run</param>
        protected virtual void RunAsync(AsyncAction action)
        {
            CurrentError = string.Empty;
            CurrentInfo = string.Empty;
            EventType = EventLog.EventType.INFORMATION;

            AsyncControl.RunAsync(action, WindowsIdentity.GetCurrent());
        }


        /// <summary>
        /// Deletes the tasks
        /// </summary>
        /// <param name="list">List of task IDs</param>
        protected string DeleteTasks(IEnumerable<string> list)
        {
            AddLog(GetString("Synchronization.DeletingTasks"));

            foreach (string taskIdString in list)
            {
                int taskId = ValidationHelper.GetInteger(taskIdString, 0);
                if (taskId <= 0)
                {
                    continue;
                }

                StagingTaskInfo task = StagingTaskInfoProvider.GetTaskInfo(taskId);
                if (task == null)
                {
                    continue;
                }

                AddLog(string.Format(ResHelper.GetAPIString("deletion.running", "Deleting '{0}' task"), HTMLHelper.HTMLEncode(task.TaskTitle)));

                SynchronizationInfoProvider.DeleteSynchronizationInfo(task.TaskID, SelectedServerID, CurrentSiteID);
            }

            return null;
        }


        /// <summary>
        /// Deletes the tasks
        /// </summary>
        /// <param name="ds">DataSet with task IDs</param>
        protected void DeleteTasks(DataSet ds)
        {
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                int taskId = ValidationHelper.GetInteger(row["TaskID"], 0);
                if (taskId <= 0)
                {
                    continue;
                }

                string taskTitle = ValidationHelper.GetString(row["TaskTitle"], null);
                AddLog(String.Format(ResHelper.GetAPIString("deletion.running", "Deleting '{0}' task"), HTMLHelper.HTMLEncode(taskTitle)));

                // Delete synchronization
                SynchronizationInfoProvider.DeleteSynchronizationInfo(taskId, SelectedServerID, CurrentSiteID);
            }
        }

        #endregion


        #region "Log handling"

        /// <summary>
        /// Adds the log information.
        /// </summary>
        /// <param name="newLog">New log information</param>
        protected void AddLog(string newLog)
        {
            AddEventLog(newLog);
            AsyncControl.AddLog(newLog);
        }


        /// <summary>
        /// Adds the log error.
        /// </summary>
        /// <param name="newLog">New log information</param>
        protected void AddErrorLog(string newLog)
        {
            AddErrorLog(newLog, null);
        }


        /// <summary>
        /// Adds the log error.
        /// </summary>
        /// <param name="newLog">New log information</param>
        /// <param name="errorMessage">Error message</param>
        protected void AddErrorLog(string newLog, string errorMessage)
        {
            LogContext.AppendLine(newLog, StagingTaskInfoProvider.LOGCONTEXT_SYNCHRONIZATION);

            string logMessage = newLog;
            if (errorMessage != null)
            {
                logMessage = errorMessage + "<br />" + logMessage;
            }

            EventType = EventLog.EventType.ERROR;

            AddEventLog(logMessage);
        }


        /// <summary>
        /// Adds message to event log object and updates event type.
        /// </summary>
        /// <param name="logMessage">Message to log</param>
        protected void AddEventLog(string logMessage)
        {
            var currentUser = CurrentUser;

            // Log event to event log
            LogContext.LogEventToCurrent(EventType, "Staging", EventCode, logMessage,
                                RequestContext.RawURL, currentUser.UserID, currentUser.UserName,
                                0, null, RequestContext.UserHostAddress, CurrentSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
        }

        #endregion
    }
}
