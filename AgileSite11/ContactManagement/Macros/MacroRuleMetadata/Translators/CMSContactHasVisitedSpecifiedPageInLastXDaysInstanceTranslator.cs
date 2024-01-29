using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasVisitedSpecifiedPageInLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasVisitedSpecifiedPageInLastXDays
        /// Contact {_perfectum} visited page {item} in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string stringGuid = ruleParameters["item"].Value;
            string perfectum = ruleParameters["_perfectum"].Value;
            Guid nodeGuid = stringGuid.ToGuid(Guid.Empty);
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);
            
            if (nodeGuid == Guid.Empty)
            {
                MacroValidationHelper.LogInvalidGuidParameter("VisitedPage", stringGuid);
                return new ObjectQuery<ContactInfo>().NoResults();
            }

            var nodeIDsQuery = new TreeProvider().SelectNodes()
                                                 .All()
                                                 .WhereEquals("NodeGUID", nodeGuid)
                                                 .Column("NodeID");

            var activitiesQuery = ActivityInfoProvider.GetActivities()
                                                      .WhereEquals("ActivityType", PredefinedActivityType.PAGE_VISIT)
                                                      .WhereIn("ActivityNodeID", nodeIDsQuery)
                                                      .Columns("ActivityContactID");

            if (lastXDays > 0)
            {
                activitiesQuery.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            var contacts = ContactInfoProvider.GetContacts();
            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", activitiesQuery);
            }
            else
            {
                contacts.WhereIn("ContactID", activitiesQuery);
            }

            return contacts;
        }
    }
}