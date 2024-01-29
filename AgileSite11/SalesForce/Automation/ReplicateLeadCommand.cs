using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.ContactManagement;

namespace CMS.SalesForce.Automation
{

    /// <summary>
    /// Represents a command that replicates contacts to Salesforce leads.
    /// </summary>
    public class ReplicateLeadCommand
    {

        #region "Public properties"

        /// <summary>
        /// Gets or sets a value indicating whether the contact will not be replicated immediately, but during the next scheduled replication.
        /// </summary>
        public bool DeferredReplication { get; set; }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Replicates the specified contact to Salesforce leads, if applicable, and returns the result.
        /// </summary>
        /// <remarks>
        /// The complete replication process is executed, however, it's limited to the specified contact.
        /// </remarks>
        /// <param name="contact">The contact to replicate.</param>
        /// <returns>The action result.</returns>
        public ReplicateLeadCommandResultEnum Execute(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            var license = LicenseKeyInfoProvider.GetBestLicense();
            var isEms = (license != null) && (license.Edition == ProductEditionEnum.EnterpriseMarketingSolution);
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING) || !isEms)
            {
                return ReplicateLeadCommandResultEnum.FeatureNotAvailable;
            }

            ContactInfoProvider.RequireLeadReplication(contact);
            // Prevent possible future updates from overwriting contact changes
            contact["ContactSalesForceLeadReplicationRequired"] = true;
            contact.Generalized.Invalidate(true);

            if (!DeferredReplication)
            {
                OrganizationCredentials credentials = GetCredentials();
                if (credentials == null)
                {
                    return ReplicateLeadCommandResultEnum.NoCredentials;
                }

                ISalesForceClientProvider clientProvider = CreateSalesForceClientProvider(credentials);
                LeadReplicationEngineHost replication = new LeadReplicationEngineHost(clientProvider);
                ContactInfo[] contacts = new ContactInfo[] { contact };
                replication.Run(contacts);
            }

            return ReplicateLeadCommandResultEnum.Success;
        }

        #endregion


        #region "Private methods"

        private OrganizationCredentials GetCredentials()
        {
            string content = SettingsKeyInfoProvider.GetValue("CMSSalesForceCredentials");
            if (!String.IsNullOrEmpty(content))
            {
                return OrganizationCredentials.Deserialize(EncryptionHelper.DecryptData(content).TrimEnd('\0'));
            }

            return null;
        }


        private ISalesForceClientProvider CreateSalesForceClientProvider(OrganizationCredentials credentials)
        {
            RefreshTokenSessionProvider sessionProvider = new RefreshTokenSessionProvider
            {
                RefreshToken = credentials.RefreshToken,
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret
            };

            return new SingletonScopeSalesForceClientProvider(sessionProvider);
        }

        #endregion

    }

}