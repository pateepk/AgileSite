using System;
using System.Threading;

using CMS.Base;
using CMS.EventLog;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// The class that provides timing for web application performance.
    /// </summary>
    public class PerformanceCounterTimer
    {
        #region "Variables"

        /// <summary>
        /// Indicates if the timer runs.
        /// </summary>
        private bool mRunning = false;

        /// <summary>
        /// If true, the timer cancels execution.
        /// </summary>
        private bool mCancel = false;

        /// <summary>
        /// Indicates if thread was started.
        /// </summary>
        private bool threadStarted = false;

        /// <summary>
        /// Thread.
        /// </summary>
        private static CMSThread thread = null;

        /// <summary>
        /// Timer.
        /// </summary>
        private static PerformanceCounterTimer timer = null;

        /// <summary>
        /// Lock used for running thread.
        /// </summary>
        private static readonly object threadLocker = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if the timer runs.
        /// </summary>
        public bool Running
        {
            get
            {
                return mRunning;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures that the timer runs in an asynchronous thread.
        /// </summary>
        public void EnsureRunTimerAsync()
        {
            if (!Running)
            {
                lock (threadLocker)
                {
                    if (!Running && !threadStarted)
                    {
                        thread = new CMSThread(Run);
                        thread.Start();
                        threadStarted = true;
                    }
                }
            }
        }


        /// <summary>
        /// Ensures performance counter timer.
        /// </summary>
        public static PerformanceCounterTimer EnsureTimer()
        {
            if (timer == null)
            {
                lock (threadLocker)
                {
                    if (timer == null)
                    {
                        // Create new timer object
                        timer = new PerformanceCounterTimer();
                    }
                }
            }

            return timer;
        }


        /// <summary>
        /// Starts the timer execution.
        /// </summary>
        public void Run()
        {
            try
            {
                mRunning = true;
                mCancel = false;

                while (!mCancel)
                {
                    // If Health monitoring is disabled or application interval is less or equals zero, stop thread
                    if (!HealthMonitoringHelper.LogCounters || (HealthMonitoringHelper.ApplicationMonitoringInterval <= 0))
                    {
                        // Stop thread
                        StopTimer();
                        break;
                    }

                    Execute();

                    Thread.Sleep(HealthMonitoringHelper.ApplicationMonitoringInterval * 1000);
                }
            }
            catch (ThreadAbortException abortEx)
            {
                // Log exception only when thread isn't aborted for reason
                if (!CMSThread.Stopped(abortEx))
                {
                    EventLogProvider.LogException("PerformanceCounterTimer", "Run", abortEx);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    EventLogProvider.LogException("PerformanceCounterTimer", "Run", ex);
                }
                catch
                {
                    // Unable to log the event
                }
            }
            finally
            {
                mRunning = false;
                mCancel = false;
                threadStarted = false;
            }
        }


        /// <summary>
        /// Stops the timer execution.
        /// </summary>
        public static void StopTimer()
        {
            // Cancel the timer execution
            if (timer != null)
            {
                timer.mCancel = true;
            }

            // Stop the thread
            if (thread != null)
            {
                thread.Stop();
            }
        }


        /// <summary>
        /// Logs to the counters.
        /// </summary>
        private void Execute()
        {
            // Log application data to counters
            HealthMonitoringLogHelper.LogApplicationCounters();
        }

        #endregion
    }
}