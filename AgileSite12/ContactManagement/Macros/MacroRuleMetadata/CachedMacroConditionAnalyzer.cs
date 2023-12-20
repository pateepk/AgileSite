using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.ContactManagement
{
    internal static class CachedMacroConditionAnalyzer
    {
        internal static bool IsAffectedByActivityType(string macroCondition, string activityType)
        {
            if (macroCondition == null)
            {
                throw new ArgumentNullException("macroCondition");
            }
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }

            return CacheHelper.Cache(() =>
            {
                var macroRuleTree = CachedMacroRuleTrees.GetParsedTree(macroCondition);

                // The macro is not translatable to MacroRuleTree, so it's most probably something like '{%CustomMacro(Contact)%}', return all activity types
                // to be sure that it gets recalculated.
                if (macroRuleTree == null)
                {
                    return true;
                }

                return MacroRuleTreeAnalyzer.IsAffectedByActivityType(macroRuleTree, activityType);
            }, new CacheSettings(10, "CachedMacroConditionAnalyzer", macroCondition, activityType));
        }


        internal static bool IsAffectedByAttributeChange(string macroCondition, string columnChanged)
        {
            if (macroCondition == null)
            {
                throw new ArgumentNullException("macroCondition");
            }

            if (columnChanged == null)
            {
                throw new ArgumentNullException("columnChanged");
            }

            return CacheHelper.Cache(() =>
            {
                var macroRuleTree = CachedMacroRuleTrees.GetParsedTree(macroCondition);

                // The macro is not translatable to MacroRuleTree, so it's most probably something like '{%CustomMacro(Contact)%}', return all activity types
                // to be sure that it gets recalculated.
                if (macroRuleTree == null)
                {
                    return true;
                }

                return MacroRuleTreeAnalyzer.IsAffectedByAttributeChange(macroRuleTree, columnChanged);
            }, new CacheSettings(10, "CachedMacroConditionAnalyzer", macroCondition, columnChanged));
        }
    }
}