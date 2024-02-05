using System;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.ContactManagement
{
    internal class CMSContactHasVisitedSpecifiedSiteInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasVisitedSpecifiedSiteInTheLastXDays
        /// Contact {_perfectum} visited site {siteName} in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string siteName = ruleParameters["siteName"].Value;
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);

            var siteID = SiteInfoProvider.GetSiteID(siteName);
            var activitiesQuery = ActivityInfoProvider.GetActivities()
                                                      .WhereEquals("ActivityType", PredefinedActivityType.PAGE_VISIT)
                                                      .WhereEquals("ActivitySiteID", siteID)
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