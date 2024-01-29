using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents activity rule definition.
    /// </summary>
    internal class RuleActivityItem : RuleItem
    {
        #region "Nested class"

        /// <summary>
        /// Represents field definition of an activity rule.
        /// </summary>
        public class RuleActivityField
        {
            #region "Variables"

            private Hashtable mParameters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            #endregion


            #region "Properties"

            /// <summary>
            /// Activity field name.
            /// </summary>
            public string Name
            {
                get;
                set;
            }


            /// <summary>
            /// Activity field value.
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
            /// Constructor.
            /// </summary>
            public RuleActivityField()
            {
            }


            /// <summary>
            /// Constructor, initializes the activity field info instance from given properties.
            /// </summary>
            /// <param name="name">Field name</param>
            /// <param name="value">Field value</param>
            public RuleActivityField(string name, string value)
            {
                Name = name;
                Value = value;
            }


            /// <summary>
            /// Constructor, initializes the activity field info instance from the given XML node.
            /// </summary>
            /// <param name="fieldNode">XML node with the field data</param>
            public RuleActivityField(XmlNode fieldNode)
            {
                if (fieldNode == null)
                {
                    return;
                }

                // Get activity field name
                Name = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["name"], string.Empty);

                // Get activity field value
                var valueNode = fieldNode.SelectSingleNode("value");
                if (valueNode != null)
                {
                    Value = valueNode.InnerText;
                }

                // Process params elements
                var paramsNode = fieldNode.SelectSingleNode("params");
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
            /// Returns activity field name.
            /// </summary>
            public override string ToString()
            {
                return Name;
            }


            /// <summary>
            /// Clones the object.
            /// </summary>
            /// <returns>Returns RuleActivityField</returns>
            public RuleActivityField Clone()
            {
                RuleActivityField raf = new RuleActivityField();

                raf.Name = Name;
                raf.Value = Value;
                raf.Parameters = (Hashtable)Parameters.Clone();

                return raf;
            }


            /// <summary>
            /// Returns the XML node representing the RuleActivityField object.
            /// </summary>
            public XmlNode GetXmlNode(XmlDocument doc)
            {
                if ((doc != null) && (doc.DocumentElement != null))
                {
                    // Activity field node to add
                    var fieldNode = doc.CreateElement("field");

                    // Add name to a new field node
                    var attribute = doc.CreateAttribute("name");
                    attribute.Value = Name;
                    fieldNode.Attributes.Append(attribute);

                    // Add value to a new value node
                    if (!string.IsNullOrEmpty(Value))
                    {
                        var valueNode = doc.CreateElement("value");
                        valueNode.InnerText = Value;

                        // Add new value node to the activity field node
                        fieldNode.AppendChild(valueNode);
                    }

                    if (Parameters.Keys.Count > 0)
                    {
                        var paramsNode = doc.CreateElement("params");

                        paramsNode.AddChildElements(Parameters);
                        if (paramsNode.HasChildNodes)
                        {
                            fieldNode.AppendChild(paramsNode);
                        }
                    }

                    if (fieldNode.ChildNodes.Count > 0)
                    {
                        // Return field node only if it contains value or a parameter
                        return fieldNode;
                    }
                }

                return null;
            }

            #endregion
        }

        #endregion


        #region "Variables"

        private List<RuleActivityField> mFields;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Activity type name.
        /// </summary>
        public string ActivityName
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the list of activity fields' definitions.
        /// </summary>
        public List<RuleActivityField> Fields
        {
            get
            {
                return mFields ?? (mFields = new List<RuleActivityField>());
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns activity type name.
        /// </summary>
        public override string ToString()
        {
            return ActivityName;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public RuleActivityItem()
        {
        }


        /// <summary>
        /// Constructor, initializes the activity rule info instance from the given XML node.
        /// </summary>
        /// <param name="ruleNode">XML node with the rule data</param>
        public RuleActivityItem(XmlNode ruleNode)
        {
            if (ruleNode == null)
            {
                return;
            }

            // Get activity type name
            ActivityName = XmlHelper.GetXmlAttributeValue(ruleNode.Attributes["name"], string.Empty);
            
            // Process all activity field nodes
            foreach (XmlNode fieldNode in ruleNode.ChildNodes)
            {
                if (fieldNode.Name.EqualsCSafe("field", true))
                {
                    RuleActivityField raf = new RuleActivityField(fieldNode);
                    
                    Fields.Add(raf);
                }
            }
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>Returns RuleAttributeInfo</returns>
        public override RuleItem Clone()
        {
            RuleActivityItem rai = new RuleActivityItem();

            rai.ActivityName = ActivityName;

            foreach (RuleActivityField item in Fields)
            {
                rai.Fields.Add(item.Clone());
            }
            return rai;
        }


        /// <summary>
        /// Returns the XML node representing the RuleActivityInfo object.
        /// </summary>
        public override XmlNode GetXmlNode(XmlDocument doc)
        {
            if ((doc == null) || (doc.DocumentElement == null))
            {
                return null;
            }

            // Activity node to add
            var activityNode = doc.CreateElement("activity");

            // Add name to a new activity node
            var attribute = doc.CreateAttribute("name");
            attribute.Value = ActivityName;
            activityNode.Attributes.Append(attribute);

            // Add fields nodes
            foreach (RuleActivityField item in Fields)
            {
                var itemNode = item.GetXmlNode(doc);
                if (itemNode != null)
                {
                    activityNode.AppendChild(itemNode);
                }
            }

            return activityNode;
        }

        #endregion
    }
}