using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;

namespace CMS.SalesForce
{

    /// <summary>
    /// Task that executes SalesForce replication.
    /// </summary>
    public sealed class SalesForceReplicationTask : ITask
    {

        #region "Public methods"

        /// <summary>
        /// Executes SalesForce replication.
        /// </summary>
        /// <param name="task">The task that is being executed.</param>
        /// <returns>A message if there was an error during replication; otherwise, an empty string.</returns>
        public string Execute(TaskInfo task)
        {
            if (!LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.SalesForce, ModuleName.ONLINEMARKETING))
            {
                return String.Format(ResHelper.GetString("licenselimitation.featurenotavailable"), "Salesforce.com Connector");
            }

            string credentials = SettingsKeyInfoProvider.GetValue("CMSSalesForceCredentials");
            if (String.IsNullOrEmpty(credentials))
            {
                return ResHelper.GetString("sf.nocredentials");
            }

            ISalesForceClientProvider clientProvider = CreateSalesForceClientProvider(credentials);
            IRunnable replication = new LeadReplicationEngineHost(clientProvider);

            string errorMesssage = RunReplication(replication);

            return errorMesssage;
        }

        #endregion


        #region "Private methods"

        private ISalesForceClientProvider CreateSalesForceClientProvider(string content)
        {
            OrganizationCredentials credentials = OrganizationCredentials.Deserialize(EncryptionHelper.DecryptData(content).TrimEnd('\0'));
            RefreshTokenSessionProvider sessionProvider = new RefreshTokenSessionProvider
            {
                RefreshToken = credentials.RefreshToken,
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret
            };

            return new SingletonScopeSalesForceClientProvider(sessionProvider);
        }


        /// <summary>
        /// Runs SalesForce replication.
        /// </summary>
        /// <param name="replication">Replication.</param>
        /// <returns>A message if there was an error during replication; otherwise, an empty string.</returns>
        private static string RunReplication(IRunnable replication)
        {
            try
            {
                replication.Run();
            }
            catch (SalesForceReplicationException exception)
            {
                string description = exception.Errors.Join(Environment.NewLine);
                EventLogProvider.LogException("Salesforce.com Connector", "Replication", exception, 0, description);
                return exception.Message;
            }
            catch (Exception exception)
            {
                EventLogProvider.LogException("Salesforce.com Connector", "Replication", exception);
                return exception is SalesForceException ? exception.Message : ResHelper.GetString("sf.replicationexception");
            }

            return String.Empty;
        }

        #endregion
    }

}