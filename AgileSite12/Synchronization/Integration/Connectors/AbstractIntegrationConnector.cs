using System;
using System.Threading;

using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.EventLog;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class that needs to be inherited during implementing custom integration connector.
    /// Several members have to be implemented in order to achieve desired functionality.
    /// </summary>
    public abstract class AbstractIntegrationConnector
    {
        #region "Private variables"

        private string mConnectorName = null;
        
        // Synchronization flags for tasks processing. Must be accessed in a thread-safe manner
        private int mProcessingInternalTasks;
        private int mProcessingExternalTasks;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Name of the connector.
        /// </summary>
        public string ConnectorName
        {
            get
            {
                if (mConnectorName == null)
                {
                    throw new Exception("[AbstractIntegrationConnector.ConnectorName]: Property has to be initialized and has to correspond with IntegrationConnectorInfo.ConnectorName (code name of integration connector).");
                }
                return mConnectorName;
            }
            set
            {
                // If connector does not exist
                var connector = IntegrationConnectorInfoProvider.GetIntegrationConnectorInfo(value);
                if (connector == null)
                {
                    // Log the error
                    EventLogProvider.LogEvent(EventType.WARNING, "Integration", "LOADCONNECTORS", String.Format(ResHelper.GetAPIString("integration.connectornameerror", "Given Connector name '{0}' does not correspond with any code name of defined connectors. Please review your settings."), value));
                }
                mConnectorName = value;
            }
        }


        /// <summary>
        /// Gets integration connector info object.
        /// </summary>
        protected IntegrationConnectorInfo ConnectorInfo
        {
            get
            {
                return IntegrationConnectorInfoProvider.GetIntegrationConnectorInfo(ConnectorName);
            }
        }


        /// <summary>
        /// Gets integration connector identifier.
        /// </summary>
        public int ConnectorID
        {
            get
            {
                return (ConnectorInfo != null) ? ConnectorInfo.ConnectorID : 0;
            }
        }


        /// <summary>
        /// Gets a value that indicates if the connector is enabled.
        /// </summary>
        public bool ConnectorEnabled
        {
            get
            {
                if (ConnectorInfo != null)
                {
                    return ConnectorInfo.ConnectorEnabled;
                }
                return false;
            }
        }


        /// <summary>
        /// Indicates whether the thread for processing outgoing tasks is already running.
        /// </summary>
        /// <seealso cref="SetProcessingInternalTasks"/>
        /// <seealso cref="ClearProcessingInternalTasks"/>
        internal bool ProcessingInternalTasks
        {
            get
            {
                return Volatile.Read(ref mProcessingInternalTasks) != 0;
            }
        }


        /// <summary>
        /// Indicates whether the thread for processing incoming tasks is already running.
        /// </summary>
        /// <seealso cref="SetProcessingExternalTasks"/>
        /// <seealso cref="ClearProcessingExternalTasks"/>
        internal bool ProcessingExternalTasks
        {
            get
            {
                return Volatile.Read(ref mProcessingExternalTasks) != 0;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the <see cref="ProcessingInternalTasks"/> flag to true, if not already set and returns a value indicating whether the flag was set by the current call.
        /// To ensure mutual exclusion, only the thread successfully setting the flag can process internal tasks.
        /// </summary>
        /// <returns>Returns true if the <see cref="ProcessingInternalTasks"/> flag was set by the current call.</returns>
        internal bool SetProcessingInternalTasks()
        {
            return Interlocked.CompareExchange(ref mProcessingInternalTasks, 1, 0) == 0;
        }


        /// <summary>
        /// Clears the "<see cref="ProcessingInternalTasks"/>"/> flag. The flag can be cleared only after it has been previously set by a <see cref="SetProcessingInternalTasks"/> call.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when clearing an already clear flag. Such exception indicates possible multi-threaded access and violation of the mutual exclusion.</exception>
        internal void ClearProcessingInternalTasks()
        {
            if (Interlocked.CompareExchange(ref mProcessingInternalTasks, 0, 1) != 1)
            {
                throw new InvalidOperationException($"Clearing the {nameof(ProcessingInternalTasks)} flag failed. The flag has already been cleared.");
            }
        }


        /// <summary>
        /// Sets the <see cref="ProcessingExternalTasks"/> flag to true, if not already set and returns a value indicating whether the flag was set by the current call.
        /// To ensure mutual exclusion, only the thread successfully setting the flag can process internal tasks.
        /// </summary>
        /// <returns>Returns true if the <see cref="ProcessingExternalTasks"/> flag was set by the current call.</returns>
        internal bool SetProcessingExternalTasks()
        {
            return Interlocked.CompareExchange(ref mProcessingExternalTasks, 1, 0) == 0;
        }


        /// <summary>
        /// Clears the "<see cref="ProcessingExternalTasks"/>"/> flag. The flag can be cleared only after it has been previously set by a <see cref="SetProcessingExternalTasks"/> call.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when clearing an already clear flag. Such exception indicates possible multi-threaded access and violation of the mutual exclusion.</exception>
        internal void ClearProcessingExternalTasks()
        {
            if (Interlocked.CompareExchange(ref mProcessingExternalTasks, 0, 1) != 1)
            {
                throw new InvalidOperationException($"Clearing the {nameof(ProcessingExternalTasks)} flag failed. The flag has already been cleared.");
            }
        }

        #endregion


        #region "Abstract methods"

        /// <summary>
        /// Initializes the provider. Suitable for making subscriptions etc.
        /// ConnectorName has to be initialized within this method.
        /// </summary>
        public abstract void Init();


        /// <summary>
        /// Suitable for implementing asynchronous outgoing CMS object processing. Identifiers of object is already prepared to match external application.
        /// </summary>
        /// <param name="cmsObject">CMS object to process</param>
        /// <param name="translations">Translation helper object containing translations for given object</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <returns>Processing result</returns>
        public abstract IntegrationProcessResultEnum ProcessInternalTaskAsync(ICMSObject cmsObject, TranslationHelper translations, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName, out string errorMessage);


        /// <summary>
        /// Suitable for implementing synchronous outgoing CMS object processing.
        /// </summary>
        /// <param name="cmsObject">CMS object to process</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <param name="siteName">Name of site</param>
        /// <returns>Processing result</returns>
        public abstract IntegrationProcessResultEnum ProcessInternalTaskSync(ICMSObject cmsObject, TaskTypeEnum taskType, string siteName, out string errorMessage);


        /// <summary>
        /// Processes incoming task.
        /// </summary>
        /// <param name="obj">Object or document to process (either already prepared ICMSObject or raw external object)</param>
        /// <param name="result">What to do if the processing fails</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of input data</param>
        /// <param name="siteName">Name of the target site</param>
        public abstract void ProcessExternalTask(object obj, IntegrationProcessTypeEnum result, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName);


        /// <summary>
        /// Repeatedly fetches outgoing tasks from oldest to newest and processes them.
        /// </summary>
        public abstract void ProcessInternalTasks();


        /// <summary>
        /// Repeatedly fetches incoming tasks from oldest to newest and processes them.
        /// </summary>
        public abstract void ProcessExternalTasks();

        #endregion
    }
}