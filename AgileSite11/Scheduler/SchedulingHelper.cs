using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Modules;

namespace CMS.Scheduler
{
    /// <summary>
    /// Global class providing Scheduler methods.
    /// </summary>
    public static class SchedulingHelper
    {
        #region "Public constants"

        /// <summary>
        /// Second period.
        /// </summary>
        public const string PERIOD_SECOND = "timesecond";

        /// <summary>
        /// Minute period.
        /// </summary>
        public const string PERIOD_MINUTE = "minute";

        /// <summary>
        /// Hour period.
        /// </summary>
        public const string PERIOD_HOUR = "hour";

        /// <summary>
        /// Day period.
        /// </summary>
        public const string PERIOD_DAY = "day";

        /// <summary>
        /// Week period.
        /// </summary>
        public const string PERIOD_WEEK = "week";

        /// <summary>
        /// Month period.
        /// </summary>
        public const string PERIOD_MONTH = "month";

        /// <summary>
        /// Once period.
        /// </summary>
        public const string PERIOD_ONCE = "once";

        /// <summary>
        /// Year period.
        /// </summary>
        public const string PERIOD_YEAR = "year";

        /// <summary>
        /// First day of the month.
        /// </summary>
        public const string MONTHS_FIRST = "first";

        /// <summary>
        /// Second day of the month.
        /// </summary>
        public const string MONTHS_SECOND = "second";

        /// <summary>
        /// Third day of the month.
        /// </summary>
        public const string MONTHS_THIRD = "third";

        /// <summary>
        /// Fourth day of the month.
        /// </summary>
        public const string MONTHS_FOURTH = "fourth";

        /// <summary>
        /// Last day of the month.
        /// </summary>
        public const string MONTHS_LAST = "last";

        /// <summary>
        /// Sample scheduler page content which allows for automatic scheduler page availability check.
        /// </summary>
        internal const string SCHEDULER_PING_CONTENT = "SCHEDULER_PING_CONTENT_AVAILABILITY";

        #endregion


        #region "Variables"

        /// <summary>
        /// If true, automatic scheduler is to be used.
        /// </summary>
        private static bool? mUseAutomaticScheduler;

        /// <summary>
        /// If true, the scheduler runs within the request process (on request_end).
        /// </summary>
        private static bool? mRunSchedulerWithinRequest;

        /// <summary>
        /// If true, executed tasks are logged into file.
        /// </summary>
        private static bool? mLogTasks;

        /// <summary>
        /// Logs file path.
        /// </summary>
        public static string mLogFile;

        /// <summary>
        /// Direct URL of the scheduler page.
        /// </summary>
        private static string mSchedulerUrl;

        /// <summary>
        /// Scheduler user name.
        /// </summary>
        private static string mSchedulerUserName;

        /// <summary>
        /// Scheduler user password.
        /// </summary>
        private static string mSchedulerPassword;

        /// <summary>
        /// Scheduler user domain.
        /// </summary>
        private static string mSchedulerUserDomain = String.Empty;


        /// <summary>
        /// If true, running of scheduler is enabled.
        /// </summary>
        private static bool? mEnableScheduler;


        private static bool unavailableSchedulerPageLogged;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets the scheduler user name.
        /// </summary>
        private static string SchedulerUserName
        {
            get
            {
                if (mSchedulerUserName == null)
                {
                    // Get user name from application settings
                    string username = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSSchedulerUserName"], String.Empty);

                    // Check whether user name contains domain
                    int slashIndex = username.IndexOf("\\", StringComparison.Ordinal);
                    if (slashIndex > 0)
                    {
                        // Set domain name
                        mSchedulerUserDomain = username.Substring(0, slashIndex);
                        // Set user name
                        username = username.Substring(slashIndex + 1);
                    }

                    // Save user name into the global variable
                    mSchedulerUserName = username;
                }

                return mSchedulerUserName;
            }
        }


        /// <summary>
        /// Gets the scheduler user password.
        /// </summary>
        private static string SchedulerPassword
        {
            get
            {
                return mSchedulerPassword ?? (mSchedulerPassword = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSSchedulerPassword"], String.Empty));
            }
        }


        /// <summary>
        /// Gets the scheduler user domain.
        /// </summary>
        private static string SchedulerDomain
        {
            get
            {
                return mSchedulerUserDomain;
            }
        }


        /// <summary>
        /// Gets the direct URL of the scheduler page.
        /// </summary>
        private static string SchedulerUrl
        {
            get
            {
                return mSchedulerUrl ?? (mSchedulerUrl = DataHelper.GetNotEmpty(SettingsHelper.AppSettings[SCHEDULER_URL_SETTING_NAME], String.Empty));
            }
        }


        /// <summary>
        /// If true, the automatic version of scheduler is used (standalone thread and scheduler handler). The default settings is false (scheduler is executed by the activity of the requests).
        /// </summary>
        public static bool UseAutomaticScheduler
        {
            get
            {
                if (mUseAutomaticScheduler == null)
                {
                    mUseAutomaticScheduler = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseAutomaticScheduler"], false);
                }

                return mUseAutomaticScheduler.Value;
            }
            set
            {
                mUseAutomaticScheduler = value;
            }
        }


        /// <summary>
        /// If true, the scheduler tasks are executed within the standard requests (at the end of requests). The default settings is true (scheduler is executed within the request).
        /// </summary>
        public static bool RunSchedulerWithinRequest
        {
            get
            {
                if (mRunSchedulerWithinRequest == null)
                {
                    mRunSchedulerWithinRequest = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRunSchedulerWithinRequest"], true);
                }

                return mRunSchedulerWithinRequest.Value;
            }
            set
            {
                mRunSchedulerWithinRequest = value;
            }
        }


        /// <summary>
        /// If true, executed tasks are logged into file.
        /// </summary>
        public static bool LogTasks
        {
            get
            {
                if (mLogTasks == null)
                {
                    mLogTasks = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogScheduledTasks"], false);
                }

                return mLogTasks.Value;
            }
            set
            {
                mLogTasks = value;
            }
        }


        /// <summary>
        /// Logs file.
        /// </summary>
        public static string LogFile
        {
            get
            {
                if (mLogFile == null)
                {
                    mLogFile = SystemContext.WebApplicationPhysicalPath + "\\App_Data\\logtasks.log";

                    // Ensure the directory
                    DirectoryHelper.EnsureDiskPath(mLogFile, SystemContext.WebApplicationPhysicalPath);
                }
                return mLogFile;
            }
        }


        /// <summary>
        /// If true (default value), running of scheduler is enabled.
        /// </summary>
        public static bool EnableScheduler
        {
            get
            {
                if (mEnableScheduler == null)
                {
                    mEnableScheduler = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEnableScheduler"], true);
                }

                return mEnableScheduler.Value && CoreServices.Settings["CMSSchedulerTasksEnabled"].ToBoolean(false);
            }
            set
            {
                mEnableScheduler = value;
            }
        }


        /// <summary>
        /// Indicates if the windows service should be used.
        /// </summary>
        public static bool UseExternalService
        {
            get
            {
                return SystemHelper.WinServicesForceUsage || CoreServices.Settings["CMSSchedulerUseExternalService"].ToBoolean(false);
            }
        }


        /// <summary>
        /// Gets windows service interval (in seconds).
        /// </summary>
        public static int ServiceInterval
        {
            get
            {
                return CoreServices.Settings["CMSSchedulerServiceInterval"].ToInteger(0);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Clears the enabled status of the scheduler so it can reload
        /// </summary>
        public static void Clear()
        {
            mEnableScheduler = null;
        }


        /// <summary>
        /// Returns <c>true</c> if externally handled task did not execute for more than certain amount of times.
        /// </summary>
        /// <param name="taskInfo">Information about the task</param>
        /// <param name="timesToBeTooLate">How many scheduler intervals must not the task run before it is considered too long</param>
        /// <param name="currentTime">Date and time to resolve against, if null DateTime.Now is used</param>
        public static bool IsExternalTaskTooLate(TaskInfo taskInfo, int timesToBeTooLate, DateTime? currentTime = null)
        {
            if (!IsLateEligibleExternalTask(taskInfo))
            {
                return false;
            }

            if (!currentTime.HasValue)
            {
                currentTime = DateTime.Now;
            }

            return taskInfo.TaskNextRunTime.AddSeconds(ServiceInterval * timesToBeTooLate) < currentTime.Value;
        }


        /// <summary>
        /// Returns date and time when the very first scheduling moment occurs based on the current date and time and the scheduling pattern.
        /// </summary>
        /// <remarks>
        /// This method is intended to handle task planning before its first execution (at the time of its creation or edition).
        /// Use <see cref="GetNextTime(CMS.Scheduler.TaskInterval,System.Nullable{System.DateTime},System.Nullable{System.DateTime})"/> whenever there is a chance the task was run already.
        /// </remarks>
        /// <param name="taskInterval">Information about task repeating</param>
        /// <param name="currentTime">Date and time to resolve against, if null DateTime.Now is used</param>
        /// <exception cref="ArgumentNullException">When task interval is null</exception>
        public static DateTime GetFirstRunTime(TaskInterval taskInterval, DateTime? currentTime = null)
        {
            var nextRunTime = GetNextTime(taskInterval, null, currentTime);
            var isNoTimeWithPriodOnce = (nextRunTime == TaskInfoProvider.NO_TIME) && taskInterval.Period.Equals(PERIOD_ONCE, StringComparison.Ordinal);

            return isNoTimeWithPriodOnce
                ? taskInterval.StartTime
                : nextRunTime;
        }


        /// <summary>
        /// Returns date and time when the next scheduling moment occurs based on the current date and time and the scheduling pattern.
        /// </summary>
        /// <remarks>Use <see cref="GetFirstRunTime"/> to obtain correct result when a task was not run yet (at the time of its creation or edition).</remarks>
        /// <param name="taskInterval">Information about task repeating</param>
        /// <param name="lastPlannedRunTime">Last known planned run time of task, if null taskInterval.StartTime is used</param>
        /// <param name="currentTime">Date and time to resolve against, if null DateTime.Now is used</param>
        /// <exception cref="ArgumentNullException">When task interval is null</exception>
        public static DateTime GetNextTime(TaskInterval taskInterval, DateTime? lastPlannedRunTime = null, DateTime? currentTime = null)
        {
            // Check an unify parameters (get true default values, ...)
            if (taskInterval == null)
            {
                throw new ArgumentNullException(nameof(taskInterval));
            }

            if (!currentTime.HasValue || currentTime.Value == TaskInfoProvider.NO_TIME)
            {
                currentTime = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
            }

            if (taskInterval.StartTime == TaskInfoProvider.NO_TIME)
            {
                taskInterval.StartTime = currentTime.Value;
            }

            if (lastPlannedRunTime == TaskInfoProvider.NO_TIME)
            {
                lastPlannedRunTime = null;
            }

            // Handle elemental special cases (without base time computation required)
            if (taskInterval.Period.Equals(PERIOD_ONCE, StringComparison.Ordinal))
            {
                // Handle period "once" as it has very simple and unique rules
                return (taskInterval.StartTime <= currentTime.Value)
                    ? TaskInfoProvider.NO_TIME
                    : taskInterval.StartTime;
            }

            var isSpecialDayInWeekCase = taskInterval.IsSpecialDayInWeekCase();
            if (taskInterval.Every < 0 || (taskInterval.Every == 0 && !isSpecialDayInWeekCase))
            {
                // Handle incorrect value of Every property (quantity of period units between repetitions)
                return TaskInfoProvider.NO_TIME;
            }

            if (lastPlannedRunTime.HasValue && lastPlannedRunTime > currentTime.Value)
            {
                // Once the lastPlannedRunTime is in future, presumes it is correct and uses it
                return (taskInterval.StartTime > lastPlannedRunTime.Value)
                    ? taskInterval.StartTime
                    : lastPlannedRunTime.Value;
            }

            // Identify what base time compute with (start time or last planned, what is correct and later)
            DateTime baseTime = !lastPlannedRunTime.HasValue || (lastPlannedRunTime < taskInterval.StartTime) || !lastPlannedRunTime.Value.IsValidSuccessor(taskInterval)
                ? taskInterval.StartTime
                : lastPlannedRunTime.Value;

            // Handle all other special cases (with base time computation required)
            if (currentTime.Value < taskInterval.StartTime)
            {
                // Once start time is in future, only shift into to allowed day of week and time of day and use it
                // (for tasks that will be run for the first time in a future and have no planned time yet)
                return taskInterval.StartTime.ShiftIntoAllowedDayAndTime(taskInterval, baseTime);
            }

            if (isSpecialDayInWeekCase)
            {
                return currentTime.Value.MoveToSpecificDayOfWeekInFuture(taskInterval, baseTime);
            }

            if (taskInterval.IsSpecialDayOrWeekOfMonthCase())
            {
                return currentTime.Value.MoveToSpecificDayOrWeekOfMonthInFuture(taskInterval, baseTime);
            }

            if (taskInterval.IsSpecialMonthAndDayOfYearCase())
            {
                return currentTime.Value.MoveToSpecificMonthAndDayOfYearInFuture(taskInterval, baseTime);
            }

            // Handle all generic (remaining) cases for all periods
            var nextRunTime = currentTime.Value.MoveToNearestPeriodInFuture(taskInterval, baseTime);

            // Shift time to allowed day and time (if specified in taskInterval)
            var shiftedNextRunTime = nextRunTime.ShiftIntoAllowedDayAndTime(taskInterval, baseTime);
            return shiftedNextRunTime == nextRunTime
                ? nextRunTime
                : shiftedNextRunTime.AddMilliseconds(-1).MoveToNearestPeriodInFuture(taskInterval, nextRunTime);
        }


        /// <summary>
        /// Encodes interval for save it to database.
        /// </summary>
        /// <param name="infoObj">Task to encode</param>
        /// <returns>Encoded string</returns>
        public static string EncodeInterval(TaskInterval infoObj)
        {
            if (infoObj == null)
            {
                return String.Empty;
            }

            string result = infoObj.Period + ";";
            result += infoObj.StartTime.ToString(CultureHelper.EnglishCulture.DateTimeFormat);
            if (infoObj.UseSpecificTime)
            {
                result += "|1";
            }
            result += ";";

            switch (infoObj.Period.ToLowerInvariant())
            {
                case PERIOD_SECOND:
                case PERIOD_MINUTE:
                case PERIOD_HOUR:
                    result += infoObj.Every + ";";
                    result += infoObj.BetweenStart.TimeOfDay + ";";
                    result += infoObj.BetweenEnd.TimeOfDay + ";";
                    result += String.Join(",", infoObj.Days.Select(day => day.ToString()));
                    break;

                case PERIOD_DAY:
                    result += infoObj.Every + ";";
                    result += String.Join(",", infoObj.Days.Select(day => day.ToString()));
                    break;

                case PERIOD_WEEK:
                    result += infoObj.Every + ";";
                    if (!String.IsNullOrEmpty(infoObj.Day))
                    {
                        result += infoObj.Day;
                    }
                    break;

                case PERIOD_MONTH:
                    result += infoObj.Order + ";";
                    if (infoObj.Day != String.Empty)
                    {
                        result += infoObj.Day + ";";
                    }

                    if (infoObj.Every > 0)
                    {
                        result += infoObj.Every;
                    }
                    break;

                case PERIOD_YEAR:
                    result += infoObj.Every + ";";
                    if (!String.IsNullOrEmpty(infoObj.Order) && !String.IsNullOrEmpty(infoObj.Day))
                    {
                        result += infoObj.Order + ";";
                        result += infoObj.Day;
                    }
                    break;

                case PERIOD_ONCE:
                    break;
            }

            return result.TrimEnd(';');
        }


        /// <summary>
        /// Decodes interval string from database.
        /// </summary>
        /// <param name="interval">Interval string from database</param>
        /// <returns>TaskInterval object</returns>
        public static TaskInterval DecodeInterval(string interval)
        {
            TaskInterval result = new TaskInterval();
            if (interval == null)
            {
                return result;
            }

            string[] intervals = interval.Split(';');
            if (intervals.Length > 0)
            {
                // Period type
                result.Period = intervals[0].ToLowerInvariant();

                // Start time
                if ((intervals.Length >= 2) && !String.IsNullOrEmpty(intervals[1]))
                {
                    string[] date = intervals[1].Split('|');
                    result.StartTime = DecodeTime(date[0]);
                    if (date.Length == 2)
                    {
                        result.UseSpecificTime = ValidationHelper.GetBoolean(date[1], false);
                    }
                }

                switch (result.Period)
                {
                    case PERIOD_SECOND:
                    case PERIOD_MINUTE:
                    case PERIOD_HOUR:
                        // Every
                        if ((intervals.Length >= 3) && !String.IsNullOrEmpty(intervals[2]))
                        {
                            result.Every = DecodeEvery(intervals[2]);
                        }
                        //between start
                        if ((intervals.Length >= 4) && !String.IsNullOrEmpty(intervals[3]))
                        {
                            result.BetweenStart = DecodeTime(intervals[3]);
                        }
                        // between end
                        if ((intervals.Length >= 5) && !String.IsNullOrEmpty(intervals[4]))
                        {
                            result.BetweenEnd = DecodeTime(intervals[4]);
                        }
                        // Days
                        if ((intervals.Length >= 6) && !String.IsNullOrEmpty(intervals[5]))
                        {
                            result.Days = DecodeDays(intervals[5]).ToList();
                        }
                        break;

                    case PERIOD_DAY:
                        // Every
                        if ((intervals.Length >= 3) && !String.IsNullOrEmpty(intervals[2]))
                        {
                            result.Every = DecodeEvery(intervals[2]);
                        }
                        // Days
                        if ((intervals.Length >= 4) && !String.IsNullOrEmpty(intervals[3]))
                        {
                            result.Days = DecodeDays(intervals[3]).ToList();
                        }
                        break;

                    case PERIOD_WEEK:
                        // Every
                        if ((intervals.Length >= 3) && !String.IsNullOrEmpty(intervals[2]))
                        {
                            result.Every = DecodeEvery(intervals[2]);
                        }

                        // Specific day description
                        if ((intervals.Length >= 4) && !String.IsNullOrEmpty(intervals[3]))
                        {
                            result.Day = intervals[3];
                        }
                        break;

                    case PERIOD_MONTH:
                        if ((intervals.Length >= 3) && !String.IsNullOrEmpty(intervals[2]))
                        {
                            result.Order = intervals[2];
                        }
                        if ((intervals.Length >= 4) && !String.IsNullOrEmpty(intervals[3]))
                        {
                            if (ValidationHelper.GetInteger(intervals[3], -1) == -1)
                            {
                                result.Day = intervals[3];
                            }
                            else
                            {
                                result.Every = ValidationHelper.GetInteger(intervals[3], 0);
                            }
                        }
                        if ((intervals.Length >= 5) && (intervals[4] != null) && (ValidationHelper.GetInteger(intervals[4], -1) != -1))
                        {
                            result.Every = DecodeEvery(intervals[4]);
                        }
                        break;

                    case PERIOD_YEAR:
                        // Every
                        if ((intervals.Length >= 3) && !String.IsNullOrEmpty(intervals[2]))
                        {
                            result.Every = DecodeEvery(intervals[2]);
                        }

                        // Specific day description
                        if ((intervals.Length >= 5) && !String.IsNullOrEmpty(intervals[3]) && !String.IsNullOrEmpty(intervals[4]))
                        {
                            result.Order = intervals[3];
                            result.Day = intervals[4];
                        }
                        break;

                    case PERIOD_ONCE:
                        break;
                }
            }
            return result;
        }


        /// <summary>
        /// Logs task.
        /// </summary>
        /// <param name="task">Task to log</param>
        public static void LogTask(TaskInfo task)
        {
            if (!LogTasks)
            {
                return;
            }

            // Build the log text
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(task.TaskDisplayName))
            {
                sb.AppendLine("TaskDisplayName: " + task.TaskDisplayName);
            }

            if (!String.IsNullOrEmpty(task.TaskClass))
            {
                sb.AppendLine("TaskClass: " + task.TaskClass);
            }

            if (!String.IsNullOrEmpty(task.TaskInterval))
            {
                sb.AppendLine("TaskInterval: " + task.TaskInterval);
            }

            if (!String.IsNullOrEmpty(task.TaskData))
            {
                sb.AppendLine("TaskData: " + task.TaskData);
            }

            if (task.TaskLastRunTime != TaskInfoProvider.NO_TIME)
            {
                sb.AppendLine("TaskLastRunTime: " + task.TaskLastRunTime);
            }

            if (task.TaskNextRunTime != TaskInfoProvider.NO_TIME)
            {
                sb.AppendLine("TaskNextRunTime: " + task.TaskNextRunTime);
            }

            if (!String.IsNullOrEmpty(task.TaskLastResult))
            {
                sb.AppendLine("TaskLastResult: " + task.TaskLastResult);
            }

            if (task.TaskSiteID != 0)
            {
                sb.AppendLine("TaskSiteID: " + task.TaskSiteID);
            }

            if (!String.IsNullOrEmpty(task.TaskServerName))
            {
                sb.AppendLine("TaskServerName: " + task.TaskServerName);
            }

            sb.AppendLine();

            // Log the task
            try
            {
                // Log directly to the file
                string file = LogFile;
                if (file != null)
                {
                    File.AppendAllText(file, sb.ToString());
                }
            }
            catch (Exception ex)
            {
                // Log the exception in case of error
                EventLogProvider.LogException("Scheduler", "LogTask", ex);
            }
        }


        /// <summary>
        /// Runs the request to the scheduler page.
        /// </summary>
        /// <param name="url">URL to request</param>
        public static void RunSchedulerRequest(string url)
        {
            if (url == null)
            {
                return;
            }

            using(WebClient client = new WebClient())
            {
                try
                {
                    // Ensure certificate security
                    SecurityHelper.EnsureCertificateSecurity();

                    // Set direct URL if is defined
                    if (!String.IsNullOrEmpty(SchedulerUrl))
                    {
                        url = SchedulerUrl;
                    }

                    // If user name is defined use scheduler user credentials
                    if (SchedulerUserName != String.Empty)
                    {
                        client.Credentials = new NetworkCredential(SchedulerUserName, SchedulerPassword, SchedulerDomain);
                    }

                    // Get the page
                    string schedulerPageContent = client.DownloadString(url);

                    if (!unavailableSchedulerPageLogged && !schedulerPageContent.Contains(SCHEDULER_PING_CONTENT))
                    {
                        LogInvalidSchedulerPageEvent(url);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception in case of error
                    EventLogProvider.LogEvent(EventType.ERROR, "Scheduler", "Run", EventLogProvider.GetExceptionLogMessage(ex) + "\r\n\r\nREQUESTED URL: " + url, RequestContext.RawURL, 0, null, 0, null, RequestContext.UserHostAddress);
                }
            }
        }


        /// <summary>
        /// Attempts to run the scheduler request based on scheduler configuration.
        /// </summary>
        public static void RunEndRequestScheduler()
        {
            string siteName = SchedulingTimer.SchedulerRunImmediatelySiteName;
            if (UseAutomaticScheduler)
            {
                // Ensure the active timer running in an asynchronous thread
                var timer = SchedulingTimer.EnsureTimer(siteName, true);
                if ((timer != null) && SchedulingTimer.RunSchedulerImmediately)
                {
                    timer.ExecuteAsync();
                }
            }
            else
            {
                // --- Default scheduler settings
                // If scheduler run request acquired, run the actions
                bool runScheduler = SchedulingTimer.RequestRun(siteName) || SchedulingTimer.RunSchedulerImmediately;
                if (runScheduler)
                {
                    if (RunSchedulerWithinRequest)
                    {
                        // --- Default scheduler settings
                        try
                        {
                            // Flush the output
                            HttpContext.Current.Response.Flush();
                        }
                        catch
                        {
                            // Do not display closed host exception
                        }

                        // Run scheduler actively within the request 
                        var schedulingParams = new SchedulingExecutorParameters { SiteName = siteName, ServerName = WebFarmHelper.ServerName };

                        // Create synchronous thread
                        var schedulerThread = new CMSThread(schedulingParams.ExecuteScheduledTasks, true, ThreadModeEnum.Sync);
                        schedulerThread.Start();
                    }
                    else
                    {
                        // Get passive timer and execute
                        var timer = SchedulingTimer.EnsureTimer(siteName, false);
                        timer?.ExecuteAsync();
                    }
                }
            }
        }


        /// <summary>
        /// Gets application scheduler interval.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int ApplicationInterval(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSSchedulerInterval"].ToInteger(0);
        }


        /// <summary>
        /// Returns true if there are any currently running tasks.
        /// </summary>
        public static bool IsAnyTaskRunning()
        {
            // Get only running task, also filter out task without resource (these tasks look like they are running but they cannot be run)
            return TaskInfoProvider.GetTasks().WhereTrue("TaskIsRunning")
                                   .Column("TaskResourceID")
                                   .Any(t => ResourceInfoProvider.IsResourceAvailable(t.TaskResourceID));
        }

        #endregion


        #region "Private constants"

        private const int NUMBER_OF_DAYS_IN_WEEK = 7;

        private const string SCHEDULER_URL_SETTING_NAME = "CMSSchedulerURL";

        #endregion


        #region "Private types"

        /// <summary>
        /// Provides delegates to compute total number of units from a TimeSpan and to add units into a DateTime.
        /// </summary>
        private class PeriodUnitDelegates
        {
            public Func<DateTime, double, DateTime> UnitAdditionDelegate
            {
                get;
                set;
            }


            public Func<TimeSpan, double> UnitTotalDelegate
            {
                get;
                set;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns <c>true</c> if provided task is set to <see cref="TaskInfo.TaskUseExternalService"/> and has <see cref="TaskInfo.TaskNextRunTime"/> properly set.
        /// Also task have to be enabled for execution (running or disabled tasks cannot be late).
        /// </summary>
        private static bool IsLateEligibleExternalTask(TaskInfo taskInfo)
        {
            return (taskInfo != null)
                && taskInfo.TaskUseExternalService
                && taskInfo.TaskEnabled
                && !taskInfo.TaskIsRunning
                && (taskInfo.TaskNextRunTime != TaskInfoProvider.NO_TIME);
        }


        /// <summary>
        /// Returns date and time that is less than one interval before current time provided.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="baseTime">Time to apply task interval on</param>
        /// <param name="interval">Number of units that are supposed to be add to base time until current time is reached</param>
        /// <param name="unitAdditionDelegate">Method adding interval number to a date and time in appropriate units (e.g. AddMinutes, AddHours, ...)</param>
        /// <param name="unitTotalDelegate">Method computing total number of appropriate units in a time span (e.g. TotalMinutes, TotalHours, ...)</param>
        /// <exception cref="ArgumentNullException">When any delegate is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">When base time succeeds current time</exception>
        private static DateTime GetNearestPeriodOfUnitInPast(this DateTime currentTime, DateTime baseTime, int interval, Func<DateTime, double, DateTime> unitAdditionDelegate, Func<TimeSpan, double> unitTotalDelegate)
        {
            if (unitTotalDelegate == null)
            {
                throw new ArgumentNullException(nameof(unitTotalDelegate));
            }
            if (unitAdditionDelegate == null)
            {
                throw new ArgumentNullException(nameof(unitAdditionDelegate));
            }
            if (baseTime > currentTime)
            {
                throw new ArgumentOutOfRangeException(nameof(baseTime), "BaseTime has to precede currentTime!");
            }

            return currentTime == baseTime
                ? currentTime
                : unitAdditionDelegate(baseTime, Math.Floor(unitTotalDelegate((currentTime - baseTime)) / interval) * interval);
        }


        /// <summary>
        /// Returns date and time that is less than one interval before or after current time provided.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="baseTime">Time to apply task interval on</param>
        /// <param name="interval">Number of units that are supposed to be add to base time until current time is reached</param>
        /// <param name="unitAdditionDelegate">Method adding interval number to a date and time in appropriate units (e.g. AddMinutes, AddHours, ...)</param>
        /// <param name="unitTotalDelegate">Method computing total number of appropriate units in a time span (e.g. TotalMinutes, TotalHours, ...)</param>
        /// <param name="inFuture">Determines whether the time returned should precede or succeed current time</param>
        private static DateTime GetNearestPeriodOfUnit(this DateTime currentTime, DateTime baseTime, int interval, Func<DateTime, double, DateTime> unitAdditionDelegate, Func<TimeSpan, double> unitTotalDelegate, bool inFuture)
        {
            DateTime nearestInPast = currentTime.GetNearestPeriodOfUnitInPast(baseTime, interval, unitAdditionDelegate, unitTotalDelegate);

            return inFuture && nearestInPast <= currentTime
                ? unitAdditionDelegate(nearestInPast, interval)
                : nearestInPast;
        }


        /// <summary>
        /// Gets unit-based (minute-, hour-, day-) delegates for the unit addition and total units counting.
        /// </summary>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        /// <returns>PeriodUnitDelegates object with addition and total delegates defined</returns>
        /// <exception cref="NotSupportedException">When task interval's period string is not known</exception>
        private static PeriodUnitDelegates GetPeriodDelegates(TaskInterval taskInterval, DateTime currentTime, DateTime baseTime)
        {
            const int NUMBER_OF_MONTHS_IN_YEAR = 12;

            var result = new PeriodUnitDelegates();
            switch (taskInterval.Period.ToLowerInvariant())
            {
                case PERIOD_SECOND:
                    result.UnitAdditionDelegate = (time, interval) => time.AddSeconds(interval);
                    result.UnitTotalDelegate = span => span.TotalSeconds;
                    break;

                case PERIOD_MINUTE:
                    result.UnitAdditionDelegate = (time, interval) => time.AddMinutes(interval);
                    result.UnitTotalDelegate = span => span.TotalMinutes;
                    break;

                case PERIOD_HOUR:
                    result.UnitAdditionDelegate = (time, interval) => time.AddHours(interval);
                    result.UnitTotalDelegate = span => span.TotalHours;
                    break;

                case PERIOD_DAY:
                    result.UnitAdditionDelegate = (time, interval) => time.AddDays(interval);
                    result.UnitTotalDelegate = span => span.TotalDays;
                    break;

                case PERIOD_WEEK:
                    result.UnitAdditionDelegate = (time, interval) => time.AddDays(NUMBER_OF_DAYS_IN_WEEK * interval);
                    result.UnitTotalDelegate = span => span.TotalDays / NUMBER_OF_DAYS_IN_WEEK;
                    break;

                case PERIOD_MONTH:
                    result.UnitAdditionDelegate = (time, interval) => time.AddMonths((int)interval);
                    result.UnitTotalDelegate = _ => (currentTime.Year - baseTime.Year) * NUMBER_OF_MONTHS_IN_YEAR + (currentTime.Month - baseTime.Month);
                    break;

                case PERIOD_YEAR:
                    result.UnitAdditionDelegate = (time, interval) => time.AddYears((int)interval);
                    result.UnitTotalDelegate = _ => currentTime.Year - baseTime.Year;
                    break;

                default:
                    throw new NotSupportedException("Task period is not supported");
            }

            return result;
        }


        /// <summary>
        /// Moves given date and time forward or backwards to the nearest period occurrence in future/past.
        /// No other aspects of interval are taken into account (e.g. BetweenStart or Days).
        /// The time between provided current time and returned time will always be equal or less than the period (in respective units).
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        /// <param name="inFuture">Determines whether the time returned should precede or succeed current time</param>
        private static DateTime MoveToNearestPeriod(this DateTime currentTime, TaskInterval taskInterval, DateTime baseTime, bool inFuture)
        {
            PeriodUnitDelegates periodUnitDelegates = GetPeriodDelegates(taskInterval, currentTime, baseTime);

            DateTime nextRunTime = currentTime.GetNearestPeriodOfUnit(baseTime, taskInterval.Every, periodUnitDelegates.UnitAdditionDelegate, periodUnitDelegates.UnitTotalDelegate, inFuture);

            return nextRunTime;
        }


        /// <summary>
        /// Moves given date and time backwards to the nearest period occurrence in past.
        /// No other aspects of interval are taken into account (e.g. BetweenStart or Days).
        /// The time between provided current time and returned time will always be equal or less than the period (in respective units).
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime MoveToNearestPeriodInPast(this DateTime currentTime, TaskInterval taskInterval, DateTime baseTime)
        {
            return currentTime.MoveToNearestPeriod(taskInterval, baseTime, false);
        }


        /// <summary>
        /// Moves given date and time forward to the nearest period occurrence in future.
        /// No other aspects of interval are taken into account (e.g. BetweenStart or Days).
        /// The time between provided current time and returned time will always be equal or less than the period (in respective units).
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime MoveToNearestPeriodInFuture(this DateTime currentTime, TaskInterval taskInterval, DateTime baseTime)
        {
            return currentTime.MoveToNearestPeriod(taskInterval, baseTime, true);
        }


        /// <summary>
        /// For periods directly affecting time of day (second, minute, hour), method moves given date and time forward to the nearest period based occurrence in future.
        /// For other periods, returns day time of provided date and time (as time of day will never change).
        /// </summary>
        /// <param name="value">Date and time to shift</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        /// <exception cref="NotSupportedException">When task interval's period string is not known</exception>
        private static TimeSpan MoveToNextTimeOfDayInFuture(this DateTime value, TaskInterval taskInterval, DateTime baseTime)
        {
            TimeSpan wouldBeNextRunTimeSpan;
            switch (taskInterval.Period.ToLowerInvariant())
            {
                case PERIOD_SECOND:
                case PERIOD_MINUTE:
                case PERIOD_HOUR:
                    wouldBeNextRunTimeSpan = value.MoveToNearestPeriodInFuture(taskInterval, baseTime).TimeOfDay;
                    break;

                case PERIOD_DAY:
                case PERIOD_WEEK:
                case PERIOD_MONTH:
                case PERIOD_YEAR:
                    wouldBeNextRunTimeSpan = value.TimeOfDay;
                    break;

                default:
                    throw new NotSupportedException("Task period is not supported");
            }

            return wouldBeNextRunTimeSpan;
        }


        /// <summary>
        /// Tests if given task interval describes a special Year period case when specific month and day is required.
        /// </summary>
        /// <param name="taskInterval">Information about task repetition</param>
        private static bool IsSpecialMonthAndDayOfYearCase(this TaskInterval taskInterval)
        {
            return taskInterval.Period.Equals(PERIOD_YEAR, StringComparison.Ordinal) && !String.IsNullOrEmpty(taskInterval.Order);
        }


        /// <summary>
        /// Tests if given task interval describes a special Week period case when either the nearest specific day of month or the nearest specific day of nth week of month is supposed to be reached.
        /// </summary>
        /// <param name="taskInterval">Information about task repetition</param>
        private static bool IsSpecialDayOrWeekOfMonthCase(this TaskInterval taskInterval)
        {
            return taskInterval.Period.Equals(PERIOD_MONTH, StringComparison.Ordinal) && !String.IsNullOrEmpty(taskInterval.Order);
        }


        /// <summary>
        /// Tests if given task interval describes a special Week period case when specific number of weeks is supposed to be added and then nearest (single) day of week found.
        /// </summary>
        /// <param name="taskInterval">Information about task repetition</param>
        private static bool IsSpecialDayInWeekCase(this TaskInterval taskInterval)
        {
            return taskInterval.Period.Equals(PERIOD_WEEK, StringComparison.Ordinal) && !String.IsNullOrEmpty(taskInterval.Day);
        }


        /// <summary>
        /// Tests if given date and time's time of day is lower than the upper allowed time threshold.
        /// </summary>
        /// <param name="value">Date and time to test</param>
        /// <param name="taskInterval">Information about task repetition</param>
        private static bool IsBeforeAllowedTimeEnd(this TimeSpan value, TaskInterval taskInterval)
        {
            return value < taskInterval.BetweenEnd.AddSeconds(-taskInterval.BetweenEnd.Second).TimeOfDay;
        }


        /// <summary>
        /// Tests if given date and time's time of day is within time of day allowed by task interval.
        /// If there is no time of day specified in the task interval, true is returned.
        /// </summary>
        /// <param name="value">Date and time to test</param>
        /// <param name="taskInterval">Information about task repetition</param>
        private static bool IsWithinAllowedDayTime(this DateTime value, TaskInterval taskInterval)
        {
            bool isWithinInterval = (taskInterval.BetweenEnd == TaskInfoProvider.NO_TIME)
                || (taskInterval.BetweenStart >= taskInterval.BetweenEnd)
                || (value.TimeOfDay >= taskInterval.BetweenStart.TimeOfDay && value.TimeOfDay.IsBeforeAllowedTimeEnd(taskInterval));

            return isWithinInterval;
        }


        /// <summary>
        /// Shifts given date and time until time of day is within allowed range.
        /// </summary>
        /// <param name="value">Date and time to shift</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime ShiftIntoAllowedDayTime(this DateTime value, TaskInterval taskInterval, DateTime baseTime)
        {
            if (taskInterval.BetweenEnd != TaskInfoProvider.NO_TIME && taskInterval.BetweenStart < taskInterval.BetweenEnd)
            {
                // Test if time of day precedes allowed time span, if so shift it forward
                bool shiftToStart = value.TimeOfDay < taskInterval.BetweenStart.TimeOfDay;

                if (!shiftToStart)
                {
                    // Test if given time falls in the allowed time span - adding period will only make it worse
                    shiftToStart = !value.TimeOfDay.IsBeforeAllowedTimeEnd(taskInterval) || !value
                        .MoveToNextTimeOfDayInFuture(taskInterval, baseTime)
                        .IsBeforeAllowedTimeEnd(taskInterval);

                    // If adding period to would exceed allowed time span, add day
                    if (shiftToStart)
                        value = value.AddDays(1);
                }

                if (shiftToStart)
                {
                    // Replace time of day with BetweenStart time of day
                    value = value
                        .Date
                        .Add(taskInterval.BetweenStart.TimeOfDay);
                }
            }

            return value;
        }


        /// <summary>
        /// Tests if given date's day of week is allowed by task interval.
        /// </summary>
        /// <param name="value">Date and time to test</param>
        /// <param name="taskInterval">Information about task repetition</param>
        private static bool IsInAllowedDayOfWeek(this DateTime value, TaskInterval taskInterval)
        {
            return !taskInterval.Days.Any()
                   || (taskInterval.Days.Count == Enum.GetValues(typeof(DayOfWeek)).Length)
                   || taskInterval.Days.Contains(value.DayOfWeek);
        }


        /// <summary>
        /// Shifts given date and time until day of week is allowed.
        /// </summary>
        /// <param name="value">Date and time to shift</param>
        /// <param name="taskInterval">Information about task repetition</param>
        private static DateTime ShiftIntoAllowedDayOfWeek(this DateTime value, TaskInterval taskInterval)
        {
            if (taskInterval.Days.Any() && (taskInterval.Days.Count != Enum.GetValues(typeof(DayOfWeek)).Length))
            {
                while (!taskInterval.Days.Contains(value.DayOfWeek))
                {
                    value = value.MoveToNearestPeriodInFuture(taskInterval, value);
                }
            }

            return value;
        }


        /// <summary>
        /// Shifts given date and time until both day of week and time of day are allowed.
        /// </summary>
        /// <param name="value">Date and time to shift</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime ShiftIntoAllowedDayAndTime(this DateTime value, TaskInterval taskInterval, DateTime baseTime)
        {
            while (!value.IsWithinAllowedDayTime(taskInterval) || !value.IsInAllowedDayOfWeek(taskInterval))
            {
                value = value
                        .ShiftIntoAllowedDayTime(taskInterval, baseTime)
                        .ShiftIntoAllowedDayOfWeek(taskInterval);
            }

            return value;
        }


        /// <summary>
        /// Returns total units (seconds, minutes, ... - based on interval object) between two given dates.
        /// </summary>
        /// <param name="taskInterval">Defines what units will be computed with</param>
        /// <param name="laterTime">Supposedly later of the times to make the difference of</param>
        /// <param name="earlierTime">Supposedly earlier of the times to make the difference of</param>
        private static double GetTotalUnits(TaskInterval taskInterval, DateTime laterTime, DateTime earlierTime)
        {
            return GetPeriodDelegates(taskInterval, laterTime, earlierTime).UnitTotalDelegate(laterTime - earlierTime);
        }


        /// <summary>
        /// Determines if the given date and time is based on start time and period provided in task interval or not.
        /// </summary>
        /// <param name="value">Date and time supposedly based on task interval start time</param>
        /// <param name="taskInterval">Information about task repetition</param>
        private static Boolean IsValidSuccessor(this DateTime value, TaskInterval taskInterval)
        {
            if (taskInterval.Every == 0)
            {
                // If period repetition is zero, only start time is valid successor (reflexion)
                return taskInterval.StartTime == value;
            }

            var shiftedStartTime = taskInterval.StartTime.ShiftIntoAllowedDayAndTime(taskInterval, taskInterval.StartTime);
            var totalUnits = GetTotalUnits(taskInterval, value, shiftedStartTime);
            return Equals(totalUnits % taskInterval.Every, 0d);
        }


        /// <summary>
        /// Moves given date and time forward to the nearest  month and day of a month as specified in task.
        /// Method implies that task interval is set to year period with Order any Day properties specified.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime MoveToSpecificMonthAndDayOfYearInFuture(this DateTime currentTime, TaskInterval taskInterval, DateTime baseTime)
        {
            const int DEFAULT_DAY = 1;
            const int DEFAULT_MONTH = 1;

            // shift base time to appropriate month
            int month = ValidationHelper.GetInteger(taskInterval.Order, DEFAULT_MONTH);
            baseTime = baseTime.AddMonths(month - baseTime.Month);

            // shift base time to appropriate day
            int day = ValidationHelper.GetInteger(taskInterval.Day, DEFAULT_DAY);
            int baseDay = Math.Min(DateTime.DaysInMonth(baseTime.Year, baseTime.Month), day);
            baseTime = baseTime.AddDays(baseDay - baseTime.Day);

            DateTime nextRunTime = currentTime.MoveToNearestPeriodInFuture(taskInterval, baseTime);
            if (nextRunTime.Day != day && DateTime.DaysInMonth(nextRunTime.Year, nextRunTime.Month) >= day)
            {
                // Once the day is 29, month refers to February and the current year is not a leap-year, yet the nextRunTime year is leap-year
                nextRunTime = nextRunTime.AddDays(day - nextRunTime.Day);
            }

            return nextRunTime;
        }


        /// <summary>
        /// Moves given date and time forward to the nearest  day of a month.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="dayOfMonth">Desired day of a month</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime MoveToSpecificDayOfMonthInFuture(this DateTime currentTime, TaskInterval taskInterval, int dayOfMonth, DateTime baseTime)
        {
            DateTime nextRunTime = currentTime.MoveToNearestPeriodInPast(taskInterval, baseTime);
            if (nextRunTime.Day != dayOfMonth)
            {
                int currentDaysInMonth = DateTime.DaysInMonth(nextRunTime.Year, nextRunTime.Month);
                if ((dayOfMonth == -1) || taskInterval.IsImpossibleToReachGivenDay(currentDaysInMonth, dayOfMonth))
                {
                    // Last day of month
                    dayOfMonth = currentDaysInMonth;
                }
                else
                {
                    // Adds period-number of units (usually months) until the month of the year does have enough days
                    while (DateTime.DaysInMonth(nextRunTime.Year, nextRunTime.Month) < dayOfMonth)
                    {
                        nextRunTime = nextRunTime
                            .AddDays(1)
                            .MoveToNearestPeriodInFuture(taskInterval, baseTime);
                    }
                }
                nextRunTime = nextRunTime.AddDays(dayOfMonth - nextRunTime.Day);
            }

            if (nextRunTime <= currentTime)
            {
                // It might happen that current time is commensurable with interval start time and period, in this case, next run time should always be greater than (not greater or equal to) provided current time (to be truly in future)
                nextRunTime = currentTime.MoveToNearestPeriodInFuture(taskInterval, nextRunTime);
            }

            return nextRunTime;
        }


        /// <summary>
        /// Returns true if there is less days in the current month than is the desired day and the period will end in this month every time (e.g. 12 month period).
        /// </summary>
        /// <param name="interval">Information about task repetition</param>
        /// <param name="daysInMonth">Number of days in the current month</param>
        /// <param name="selectedDay">Selected day in month when the repetition should occur</param>
        private static bool IsImpossibleToReachGivenDay(this TaskInterval interval, int daysInMonth, int selectedDay)
        {
            return (interval.Period == PERIOD_MONTH) && (interval.Every % 12 == 0) && (daysInMonth < selectedDay);
        }


        /// <summary>
        /// Gets number of week in the month provided within time.
        /// </summary>
        /// <param name="time">Time to get number of week from</param>
        private static int GetWeekInMonthNumber(this DateTime time)
        {
            return (int)Math.Ceiling(time.Day / (double)NUMBER_OF_DAYS_IN_WEEK);
        }


        /// <summary>
        /// Moves given date and time forward to the last week within month provided.
        /// </summary>
        /// <param name="timeToMove">Date and time to move forward from</param>
        private static DateTime MoveToLastWeekOfMonth(this DateTime timeToMove)
        {
            DateTime nextWeekRunTime = timeToMove;

            do
            {
                nextWeekRunTime = nextWeekRunTime.AddDays(NUMBER_OF_DAYS_IN_WEEK);
            } while (nextWeekRunTime.Month == timeToMove.Month);

            return nextWeekRunTime.AddDays(-NUMBER_OF_DAYS_IN_WEEK);
        }


        /// <summary>
        /// Moves given date and time forward to the nearest weekNumberth week of a month. Given month might moved to the following if desired week number precede current week number.
        /// </summary>
        /// <param name="timeToMove">Date and time to move forward from</param>
        /// <param name="weekNumber">Desired number of week in a month (1..4)</param>
        private static DateTime MoveToNearestNthWeekOfMonth(this DateTime timeToMove, int weekNumber)
        {
            int currentWeekNumber = timeToMove.GetWeekInMonthNumber();

            while (currentWeekNumber != weekNumber)
            {
                timeToMove = timeToMove.AddDays(NUMBER_OF_DAYS_IN_WEEK);
                currentWeekNumber = timeToMove.GetWeekInMonthNumber();
            }

            return timeToMove;
        }


        /// <summary>
        /// Gets simple task interval that is only meant to allow day-by-day shifting from any given date to (single) allowed day of week specified in provided task interval.
        /// </summary>
        /// <param name="taskInterval">Source task interval with the Day property specified properly</param>
        /// <exception cref="NotSupportedException">When day parameter of taskInterval cannot be interpreted as a day of week</exception>
        private static TaskInterval ExtractDayOfWeekShiftingInterval(this TaskInterval taskInterval)
        {
            DayOfWeek dayOfWeek;
            if (!Enum.TryParse(taskInterval.Day, true, out dayOfWeek))
            {
                throw new NotSupportedException("Day property of taskInterval is not valid DayOfWeek");
            }

            // Period that allows day-by-day shifting until day of week equals day specified in task
            return new TaskInterval
            {
                Period = PERIOD_DAY,
                Every = 1,
                Days = new List<DayOfWeek>
                {
                    dayOfWeek
                }
            };
        }


        /// <summary>
        /// Moves current date and time forward to the nearest specific day of nth week of month.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="weekNumber">Week number in a month</param>
        /// <exception cref="ArgumentOutOfRangeException">When week is not in interval 1..5</exception>
        private static DateTime MoveToSpecificWeekDayOfMonthInFuture(this DateTime currentTime, TaskInterval taskInterval, int weekNumber)
        {
            if (weekNumber < 1 || weekNumber > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(weekNumber), weekNumber, "Only values 1 to 5 are supported.");
            }

            var dayOfWeekInterval = taskInterval.ExtractDayOfWeekShiftingInterval();
            var shiftedCurrentTime = currentTime.AddDays(1).Date.ShiftIntoAllowedDayOfWeek(dayOfWeekInterval);
            var currentWeekNumber = shiftedCurrentTime.GetWeekInMonthNumber();

            // Week number 5 means last week
            return currentWeekNumber == weekNumber
                ? shiftedCurrentTime
                : (weekNumber == 5
                    ? shiftedCurrentTime.MoveToLastWeekOfMonth()
                    : shiftedCurrentTime.MoveToNearestNthWeekOfMonth(weekNumber));
        }


        /// <summary>
        /// Moves current date and time forward based on given task interval and base time and further on to the nearest specific day of week.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime MoveToSpecificDayOfWeekInFuture(this DateTime currentTime, TaskInterval taskInterval, DateTime baseTime)
        {
            var dayOfWeekInterval = taskInterval.ExtractDayOfWeekShiftingInterval();

            // handling special zero weeks case
            DateTime nextUnshiftedRunTime = taskInterval.Every <= 0
                ? currentTime.AddDays(1)
                : currentTime.MoveToNearestPeriodInFuture(taskInterval, baseTime);

            return nextUnshiftedRunTime
                .Date
                .ShiftIntoAllowedDayOfWeek(dayOfWeekInterval);
        }


        /// <summary>
        /// Moves current date and time forward to either the nearest specific day of month or the nearest specific day of nth week of month.
        /// </summary>
        /// <param name="currentTime">Date and time to resolve against</param>
        /// <param name="taskInterval">Information about task repetition</param>
        /// <param name="baseTime">Time to apply task repetition criteria on</param>
        private static DateTime MoveToSpecificDayOrWeekOfMonthInFuture(this DateTime currentTime, TaskInterval taskInterval, DateTime baseTime)
        {
            int order = taskInterval.ToIntOrder();

            return String.IsNullOrEmpty(taskInterval.Day)
                ? currentTime.MoveToSpecificDayOfMonthInFuture(taskInterval, order, baseTime)
                : currentTime.MoveToSpecificWeekDayOfMonthInFuture(taskInterval, order);
        }


        /// <summary>
        /// Converts day of week from string to DayOfWeek enum.
        /// </summary>
        /// <param name="day">Day name</param>
        private static DayOfWeek ConvertDayToNumber(string day)
        {
            DayOfWeek dayOfWeek;
            if (Enum.TryParse(day, true, out dayOfWeek))
            {
                return dayOfWeek;
            }

            return default(DayOfWeek);
        }


        /// <summary>
        /// Decodes 'every' value from string to integer.
        /// </summary>
        /// <param name="interval">String representation of every value</param>
        private static int DecodeEvery(string interval)
        {
            int result = ValidationHelper.GetInteger(interval, 1);
            if ((Convert.ToInt32(result) < 0) && (Convert.ToInt32(result) > 10000))
            {
                result = 1;
            }
            return result;
        }


        /// <summary>
        /// Returns date and time in English culture format.
        /// </summary>
        /// <param name="interval">DateTime string</param>
        private static DateTime DecodeTime(string interval)
        {
            return ValidationHelper.GetDateTime(interval, DateTime.Now, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Splits days from input string into DayOfWeek collection.
        /// </summary>
        /// <param name="interval">String with days' names separated by ','</param>
        private static IEnumerable<DayOfWeek> DecodeDays(string interval)
        {
            IEnumerable<DayOfWeek> days = interval.Split(',').Select(ConvertDayToNumber);

            return days;
        }


        /// <summary>
        /// Logs unavailable scheduler page error and sets the <see cref="unavailableSchedulerPageLogged"/> flag.
        /// </summary>
        private static void LogInvalidSchedulerPageEvent(string url)
        {
            EventLogProvider.LogEvent(EventType.ERROR, "Scheduler", "PingFailed",
                String.Format("The scheduler page '{0}' could not be reached. The scheduler might not work correctly. Make sure the '{1}' setting value is set correctly and the URL is visible for the application. This error is logged only once per application run.", url, SCHEDULER_URL_SETTING_NAME));
            unavailableSchedulerPageLogged = true;
        }

        #endregion
    }
}