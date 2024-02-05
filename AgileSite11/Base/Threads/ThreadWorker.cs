using System;
using System.Security.Principal;
using System.Threading;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Worker processing actions in single (one per application and generic variant), ever-running asynchronous thread.
    /// </summary>
    public abstract class ThreadWorker<T> : IDisposable
        where T : ThreadWorker<T>, new()
    {
        #region "Variables"

        /// <summary>
        /// Maximum length of the worker log to prevent memory leaks
        /// </summary>
        protected const int LOG_MAX_LENGTH = 100 * 1024;


        /// <summary>
        /// Event indicating that the thread stopped
        /// </summary>
        internal ManualResetEvent ThreadStopped = new ManualResetEvent(false);


        /// <summary>
        /// Lock this object when doing actions that might change the result of condition run while stopping execution
        /// </summary>
        protected readonly object SyncRoot = new object();

        // Internal flag to control start of only single thread
        private bool mIsThreadRunning;


        private readonly object runLock = new object();

        private int mFinishExecuted;
        private int mInterval;
        private bool disposed = false;
        private bool mThreadFinishing;

        private Func<bool> mStoppingCondition;
        private bool mStopRequested;
        private bool mStop;
        private bool acquiredExecuteStepLock;

        // Worker object, each worker type has a singleton stored in this field. Other instances of the same worker type should not exist.
        private static T mWorker;
        private readonly static object mWorkerInitializationLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the maintenance interval in milliseconds for the worker. When 0 (default), the maintenance is not performed.
        /// </summary>
        protected virtual int MaintenanceInterval
        {
            get
            {
                return 0;
            }
        }


        /// <summary>
        /// If true, the thread uses a log context for its operations
        /// </summary>
        protected virtual bool UseLogContext
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Indicates whether worker routine is temporarily paused.
        /// </summary>
        /// <remarks>
        /// This property supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public bool ProcessingPaused
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets thread which is periodically checking for new tasks.  
        /// </summary>
        protected Thread PollThread
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the default interval in milliseconds for the worker.
        /// </summary>
        protected abstract int DefaultInterval
        {
            get;
        }


        /// <summary>
        /// Returns interval value used to run worker tasks periodically.
        /// </summary>
        private int Interval
        {
            get
            {
                if (mInterval == 0)
                {
                    var className = GetType().Name;

                    // Try to get interval value from web.config key
                    mInterval = SettingsHelper.AppSettings[className + "Interval"].ToInteger(0);

                    if (mInterval <= 0)
                    {
                        return DefaultInterval;
                    }
                }
                return mInterval;
            }
        }


        /// <summary>
        /// Current thread worker object
        /// </summary>
        public static T Current
        {
            get
            {
                if (mWorker == null)
                {
                    lock (mWorkerInitializationLock)
                    {
                        if (mWorker == null)
                        {
                            mWorker = new T();
                        }
                    }
                }

                return mWorker;
            }
        }


        /// <summary>
        /// Asynchronous log context used by the worker thread
        /// </summary>
        protected static ILogContext Log
        {
            get
            {
                return CoreServices.EventLog.CurrentLogContext;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Method processing actions.
        /// </summary>
        /// <remarks>If exception from override arises, the thread will not end, but exception will not be logged. Custom implementation of exception handling is required.</remarks>
        protected abstract void Process();


        /// <summary>
        /// Finishes the worker process.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implement this method to specify what the worker must do in order to not lose its internal data when being finished.
        /// </para>
        /// <para>
        /// Execution or completeness is not guaranteed. Do not use this method for time-consuming actions. Code execution can be forcibly interrupted after a short period of time. 
        /// </para>
        /// <para>
        /// The <see cref="Finish"/> method is called in following scenarios: 
        /// <list type="bullet">
        /// <item><description>Application is terminating and <see cref="ApplicationEvents.Finalize"/> event is called.</description></item>
        /// <item><description>Thread was aborted (for any reason).</description></item>
        /// <item><description><see cref="StopExecution(Func{bool})"/> method was called.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        protected abstract void Finish();


        /// <summary>
        /// Runs the maintenance routine for the worker
        /// </summary>
        /// <remarks>If exception from override arises, the thread will not end, but exception will not be logged. Custom implementation of exception handling is required.</remarks>
        protected virtual void DoMaintenance()
        {
            // No maintenance actions by default
        }


        /// <summary>
        /// Resets the instance to initial state.
        /// </summary>
        private void Reset()
        {
            mFinishExecuted = 0;

            mStopRequested = false;
            mStoppingCondition = null;
            mStop = false;
        }


        /// <summary>
        /// Returns next time maintenance should be scheduled.
        /// </summary>
        internal DateTime GetNextMaintenanceTime(DateTime now, DateTime scheduledMaintenance)
        {
            if (MaintenanceInterval <= 0)
            {
                return DateTime.MaxValue;
            }
            if (scheduledMaintenance > now)
            {
                // Schedules time should stay as is
                return scheduledMaintenance;
            }
            // Schedule next maintenance time
            return scheduledMaintenance.AddMilliseconds(MaintenanceInterval);
        }


        /// <summary>
        /// Returns time required to sleep
        /// </summary>
        internal TimeSpan GetTimeBeforeNextRun(DateTime now, DateTime expectedNextRunTime)
        {
            // Count the sleep time to the next iteration
            var sleepTime = expectedNextRunTime - now;
            if (sleepTime.TotalMilliseconds > 0)
            {
                return sleepTime;
            }
            return new TimeSpan(0);
        }


        /// <summary>
        /// Returns true if the worker thread is currently running
        /// </summary>
        /// <remarks>
        /// This method supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        public bool IsThreadRunning()
        {
            var thread = PollThread;
            return (thread != null) && thread.IsAlive && !mThreadFinishing;
        }


        /// <summary>
        /// Runs the task in an asynchronous thread.
        /// </summary>
        private void StartAsyncThread()
        {
            // This worker must always run in asynchronous mode to prevent hangs. Even if started from a thread.
            using (new CMSActionContext { AllowAsyncActions = true })
            {
                ThreadStopped = new ManualResetEvent(false);
                mThreadFinishing = false;

                var thr = CreateThread();

                // Do not allow debug for worker threads
                thr.DisableDebug = !SystemContext.DevelopmentMode;

                PollThread = thr.InnerThread;
                thr.Start();
            }
        }


        /// <summary>
        /// Creates <see cref="ThreadSettings"/> object and enables the <see cref="ThreadSettings.IsBackground"/> and <see cref="ThreadSettings.UseEmptyContext"/> properties.
        /// </summary>
        protected virtual ThreadSettings CreateThreadSettings()
        {
            return new ThreadSettings
            {
                CreateLog = UseLogContext,
                IsBackground = true, // Run thread as a background thread so that it can be terminated within a process exit (does not prevent console to finish)
                UseEmptyContext = true // Use empty context to always start as a clean thread, not influenced by the initiating thread / request
            };
        }


        /// <summary>
        /// Creates a worker thread object. Override to modify the thread configuration
        /// </summary>
        private CMSThread CreateThread()
        {
            var settings = CreateThreadSettings();

            var thr = new CMSThread(() => Run(WindowsIdentity.GetCurrent()), settings);

            // Provide more appropriate delegate info for the target action
            thr.LoadTargetDelegateInfo((Action<WindowsIdentity>)Run);
            thr.OnStop += (object sender, EventArgs e) => ThreadStopped.Set();

            return thr;
        }


        /// <summary>
        /// Runs the action under given identity.
        /// </summary>
        /// <param name="wi">Windows identity</param>
        private void Run(WindowsIdentity wi)
        {
            WindowsImpersonationContext ctx = null;

            try
            {
                ApplicationEvents.Finalize.Execute += OnFinishHandler;

                ctx = wi.Impersonate();

                Initialize();

                Run();

            }
            finally
            {
                FinalizeThread(ctx);
            }
        }


        private bool CheckStop()
        {
            if (mStop)
            {
                // Stop already detected
                return true;
            }

            try
            {
                // The acquired lock may be released by unfulfilled stop request or within FinalizeThread method in case of ThreadAbortException or fulfilled stop request
                Monitor.Enter(SyncRoot, ref acquiredExecuteStepLock);

                // Stop requested, but depends on condition
                if (mStopRequested && ((mStoppingCondition == null) || mStoppingCondition()))
                {
                    mStop = true;
                    return true;
                };
            }
            finally
            {
                // Lock will be released within FinalizeThread method if stop was request was fulfilled
                if (!mStop && acquiredExecuteStepLock)
                {
                    acquiredExecuteStepLock = false;
                    Monitor.Exit(SyncRoot);
                }
            }

            return false;
        }


        private void FinalizeThread(WindowsImpersonationContext ctx)
        {
            try
            {
                mThreadFinishing = true;

                ApplicationEvents.Finalize.Execute -= OnFinishHandler;

                RunFinish();
            }
            finally
            {

                mIsThreadRunning = false;
                if (acquiredExecuteStepLock)
                {
                    acquiredExecuteStepLock = false;
                    Monitor.Exit(SyncRoot);
                }

                if (ctx != null)
                {
                    ctx.Undo();
                }
            }
        }


        /// <summary>
        /// Executes specified actions with stop request check before each step and after all steps
        /// </summary>
        /// <returns><c>true</c> if can continue; otherwise <c>false</c></returns>
        private bool ExecuteCancellableSteps(params Action[] steps)
        {
            foreach (var step in steps)
            {
                if (CheckStop())
                {
                    return false;
                }

                if (!ProcessingPaused)
                {
                    step();
                }
            }

            return !CheckStop();
        }


        /// <summary>
        /// Runs the action.
        /// </summary>
        private void Run()
        {
            var nextMaintenance = DateTime.Now.AddMilliseconds(MaintenanceInterval);
            var expectedNextRunTime = DateTime.Now;

            while (true)
            {
                var canContinue = ExecuteCancellableSteps(
                    RunProcess,
                    () => PerformMaintenance(ref nextMaintenance)
                );

                if (!canContinue)
                {
                    break;
                }

                WaitUntilNextRun(ref expectedNextRunTime);
            }
        }


        private void WaitUntilNextRun(ref DateTime expectedNextRunTime)
        {
            // Recalculate next expected time to process
            expectedNextRunTime = expectedNextRunTime.AddMilliseconds(Interval);

            // Sleep between runs
            Thread.Sleep(GetTimeBeforeNextRun(DateTime.Now, expectedNextRunTime));
        }


        private void PerformMaintenance(ref DateTime nextMaintenance)
        {
            var now = DateTime.Now;

            if (nextMaintenance >= now)
            {
                return;
            }

            try
            {
                DoMaintenance();
            }
            catch
            {
                // Thread should not die after exception. Exception handling should be implemented in DoMaintenance method override.
            }
            finally
            {
                nextMaintenance = GetNextMaintenanceTime(now, nextMaintenance);
            }
        }


        /// <summary>
        /// Runs the internal process of the worker
        /// </summary>
        protected void RunProcess()
        {
            bool lockTaken = false;

            try
            {
                // Try obtain lock
                Monitor.TryEnter(runLock, 0, ref lockTaken);

                if (lockTaken)
                {
                    Process();
                }
            }
            catch
            {
                // Thread should not die after exception. Exception handling should be implemented in Process method override.
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(runLock);
                }
            }
        }


        /// <summary>
        /// Initializes the worker. Runs in the worker thread before the thread processes the first iteration.
        /// </summary>
        protected virtual void Initialize()
        {
            // No default actions
        }


        /// <summary>
        /// Ensures a running thread for this processor
        /// </summary>
        public void EnsureRunningThread()
        {
            if (mIsThreadRunning)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (mIsThreadRunning)
                {
                    return;
                }

                // The flag is set here to prevent concurrency because a started thread which fails fast could set it back to false too soon, and result would be incorrectly true
                mIsThreadRunning = true;

                Reset();

                try
                {
                    StartAsyncThread();
                }
                catch
                {
                    // Reset the flag in case the thread was not able to start for some reason
                    mIsThreadRunning = false;
                    throw;
                }
            }
        }


        /// <summary>
        /// Stops the worker after finishing its job.
        /// </summary>
        /// <remarks>
        /// You should use parameter <paramref name="condition"/> to make sure that it is safe to stop the worker and no data are lost. 
        /// </remarks>
        /// <param name="condition">Enables you to cancel stopping the worker by returning false from the function.</param>
        protected void StopExecution(Func<bool> condition = null)
        {
            lock (SyncRoot)
            {
                if (mThreadFinishing)
                {
                    return;
                }

                mStopRequested = true;
                mStoppingCondition = condition;
            }
        }


        /// <summary>
        /// Runs the finish action
        /// </summary>
        private void RunFinish()
        {
            if (Interlocked.Exchange(ref mFinishExecuted, 1) == 0)
            {
                Finish();
            }
        }


        private void OnFinishHandler(object sender, EventArgs e)
        {
            lock (SyncRoot)
            {
                if (!mThreadFinishing)
                {
                    mThreadFinishing = true;
                    mStopRequested = true;
                    mStoppingCondition = null;
                    RunFinish();
                }
            }
        }


        /// <summary>
        /// Disposes instance of thread worker.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Disposes instance of thread worker.
        /// </summary>
        /// <param name="disposing">Indicates to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                ThreadStopped.Dispose();
            }
            disposed = true;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Static constructor
        /// </summary>
        static ThreadWorker()
        {
            TypeManager.RegisterGenericType(typeof(ThreadWorker<T>));
        }

        #endregion
    }
}