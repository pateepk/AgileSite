using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Defines methods, which are able to recalculate score points for a score rules based on macros.
    /// </summary>
    /// <remarks>
    /// Before calling any of the methods in this interface, ensure that previously saved score points which can be overwritten are cleared via
    /// <see cref="ScoreContactRuleInfoProvider.DeleteScoreContactRule"/> method.
    /// </remarks>
    internal class MacroRuleTypeCalculator : IRuleTypeCalculator
    {
        /// <summary>
        /// Size of page used when recalculating multiple contacts.
        /// </summary>
        private const int PAGE_SIZE = 20000;


        /// <summary>
        /// Recalculates points for one contact for one rule based on macro.
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
            if (rule.RuleType != RuleTypeEnum.Macro)
            {
                throw new ArgumentException("This rule calculator is able to recalculate score only for rules based on macros");
            }

            Recalculate(rule, new[] { contact });
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
            if (rule.RuleType != RuleTypeEnum.Macro)
            {
                throw new ArgumentException("This rule calculator is able to recalculate score only for rules based on macros");
            }

            var sqlRecalculator = new ScoreMacroRuleSqlRecalculator(rule);
            if (sqlRecalculator.CanBeRecalculated())
            {
                sqlRecalculator.RecalculateRuleForContacts(contactIDs);
            }
            else
            {
                var contacts = ContactInfoProvider.GetContacts()
                                                  .WhereIn("ContactID", contactIDs.ToList());

                contacts.ForEachPage(page => Recalculate(rule, page), PAGE_SIZE);
            }
        }


        /// <summary>
        /// Recalculates points for all contacts for one specified rule based on macro.
        /// </summary>
        /// <remarks>
        /// Before calling this method, previously saved score points for this rule must be cleared.
        /// 
        /// Paginates contacts by <see cref="PAGE_SIZE"/> and executes recalculation routine for every page.
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
            if (rule.RuleType != RuleTypeEnum.Macro)
            {
                throw new ArgumentException("This rule calculator is able to recalculate score only for rules based on macros");
            }

            var sqlRecalculator = new ScoreMacroRuleSqlRecalculator(rule);
            if (sqlRecalculator.CanBeRecalculated())
            {
                sqlRecalculator.RecalculateRule();
            }
            else
            {
                var score = ScoreInfoProvider.GetScoreInfo(rule.RuleScoreID);

                EventLogProvider.LogWarning("MacroRuleTypeCalculator", "SLOWMACRO", null, 0,
                            "The rule '" + rule.RuleDisplayName + "' in score '" + score.ScoreDisplayName + "' is recalculated using " +
                            "memory recalculation, which is significantly slower than SQL recalculation. Consider using only macro rules " +
                            "translatable to ObjectQuery in your score macro rule condition. " +
                            "Refer to documentation for more information: " + DocumentationHelper.GetDocumentationTopicUrl(DocumentationLinks.Scoring.CUSTOM_MACRO_RECALCULATION_SPEED));

                ContactInfoProvider.GetContacts()
                                   .ForEachPage(page => Recalculate(rule, page), PAGE_SIZE);
            }
        }


        /// <summary>
        /// Recalculates macro rule for specified contacts.
        /// </summary>
        /// <param name="rule">Rule to be recalculated</param>
        /// <param name="contacts">Those contact's score points will be recalculated</param>
        private void Recalculate(RuleInfo rule, IEnumerable<ContactInfo> contacts)
        {
            string macro = RuleHelper.GetMacroConditionFromRule(rule);
            var resolver = MacroResolver.GetInstance();

            foreach (var contact in contacts)
            {
                resolver.SetNamedSourceData("Contact", contact);

                bool contactPassed = ValidationHelper.GetBoolean(resolver.ResolveMacros(macro), false);
                if (contactPassed)
                {
                    var scoreContactRuleInfo = new ScoreContactRuleInfo
                    {
                        ContactID = contact.ContactID,
                        ScoreID = rule.RuleScoreID,
                        RuleID = rule.RuleID,
                        Value = rule.RuleValue
                    };

                    // When saving result of recalculation for contact, 
                    // contact or other objects might not be in database so check for foreign key exceptions
                    try
                    {
                        ScoreContactRuleInfoProvider.SetScoreContactRuleInfo(scoreContactRuleInfo);
                    }
                    catch (Exception ex)
                    {
                        // When saving result of recalculation for contact, 
                        // contact or other objects might not be in database so check for foreign key constraint exceptions
                        // Suppress this exception to continue in recalculation because deleted contact does not need score anymore
                        if (IsSqlConstraintException(ex.InnerException))
                        {
                            continue;
                        }

                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if exception is sql constraint exception.
        /// SqlException number for this type of error is 547, for more details go to <a href="http://msdn.microsoft.com/en-us/library/cc645603.aspx">http://msdn.microsoft.com/en-us/library/cc645603.aspx</a>.
        /// </summary>
        /// <param name="exception">Exception to check for constraint error</param>
        private static bool IsSqlConstraintException(Exception exception)
        {
            var sqlException = exception as SqlException;
            if ((sqlException != null) && (sqlException.Number == 547))
            {
                return true;
            }
            return false;
        }
    }
}