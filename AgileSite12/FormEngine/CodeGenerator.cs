using System;
using System.Text;

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
        /// <param name="useSystemMethods">If true, system method are used for conversion of values</param>
        public static string GetPropertyCode(FormFieldInfo ffi, bool useSystemMethods)
        {
            StringBuilder sb = new StringBuilder();

            string fieldName = ffi.Name;

            // Prepare the property
            string def;
            string type = GetDataType(ffi);
            string defaultValue = ffi.GetPropertyValue(FormFieldPropertyEnum.DefaultValue);
            string func = GetValidationHelperMethodName(ffi.DataType);

            switch (ffi.DataType)
            {
                case FieldDataType.DateTime:
                    {
                        // Date time
                        DateTime defDt = ValidationHelper.GetDateTime(defaultValue, DateTime.MinValue);
                        def = (defDt != DateTime.MinValue) ? defDt.ToString() : "DateTime.MinValue";

                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Date:
                    {
                        // Date time
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
                        def = ValidationHelper.GetDouble(defaultValue, 0).ToString();
                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Decimal:
                    {
                        // Decimal
                        def = ValidationHelper.GetDecimal(defaultValue, 0).ToString();
                        if (useSystemMethods)
                        {
                            func += "System";
                        }
                    }
                    break;

                case FieldDataType.Integer:
                    {
                        // Integer
                        def = ValidationHelper.GetInteger(defaultValue, 0).ToString();
                    }
                    break;

                case FieldDataType.LongInteger:
                    {
                        // Long
                        def = ValidationHelper.GetLong(defaultValue, 0).ToString();
                    }
                    break;

                case FieldDataType.Boolean:
                    {
                        // Bool
                        def = ValidationHelper.GetBoolean(defaultValue, false).ToString().ToLowerInvariant();
                    }
                    break;

                case FieldDataType.Text:
                case FieldDataType.LongText:
                    {
                        // String
                        def = "@\"" + defaultValue.Replace("\"", "\"\"") + "\"";
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
            sb.Append(def);

            // Setter
            sb.Append(");\r\n\t\t\t}\r\n\t\t\tset\r\n\t\t\t{\r\n\t\t\t\tthis.SetValue(\"", fieldName, "\", value);");
            sb.Append("\r\n\t\t\t}\r\n\t\t}");

            return sb.ToString();
        }


        /// <summary>
        /// Gets the C# code for the properties based on the given form information.
        /// </summary>
        /// <param name="fi">Form information</param>
        /// <param name="systemFields">If true, the system fields are included to the code generation</param>
        /// <param name="useSystemMethods">If true, system method are used for conversion of values</param>
        public static string GetPropertiesCode(FormInfo fi, bool systemFields, bool useSystemMethods)
        {
            StringBuilder sb = new StringBuilder();

            int index = 0;
            foreach (FormFieldInfo ffi in fi.GetFields(true, true))
            {
                if (systemFields || !ffi.System)
                {
                    // Property code
                    string propertyCode = GetPropertyCode(ffi, useSystemMethods);
                    if (!String.IsNullOrEmpty(propertyCode))
                    {
                        // New lines before properties
                        if (index > 0)
                        {
                            sb.Append("\r\n\r\n\r\n\t\t");
                        }

                        sb.Append(propertyCode);

                        index++;
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns ValidationHelper Get* method name for specified data type.
        /// </summary>
        internal static string GetValidationHelperMethodName(string dataType)
        {
            switch (dataType.ToLowerInvariant())
            {
                case FieldDataType.Text:
                case FieldDataType.LongText:
                case FieldDataType.DocAttachments:
                    return nameof(ValidationHelper.GetString);

                case FieldDataType.Integer:
                    return nameof(ValidationHelper.GetInteger);

                case FieldDataType.LongInteger:
                    return nameof(ValidationHelper.GetLong);

                case FieldDataType.Double:
                    return nameof(ValidationHelper.GetDouble);

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return nameof(ValidationHelper.GetDateTime);

                case FieldDataType.Boolean:
                    return nameof(ValidationHelper.GetBoolean);

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return nameof(ValidationHelper.GetGuid);

                case FieldDataType.Decimal:
                    return nameof(ValidationHelper.GetDecimal);

                case FieldDataType.TimeSpan:
                    return nameof(ValidationHelper.GetTimeSpan);

                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), "Specified datatype is not supported.");
            }
        }


        /// <summary>
        /// Returns proper DataType representation
        /// </summary>
        internal static string GetDataType(FormFieldInfo field)
        {
            switch (field.DataType.ToLowerInvariant())
            {
                case FieldDataType.Text:
                case FieldDataType.LongText:
                case FieldDataType.DocAttachments:
                    return "string";

                case FieldDataType.Integer:
                    return "int";

                case FieldDataType.LongInteger:
                    return "long";

                case FieldDataType.Double:
                    return "double";

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return "DateTime";

                case FieldDataType.Boolean:
                    return "bool";

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return "Guid";

                case FieldDataType.Decimal:
                    return "decimal";

                case FieldDataType.TimeSpan:
                    return "TimeSpan";

                default:
                    throw new NotSupportedException("Specified datatype is not supported.");
            }
        }


        /// <summary>
        /// Returns default value for field. If Default value is macro, the default value of data type is used.
        /// </summary>
        internal static string GetDefaultValue(FormFieldInfo field)
        {
            string dataType = field.DataType;
            string defaultValue = field.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out var isMacro);

            bool defaultValueIsEmptyOrMacro = String.IsNullOrEmpty(defaultValue) || isMacro;

            switch (dataType.ToLowerInvariant())
            {
                case FieldDataType.DocAttachments:
                    return $"\"{(defaultValueIsEmptyOrMacro ? string.Empty : defaultValue)}\"";

                case FieldDataType.Text:
                case FieldDataType.LongText:
                    return $"@\"{(defaultValueIsEmptyOrMacro ? string.Empty : defaultValue.Replace("\"", "\"\""))}\"";

                case FieldDataType.Integer:
                case FieldDataType.LongInteger:
                case FieldDataType.Double:
                    return defaultValueIsEmptyOrMacro ? "0" : defaultValue;

                case FieldDataType.Decimal:
                    return defaultValueIsEmptyOrMacro ? "0" : $"ValidationHelper.GetDecimal(\"{defaultValue}\", 0)";

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return defaultValueIsEmptyOrMacro ? "DateTimeHelper.ZERO_TIME" : $"ValidationHelper.GetDateTime(\"{defaultValue}\", DateTimeHelper.ZERO_TIME)";

                case FieldDataType.Boolean:
                    return defaultValueIsEmptyOrMacro ? "false" : defaultValue.ToLowerInvariant();

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return defaultValueIsEmptyOrMacro ? "Guid.Empty" : $"ValidationHelper.GetGuid(\"{defaultValue}\", Guid.Empty)";

                case FieldDataType.TimeSpan:
                    return defaultValueIsEmptyOrMacro ? "TimeSpan.Zero" : $"ValidationHelper.GetTimeSpan(\"{defaultValue}\", TimeSpan.Zero)";

                default:
                    throw new NotSupportedException("Specified datatype is not supported.");
            }
        }


        /// <summary>
        /// Returns field value for field summary.
        /// </summary>
        internal static string GetSummary(FormFieldInfo fieldInfo, int indentation = 3)
        {
            var summary = fieldInfo.GetPropertyValue(FormFieldPropertyEnum.FieldDescription, out var isMacro);

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