using System;
using System.Xml;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Class represents field validation macro rule.
    /// </summary>
    [Serializable]
    public class FieldMacroRule
    {
        #region "Constants"

        private const string RULE_NODE = "rule";
        private const string ERROR_MESSAGE = "errormsg";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the macro rule.
        /// </summary>
        public string MacroRule
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns XML definition of macro rule.
        /// </summary>
        /// <param name="doc">XML document</param>
        public XmlNode GetXML(XmlDocument doc)
        {
            var ruleNode = doc.CreateElement(RULE_NODE);

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ruleNode.SetAttribute(ERROR_MESSAGE, HTMLHelper.HTMLEncode(ErrorMessage));
            }

            ruleNode.InnerText = MacroRule;
            return ruleNode;
        }


        /// <summary>
        /// Loads macro rule from XML definition.
        /// </summary>
        /// <param name="node">Macro rule XML node</param>
        public void LoadFromXml(XmlNode node)
        {
            if (node != null)
            {
                ErrorMessage = HTMLHelper.HTMLDecode(XmlHelper.GetXmlAttributeValue(node.Attributes[ERROR_MESSAGE], string.Empty));
                MacroRule = node.InnerText;
            }
        }


        /// <summary>
        /// Returns <see cref="CMS.MacroEngine.MacroRuleTree"/> based on this object.
        /// </summary>
        public MacroRuleTree GetMacroRuleTree()
        {
            MacroIdentityOption identityOption;
            string rule = MacroProcessor.RemoveDataMacroBrackets(MacroRule);
            rule = MacroSecurityProcessor.RemoveMacroSecurityParams(rule, out identityOption);
            rule = ValidationHelper.GetString(MacroExpression.ExtractParameter(rule, "rule", 1).Value, string.Empty);

            MacroRuleTree result = new MacroRuleTree();
            result.LoadFromXml(rule);

            return result;
        }

        #endregion
    }
}