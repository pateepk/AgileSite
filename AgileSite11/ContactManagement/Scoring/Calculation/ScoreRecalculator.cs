using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Activities;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.EventLog;

[assembly: RegisterImplementation(typeof (IScoreRecalculator), typeof (ScoreRecalculator), Priority = CMS.Core.RegistrationPriority.SystemDefault, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Recalculates more than one scoring rule (for example all rules for one contact or all rules after contact performed activity). If you want to recalculate one rule only, use <see cref="RuleRecalculator"/>. 
    /// </summary>
    internal class ScoreRecalculator : IScoreRecalculator
    {
        private class ContactsAffectedByRule
        {
            public RuleInfo Rule
            {
                get;
                set;
            }


            public ISet<int> AffectedContactIDs
            {
                get;
                set;
            }
        }


        #region "Fields"

        private readonly IRuleRecalculator mRuleRecalculator;
        private readonly ICachedRulesManager mCachedRulesManager;
        private readonly IRuleWithContactActionsMatchersFactory mRuleMatchersFactory;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ruleRecalculator">Class handling recalculation of individual rules</param>
        /// <param name="cachedRulesManager">Cached macro rules</param>
        /// <param name="ruleMatchersFactory">Factory for producing rule matchers</param>
        public ScoreRecalculator(IRuleRecalculator ruleRecalculator, ICachedRulesManager cachedRulesManager, IRuleWithContactActionsMatchersFactory ruleMatchersFactory)
        {
            if (ruleRecalculator == null)
            {
                throw new ArgumentNullException("ruleRecalculator");
            }
            if (cachedRulesManager == null)
            {
                throw new ArgumentNullException("cachedRulesManager");
            }
            if (ruleMatchersFactory == null)
            {
                throw new ArgumentNullException("ruleMatchersFactory");
            }

            mRuleRecalculator = ruleRecalculator;
            mCachedRulesManager = cachedRulesManager;
            mRuleMatchersFactory = ruleMatchersFactory;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Recalculates score points for all rules in one score for all contact.
        /// </summary>
        /// <param name="score">Points for this score will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        public void RecalculateScoreRulesForAllContacts(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException("score");
            }

            using (var h = ScoringEvents.RecalculateScoreForAllContacts.StartEvent(score))
            {
                if (h.CanContinue())
                {
                    try
                    {
                        ScoreInfoProvider.MarkScoreAsRecalculating(score);

                        // Deletion of large number of records may take longer that expected
                        // For example deletion of 2M record takes more than 30 seconds
                        // Also recalculation of large number of records could timeout
                        // Using connection scope with larger timeout to ensure that command will succeed
                        using (var cs = new CMSConnectionScope(false))
                        { 
                            cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                            // Get all rules and recalculate them one by one
                            var rules = RuleInfoProvider.GetRules()
                                                        .WhereEquals("RuleScoreID", score.ScoreID);

                            foreach (var rule in rules)
                            {
                                mRuleRecalculator.RecalculateRuleForAllContacts(rule);
                            }
                        }

                        if (score.ScoreEnabled)
                        {
                            ScoreInfoProvider.MarkScoreAsReady(score);
                        }
                        else
                        {
                            // If score is disabled, it cannot be marked as ready, because it is not being recalculated continuously
                            ScoreInfoProvider.MarkScoreAsRecalculationRequired(score);
                        }
                    }
                    catch (Exception ex)
                    {
                        ScoreInfoProvider.MarkScoreAsFailed(score);
                        EventLogProvider.LogException("OnlineMarketing", "ScoreCalculator", ex);
                    }

                    // Send notification e-mails if any contact exceeds score limit
                    new AllContactsScoreNotificationsChecker().SendAllNotifications(score);
                }
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Recalculates scoring rules for contacts who performed some action. Recalculates only scoring rules which could have been affected by performed activities.
        /// Recalculation is made in batches, so only one operation is made for one scoring rule.
        /// </summary>
        /// <param name="processedActivities">Activities performed by contacts</param>
        /// <param name="contactChanges">Changes of contacts properties</param>
        public void RecalculateScoreRulesAfterContactActionsBatch(IList<ActivityInfo> processedActivities, IList<ContactChangeData> contactChanges)
        {
            if (processedActivities == null)
            {
                throw new ArgumentNullException("processedActivities");
            }
            if (contactChanges == null)
            {
                throw new ArgumentNullException("contactChanges");
            }

            var rulesWithAffectedContacts = GetRulesWithAffectedContacts(processedActivities, contactChanges).ToList();

            if (rulesWithAffectedContacts.Any())
            {
                // Contacts whose points are going to be changed by at least one rule                
                HashSet<int> allAffectedContactIDs = new HashSet<int>(rulesWithAffectedContacts.SelectMany(pair => pair.AffectedContactIDs).OrderBy(x => x));

                using (var h = ScoringEvents.RecalculateAfterContactActionsBatch.StartEvent(allAffectedContactIDs))
                {
                    if (h.CanContinue())
                    {
                        foreach (var ruleContactsPair in rulesWithAffectedContacts)
                        {
                            mRuleRecalculator.RecalculateRuleForContacts(ruleContactsPair.Rule, ruleContactsPair.AffectedContactIDs);
                        }
                    }

                    h.FinishEvent();
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Calculates the pair of scoring rule and contacts whose score points in the rule could have been affected by performed actions.
        /// </summary>
        /// <param name="processedActivities">Activities performed by contacts</param>
        /// <param name="contactChanges">Changes applied to contacts</param>
        /// <returns>Scoring rules and contacts who could be affected by them</returns>
        private IEnumerable<ContactsAffectedByRule> GetRulesWithAffectedContacts(IList<ActivityInfo> processedActivities, IList<ContactChangeData> contactChanges)
        {
            var matchers = mRuleMatchersFactory.CreateRulesMatchers(processedActivities, contactChanges);

            var rules = mCachedRulesManager.GetEnabledRules();

            return from rule in rules
                   let affectedContactIDs = matchers[rule.RuleType].GetAffectedContacts(rule)
                   where affectedContactIDs.Any()
                   select new ContactsAffectedByRule
                   {
                       Rule = rule,
                       AffectedContactIDs = affectedContactIDs,
                   };
        }

        #endregion
    }
}