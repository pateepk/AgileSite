using System;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.DataProtection.Internal;
using CMS.FormEngine.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("ConsentAgreementListUniGridExtender", typeof(ConsentAgreementListUniGridExtender))]

namespace CMS.UIControls
{
    /// <summary>
    /// Consent agreements list extender
    /// </summary>
    public class ConsentAgreementListUniGridExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.Load += (object sender, EventArgs e) =>
            {
            // Remove the object type set in UI. Data will be retrieved manually via the "GetContactsWithAgreedConsent" method.
            Control.ObjectType = null;
            };

            Control.OnDataReload += GetContactsWithAgreedConsent;
        }


        private DataSet GetContactsWithAgreedConsent(string completeWhere, string currentOrder, int currentTopN, string columns, int currentOffset, int currentPageSize, ref int totalRecords)
        {
            ConsentInfo consent = UIContext.Current.EditedObjectParent as ConsentInfo;
            if (consent == null)
            {
                return null;
            }

            var contactsRetriever = new ConsentContactsRetriever(consent);
            var contacts = contactsRetriever.GetContacts()
                                            // Apply the filter condition
                                            .Where(completeWhere);

            return new InfoDataSet<ContactInfo>(contacts.ToArray());
        }
    }
}