using System;
using System.Web.Services;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Messaging;

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Content staging service.
    /// </summary>
    [WebService(Namespace = "http://localhost/SyncWebService/SyncServer")]
    [Policy(typeof(ServicePolicy))]
    [SoapActor("*")]
    public class SyncServer : WebService
    {
        #region "Variables"

        private bool? mLogTasks = null;
        private string currentSiteName = SiteContext.CurrentSiteName;
        private SyncManager mSyncManager = null;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Indicates if logging staging task is enabled.
        /// </summary>
        protected bool LogTasks
        {
            get
            {
                if (mLogTasks == null)
                {
                    mLogTasks = SettingsKeyInfoProvider.GetBoolValue(currentSiteName + ".CMSStagingLogStagingChanges");
                }
                return mLogTasks.Value;
            }
        }


        /// <summary>
        /// Indicates if custom handlers should be used for document staging operations.
        /// </summary>
        public static BoolAppSetting UseTreeCustomHandlers = new BoolAppSetting("CMSStagingUseTreeCustomHandlers", true);


        /// <summary>
        /// Indicates if automatic ordering for documents should be used on target server.
        /// </summary>
        public static BoolAppSetting UseAutomaticOrdering = new BoolAppSetting("CMSStagingUseAutomaticOrdering", true);


        /// <summary>
        /// SyncHelper instance.
        /// </summary>
        protected SyncManager SyncManager
        {
            get
            {
                if (mSyncManager == null)
                {
                    var man = SyncManager.GetInstance();
                    man.OperationType = OperationTypeEnum.Synchronization;
                    man.LogTasks = LogTasks;
                    man.UseTreeCustomHandlers = UseTreeCustomHandlers;
                    man.UseAutomaticOrdering = UseAutomaticOrdering;
                    man.SiteName = SiteContext.CurrentSiteName;

                    mSyncManager = man;
                }

                return mSyncManager;
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Processes the given synchronization task.
        /// </summary>
        /// <param name="stagingTaskData">Serialized staging task data</param>
        [WebMethod(MessageName = "ProcessSynchronizationTaskData")]
        public virtual string ProcessSynchronizationTaskData(string stagingTaskData)
        {
            IStagingTaskData std = new StagingTaskData(stagingTaskData);
            return ProcessSynchronizationTaskInternal(std);
        }


        /// <summary>
        /// Processes the given synchronization task.
        /// </summary>
        /// <param name="stagingTaskData">Serialized staging task data</param>
        private string ProcessSynchronizationTaskInternal(IStagingTaskData stagingTaskData)
        {
            // Check the version
            if (CMSVersion.MainVersion.ToLowerCSafe() != stagingTaskData.SystemVersion.ToLowerCSafe())
            {
                return GetError(String.Format(ResHelper.GetString("SyncServer.ErrorVersion"), CMSVersion.MainVersion, stagingTaskData.SystemVersion));
            }

            // Get current site
            SiteInfo currentSite = SiteContext.CurrentSite;
            if (currentSite == null)
            {
                return GetError("[SyncServer.ProcessSyncTask]: Site not running.");
            }

            // Check license
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Staging))
            {
                return GetError(ResHelper.GetString("SyncServer.ErrorLicense"));
            }

            try
            {
                // If not enabled, refuse to serve
                if (!StagingTaskRunner.IsServerEnabled(currentSite.SiteName))
                {
                    return GetError(ResHelper.GetString("SyncServer.ErrorServiceNotEnabled"));
                }

                // Reject any requests which are not valid SOAP requests
                SoapContext currentContext = RequestSoapContext.Current;
                if (currentContext == null)
                {
                    return "[SyncServer.ProcessSyncTask]: Only SOAP requests are permitted.";
                }

                // Check the authentication
                switch (StagingTaskRunner.ServerAuthenticationType(currentSite.SiteName))
                {
                    // Username and password authentication
                    case ServerAuthenticationEnum.UserName:
                        // Check if token has been authenticated
                        bool tokenAuthenticated = ValidationHelper.GetBoolean(RequestStockHelper.GetItem(StagingTaskRunner.AUTHENTICATION_PROCESSED), false);

                        if (!tokenAuthenticated)
                        {
                            return GetError(ResHelper.GetString("SyncServer.ErrorInvalidAuthentication"));
                        }
                        break;

                    // X509 authentication
                    case ServerAuthenticationEnum.X509:
                        // Verify message content
                        string errorMsg = X509Helper.VerifyMessageParts(currentContext);
                        if (errorMsg != "")
                        {
                            return GetError(errorMsg);
                        }
                        break;

                    default:
                        return GetError(ResHelper.GetString("SyncServer.ErrorInvalidAuthentication"));
                }

                // Check task type presence
                if (stagingTaskData.TaskType == TaskTypeEnum.Unknown)
                {
                    return "[SyncServer.ProcessSyncTask]: Missing task type or task type Unknown.";
                }

                // Set task server list for further use
                StagingTaskInfoProvider.TaskServerList = stagingTaskData.TaskServers;

                SyncManager.ProcessTask(stagingTaskData, true, StagingEvents.ProcessTask);

                return "";
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogEvent(EventType.ERROR, "Staging", "RUNTASK", EventLogProvider.GetExceptionLogMessage(ex), RequestContext.RawURL, 0, null, 0, null, RequestContext.UserHostAddress, currentSite.SiteID);

                // Return the error message
                return GetError(ResHelper.GetString("SyncServer.ErrorException") + ": " + ex.Message);
            }
        }


        /// <summary>
        /// Returns the full error message.
        /// </summary>
        private string GetError(string errorMsg)
        {
            return ResHelper.GetString("SyncServer.ServerError") + ": " + errorMsg;
        }

        #endregion
    }
}