using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class CMSContactHasPurchasedNumberOfProductsInTheLastXDaysInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        /// <summary>
        /// Translates CMSContactHasPurchasedNumberOfProductsInTheLastXDays Macro rule.
        /// Contact {_perfectum} purchased at least {num} product(s) in the last {days} day(s)
        /// </summary>
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value;
            int num = ValidationHelper.GetInteger(ruleParameters["num"].Value, 0);
            int days = ValidationHelper.GetInteger(ruleParameters["days"].Value, 0);
            
            var whenThenStatements = new Dictionary<string, string>
            {
                {    
                    new WhereCondition().WhereEquals(SqlHelper.GetIsNumeric("ActivityValue").AsExpression(), 1).ToString(true), 
                    SqlHelper.GetConvert("ActivityValue", "INT")
                }
            };

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.PURCHASEDPRODUCT)
                                                 .Column("ActivityContactID")
                                                 .GroupBy("ActivityContactID")
                                                 .Having(w => w.Where(
                                                     new AggregatedColumn(AggregationType.Sum, SqlHelper.GetCase(whenThenStatements, elseCase: "1")), QueryOperator.LargerOrEquals, num)
                                                 );

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