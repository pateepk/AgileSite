using System;

using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class that parses a macro condition <see cref="MacroRuleTree"/> from a <see cref="String"/>.
    /// </summary>
    internal static class MacroRuleTreeParser
    {
        /// <summary>
        /// Parses macro condition (for example in a contact group) to a <see cref="MacroRuleTree"/>.
        /// </summary>
        /// <param name="macroCondition">Macro condition as a string</param>
        /// <returns>MacroRuleTree that is parsed from <paramref name="macroCondition"/> or null if it cannot be parsed</returns>
        /// <exception cref="ArgumentNullException"><paramref name="macroCondition"/> cannot be null</exception>
        public static MacroRuleTree TryParse(string macroCondition)
        {
            if (macroCondition == null)
            {
                throw new ArgumentNullException("macroCondition");
            }

            string xml = MacroProcessor.RemoveDataMacroBrackets(MacroSecurityProcessor.RemoveSecurityParameters(macroCondition, false, null));
            var macroExpression = MacroExpression.ExtractParameter(xml, "rule", 1);

            // Input parameter is in wrong format
            if (macroExpression == null || macroExpression.Value == null)
            {
                return null;
            }

            var tree = new MacroRuleTree();
            tree.LoadFromXml(macroExpression.Value.ToString());
            return tree;
        }
    }
}