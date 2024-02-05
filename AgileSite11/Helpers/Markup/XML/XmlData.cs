using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Xml;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// XML data container.
    /// </summary>
    [Serializable]
    public class XmlData : ISerializable, IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Collection of the data contents.
        /// </summary>
        protected StringSafeDictionary<object> mData = new StringSafeDictionary<object>(false);


        /// <summary>
        /// Collection of the macros.
        /// </summary>
        private Hashtable mMacros;


        /// <summary>
        /// Default xml root element name.
        /// </summary>
        private readonly string mXmlRootName = "Data";

        #endregion


        #region "Properties"

        private string ValueInternal
        {
            get;
            set;
        }
        

        /// <summary>
        /// Gets name of xml root element.
        /// </summary>
        public string XmlRootName
        {
            get
            {
                return mXmlRootName;
            }
        }


        /// <summary>
        /// Column names.
        /// </summary>
        public virtual List<string> ColumnNames
        {
            get
            {
                return TypeHelper.NewList(mData.Keys);
            }
        }


        /// <summary>
        /// List of excluded column name prefixes (delimited by ';'). Columns starting with these won't be loaded to collection.
        /// </summary>
        public String ExcludedColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Macro table
        /// </summary>
        public virtual Hashtable MacroTable
        {
            get
            {
                // Ensure table
                if ((mMacros == null) && AllowMacros)
                {
                    mMacros = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
                }

                return mMacros;
            }
            set
            {
                mMacros = value;
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="key">Column name</param>
        public virtual object this[string key]
        {
            get
            {
                if (key == null)
                {
                    return null;
                }

                // Get the content
                return mData[key];
            }
            set
            {
                // If null, remove the item
                if (value == null)
                {
                    if (mData.Contains(key))
                    {
                        mData.Remove(key);
                    }
                }
                else
                {
                    mData[key] = value;
                }
            }
        }


        /// <summary>
        /// Indicates if macro values are allowed
        /// </summary>
        public virtual bool AllowMacros
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts XML data to Hashtable.
        /// </summary>        
        public virtual Hashtable ConvertToHashtable()
        {
            return mData;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetValue(string columnName, out object value)
        {
            if ((mData == null) || !mData.ContainsKey(columnName))
            {
                value = null;
                return false;
            }

            // Get the value
            value = mData[columnName];
            if (value == DBNull.Value)
            {
                value = null;
            }

            return true;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetValue(string columnName)
        {
            if (!String.IsNullOrEmpty(ValueInternal))
            {
                return ValueInternal;
            }

            if (mData == null)
            {
                return null;
            }

            // Get the value
            object value = mData[columnName];
            if (value == DBNull.Value)
            {
                value = null;
            }

            return value;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public virtual bool SetValue(string columnName, object value)
        {
            if (mData != null)
            {
                ValueInternal = null;

                if (value == null)
                {
                    value = DBNull.Value;
                }

                mData.Add(columnName, value);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool ContainsColumn(string columnName)
        {
            return mData.Contains(columnName);
        }


        /// <summary>
        /// Loads the XML to the content table.
        /// </summary>
        /// <param name="data">Content XML to load</param>
        public virtual void LoadData(string data)
        {
            Clear();
            
            if (!String.IsNullOrEmpty(data))
            {
                if (data.Trim().StartsWithCSafe("<" + XmlRootName + ">", true))
                {
                    // Load data if it contains expected xml format
                    var xml = new XmlDocument();
                    xml.LoadXml(data);
                    LoadFromXmlElement(xml.DocumentElement);
                }
                else
                {
                    // Store it as a single value otherwise
                    ValueInternal = data;
                }
            }
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public virtual void LoadFromXmlElement(XmlElement element)
        {
            // Clear old data before loading new
            Clear();

            if (element != null)
            {
                // Load array with excluded columns
                string[] excludedPrefixes = null;
                if (!String.IsNullOrEmpty(ExcludedColumns))
                {
                    excludedPrefixes = ExcludedColumns.Split(';');
                }

                if (!element.ChildNodes.OfType<XmlElement>().Any())
                {
                    // Due to backward compatibility, read the single value from inner text.
                    ValueInternal = element.InnerText;
                }
                else
                {
                    // Load the data fields
                    foreach (XmlNode item in element.ChildNodes)
                    {
                        // Load all the elements
                        if (item.NodeType == XmlNodeType.Element)
                        {
                            string key = item.Name;
                            string value = item.InnerText;

                            // Check excluded columns
                            bool exclude = (excludedPrefixes != null) && excludedPrefixes.Any(m => key.StartsWithCSafe(m, true));
                            if (!exclude)
                            {
                                mData[key] = value;

                                // Macro flag
                                if (AllowMacros)
                                {
                                    XmlAttribute att = item.Attributes["ismacro"];
                                    if (att != null)
                                    {
                                        bool isMacro = ValidationHelper.GetBoolean(att.Value, false);
                                        if (isMacro)
                                        {
                                            MacroTable[key] = value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns the XML code of the data.
        /// </summary>
        /// <remarks>Data are represented as a string values</remarks>
        public virtual string GetData()
        {
            // If single value has been set, pass it out and ignore the data collection.
            if (!String.IsNullOrEmpty(ValueInternal))
            {
                return ValueInternal;
            }

            // Prepare the XML document
            var xml = new XmlDocument();

            var element = GetXmlElement(xml);

            if ((element != null) && element.HasChildNodes)
            {
                xml.AppendChild(element);
            }

            // Return the xml
            return xml.InnerXml;
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="document">Parent XML document</param>
        public virtual XmlElement GetXmlElement(XmlDocument document)
        {
            XmlElement element = document.CreateElement(XmlRootName);

            if (!String.IsNullOrEmpty(ValueInternal))
            {
                // Due to backward compatibility, store the single value in the inner text.
                element.InnerText = ValueInternal;
            }
            else
            {
                element.AddChildElements(mData, transform: (child, key, _) =>
                {
                    if (AllowMacros && (MacroTable[key] != null))
                    {
                        child.SetAttribute("ismacro", "True");
                    }
                });
            }

            return element;
        }


        /// <summary>
        /// Clears the custom data.
        /// </summary>
        public virtual void Clear()
        {
            ValueInternal = null;
            mData.Clear();
            if (MacroTable != null)
            {
                MacroTable.Clear();
            }
        }


        /// <summary>
        /// Returns the cloned object.
        /// </summary>
        public virtual XmlData Clone()
        {
            var newData = new XmlData(XmlRootName)
            {
                ExcludedColumns = ExcludedColumns,
                mData = (StringSafeDictionary<object>)mData.Clone(),
                ValueInternal = ValueInternal,
                AllowMacros = AllowMacros,
                MacroTable = (MacroTable == null) ? null : (Hashtable)MacroTable.Clone()
            };

            return newData;
        }


        /// <summary>
        /// Removes element with specified key from collection
        /// </summary>
        /// <param name="key">Key to remove</param>
        public virtual void Remove(String key)
        {
            if (mData != null)
            {
                mData.Remove(key);
            }
        }


        /// <summary>
        /// Returns xml as a string.
        /// </summary>
        public override string ToString()
        {
            return GetData();
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("XmlRootName", mXmlRootName);
            info.AddValue("Macros", mMacros);
            info.AddValue("Data", GetData());
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public XmlData(SerializationInfo info, StreamingContext context)
            : this()
        {
            mXmlRootName = (string)info.GetValue("XmlRootName", typeof(string));
            mMacros = (Hashtable)info.GetValue("Macros", typeof(Hashtable));

            LoadData(info.GetString("Data"));
        }


        /// <summary>
        /// Constructor - creates empty XmlData object.
        /// </summary>
        public XmlData()
        {
        }


        /// <summary>
        /// Constructor with root element name specification.
        /// </summary>
        /// <param name="rootName">Root element name</param>
        public XmlData(string rootName)
            : this()
        {
            if (!String.IsNullOrEmpty(rootName))
            {
                mXmlRootName = rootName;
            }
        }

        #endregion  
    }
}