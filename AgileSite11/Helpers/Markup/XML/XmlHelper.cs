using System;
using System.Data;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Xsl;

using CMS.Base;
using CMS.Core;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Class providing xml validation methods.
    /// </summary>
    public static class XmlHelper
    {
        #region "Variables"

        private static readonly CMSRegex IllegalCharactersRegEx = new CMSRegex(@"[\u0000-\u0008\u000B\u000C\u000E-\u001F]");

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns value of the specified attribute from the given node representing a form field.
        /// </summary>
        /// <param name="node">Node representing a form field</param>
        /// <param name="attributeName">Attribute name</param>
        public static string GetAttributeValue(XmlNode node, string attributeName)
        {
            return GetAttributeValue(node, attributeName, String.Empty);
        }


        /// <summary>
        /// Returns value of the specified attribute from the given node representing a form field.
        /// </summary>
        /// <param name="node">Node representing a form field</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="defaultValue">Default value returned when attribute is not found</param>
        public static string GetAttributeValue(XmlNode node, string attributeName, string defaultValue)
        {
            if (node?.Attributes?[attributeName] == null)
            {
                return defaultValue;
            }

            return node.Attributes[attributeName].Value;
        }


        /// <summary>
        /// Determines whether specified xml attribute is defined.
        /// </summary>
        /// <param name="attribute">Xml attribute to check</param>
        public static bool XmlAttributeIsEmpty(XmlAttribute attribute)
        {
            return String.IsNullOrEmpty(attribute?.Value);
        }


        /// <summary>
        /// Returns string value of the specified xml attribute.
        /// </summary>
        /// <param name="attribute">Xml attribute to get value of</param>
        /// <param name="defValue">Default value if xml attribute is not defined</param>
        public static string GetXmlAttributeValue(XmlAttribute attribute, string defValue)
        {
            if (XmlAttributeIsEmpty(attribute))
            {
                return defValue;
            }

            return attribute.Value;
        }


        /// <summary>
        ///  Returns attributes as an array (0 - attribute name, 1 - value).
        /// </summary>
        /// <param name="attribute">Node attributes collection</param>
        public static string[,] GetXmlAttributes(XmlAttributeCollection attribute)
        {
            string[,] result = null;

            if (attribute.Count > 0)
            {
                result = new string[attribute.Count, 2];

                int i = 0;
                foreach (XmlAttribute xa in attribute)
                {
                    result[i, 0] = xa.LocalName;
                    result[i, 1] = xa.Value;
                    i++;
                }
            }
            return result;
        }


        /// <summary>
        /// Sets (updates or inserts) xml attributes to specified xml node.
        /// </summary>
        /// <param name="node">Xml node</param>
        /// <param name="attributes">Array of attributes (0 - name, 1 - value]) to set</param>
        /// <param name="insertEmpty">Indicates whether attributes with value "" (empty string) should be inserted to the XML</param>
        public static void SetXmlNodeAttributes(XmlNode node, string[,] attributes, bool insertEmpty = true)
        {
            if (node != null)
            {
                if (node.OwnerDocument != null)
                {
                    if (attributes != null)
                    {
                        var nodeAttributes = node.Attributes;
                        if (nodeAttributes != null)
                        {
                            for (int i = 0; i < attributes.GetUpperBound(0) + 1; i++)
                            {
                                // Empty attribute value
                                if ((!insertEmpty) && (String.IsNullOrEmpty(attributes[i, 1])))
                                {
                                    // Remove attribute if exists
                                    XmlAttribute attrib = nodeAttributes[attributes[i, 0]];
                                    if (attrib != null)
                                    {
                                        nodeAttributes.Remove(attrib);
                                    }
                                }
                                // Not empty attribute value
                                else
                                {
                                    // Update
                                    if (nodeAttributes[attributes[i, 0]] != null)
                                    {
                                        nodeAttributes[attributes[i, 0]].Value = attributes[i, 1];
                                    }
                                    // Create new
                                    else
                                    {
                                        XmlAttribute newAttr = node.OwnerDocument.CreateAttribute(attributes[i, 0]);
                                        newAttr.Value = attributes[i, 1];
                                        nodeAttributes.Append(newAttr);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("[XmlHelper.SetXmlNodeAttributes]: Owner document of the specified node does not exist.");
                }
            }
        }


        /// <summary>
        /// Writes DataRow to Xml writer.
        /// </summary>
        /// <param name="xml">Xml writer</param>
        /// <param name="dt">DataTable to write</param>
        public static void WriteDataTableToXml(XmlWriter xml, DataTable dt)
        {
            if (dt == null)
            {
                return;
            }

            // Write all the rows
            foreach (DataRow dr in dt.Rows)
            {
                xml.WriteStartElement(dt.TableName);

                foreach (DataColumn dc in dt.Columns)
                {
                    object value = dr[dc.ColumnName];
                    if (value != DBNull.Value)
                    {
                        // Write field value
                        xml.WriteStartElement(dc.ColumnName);

                        if (dc.DataType == typeof(byte[]))
                        {
                            // Binary data - write as Base64
                            byte[] data = (byte[])value;
                            xml.WriteBase64(data, 0, data.Length);
                        }
                        else
                        {
                            // Other
                            xml.WriteString(value.ToString());
                        }

                        xml.WriteEndElement();
                    }
                }

                xml.WriteEndElement();
            }
        }


        /// <summary>
        /// Reads the current content from the XML reader as Base64 value.
        /// </summary>
        /// <param name="xml">Xml reader</param>
        /// <param name="expectedSize">Expected size of the data</param>
        public static byte[] ReadContentAsBase64(XmlReader xml, int expectedSize)
        {
            const int bufferSize = 1024 * 1024;
            if (expectedSize > bufferSize)
            {
                GC.Collect();
            }
            
            byte[] buffer = new byte[bufferSize];

            // Read by blocks to memory stream
            using (var ms = new SystemIO.MemoryStream(expectedSize))
            {
                while (true)
                {
                    // Read the content
                    int read = xml.ReadContentAsBase64(buffer, 0, bufferSize);
                    if (read <= 0)
                    {
                        if (ms.Length > bufferSize)
                        {
                            GC.Collect();
                        }

                        // Return the full array
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, read);
                }
            }
        }


        /// <summary>
        /// Writes the given physical file to the XML as Base64.
        /// </summary>
        /// <param name="xml">Xml writer</param>
        /// <param name="filePath">File path</param>
        /// <returns>Returns the file size</returns>
        public static long WriteFileAsBase64(XmlWriter xml, string filePath)
        {
            // Check if file exists
            FileInfo fi = FileInfo.New(filePath);
            if (!fi.Exists)
            {
                return 0;
            }

            const int bufferSize = 1024 * 1024;

            // Collect the memory if the file is long enough
            if (fi.Length > bufferSize)
            {
                GC.Collect();
            }
            
            byte[] buffer = new byte[bufferSize];

            long totalRead = 0;

            try
            {
                // Read and convert by blocks
                using (FileStream reader = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
                {
                    while (true)
                    {
                        // Read the content
                        int read = reader.Read(buffer, 0, bufferSize);
                        if (read <= 0)
                        {
                            return totalRead;
                        }

                        // Write as Base64
                        xml.WriteBase64(buffer, 0, read);
                        xml.Flush();

                        totalRead += read;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Exception newEx = new Exception("[XmlHelper.WriteFileAsBase64]: Failed to write the file, " + DataHelper.GetSizeString(totalRead) + " written so far. Original message: " + ex.Message, ex);

                CoreServices.EventLog.LogException("XmlHelper", "WRITEFILEBASE64", newEx);

                throw newEx;
            }
        }


        /// <summary>
        /// Replaces the namespace colons (:) in the XML from the input XML reader with the dash (-) and writes the resulting XML using the output XML writer.
        /// E.g. atom:item element is translated to atom-item element.
        /// </summary>
        /// <param name="inputReader">Input XML reader</param>
        /// <param name="outputWriter">Output XML writer</param>
        public static void ReplaceNamespaceColon(XmlReader inputReader, XmlWriter outputWriter)
        {
            // Get the embedded namespace colon replace XSLT
            var xslt = new XslCompiledTransform();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CMS.Helpers.Markup.XML.XmlHelper.NamespaceColonReplace.xslt"))
            {
                using (var reader = XmlReader.Create(stream))
                {
                    xslt.Load(reader);
                }
            }

            // Transform
            xslt.Transform(inputReader, outputWriter);
        }


        /// <summary>
        /// XML encoding function.
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string XMLEncode(string inputText)
        {
            if (inputText == null)
            {
                return null;
            }

            return HttpUtility.HtmlEncode(inputText);
        }


        /// <summary>
        /// Converts a string that has been XML encoded into a decoded string.
        /// </summary>
        /// <param name="inputText">Input text to encode</param>
        public static string XMLDecode(string inputText)
        {
            if (inputText == null)
            {
                return null;
            }

            return HttpUtility.HtmlDecode(inputText);
        }


        /// <summary>
        /// Removes illegal characters from the given string (i.e. all non-printable characters except \t, \r, \n).
        /// </summary>
        /// <param name="inputText">Input text to process</param>
        public static string RemoveIllegalCharacters(string inputText)
        {
            if (inputText == null)
            {
                return null;
            }

            return IllegalCharactersRegEx.Replace(inputText, "");
        }


        /// <summary>
        /// Uses <see cref="XmlConvert"/> to convert these value types: <see cref="DateTime"/>, <see cref="double"/>,
        /// <see cref="decimal"/>, <see cref="float"/>, <see cref="int"/>, <see cref="long"/>, <see cref="Guid"/>, 
        /// <see cref="bool"/> to their string representation. If value is of other type, returns result of its ToString() method.
        /// Value type is determined by GetType() invocation.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static string ConvertToString(object value)
        {
            return ConvertToString(value, value.GetType());
        }


        /// <summary>
        /// Uses <see cref="XmlConvert"/> to convert these value types: <see cref="DateTime"/>, <see cref="double"/>,
        /// <see cref="decimal"/>, <see cref="float"/>, <see cref="int"/>, <see cref="long"/>, <see cref="Guid"/>, 
        /// <see cref="bool"/> to their string representation. If value is of other type, returns result of its ToString() method.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="type">Type of the value.</param>
        public static string ConvertToString(object value, Type type)
        {
            if (type == typeof(DateTime))
            {
                return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Local);
            }
            else if (type == typeof(double))
            {
                return XmlConvert.ToString((double)value);
            }
            else if (type == typeof(decimal))
            {
                return XmlConvert.ToString((decimal)value);
            }
            else if (type == typeof(float))
            {
                return XmlConvert.ToString((float)value);
            }
            else if (type == typeof(int))
            {
                return XmlConvert.ToString((int)value);
            }
            else if (type == typeof(long))
            {
                return XmlConvert.ToString((long)value);
            }
            else if (type == typeof(Guid))
            {
                return XmlConvert.ToString((Guid)value);
            }
            else if (type == typeof(bool))
            {
                return XmlConvert.ToString((bool)value);
            }
            else
            {
                return value.ToString();
            }
        }


        #endregion
    }
}