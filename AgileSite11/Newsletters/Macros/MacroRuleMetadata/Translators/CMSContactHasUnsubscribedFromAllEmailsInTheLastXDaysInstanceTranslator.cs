using System;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.Newsletters
{
    internal class CMSContactHasUnsubscribedFromAllEmailsInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasUnsubscribedFromAllEmailsInTheLastXDays
        /// {_perfectum}Contact.UnsubscribedFromAllEmails(ToInt({days}))
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);

            return TranslateInternal(perfectum != "!", lastXDays);
        }


        /// <summary>
        /// CMSContactHasUnsubscribedFromAllEmailsInTheLastXDays
        /// {_perfectum}Contact.UnsubscribedFromAllEmails(ToInt({days}))
        /// </summary>
        /// <param name="getUnsubscribed">If true, unsubscribed contacts will be returned, otherwise not unsubscribed</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        internal ObjectQuery<ContactInfo> TranslateInternal(bool getUnsubscribed, int lastXDays)
        {
            var unsubscribedEmails = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                               .Column("UnsubscriptionEmail")
                                                               .WhereNull("UnsubscriptionNewsletterID");

            if (lastXDays > 0)
            {
                var currentTime = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
                unsubscribedEmails.WhereGreaterThan("UnsubscriptionCreated", currentTime.Subtract(TimeSpan.FromDays(lastXDays)));
            }

            var unsubscribedContactIDs = ContactInfoProvider.GetContacts().Column("ContactID").WhereIn("ContactEmail", unsubscribedEmails);

            if (getUnsubscribed)
            {
                return ContactInfoProvider.GetContacts().WhereIn("ContactID", unsubscribedContactIDs);
            }

            return ContactInfoProvider.GetContacts().WhereNotIn("ContactID", unsubscribedContactIDs);
        }
    }
}