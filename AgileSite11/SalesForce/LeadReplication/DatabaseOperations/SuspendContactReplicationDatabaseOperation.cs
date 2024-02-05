using System;
using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class SuspendContactReplicationDatabaseOperation : IDatabaseOperation
    {

        #region "Private members"

        private ContactInfo mContact;
        private DateTime mDateTime;

        #endregion

        #region "Constructors"

        public SuspendContactReplicationDatabaseOperation(ContactInfo contact, DateTime dateTime)
        {
            mContact = contact;
            mDateTime = dateTime;
        }

        #endregion

        #region "Public methods"

        public void Execute(string entityId, Dictionary<int, ContactInfo> contacts)
        {
            mContact.SetValue("ContactSalesForceLeadReplicationSuspensionDateTime", mDateTime);
            contacts[mContact.ContactID] = mContact;
        }

        #endregion

    }

}