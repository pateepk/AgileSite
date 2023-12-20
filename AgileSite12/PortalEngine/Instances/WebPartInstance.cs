using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

using CMS.Base;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Web part instance representation.
    /// </summary>
    [DebuggerDisplay("WebPartInstance({ControlID})")]
    public class WebPartInstance : ISimpleDataContainer
    {
        #region "Variables"
        
        private string mControlID;
        private string mWebPartType;
        private Guid mInstanceGUID = Guid.Empty;

        private readonly Hashtable mProperties = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        private Hashtable mMacroTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        private static Regex mPropertyRegEx;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the web part instance XML version
        /// </summary>
        public int XMLVersion { get; set; }


        /// <summary>
        /// Gets or sets the current variant webpart instance
        /// </summary>
        public WebPartInstance CurrentVariantInstance { get; set; }


        /// <summary>
        /// Hashtable with IsMacro flags.
        /// </summary>
        public Hashtable MacroTable
        {
            get
            {
                return mMacroTable;
            }
            set
            {
                mMacroTable = value;
            }
        }


        /// <summary>
        /// Regular expression to search the property macro.
        /// </summary>
        private static Regex PropertyRegEx => mPropertyRegEx ?? (mPropertyRegEx = RegexHelper.GetRegex("\\{%(\\w+)%}"));


        /// <summary>
        /// WebPart control ID.
        /// </summary>
        public string ControlID
        {
            get
            {
                return mControlID;
            }
            set
            {
                mControlID = value;
                mProperties["controlid"] = mControlID;
            }
        }


        /// <summary>
        /// WebPart type (codename).
        /// </summary>
        public string WebPartType
        {
            get
            {
                return mWebPartType;
            }
            set
            {
                mWebPartType = value;
                mProperties["webparttype"] = mWebPartType;
            }
        }


        /// <summary>
        /// Parent web part zone.
        /// </summary>
        public WebPartZoneInstance ParentZone { get; set; }


        /// <summary>
        /// Web part properties table.
        /// </summary>
        public Hashtable Properties => mProperties;


        /// <summary>
        /// Instance GUID to identify the web part.
        /// </summary>
        public Guid InstanceGUID
        {
            get
            {
                return mInstanceGUID;
            }
            set
            {
                mInstanceGUID = value;
                mProperties["instanceguid"] = mInstanceGUID;
            }
        }


        /// <summary>
        /// Remove flag. If true, web part has been removed. Only for internal purposes.
        /// </summary>
        public bool Removed { get; set; }


        /// <summary>
        /// Indicates if webpart instance is actually widget.
        /// </summary>
        public bool IsWidget { get; set; }


        /// <summary>
        /// Indicates if webpart instance is a variant.
        /// </summary>
        public bool IsVariant => (VariantID > 0);


        /// <summary>
        /// Gets or sets the variant id.
        /// </summary>
        public int VariantID { get; set; }


        /// <summary>
        /// Indicates whether the web part has any variants.
        /// </summary>
        public bool HasVariants => PartInstanceVariants != null && PartInstanceVariants.Count > 0;


        /// <summary>
        /// Gets or sets the variant mode which is used for the variants of this web part instance.
        /// </summary>
        public VariantModeEnum VariantMode
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the widget is minimized.
        /// </summary>
        public bool Minimized
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("Minimized"), false);
            }
            set
            {
                SetValue("Minimized", value);
            }
        }


        /// <summary>
        /// Gets the part instance variant list.
        /// </summary>
        public virtual List<WebPartInstance> PartInstanceVariants { get; set; }
        

        #endregion


        #region "Public methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebPartInstance()
        {
            XMLVersion = 1;
            VariantMode = VariantModeEnum.None;
        }


        /// <summary>
        /// Constructor, initializes the instance with given XML data.
        /// </summary>
        public WebPartInstance(XmlNode partNode)
            : this()
        {
            LoadFromXmlNode(partNode);
        }


        /// <summary>
        /// Finds the variant with the given variant ID
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        public WebPartInstance FindVariant(int variantId)
        {
            return PartInstanceVariants.Find(v => v.VariantID.Equals(variantId));
        }


        /// <summary>
        /// Loads all the MVT/Content personalization variants for this web part instance.
        /// </summary>
        /// <param name="forceLoad">Indicates if already loaded variants should be reloaded</param>
        /// <param name="variantMode">Specifies which variants should be loaded (MVT/ContentPersonalization/None - means both MVT+CP variants should try to load)</param>
        /// <param name="documentId">Document ID if the instance holds a widget</param>
        public void LoadVariants(bool forceLoad, VariantModeEnum variantMode, int documentId = 0)
        {
            WebPartEvents.WebPartLoadVariant.StartEvent(new WebPartLoadVariantsArgs
            {
                ForceLoad = forceLoad,
                VariantMode = variantMode,
                WebPartInstance = this,
                DocumentID = documentId
            });
        }


        /// <summary>
        /// Resolves the property macros, replaces the {%propertyname%} macro with the property value.
        /// </summary>
        /// <param name="text">Text to resolve</param>
        public string ResolvePropertyMacros(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            return PropertyRegEx.Replace(text, ReplaceProperty);
        }


        /// <summary>
        /// Property match evaluator to replace the found property match with the property value.
        /// </summary>
        /// <param name="m">Match found</param>
        private string ReplaceProperty(Match m)
        {
            return ValidationHelper.GetString(GetValue(m.Groups[1].ToString()), m.ToString());
        }


        /// <summary>
        /// Clones the web part object (GUID stays the same when cloned).
        /// </summary>
        /// <param name="copyMacroTable">Indicates whether macro values should be cloned</param>
        public WebPartInstance Clone(bool copyMacroTable = false)
        {
            WebPartInstance wpi = new WebPartInstance();

            // Clone the data
            wpi.mControlID = mControlID;
            wpi.ParentZone = ParentZone;
            wpi.mWebPartType = mWebPartType;
            wpi.mInstanceGUID = mInstanceGUID;
            wpi.IsWidget = IsWidget;
            wpi.VariantID = VariantID;
            wpi.VariantMode = VariantMode;
            wpi.XMLVersion = XMLVersion;

            // Clone the variant list
            if ((PartInstanceVariants != null) && (VariantID == 0))
            {
                wpi.PartInstanceVariants = PartInstanceVariants.ConvertAll(variant => variant.Clone(copyMacroTable));
            }

            // Clone properties
            foreach (DictionaryEntry property in mProperties)
            {
                wpi.mProperties.Add(property.Key, property.Value);
            }

            // Clone macro values
            if (copyMacroTable)
            {
                foreach (DictionaryEntry macro in MacroTable)
                {
                    wpi.mMacroTable.Add(macro.Key, macro.Value);
                }
            }

            // Return cloned instance
            return wpi;
        }


        /// <summary>
        /// Loads the webpart properties from given webpart.
        /// </summary>
        /// <param name="webpart">The web part instance</param>
        public void LoadProperties(WebPartInstance webpart)
        {
            // Clone properties
            Properties.Clear();

            foreach (DictionaryEntry property in webpart.Properties)
            {
                Properties[property.Key] = property.Value;
            }
        }


        /// <summary>
        /// Loads default values from specified DataRow to the webpart properties.
        /// </summary>
        /// <param name="dr">Datarow with properties default values</param>
        public void LoadProperties(DataRow dr)
        {
            if (dr != null)
            {
                foreach (DataColumn column in dr.Table.Columns)
                {
                    SetValue(column.ColumnName, dr[column.ColumnName]);
                }
            }
        }


        /// <summary>
        /// Returns the value of the given web part property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public object GetValue(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            // Handle special properties
            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "webpartcontrolid":
                    return ControlID;

                default:
                    return mProperties[propertyName];
            }
        }


        /// <summary>
        /// Sets the property value of the control.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        public bool SetValue(string propertyName, object value)
        {
            if (value == DBNull.Value)
            {
                value = null;
            }

            if (String.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            // Handle special properties
            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "webpartcontrolid":
                    ControlID = DataHelper.GetNotEmpty(value, ControlID);
                    return true;
            }

            mProperties[propertyName] = value;
            return true;
        }


        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public object this[string propertyName]
        {
            get
            {
                return GetValue(propertyName);
            }
            set
            {
                SetValue(propertyName, value);
            }
        }


        /// <summary>
        /// Clears the given properties
        /// </summary>
        /// <param name="propertyNames">Property names to clear</param>
        public void ClearValues(params string[] propertyNames)
        {
            foreach (string name in propertyNames)
            {
                SetValue(name, null);
            }
        }

        #endregion


        #region "XML methods"

        /// <summary>
        /// Loads the web part instance from the given XML node
        /// </summary>
        /// <param name="partNode">Web part node</param>
        private void LoadFromXmlNode(XmlNode partNode)
        {
            // If no data given, exit
            if (partNode == null)
            {
                return;
            }

            XmlAttributeCollection attrs = partNode.Attributes;

            // Get the control ID
            if (attrs?["controlid"] == null || (attrs["controlid"].Value.Trim() == ""))
            {
                throw new Exception("[new WebPartInstance(XmlNode)]: Missing web part 'controlID' attribute.");
            }
            ControlID = attrs["controlid"].Value.Trim();
            mProperties["controlid"] = ControlID;

            // Get the type (codename)
            if ((attrs["type"] == null) || (attrs["type"].Value.Trim() == ""))
            {
                throw new Exception("[new WebPartInstance(XmlNode)]: Missing web part 'type' attribute.");
            }
            WebPartType = attrs["type"].Value.Trim();
            mProperties["webparttype"] = WebPartType;

            // Get GUID
            if (attrs["guid"] != null)
            {
                InstanceGUID = ValidationHelper.GetGuid(attrs["guid"].Value, Guid.Empty);
                mProperties["instanceguid"] = InstanceGUID;
            }

            // Widget status
            if (attrs["iswidget"] != null)
            {
                IsWidget = ValidationHelper.GetBoolean(attrs["iswidget"].Value, false);
                mProperties["iswidget"] = IsWidget;
            }

            XMLVersion = 0;
            if (attrs["v"] != null)
            {
                XMLVersion = ValidationHelper.GetInteger(attrs["v"].Value, 0);
            }

            // Get the properties
            XmlNodeList properties = partNode.SelectNodes("property");
            if (properties != null)
            {
                // Load the properties
                foreach (XmlNode propNode in properties)
                {
                    var attributes = propNode.Attributes;

                    // Get the property name
                    var nameAttribute = attributes?["name"];
                    if (nameAttribute == null || (nameAttribute.Value.Trim() == ""))
                    {
                        continue;
                    }

                    string name = nameAttribute.Value.Trim();

                    // Get the property value and ismacro flag
                    string value = propNode.InnerText;
                    var isMacroAttribute = attributes["ismacro"];
                    bool isMacro = (isMacroAttribute != null) && ValidationHelper.GetBoolean(isMacroAttribute.Value, false);
                    MacroTable[name] = isMacro ? value : null;
                    SetValue(name, value);
                }
            }
        }


        private Dictionary<string, string> GetAttributes()
        {
            var attributes = new Dictionary<string, string>();

            attributes["controlid"] = ControlID;
            attributes["type"] = WebPartType;

            // GUID (Create one if not present)
            if (InstanceGUID == Guid.Empty)
            {
                InstanceGUID = Guid.NewGuid();
            }
            attributes["guid"] = InstanceGUID.ToString();

            if (XMLVersion > 0)
            {
                // XML Version
                attributes["v"] = XMLVersion.ToString();
            }

            // Is widget status
            if (IsWidget)
            {
                attributes["iswidget"] = "true";
            }

            return attributes;
        }


        /// <summary>
        /// Returns the XML node representing the webpart instance.
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        /// <param name="nodeName">XML node name</param>
        public XmlElement GetXmlNode(XmlDocument doc = null, string nodeName = "webpart")
        {
            // Ensure XML document
            if (doc == null)
            {
                doc = new XmlDocument();
            }

            // Create the web part node
            var partNode = doc.CreateElement(nodeName);

            var attributes = GetAttributes();
            partNode.AddAttributes(attributes);

            // Properties
            partNode.AddChildElements(mProperties, "property", FormHelper.GetPropertyMacroTransformation(mMacroTable), false);
            
            return partNode;
        }

        #endregion
    }
}