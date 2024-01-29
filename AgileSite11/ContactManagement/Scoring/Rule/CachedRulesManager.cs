using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.ContactManagement;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(ICachedRulesManager), typeof(CachedRulesManager), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Loads rules that are eligible for recalculation from cache. 
    /// </summary>
    public class CachedRulesManager : ICachedRulesManager
    {
        /// <summary>
        /// Returns cached attribute RuleInfo objects based on attribute name.
        /// </summary>
        /// <param name="attributeName">Name of the attribute which rules correspond to</param>
        public List<RuleInfo> GetAttributeRulesCached(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentNullException("attributeName");
            }

            return CacheHelper.Cache(
                () => RuleInfoProvider.LoadRulesByRuleParameter(attributeName, RuleTypeEnum.Attribute).ToList(),
                new CacheSettings(60, "ScoringMacroRules", attributeName)
                {
                    CacheDependency = GetCacheDependency()
                });
        }


        /// <summary>
        /// Returns cached activity RuleInfo objects based on activity type name.
        /// </summary>
        /// <param name="activityTypeName">Name of the activity type name which rules correspond to</param>
        public List<RuleInfo> GetActivityRulesCached(string activityTypeName)
        {
            if (string.IsNullOrEmpty(activityTypeName))
            {
                throw new ArgumentNullException("activityTypeName");
            }

            return CacheHelper.Cache(
                () => RuleInfoProvider.LoadRulesByRuleParameter(activityTypeName, RuleTypeEnum.Activity).ToList(),
                new CacheSettings(60, "ScoringActivityRules", activityTypeName)
                {
                    CacheDependency = GetCacheDependency()
                });
        }


        /// <summary>
        /// Returns cached macro RuleInfo objects.
        /// </summary>
        public List<RuleInfo> GetMacroRulesCached()
        {
            return CacheHelper.Cache(
                () => RuleInfoProvider.LoadMacroRules().ToList(),
                new CacheSettings(60, "CachedRulesManager", "GetMacroRulesCached")
                {
                    CacheDependency = GetCacheDependency()
                });
        }


        /// <summary>
        /// Returns cached RuleInfos types belonging to enabled Scores on all sites.
        /// </summary>
        public List<RuleInfo> GetEnabledRules()
        {
            return CacheHelper.Cache(() => RuleInfoProvider.GetRules()
                                                           .Source(s => s.Join<ScoreInfo>("RuleScoreID", "ScoreID"))
                                                           .WhereTrue("ScoreEnabled")
                                                           .ToList(),
                new CacheSettings(20, "CachedRulesManager", "GetEnabledRules")
                {
                    CacheDependency = GetCacheDependency()
                });
        }


        /// <summary>
        /// Gets <see cref="CMSCacheDependency"/> for score/persona rules, so that any change to score or rule will drop the cache.
        /// </summary>
        private CMSCacheDependency GetCacheDependency()
        {
            return CacheHelper.GetCacheDependency(new[]
            {
                RuleInfo.OBJECT_TYPE + "|all",
                RuleInfo.OBJECT_TYPE_PERSONA + "|all",
                ScoreInfo.OBJECT_TYPE + "|all",
                ScoreInfo.OBJECT_TYPE_PERSONA + "|all",
            });
        }
    }
}