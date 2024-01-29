using System;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides data operations required by the lead replication.
    /// </summary>
    internal sealed class LeadReplicationDataProvider
    {

        #region "Public methods"

        public ContactInfo[] GetContactsForReplicationUpdate(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold, int batchSize, int[] contactIdentifiers)
        {
            return LeadReplicationHelper.GetContactsForUpdate(modificationDateTimeThreshold, suspensionDateTimeThreshold, batchSize, contactIdentifiers);
        }

        public ContactInfo[] GetContactsForReplicationInsert(DateTime modificationDateTimeThreshold, DateTime suspensionDateTimeThreshold, int scoreId, int minScoreValue, int batchSize, int[] contactIdentifiers)
        {
            return LeadReplicationHelper.GetContactsForInsert(modificationDateTimeThreshold, suspensionDateTimeThreshold, scoreId, minScoreValue, batchSize, contactIdentifiers);
        }

        public ContactInfo GetContact(int contactId)
        {
            return LeadReplicationHelper.GetContact(contactId);
        }

        #endregion

    }

}