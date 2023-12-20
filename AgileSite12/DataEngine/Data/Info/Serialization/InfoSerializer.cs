using System;
using System.Linq;
using System.Web;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Serialization support for info objects
    /// </summary>
    internal class InfoSerializer
    {
        #region "Variables"

        // Indicated a string is too long to be written in element and should be wrapped in CDATA so it does not interrupt reader by moving end tag too far off the screen
        private const int TEXT_TOO_LONG = 50;

        // Translations
        private TranslationHelper mTranslationHelper;

        #endregion


        #region "Properties"

        /// <summary>
        /// Provides objects translation interface.
        /// </summary>
        internal TranslationHelper TranslationHelper
        {
            get
            {
                return mTranslationHelper ?? (mTranslationHelper = new TranslationHelper());
            }
            set
            {
                mTranslationHelper = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Serializes the given object to XML element.
        /// </summary>
        /// <param name="infoObject">Info object to serialize</param>
        public XmlElement SerializeObject(BaseInfo infoObject)
        {
            return SerializeObject(infoObject, null);
        }


        /// <summary>
        /// Serializes the given object to XML element.
        /// </summary>
        /// <param name="infoObject">Info object to serialize</param>
        /// <param name="settings">Specific serialization settings that can override those from type info.</param>
        internal XmlElement SerializeObject(BaseInfo infoObject, SerializationSettings settings)
        {
            var document = new XmlDocument
            {
                PreserveWhitespace = true
            };

            // Add object
            var element = GetObjectElement(infoObject, document, settings ?? infoObject.TypeInfo.SerializationSettings);
            document.AppendChild(element);

            return document.DocumentElement;
        }


        /// <summary>
        /// Gets the object XML element.
        /// </summary>
        /// <param name="infoObject">Info object</param>
        /// <param name="document">XML document for serialization</param>
        /// <param name="settings">Specific serialization settings that override those from type info.</param>
        private XmlElement GetObjectElement(IInfo infoObject, XmlDocument document, SerializationSettings settings)
        {
            var typeInfo = infoObject.TypeInfo;

            var element = document.CreateElement(typeInfo.ObjectType);

            // Use alphabetic column order for serialization to maintain stable column order and exclude columns specified in the type info
            var columnNames = infoObject.ColumnNames
                                        .Where(name => !settings.IsExcludedField(name))
                                        .OrderBy(name => name, StringComparer.InvariantCulture);

            // Serialize columns
            foreach (var columnName in columnNames)
            {
                // Skip columns with NULL values
                var value = infoObject.GetValue(columnName);
                if (value == null)
                {
                    continue;
                }

                var columnElement = document.CreateElement(columnName);

                var targetObjectType = GetReferencedObjectType(infoObject, columnName);
                if (!String.IsNullOrEmpty(targetObjectType))
                {
                    WriteForeignKey(typeInfo, document, columnElement, columnName, targetObjectType, value, settings);
                }
                else
                {
                    WriteValue(typeInfo, document, columnElement, columnName, value, settings);
                }

                element.AppendChild(columnElement);
            }

            return element;
        }


        /// <summary>
        /// Returns object type name if column holds reference to another object.
        /// </summary>
        /// <param name="infoObject">Serialized info object</param>
        /// <param name="columnName">Name of the column to check</param>
        private string GetReferencedObjectType(IInfo infoObject, string columnName)
        {
            var typeInfo = infoObject.TypeInfo;
            var objectType = typeInfo.GetObjectTypeForColumn(columnName);

            if (String.IsNullOrEmpty(objectType))
            {
                var dependency = typeInfo.GetDependencyForColumn(columnName);
                if (dependency != null)
                {
                    objectType = Convert.ToString(infoObject.GetValue(dependency.ObjectTypeColumn));
                }
            }

            return objectType;
        }


        /// <summary>
        /// Writes foreign key translation into the field element.
        /// </summary>
        /// <param name="typeInfo">Type info object</param>
        /// <param name="document">XML document for serialization</param>
        /// <param name="fieldElement">Parent element the translations will be written into</param>
        /// <param name="fieldName">Foreign key field name</param>
        /// <param name="targetObjectType">Target object type name</param>
        /// <param name="value">Raw target object ID</param>
        /// <param name="settings">Specific serialization settings that override those from type info.</param>
        private void WriteForeignKey(ObjectTypeInfo typeInfo, XmlDocument document, XmlElement fieldElement, string fieldName, string targetObjectType, object value, SerializationSettings settings)
        {
            var targetId = ValidationHelper.GetInteger(value, 0);
            if (targetId <= 0)
            {
                // Key cannot be translated - write raw value
                WriteValue(typeInfo, document, fieldElement, fieldName, targetId, settings);

                return;
            }

            // Populate ID with its translation if the column is foreign key to some object type
            WriteTranslationNodes(targetObjectType, targetId, fieldElement);
        }


        /// <summary>
        /// Reads raw value of DateTime and returns well-formatted invariant string.
        /// </summary>
        /// <param name="value">Object of DateType type</param>
        private static string ReadRawDateTimeValueToString(object value)
        {
            // Convert date time value to universal sortable format (culture invariant)
            return ((DateTime)value).ToUniversalTime().ToString("u");
        }


        /// <summary>
        /// Reads raw value of double and returns string that can round-trip to an identical number.
        /// </summary>
        /// <param name="value">Object of double type</param>
        private static string ReadRawDoubleValueToString(object value)
        {
            return ((double)value).ToString("r", CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Reads raw value of decimal and returns string with trailing zeroes trimmed.
        /// </summary>
        /// <param name="value">Object of decimal type.</param>
        private static string ReadRawDecimalValueToString(object value)
        {
            return ((decimal)value).TrimEnd().ToString(CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Reads raw value of any type and returns well-formatted invariant string.
        /// </summary>
        /// <param name="value">Object for interpretation as string</param>
        private static string ReadRawValueToString(object value)
        {
            // Get string value for the column in system (en-us) culture
            return ValidationHelper.GetString(value, "", CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Writes structured field into the field element.
        /// </summary>
        /// <param name="document">XML document for serialization</param>
        /// <param name="fieldElement">Parent element the structured field will be written into</param>
        /// <param name="value">Value to be structured</param>
        /// <param name="structuredField">Definition of the structured field configuration</param>
        private static void WriteStructuredField(XmlDocument document, XmlNode fieldElement, string value, IStructuredField structuredField)
        {
            // Structured columns, serialize the value through a strongly typed structured object
            var structuredValue = structuredField.CreateStructuredValue(value);

            if (structuredValue != null)
            {
                var structuredElement = structuredValue.GetXmlElement(document);
                EnsureCData(structuredElement);
                fieldElement.AppendChild(structuredElement);
            }
        }


        /// <summary>
        /// Writes string value into the field element, encapsulating it with CDATA
        /// </summary>
        /// <param name="typeInfo">Type info object</param>
        /// <param name="fieldElement">Parent element the structured field will be written into</param>
        /// <param name="fieldName">Type info field name</param>
        /// <param name="value">Value to be structured</param>
        private static void WriteStringValue(ObjectTypeInfo typeInfo, XmlElement fieldElement, string fieldName, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }

            // Encapsulate strings into CDATA
            var type = typeInfo.ClassStructureInfo.GetColumnType(fieldName);
            if ((type == typeof(string)) && NeedsCData(value))
            {
                fieldElement.AppendCData(value);
            }
            else
            {
                // Regular columns, store the value as element text
                fieldElement.InnerText = value;
            }
        }


        /// <summary>
        /// Writes a value into given field element
        /// </summary>
        /// <param name="typeInfo">Type info object</param>
        /// <param name="document">XML document for serialization</param>
        /// <param name="fieldElement">Parent element the structured field value will be written into</param>
        /// <param name="fieldName">Type info field name</param>
        /// <param name="value">Value to be structured</param>
        /// <param name="settings">Specific serialization settings that override those from type info.</param>
        private void WriteValue(ObjectTypeInfo typeInfo, XmlDocument document, XmlElement fieldElement, string fieldName, object value, SerializationSettings settings)
        {
            string stringValue;
            
            if (value is DateTime)
            {
                stringValue = ReadRawDateTimeValueToString(value);
            }
            else if ((value is double) || (value is float))
            {
                stringValue = ReadRawDoubleValueToString(value);
            }
            else if (value is decimal)
            {
                stringValue = ReadRawDecimalValueToString(value);
            }
            else
            {
                stringValue = ReadRawValueToString(value);
            }

            // Process structured columns as structured data
            var structuredField = settings.GetStructuredField(fieldName);
            if (structuredField != null)
            {
                WriteStructuredField(document, fieldElement, stringValue, structuredField);
            }
            else
            {
                WriteStringValue(typeInfo, fieldElement, fieldName, stringValue);
            }
        }


        /// <summary>
        /// Adds the translation nodes to the serialization
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="fieldElement">Parent element for translation data</param>
        private void WriteTranslationNodes(string objectType, int objectId, XmlNode fieldElement)
        {
            var translationReference = TranslationHelper.TranslationReferenceLoader.LoadFromDatabase(objectType, objectId);
            translationReference?.WriteToXmlElement(fieldElement);
        }


        /// <summary>
        /// Ensures CDATA sections for element values with complex content.
        /// </summary>
        /// <param name="element">Root of element sub-tree to process</param>
        private static void EnsureCData(XmlElement element)
        {
            var hasChildElements = false;

            if (element.ChildNodes.Count > 0)
            {
                foreach (var childElement in element.ChildNodes.OfType<XmlElement>())
                {
                    EnsureCData(childElement);
                    hasChildElements = true;
                }
            }

            if (!hasChildElements)
            {
                var content = element.InnerText;
                if (NeedsCData(content))
                {
                    element.AppendCData(content);
                }
            }
        }


        /// <summary>
        /// Returns true if the given string needs to be encapsulated into CDATA section
        /// </summary>
        /// <param name="text">Text to check</param>
        private static bool NeedsCData(string text)
        {
            // Long text, text with new lines or text that needs encoding needs to be wrapped in CDATA
            return (text.Length > TEXT_TOO_LONG) || (text.IndexOf('\n') >= 0) || (HttpUtility.HtmlEncode(text) != text);
        }

        #endregion
    }
}
