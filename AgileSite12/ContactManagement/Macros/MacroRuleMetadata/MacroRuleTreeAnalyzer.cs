using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods to analyze a MacroRuleTree.
    /// </summary>
    public class MacroRuleTreeAnalyzer
    {
        /// <summary>
        /// Checks whether a macro tree can be translated to a <see cref="DataQuery" />.
        /// </summary>
        /// <param name="macroRuleTree">Macro rule tree</param>
        /// <returns>True when the macro rule tree can be translated to DataQuery</returns>
        public static bool CanTreeBeTranslated(MacroRuleTree macroRuleTree)
        {
            if (macroRuleTree == null)
            {
                throw new ArgumentNullException("macroRuleTree");
            }

            var instanceTranslator = new MacroRuleInstanceToDataQueryTranslator();
            var treeTranslator = new MacroRuleTreeToDataQueryTranslator(instanceTranslator);

            return treeTranslator.CanBeTranslated(macroRuleTree);
        }


        /// <summary>
        /// Gets all attributes that can affect given <see cref="MacroRuleTree"/>. Uses <see cref="MacroRuleMetadata.ALL_ATTRIBUTES"/> to mark that it
        /// should be recalculated no matter what attribute changed.
        /// </summary>
        /// <param name="tree">Macro rule tree</param>
        /// <returns>Set of attributes that affect given Macro rule tree</returns>
        public static ISet<string> GetAffectingAttributeNames(MacroRuleTree tree)
        {
            return GetAffectingItems(tree, metadata => metadata.AffectingAttributes);
        }


        /// <summary>
        /// Gets all activities that can affect given <see cref="MacroRuleTree"/>. Uses <see cref="MacroRuleMetadata.ALL_ACTIVITIES"/> to mark that it
        /// should be recalculated no matter what activity performed.
        /// </summary>
        /// <param name="tree">Macro rule tree</param>
        /// <returns>Set of activities that affect given Macro rule tree</returns>
        public static ISet<string> GetAffectingActivityCodeNames(MacroRuleTree tree)
        {
            return GetAffectingItems(tree, metadata => metadata.AffectingActivities);
        }


        internal static bool IsAffectedByActivityType(MacroRuleTree tree, string activityType)
        {
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }
            if (activityType == null)
            {
                throw new ArgumentNullException("activityType");
            }

            var affectedActivities = GetAffectingActivityCodeNames(tree);

            return affectedActivities.Contains(MacroRuleMetadata.ALL_ACTIVITIES) || affectedActivities.Contains(activityType);
        }


        internal static bool IsAffectedByAttributeChange(MacroRuleTree tree, string attributeName)
        {
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }

            var affectedByAttributeChange = GetAffectingAttributeNames(tree);

            return affectedByAttributeChange.Contains(MacroRuleMetadata.ALL_ATTRIBUTES) || affectedByAttributeChange.Contains(attributeName, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Gets all items that can affect given <see cref="MacroRuleTree"/>. 
        /// </summary>
        /// <param name="tree">Macro rule tree</param>
        /// <param name="type">Provide attribute which should be selected</param>
        /// <returns>Set of items that affect given Macro rule tree</returns>
        private static ISet<string> GetAffectingItems(MacroRuleTree tree, Func<MacroRuleMetadata, IList<string>> type)
        {
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            var affectingItemsList = new HashSet<string>();

            tree.Accept(macroRuleTree =>
            {
                if (macroRuleTree.RuleName != null)
                {
                    if (MacroRuleMetadataContainer.IsMetadataAvailable(macroRuleTree.RuleName))
                    {
                        var metadata = MacroRuleMetadataContainer.GetMetadata(macroRuleTree.RuleName);
                        affectingItemsList.UnionWith(type(metadata));
                    }
                    else
                    {
                        affectingItemsList.Add(MacroRuleMetadata.ALL_ATTRIBUTES);
                        affectingItemsList.Add(MacroRuleMetadata.ALL_ACTIVITIES);
                    }
                }
            });

            return affectingItemsList;
        }
    }
}
