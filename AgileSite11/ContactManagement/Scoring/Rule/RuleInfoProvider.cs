using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing RuleInfo management.
    /// </summary>
    public class RuleInfoProvider : AbstractInfoProvider<RuleInfo, RuleInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public RuleInfoProvider()
            : base(RuleInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods"
        
        /// <summary>
        /// Returns a query for all the RuleInfo objects.
        /// </summary>
        public static ObjectQuery<RuleInfo> GetRules()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns rule with specified ID.
        /// </summary>
        /// <param name="ruleId">Rule ID</param>
        public static RuleInfo GetRuleInfo(int ruleId)
        {
            return ProviderObject.GetInfoById(ruleId);
        }


        /// <summary>
        /// Returns rule with specified GUID.
        /// </summary>
        /// <param name="ruleGuid">Rule GUID</param>
        public static RuleInfo GetRuleInfo(Guid ruleGuid)
        {
            return ProviderObject.GetInfoByGuid(ruleGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified rule.
        /// </summary>
        /// <param name="ruleObj">Rule to be set</param>
        public static void SetRuleInfo(RuleInfo ruleObj)
        {
            ProviderObject.SetInfo(ruleObj);
        }


        /// <summary>
        /// Deletes specified rule.
        /// </summary>
        /// <param name="ruleObj">Rule to be deleted</param>
        public static void DeleteRuleInfo(RuleInfo ruleObj)
        {
            ProviderObject.DeleteInfo(ruleObj);
        }


        /// <summary>
        /// Deletes rule with specified ID.
        /// </summary>
        /// <param name="ruleId">Rule ID</param>
        public static void DeleteRuleInfo(int ruleId)
        {
            RuleInfo ruleObj = GetRuleInfo(ruleId);
            DeleteRuleInfo(ruleObj);
        }


        /// <summary>
        /// Returns RuleInfo objects based on item name (attribute or activity name).
        /// </summary>
        /// <param name="ruleParameter">Item name</param>
        /// <param name="ruleType">Item type - attribute or activity</param>
        public static ObjectQuery<RuleInfo> LoadRulesByRuleParameter(string ruleParameter, RuleTypeEnum ruleType)
        {
            return ProviderObject.LoadRulesByRuleParameterInternal(ruleParameter, ruleType);
        }


        /// <summary>
        /// Returns RuleInfo objects based on macros.
        /// </summary>
        public static ObjectQuery<RuleInfo> LoadMacroRules()
        {
            return LoadRulesByRuleParameter(null, RuleTypeEnum.Macro);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(RuleInfo info)
        {
            // Reset where condition property
            info.ClearWhereCondition();

            // If some properties changed, update score to status where it needs to be recalculated
            var refreshScore = info.ChangedColumns().Except(new[] { "RuleDisplayName", "RuleCodeName" }).Any();
            if (refreshScore)
            {
                RefreshScore(info.RuleScoreID);
            }

            // Set RuleIsPersona flag
            if (info.RuleID == 0)
            {
                info.RuleBelongsToPersona = ScoreInfoProvider.GetScoreInfo(info.RuleScoreID).ScoreBelongsToPersona;
            }

            base.SetInfo(info);
        }


        /// <summary>
        /// Returns RuleInfo objects based on item name (attribute or activity name).
        /// </summary>
        /// <param name="ruleParameter">Item name</param>
        /// <param name="ruleType">Item type - attribute or activity</param>
        protected virtual ObjectQuery<RuleInfo> LoadRulesByRuleParameterInternal(string ruleParameter, RuleTypeEnum ruleType)
        {
            return GetRules()
                .From(new QuerySource("OM_Rule").Join("OM_Score", "OM_Rule.RuleScoreID", "OM_Score.ScoreID"))
                .WhereEquals("ScoreEnabled", true)
                .WhereEquals("RuleType", ruleType)
                .WhereEquals("RuleParameter", ruleParameter);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Sets score to status where recalculation might be needed.
        /// </summary>
        private void RefreshScore(int scoreID)
        {
            ScoreInfo score = ScoreInfoProvider.GetScores().WhereEquals("ScoreID", scoreID).TopN(1).FirstOrDefault();
            if (score != null)
            {
                score.ScoreStatus = ScoreStatusEnum.RecalculationRequired;
                ScoreInfoProvider.SetScoreInfo(score);
            }
        }

        #endregion
    }
}