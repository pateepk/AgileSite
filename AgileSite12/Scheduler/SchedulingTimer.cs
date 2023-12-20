using System;
using System.Collections;
using System.Threading;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Scheduler
{
    /// <summary>
    /// Class to provide timing for scheduler.
    /// </summary>
    public class SchedulingTimer
    {
        #region "Variables"

        /// <summary>
        /// Timer objects.
        /// </summary>
        private static Hashtable mTimers = new Hashtable();


        /// <summary>
        /// Timer threads.
        /// </summary>
        private static Hashtable mThreads = new Hashtable();


        /// <summary>
        /// Last timer run.
        /// </summary>
        private static Hashtable mLastRuns = new Hashtable();


        /// <summary>
        /// If true, the timer is running.
        /// </summary>
        protected bool mRunning = false;


        /// <summary>
        /// If true, the timer cancels execution.
        /// </summary>
        protected bool mCancel = false;


        /// <summary>
        /// URL to scheduler.ashx.
        /// </summary>
        private string mScheduleURL = String.Empty;


        /// <summary>
        /// Scheduler site name.
        /// </summary>
        private string mSiteName = String.Empty;


        /// <summary>
        /// Timer thread.
        /// </summary>
        private CMSThread mThread;


        /// <summary>
        /// Execute thread.
        /// </summary>
        private CMSThread mExecuteThread;

        #endregion


        #region "Properties"

        /// <summary>
        /// Last timer run.
        /// </summary>
        public static Hashtable LastRuns
        {
            get
            {
                return mLastRuns;
            }
        }


        /// <summary>
        /// If true, the scheduler runs immediately after the request finishes.
        /// </summary>
        public static bool RunSchedulerImmediately
        {
            get
            {
                return ValidationHelper.GetBoolean(RequestStockHelper.GetItem("RunSchedulerImmediately", true), false);
            }
            set
            {
                RequestStockHelper.Add("RunSchedulerImmediately", value, true);
            }
        }


        /// <summary>
        /// Gets or sets the site name which should be used for immediate run
        /// </summary>
        public static string SchedulerRunImmediatelySiteName
        {
            get
            {
                return ValidationHelper.GetString(RequestStockHelper.GetItem("CMSSchedulerRunImmediatelySiteName", true), SiteContext.CurrentSiteName);
            }

            set
            {
                RequestStockHelper.Add("CMSSchedulerRunImmediatelySiteName", (object)value, true);
            }
        }


        /// <summary>
        /// Schedule URL.
        /// </summary>
        public string ScheduleURL
        {
            get
            {
                return mScheduleURL;
            }
            set
            {
                mScheduleURL = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates thread which calls SchedulerExecutor periodically.
        /// </summary>
        /// <param name="schedulerURL">Scheduler URL</param>
        /// <param name="siteName">Site name</param>
        /// <param name="runTimer">Run the timer immediately</param>
        public SchedulingTimer(string schedulerURL, string siteName, bool runTimer)
        {
            // Setup
            mScheduleURL = schedulerURL;
            mSiteName = siteName.ToLowerCSafe();

            // Register timer
            mTimers[mSiteName] = this;

            // Run the timer
            if (runTimer)
            {
                RunTimerAsync();
            }
        }


        /// <summary>
        /// Runs the timer in an asynchronous thread.
        /// </summary>
        public void RunTimerAsync()
        {
            mThread = new CMSThread(Run);
            mThread.Start();
        }


        /// <summary>
        /// Downloads scheduler.ashx in neverending loop.
        /// </summary>
        public void Run()
        {
            try
            {
                mThreads[mSiteName] = Thread.CurrentThread;

                mRunning = true;
                mCancel = false;

                while (!mCancel)
                {
                    // If run acquired, execute
                    if (RequestRun(mSiteName))
                    {
                        Execute();
                    }

                    //Timer interval depends on scheduler settings
                    int interval = SchedulingHelper.ApplicationInterval(mSiteName);

                    // If wrong value,zero or interval larger then 30 sec set timer to 30 sec
                    if ((interval <= 0) || (interval > 30))
                    {
                        interval = 30;
                    }

                    Thread.Sleep(interval * 1000);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Scheduler", "Run", ex);
            }
            finally
            {
                mRunning = false;
                mCancel = false;

                // If hashtables contains current timer and thread(not replaced by new ones), clear it
                if (mTimers[mSiteName] == this)
                {
                    mTimers[mSiteName] = null;
                }
                if (mThreads[mSiteName] == Thread.CurrentThread)
                {
                    mThreads[mSiteName] = null;
                }
            }
        }


        /// <summary>
        /// Downloads scheduler.ashx once.
        /// </summary>
        public void Execute()
        {
            // Mark last run
            mLastRuns[mSiteName] = DateTime.Now;

            SchedulingHelper.RunSchedulerRequest(mScheduleURL);
        }


        /// <summary>
        /// Executes the request in an asynchronous thread.
        /// </summary>
        public void ExecuteAsync()
        {
            mExecuteThread = new CMSThread(Execute);
            mExecuteThread.Start();
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Ensures the scheduling timer.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="running">If true, the timer is running, else the timer is passive (executes at request)</param>
        public static SchedulingTimer EnsureTimer(string siteName, bool running)
        {
            siteName = siteName.ToLowerCSafe();

            return mTimers[siteName] as SchedulingTimer ?? CreateTimer(siteName, running);
        }


        /// <summary>
        /// Stops all the timers.
        /// </summary>
        public static void StopTimers()
        {
            // Cancel the timers
            foreach (SchedulingTimer timer in mTimers.Values)
            {
                StopTimer(timer.mSiteName);
            }
        }


        /// <summary>
        /// Stops the specified timer.
        /// </summary>
        /// <param name="siteName">Timer site name to stop</param>
        public static void StopTimer(string siteName)
        {
            siteName = siteName.ToLowerCSafe();

            // Cancel the timer execution
            SchedulingTimer timer = (SchedulingTimer)mTimers[siteName];
            if (timer != null)
            {
                timer.mCancel = true;
                mTimers[siteName] = null;
            }

            // Stop the thread
            Thread timerThread = (Thread)mThreads[siteName];
            if (timerThread != null)
            {
                timerThread.Abort();
            }

            mThreads[siteName] = null;
        }


        /// <summary>
        /// Returns true if specified site timer exists.
        /// </summary>
        /// <param name="siteName">Site name to check</param>
        public static bool TimerExists(string siteName)
        {
            return (mTimers[siteName.ToLowerCSafe()] != null);
        }


        /// <summary>
        /// Restarts the given site timer.
        /// </summary>
        public static void RestartTimer(string siteName)
        {
            siteName = siteName.ToLowerCSafe();

            // Stop timer
            StopTimer(siteName);

            // Run new timer thread
            SchedulingTimer timer = (SchedulingTimer)mTimers[siteName];
            if (timer != null)
            {
                timer.RunTimerAsync();
            }
            else
            {
                CreateTimer(siteName, true);
            }
        }


        /// <summary>
        /// Creates a timer for specified site name.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="runTimer">Run the timer immediately</param>
        public static SchedulingTimer CreateTimer(string siteName, bool runTimer)
        {
            try
            {
                // Get the site data
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                string name = si != null ? si.SiteName : siteName;

                int interval = SchedulingHelper.ApplicationInterval(name);
                if (interval > 0)
                {
                    if (CMSHttpContext.Current != null)
                    {
                        // Prepare the path parts
                        string appPath = SystemContext.ApplicationPath.TrimEnd('/');
                        string domain = si != null ? si.DomainName : RequestContext.URL.Host;

                        if (domain.Contains("/"))
                        {
                            // If domain contains the application path, do not add it
                            appPath = null;
                        }

                        // Prepare the URL
                        string url = RequestContext.CurrentScheme + "://" + domain + appPath + "/CMSPages/Scheduler.ashx";
                        return new SchedulingTimer(url, name, runTimer);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogEvent(EventType.ERROR, "Scheduler", "CreateTimer", EventLogProvider.GetExceptionLogMessage(ex), RequestContext.RawURL, 0, "", 0, "", RequestContext.UserHostAddress);
            }

            return null;
        }


        /// <summary>
        /// Requests scheduler run for specified site name, updates the Last runs HashTable and returns true if the scheduler run is allowed.
        /// </summary>
        /// <param name="siteName">Scheduler site name</param>
        public static bool RequestRun(string siteName)
        {
            // Get interval
            int interval = SchedulingHelper.ApplicationInterval(siteName);
            if (interval <= 0)
            {
                return false;
            }
            siteName = siteName.ToLowerCSafe();

            // Get last run time
            object lastRun = mLastRuns[siteName];
            bool allowRun = (lastRun == null) || (((DateTime)lastRun).AddSeconds(interval) < DateTime.Now);

            // If allowed, update last run time
            if (allowRun)
            {
                mLastRuns[siteName] = DateTime.Now;
            }

            return allowRun;
        }


        /// <summary>
        /// Initializes the site scheduler to run ASAP.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void RunSchedulerASAP(string siteName)
        {
            RunSchedulerImmediately = true;

            LastRuns[siteName.ToLowerCSafe()] = null;
        }

        #endregion
    }
}