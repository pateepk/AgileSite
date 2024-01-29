using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class SetContactLeadDatabaseOperation : IDatabaseOperation
    {

        #region "Private members"

        private ContactInfo mContact;
        private string mLeadId;

        #endregion

        #region "Constructors"

        public SetContactLeadDatabaseOperation(ContactInfo contact, string leadId)
        {
            mContact = contact;
            mLeadId = leadId;
        }

        #endregion

        #region "Public methods"

        public void Execute(string entityId, Dictionary<int, ContactInfo> contacts)
        {
            mContact.SetValue("ContactSalesForceLeadID", mLeadId ?? entityId);
            contacts[mContact.ContactID] = mContact;
        }

        #endregion

    }

}