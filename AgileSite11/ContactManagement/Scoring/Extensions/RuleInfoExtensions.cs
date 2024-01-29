using System;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Extensions of <see cref="RuleInfo"/>.
    /// </summary>
    internal static class RuleInfoExtensions
    {
        /// <summary>
        /// Indicates whether given activity or macro rule should be recalculated for given performed activity.
        /// </summary>
        /// <returns>True when the rule should be recalculated, false otherwise</returns>
        /// <param name="rule">Rule to check the activity type for</param>
        /// <param name="activity">Activity to check affection for</param>
        /// <exception cref="ArgumentException"><paramref name="rule"/> type has to be macro rule</exception>
        public static bool IsAffectedByActivity(this RuleInfo rule, ActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            switch (rule.RuleType)
            {
                case RuleTypeEnum.Activity:
                    return rule.IsActivityRuleAffected(activity);
                case RuleTypeEnum.Macro:
                    return rule.IsMacroRuleAffectedByActivity(activity);
                default:
                    throw new ArgumentException("[RuleInfoExtensions.IsAffectedByActivity]: Only Activity and Macro rules can be checked if they are affected by activity", "rule");
            }
        }


        /// <summary>
        /// Checks whether given contact rule is affected by contact attribute change, that means if it needs to be recalculated when attribute has changed.
        /// </summary>
        /// <remarks>
        /// If given rule is not macro type, it cannot be determined whether the rule is affected or not. Therefore true is returned in this case.
        /// </remarks>
        /// <param name="rule">Rule to check the attribute affection</param>
        /// <param name="columnChanged">Name of the changed column</param>
        /// <exception cref="ArgumentNullException"><paramref name="columnChanged"/> is null</exception>
        public static bool IsAffectedByAttributeChange(this RuleInfo rule, string columnChanged)
        {
            if (columnChanged == null)
            {
                throw new ArgumentNullException("columnChanged");
            }

            switch (rule.RuleType)
            {
                case RuleTypeEnum.Macro:
                    return IsMacroRuleAffectedByAttributeChange(rule, columnChanged);
                case RuleTypeEnum.Activity:
                    return false;
                case RuleTypeEnum.Attribute:
                    return rule.RuleParameter.EqualsCSafe(columnChanged, true);
                default:
                    throw new ArgumentException("[RuleInfoExtensions.IsAffectedByAttributeChange]: Unknown type of rule", "rule");
            }
        }
        

        /// <summary>
        /// Checks whether given contact rule is affected by contact attribute change, that means if it needs to be recalculated when attribute has changed.
        /// </summary>
        /// <param name="rule">Rule to check the attribute affection</param>
        /// <param name="columnChanged">Name of the changed column</param>
        private static bool IsMacroRuleAffectedByAttributeChange(this RuleInfo rule, string columnChanged)
        {
            return CachedMacroConditionAnalyzer.IsAffectedByAttributeChange(RuleHelper.GetMacroConditionFromRule(rule), columnChanged);
        }


        /// <summary>
        /// Checks whether given rule is affected by given activity, that means if it needs to be recalculated when it performs.
        /// </summary>
        /// <param name="rule">Rule to check the activity type for</param>
        /// <param name="activity">Activity to check affection for</param>
        private static bool IsMacroRuleAffectedByActivity(this RuleInfo rule, ActivityInfo activity)
        {
            return CachedMacroConditionAnalyzer.IsAffectedByActivityType(RuleHelper.GetMacroConditionFromRule(rule), activity.ActivityType);
        }


        private static bool IsActivityRuleAffected(this RuleInfo rule, ActivityInfo activity)
        {
            if (!activity.ActivityType.EqualsCSafe(rule.RuleParameter, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var parsedRule = new RuleCondition(rule.RuleCondition);
            var ruleActivityItem = parsedRule.ItemsList.First() as RuleActivityItem;

            if (ruleActivityItem != null)
            {
                foreach (var field in ruleActivityItem.Fields)
                {
                    // Get values from rule and performed activity
                    var operatorValue = ValidationHelper.GetInteger(field.Parameters["operator"], (int)TextCompareOperatorEnum.Equals);
                    var operatorEnum = (TextCompareOperatorEnum)operatorValue;
                    var activityValue = activity.GetStringValue(field.Name, string.Empty);
                    string activityFieldType = GetActivityFieldType(field.Name);

                    if ((activityFieldType == FieldDataType.Text)
                        || (activityFieldType == FieldDataType.LongText))
                    {
                        if (operatorEnum == TextCompareOperatorEnum.Equals && String.IsNullOrEmpty(field.Value))
                        {
                            // When there is equality comparer and the filter is set to empty string, we don't know whether it's meant to be
                            // 'all' or exact empty string, therefore the rule should be recalculated always
                            continue;
                        }

                        if (!TextComparer.Compare(activityValue, operatorEnum, field.Value))
                        {
                            return false;
                        }
                    }
                    else if ((activityFieldType == FieldDataType.Integer)
                             || (activityFieldType == FieldDataType.LongInteger))
                    {
                        if (ValidationHelper.GetInteger(field.Value, 0) == 0)
                        {
                            // When field value is not set or is set to the default value, system ignores this field and contact must match always
                            continue;
                        }

                        if (!TextComparer.Compare(activityValue, operatorEnum, field.Value))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Gets data type of activity fields.
        /// </summary>
        private static string GetActivityFieldType(string columnName)
        {
            switch (columnName)
            {
                case "ActivityURL":
                case "ActivityTitle":
                case "ActivityCampaign":
                case "ActivityCulture":
                case "ActivityURLReferrer":
                case "ActivityValue":
                    return FieldDataType.Text;
                case "ActivityComment":
                    return FieldDataType.LongText;
                case "ActivityNodeID":
                case "ActivityItemID":
                    return FieldDataType.Integer;
            }

            return null;
        }
    }
}
