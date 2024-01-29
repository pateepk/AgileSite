using System;
using System.Linq;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ContactManagement;
using CMS.Core;

namespace CMS.SalesForce
{

    /// <summary>
    /// Hosts a lead replication engine.
    /// </summary>
    public sealed class LeadReplicationEngineHost : IRunnable
    {

        #region "Private members"

        private readonly ISalesForceClientProvider mClientProvider;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the LeadReplicationEngineHost.
        /// </summary>
        /// <param name="clientProvider">The provider of SalesForce clients.</param>
        public LeadReplicationEngineHost(ISalesForceClientProvider clientProvider)
        {
            mClientProvider = clientProvider;
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Executes the replication.
        /// </summary>
        public void Run()
        {
            ContactInfo[] contacts = new ContactInfo[0];
            Run(contacts);
        }


        /// <summary>
        /// Executes the replication of the specified contacts.
        /// </summary>
        /// <param name="contacts">A white list of contact identifiers.</param>
        public void Run(ContactInfo[] contacts)
        {
            bool enabled = GetEnabled();
            if (enabled)
            {
                LeadReplicationContext context = new LeadReplicationContext
                {

                    ScoreId = GetScoreId(),
                    MinScoreValue = GetMinScoreValue(),
                    BatchSize = GetBatchSize(),
                    DescriptionMacro = GetDescriptionMacro(),
                    DefaultCompanyName = GetDefaultCompanyName(),
                    UpdateEnabled = GetUpdateEnabled(),
                    Mapping = GetMapping(),
                    ContactIdentifiers = contacts.Select(x => x.ContactID).ToArray()
                };

                var eventLogService = Service.Resolve<IEventLogService>();
                LeadReplicationEngine engine = new LeadReplicationEngine(mClientProvider, context, eventLogService);
                engine.Run();
            }
        }

        #endregion

        #region "Private methods"

        private bool GetEnabled()
        {
            string settingName = "CMSSalesForceLeadReplicationEnabled";

            return SettingsKeyInfoProvider.GetBoolValue(settingName);
        }

        private int GetScoreId()
        {
            string settingName = "CMSSalesForceLeadReplicationScoreID";
            int scoreId = SettingsKeyInfoProvider.GetIntValue(settingName);
            if (scoreId == 0)
            {
                return 0;
            }
            ScoreInfo score = ScoreInfoProvider.GetScoreInfo(scoreId);
            if (score == null)
            {
                throw new SalesForceException(ResHelper.GetString("sf.leadreplication.noscore"));
            }

            return score.ScoreID;
        }

        private int GetMinScoreValue()
        {
            string settingName = "CMSSalesForceLeadReplicationMinScoreValue";
            int minScoreValue = SettingsKeyInfoProvider.GetIntValue(settingName);
            if (minScoreValue < 0)
            {
                throw new SalesForceException(ResHelper.GetString("sf.leadreplication.novalidminscorevalue"));
            }

            return minScoreValue;
        }

        private bool GetUpdateEnabled()
        {
            string settingName = "CMSSalesForceLeadReplicationUpdateEnabled";

            return SettingsKeyInfoProvider.GetBoolValue(settingName);
        }

        private int GetBatchSize()
        {
            string settingName = "CMSSalesForceLeadReplicationBatchSize";
            int batchSize = SettingsKeyInfoProvider.GetIntValue(settingName);
            if (batchSize < 1)
            {
                throw new SalesForceException(ResHelper.GetString("sf.leadreplication.novalidbatchsize"));
            }

            return batchSize;
        }

        private string GetDescriptionMacro()
        {
            string settingName = "CMSSalesForceLeadReplicationLeadDescription";

            return SettingsKeyInfoProvider.GetValue(settingName);
        }

        private string GetDefaultCompanyName()
        {
            string settingName = "CMSSalesForceLeadReplicationDefaultCompanyName";
            string content = SettingsKeyInfoProvider.GetValue(settingName);
            if (String.IsNullOrEmpty(content))
            {
                throw new SalesForceException(ResHelper.GetString("sf.leadreplication.nodefaultcompanyname"));
            }

            return content;
        }

        private Mapping GetMapping()
        {
            string settingName = "CMSSalesForceLeadReplicationMapping";
            string content = SettingsKeyInfoProvider.GetValue(settingName);
            if (String.IsNullOrEmpty(content))
            {
                throw new SalesForceException(ResHelper.GetString("sf.leadreplication.nocontactmapping"));
            }
            MappingSerializer serializer = new MappingSerializer();
            Mapping mapping = serializer.DeserializeMapping(content);
            if (String.IsNullOrEmpty(mapping.ExternalIdentifierAttributeName))
            {
                throw new SalesForceException(ResHelper.GetString("sf.leadreplication.noexternalid"));
            }

            return mapping;
        }

        #endregion

    }

}