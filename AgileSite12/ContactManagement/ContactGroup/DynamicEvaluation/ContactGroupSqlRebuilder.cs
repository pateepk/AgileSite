using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Rebuilds the whole contact group. Rebuild is done by converting dynamic macro condition to SQL query and thus leaving most of 
    /// the work on the SQL Server.
    /// </summary>
    internal class ContactGroupSqlRebuilder
    {
        private readonly ContactGroupInfo mContactGroup;
        private readonly MacroRuleTreeEvaluator mEvaluator;


        /// <summary>
        /// Constructor.
        /// </summary>
        public ContactGroupSqlRebuilder(ContactGroupInfo contactGroup)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            if (string.IsNullOrEmpty(contactGroup.ContactGroupDynamicCondition))
            {
                throw new InvalidOperationException("[ContactGroupSqlRebuilder]: ContactGroupSqlRebuilder can work only with dynamic contact groups");
            }

            mContactGroup = contactGroup;
            var instanceTranslator = new MacroRuleInstanceToDataQueryTranslator();
            var treeTranslator = new MacroRuleTreeToDataQueryTranslator(instanceTranslator);
            var macroRuleTree = CachedMacroRuleTrees.GetParsedTree(mContactGroup.ContactGroupDynamicCondition);
            mEvaluator = new MacroRuleTreeEvaluator(treeTranslator);
            mEvaluator.SetMacroRuleTree(macroRuleTree);
        }


        /// <summary>
        /// Checks whether the contact group has been set and can be recalculated.
        /// </summary>
        public bool CanBeRebuilt()
        {
            return mEvaluator.CanBeEvaluated();
        }


        /// <summary>
        /// Recalculates the contact group.
        /// </summary>
        public void RebuildGroup()
        {
            IEnumerable<int> contactIDs = mEvaluator.EvaluateAllContactIDs();
            ContactGroupMemberInfoProvider.SetContactsAsDynamic(mContactGroup, contactIDs);
        }


        /// <summary>
        /// Reevaluates membership of the given contacts in the contact group.
        /// </summary>
        /// <param name="contactIDs">Contacts whose membership in the group will be reevaluated</param>
        public void RebuildPartOfContactGroup(ISet<int> contactIDs)
        {
            if (contactIDs == null)
            {
                throw new ArgumentNullException("contactIDs");
            }

            var matchedContactIDs = mEvaluator.EvaluateContacts(contactIDs);

            ContactGroupMemberInfoProvider.SetContactsAsDynamic(mContactGroup, matchedContactIDs, contactIDs);
        }
    }
}