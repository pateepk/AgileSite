using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Class for storing info about categories.
    /// </summary>
    public class FormCategoryInfo : AbstractDataContainer<FormCategoryInfo>, IDataDefinitionItem
    {
        #region "Variables"

        private Hashtable mProperties = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        private Hashtable mPropertiesMacroTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Public properties"

        /// <summary>
        /// Category name.
        /// </summary>
        public string CategoryName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the category is inherited from parent class.
        /// </summary>
        public bool IsInherited
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if category is not in original form definition.
        /// </summary>
        public bool IsDummy
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if category is not in original form definition.
        /// </summary>
        public bool IsExtra
        {
            get;
            set;
        }


        /// <summary>
        /// Properties (hashtable).
        /// </summary>
        public Hashtable Properties
        {
            get
            {
                return mProperties;
            }
            set
            {
                mProperties = value;
            }
        }


        /// <summary>
        /// Properties macro table (hashtable).
        /// </summary>
        public Hashtable PropertiesMacroTable
        {
            get
            {
                return mPropertiesMacroTable;
            }
            set
            {
                mPropertiesMacroTable = value;
            }
        }

        #endregion


        /// <summary>
        /// Constructor for default FormCategoryInfo.
        /// </summary>
        public FormCategoryInfo()
        {
            SetPropertyValue(FormCategoryPropertyEnum.Visible, "true");
        }


        /// <summary>
        /// Initializes the category instance from given XML data.
        /// </summary>
        /// <param name="categoryNode">XML node with the category data</param>
        public virtual void LoadFromXmlNode(XmlNode categoryNode)
        {
            if (categoryNode.Attributes == null)
            {
                throw new InvalidOperationException("categoryNode does not have any attributes");
            }

            CategoryName = XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["name"], string.Empty);
            IsInherited = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["isinherited"], "false"));
            IsDummy = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["dummy"], "false"));
            IsExtra = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["extra"], "false"));

            Properties["caption"] = XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["caption"], CategoryName);
            Properties["collapsible"] = XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["collapsible"], "false");
            Properties["collapsedbydefault"] = XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["collapsedbydefault"], "false");
            Properties["visible"] = XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["visible"], "true");

            if (!XmlHelper.XmlAttributeIsEmpty(categoryNode.Attributes["visiblemacro"]))
            {
                PropertiesMacroTable["visible"] = XmlHelper.GetXmlAttributeValue(categoryNode.Attributes["visiblemacro"], string.Empty);
            }

            // Process properties elements
            if (categoryNode.SelectSingleNode("properties") != null)
            {
                XmlNodeList propertiesNodesList = categoryNode.SelectSingleNode("properties").ChildNodes;

                // Fill hash table with properties pairs (property name, property value)
                foreach (XmlNode propertyNode in propertiesNodesList)
                {
                    Properties[propertyNode.Name] = HttpUtility.HtmlDecode(propertyNode.InnerXml);

                    // Add property value to macro table if property contains macro
                    if ((propertyNode.Attributes["ismacro"] != null) && ValidationHelper.GetBoolean(propertyNode.Attributes["ismacro"].Value, false))
                    {
                        PropertiesMacroTable[propertyNode.Name] = HttpUtility.HtmlDecode(propertyNode.InnerXml);
                    }
                    else
                    {
                        PropertiesMacroTable[propertyNode.Name] = null;
                    }
                }
            }
        }


        /// <summary>
        /// Loads the field info from plain database structure data
        /// </summary>
        /// <param name="row">Data row with structure information</param>
        /// <param name="isPrimary">Indicates if field represents primary key</param>
        /// <param name="isSystem">Indicates if field is system field</param>
        public void LoadFromTableData(DataRow row, bool isPrimary, bool isSystem)
        {
            throw new NotSupportedException("This operation is not supported, category field doesn't have representation in database structure.");
        }


        /// <summary>
        /// Returns cloned object of current FormCategoryInfo object.
        /// </summary>
        /// <returns>Returns clone of FormCategoryInfo</returns>
        public IDataDefinitionItem Clone()
        {
            FormCategoryInfo clone = (FormCategoryInfo)MemberwiseClone();
            clone.Properties = (Hashtable)Properties.Clone();
            clone.PropertiesMacroTable = (Hashtable)PropertiesMacroTable.Clone();

            return clone;
        }


        private Dictionary<string, string> GetAttributes()
        {
            var attributes = new Dictionary<string, string>();

            attributes["name"] = CategoryName;
            attributes["isinherited"] = (IsInherited ? "true" : string.Empty);
            attributes["dummy"] = (IsDummy ? "true" : string.Empty);
            attributes["extra"] = (IsExtra ? "true" : string.Empty);

            return attributes;
        }


        /// <summary>
        /// Gets XmlNode for current FormCategoryInfo object.
        /// </summary>
        public XmlNode GetXmlNode(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            // Create new category node
            var newCategoryNode = doc.CreateElement("category");

            var attributes = GetAttributes();

            newCategoryNode.AddAttributes(attributes);

            AddPropertiesToNode(newCategoryNode);

            return newCategoryNode;
        }


        /// <summary>
        /// Adds properties values to given XmlNode.
        /// </summary>
        private void AddPropertiesToNode(XmlElement node)
        {
            // Prepare properties for XML node
            var properties = (Hashtable)Properties.Clone();

            if (CategoryName.Equals(ValidationHelper.GetString(properties["caption"], ""), StringComparison.InvariantCulture))
            {
                // Reset caption, it's same as the category name
                properties["caption"] = null;
            }
            if (!ValidationHelper.GetBoolean(properties["collapsible"], false))
            {
                // Reset "collapsible", False is default value
                properties["collapsible"] = null;
            }
            if (!ValidationHelper.GetBoolean(properties["collapsedbydefault"], false))
            {
                // Reset "collapsedbydefault", False is default value
                properties["collapsedbydefault"] = null;
            }

            var newNode = node.OwnerDocument.CreateElement("properties");

            newNode.AddChildElements(properties, transform: FormHelper.GetPropertyMacroTransformation(PropertiesMacroTable));

            if (newNode.HasChildNodes)
            {
                node.AppendChild(newNode);
            }
        }


        /// <summary>
        /// Gets unresolved property value.
        /// </summary>
        /// <param name="property">Property</param>
        public string GetPropertyValue(FormCategoryPropertyEnum property)
        {
            bool isMacro;
            return GetPropertyValue(property, out isMacro);
        }


        /// <summary>
        /// Gets unresolved property value.
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="isMacro">Returns true if property contains macro</param>
        /// <returns></returns>
        public string GetPropertyValue(FormCategoryPropertyEnum property, out bool isMacro)
        {
            isMacro = (PropertiesMacroTable != null) && (PropertiesMacroTable[property.ToStringRepresentation()] != null);
            if (isMacro)
            {
                return ValidationHelper.GetString(PropertiesMacroTable[property.ToStringRepresentation()], string.Empty);
            }

            return ValidationHelper.GetString(Properties[property.ToStringRepresentation()], string.Empty);
        }


        /// <summary>
        /// Gets resolved property value.
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="resolver">Macro resolver</param>
        /// <param name="macroSettings">Macro context</param>
        public string GetPropertyValue(FormCategoryPropertyEnum property, MacroResolver resolver, MacroSettings macroSettings = null)
        {
            bool isMacro = false;
            string value = GetPropertyValue(property, out isMacro);

            if (isMacro && (resolver != null))
            {
                value = resolver.ResolveMacros(value, macroSettings);
            }
            return value;
        }


        /// <summary>
        /// Sets property value.
        /// </summary>
        /// <param name="property">Property which value is set</param>
        /// <param name="value">Property value</param>
        /// <param name="isMacro">Indicates if value is macro. Default value is false.</param>
        public void SetPropertyValue(FormCategoryPropertyEnum property, string value, bool isMacro = false)
        {
            Properties[property.ToStringRepresentation()] = value;
            if (isMacro)
            {
                PropertiesMacroTable[property.ToStringRepresentation()] = value;
            }
        }


        /// <summary>
        /// Registers the Columns of this object for resolving data macros.
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();
            RegisterColumn("Name", x => x.CategoryName);
            RegisterColumn("Caption", x => x.Properties["caption"]);
            RegisterColumn("IsDummy", x => x.IsDummy);
            RegisterColumn("IsExtra", x => x.IsExtra);
            RegisterColumn("IsInherited", x => x.IsInherited);
        }
    }
}