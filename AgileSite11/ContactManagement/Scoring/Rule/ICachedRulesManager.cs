using System.Collections.Generic;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Loads rules that are eligible for recalculation from cache. 
    /// </summary>
    public interface ICachedRulesManager
    {
        /// <summary>
        /// Returns cached attribute RuleInfo objects based on attribute name.
        /// </summary>
        /// <param name="attributeName">Name of the attribute which rules correspond to</param>
        List<RuleInfo> GetAttributeRulesCached(string attributeName);


        /// <summary>
        /// Returns cached activity RuleInfo objects based on activity type name.
        /// </summary>
        /// <param name="activityTypeName">Name of the activity type name which rules correspond to</param>
        List<RuleInfo> GetActivityRulesCached(string activityTypeName);


        /// <summary>
        /// Returns cached macro RuleInfo objects.
        /// </summary>
        List<RuleInfo> GetMacroRulesCached();


        /// <summary>
        /// Returns cached RuleInfos types belonging to enabled Scores on all sites.
        /// </summary>
        List<RuleInfo> GetEnabledRules();
    }
}