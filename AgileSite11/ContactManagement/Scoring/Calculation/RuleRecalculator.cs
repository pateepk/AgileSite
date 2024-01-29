using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.ContactManagement;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(IRuleRecalculator), typeof(RuleRecalculator), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// This class performs tasks which are required to be done when recalculating score points for single rules. Those tasks include
    /// clearing cache after recalculation or calling right calculator (based on rule type). Calculation itself is handled by classes implementing
    /// <see cref="IRuleTypeCalculator"/> interface.
    /// </summary>
    internal class RuleRecalculator : IRuleRecalculator
    {
        private readonly IRuleTypeCalculatorFactory mRuleTypeCalculatorFactory;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ruleTypeCalculatorFactory">Factory constructing calculators for needed rule types</param>
        /// <exception cref="ArgumentNullException"><paramref name="ruleTypeCalculatorFactory"/> is null</exception>
        public RuleRecalculator(IRuleTypeCalculatorFactory ruleTypeCalculatorFactory)
        {
            if (ruleTypeCalculatorFactory == null)
            {
                throw new ArgumentNullException("ruleTypeCalculatorFactory");
            }

            mRuleTypeCalculatorFactory = ruleTypeCalculatorFactory;
        }


        /// <summary>
        /// Recalculates score points for all contacts and one specified rule.
        /// </summary>
        /// <param name="rule">Score points for this rule will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is null</exception>
        public void RecalculateRuleForAllContacts(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            var calculator = mRuleTypeCalculatorFactory.GetRuleCalculator(rule.RuleType);

            ScoreContactRuleInfoProvider.DeleteScoreContactRule(rule.RuleScoreID, null, rule.RuleID);
            calculator.CalculateAllContacts(rule);

            CacheHelper.TouchKey(String.Format("om.score|byid|{0}|children|om.scorecontactlist", rule.RuleScoreID));
        }


        /// <summary>
        /// Recalculates score points for a set of contacts and one specified rule.
        /// </summary>
        /// <param name="rule">Score points for this rule will be recalculated</param>
        /// <param name="contactIDs">ContactIDs whose points will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="contactIDs"/> is null</exception>
        public void RecalculateRuleForContacts(RuleInfo rule, ISet<int> contactIDs)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (contactIDs == null)
            {
                throw new ArgumentNullException("contactIDs");
            }

            var calculator = mRuleTypeCalculatorFactory.GetRuleCalculator(rule.RuleType);

            ScoreContactRuleInfoProvider.DeleteScoreContactRules(rule.RuleID, contactIDs);
            calculator.CalculateContacts(rule, contactIDs);

            CacheHelper.TouchKey(String.Format("om.score|byid|{0}|children|om.scorecontactlist", rule.RuleScoreID));
        }


        /// <summary>
        /// Recalculates score points for one specified rule and one contact.
        /// </summary>
        /// <param name="rule">Score points for this rule will be recalculated</param>
        /// <param name="contact">Score points for this rule will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> or <paramref name="contact"/> is null</exception>
        public void RecalculateRuleForContact(RuleInfo rule, ContactInfo contact)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            ScoreContactRuleInfoProvider.DeleteScoreContactRule(rule.RuleScoreID, contact.ContactID, rule.RuleID);
            var calculator = mRuleTypeCalculatorFactory.GetRuleCalculator(rule.RuleType);

            calculator.CalculateContact(rule, contact);

            CacheHelper.TouchKey(String.Format("om.score|byid|{0}|children|om.scorecontactlist", rule.RuleScoreID));
        }
    }
}