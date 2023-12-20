using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class HelpersExtensions
    {
        /// <summary>
        /// Appends the specified items to the string builder.
        /// </summary>
        /// <param name="sb">String builder object</param>
        /// <param name="parts">Objects to append</param>
        public static void Append(this StringBuilder sb, params object[] parts)
        {
            foreach (object part in parts)
            {
                sb.Append(part);
            }
        }


        /// <summary>
        /// Appends the specified indentation to the string builder.
        /// </summary>
        /// <param name="sb">String builder object</param>
        /// <param name="level">Indentation level</param>
        public static void AppendIndent(this StringBuilder sb, int level)
        {
            sb.AppendIndent(level, "  ");
        }


        /// <summary>
        /// Appends the specified indentation to the string builder.
        /// </summary>
        /// <param name="sb">String builder object</param>
        /// <param name="level">Indentation level</param>
        /// <param name="indentString">Indentation string</param>
        public static void AppendIndent(this StringBuilder sb, int level, string indentString)
        {
            // Add the specified number of levels
            for (int i = 0; i < level; i++)
            {
                sb.Append(indentString);
            }
        }


        /// <summary>
        /// Adds the new line into the string builder in case there is already some content.
        /// </summary>
        /// <param name="sb">String builder object</param>
        public static void NewLine(this StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.Append("\r\n");
            }
        }
        

        /// <summary>
        /// Joins the given list of values with a given separator.
        /// </summary>
        /// <remarks>
        /// Null entries from input are skipped. Returns empty string if <paramref name="values"/> is null. Consider using framework method <see cref="string.Join(string,IEnumerable{string})"/>
        /// </remarks>
        /// <param name="values">Values</param>
        /// <param name="separator">Separator</param>
        public static string Join(this IEnumerable<string> values, string separator)
        {
            if (values == null)
            {
                return String.Empty;
            }

            return string.Join(separator, values.Where(str => str != null));
        }


        /// <summary>
        /// Finds index of nth occurrence of string.
        /// </summary>
        /// <param name="str">String to examine</param>
        /// <param name="value">String to find</param>
        /// <param name="n">Number determining index of which occurrence to return</param>
        /// <param name="fromBegining">Whether to search from beginning or end</param>
        /// <returns>If found, returns an index of the nth occurrence. Otherwise returns -1.</returns>
        public static int NthIndexOf(this string str, string value, int n, bool fromBegining)
        {
            int lastIndex = -1;
            if ((str != null) && (value != null) && (str.Length >= value.Length))
            {
                for (int i = 1; i <= n; i++)
                {
                    if (fromBegining)
                    {
                        int startIndex = (i == 1) ? 0 : (lastIndex + value.Length);
                        if (startIndex < str.Length)
                        {
                            // Search from beginning to end
                            lastIndex = str.IndexOf(value, startIndex, StringComparison.InvariantCulture);
                        }
                    }
                    else
                    {
                        int startIndex = (i == 1) ? (str.Length - 1) : (lastIndex - value.Length);
                        if (startIndex < str.Length)
                        {
                            // Search from end to beginning
                            lastIndex = str.LastIndexOf(value, startIndex, StringComparison.InvariantCulture);
                        }
                    }
                    if (lastIndex == -1)
                    {
                        // There won't be any other occurrence
                        break;
                    }
                }
            }
            return lastIndex;
        }


        /// <summary>
        /// Truncates string to specified length.
        /// </summary>
        /// <param name="str">Given string</param>
        /// <param name="length">Length to truncate string to</param>
        /// <returns>Truncated string.</returns>
        public static string Truncate(this string str, int length)
        {
            if (String.IsNullOrEmpty(str) || (str.Length < length))
            {
                return str;
            }

            return str.Substring(0, length);
        }


        /// <summary>
        /// Tries to read xml and load the data from it to the DataSet via standard .NET method. If the load fails, it removes all the forbidden whitespace characters and tries it again.
        /// </summary>
        /// <param name="ds">DataSet to fill</param>
        /// <param name="xml">XML with data</param>
        public static void TryReadXml(this DataSet ds, string xml)  
        {
            XmlParserContext xmlContext = new XmlParserContext(null, null, null, XmlSpace.None);
            XmlReaderSettings rs = new XmlReaderSettings();
            rs.ConformanceLevel = ConformanceLevel.Auto;
            rs.CheckCharacters = false;

            try
            {
                ReadXmlToDataSet(ds, xml, xmlContext, rs);
            }
            catch
            {
                string sanitizedXml = ValidationHelper.GetSafeXML(xml);
                ReadXmlToDataSet(ds, sanitizedXml, xmlContext, rs);
            }
        }


        /// <summary>
        /// Reads XML string to data set using the supplied XML context and settings.
        /// </summary>
        private static void ReadXmlToDataSet(DataSet ds, string xml, XmlParserContext xmlContext, XmlReaderSettings rs)
        {
            using (XmlTextReader xmlTextReader = new XmlTextReader(xml, XmlNodeType.Element, xmlContext))
            {
                using (XmlReader xmlReader = XmlReader.Create(xmlTextReader, rs))
                {
                    ds.ReadXml(xmlReader);
                }
            }
        }
    }
}