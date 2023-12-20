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
        private SyncManager mSyncManager;


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
                    var manager = SyncManager.GetInstance();
                    manager.OperationType = OperationTypeEnum.Synchronization;
                    manager.LogTasks = SettingsKeyInfoProvider.GetBoolValue("CMSStagingLogStagingChanges", SiteContext.CurrentSiteName);
                    manager.UseTreeCustomHandlers = UseTreeCustomHandlers;
                    manager.UseAutomaticOrdering = UseAutomaticOrdering;
                    manager.SiteName = SiteContext.CurrentSiteName;

                    mSyncManager = manager;
                }

                return mSyncManager;
            }
        }

        
        /// <summary>
        /// Processes the given synchronization task.
        /// </summary>
        /// <param name="stagingTaskData">Serialized staging task data</param>
        [WebMethod(MessageName = "ProcessSynchronizationTaskData")]
        public virtual string ProcessSynchronizationTaskData(string stagingTaskData)
        {
            var errorMsg = CheckStagingFeature();
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return errorMsg;
            }

            IStagingTaskData taskData = new StagingTaskData(stagingTaskData);

            errorMsg = CheckVersion(taskData);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return errorMsg;
            }
            
            return ProcessSynchronizationTaskInternal(taskData);
        }


        private string CheckStagingFeature()
        {
            // Get current site
            var currentSite = SiteContext.CurrentSite;
            if (currentSite == null)
            {
                return GetError("Site not running.");
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
                var currentContext = RequestSoapContext.Current;
                if (currentContext == null)
                {
                    return "Only SOAP requests are permitted.";
                }

                // Check the authentication
                switch (StagingTaskRunner.ServerAuthenticationType(currentSite.SiteName))
                {
                    // Username and password authentication
                    case ServerAuthenticationEnum.UserName:
                        // Check if token has been authenticated
                        var tokenAuthenticated = ValidationHelper.GetBoolean(RequestStockHelper.GetItem(StagingTaskRunner.AUTHENTICATION_PROCESSED), false);
                        if (!tokenAuthenticated)
                        {
                            return GetError(ResHelper.GetString("SyncServer.ErrorInvalidAuthentication"));
                        }
                        break;

                    // X509 authentication
                    case ServerAuthenticationEnum.X509:
                        // Verify message content
                        var errorMsg = X509Helper.VerifyMessageParts(currentContext);
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            return GetError(errorMsg);
                        }
                        break;

                    default:
                        return GetError(ResHelper.GetString("SyncServer.ErrorInvalidAuthentication"));
                }
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }

            return string.Empty;
        }


        /// <summary>
        /// Processes the given synchronization task.
        /// </summary>
        /// <param name="stagingTaskData">Serialized staging task data</param>
        private string ProcessSynchronizationTaskInternal(IStagingTaskData stagingTaskData)
        {
            try
            {
                // Check task type presence
                if (stagingTaskData.TaskType == TaskTypeEnum.Unknown)
                {
                    return "Missing task type or task type Unknown.";
                }

                // Set task server list for further use
                StagingTaskInfoProvider.TaskServerList = stagingTaskData.TaskServers;

                SyncManager.ProcessTask(stagingTaskData, true, StagingEvents.ProcessTask);

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }


        private static string CheckVersion(IStagingTaskData stagingTaskData)
        {
            if (CMSVersion.MainVersion.Equals(stagingTaskData.SystemVersion, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return GetError(string.Format(ResHelper.GetString("SyncServer.ErrorVersion"), CMSVersion.MainVersion, stagingTaskData.SystemVersion));
        }


        /// <summary>
        /// Returns the full error message.
        /// </summary>
        private static string GetError(string errorMsg)
        {
            return ResHelper.GetString("SyncServer.ServerError") + ": " + errorMsg;
        }


        private string ProcessException(Exception ex)
        {
            // Log the exception
            EventLogProvider.LogEvent(EventType.ERROR, "Staging", "RUNTASK", EventLogProvider.GetExceptionLogMessage(ex), RequestContext.RawURL, 0, null, 0, null, RequestContext.UserHostAddress, SiteContext.CurrentSite.SiteID);

            // Return the error message
            return GetError(ResHelper.GetString("SyncServer.ErrorException") + ": " + ex.Message);
        }
    }
}