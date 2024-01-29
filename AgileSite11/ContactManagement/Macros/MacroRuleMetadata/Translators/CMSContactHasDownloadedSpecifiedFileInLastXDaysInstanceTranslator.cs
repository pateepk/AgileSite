using System;
using System.Linq;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasDownloadedSpecifiedFileInLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasDownloadedSpecifiedFileInLastXDays Macro rule.
        /// Contact {_perfectum} downloaded file {item} in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            Guid nodeGuid = ruleParameters["item"].Value.ToGuid(Guid.Empty);
            int days = ruleParameters["days"].Value.ToInteger(0);

            var nodeIDs = new TreeProvider().SelectNodes()
                                            .All()
                                            .WhereEquals("NodeGUID", nodeGuid)
                                            .Column("NodeID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PAGE_VISIT)
                                                 .WhereIn("ActivityNodeID", nodeIDs)
                                                 .Columns("ActivityContactID");

            if (days > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(days));
            }

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