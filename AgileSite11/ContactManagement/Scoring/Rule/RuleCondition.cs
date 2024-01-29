using System;
using System.Collections.Generic;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Ensures management of XML file that represents the rule condition.
    /// </summary>
    public class RuleCondition
    {
        #region "Variables"

        /// <summary>
        /// Items array list.
        /// </summary>
        private readonly List<RuleItem> mItemsList = new List<RuleItem>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns the list of all RuleAttributeInfos or RuleActivityInfos.
        /// </summary>
        internal List<RuleItem> ItemsList
        {
            get
            {
                return mItemsList;
            }
        }


        /// <summary>
        /// Gets or sets where condition of the rule.
        /// </summary>
        public string WhereCondition
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates the rule condition structure and loads specified rule definition.
        /// </summary>
        /// <param name="ruleCondition">XML definition of the rule condition</param>
        public RuleCondition(string ruleCondition)
        {
            if (String.IsNullOrEmpty(ruleCondition))
            {
                return;
            }

            // Load XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ruleCondition);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name.ToLowerCSafe())
                {
                    case "attribute":
                        //Initialize RuleAttributeInfo from XML
                        var attributeInfo = new RuleAttributeItem(node);
                        mItemsList.Add(attributeInfo);
                        break;

                    case "activity":
                        //Initialize RuleActivityInfo from XML
                        var activityInfo = new RuleActivityItem(node);
                        mItemsList.Add(activityInfo);
                        break;

                    case "macro":
                        //Initialize RuleMacroInfo from XML
                        var ruleMacroInfo = new RuleMacroItem(node);
                        mItemsList.Add(ruleMacroInfo);
                        break;

                    case "wherecondition":
                        WhereCondition = node.InnerText;
                        break;
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns rule condition definition in xml.
        /// </summary>
        public string GetXmlDefinition()
        {
            XmlDocument xml = new XmlDocument();
            var docElem = xml.CreateElement("condition");
            xml.AppendChild(docElem);

            // Create XML representation of rule items
            foreach (RuleItem item in mItemsList)
            {
                XmlNode itemNode = item.GetXmlNode(xml);
                if (itemNode != null)
                {
                    docElem.AppendChild(itemNode);
                }
            }

            if (!string.IsNullOrEmpty(WhereCondition))
            {
                // Add where condition
                XmlNode where = xml.CreateElement("wherecondition");
                where.InnerText = WhereCondition;

                // Add new node to xml definition
                docElem.AppendChild(where);
            }

            return xml.ToFormattedXmlString(true);
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        public RuleCondition Clone()
        {
            RuleCondition rc = new RuleCondition(null);

            foreach (RuleItem item in mItemsList)
            {
                rc.ItemsList.Add(item.Clone());
            }
            rc.WhereCondition = WhereCondition;

            return rc;
        }

        #endregion
    }
}