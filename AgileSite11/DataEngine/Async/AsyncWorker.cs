using System;
using System.Security.Principal;
using System.Threading;

using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    #region "Delegates and events"

    /// <summary>
    /// Action event delegate.
    /// </summary>
    public delegate void AsyncAction(object parameter);

    #endregion


    /// <summary>
    /// Worker class for asynchronous operations.
    /// </summary>
    public class AsyncWorker
    {
        #region "Variables"
        
        /// <summary>
        /// Action to run.
        /// </summary>
        private AsyncAction mAction;

        /// <summary>
        /// Async thread.
        /// </summary>
        private CMSThread mThread;

        /// <summary>
        /// Last exception.
        /// </summary>
        private Exception mLastException;

        /// <summary>
        /// Process GUID.
        /// </summary>
        private Guid mProcessGUID = Guid.Empty;


        /// <summary>
        /// Windows identity.
        /// </summary>
        private WindowsIdentity mWindowsIdentity;

        /// <summary>
        /// If true, the process waits for finishing.
        /// </summary>
        private bool mWaitForFinish;

        /// <summary>
        /// Async process data
        /// </summary>
        private AsyncProcessData mProcessData;

        #endregion


        #region "Delegates and events"

        /// <summary>
        /// On error event handler.
        /// </summary>
        public event EventHandler OnError;


        /// <summary>
        /// On finished event handler.
        /// </summary>
        public event EventHandler OnFinished;

        #endregion


        #region "Properties"

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
        /// Running thread of the worker
        /// </summary>
        public CMSThread Thread
        {
            get
            {
                return mThread;
            }
        }


        /// <summary>
        /// Worker status.
        /// </summary>
        public AsyncWorkerStatusEnum Status
        {
            get
            {
                return ProcessData.Status;
            }
            private set
            {
                ProcessData.Status = value;
                ProcessData.SaveToPersistentMedium();
            }
        }


        /// <summary>
        /// Last exception.
        /// </summary>
        public Exception LastException
        {
            get
            {
                return mLastException;
            }
        }


        /// <summary>
        /// Process GUID.
        /// </summary>
        public Guid ProcessGUID
        {
            get
            {
                return mProcessGUID;
            }
            set
            {
                mProcessGUID = value;
            }
        }


        /// <summary>
        /// Parameter.
        /// </summary>
        public string Parameter
        {
            get
            {
                return ProcessData.Parameter;
            }
            set
            {
                ProcessData.Parameter = value;
            }
        }
        
        #endregion


        #region "Methods"
        
        /// <summary>
        /// Resets the worker.
        /// </summary>
        public void Reset()
        {
            if (Status == AsyncWorkerStatusEnum.Running)
            {
                throw new Exception("[AsyncWorker.RunAsync]: Cannot reset running action.");
            }
            
            Status = AsyncWorkerStatusEnum.Error;
            
            mAction = null;
            mThread = null;
            mLastException = null;
        }


        /// <summary>
        /// Stops the process.
        /// </summary>
        public void Stop()
        {
            if (mThread != null)
            {
                mThread.Stop();
                mThread = null;
            }

            Status = AsyncWorkerStatusEnum.Stopped;
        }


        /// <summary>
        /// Sets the process to wait until the process finishes (sets the waiting flag). Call from the OnFinished event handler.
        /// </summary>
        public void WaitForFinish()
        {
            mWaitForFinish = true;
        }


        /// <summary>
        /// Resets the waiting flag for the operation finish.
        /// </summary>
        public void HasFinished()
        {
            mWaitForFinish = false;
        }


        /// <summary>
        /// Runs the action in an asynchronous thread.
        /// </summary>
        /// <param name="action">Action callback to run</param>
        /// <param name="wi">Windows identity</param>
        public void RunAsync(AsyncAction action, WindowsIdentity wi)
        {
            // Store windows identity
            mWindowsIdentity = wi;

            if (Status == AsyncWorkerStatusEnum.Running)
            {
                throw new Exception("[AsyncWorker.RunAsync]: Worker is currently running.");
            }

            Status = AsyncWorkerStatusEnum.Running;
            mAction = action;

            ThreadStart threadStart = Run;

            // Allow threads creation to prevent asynchronous log blocking
            var thr = new CMSThread(threadStart) { AllowAsyncActions = true };

            // Provide more appropriate delegate info for the target action
            thr.LoadTargetDelegateInfo(action);

            thr.Start();

            mThread = thr;
        }


        /// <summary>
        /// Attaches to existing thread
        /// </summary>
        /// <param name="thread">Thread</param>
        public void AttachToThread(CMSThread thread)
        {
            // CMSThread is able to run synchronously. Thats why we can't check whether inner thread is running.
            // It will be running even after this CMSThread is finished.
            if (thread.ThreadFinished != DateTime.MinValue)
            {
                Status = AsyncWorkerStatusEnum.Finished;
            }
            else
            {
                Status = AsyncWorkerStatusEnum.Running;

                thread.OnStop += (sender, eventArgs) =>
                {
                    Status = AsyncWorkerStatusEnum.Finished;
                };

                mThread = thread;
            }
        }


        /// <summary>
        /// Runs the action.
        /// </summary>
        private void Run()
        {
            AsyncWorkerStatusEnum finalStatus = AsyncWorkerStatusEnum.Stopped;

            // Impersonation context
            WindowsImpersonationContext ctx = null;
            try
            {
                // Impersonate current thread
                ctx = mWindowsIdentity.Impersonate();

                mAction(Parameter);

                // Call finished handler
                if (OnFinished != null)
                {
                    OnFinished(this, new EventArgs());
                }

                Status = AsyncWorkerStatusEnum.WaitForFinish;
                finalStatus = AsyncWorkerStatusEnum.Finished;
            }
            catch (ThreadAbortException ex)
            {
                if (!CMSThread.Stopped(ex))
                {
                    finalStatus = SetException(ex);
                }
            }
            catch (Exception ex)
            {
                finalStatus = SetException(ex);
            }
            finally
            {
                // Wait for finishing, not more than 10 seconds
                int index = 0;
                while (mWaitForFinish && (index < 100))
                {
                    System.Threading.Thread.Sleep(100);
                    index++;
                }

                Status = finalStatus;
                mThread = null;

                // Undo impersonation
                if (ctx != null)
                {
                    ctx.Undo();
                }
            }
        }


        /// <summary>
        /// Performs actions necessary to handle exception.
        /// </summary>
        /// <param name="ex">Exception to handle</param>
        /// <returns>Error status</returns>
        private AsyncWorkerStatusEnum SetException(Exception ex)
        {
            // Log the exception
            CoreServices.EventLog.LogException("AsyncControl", "RUN", ex);

            Status = AsyncWorkerStatusEnum.WaitForFinish;
            mLastException = ex;

            // Call error handler
            if (OnError != null)
            {
                OnError(this, new EventArgs());
            }

            return AsyncWorkerStatusEnum.Error;
        }

        #endregion
    }
}