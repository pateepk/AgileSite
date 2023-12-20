using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Defines methods, which are able to recalculate score points for a score rules based on activities.
    /// </summary>
    /// <remarks>
    /// Before calling any of the methods in this interface, ensure that previously saved score points which can be overwritten are cleared via
    /// <see cref="ScoreContactRuleInfoProvider.DeleteScoreContactRule"/> method.
    /// </remarks>
    internal class ActivityRuleTypeCalculator : IRuleTypeCalculator
    {
        /// <summary>
        /// Recalculates points for one contact for one specific rule.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule and contact must be cleared.
        /// </remarks>
        /// <param name="rule">Points for this rule will be recalculated</param>
        /// <param name="contact">Points for this contact will be recalculated</param>
        /// <exception cref="ArgumentException"><paramref name="rule"/> is of type not supported by this instance</exception>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> or <paramref name="contact"/> is null</exception>
        public void CalculateContact(RuleInfo rule, ContactInfo contact)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (rule.RuleType != RuleTypeEnum.Activity)
            {
                throw new ArgumentException("This rule calculator is able to recalculate score only for rules based on activities");
            }
            if (!CheckRuleValidity(rule))
            {
                return;
            }

            ScoreContactRuleInfoProvider.RecalculateRuleWithWhereCondition(rule, new[] { contact.ContactID });
        }


        /// <summary>
        /// Recalculates points for a set of contacts for one specific rule.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule and contacts must be cleared.
        /// </remarks>
        /// <param name="rule">Points for this rule will be recalculated</param>
        /// <param name="contactIDs">Points for contacts identified by those IDs will be recalculated</param>
        public void CalculateContacts(RuleInfo rule, ISet<int> contactIDs)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (contactIDs == null)
            {
                throw new ArgumentNullException(nameof(contactIDs));
            }
            if (rule.RuleType != RuleTypeEnum.Activity)
            {
                throw new ArgumentException("This rule calculator is able to recalculate score only for rules based on activities");
            }
            if (!CheckRuleValidity(rule))
            {
                return;
            }

            ScoreContactRuleInfoProvider.RecalculateRuleWithWhereCondition(rule, contactIDs);
        }


        /// <summary>
        /// Recalculates points for all contacts for one specified rule.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule must be cleared.
        /// </remarks>
        /// <param name="rule">Points for this rule will be recalculated</param>
        /// <exception cref="ArgumentException"><paramref name="rule"/> is of type not supported by this instance</exception>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is null</exception>
        public void CalculateAllContacts(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (rule.RuleType != RuleTypeEnum.Activity)
            {
                throw new ArgumentException("This rule calculator is able to recalculate score only for rules based on activities");
            }
            if (!CheckRuleValidity(rule))
            {
                return;
            }

            ScoreContactRuleInfoProvider.RecalculateRuleWithWhereCondition(rule);
        }


        /// <summary>
        /// Checks if validity in time for this rule is valid. I.e. if rule should be even recalculated.
        /// </summary>
        /// <param name="rule">Rule to check</param>
        /// <returns>True if rule is ok and score should be recalculated, false otherwise</returns>
        private bool CheckRuleValidity(RuleInfo rule)
        {
            return (rule.RuleValidity != ValidityEnum.Until) ||
                   (rule.RuleValidUntil == DateTimeHelper.ZERO_TIME) ||
                   (rule.RuleValidUntil.Date >= DateTime.Now.Date);
        }
    }
}