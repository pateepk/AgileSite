using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasComeToSpecifiedLandingPageInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasComeToSpecifiedLandingPage Macro rule.
        /// Contact {_perfectum} come to landing page {page}
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string nodeAliasPath = ruleParameters["page"].Value;

            var nodeIDs = new TreeProvider().SelectNodes()
                                            .All()
                                            .WhereEquals("NodeAliasPath", nodeAliasPath)
                                            .Column("NodeID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.LANDING_PAGE)
                                                 .WhereIn("ActivityNodeID", nodeIDs)
                                                 .Column("ActivityContactID");

            var contacts = ContactInfoProvider.GetContacts();
            if (perfectum == "!")
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