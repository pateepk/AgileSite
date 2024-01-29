using System;
using System.Xml;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents the code generation configuration for the data class.
    /// </summary>
    [Serializable]
    public class ClassCodeGenerationSettings : IStructuredData
    {
        #region "Variables"

        private readonly XmlData xmlData;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the namespace of generated files.
        /// </summary>
        public string NameSpace
        {
            get
            {
                return ValidationHelper.GetString(xmlData["NameSpace"], null);
            }
            set
            {
                xmlData["NameSpace"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return ValidationHelper.GetString(xmlData["ObjectType"], null);
            }
            set
            {
                xmlData["ObjectType"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the display name column.
        /// </summary>
        public string DisplayNameColumn
        {
            get
            {
                return ValidationHelper.GetString(xmlData["DisplayNameColumn"], null);
            }
            set
            {
                xmlData["DisplayNameColumn"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the code name column.
        /// </summary>
        public string CodeNameColumn
        {
            get
            {
                return ValidationHelper.GetString(xmlData["CodeNameColumn"], null);
            }
            set
            {
                xmlData["CodeNameColumn"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the GUID column.
        /// </summary>
        public string GuidColumn
        {
            get
            {
                return ValidationHelper.GetString(xmlData["GUIDColumn"], null);
            }
            set
            {
                xmlData["GUIDColumn"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the "last modified" column.
        /// </summary>
        public string LastModifiedColumn
        {
            get
            {
                return ValidationHelper.GetString(xmlData["LastModifiedColumn"], null);
            }
            set
            {
                xmlData["LastModifiedColumn"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the binary column.
        /// </summary>
        public string BinaryColumn
        {
            get
            {
                return ValidationHelper.GetString(xmlData["BinaryColumn"], null);
            }
            set
            {
                xmlData["BinaryColumn"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the site ID column.
        /// </summary>
        public string SiteIdColumn
        {
            get
            {
                return ValidationHelper.GetString(xmlData["SiteIDColumn"], null);
            }
            set
            {
                xmlData["SiteIDColumn"] = value;
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates if the code should use the ID hashtable.
        /// </summary>
        public bool UseIdHashtable
        {
            get
            {
                return ValidationHelper.GetBoolean(xmlData["UseIdHashtable"], false);
            }
            set
            {
                xmlData["UseIdHashtable"] = value;
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates if the code should use the name hashtable.
        /// </summary>
        public bool UseNameHashtable
        {
            get
            {
                return ValidationHelper.GetBoolean(xmlData["UseNameHashtable"], false);
            }
            set
            {
                xmlData["UseNameHashtable"] = value;
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates if the code should use the GUID hashtable.
        /// </summary>
        public bool UseGuidHashtable
        {
            get
            {
                return ValidationHelper.GetBoolean(xmlData["UseGuidHashtable"], false);
            }
            set
            {
                xmlData["UseGuidHashtable"] = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a settings object.
        /// </summary>
        public ClassCodeGenerationSettings()
        {
            xmlData = new XmlData();
        }


        /// <summary>
        /// Creates a settings object and initializes it using the given XML representation.
        /// </summary>
        /// <param name="xml"></param>
        public ClassCodeGenerationSettings(string xml)
            : this()
        {
            xmlData.LoadData(xml);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns an XML representation of the settings as string.
        /// </summary>
        public override string ToString()
        {
            return xmlData.ToString();
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            xmlData.LoadFromXmlElement(element);
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            return xmlData.GetXmlElement(doc);
        }

        #endregion
    }
}