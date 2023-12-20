using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class DisableContactReplicationDatabaseOperation : IDatabaseOperation
    {

        #region "Private members"

        private ContactInfo mContact;

        #endregion

        #region "Constructors"

        public DisableContactReplicationDatabaseOperation(ContactInfo contact)
        {
            mContact = contact;
        }

        #endregion

        #region "Public methods"

        public void Execute(string entityId, Dictionary<int, ContactInfo> contacts)
        {
            mContact.SetValue("ContactSalesForceLeadReplicationDisabled", true);
            contacts[mContact.ContactID] = mContact;
        }

        #endregion

    }

}