using System;
using System.Threading;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Synchronization;
using CMS.Synchronization.WSE3;
using CMS.Synchronization.WSE3.Server;

using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security.Tokens;

[assembly: RegisterImplementation(typeof(ISyncClient), typeof(SyncClient), Priority = CMS.Core.RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Transient)]

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Synchronization client.
    /// </summary>
    public class SyncClient : ISyncClient
    {
        #region "Variables"

        private const string CURRENT_SERVICE_LOCATION = "/CMSPages/Staging/SyncServer.asmx";

        private SyncServerWse mService;
        private static bool? mStagingSOAPMustUnderstand;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets timeout for staging service.
        /// </summary>
        public static int Timeout
        {
            get
            {
                return ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSStagingServiceTimeout"], 180);
            }
        }


        /// <summary>
        /// Indicates if external configuration file should be used (wse3policy.config).
        /// </summary>
        public static bool UseConfigFile
        {
            get
            {
                return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStagingUseConfigFile"], false);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether staging SOAP must understand.
        /// </summary>
        public static bool StagingSOAPMustUnderstand
        {
            get
            {
                if (mStagingSOAPMustUnderstand == null)
                {
                    mStagingSOAPMustUnderstand = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStagingSOAPMustUnderstand"], true);
                }
                return mStagingSOAPMustUnderstand.Value;
            }
            set
            {
                mStagingSOAPMustUnderstand = value;
            }
        }


        /// <summary>
        /// Synchronization service.
        /// </summary>
        public SyncServerWse Service
        {
            get
            {
                if (mService == null)
                {
                    // Initialize service
                    mService = new SyncServerWse();

                    // Check whether SOAP key is defined and set security option if it is required
                    if (!StagingSOAPMustUnderstand)
                    {
#pragma warning disable 0618
                        mService.RequestSoapContext.Security.MustUnderstand = false;
#pragma warning restore 0618
                    }

                    // Setup credentials
                    switch (Server.ServerAuthentication)
                    {
                        // Username authentication
                        case ServerAuthenticationEnum.UserName:
                            {
                                // Set the username token
                                UsernameToken token = new UsernameToken(Server.ServerUsername, StagingTaskRunner.GetSHA1Hash(EncryptionHelper.DecryptData(Server.ServerPassword)), PasswordOption.SendHashed);
                                mService.SetClientCredential(token);

                                // Setup the policy
                                if (UseConfigFile)
                                {
                                    mService.SetPolicy("ClientPolicy");
                                }
                                else
                                {
                                    Policy policy = new Policy();
                                    policy.Assertions.Add(new UsernameOverTransportAssertion());
                                    policy.Assertions.Add(new RequireActionHeaderAssertion());
                                    mService.SetPolicy(policy);
                                }

                                mService.Timeout = Timeout * 1000;
                            }
                            break;

                        // X509 Authentication
                        case ServerAuthenticationEnum.X509:
                            {
                                // Setup the policy
                                Policy policy = new Policy();
                                policy.Assertions.Add(new X509ClientAssertion(Server.ServerX509ClientKeyID, Server.ServerX509ServerKeyID));
                                mService.SetPolicy(policy);

                                mService.Timeout = Timeout * 1000;
                            }
                            break;

                        default:
                            throw new Exception("[SyncClient.Service]: Unknown authentication type.");
                    }

                    mService.Url = GetFullServerUrl(Server.ServerURL);
                }
                return mService;
            }
        }


        /// <summary>
        /// Synchronization server.
        /// </summary>
        public ServerInfo Server
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Returns full path of the SyncServer.asmx server from the starting path of target server instance.
        /// If the full path in the old format comes, the suffix /CMSPages/SyncServer.asmx is trimmed and replaced by the current location.
        /// </summary>
        /// <param name="serverServiceBaseUrl">Synchronization server base URL</param>
        private string GetFullServerUrl(string serverServiceBaseUrl)
        {
            // Make sure the old suffix before the service was moved is trimmed
            const string suffixToTrim = "/CMSPages/SyncServer.asmx";

            string url = serverServiceBaseUrl;

            if (url.EndsWithCSafe(suffixToTrim, true))
            {
                url = url.Substring(0, url.Length - suffixToTrim.Length);
            }

            // Add protocol
            if (!URLHelper.ContainsProtocol(url))
            {
                url = "http://" + url;
            }

            url = url.TrimEnd('/') + CURRENT_SERVICE_LOCATION;

            return url;
        }


        /// <summary>
        /// Runs the synchronization task.
        /// </summary>
        /// <param name="taskObj">Task object</param>
        /// <returns>Returns error message</returns>
        public string RunTask(StagingTaskInfo taskObj)
        {
            if (taskObj == null)
            {
                return "[SyncClient.RunTask]: No task given.";
            }

            string result = "";
            try
            {
                // Set running
                taskObj.TaskRunning = true;
                StagingTaskInfoProvider.SetTaskInfo(taskObj);

                var stagingTaskData = new StagingTaskData(taskObj);

                // Run the task
                result = Service.ProcessSynchronizationTaskData(stagingTaskData);
            }
            catch (ThreadAbortException ex)
            {
                if (!CMSThread.Stopped(ex))
                {
                    LogContext.LogEventToCurrent(EventType.ERROR, "Staging", "RUNTASK", EventLogProvider.GetExceptionLogMessage(ex), RequestContext.RawURL, 0, null, 0, null, RequestContext.UserHostAddress, taskObj.TaskSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);

                    // Return the error message
                    return GetError(ResHelper.GetAPIString("SyncClient.ErrorException", "Exception occurred") + ": " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                LogContext.LogEventToCurrent(EventType.ERROR, "Staging", "RUNTASK",
                                    EventLogProvider.GetExceptionLogMessage(ex),
                                    RequestContext.RawURL, 0, null,
                                    0, null, RequestContext.UserHostAddress, taskObj.TaskSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);

                // Return the error message
                return GetError(ResHelper.GetAPIString("SyncClient.ErrorException", "Exception occurred") + ": " + ex.Message);
            }
            finally
            {
                if (taskObj.TaskRunning)
                {
                    // Set back to not running
                    taskObj.TaskRunning = false;
                    StagingTaskInfoProvider.SetTaskInfo(taskObj);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the full error message.
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        private static string GetError(string errorMessage)
        {
            return ResHelper.GetAPIString("SyncClient.ClientError", "Synchronization client error") + ": " + errorMessage;
        }
    }
}