using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{

    internal sealed class SetContactChainLeadDatabaseOperation : IDatabaseOperation
    {

        #region "Private members"

        private IEnumerable<ContactInfo> mContacts;
        private string mLeadId;

        #endregion

        #region "Constructors"

        public SetContactChainLeadDatabaseOperation(IEnumerable<ContactInfo> contacts, string leadId)
        {
            mContacts = contacts;
            mLeadId = leadId;
        }

        #endregion

        #region "Public methods"

        public void Execute(string entityId, Dictionary<int, ContactInfo> contacts)
        {
            int lastContactId = mContacts.Last().ContactID;
            foreach (ContactInfo contact in mContacts)
            {
                if (contact.ContactID == lastContactId)
                {
                    contact.SetValue("ContactSalesForceLeadID", mLeadId ?? entityId);
                }
                else
                {
                    contact.SetValue("ContactSalesForceLeadID", null);
                }
                contacts[contact.ContactID] = contact;
            }
        }

        #endregion

    }

}