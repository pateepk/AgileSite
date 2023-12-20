using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Evaluates <see cref="MacroRuleTree" /> and returns IDs of contacts that match it.
    /// </summary>
    internal class MacroRuleTreeEvaluator
    {
        private readonly IMacroRuleTreeToDataQueryTranslator mRuleTreeToDataQueryTranslator;
        private MacroRuleTree mMacroRuleTree;


        /// <summary>
        /// Constructor.
        /// </summary>
        public MacroRuleTreeEvaluator(IMacroRuleTreeToDataQueryTranslator ruleTreeToDataQueryTranslator)
        {
            if (ruleTreeToDataQueryTranslator == null)
            {
                throw new ArgumentNullException(nameof(ruleTreeToDataQueryTranslator));
            }

            mRuleTreeToDataQueryTranslator = ruleTreeToDataQueryTranslator;
        }


        /// <summary>
        /// Checks whether the condition that has been set in the <see cref="SetMacroRuleTree"/> property can be rebuilt using this rebuilder.
        /// </summary>
        public bool CanBeEvaluated()
        {
            if (mMacroRuleTree == null)
            {
                return false;
            }

            return mRuleTreeToDataQueryTranslator.CanBeTranslated(mMacroRuleTree);
        }


        /// <summary>
        /// Evaluates the condition set by the <see cref="SetMacroRuleTree"/> method. At first, macro rule tree is converted to data query. This data query is then executed on the database
        /// and a list of ContactIDs is retrieved. Exception is thrown if macro rule tree cannot be converted to data query (can be checked with the <see cref="CanBeEvaluated"/> method.
        /// </summary>
        /// <returns>IDs of contacts satisfying the condition</returns>
        public IEnumerable<int> EvaluateAllContactIDs()
        {
            if (mMacroRuleTree == null)
            {
                throw new Exception("Cannot evaluate contact IDs. Macro rule tree cannot be constructed.");
            }

            Func<ObjectQuery<ContactInfo>, ObjectQuery<ContactInfo>> queryTransformation;
            
            queryTransformation = query => query.Column("ContactID");

            ObjectQuery<ContactInfo> contactIDsQuery = mRuleTreeToDataQueryTranslator.TranslateWithTransformation(mMacroRuleTree, queryTransformation);

            return ReadIDs(contactIDsQuery);
        }


        /// <summary>
        /// Evaluates the condition set by the <see cref="SetMacroRuleTree"/> method. At first, macro rule tree is converted to data query. This data query is then executed on the database
        /// and a list of ContactIDs is retrieved. Exception is thrown if macro rule tree cannot be converted to data query (can be checked with the <see cref="CanBeEvaluated"/> method.
        /// </summary>
        /// <param name="contactIDsToEvaluate">Only those contacts will be checked for compliance with condition</param>
        /// <returns>IDs of contacts satisfying the condition</returns>
        public IEnumerable<int> EvaluateContacts(IEnumerable<int> contactIDsToEvaluate)
        {
            if (mMacroRuleTree == null)
            {
                throw new Exception("Cannot evaluate contact IDs. Macro rule tree cannot be constructed.");
            }
            if (contactIDsToEvaluate == null)
            {
                throw new ArgumentNullException(nameof(contactIDsToEvaluate));
            }

            Func<ObjectQuery<ContactInfo>, ObjectQuery<ContactInfo>> queryTransformation;
            
            queryTransformation = query => query.WhereIn("ContactID", contactIDsToEvaluate.ToList())
                                                .Column("ContactID");

            ObjectQuery<ContactInfo> contactIDsQuery = mRuleTreeToDataQueryTranslator.TranslateWithTransformation(mMacroRuleTree, queryTransformation);

            return ReadIDs(contactIDsQuery);
        }


        /// <summary>
        /// Sets macro rule tree that can be evaluated.
        /// </summary>
        /// <param name="macroRuleTree">Macro rule tree</param>
        internal void SetMacroRuleTree(MacroRuleTree macroRuleTree)
        {
            mMacroRuleTree = macroRuleTree;
        }


        private IEnumerable<int> ReadIDs(IDataQuery idQuery)
        {
            using (var dataReader = idQuery.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.CloseConnection, true))
            {
                while (dataReader.Read())
                {
                    yield return dataReader.GetInt32(0);
                }
            }
        }
    }
}