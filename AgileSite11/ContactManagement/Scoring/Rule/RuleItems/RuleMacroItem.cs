using System.Xml;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents macro rule definition.
    /// </summary>
    internal class RuleMacroItem : RuleItem
    {
        /// <summary>
        /// Value of macro condition.
        /// </summary>
        public string MacroValue
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor, initializes the macro field to given string value.
        /// </summary>
        /// <param name="macroValue">Value of macro rule</param>
        public RuleMacroItem(string macroValue)
        {
            MacroValue = macroValue;
        }


        /// <summary>
        /// Constructor, initializes the macro field info instance from the given XML node.
        /// </summary>
        /// <param name="fieldNode">XML node with the field data</param>
        public RuleMacroItem(XmlNode fieldNode)
        {
            if (fieldNode == null)
            {
                return;
            }
            
            XmlNode valueNode = fieldNode.SelectSingleNode("value");
            if (valueNode != null)
            {
                MacroValue = valueNode.InnerText;
            }
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>Cloned RuleMacroInfo</returns>
        public override RuleItem Clone()
        {
            var clonedRuleMacroInfo = new RuleMacroItem(MacroValue);

            return clonedRuleMacroInfo;
        }


        /// <summary>
        /// Returns the XML node representing the macro condition.
        /// </summary>
        public override XmlNode GetXmlNode(XmlDocument doc)
        {
            if ((doc == null) || (doc.DocumentElement == null))
            {
                return null;
            }

            // Macro node to add
            var macroNode = doc.CreateElement("macro");

            var valueNode = doc.CreateElement("value");
            valueNode.InnerText = MacroValue;

            // Add new value node to the macro node
            macroNode.AppendChild(valueNode);

            return macroNode;
        }
    }
}
