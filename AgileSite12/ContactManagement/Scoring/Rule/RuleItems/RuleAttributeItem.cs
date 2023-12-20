using System;
using System.Collections;
using System.Xml;

using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents attribute rule definition.
    /// </summary>
    internal class RuleAttributeItem : RuleItem
    {
        #region "Variables"

        private Hashtable mParameters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Public properties"

        /// <summary>
        /// Attribute name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Attribute value.
        /// </summary>
        public string Value
        {
            get;
            set;
        }


        /// <summary>
        /// Parameters pairs (hash table).
        /// </summary>
        public Hashtable Parameters
        {
            get
            {
                return mParameters;
            }
            set
            {
                mParameters = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns attribute name.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public RuleAttributeItem()
        {
        }


        /// <summary>
        /// Constructor, initializes the attribute rule info instance from the given XML node.
        /// </summary>
        /// <param name="ruleNode">XML node with the rule data</param>
        public RuleAttributeItem(XmlNode ruleNode)
        {
            if (ruleNode == null)
            {
                return;
            }

            // Get attribute name
            Name = XmlHelper.GetXmlAttributeValue(ruleNode.Attributes["name"], string.Empty);

            // Get attribute value
            var valueNode = ruleNode.SelectSingleNode("value");
            if (valueNode != null)
            {
                Value = valueNode.InnerText;
            }

            // Process params elements
            var paramsNode = ruleNode.SelectSingleNode("params");
            if (paramsNode != null)
            {
                XmlNodeList paramsNodesList = paramsNode.ChildNodes;

                // Fill hash table with params pairs (parameter name, parameter value)
                foreach (XmlNode paramNode in paramsNodesList)
                {
                    Parameters[paramNode.Name] = paramNode.InnerText;
                }
            }
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>Returns RuleAttributeInfo</returns>
        public override RuleItem Clone()
        {
            RuleAttributeItem rai = new RuleAttributeItem();

            rai.Name = Name;
            rai.Value = Value;
            rai.Parameters = (Hashtable)Parameters.Clone();

            return rai;
        }


        /// <summary>
        /// Returns the XML node representing the RuleAttributeInfo object.
        /// </summary>
        public override XmlNode GetXmlNode(XmlDocument doc)
        {
            if ((doc == null) || (doc.DocumentElement == null))
            {
                return null;
            }

            // Attribute node to add
            var newAttributeNode = doc.CreateElement("attribute");

            // Add name to a new attribute node
            var attribute = doc.CreateAttribute("name");
            attribute.Value = Name;
            newAttributeNode.Attributes.Append(attribute);

            // Add value to a new value node
            if (!string.IsNullOrEmpty(Value))
            {
                var newValueNode = doc.CreateElement("value");
                newValueNode.InnerText = Value;

                // Add new value node to the attribute node
                newAttributeNode.AppendChild(newValueNode);
            }

            if (Parameters.Keys.Count > 0)
            {
                var paramsNode = doc.CreateElement("params");
                
                // Add parameters node and its child nodes
                paramsNode.AddChildElements(Parameters);

                if (paramsNode.HasChildNodes)
                {
                    newAttributeNode.AppendChild(paramsNode);
                }
            }

            return newAttributeNode;
        }

        #endregion
    }
}