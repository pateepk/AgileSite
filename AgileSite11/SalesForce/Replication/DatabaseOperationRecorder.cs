using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class DatabaseOperationRecorder
    {

        #region "Private members"

        private List<IDatabaseOperation> mDatabaseOperations;

        #endregion

        #region "Public properties"

        public IEnumerable<IDatabaseOperation> DatabaseOperations
        {
            get
            {
                return mDatabaseOperations.AsEnumerable();
            }
        }

        #endregion

        #region "Constructors"

        public DatabaseOperationRecorder()
        {
            mDatabaseOperations = new List<IDatabaseOperation>();
        }

        #endregion

        #region "Public methods"

        public void Clear()
        {
            mDatabaseOperations.Clear();
        }

        public void SetContactLead(ContactInfo contact, string leadId)
        {
            mDatabaseOperations.Add(new SetContactLeadDatabaseOperation(contact, leadId));
        }

        public void SetContactChainLead(IEnumerable<ContactInfo> contacts, string leadId)
        {
            mDatabaseOperations.Add(new SetContactChainLeadDatabaseOperation(contacts, leadId));
        }

        public void SetContactReplicationDateTime(ContactInfo contact, DateTime replicationDateTime)
        {
            mDatabaseOperations.Add(new SetContactReplicationDateTimeDatabaseOperation(contact, replicationDateTime));
        }

        public void DisableContactLeadReplication(ContactInfo contact)
        {
            mDatabaseOperations.Add(new DisableContactReplicationDatabaseOperation(contact));
        }

        public void SuspendContactLeadReplication(ContactInfo contact, DateTime dateTime)
        {
            mDatabaseOperations.Add(new SuspendContactReplicationDatabaseOperation(contact, dateTime));
        }

        #endregion

    }

}