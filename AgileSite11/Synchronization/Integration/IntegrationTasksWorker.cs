using System;
using System.Threading;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class for asynchronous task processing.
    /// </summary>
    public class IntegrationTasksWorker : AbstractWorker
    {
        #region "Properties"

        /// <summary>
        /// Connector info object
        /// </summary>
        public AbstractIntegrationConnector Connector
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether to process internal or external tasks
        /// </summary>
        public bool InternalTasks
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IntegrationTasksWorker()
        {
            // Always execute task worker in sequence
            RunInSequence = true;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs the worker as a new thread.
        /// </summary>
        public override void Run()
        {
            string eventCode = null;
            try
            {
                // Check if the object is available
                if (Connector == null)
                {
                    throw new Exception("[IntegrationTasksWorker]: Connector is not specified.");
                }

                if (InternalTasks)
                {
                    if (!Connector.ProcessingInternalTasks)
                    {
                        bool process = false;
                        try
                        {
                            process = Connector.SetProcessingInternalTasks();
                            if (process)
                            {
                                eventCode = "PROCESSINTERNALTASKS";
                                ProcessInternalTasks();
                            }
                        }
                        finally
                        {
                            if (process)
                            {
                                Connector.ClearProcessingInternalTasks();
                            }
                        }
                    }
                }
                else
                {
                    if (!Connector.ProcessingExternalTasks)
                    {
                        bool process = false;
                        try
                        {
                            process = Connector.SetProcessingExternalTasks();
                            if (process)
                            {
                                eventCode = "PROCESSEXTERNALTASKS";
                                ProcessExternalTasks();
                            }
                        }
                        finally
                        {
                            if (process)
                            {
                                Connector.ClearProcessingExternalTasks();
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                // Log the thread abort
                if (!CMSThread.Stopped(ex))
                {
                    LogException(ex, eventCode);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                LogException(ex, eventCode);
            }
        }


        /// <summary>
        /// Logs exception to event log
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="eventCode">Code of event</param>
        protected void LogException(Exception exception, string eventCode)
        {
            EventLogProvider.LogException("Integration", "LOGOBJECTTASK", exception);
        }


        /// <summary>
        /// Processes internal tasks.
        /// </summary>
        protected void ProcessInternalTasks()
        {
            Connector.ProcessInternalTasks();
        }


        /// <summary>
        /// Processes external tasks.
        /// </summary>
        protected void ProcessExternalTasks()
        {
            Connector.ProcessExternalTasks();
        }

        #endregion
    }
}