using System;
using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class SetContactReplicationDateTimeDatabaseOperation : IDatabaseOperation
    {

        #region "Private members"

        private ContactInfo mContact;
        private DateTime mReplicationDateTime;

        #endregion

        #region "Constructors"

        public SetContactReplicationDateTimeDatabaseOperation(ContactInfo contact, DateTime replicationDateTime)
        {
            mContact = contact;
            mReplicationDateTime = replicationDateTime;
        }

        #endregion

        #region "Public methods"

        public void Execute(string entityId, Dictionary<int, ContactInfo> contacts)
        {
            mContact.SetValue("ContactSalesForceLeadReplicationDateTime", mReplicationDateTime);
            mContact.SetValue("ContactSalesForceLeadReplicationSuspensionDateTime", null);
            mContact.SetValue("ContactSalesForceLeadReplicationRequired", false);
            contacts[mContact.ContactID] = mContact;
        }

        #endregion

    }

}