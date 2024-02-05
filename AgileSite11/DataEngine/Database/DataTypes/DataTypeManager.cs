using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides management of SQL data types
    /// </summary>
    public class DataTypeManager
    {
        #region "Variables"

        /// <summary>
        /// Minimum DateTime value for date time data type
        /// </summary>
        public static readonly DateTime MIN_DATETIME = new DateTime(1, 1, 1);

        /// <summary>
        /// Minimum DateTime value for date time data type
        /// </summary>
        public static readonly DateTime MAX_DATETIME = new DateTime(9999, 12, 31, 23, 59, 59, 997);


        /// <summary>
        /// Registered data types
        /// </summary>
        private static readonly List<DataType> mDataTypes = new List<DataType>();

        /// <summary>
        /// Data types by field type
        /// </summary>
        private static readonly StringSafeDictionary<DataType> mDataTypesByFieldType = new StringSafeDictionary<DataType>();

        /// <summary>
        /// Data types by SQL type
        /// </summary>
        private static readonly StringSafeDictionary<DataType> mDataTypesBySqlType = new StringSafeDictionary<DataType>();

        /// <summary>
        /// Data types by system type
        /// </summary>
        private static readonly SafeDictionary<Type, DataType> mDataTypesByType = new SafeDictionary<Type, DataType>();

        /// <summary>
        /// Data types by schema type
        /// </summary>
        private static readonly StringSafeDictionary<DataType> mDataTypesBySchemaType = new StringSafeDictionary<DataType>();



        private static bool defaultTypesRegistered;
        private static readonly object lockObject = new object();

        private static IEnumerable<string> mFieldTypes;
        private static IEnumerable<string> mVisibleFieldTypes;


        /// <summary>
        /// Represents the format string for a plain SQL value
        /// </summary>
        public const string PLAIN = "{0}";

        /// <summary>
        /// Represents the format string for a unicode text on SQL server
        /// </summary>
        public const string UNICODE = "N'{0}'";

        /// <summary>
        /// Represents the format string for a string value in C# code
        /// </summary>
        public const string CODE_VALUE_FORMAT_STRING = "\"{0}\"";

        /// <summary>
        /// Represents null value
        /// </summary>
        internal const string NULL = "NULL";


        /// <summary>
        /// Collection of recognized numeric types
        /// </summary>
        private static readonly HashSet<Type> numericTypes = new HashSet<Type>
                {
                    typeof(float),
                    typeof(double),
                    
                    typeof(decimal)
                };

        /// <summary>
        /// Collection of recognized numeric types
        /// </summary>
        private static readonly HashSet<Type> integerTypes = new HashSet<Type>
                {
                    typeof(byte),
                    typeof(sbyte),

                    typeof(ushort),
                    typeof(short),

                    typeof(uint),
                    typeof(int),

                    typeof(ulong),
                    typeof(long)                    
                };

        #endregion


        #region "Properties"

        /// <summary>
        /// Registered data types
        /// </summary>
        public static IEnumerable<string> FieldTypes
        {
            get
            {
                EnsureFieldTypes(ref mFieldTypes);

                return mFieldTypes;
            }
        }


        /// <summary>
        /// Registered data types
        /// </summary>
        public static IEnumerable<string> VisibleFieldTypes
        {
            get
            {
                EnsureFieldTypes(ref mVisibleFieldTypes, type => !type.Hidden);

                return mVisibleFieldTypes;
            }
        }


        /// <summary>
        /// Registered data types
        /// </summary>
        public static IEnumerable<DataType> DataTypes
        {
            get
            {
                EnsureSystemTypes();

                return mDataTypes;
            }
        }


        /// <summary>
        /// Registered data types by field type
        /// </summary>
        private static StringSafeDictionary<DataType> DataTypesByFieldType
        {
            get
            {
                EnsureSystemTypes();

                return mDataTypesByFieldType;
            }
        }


        /// <summary>
        /// Registered data types by system type
        /// </summary>
        private static SafeDictionary<Type, DataType> DataTypesByType
        {
            get
            {
                EnsureSystemTypes();

                return mDataTypesByType;
            }
        }


        /// <summary>
        /// Registered data types by field type
        /// </summary>
        private static StringSafeDictionary<DataType> DataTypesBySqlType
        {
            get
            {
                EnsureSystemTypes();

                return mDataTypesBySqlType;
            }
        }


        /// <summary>
        /// Registered data types by schema type
        /// </summary>
        private static StringSafeDictionary<DataType> DataTypesBySchemaType
        {
            get
            {
                EnsureSystemTypes();

                return mDataTypesBySchemaType;
            }
        }

        #endregion


        #region "Registration methods"

        /// <summary>
        /// Ensures the list of field types
        /// </summary>
        /// <param name="fieldTypes">Resulting field types</param>
        /// <param name="condition">Condition for the data type to be included</param>
        private static void EnsureFieldTypes(ref IEnumerable<string> fieldTypes, Func<DataType, bool> condition = null)
        {
            if (fieldTypes == null)
            {
                lock (lockObject)
                {
                    if (fieldTypes == null)
                    {
                        fieldTypes = GetDataTypes(condition).Select(type => type.FieldType).ToList();
                    }
                }
            }
        }


        /// <summary>
        /// Gets the data types matching the given condition
        /// </summary>
        /// <param name="condition">Data type condition</param>
        public static IEnumerable<DataType> GetDataTypes(Func<DataType, bool> condition = null)
        {
            // Collect all field types
            foreach (var type in DataTypes)
            {
                if ((condition == null) || condition(type))
                {
                    yield return type;
                }
            }
        }


        /// <summary>
        /// Registers the given data type to the system
        /// </summary>
        public static void RegisterDataTypes(params DataType[] types)
        {
            EnsureSystemTypes();

            RegisterDataTypesInternal(false, types);
        }


        /// <summary>
        /// Registers the given data type to the system
        /// </summary>
        /// <param name="defaultTypes">If true, the registered types are default types</param>
        /// <param name="types">Types to register</param>
        private static void RegisterDataTypesInternal(bool defaultTypes, params DataType[] types)
        {
            foreach (var type in types)
            {
                // Ensure default flag
                if (defaultTypes)
                {
                    type.IsDefaultType = true;
                }

                RegisterDataTypeInternal(type);
            }
        }


        /// <summary>
        /// Registers the given data type
        /// </summary>
        /// <param name="type">Data type to register</param>
        private static void RegisterDataTypeInternal(DataType type)
        {
            mDataTypes.Add(type);

            RegisterType(mDataTypesByFieldType, type.FieldType, type);
            RegisterType(mDataTypesBySqlType, type.SqlType, type);
            RegisterType(mDataTypesBySchemaType, type.SchemaType, type);

            RegisterType(mDataTypesByType, type.Type, type);
        }


        /// <summary>
        /// Registers the given type within the given dictionary
        /// </summary>
        /// <param name="dictionary">Type dictionary</param>
        /// <param name="key">Type key</param>
        /// <param name="type">Type to register</param>
        private static void RegisterType<TKey>(IGeneralIndexable<TKey, DataType> dictionary, TKey key, DataType type)
        {
            var existing = dictionary[key];

            // Overwrite only in case not registered, previous not default, or this is default
            if ((existing == null) || !existing.IsDefaultType || type.IsDefaultType)
            {
                dictionary[key] = type;
            }
        }


        /// <summary>
        /// Registers the default supported data types
        /// </summary>
        private static void EnsureSystemTypes()
        {
            if (!defaultTypesRegistered)
            {
                lock (lockObject)
                {
                    if (!defaultTypesRegistered)
                    {
                        RegisterSystemTypes();

                        defaultTypesRegistered = true;
                    }
                }
            }
        }


        /// <summary>
        /// Registers the default data types to the system
        /// </summary>
        private static void RegisterSystemTypes()
        {
            RegisterDataTypesInternal(true,
                new DataType<bool>("bit", FieldDataType.Boolean, "xs:boolean", ValidationHelper.GetBoolean)
                {
                    TypeAlias = "bool",
                    SqlValueFormat = PLAIN,
                    AllowEmpty = false,
                    TypeGroup = "Boolean"
                },
                new DataType<int>("int", FieldDataType.Integer, "xs:int", ValidationHelper.GetInteger)
                {
                    TypeAlias = "int",
                    SqlValueFormat = PLAIN,
                    MaxValueLength = 11,
                    TypeGroup = "Integer"
                },
                new DataType<long>("bigint", FieldDataType.LongInteger, "xs:long", ValidationHelper.GetLong)
                {
                    TypeAlias = "long",
                    SqlValueFormat = PLAIN,
                    MaxValueLength = 20,
                    TypeGroup = "Integer"
                },
                new DataType<double>("float", FieldDataType.Double, "xs:double", ValidationHelper.GetDouble)
                {
                    CodeValueFormat = "{0}d",
                    DefaultValue = 0d,
                    DefaultValueCode = "0d",
                    TypeAlias = "double",
                    SqlValueFormat = PLAIN,
                    TypeGroup = "Decimal",
                    GetFormatProvider = culture => culture.NumberFormat
                },
                new DataType<DateTime>("datetime2", FieldDataType.DateTime, "xs:dateTime", ValidationHelper.GetDateTime)
                {
                    DbType = SqlDbType.DateTime2,
                    DefaultValueCode = "DateTimeHelper.ZERO_TIME",
                    CodeValueFormat = "DateTime.Parse(\"{0}\")",

                    VariablePrecision = true,
                    DefaultPrecision = 7,
                    MaxPrecision = 7,
                    
                    SpecialDefaultValues = new List<string>
                            {
                                DateTimeHelper.MACRO_DATE_TODAY,
                                DateTimeHelper.MACRO_TIME_NOW
                            },

                    TypeGroup = "DateTime",
                    GetFormatProvider = culture => culture.DateTimeFormat
                },
                new DataType<string>("nvarchar(max)", FieldDataType.LongText, "xs:string", ValidationHelper.GetString)
                {
                    TypeAlias = "string",
                    SqlValueFormat = UNICODE,
                    CodeValueFormat = CODE_VALUE_FORMAT_STRING,
                    DefaultValueCode = "String.Empty",

                    DefaultValue = "",
                    SupportsTranslation = true,
                    TypeGroup = "LongText"
                }, 
                new DataType<byte[]>("varbinary(max)", FieldDataType.Binary, "xs:base64binary", ValidationHelper.GetBinary)
                {
                    TypeAlias = "byte[]",
                    SupportsTranslation = true,
                    DbType = SqlDbType.VarBinary,
                    TypeGroup = "Binary",
                    HasConfigurableDefaultValue = false,
                    AllowedObjectTypes = new []
                    {
                        DataClassInfo.OBJECT_TYPE_SYSTEMTABLE, 
                        DataClassInfo.OBJECT_TYPE
                    }
                },
                new DataType<Guid>("uniqueidentifier", FieldDataType.Guid, "xs:string", ValidationHelper.GetGuid)
                {
                    DefaultValueCode = "Guid.Empty",
                    TypeGroup = "Guid",
                },
                new DataType<decimal>("decimal", FieldDataType.Decimal, "xs:decimal", ValidationHelper.GetDecimal)
                {
                    TypeAlias = "decimal",
                    SqlValueFormat = PLAIN,
                    CodeValueFormat = "{0}m",
                    DefaultValue = 0m,
                    DefaultValueCode = "0m",
                    VariableSize = true,
                    DefaultSize = 19,
                    MaxSize = 28,
                    VariablePrecision = true,
                    DefaultPrecision = 4,
                    MaxPrecision = 28,
                    TypeGroup = "Decimal",
                    GetFormatProvider = culture => culture.NumberFormat
                },
                new DataType<TimeSpan>("time", FieldDataType.TimeSpan, "xs:duration", ValidationHelper.GetTimeSpan)
                {
                    DbType = SqlDbType.Time,
                    DefaultValueCode = "TimeSpan.MinValue",
                    VariablePrecision = true,
                    DefaultPrecision = 7,
                    MaxPrecision = 7,
                    CodeValueFormat = "TimeSpan.Parse(\"{0}\")",
                    TypeGroup = "Decimal",
                    GetFormatProvider = culture => culture.DateTimeFormat
                },
                new DataType<IEnumerable<int>>("Type_CMS_IntegerTable", "", PLAIN, (value, defaultValue, culture) => value as IEnumerable<int> ?? defaultValue)
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true,
                    TypeName = "Type_CMS_IntegerTable"
                },
                new DataType<IOrderedEnumerable<int>>("Type_CMS_OrderedIntegerTable", "", PLAIN, (value, defaultValue, culture) => value as IOrderedEnumerable<int> ?? defaultValue)
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true,
                    TypeName = "Type_CMS_OrderedIntegerTable"
                },
                new DataType<IEnumerable<Guid>>("Type_CMS_GuidTable", "", PLAIN, (value, defaultValue, culture) => value as IEnumerable<Guid> ?? defaultValue)
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true,
                    TypeName = "Type_CMS_GuidTable"
                },
                new DataType<IEnumerable<long>>("Type_CMS_BigIntTable", "", PLAIN, (value, defaultValue, culture) => value as IEnumerable<long> ?? defaultValue)
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true,
                    TypeName = "Type_CMS_BigIntTable"
                },
                new DataType<IEnumerable<string>>("Type_CMS_StringTable", "", PLAIN, (value, defaultValue, culture) => value as IEnumerable<string> ?? defaultValue)
                {
                    DbType = SqlDbType.Structured,
                    Hidden = true,
                    TypeName = "Type_CMS_StringTable"
                }
            );

            RegisterDataTypesInternal(false,
                new DataType<byte[]>("varbinary", FieldDataType.Binary, "xs:base64binary", ValidationHelper.GetBinary)
                {
                    Hidden = true,
                    TypeAlias = "byte[]",
                    SupportsTranslation = true,
                    DbType = SqlDbType.VarBinary,
                    TypeGroup = "Binary",

                    VariableSize = true,
                    DefaultSize = 50,
                    MaxSize = 4000,

                    HasConfigurableDefaultValue = false,
                    AllowedObjectTypes = new[]
                    {
                        DataClassInfo.OBJECT_TYPE_SYSTEMTABLE, 
                        DataClassInfo.OBJECT_TYPE
                    }
                },
                new DataType<Guid>("uniqueidentifier", FieldDataType.File, "xs:string", ValidationHelper.GetGuid)
                {
                    TypeGroup = "File",
                    DefaultValueCode = "Guid.Empty",
                    SupportsTranslation = true,
                    AllowAsAliasSource = false
                },
                new DataType<string>("nvarchar", FieldDataType.Text, "xs:string", ValidationHelper.GetString)
                {
                    TypeAlias = "string",
                    SqlValueFormat = UNICODE,
                    CodeValueFormat = CODE_VALUE_FORMAT_STRING,
                    DefaultValueCode = "String.Empty",

                    VariableSize = true,
                    DefaultSize = 200,
                    MaxSize = 4000,
                    DefaultValue = "",
                    SupportsTranslation = true,
                    DbType = SqlDbType.NVarChar,
                    TypeGroup = "Text"
                },
                new DataType<DateTime>("date", FieldDataType.Date, "xs:dateTime", ValidationHelper.GetDate)
                {
                    DbType = SqlDbType.Date,
                    DefaultValueCode = "DateTimeHelper.ZERO_TIME",
                    CodeValueFormat = "DateTime.Parse(\"{0}\")",
                    SpecialDefaultValues = new List<string> { DateTimeHelper.MACRO_DATE_TODAY },
                    TypeGroup = "DateTime",
                    StringFormat = "{0:d}",
                    GetFormatProvider = culture => culture.DateTimeFormat
                },
                new DataType<string>("xml", FieldDataType.Xml, "xs:string", ValidationHelper.GetString)
                {
                    DbType = SqlDbType.Xml,
                    Hidden = true
                }
            );
        }


        /// <summary>
        /// Clears the registered field types
        /// </summary>
        public static void ClearRegisteredTypes()
        {
            mDataTypes.Clear();

            mDataTypesByFieldType.Clear();
            mDataTypesBySqlType.Clear();
            mDataTypesBySchemaType.Clear();

            mDataTypesByType.Clear();

            mFieldTypes = null;
            mVisibleFieldTypes = null;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets column type for defined data type and size.
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="size">Size of the type</param>
        /// <param name="precision">Precision of the type</param>
        public static string GetSqlType(string dataType, int size, int precision)
        {
            var type = GetDataType(TypeEnum.Field, dataType);
            if (type == null)
            {
                throw new NotSupportedException("Field type '" + dataType + "' is not registered, register the type with method RegisterDataTypes(...)");
            }

            return type.GetSqlType(size, precision);
        }


        /// <summary>
        /// Returns form field data type from database column data type.
        /// Some sql types map to multiple field types that differ by size, e.g. nvarchar has more field data types that differ by size.
        /// Specify size to get more accurate result. By default max size for given sql type is used.
        /// </summary>
        /// <param name="sqlType">Database column data type</param>
        /// <param name="size">Specify the size of sql type to get accurate result</param>
        /// <param name="throwException">Indicates if exception should be thrown for unsupported SQL data type</param>
        public static string GetFieldType(string sqlType, int size = -1, bool throwException = false)
        {
            var type = GetDataType(TypeEnum.SQL, sqlType);
            if (type == null)
            {
                if (throwException)
                {
                    throw new NotSupportedException("SQL type '" + sqlType + "' is not registered, register the type with method RegisterDataTypes(...)");
                }

                return FieldDataType.Unknown;
            }
            else if ((type.FieldType == FieldDataType.Text) && (size == -1))
            {
                // If the size is -1 and type was recognized as Text, then the data type is nvarchar(max) = LongText
                return FieldDataType.LongText;
            }

            return type.FieldType;
        }


        /// <summary>
        /// Gets the value representation for a SQL query text.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        public static string GetSqlValue(object value)
        {
            if (value == null)
            {
                return GetSqlValue(null, null);
            }

            var type = GetDataType(value.GetType());
            return GetSqlValue(type, value);
        }


        /// <summary>
        /// Gets the value representation for a SQL query text
        /// </summary>
        /// <param name="typeEnum">Type to use</param>
        /// <param name="typeValue">Sql type</param>
        /// <param name="value">Value to be converted.</param>
        public static string GetSqlValue(TypeEnum typeEnum, string typeValue, object value)
        {
            return GetSqlValue(GetDataType(typeEnum, typeValue), value);
        }


        /// <summary>
        /// Converts the value to a proper data type
        /// </summary>
        /// <param name="typeEnum">Type to use</param>
        /// <param name="typeValue">Data type</param>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture to use for conversion</param>
        /// <param name="nullIfDefault">If true, and the resulting value is the default value, returns null</param>
        public static object ConvertToSystemType(TypeEnum typeEnum, string typeValue, object value, CultureInfo culture = null, bool nullIfDefault = false)
        {
            // For unknown type, convert to string
            if (String.IsNullOrEmpty(typeValue))
            {
                return ValidationHelper.GetString(value, "");
            }

            var type = GetDataType(typeEnum, typeValue);
            if (type == null)
            {
                throw new NotSupportedException("SQL type '" + typeValue + "' is not registered, register the type with method RegisterDataTypes(...)");
            }

            // Convert the value
            var result = type.Convert(value, culture, type.ObjectDefaultValue);

            // Null, in case the conversion falls back to default value
            if (nullIfDefault && (result != null) && result.Equals(type.ObjectDefaultValue))
            {
                result = null;
            }
            
            return result;
        }


        /// <summary>
        /// Converts the value to a string value
        /// </summary>
        /// <param name="typeEnum">Type to use</param>
        /// <param name="typeValue">Data type</param>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture to use for conversion</param>
        public static string GetStringValue(TypeEnum typeEnum, string typeValue, object value, CultureInfo culture = null)
        {
            // For unknown type, convert to string
            if (String.IsNullOrEmpty(typeValue))
            {
                return ValidationHelper.GetString(value, "");
            }

            var type = GetDataType(typeEnum, typeValue);
            if (type == null)
            {
                throw new NotSupportedException("SQL type '" + typeValue + "' is not registered, register the type with method RegisterDataTypes(...)");
            }

            // Convert the value
            return type.GetString(value, culture);
        }


        /// <summary>
        /// Gets the value representation for a SQL query text
        /// </summary>
        /// <param name="type">Data type</param>
        /// <param name="value">Value</param>
        internal static string GetSqlValue(DataType type, object value)
        {
            if (type == null)
            {
                if ((value == null) || (value == DBNull.Value))
                {
                    return NULL;
                }

                // Encapsulate unknown types in string to prevent injection
                return String.Format("'{0}'", SqlHelper.GetSafeQueryString(value.ToString(), false));
            }

            // Convert the value to a proper type
            value = type.Convert(value, CultureHelper.EnglishCulture, type.ObjectDefaultValue);

            return type.GetSqlValue(value);
        }


        /// <summary>
        /// Gets the data type settings by its SQL type
        /// </summary>
        /// <param name="typeEnum"></param>
        /// <param name="typeValue">SQL type</param>
        public static DataType GetDataType(TypeEnum typeEnum, string typeValue)
        {
            // Try to find by exact type
            DataType result = null;

            switch (typeEnum)
            {
                case TypeEnum.Field:
                    // Field type
                    result = DataTypesByFieldType[typeValue];
                    break;

                case TypeEnum.Schema:
                    // Schema type
                    result = DataTypesBySchemaType[typeValue];
                    break;

                case TypeEnum.SQL:
                    // SQL type
                    {
                        result = DataTypesBySqlType[typeValue];

                        if (result == null)
                        {
                            // Remove size from type to cover variable SQL types
                            int bracket = typeValue.IndexOf('(');
                            if (bracket > 0)
                            {
                                typeValue = typeValue.Substring(0, bracket);
                            }
                            result = DataTypesBySqlType[typeValue];
                        }
                    }
                    break;
            }

            return result;
        }


        /// <summary>
        /// Gets the data type by the system type
        /// </summary>
        /// <param name="type">System type</param>
        public static DataType GetDataType(Type type)
        {
            return DataTypesByType[type];
        }


        /// <summary>
        /// Gets the system type by the field type
        /// </summary>
        /// <param name="typeEnum">Type to search</param>
        /// <param name="typeValue">Field type</param>
        public static Type GetSystemType(TypeEnum typeEnum, string typeValue)
        {
            var type = GetDataType(typeEnum, typeValue);
            if (type == null)
            {
                return typeof(string);
            }

            return type.Type;
        }


        /// <summary>
        /// Gets the default type for the given type by its system type
        /// </summary>
        /// <param name="typeEnum">Type to use</param>
        /// <param name="typeValue">Data type</param>
        public static DataType GetDefaultType(TypeEnum typeEnum, string typeValue)
        {
            var type = GetDataType(typeEnum, typeValue);
            if ((type != null) && !type.IsDefaultType)
            {
                // Get original type by system type
                type = DataTypesByType[type.Type];
            }

            return type;
        }


        /// <summary>
        /// Returns TRUE, if the given type corresponds to variable-length column. A variable-length column can be one
        /// of the following data types: varchar, nvarchar, varchar(max), nvarchar(max), varbinary, varbinary(max),
        /// text, ntext, image, sql_variant, and xml.
        /// </summary>
        /// <param name="dataType">Data type</param>
        internal static bool IsVariableLengthType(string dataType)
        {
            string sqlType = GetSqlType(dataType, 1, 1);
            return (sqlType != null) && sqlType.StartsWithAny(StringComparison.InvariantCultureIgnoreCase, "varchar", "nvarchar", "varbinary", "text", "ntext", "image", "sql_variant", "xml");
        }


        /// <summary>
        /// Returns given default value represented as string for further usage
        /// </summary>
        /// <param name="dataType">SQL data type</param>
        /// <param name="defaultValue">default value representation in obtained directly from SQL database</param>
        /// <returns>default value as string</returns>
        internal static string GetDefaultStringValue(string dataType, object defaultValue)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException(nameof(defaultValue));
            }

            // Default nvarchar value could be wrapped.
            if (IsString(TypeEnum.SQL, dataType))
            {
                var regex = new Regex(SqlSecurityHelper.SQLString);
                var match = regex.Match(defaultValue.ToString());
                defaultValue = match.Value.Trim('\'');

                return GetStringValue(TypeEnum.SQL, dataType, defaultValue);
            }

            return defaultValue.ToString(null);
        }

        #endregion


        #region "Type validation methods"

        /// <summary>
        /// Returns true, if the given type is a registered known type
        /// </summary>
        /// <param name="type">Type to check</param>
        public static bool IsKnownType(Type type)
        {
            return GetDataType(type) != null;
        }


        /// <summary>
        /// Returns true if the given field type supports translation
        /// </summary>
        /// <param name="typeEnum">Type to search</param>
        /// <param name="typeValue">DataType of the field</param>
        public static bool SupportsTranslation(TypeEnum typeEnum, string typeValue)
        {
            // Try to find by exact type
            var type = GetDataType(typeEnum, typeValue);
            if (type != null)
            {
                return type.SupportsTranslation;
            }

            return false;
        }


        /// <summary>
        /// Returns if start date of the scheduled interval is valid.
        /// </summary>
        /// <param name="date">Start date of the scheduled interval</param>
        public static bool IsValidDate(DateTime date)
        {
            return (date > MIN_DATETIME) && (date < MAX_DATETIME);
        }


        /// <summary>
        /// Returns true, if the given type is a string type
        /// </summary>
        /// <param name="typeEnum">Type enum</param>
        /// <param name="typeValue">Type value</param>
        public static bool IsString(TypeEnum typeEnum, string typeValue)
        {
            return IsType<string>(typeEnum, typeValue);
        }


        /// <summary>
        /// Returns true, if the given type is a number type
        /// </summary>
        /// <param name="typeEnum">Type enum</param>
        /// <param name="typeValue">Type value</param>
        public static bool IsNumber(TypeEnum typeEnum, string typeValue)
        {
            var systemType = GetSystemType(typeEnum, typeValue);

            return numericTypes.Contains(systemType) || integerTypes.Contains(systemType);
        }


        /// <summary>
        /// Returns true, if the given type is an integer type
        /// </summary>
        /// <param name="typeEnum">Type enum</param>
        /// <param name="typeValue">Type value</param>
        public static bool IsInteger(TypeEnum typeEnum, string typeValue)
        {
            var systemType = GetSystemType(typeEnum, typeValue);

            return integerTypes.Contains(systemType);
        }


        /// <summary>
        /// Returns true, if the given type is a GUID type
        /// </summary>
        /// <param name="typeEnum">Type enum</param>
        /// <param name="typeValue">Type value</param>
        public static bool IsType<T>(TypeEnum typeEnum, string typeValue)
        {
            var systemType = GetSystemType(typeEnum, typeValue);

            return systemType == typeof(T);
        }


        /// <summary>
        /// Gets the registered field groups
        /// </summary>
        public static IEnumerable<string> GetFieldGroups()
        {
            return
                DataTypes
                    .Select(t => t.TypeGroup)
                    .Where(g => !string.IsNullOrEmpty(g))
                    .Distinct()
                    .Union(new[] { "Visibility" });
        }


        /// <summary>
        /// Returns list of all field types for the given object type
        /// </summary>
        /// <param name="objectType">Object type, if not specified or null, all field types are returned</param>
        /// <param name="onlyVisible">If true, only visible field types are provided</param>
        public static IEnumerable<string> GetFieldTypes(string objectType = null, bool onlyVisible = true)
        {
            var types = GetDataTypes().Where(t => t.IsAvailableForObjectType(objectType));

            if (onlyVisible)
            {
                types = types.Where(t => !t.Hidden);
            }

            return types.Select(t => t.FieldType.ToLowerInvariant());
        }

        #endregion
    }
}
