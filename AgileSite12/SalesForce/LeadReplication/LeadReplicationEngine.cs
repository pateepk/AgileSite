using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.SalesForce
{

    /// <summary>
    /// Replicates contacts to SalesForce leads.
    /// </summary>
    public sealed class LeadReplicationEngine : IRunnable
    {

        #region "Private members"

        private readonly ISalesForceClientProvider mClientProvider;
        private readonly LeadReplicationDataProvider mDataProvider;
        private readonly LeadReplicationContext mContext;
        private readonly IEventLogService mEventLogService;

        private SalesForceClient mClient;
        private EntityModel mLeadModel;
        private ContactMapper mContactMapper;
        
        #endregion

        #region "Private properties"

        private SalesForceClient Client
        {
            get
            {
                if (mClient == null)
                {
                    mClient = mClientProvider.CreateClient();
                }

                return mClient;
            }
        }

        private EntityModel LeadModel
        {
            get
            {
                if (mLeadModel == null)
                {
                    mLeadModel = Client.DescribeEntity("Lead");
                }

                return mLeadModel;
            }
        }

        private ContactMapper ContactMapper
        {
            get
            {
                if (mContactMapper == null)
                {
                    ContactFormInfoProvider formInfoProvider = new ContactFormInfoProvider();
                    FormInfo formInfo = formInfoProvider.GetFormInfo();
                    AttributeValueConverterFactory factory = new AttributeValueConverterFactory();
                    mContactMapper = new ContactMapper(factory, mContext.Mapping, LeadModel, formInfo, mContext.DescriptionMacro, mContext.DefaultCompanyName);
                }

                return mContactMapper;
            }
        }


        internal ReplicationLog Log
        {
            get;
            private set;
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the LeadReplicationEngine class.
        /// </summary>
        /// <param name="clientProvider">The provider of SalesForce clients.</param>
        /// <param name="context">The lead replication context.</param>
        /// <param name="eventLogService">Event log service.</param>
        public LeadReplicationEngine(ISalesForceClientProvider clientProvider, LeadReplicationContext context, IEventLogService eventLogService)
        {
            mClientProvider = clientProvider;
            mContext = context;
            mEventLogService = eventLogService;
            mDataProvider = new LeadReplicationDataProvider();
            Log = new ReplicationLog();
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Executes the replication.
        /// </summary>
        public void Run()
        {
            if (InsufficientLicense())
            {
                string message = "LeadReplicationEngine.Run";
                mEventLogService.LogEvent(EventType.WARNING, message, LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message);
                return;
            }

            Log.Clear();
            Log.AppendInformation("Replication of contacts to SalesForce leads has started");
            DateTime modificationDateTimeThreshold = DateTime.Now;
            DateTime suspensionDateTimeThreshold = GetSuspensionDateTimeThreshold();
            if (mContext.UpdateEnabled)
            {
                ExecuteUpdatePhase(modificationDateTimeThreshold, suspensionDateTimeThreshold);
            }
            ExecuteInsertPhase(modificationDateTimeThreshold, suspensionDateTimeThreshold);
            Log.AppendInformation("Replication of contacts to SalesForce leads has ended");
            if (Log.HasErrors)
            {
                RaiseReplicationException();
            }
            else
            {
                string description = String.Join(Environment.NewLine, Log.Entries.ToArray());
                EventLogProvider.LogInformation("Salesforce.com Connector", "Replication", description);
            }
        }


        private static bool InsufficientLicense()
        {
            return !LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.SalesForce, ModuleName.ONLINEMARKETING);
        }

        #endregion

        #region "Private methods"

        private void ExecuteUpdatePhase(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold)
        {
            ContactInfo[] batch = mDataProvider.GetContactsForReplicationUpdate(modificationDateTimeThreshold, suspensionDateTimeThreshold, mContext.BatchSize, mContext.ContactIdentifiers);
            while (batch.Length > 0)
            {
                ExecuteUpdatePhaseBatch(batch, modificationDateTimeThreshold);
                if (Log.HasDatabaseErrors)
                {
                    RaiseReplicationException();
                }
                batch = mDataProvider.GetContactsForReplicationUpdate(modificationDateTimeThreshold, suspensionDateTimeThreshold, mContext.BatchSize, mContext.ContactIdentifiers);
            }
        }

        private void ExecuteUpdatePhaseBatch(ContactInfo[] batch, DateTime modificationDateTimeThreshold)
        {
            var referencingLeadsGroupedByExternalAttribute = GetReferencingLeads(batch).GroupBy(e => e.GetAttributeValue<string>(mContext.Mapping.ExternalIdentifierAttributeName))
                                                                                       .ToList();

            LogDuplicates(referencingLeadsGroupedByExternalAttribute);

            // When contact is represented with more then one record on SalesForce update only first record.
            // This should not happen when SalesForce integration is correctly used.
            Dictionary<Guid, Entity> referencingLeads = referencingLeadsGroupedByExternalAttribute.Select(e => e.First())
                                                                                                  .ToDictionary(x => new Guid(x.GetAttributeValue<string>(mContext.Mapping.ExternalIdentifierAttributeName)));

            List<LeadReplicationTask> tasks = new List<LeadReplicationTask>();
            foreach (ContactInfo contact in batch)
            {
                Entity referencingLead;
                referencingLeads.TryGetValue(contact.ContactGUID, out referencingLead);
                if (referencingLead != null && !referencingLead.GetAttributeValue<bool>("IsConverted") && !referencingLead.GetAttributeValue<bool>("IsDeleted") && String.IsNullOrEmpty(referencingLead.GetAttributeValue<string>("MasterRecordId")))
                {
                    Entity leadForUpdate = CreateLeadFromContact(contact);
                    leadForUpdate.Id = referencingLead.Id;
                    LeadReplicationTask task = CreateReplicationTask(contact, leadForUpdate, EntityOperationEnum.Update).WithDatabaseOperation(x => x.SetContactLead(contact, referencingLead.Id)).WithDatabaseOperation(x => x.SetContactReplicationDateTime(contact, modificationDateTimeThreshold));
                    tasks.Add(task);
                }
                else
                {
                    LeadReplicationTask task = CreateReplicationTask(contact).WithDatabaseOperation(x => x.DisableContactLeadReplication(contact));
                    if (referencingLead != null)
                    {
                        task.WithDatabaseOperation(x => x.SetContactLead(contact, referencingLead.Id));
                    }
                    tasks.Add(task);
                }
            }
            Log.AppendInformation("Updating {0:D} lead(s) in a batch", batch.Length);
            ExecuteReplicationTasks(tasks, modificationDateTimeThreshold);
        }

        private void ExecuteInsertPhase(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold)
        {
            ContactInfo[] batch = mDataProvider.GetContactsForReplicationInsert(modificationDateTimeThreshold, suspensionDateTimeThreshold, mContext.ScoreId, mContext.MinScoreValue, mContext.BatchSize, mContext.ContactIdentifiers);
            while (batch.Length > 0)
            {
                ExecuteInsertPhaseBatch(batch, modificationDateTimeThreshold);
                if (Log.HasDatabaseErrors)
                {
                    RaiseReplicationException();
                }
                batch = mDataProvider.GetContactsForReplicationInsert(modificationDateTimeThreshold, suspensionDateTimeThreshold, mContext.ScoreId, mContext.MinScoreValue, mContext.BatchSize, mContext.ContactIdentifiers);
            }
        }

        private void ExecuteInsertPhaseBatch(ContactInfo[] batch, DateTime modificationDateTimeThreshold)
        {
            List<LeadReplicationTask> replicationTasks = new List<LeadReplicationTask>();
            Dictionary<Guid, Entity> referencingLeads = GetReferencingLeads(batch).ToDictionary(x => new Guid(x.GetAttributeValue<string>(mContext.Mapping.ExternalIdentifierAttributeName)));
            foreach (ContactInfo contact in batch)
            {
                Entity referencingLead = null;
                referencingLeads.TryGetValue(contact.ContactGUID, out referencingLead);
                if (referencingLead != null)
                {
                    if (!referencingLead.GetAttributeValue<bool>("IsConverted") && !referencingLead.GetAttributeValue<bool>("IsDeleted") && String.IsNullOrEmpty(referencingLead.GetAttributeValue<string>("MasterRecordId")))
                    {
                        Entity leadForUpdate = CreateLeadFromContact(contact);
                        leadForUpdate.Id = referencingLead.Id;
                        LeadReplicationTask replicationTask = CreateReplicationTask(contact, leadForUpdate, EntityOperationEnum.Update).WithDatabaseOperation(x => x.SetContactLead(contact, referencingLead.Id)).WithDatabaseOperation(x => x.SetContactReplicationDateTime(contact, modificationDateTimeThreshold));
                        replicationTasks.Add(replicationTask);
                    }
                    else
                    {
                        LeadReplicationTask replicationTask = CreateReplicationTask(contact).WithDatabaseOperation(x => x.DisableContactLeadReplication(contact)).WithDatabaseOperation(x => x.SetContactLead(contact, referencingLead.Id));
                        replicationTasks.Add(replicationTask);
                    }
                }
                else
                {
                    Entity leadForInsert = CreateLeadFromContact(contact);
                    leadForInsert[mContext.Mapping.ExternalIdentifierAttributeName] = contact.ContactGUID.ToString("N");
                    LeadReplicationTask replicationTask = CreateReplicationTask(contact, leadForInsert, EntityOperationEnum.Insert).WithDatabaseOperation(x => x.SetContactLead(contact, null)).WithDatabaseOperation(x => x.SetContactReplicationDateTime(contact, modificationDateTimeThreshold));
                    replicationTasks.Add(replicationTask);
                }
            }
            Log.AppendInformation("Inserting {0:D} lead(s) in a batch", batch.Length);
            ExecuteReplicationTasks(replicationTasks, modificationDateTimeThreshold);
        }

        private void ExecuteReplicationTasks(List<LeadReplicationTask> replicationTasks, DateTime modificationDateTimeThreshold)
        {
            Dictionary<int, DateTime> timestamps = replicationTasks.Where(x => x.Contact != null).ToDictionary(x => x.Contact.ContactID, x => x.Contact.GetDateTimeValue("ContactSalesForceLeadReplicationDateTime", DateTimeHelper.ZERO_TIME));
            Dictionary<int, ContactInfo> contacts = new Dictionary<int, ContactInfo>();
            ExecuteReplicationUpdateTasks(replicationTasks, modificationDateTimeThreshold, contacts);
            ExecuteReplicationInsertTasks(replicationTasks, modificationDateTimeThreshold, contacts);
            ExecuteReplicationOtherTasks(replicationTasks, modificationDateTimeThreshold, contacts);
            foreach (ContactInfo contact in contacts.Values)
            {
                try
                {
                    ContactInfoProvider.UpdateLeadReplicationStatus(contact, timestamps[contact.ContactID]);
                }
                catch (Exception exception)
                {
                    Log.AppendDatabaseError("Database operation failed: {0}", exception.Message ?? exception.ToString());
                }
            }
        }

        private void ExecuteReplicationUpdateTasks(List<LeadReplicationTask> replicationTasks, DateTime modificationDateTimeThreshold, Dictionary<int, ContactInfo> contacts)
        {
            IEnumerable<LeadReplicationTask> updateReplicationTasks = replicationTasks.Where(x => x.EntityOperation == EntityOperationEnum.Update);
            if (updateReplicationTasks.Any())
            {
                IEnumerable<Entity> leads = updateReplicationTasks.Select(x => x.Entity);
                UpdateEntityResult[] results = Client.UpdateEntities(leads);
                ProcessReplicationTasksResults(updateReplicationTasks, results, modificationDateTimeThreshold, contacts);
            }
        }

        private void ExecuteReplicationInsertTasks(List<LeadReplicationTask> replicationTasks, DateTime modificationDateTimeThreshold, Dictionary<int, ContactInfo> contacts)
        {
            IEnumerable<LeadReplicationTask> insertReplicationTasks = replicationTasks.Where(x => x.EntityOperation == EntityOperationEnum.Insert);
            if (insertReplicationTasks.Any())
            {
                IEnumerable<Entity> leads = insertReplicationTasks.Select(x => x.Entity);
                UpsertEntityResult[] results = Client.UpsertEntities(leads, mContext.Mapping.ExternalIdentifierAttributeName);
                ProcessReplicationTasksResults(insertReplicationTasks, results, modificationDateTimeThreshold, contacts);
            }
        }

        private void ExecuteReplicationOtherTasks(List<LeadReplicationTask> replicationTasks, DateTime modificationDateTimeThreshold, Dictionary<int, ContactInfo> contacts)
        {
            foreach (LeadReplicationTask replicationTask in replicationTasks.Where(x => x.EntityOperation == EntityOperationEnum.None))
            {
                replicationTask.ExecuteDatabaseOperations(null, contacts);
            }
        }

        private void ProcessReplicationTasksResults(IEnumerable<LeadReplicationTask> replicationTasks, IEntityCommandResult[] results, DateTime modificationDateTimeThreshold, Dictionary<int, ContactInfo> contacts)
        {
            int index = 0;
            foreach (LeadReplicationTask replicationTask in replicationTasks)
            {
                IEntityCommandResult result = results[index];
                if (!result.IsSuccess)
                {
                    Log.AppendSalesForceError(result, "Could not replicate contact {0} ({1})", replicationTask.Contact.ContactLastName, replicationTask.Contact.ContactID);
                    replicationTask.DatabaseOperationRecorder.Clear();
                    replicationTask.DatabaseOperationRecorder.SuspendContactLeadReplication(replicationTask.Contact, modificationDateTimeThreshold);
                }
                replicationTask.ExecuteDatabaseOperations(result.EntityId, contacts);
                index++;
            }
        }

        private Entity CreateLeadFromContact(ContactInfo contact)
        {
            Entity lead = ContactMapper.Map(contact);
            LeadReplicationHelper.PrepareLeadForReplication(lead, contact);
            
            return lead;
        }

        private List<Entity> GetReferencingLeads(ContactInfo[] contacts)
        {
            StringBuilder statement = new StringBuilder();
            statement.AppendFormat("select Id, {0}, MasterRecordId, IsDeleted, IsConverted from Lead where {0} in (", mContext.Mapping.ExternalIdentifierAttributeName);
            bool separator = false;
            foreach (ContactInfo contact in contacts)
            {
                if (separator)
                {
                    statement.Append(",");
                }
                else
                {
                    separator = true;
                }
                statement.AppendFormat("'{0:N}'", contact.ContactGUID);
            }
            statement.Append(")");
            Client.Options.IncludeDeleted = true;
            SelectEntitiesResult result = Client.SelectEntities(statement.ToString(), LeadModel);
            List<Entity> leads = new List<Entity>(result.TotalEntityCount);
            leads.AddRange(result.Entities);
            while (!result.IsComplete)
            {
                result = Client.SelectMoreEntities(result.NextResultLocator, LeadModel);
                leads.AddRange(result.Entities);
            }

            return leads;
        }
        
        private LeadReplicationTask CreateReplicationTask(ContactInfo contact)
        {
            return new LeadReplicationTask(contact);
        }

        private LeadReplicationTask CreateReplicationTask(ContactInfo contact, Entity entity, EntityOperationEnum entityOperation)
        {
            return new LeadReplicationTask(contact, entity, entityOperation);
        }

        private void RaiseReplicationException()
        {
            throw new SalesForceReplicationException("There was an error during replication, please check the event log for more information.", Log.Entries);
        }

        private void LogDuplicates(IList<IGrouping<string, Entity>> referencingLeadsGroupedByExternalAttribute)
        {
            // ExternalIdentifierAttribute must be unique on SalesForce. Append information to log when this is not true.
            var duplicates = referencingLeadsGroupedByExternalAttribute.Where(g => g.Count() > 1)
                                                                       .Select(g => g.Key)
                                                                       .ToList();

            foreach (var duplicate in duplicates)
            {
                Log.AppendInformation("Contact with GUID {0} is represented more than once on SalesForce.", duplicate);
            }
        }

        private DateTime GetSuspensionDateTimeThreshold()
        {
            string setting = SettingsKeyInfoProvider.GetValue("CMSSalesForceLeadReplicationMappingDateTime");
            if (String.IsNullOrEmpty(setting))
            {
                return new DateTime(1900, 1, 1);
            }

            return DateTime.ParseExact(setting, "s", System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion

    }

}