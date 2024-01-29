using System;
using System.Collections.Generic;

namespace CMS.ContactManagement
{    
    /// <summary>
    /// Recalculates score points for one or more contacts for one scoring rule. 
    /// </summary>
    internal interface IRuleRecalculator
    {
        /// <summary>
        /// Recalculates score points for all contacts and one specified rule.
        /// </summary>
        /// <param name="rule">Score points for this rule will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is null</exception>
        void RecalculateRuleForAllContacts(RuleInfo rule);


        /// <summary>
        /// Recalculates score points for a set of contacts and one specified rule.
        /// </summary>
        /// <param name="rule">Score points for this rule will be recalculated</param>
        /// <param name="contactIDs">ContactIDs whose points will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="contactIDs"/> is null</exception>
        void RecalculateRuleForContacts(RuleInfo rule, ISet<int> contactIDs);


        /// <summary>
        /// Recalculates score points for one specified rule and one contact.
        /// </summary>
        /// <param name="rule">Score points for this rule will be recalculated</param>
        /// <param name="contact">Score points for this rule will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="rule"/> or <paramref name="contact"/> is null</exception>
        void RecalculateRuleForContact(RuleInfo rule, ContactInfo contact);
    }
}