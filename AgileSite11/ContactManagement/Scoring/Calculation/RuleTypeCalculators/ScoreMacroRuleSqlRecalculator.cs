using System;
using System.Collections.Generic;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Recalculates macro scoring rules. Recalculation is done by converting dynamic macro condition to SQL query and thus leaving most of 
    /// the work on the SQL Server.
    /// </summary>
    internal class ScoreMacroRuleSqlRecalculator
    {
        private readonly RuleInfo mRule;
        private readonly MacroRuleTreeEvaluator mEvaluator;
        

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScoreMacroRuleSqlRecalculator(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (string.IsNullOrEmpty(rule.RuleCondition))
            {
                throw new InvalidOperationException("ScoreMacroRuleSqlRecalculator can work only with rule of type Macro");
            }

            mRule = rule;
            var instanceTranslator = new MacroRuleInstanceToDataQueryTranslator();
            var treeTranslator = new MacroRuleTreeToDataQueryTranslator(instanceTranslator);
            var macroRuleTree = CachedMacroRuleTrees.GetParsedTree(RuleHelper.GetMacroConditionFromRule(rule));
            mEvaluator = new MacroRuleTreeEvaluator(treeTranslator);
            mEvaluator.SetMacroRuleTree(macroRuleTree);
        }


        /// <summary>
        /// Checks whether the scoring rule can be recalculated with this class.
        /// </summary>
        public bool CanBeRecalculated()
        {
            return mEvaluator.CanBeEvaluated();
        }


        /// <summary>
        /// Recalculates the given rule.
        /// </summary>
        public void RecalculateRule()
        {
            IEnumerable<int> contactIDs = mEvaluator.EvaluateAllContactIDs();
            ScoreContactRuleInfoProvider.AddScoreContactRules(mRule, contactIDs, mRule.RuleValue);
        }


        /// <summary>
        /// Recalculates the given rule for contacts.
        /// </summary>
        /// <param name="contactsToRebuild">Contacts whose rule points will be recalculated</param>
        public void RecalculateRuleForContacts(IEnumerable<int> contactsToRebuild)
        {
            if (contactsToRebuild == null)
            {
                throw new ArgumentNullException(nameof(contactsToRebuild));
            }

            IEnumerable<int> contactIDs = mEvaluator.EvaluateContacts(contactsToRebuild);
            ScoreContactRuleInfoProvider.AddScoreContactRules(mRule, contactIDs, mRule.RuleValue);
        }

    }
}