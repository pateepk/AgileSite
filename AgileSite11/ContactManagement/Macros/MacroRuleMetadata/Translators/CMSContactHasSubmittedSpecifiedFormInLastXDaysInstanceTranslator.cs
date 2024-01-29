using System;
using System.Linq;
using System.Text;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasSubmittedSpecifiedFormInLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// CMSContactHasSubmittedSpecifiedFormInLastXDays
        /// Contact {_perfectum} submitted form {item} in last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            string formCodeName = ruleParameters["item"].Value;
            int lastXDays = ruleParameters["days"].Value.ToInteger(0);

            var forms = new DataQuery().From("CMS_Form")
                                       .WhereEquals("FormName", formCodeName)
                                       .Column("FormID");

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.BIZFORM_SUBMIT)
                                                 .WhereIn("ActivityItemID", forms)
                                                 .Column("ActivityContactID");

            if (lastXDays > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(lastXDays));
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