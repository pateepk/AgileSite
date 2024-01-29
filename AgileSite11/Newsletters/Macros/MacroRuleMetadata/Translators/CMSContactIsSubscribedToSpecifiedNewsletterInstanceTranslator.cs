using System;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.MacroEngine;

namespace CMS.Newsletters
{
    internal class CMSContactIsSubscribedToSpecifiedNewsletterInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactIsSubscribedToSpecifiedNewsletter Macro rule.
        /// Contact {_is} subscribed to newsletter {item}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            var stringGuid = ruleParameters["item"].Value;
            string isSubscribed = ruleParameters["_is"].Value;
            Guid newsletterGuid = stringGuid.ToGuid(Guid.Empty);

            if (newsletterGuid == Guid.Empty)
            {
                MacroValidationHelper.LogInvalidGuidParameter("SubscribedToNewsletter", stringGuid);
                return new ObjectQuery<ContactInfo>().NoResults();
            }

            // Materializing newsletters to be able to reach its site ID. We can assume there will be only a single or a few newsletters with the same codename on different sites,
            // so performance should not be the big issue. Following logic of selecting the contacts has to be made for every single newsletter.
            var newsletters = NewsletterInfoProvider.GetNewsletters()
                                                    .WhereEquals("NewsletterGUID", newsletterGuid)
                                                    .ToList();

            
            var unsubscriptionProvider = Service.Resolve<IUnsubscriptionProvider>();

            // This method has to be performed separately for each newsletter in order to be able to use newsletter ID, newsletter site ID and unsubscription table. 
            // From all emails that are subscribed to the newsletter are subtracted those present in the Unsubscription table.
            var childrenQueries = newsletters.Select(n => GetContactIdsForSingleNewsletter(n, unsubscriptionProvider));
            var operators = newsletters.Select(n => SqlOperator.UNION);

            ObjectQuery<ContactInfo> resultContacts = DataQuery.Combine(childrenQueries, operators, DatabaseSeparationHelper.OM_CONNECTION_STRING);

            var contacts = ContactInfoProvider.GetContacts();
            if (isSubscribed != "!")
            {
                contacts.WhereIn("ContactID", resultContacts);
            }
            else
            {
                contacts.WhereNotIn("ContactID", resultContacts);
            }

            return contacts;
        }


        /// <summary>
        /// Get IDs of all contacts that are subscribed to given <paramref name="newsletter"/>. Removes contacts that have unsubscription in the Unsubscription table.
        /// </summary>
        /// <param name="newsletter">Newsletter the contacts are subscribed to</param>
        /// <param name="unsubscriptionProvider">Service for getting emails unsubscribed from given <paramref name="newsletter"/></param>
        /// <returns>Object query containing all IDs of contacts subscribed to given <paramref name="newsletter"/></returns>
        private ObjectQuery<ContactInfo> GetContactIdsForSingleNewsletter(NewsletterInfo newsletter, IUnsubscriptionProvider unsubscriptionProvider)
        {
            var subscribersQuery = new DataQuery(PredefinedObjectType.SUBSCRIBERTONEWSLETTER, "selectsubscriptions")
                .WhereEquals("NewsletterID", newsletter.NewsletterID)
                .WhereTrue("SubscriptionApproved");

            var subscribersEmails = subscribersQuery.Column("SubscriberEmail");

            var contactsBySubscribersQuery = ContactInfoProvider.GetContacts()
                                                                .WhereIn("ContactEmail", subscribersEmails)
                                                                .Columns("ContactID", "ContactEmail");

            var unsubscriptions = unsubscriptionProvider.GetUnsubscriptionsFromSingleNewsletter(newsletter.NewsletterID);

            return contactsBySubscribersQuery.WhereNotIn("ContactEmail", unsubscriptions.Column("UnsubscriptionEmail")).Column("ContactID");
        }
    }
}