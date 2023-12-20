using System;
using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class LeadReplicationTask
    {

        #region "Private members"

        private ContactInfo mContact;
        private Entity mEntity;
        private EntityOperationEnum mEntityOperation = EntityOperationEnum.None;
        private DatabaseOperationRecorder mDatabaseOperationRecorder;

        #endregion

        #region "Public properties"

        public ContactInfo Contact
        {
            get
            {
                return mContact;
            }
        }

        public Entity Entity
        {
            get
            {
                return mEntity;
            }
        }

        public EntityOperationEnum EntityOperation
        {
            get
            {
                return mEntityOperation;
            }
        }

        public DatabaseOperationRecorder DatabaseOperationRecorder
        {
            get
            {
                return mDatabaseOperationRecorder;
            }
        }

        #endregion

        #region "Constructors"

        public LeadReplicationTask(ContactInfo contact)
        {
            mContact = contact;
            mDatabaseOperationRecorder = new DatabaseOperationRecorder();
        }

        public LeadReplicationTask(ContactInfo contact, Entity entity, EntityOperationEnum entityOperation) : this(contact)
        {
            mEntity = entity;
            mEntityOperation = entityOperation;
        }

        #endregion

        #region "Public methods"

        public void ExecuteDatabaseOperations(string entityId, Dictionary<int, ContactInfo> contacts)
        {
            foreach (IDatabaseOperation databaseOperation in mDatabaseOperationRecorder.DatabaseOperations)
            {
                databaseOperation.Execute(entityId, contacts);
            }
        }

        public LeadReplicationTask WithDatabaseOperation(Action<DatabaseOperationRecorder> recorderAction)
        {
            recorderAction(DatabaseOperationRecorder);

            return this;
        }

        #endregion

    }

}