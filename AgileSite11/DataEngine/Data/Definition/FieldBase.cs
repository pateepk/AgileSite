using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Form field base class
    /// </summary>
    public abstract class FieldBase<FieldType> : AbstractDataContainer<FieldType>, IField
        where FieldType : FieldBase<FieldType>
    {
        private const string DEFAULT_VALUE_COLUMN_NAME = "DefaultValue";


        #region "Variables"

        private string mDummyField;

        #endregion


        #region "Properties"

        /// <summary>
        /// Column caption.
        /// </summary>
        public string Caption
        {
            get
            {
                return ValidationHelper.GetString(Properties["fieldcaption"], null);
            }
            set
            {
                Properties["fieldcaption"] = value;
            }
        }


        /// <summary>
        /// Column name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field allow empty values.
        /// </summary>
        public bool AllowEmpty
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field is a primary key.
        /// </summary>
        public bool PrimaryKey
        {
            get;
            set;
        }


        /// <summary>
        /// Data type.
        /// </summary>
        public string DataType
        {
            get;
            set;
        } = FieldDataType.Unknown;


        /// <summary>
        /// Size of the field.
        /// </summary>
        public int Size
        {
            get;
            set;
        }


        /// <summary>
        /// Precision of the field
        /// </summary>
        public int Precision
        {
            get;
            set;
        } = -1;


        /// <summary>
        /// Field default value.
        /// </summary>
        public string DefaultValue
        {
            get
            {
                return ValidationHelper.GetString(Properties["defaultvalue"], null);
            }
            set
            {
                Properties["defaultvalue"] = value;
            }
        }


        /// <summary>
        /// Field unique identifier.
        /// </summary>
        public Guid Guid
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field is a system field.
        /// </summary>
        public bool System
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether field is unique.
        /// </summary>
        public bool IsUnique
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field is external, if so it represents column from another table which is included in CMS_Tree_View_Joined (CMS_Document, CMS_Node, ...).
        /// </summary>
        public bool External
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the field is inherited from parent class.
        /// </summary>
        public bool IsInherited
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that field has no representation in database.
        /// </summary>
        public bool IsDummyField
        {
            get;
            set;
        }


        /// <summary>
        /// If true the field was added into the main form else it was added into the alt.form (expects <see cref="IsDummyField"/> property to be true).
        /// </summary>
        public bool IsDummyFieldFromMainForm
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that field is extra field (field is not in original form definition).
        /// </summary>
        public bool IsExtraField
        {
            get;
            set;
        }


        /// <summary>
        /// Properties of the field
        /// </summary>
        public Hashtable Properties
        {
            get;
            set;
        } = new Hashtable(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// Macro table for the field properties.
        /// </summary>
        public Hashtable PropertiesMacroTable
        {
            get;
            set;
        } = new Hashtable(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// ObjectType to which the given field refers (for example as a foreign key).
        /// </summary>
        public string ReferenceToObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the reference (used only when ReferenceToObjectType is set).
        /// </summary>
        public ObjectDependencyEnum ReferenceType
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        protected FieldBase()
        {
            // Create a unique identifier of the new field
            Guid = Guid.NewGuid();
        }


        /// <summary>
        /// Loads the field info from XML node
        /// </summary>
        /// <param name="fieldNode">Field node</param>
        public virtual void LoadFromXmlNode(XmlNode fieldNode)
        {
            if (fieldNode.Attributes == null)
            {
                return;
            }

            Name = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["column"], null);
            DataType = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["columntype"], FieldDataType.Unknown).ToLowerInvariant();
            AllowEmpty = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["allowempty"], "false"));
            Size = Convert.ToInt32(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["columnsize"], "0"));
            Precision = Convert.ToInt32(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["columnprecision"], "-1"));
            PrimaryKey = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["isPK"], "false"));
            System = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["system"], "false"));
            Guid = ValidationHelper.GetGuid(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["guid"], null), Guid.NewGuid());
            External = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["external"], "false"));
            IsInherited = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["isinherited"], "false"));
            mDummyField = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["dummy"], null);
            IsExtraField = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["extra"], "false"));
            IsUnique = Convert.ToBoolean(XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["isunique"], "false"));

            ReferenceToObjectType = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["refobjtype"], null);
            ReferenceType = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["reftype"], null).ToEnum<ObjectDependencyEnum>();

            Properties["defaultvalue"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["defaultvalue"], null);
            Properties["fieldcaption"] = XmlHelper.GetXmlAttributeValue(fieldNode.Attributes["fieldcaption"], null);

            IsDummyField = !string.IsNullOrEmpty(mDummyField);
            IsDummyFieldFromMainForm = !"altform".Equals(mDummyField, StringComparison.InvariantCultureIgnoreCase);

            // Process properties elements
            var propertiesNode = fieldNode.SelectSingleNode("properties");
            if (propertiesNode != null)
            {
                XmlNodeList propertiesNodesList = propertiesNode.ChildNodes;

                // Fill hash tables with properties pairs (property name, property value)
                foreach (XmlNode propertyNode in propertiesNodesList)
                {
                    Properties[propertyNode.Name] = HttpUtility.HtmlDecode(propertyNode.InnerXml);

                    // Add property value to macro table if property contains macro
                    var attributes = propertyNode.Attributes;
                    var macroAttribute = attributes?["ismacro"];
                    if ((macroAttribute != null) && ValidationHelper.GetBoolean(macroAttribute.Value, false))
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
        /// Loads the field info from plain database structure data.
        /// </summary>
        /// <param name="row">Data row with structure information</param>
        /// <param name="isPrimary">Indicates if field represents primary key</param>
        /// <param name="isSystem">Indicates if field is system field</param>
        /// <remarks>Database structure data can be obtained via <see cref="CMS.DataEngine.TableManager.GetColumnInformation(string, string)"/>.</remarks>
        public virtual void LoadFromTableData(DataRow row, bool isPrimary, bool isSystem)
        {
            PrimaryKey = isPrimary;
            System = isSystem;
            Name = DataHelper.GetDataRowValue(row, "ColumnName").ToString(null);
            AllowEmpty = string.Equals(DataHelper.GetDataRowValue(row, "Nullable").ToString(null), "yes", StringComparison.InvariantCultureIgnoreCase);
            Size = DataHelper.GetDataRowValue(row, "DataSize").ToInteger(-1);
            Precision = DataHelper.GetDataRowValue(row, "DataPrecision").ToInteger(-1);

            var dataType = DataHelper.GetDataRowValue(row, "DataType").ToString(null);
            DataType = DataTypeManager.GetFieldType(dataType, Size, true);
            DefaultValue = GetDefaultValue(row, dataType);
        }


        private static string GetDefaultValue(DataRow row, string type)
        {
            // Non-string values has to be handled same way loaded from both xml and table => null value represents empty default.
            var defaultValue = DataHelper.GetDataRowValue(row, DEFAULT_VALUE_COLUMN_NAME) ?? DBNull.Value;
            return DataTypeManager.GetDefaultStringValue(type, defaultValue);
        }


        /// <summary>
        /// Returns field name value.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// Registers the Columns of this object for resolving data macros.
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("AllowEmpty", x => x.AllowEmpty);
            RegisterColumn("Name", x => x.Name);
            RegisterColumn("PrimaryKey", x => x.PrimaryKey);
            RegisterColumn("IsDummy", x => x.IsDummyField);
            RegisterColumn("IsDummyFromMainForm", x => x.IsDummyFieldFromMainForm);
            RegisterColumn("DataType", x => x.DataType);
            RegisterColumn("IsInherited", x => x.IsInherited);
            RegisterColumn("System", x => x.System);
            RegisterColumn("Size", x => x.Size);
            RegisterColumn("Guid", x => x.Guid);
            RegisterColumn("External", x => x.External);
            RegisterColumn("IsExtra", x => x.IsExtraField);
            RegisterColumn("IsUnique", x => x.IsUnique);
            RegisterColumn("ReferenceToObjectType", x => x.ReferenceToObjectType);
            RegisterColumn("ReferenceType", x => x.ReferenceType);

            RegisterColumn(DEFAULT_VALUE_COLUMN_NAME, x => x.DefaultValue);
            RegisterColumn("Precision", x => x.Precision);
            RegisterColumn("Caption", x => x.Caption);
        }


        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>Returns clone of FormFiedlInfo</returns>
        public virtual IDataDefinitionItem Clone()
        {
            var clone = (FieldBase<FieldType>)MemberwiseClone();

            clone.Properties = (Hashtable)Properties.Clone();
            clone.PropertiesMacroTable = (Hashtable)PropertiesMacroTable.Clone();

            return clone;
        }


        /// <summary>
        /// Returns the XML node representing the FormFieldInfo object.
        /// </summary>
        /// <param name="doc">XML document</param>
        public virtual XmlNode GetXmlNode(XmlDocument doc)
        {
            throw new NotSupportedException("[FieldBase.GetXMLNode]: Export of the data for basic field definition is not supported.");
        }

        #endregion
    }
}
