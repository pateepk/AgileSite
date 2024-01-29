using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(ScoringUsageDataSource))]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides statistical information about module.
    /// </summary>
    internal class ScoringUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.OnlineMarketing.Scoring";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            var scores = ScoreInfoProvider.GetScores().WhereFalse("ScoreBelongsToPersona").ToList();

            usageData.Add("TotalScoringCount", scores.Count);
            usageData.Add("EnabledScoringCount", GetTotalEnabledScoringCount(scores));
            usageData.Add("ScoringRulesCountPerRuleType", JsonConvert.SerializeObject(GetNumberOfRulesInScoresGroupedByType()));
            usageData.Add("NumberOfContactsInEachScore", JsonConvert.SerializeObject(GetNumberOfContactsInEachScore()));

            return usageData;
        }


        /// <summary>
        /// Gets total count of enabled scoring within the page.
        /// </summary>
        internal int GetTotalEnabledScoringCount(IEnumerable<ScoreInfo> scores)
        {
            return scores.Count(s => s.ScoreEnabled);
        }


        /// <summary>
        /// Returns number of rule types per each score.
        /// </summary>
        internal List<Dictionary<int, int>> GetNumberOfRulesInScoresGroupedByType()
        {
            return RuleInfoProvider.GetRules()
                                   .WhereFalse("RuleBelongsToPersona")
                                   .Columns("RuleScoreID", "RuleType")
                                   .AddColumn(new CountColumn("RuleScoreID").As("RulesInScore"))
                                   .GroupBy("RuleScoreID", "RuleType")
                                   .Select(dataRow => new
                                   {
                                       RuleScoreID = dataRow[0].ToInteger(0),
                                       RuleType = dataRow[1].ToInteger(0),
                                       RulesInScore = dataRow[2].ToInteger(0)
                                   })
                                   .GroupBy(r => r.RuleScoreID, r => new KeyValuePair<int, int>(r.RuleType, r.RulesInScore))
                                   .Select(g => g.ToDictionary(pair => pair.Key, pair => pair.Value))
                                   .ToList();
        }


        /// <summary>
        /// Returns number of contacts in each score.
        /// </summary>
        internal IList<int> GetNumberOfContactsInEachScore()
        {
            return ScoreContactRuleInfoProvider.GetScoreContactRules()
                                               .Source(s => s.LeftJoin<RuleInfo>("RuleID", "RuleID"))
                                               .Column("ScoreID")
                                               .AddColumn(new CountColumn("ScoreID").As("ContactsInScore"))
                                               .WhereFalse("RuleBelongsToPersona")
                                               .GroupBy("ScoreID")
                                               .AsNested()
                                               .Column("ContactsInScore")
                                               .GetListResult<int>();
        }
    }
}
