using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.OnlineForms;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample implementation of <see cref="IPersonalDataEraser"/> interface for erasing contact's personal data.
    /// </summary>
    internal class SampleContactPersonalDataEraser : IPersonalDataEraser
    {
        /// <summary>
        /// Defines forms to delete personal data from and name of the column where data subject's email be found.
        /// </summary>
        /// <remarks>
        /// GUIDs are used to select only specific forms on the Dancing Goat sample site.
        /// </remarks>
        private readonly Dictionary<Guid, string> dancingGoatForms = new Dictionary<Guid, string>
        {
            // BusinessCustomerRegistration
            { new Guid("0A5ACBBF-48B9-40DA-B431-53491588CDA7"), "Email" },
            // ContactUs
            { new Guid("C7A6E59B-50F7-4039-ADEA-42164054E5EF"), "UserEmail" },
            // MachineRental
            { new Guid("8D0A178A-0CD1-4E95-8B37-B0B63CD28BFE"), "Email" },
            // TryAFreeSample
            { new Guid("4BE995DD-7675-4004-8BEF-0CF3971CBA9B"), "EmailAddress" }
        };

        /// <summary>
        /// The form's column name that contains the user's consent agreement.
        /// </summary>
        private const string DANCING_GOAT_FORMS_CONSENT_COLUMN_NAME = "Consent";


        /// <summary>
        /// Erases personal data based on given <paramref name="identities"/> and <paramref name="configuration"/>.
        /// </summary>
        /// <param name="identities">Collection of identities representing a data subject.</param>
        /// <param name="configuration">Configures which personal data should be erased.</param>
        /// <remarks>
        /// The erasure process can be configured via the following <paramref name="configuration"/> parameters:
        /// <list type="bullet">
        /// <item>
        /// <term>deleteContacts</term>
        /// <description>Flag indicating whether contact(s) contained in <paramref name="identities"/> are to be deleted.</description>
        /// </item>
        /// <item>
        /// <term>deleteContactFromAccounts</term>
        /// <description>Flag indicating whether contact's association with accounts is to be deleted.</description>
        /// </item>
        /// <item>
        /// <term>deleteSubscriptionFromNewsletters</term>
        /// <description>Flag indicating whether contact's subscription to newsletters is to be deleted.</description>
        /// </item>
        /// <item>
        /// <term>deleteActivities</term>
        /// <description>Flag indicating whether activities of contact are to be deleted.</description>
        /// </item>
        /// <item>
        /// <term>deleteSubmittedFormsActivities</term>
        /// <description>Flag indicating whether form activities of contact are to be deleted.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public void Erase(IEnumerable<BaseInfo> identities, IDictionary<string, object> configuration)
        {
            var contacts = identities.OfType<ContactInfo>();
            if (!contacts.Any())
            {
                return;
            }

            var contactIds = contacts.Select(c => c.ContactID).ToList();
            var contactEmails = contacts.Select(c => c.ContactEmail).ToList();

            using (new CMSActionContext { CreateVersion = false })
            {
                DeleteSubmittedFormsActivities(contactIds, configuration);

                DeleteActivities(contactIds, configuration);

                DeleteContactFromAccounts(contactIds, configuration);

                DeleteSubscriptionFromNewsletters(contactEmails, configuration);

                DeleteContacts(contacts, configuration);

                DeleteDancingGoatSubmittedFormsData(contactEmails, contactIds, configuration);
            }
        }


        /// <summary>
        /// Deletes contact's submitted forms activities based on <paramref name="configuration"/>'s <c>deleteSubmittedFormsActivities</c> flag.
        /// </summary>
        /// <remarks>Activities are deleted via bulk operation, considering the amount of activities for a contact.</remarks>
        private void DeleteSubmittedFormsActivities(ICollection<int> contactIds, IDictionary<string, object> configuration)
        {
            object deleteSubmittedFormsActivities;
            if (configuration.TryGetValue("deleteSubmittedFormsActivities", out deleteSubmittedFormsActivities)
                && ValidationHelper.GetBoolean(deleteSubmittedFormsActivities, false))
            {
                ActivityInfoProvider.ProviderObject.BulkDelete(new WhereCondition().WhereEquals("ActivityType", PredefinedActivityType.BIZFORM_SUBMIT)
                                                                                   .WhereIn("ActivityContactID", contactIds));
            }
        }


        /// <summary>
        /// Deletes all DancingGoat submitted forms data for <paramref name="emails"/> and <paramref name="contactIDs"/>, based on <paramref name="configuration"/>'s <c>deleteSubmittedFormsData</c> flag.
        /// </summary>
        private void DeleteDancingGoatSubmittedFormsData(ICollection<string> emails, ICollection<int> contactIDs, IDictionary<string, object> configuration)
        {
            object deleteSubmittedForms;
            if (configuration.TryGetValue("deleteSubmittedFormsData", out deleteSubmittedForms)
                && ValidationHelper.GetBoolean(deleteSubmittedForms, false))
            {
                var consentAgreementGuids = ConsentAgreementInfoProvider.GetConsentAgreements()
                    .Columns("ConsentAgreementGuid")
                    .WhereIn("ConsentAgreementContactID", contactIDs);

                var formClasses = BizFormInfoProvider.GetBizForms()
                    .Source(s => s.LeftJoin<DataClassInfo>("CMS_Form.FormClassID", "ClassID"))
                    .WhereIn("FormGUID", dancingGoatForms.Keys);

                formClasses.ForEachRow(row =>
                {
                    var bizForm = new BizFormInfo(row);
                    var formClass = new DataClassInfo(row);
                    string emailColumn = dancingGoatForms[bizForm.FormGUID];
                    
                    var bizFormItems = BizFormItemProvider.GetItems(formClass.ClassName)
                        .WhereIn(emailColumn, emails);

                    if (formClass.ClassFormDefinition.Contains(DANCING_GOAT_FORMS_CONSENT_COLUMN_NAME))
                    {
                        bizFormItems.Or().WhereIn(DANCING_GOAT_FORMS_CONSENT_COLUMN_NAME, consentAgreementGuids);
                    }

                    foreach (var bizFormItem in bizFormItems)
                    {
                        bizFormItem.Delete();
                    }
                });
            }
        }


        /// <summary>
        /// Deletes contact's activities based on <paramref name="configuration"/>'s <c>deleteActivities</c> flag.
        /// </summary>
        /// <remarks>Activities are deleted via bulk operation, considering the amount of activities for a contact.</remarks>
        private void DeleteActivities(List<int> contactIds, IDictionary<string, object> configuration)
        {
            object deleteActivities;
            if (configuration.TryGetValue("deleteActivities", out deleteActivities)
                && ValidationHelper.GetBoolean(deleteActivities, false))
            {
                ActivityInfoProvider.ProviderObject.BulkDelete(new WhereCondition().WhereIn("ActivityContactID", contactIds));
            }
        }


        /// <summary>
        /// Deletes contact from accounts based on <paramref name="configuration"/>'s <c>deleteContactFromAccounts</c> flag.
        /// </summary>
        private void DeleteContactFromAccounts(ICollection<int> contactIds, IDictionary<string, object> configuration)
        {
            object deleteContactFromAccounts;
            if (configuration.TryGetValue("deleteContactFromAccounts", out deleteContactFromAccounts)
                && ValidationHelper.GetBoolean(deleteContactFromAccounts, false))
            {
                var accounts = AccountContactInfoProvider.GetRelationships()
                    .WhereIn("ContactID", contactIds);

                foreach (var account in accounts)
                {
                    account.Delete();
                }
            }
        }


        /// <summary>
        /// Deletes contact from newsletter's subscription based on <paramref name="configuration"/>'s <c>deleteSubscriptionFromNewsletters</c> flag.
        /// </summary>
        private void DeleteSubscriptionFromNewsletters(ICollection<string> contactEmails, IDictionary<string, object> configuration)
        {
            object deleteSubscriptionFromNewsletters;
            if (configuration.TryGetValue("deleteSubscriptionFromNewsletters", out deleteSubscriptionFromNewsletters)
                && ValidationHelper.GetBoolean(deleteSubscriptionFromNewsletters, false))
            {
                var subscribers = SubscriberInfoProvider.GetSubscribers()
                    .WhereIn("SubscriberEmail", contactEmails)
                    .WhereEquals("SubscriberType", PredefinedObjectType.CONTACT);

                foreach (var subscriber in subscribers)
                {
                    subscriber.Delete();
                }
            }
        }


        /// <summary>
        /// Deletes <paramref name="contacts"/> based on <paramref name="configuration"/>'s <c>deleteContacts</c> flag.
        /// </summary>
        private static void DeleteContacts(IEnumerable<ContactInfo> contacts, IDictionary<string, object> configuration)
        {
            object deleteContacts;
            if (configuration.TryGetValue("deleteContacts", out deleteContacts) && ValidationHelper.GetBoolean(deleteContacts, false))
            {
                foreach (var contact in contacts)
                {
                    ContactInfoProvider.DeleteContactInfo(contact);
                }
            }
        }
    }
}