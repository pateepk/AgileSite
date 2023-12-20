using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Wrapper class for <see cref="Thread"/> object with additional debug, log and context item possibilities.
    /// </summary>
    /// <remarks>
    /// <see cref="CMSThread"/> is able to copy context items from thread where the instance of <see cref="CMSThread"/> was created. Context items are based on <see cref="AbstractContext{TContext}"/> class.
    /// </remarks>
    public sealed class CMSThread : AbstractWorker
    {
        #region "Constants"

        /// <summary>
        /// Thread was stopped by the user.
        /// </summary>
        public const string ABORT_REASON_STOP = "stop";

        #endregion


        #region "Variables"

        // Lookup table for the threads.
        private static readonly SafeDictionary<Guid, CMSThread> mThreads = new SafeDictionary<Guid, CMSThread>();

        // Thread start method.
        private readonly ThreadStart mThreadStart;

        // Thread to wait for.
        private Thread mWaitFor;

        // Gets or sets the last thread in the ordered sequence.
        private static readonly RequestStockValue<Thread> mLastSequenceThread = new RequestStockValue<Thread>("LastSequenceThread");

        // Guid of the original log context. The one before CMSThread create it's own.
        private ILogContext mOriginalLogContext;

        // Object for locking the lookup table for the threads
        private static readonly object lockHashObject = new object();

        // Object for locking thread start
        private readonly object startLock = new object();

        private static bool mNewThreadsAllowed = true;
        private ThreadModeEnum mMode = ThreadModeEnum.Async;
        private static IPerformanceCounter mRunningThreads;

        /// <summary>
        /// String constant used to store Thread GUID in <see cref="ThreadAbortData"/>.
        /// </summary>
        internal const string THREAD_GUID_ABORT_DATA_NAME = "ThreadGUID";

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the thread  was aborted
        /// </summary>
        private bool Aborted
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether debug should be disabled for current thread
        /// </summary>
        internal bool DisableDebug
        {
            get;
            set;
        }


        /// <summary>
        /// Current debug item
        /// </summary>
        internal ThreadDebugItem DebugItem
        {
            get;
            set;
        }


        /// <summary>
        /// Counter of running Threads.
        /// </summary>
        public static IPerformanceCounter RunningThreads
        {
            get
            {
                if (mRunningThreads == null)
                {
                    Interlocked.CompareExchange(ref mRunningThreads, Service.Resolve<IPerformanceCounter>(), null);
                }
                return mRunningThreads;
            }
        }


        /// <summary>
        /// Connection string name that the thread should use to access the database
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }


        /// <summary>
        /// Worker object.
        /// </summary>
        internal Thread InnerThread
        {
            get;
            private set;
        }


        /// <summary>
        /// Thread ID.
        /// </summary>
        public int ThreadID
        {
            get;
            private set;
        }


        /// <summary>
        /// Thread GUID.
        /// </summary>
        public Guid ThreadGUID
        {
            get;
            private set;
        }


        /// <summary>
        /// Time when the thread started.
        /// </summary>
        public DateTime ThreadStarted
        {
            get;
            private set;
        }


        /// <summary>
        /// Time when the thread finished.
        /// </summary>
        public DateTime ThreadFinished
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the method which is running the thread actions.
        /// </summary>
        internal string MethodName
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the class which is running the thread actions.
        /// </summary>
        internal string MethodClassName
        {
            get;
            private set;
        }


        /// <summary>
        /// Request URL which created the thread.
        /// </summary>
        internal string RequestUrl
        {
            get;
            private set;
        }


        /// <summary>
        /// Logs for long running operations.
        /// </summary>
        public ILogContext Log
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if new threads can be created within this thread. (By default no new threads are created.)
        /// </summary>
        public bool AllowAsyncActions
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the thread runs synchronously
        /// </summary>
        public ThreadModeEnum Mode
        {
            get
            {
                return mMode;
            }
            private set
            {
                mMode = value;
            }
        }


        /// <summary>
        /// Thread context
        /// </summary>
        internal ThreadContext Context
        {
            get;
            set;
        }


        /// <summary>
        /// Defines if a thread is a background thread
        /// </summary>
        public bool IsBackground
        {
            get
            {
                return Settings.IsBackground;
            }
        }


        /// <summary>
        /// Specifies the scheduling priority.
        /// </summary>
        public ThreadPriority Priority
        {
            get
            {
                return Settings.Priority;
            }
        }


        /// <summary>
        /// Thread settings
        /// </summary>
        private ThreadSettings Settings
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of <see cref="CMSThread"/>.
        /// </summary>
        /// <param name="start">Thread start method</param>
        /// <param name="createLog">Indicates whether thread should create it's own log.</param>
        /// <param name="mode">Thread mode</param>
        public CMSThread(ThreadStart start, bool createLog = false, ThreadModeEnum mode = ThreadModeEnum.Async)
            : this(start, new ThreadSettings { CreateLog = createLog, Mode = mode })
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="CMSThread"/>.
        /// </summary>
        /// <param name="start">A <see cref="ThreadStart"/> delegate that represents the methods to be invoked when this thread begins executing.</param>
        /// <param name="settings">Settings object with required behavior of <see cref="CMSThread"/> object. The <paramref name="settings"/> object must not be modified.</param>
        public CMSThread(ThreadStart start, ThreadSettings settings)
        {
            Settings = settings;

            SetThreadMode(Settings.Mode);

            ThreadGUID = Guid.NewGuid();

            if (!Settings.CreateLog && (Mode != ThreadModeEnum.Wrapper))
            {
                Log = CoreServices.EventLog.CurrentLogContext;
            }

            LoadTargetDelegateInfo(start);
            mThreadStart = start;

            // Create inner thread based on mode value
            InnerThread = (Mode == ThreadModeEnum.Async) ? CreateInnerThread() : Thread.CurrentThread;

            lock (lockHashObject)
            {
                // If no new threads are allowed, set the thread immediately as aborted so that it doesn't start
                if (!mNewThreadsAllowed)
                {
                    Aborted = true;
                }

                mThreads[ThreadGUID] = this;
            }
        }


        /// <summary>
        /// Sets the <see cref="Mode"/> property value based on <paramref name="initialMode"/> and <see cref="CMSActionContext.CurrentAllowAsyncActions"/>.
        /// </summary>
        /// <param name="initialMode">Requested thread mode.</param>
        private void SetThreadMode(ThreadModeEnum initialMode)
        {
            // Fallback of async mode to sync in case async actions are node allowed
            var mode = initialMode;

            if ((mode == ThreadModeEnum.Async) && !CMSActionContext.CurrentAllowAsyncActions)
            {
                mode = ThreadModeEnum.Sync;
            }

            Mode = mode;
        }


        /// <summary>
        /// Creates a new inner thread
        /// </summary>
        private Thread CreateInnerThread()
        {
            var thr = new Thread(Run)
            {
                // Setup the thread
                Priority = Priority,
                IsBackground = IsBackground,

                // Adopt current thread culture
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };

            return thr;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the current thread ID
        /// </summary>
        public static int GetCurrentThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }


        /// <summary>
        /// Sets the target delegate information
        /// </summary>
        public void LoadTargetDelegateInfo(Delegate del)
        {
            // Try to get the executing method name
            try
            {
                var target = del.Target;
                if (target != null)
                {
                    // Thread on specific object instance
                    MethodClassName = target.GetType().FullName;
                    MethodName = del.Method.Name;
                }
                else
                {
                    // Thread on static method
                    var method = del.Method;
                    if (method.DeclaringType != null)
                    {
                        MethodClassName = method.DeclaringType.FullName;
                    }

                    MethodName = method.Name;
                }
            }
            catch
            {
                // Suppress exception because these information are only optional for debug and log purposes (we don't want to kill thread if error occurs here)
            }
        }


        /// <summary>
        /// Blocks the calling thread until a thread terminates, while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <seealso cref="Thread.Join()"/>
        public void Join()
        {
            Join(-1);
        }


        /// <summary>
        /// Blocks the calling thread until the thread represented by this instance terminates or the specified time elapses,
        /// while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for the thread to terminate.</param>
        /// <returns>True if the thread has terminated. False if the thread has not terminated after the amount of time specified by the <paramref name="millisecondsTimeout"/> parameter has elapsed.</returns>
        /// <seealso cref="Thread.Join(int)"/>
        internal bool Join(int millisecondsTimeout)
        {
            if (Mode != ThreadModeEnum.Async)
            {
                // Synchronous threads do all the work right in the Start call, there's nothing to wait for after that.
                return true;
            }

            var thr = InnerThread;
            if (thr != null)
            {
                return thr.Join(millisecondsTimeout);
            }

            return true;
        }


        /// <summary>
        /// Starts the thread.
        /// </summary>
        /// <param name="sequence">If true, the thread is a part of the sequence and should perform the actions after the previous thread finishes</param>
        public void Start(bool sequence = false)
        {
            InitializeThread(sequence);

            if (Mode == ThreadModeEnum.Async)
            {
                if (!Aborted)
                {
                    lock (startLock)
                    {
                        if (!Aborted)
                        {
                            InnerThread.Start();
                        }
                    }
                }
            }
            else if (!Aborted)
            {
                Run();
            }
        }


        private void InitializeThread(bool sequence)
        {
            // Handle the event
            using (var h = ThreadEvents.Init.StartEvent(this))
            {
                if (h.CanContinue())
                {
                    ThreadStarted = DateTime.Now;

                    if (Mode == ThreadModeEnum.Async)
                    {
                        // Pickup new thread ID in async mode
                        ThreadID = InnerThread.ManagedThreadId;

                        // Setup waiting for sequence
                        if (sequence)
                        {
                            mWaitFor = mLastSequenceThread;
                            mLastSequenceThread.Value = InnerThread;
                        }
                    }
                    else
                    {
                        // Pick up current thread ID in sync / wrapper modes
                        ThreadID = GetCurrentThreadId();
                    }

                    if (Mode != ThreadModeEnum.Sync)
                    {
                        // Open the context for newly identified threads
                        if (!Settings.UseEmptyContext)
                        {
                            // Prepare context utilizing current thread context
                            PrepareThreadContext();
                        }
                        else
                        {
                            PrepareEmptyThreadContext();
                        }
                    }
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Raises a <see cref="ThreadAbortException"/> in the thread on which it is invoked,
        ///  to begin the process of terminating the thread. Calling this method usually terminates the thread.
        /// </summary>
        /// <remarks>In case of <see cref="ThreadModeEnum.Sync"/> this method also kills all CMSThreads running within the same thread.</remarks>
        /// <seealso cref="Stop"/>
        public void Abort()
        {
            InnerThread.Abort();
        }


        /// <summary>
        /// Stops all running threads
        /// </summary>
        /// <param name="allowNewThreads">If true, new threads are </param>
        /// <param name="waitForAbort">If true, the process waits for all threads to be finished</param>
        internal static void StopAllThreads(bool allowNewThreads, bool waitForAbort)
        {
            const int THREAD_JOIN_TIMEOUT_MILLIS = 200 * 60 * 1000;

            List<CMSThread> threadsToKill;

            lock (lockHashObject)
            {
                if (!allowNewThreads)
                {
                    mNewThreadsAllowed = false;
                }

                threadsToKill = mThreads.TypedValues.ToList();
            }

            foreach (var thread in threadsToKill)
            {
                thread.Stop();
            }

            if (waitForAbort)
            {
                foreach (var thread in threadsToKill)
                {
                    if (!thread.Join(THREAD_JOIN_TIMEOUT_MILLIS))
                    {
                        ThrowThreadNotJoined(thread, THREAD_JOIN_TIMEOUT_MILLIS);
                    }
                }
            }
        }


        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> for <see cref="CMSThread"/> that has failed to terminate within <paramref name="millisecondsTimeout"/> period.
        /// </summary>
        private static void ThrowThreadNotJoined(CMSThread thread, int millisecondsTimeout)
        {
            var innerThread = thread.InnerThread;
            var threadName = innerThread == null ? "[unknown]" : innerThread.Name;

            throw new InvalidOperationException(String.Format("CMSThread '{0}' (ID: {1}, GUID: {2}) failed to terminate within {3}ms timeout. Thread's start method is '{4}.{5}'",
                threadName, thread.ThreadID, thread.ThreadGUID, millisecondsTimeout, thread.MethodClassName, thread.MethodName));
        }


        /// <summary>
        /// Stops the thread execution.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The thread is aborted but <seealso cref="ThreadAbortException"/> is not logged.
        /// </para>
        /// <para>
        /// In case of <seealso cref="ThreadModeEnum.Sync"/> mode this method only stops this and child synchronous threads. 
        /// </para>
        /// <para>
        /// Parent threads running under same InnerThread will continue running.
        /// </para>
        /// </remarks>
        /// <seealso cref="Abort"/>
        public void Stop()
        {
            bool abortRequired = false;

            if (!Aborted)
            {
                lock (startLock)
                {
                    if (!Aborted)
                    {
                        // Mark thread as aborted so that it doesn't start if not started yet
                        Aborted = true;
                        abortRequired = true;
                    }
                }
            }

            if (abortRequired && InnerThread != null)
            {
                // Abort the thread with special data to mark intentionally stopped thread
                var data = new ThreadAbortData();
                data[THREAD_GUID_ABORT_DATA_NAME] = ThreadGUID;

                InnerThread.Abort(data);
            }
        }




        /// <summary>
        /// Returns true if thread was aborted with ABORT_REASON_STOP state info e.g. Thread.Abort(CMSThread.ABORT_REASON_STOP);
        /// </summary>
        /// <param name="exception">The thread abort exception.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/></exception>
        public static bool Stopped(ThreadAbortException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            return (exception.ExceptionState != null) && (exception.ExceptionState.ToString() == ABORT_REASON_STOP);
        }


        /// <summary>
        /// Runs the worker as a new thread. For internal purposes only, use method Start instead.
        /// </summary>
        public override void RunAsync()
        {
            Start(RunInSequence);
        }


        /// <summary>
        /// Runs the action.
        /// </summary>
        public override void Run()
        {
            try
            {
                RunningThreads.Increment(null);

                // Set the context containers
                var isSyncMode = (Mode == ThreadModeEnum.Sync);
                if (!isSyncMode && (Context != null))
                {
                    Context.SetAsCurrent();
                }

                if (DisableDebug)
                {
                    DebugHelper.DisableDebug();
                }

                // Deny asynchronous actions because already running in separate thread
                // When thread is synchronous and context allows creating async threads, allow creating async threads for the child threads
                bool allowAsyncActions = (isSyncMode && CMSActionContext.CurrentAllowAsyncActions) || AllowAsyncActions;

                using (var context = new CMSActionContext())
                {
                    // Disable restoring of ActionContext when running async. Async thread has it's own copy of ActionContext that does not affect the original.
                    if (!isSyncMode)
                    {
                        context.RestoreOriginal = false;
                    }

                    // Enable the use of LogContext when thread is creating it's own Log and does not use inherited LogContext.
                    if (Settings.CreateLog)
                    {
                        context.EnableLogContext = true;
                    }

                    context.AllowAsyncActions = allowAsyncActions;
                    context.ThreadGuid = ThreadGUID;

                    try
                    {
                        // Set the URL which started the thread
                        RequestUrl = DebugHelper.GetRequestUrl();

                        // Register the thread
                        ThreadDebug.ThreadStarted(this);

                        if (Settings.CreateLog)
                        {
                            // Backup original log
                            mOriginalLogContext = CoreServices.EventLog.CurrentLogContext;

                            // Create new log
                            Log = CoreServices.EventLog.EnsureLog(Guid.NewGuid());
                        }

                        RunThread();
                    }
                    catch (ThreadAbortException abortEx)
                    {
                        LogThreadAbort(abortEx);
                    }
                    catch (Exception ex)
                    {
                        LogThreadError(ex);
                    }
                    finally
                    {
                        if (Settings.CreateLog && (Log != null))
                        {
                            CoreServices.EventLog.CloseLog(Log.LogGuid);

                            // Restore original log
                            if (mOriginalLogContext != null)
                            {
                                CoreServices.EventLog.EnsureLog(mOriginalLogContext.LogGuid);
                            }
                        }
                    }
                }
            }
            finally
            {
                FinalizeThread();
            }
        }


        /// <summary>
        /// Logs a general thread error
        /// </summary>
        /// <param name="ex">Exception to log</param>
        private void LogThreadError(Exception ex)
        {
            var sourceName = GetMethodClassName();

            CoreServices.EventLog.LogException(sourceName, "RUN", ex);
        }


        /// <summary>
        /// Logs the thread abort exception
        /// </summary>
        /// <param name="abortEx">Abort exception</param>
        private void LogThreadAbort(ThreadAbortException abortEx)
        {
            // Do not log abort for background threads
            if (IsBackground)
            {
                return;
            }

            // Log exception only when thread isn't aborted for reason
            if (!Stopped(abortEx))
            {
                string sourceName = GetMethodClassName();

                CoreServices.EventLog.LogException(sourceName, "RUN", abortEx);
            }
        }


        private string GetMethodClassName()
        {
            string methodClassName = String.Empty;

            try
            {
                methodClassName = MethodClassName.Substring(MethodClassName.LastIndexOf('.') + 1);
            }
            catch
            {
                // Suppress exception because this value is not required (it is for debug purposes only) 
            }

            return methodClassName;
        }


        /// <summary>
        /// Runs the thread
        /// </summary>
        [HideFromDebugContext]
        private void RunThread()
        {
            if (Mode != ThreadModeEnum.Sync)
            {
                if (!DisableDebug)
                {
                    DebugHelper.RegisterLogs();
                }
            }

            if (mWaitFor != null)
            {
                mWaitFor.Join();
            }

            using (var h = ThreadEvents.Run.StartEvent(this))
            {
                try
                {
                    mThreadStart();
                }
                catch (ThreadAbortException ex)
                {
                    // Do not allow abort of main thread in case of synchronous mode (to allow it to continue)
                    ThreadAbortData data = ex.ExceptionState as ThreadAbortData;
                    if ((Mode == ThreadModeEnum.Sync) && (data != null))
                    {
                        Guid cancelThreadGuid = CoreServices.Conversion.GetGuid(data[THREAD_GUID_ABORT_DATA_NAME], Guid.Empty);

                        if (ThreadGUID == cancelThreadGuid)
                        {
                            Thread.ResetAbort();
                        }
                    }
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Finalizes the thread run
        /// </summary>
        private void FinalizeThread()
        {
            RunWithErrorHandler(RaiseStop);
            RunWithErrorHandler(() => ThreadEvents.Finalize.StartEvent(this));

            ThreadFinished = DateTime.Now;

            RunWithErrorHandler(() => ThreadDebug.ThreadFinished(this));
            RunWithErrorHandler(() => RunningThreads.Decrement(null));

            lock (lockHashObject)
            {
                mThreads.Remove(ThreadGUID);
            }
        }


        private void RunWithErrorHandler(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                try
                {
                    CoreServices.EventLog.LogException("CMSThread", "FINISH", ex);
                }
                catch
                {
                    // Suppress exception because we need to process all required methods (see method FinalizeThread())
                }
            }
        }


        /// <summary>
        /// Finds the thread based on the given GUID.
        /// </summary>
        /// <param name="threadGuid">Thread GUID</param>
        public static CMSThread GetThread(Guid threadGuid)
        {
            lock (lockHashObject)
            {
                return mThreads[threadGuid];
            }
        }

        #endregion


        #region "Thread context methods"

        /// <summary>
        /// Prepares empty thread context for thread with <see cref="ThreadSettings.UseEmptyContext"/> set.
        /// </summary>
        private void PrepareEmptyThreadContext()
        {
            Context = new ThreadContext();

            AllowEmptyContext(this);
        }


        /// <summary>
        /// Prepares the thread context
        /// </summary>
        private void PrepareThreadContext()
        {
            var context = Context;
            if (context == null)
            {
                // Copy items from current thread
                context = CopyContext();
            }
            else if (context.MultipleThreads)
            {
                // In case that items are shared between multiple threads, copy current state for the new thread to keep the original intact
                context = CopyContext(context);
            }
            else if (context.ParentThreadID != 0)
            {
                LogErrorForAlreadyUsedContext();

                // In case that items are already used by some thread, copy current state for the new thread to keep the original intact
                context = CopyContext(context);
            }

            context.SetParentThread(ThreadID, GetType());

            Context = context;
        }


        private static void LogErrorForAlreadyUsedContext()
        {
            if (SystemContext.DiagnosticLogging)
            {
                try
                {
                    var message = new StringBuilder();

                    message.AppendLine("Thread context that was already used by another thread cannot be used by the new thread, it may not be in original / deterministic state. To avoid this, call CMSThread.Wrap with parameter multipleThreads set to true to pass a snapshot of original context to all such threads.");
                    message.AppendLine("Stack trace:");
                    message.Append(new StackTrace());

                    CoreServices.EventLog.LogEvent("E", "Thread", "OPENCONTEXT", message.ToString());
                }
                catch
                {
                    // Ignore error caused by database unavailability
                }
            }
        }


        /// <summary>
        /// Creates a copy of the given thread context. If the context is not given, creates a copy of current context
        /// </summary>
        /// <param name="context">Thread context</param>
        private ThreadContext CopyContext(ThreadContext context = null)
        {
            if (context == null)
            {
                context = ThreadContext.Current;
            }

            return context.CopyForNewThread();
        }

        #endregion


        #region "Anonymous wrapper methods"

        /// <summary>
        /// Executes the given code as it was executed in a context of CMSThread. Use this method to wrap a code, that runs from anonymous thread that you have no control over. 
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="sourceContext">Source context of the originating thread</param>
        /// <param name="originalAction">Original action</param>
        private static void Execute(Action action, ThreadContext sourceContext, Delegate originalAction)
        {
            // Wrap the action to a synchronous CMSThread object
            var thr = new CMSThread(new ThreadStart(action), false, ThreadModeEnum.Wrapper)
            {
                Context = sourceContext
            };

            // Set the delegate info
            thr.LoadTargetDelegateInfo(originalAction);

            // Start the thread
            thr.Start();
        }


        /// <summary>
        /// Executes the given function as it was executed in a context of CMSThread. Use this method to wrap a code, that runs from anonymous thread that you have no control over. 
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="sourceContext">Source context items of the originating thread</param>
        /// <param name="originalAction">Original action</param>
        private static TResult ExecuteFunc<TResult>(Func<TResult> action, ThreadContext sourceContext, Delegate originalAction)
        {
            TResult result = default(TResult);

            Execute(() => result = action(), sourceContext, originalAction);

            return result;
        }


        /// <summary>
        /// Allows anonymous thread to use new dedicated context in subsequent calls.
        /// Call this method in case the anonymous thread isn't initiated from a request thread, or in case you want to start with an empty thread context on purpose.
        /// </summary>
        /// <param name="thread">Thread for which to allow the empty context. Pass null for current thread.</param>
        public static void AllowEmptyContext(CMSThread thread = null)
        {
            ThreadItems.EnsureItems(thread);
        }


        /// <summary>
        /// Wraps the given method into CMSThread context
        /// </summary>
        /// <param name="action">Action to wrap</param>
        /// <param name="multipleThreads">If true, the wrapped method may be used by multiple threads. Use this parameter to ensure that all the threads receive the original context values.</param>
        public static Action<T1, T2> Wrap<T1, T2>(Action<T1, T2> action, bool multipleThreads = false)
        {
            var ctx = ThreadContext.GetContextForNewThread(multipleThreads);

            return (p1, p2) => Execute(() => action(p1, p2), ctx, action);
        }


        /// <summary>
        /// Wraps the given method into CMSThread context
        /// </summary>
        /// <param name="action">Action to wrap</param>
        /// <param name="multipleThreads">If true, the wrapped method may be used by multiple threads. Use this parameter to ensure that all the threads receive the original context values.</param>
        public static Action<T> Wrap<T>(Action<T> action, bool multipleThreads = false)
        {
            var ctx = ThreadContext.GetContextForNewThread(multipleThreads);

            return p => Execute(() => action(p), ctx, action);
        }


        /// <summary>
        /// Wraps the given method into CMSThread context
        /// </summary>
        /// <param name="action">Action to wrap</param>
        /// <param name="multipleThreads">If true, the wrapped method may be used by multiple threads. Use this parameter to ensure that all the threads receive the original context values.</param>
        public static Action Wrap(Action action, bool multipleThreads = false)
        {
            var ctx = ThreadContext.GetContextForNewThread(multipleThreads);

            return () => Execute(action, ctx, action);
        }


        /// <summary>
        /// Wraps the given method into CMSThread context
        /// </summary>
        /// <param name="action">Action to wrap</param>
        /// <param name="multipleThreads">If true, the wrapped method may be used by multiple threads. Use this parameter to ensure that all the threads receive the original context values.</param>
        public static Func<T1, TResult> WrapFunc<T1, TResult>(Func<T1, TResult> action, bool multipleThreads = false)
        {
            var ctx = ThreadContext.GetContextForNewThread(multipleThreads);

            return p1 => ExecuteFunc(() => action(p1), ctx, action);
        }


        /// <summary>
        /// Wraps the given method into CMSThread context
        /// </summary>
        /// <param name="action">Action to wrap</param>
        /// <param name="multipleThreads">If true, the wrapped method may be used by multiple threads. Use this parameter to ensure that all the threads receive the original context values.</param>
        public static Func<TResult> WrapFunc<TResult>(Func<TResult> action, bool multipleThreads = false)
        {
            var ctx = ThreadContext.GetContextForNewThread(multipleThreads);

            return () => ExecuteFunc(action, ctx, action);
        }

        #endregion
    }
}