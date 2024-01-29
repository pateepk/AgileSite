using System;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Helper for the code generation.
    /// </summary>
    public class CodeGenerator
    {
        #region "Methods"

        /// <summary>
        /// Gets the C# code for the property based on the given field.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        /// <param name="baseControl">Base control name in case the properties are mirrored to the base control</param>
        /// <param name="sbInit">String builder for the initialization code of the base control</param>
        /// <param name="useSystemMethods">If true, system method are used for conversion of values</param>
        public static string GetPropertyCode(FormFieldInfo ffi, string baseControl, StringBuilder sbInit, bool useSystemMethods)
        {
            StringBuilder sb = new StringBuilder();

            string fieldName = ffi.Name;

            // Prepare the property
            string type;
            string func;
            string def;
            string defaultValue = ffi.GetPropertyValue(FormFieldPropertyEnum.DefaultValue);

            switch (ffi.DataType)
            {
                case FieldDataType.DateTime:
                    {
                        // Date time
                        type = "DateTime";
                        DateTime defDt = ValidationHelper.GetDateTime(defaultValue, DateTime.MinValue);
                        def = (defDt != DateTime.MinValue) ? defDt.ToString() : "DateTime.MinValue";

                        func = "GetDateTime";
                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Date:
                    {
                        // Date time
                        type = "DateTime";
                        DateTime defDt = ValidationHelper.GetDate(defaultValue, DateTime.MinValue);
                        def = (defDt != DateTime.MinValue) ? defDt.ToString() : "DateTime.MinValue";

                        func = "GetDate";
                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Double:
                    {
                        // Double
                        type = "double";
                        def = ValidationHelper.GetDouble(defaultValue, 0).ToString();
                        func = "GetDouble";
                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Decimal:
                    {
                        // Decimal
                        type = "decimal";
                        def = ValidationHelper.GetDecimal(defaultValue, 0).ToString();
                        func = "GetDecimal";
                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Integer:
                    {
                        // Integer
                        type = "int";
                        def = ValidationHelper.GetInteger(defaultValue, 0).ToString();
                        func = "GetInteger";
                    }
                    break;

                case FieldDataType.LongInteger:
                    {
                        // Long
                        type = "long";
                        def = ValidationHelper.GetLong(defaultValue, 0).ToString();
                        func = "GetLong";
                    }
                    break;

                case FieldDataType.Boolean:
                    {
                        // Bool
                        type = "bool";
                        def = ValidationHelper.GetBoolean(defaultValue, false).ToString().ToLowerCSafe();
                        func = "GetBoolean";
                    }
                    break;

                case FieldDataType.Text:
                case FieldDataType.LongText:
                    {
                        // String
                        type = "string";
                        def = "@\"" + defaultValue.Replace("\"", "\"\"") + "\"";
                        func = "GetString";
                    }
                    break;

                default:
                    // Other fields - Binary, Attachments, etc.
                    return null;
            }

            // Append the summary
            string summary = GetSummary(ffi, 2);
            sb.Append("\t\t/// <summary>\r\n\t\t/// ", summary, "\r\n\t\t/// </summary>\r\n");

            // Append the property
            sb.Append("\t\tpublic ", type, " ", fieldName);

            // Getter
            sb.Append("\r\n\t\t{\r\n\t\t\tget\r\n\t\t\t{\r\n\t\t\t\treturn ValidationHelper.", func, "(this.GetValue(\"", fieldName, "\"), ");

            if (!String.IsNullOrEmpty(baseControl))
            {
                sb.Append(baseControl + "." + fieldName);

                sbInit.Append(baseControl, ".", fieldName, " = this.", fieldName, ";");
            }
            else
            {
                sb.Append(def);
            }

            // Setter
            sb.Append(");\r\n\t\t\t}\r\n\t\t\tset\r\n\t\t\t{\r\n\t\t\t\tthis.SetValue(\"", fieldName, "\", value);");
            if (!String.IsNullOrEmpty(baseControl))
            {
                sb.Append("\r\n            ");
                sb.Append(baseControl, ".", fieldName, " = value;");
            }
            sb.Append("\r\n\t\t\t}\r\n\t\t}");

            return sb.ToString();
        }


        /// <summary>
        /// Gets the C# code for the properties based on the given form information.
        /// </summary>
        /// <param name="fi">Form information</param>
        /// <param name="systemFields">If true, the system fields are included to the code generation</param>
        /// <param name="baseControl">Base control name in case the properties are mirrored to the base control</param>
        /// <param name="sbInit">String builder for the initialization code of the base control</param>
        /// <param name="useSystemMethods">If true, system method are used for conversion of values</param>
        public static string GetPropertiesCode(FormInfo fi, bool systemFields, string baseControl, StringBuilder sbInit, bool useSystemMethods)
        {
            StringBuilder sb = new StringBuilder();

            int index = 0;
            foreach (FormFieldInfo ffi in fi.GetFields(true, true))
            {
                if (systemFields || !ffi.System)
                {
                    // Property code
                    string propertyCode = GetPropertyCode(ffi, baseControl, sbInit, useSystemMethods);
                    if (!String.IsNullOrEmpty(propertyCode))
                    {
                        // New lines before properties
                        if (index > 0)
                        {
                            sb.Append("\r\n\r\n\r\n\t\t");

                            if (!String.IsNullOrEmpty(baseControl))
                            {
                                sbInit.Append("\r\n\t\t\t\t");
                            }
                        }

                        sb.Append(propertyCode);

                        index++;
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns field value for field summary.
        /// </summary>
        internal static string GetSummary(FormFieldInfo fieldInfo, int indentation = 3)
        {
            bool isMacro;
            var summary = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription, out isMacro);
            if (isMacro || String.IsNullOrEmpty(summary))
            {
                summary = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, out isMacro);
            }

            if (isMacro || String.IsNullOrEmpty(summary))
            {
                summary = fieldInfo.Name;
            }
            summary = ResHelper.LocalizeString(summary);
            if (!String.IsNullOrEmpty(summary) && !summary.TrimEnd().EndsWith(".", StringComparison.Ordinal))
            {
                summary += ".";
            }

            summary = summary ?? String.Empty;

            summary = summary.Replace(Environment.NewLine, $"{Environment.NewLine}{GetIndentationString(indentation)}/// ");

            return summary;
        }


        private static string GetIndentationString(int indentation)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < indentation; i++)
            {
                builder.Append("\t");
            }

            return builder.ToString();
        }

        #endregion
    }
}