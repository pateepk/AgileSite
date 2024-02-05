using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement;

namespace CMS.SalesForce
{
    /// <summary>
    /// Provides customization of SalesForce lead replication.
    /// </summary>
    public class LeadReplicationHelper : AbstractHelper<LeadReplicationHelper>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the LeadReplicationHelper class.
        /// </summary>
        public LeadReplicationHelper()
        {

        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Modifies the SalesForce lead after the contact mapping.
        /// </summary>
        /// <param name="lead">The SalesForce lead to modify.</param>
        /// <param name="contact">The contact that was mapped.</param>
        public static void PrepareLeadForReplication(Entity lead, ContactInfo contact)
        {
            HelperObject.PrepareLeadForReplicationInternal(lead, contact);
        }


        /// <summary>
        /// Returns an array of contacts that have been updated since the last replication into SalesForce leads.
        /// </summary>
        /// <param name="modificationDateTimeThreshold">The maximum date and time of contact's last modification.</param>
        /// <param name="suspensionDateTimeThreshold">The maximum date and time of contact's last lead replication suspension.</param>
        /// <param name="batchSize">Maximum number of contacts returned.</param>
        /// <param name="contactIdentifiers">An optional white list of contact identifiers.</param>
        /// <returns>An array of contacts that have been updated since the last replication into SalesForce leads.</returns>
        public static ContactInfo[] GetContactsForUpdate(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold, int batchSize, int[] contactIdentifiers)
        {
            return HelperObject.GetContactsForUpdateInternal(modificationDateTimeThreshold, suspensionDateTimeThreshold, batchSize, contactIdentifiers);
        }


        /// <summary>
        /// Returns an array of contacts for replication into SalesForce leads.
        /// </summary>
        /// <param name="modificationDateTimeThreshold">The maximum date and time of contact's last modification.</param>
        /// <param name="suspensionDateTimeThreshold">The maximum date and time of contact's last lead replication suspension.</param>
        /// <param name="scoreId">The identifier of the score whose value will determine which contacts will be replicated.</param>
        /// <param name="minScoreValue">The minimum score value that the contact has to reach to be replicated.</param>
        /// <param name="batchSize">Maximum number of contacts returned.</param>
        /// <param name="contactIdentifiers">An optional white list of contact identifiers.</param>
        /// <returns>An array of contacts for replication into SalesForce leads.</returns>
        public static ContactInfo[] GetContactsForInsert(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold, int scoreId, int minScoreValue, int batchSize, int[] contactIdentifiers)
        {
            return HelperObject.GetContactsForInsertInternal(modificationDateTimeThreshold, suspensionDateTimeThreshold, scoreId, minScoreValue, batchSize, contactIdentifiers);
        }


        /// <summary>
        /// Returns a contact with the specified identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns>A contact with the specified identifier.</returns>
        public static ContactInfo GetContact(int contactId)
        {
            return HelperObject.GetContactInternal(contactId);
        }

        #endregion


        #region "Public customization methods"

        /// <summary>
        /// Modifies the SalesForce lead after the contact mapping.
        /// </summary>
        /// <param name="lead">The SalesForce lead to modify.</param>
        /// <param name="contact">The contact that was mapped.</param>
        protected virtual void PrepareLeadForReplicationInternal(Entity lead, ContactInfo contact)
        {
        }


        /// <summary>
        /// Returns an array of contacts that have been updated since the last replication into SalesForce leads.
        /// </summary>
        /// <param name="modificationDateTimeThreshold">The maximum date and time of contact's last modification.</param>
        /// <param name="suspensionDateTimeThreshold">The maximum date and time of contact's last lead replication suspension.</param>
        /// <param name="batchSize">Maximum number of contacts returned.</param>
        /// <param name="contactIdentifiers">An optional white list of contact identifiers.</param>
        /// <returns>An array of contacts that have been updated since the last replication into SalesForce leads.</returns>
        protected virtual ContactInfo[] GetContactsForUpdateInternal(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold, int batchSize, int[] contactIdentifiers)
        {
            StringBuilder condition = new StringBuilder();
            condition.AppendFormat(" ContactSalesForceLeadID is not null");
            condition.AppendFormat(" and coalesce(ContactSalesForceLeadReplicationDisabled, 0) = 0");
            condition.AppendFormat(" and (ContactSalesForceLeadReplicationSuspensionDateTime is null or ContactSalesForceLeadReplicationSuspensionDateTime <= N'{0:s}')", suspensionDateTimeThreshold);
            condition.AppendFormat(" and ContactLastModified <= N'{0:s}'", modificationDateTimeThreshold);
            condition.AppendFormat(" and ContactLastModified > ContactSalesForceLeadReplicationDateTime");
            if (contactIdentifiers != null && contactIdentifiers.Length > 0)
            {
                string[] tokens = contactIdentifiers.Select(x => x.ToString("D")).ToArray();
                condition.AppendFormat(" and ContactID in ({0})", String.Join(",", tokens));
            }

            return ContactInfoProvider.GetContacts()
                                      .TopN(batchSize)
                                      .Where(condition.ToString())
                                      .OrderBy("ContactID")
                                      .ToArray();
        }


        /// <summary>
        /// Returns an array of contacts for replication into SalesForce leads.
        /// </summary>
        /// <param name="modificationDateTimeThreshold">The maximum date and time of contact's last modification.</param>
        /// <param name="suspensionDateTimeThreshold">The maximum date and time of contact's last lead replication suspension.</param>
        /// <param name="scoreId">The identifier of the score whose value will determine which contacts will be replicated.</param>
        /// <param name="minScoreValue">The minimum score value that the contact has to reach to be replicated.</param>
        /// <param name="batchSize">Maximum number of contacts returned.</param>
        /// <param name="contactIdentifiers">An optional white list of contact identifiers.</param>
        /// <returns>An array of contacts for replication into SalesForce leads.</returns>
        protected virtual ContactInfo[] GetContactsForInsertInternal(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold, int scoreId, int minScoreValue, int batchSize, int[] contactIdentifiers)
        {
            StringBuilder condition = new StringBuilder();
            condition.AppendFormat(" ContactLastModified <= N'{0:s}'", modificationDateTimeThreshold);
            condition.AppendFormat(" and ContactSalesForceLeadID is null");
            condition.AppendFormat(" and coalesce(ContactSalesForceLeadReplicationDisabled, 0) = 0");
            condition.AppendFormat(" and (ContactSalesForceLeadReplicationSuspensionDateTime is null or ContactSalesForceLeadReplicationSuspensionDateTime <= N'{0:s}')", suspensionDateTimeThreshold);
            if (scoreId > 0)
            {
                condition.AppendFormat(" and (coalesce(ContactSalesForceLeadReplicationRequired, 0) = 1 or ContactID in (select ContactID from OM_ScoreContactRule where ScoreID = {0:D} group by ContactID having sum(cast(Value as bigint)) >= {1:D}))", scoreId, minScoreValue);
            }
            if (contactIdentifiers != null && contactIdentifiers.Length > 0)
            {
                string[] tokens = contactIdentifiers.Select(x => x.ToString("D")).ToArray();
                condition.AppendFormat(" and ContactID in ({0})", String.Join(",", tokens));
            }

            return ContactInfoProvider.GetContacts()
                                      .Where(condition.ToString())
                                      .OrderBy("ContactID")
                                      .TopN(batchSize)
                                      .ToArray();
        }


        /// <summary>
        /// Returns a contact with the specified identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns>A contact with the specified identifier.</returns>
        protected virtual ContactInfo GetContactInternal(int contactId)
        {
            return ContactInfoProvider.GetContactInfo(contactId);
        }

        #endregion
    }
}