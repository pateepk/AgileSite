using System;
using System.Collections.Generic;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Defines methods, which are able to recalculate score points for a certain rule type.
    /// </summary>
    /// <remarks>
    /// Before calling any of the methods in this interface, ensure that previously saved score points which can be overwritten are cleared 
    /// via <see cref="ScoreContactRuleInfoProvider.DeleteScoreContactRule"/>. Please use <see cref="RuleTypeCalculatorFactory"/> class to obtain instances implementing this interface.
    /// </remarks>
    internal interface IRuleTypeCalculator
    {
        /// <summary>
        /// Recalculates points for one contact for one specific rule.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule and contact must be cleared.
        /// </remarks>
        /// <param name="rule">Points for this rule will be recalculated</param>
        /// <param name="contact">Points for this contact will be recalculated</param>
        void CalculateContact(RuleInfo rule, ContactInfo contact);


        /// <summary>
        /// Recalculates points for a set of contacts for one specific rule.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule and contacts must be cleared.
        /// </remarks>
        /// <param name="rule">Points for this rule will be recalculated</param>
        /// <param name="contactIDs">Points for contacts identified by those IDs will be recalculated</param>
        void CalculateContacts(RuleInfo rule, ISet<int> contactIDs);


        /// <summary>
        /// Recalculates points for all contacts for one specified rule.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule must be cleared.
        /// </remarks>
        /// <param name="rule">Points for this rule will be recalculated</param>
        void CalculateAllContacts(RuleInfo rule);
    }
}