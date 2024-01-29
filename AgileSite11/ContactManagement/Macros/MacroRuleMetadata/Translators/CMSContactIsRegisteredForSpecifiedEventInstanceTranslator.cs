using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactIsRegisteredForSpecifiedEventInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactIsRegisteredForSpecifiedEvent Macro rule.
        /// Contact {_is} registered for event {item}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string paramIs = ruleParameters["_is"].Value;
            string nodeAliasPath = ruleParameters["item"].Value;

            var documentIds = new TreeProvider().SelectNodes()
                                                .All()
                                                .WhereEquals("NodeAliasPath", nodeAliasPath)
                                                .Column("DocumentID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.EVENT_BOOKING)
                                                 .WhereIn("ActivityItemDetailID", documentIds)
                                                 .Column("ActivityContactID");

            var contacts = ContactInfoProvider.GetContacts();
            if (paramIs == "!")
            {
                contacts.WhereNotIn("ContactID", activities);
            }
            else
            {
                contacts.WhereIn("ContactID", activities);
            }

            return contacts;
        }
    }
}