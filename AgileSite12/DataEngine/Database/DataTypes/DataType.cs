using System;
using System.Collections.Generic;
using System.Globalization;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data type definition
    /// </summary>
    public class DataType<T> : DataType
    {
        #region "Variables"

        private string mConversionMethod;
        private string mDefaultValueCode;

        #endregion


        #region "Properties"

        /// <summary>
        /// Default value for this data type
        /// </summary>
        public T DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Default value for this data type
        /// </summary>
        public override object ObjectDefaultValue
        {
            get
            {
                return DefaultValue;
            }
        }


        /// <summary>
        /// Conversion function for this data type
        /// </summary>
        protected Func<object, T, CultureInfo, T> ConversionFunc
        {
            get;
            set;
        }


        /// <summary>
        /// Conversion function which converts value to its database representation.
        /// If not defined, value is used without any conversions.
        /// </summary>
        protected Func<T, object, CultureInfo, object> DbConversionFunc
        {
            get;
            set;
        }


        /// <summary>
        /// Function used to determine whether a given value is of this data type.
        /// If left null, all objects are considered to be of this type.
        /// </summary>
        protected Func<object, CultureInfo, bool> IsOfTypeFunction;
        

        /// <summary>
        /// Returns the conversion method
        /// </summary>
        public override string ConversionMethod
        {
            get
            {
                if (mConversionMethod == null)
                {
                    // Get the name of the static conversion method
                    if ((ConversionFunc != null) && (ConversionFunc.Method != null) && (ConversionFunc.Method.DeclaringType != null) && ConversionFunc.Method.IsStatic)
                    {
                        mConversionMethod = ConversionFunc.Method.DeclaringType.Name + "." + ConversionFunc.Method.Name;
                    }
                    else
                    {
                        mConversionMethod = String.Empty;
                    }
                }

                return mConversionMethod;
            }
            set
            {
                mConversionMethod = value;
            }
        }


        /// <summary>
        /// Code of the default value for this type
        /// </summary>
        public override string DefaultValueCode
        {
            get
            {
                if (mDefaultValueCode == null)
                {
                    // Get the name of the static conversion method
                    var val = default(T);

                    mDefaultValueCode = (val != null) ? val.ToString().ToLowerInvariant() : "";
                }

                return mDefaultValueCode;
            }
            set
            {
                mDefaultValueCode = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sqlType">SQL type</param>
        /// <param name="fieldType">Field type</param>
        /// <param name="schemaType">Schema type</param>
        /// <param name="conversionFunc">Conversion function</param>
        public DataType(string sqlType, string fieldType, string schemaType, Func<object, T, CultureInfo, T> conversionFunc)
            : this(sqlType, fieldType, schemaType, conversionFunc, null, null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sqlType">SQL type</param>
        /// <param name="fieldType">Field type</param>
        /// <param name="schemaType">Schema type</param>
        /// <param name="conversionFunc">Conversion function</param>
        /// <param name="validationFunction">Determines whether passed object value is of this data type.</param>
        public DataType(string sqlType, string fieldType, string schemaType, Func<object, T, CultureInfo, T> conversionFunc, Func<object, CultureInfo, bool> validationFunction)
        : this(sqlType, fieldType, schemaType, conversionFunc, null, null, validationFunction)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sqlType">SQL type</param>
        /// <param name="fieldType">Field type</param>
        /// <param name="schemaType">Schema type</param>
        /// <param name="conversionFunc">Conversion function</param>
        /// <param name="dbConversionFunc">Function which converts value to its database representation.</param>
        /// <param name="textSerializer">Text serializer used for this type.</param>
        public DataType(string sqlType, string fieldType, string schemaType, Func<object, T, CultureInfo, T> conversionFunc,
            Func<T, object, CultureInfo, object> dbConversionFunc, IDataTypeTextSerializer textSerializer)
        : this(sqlType, fieldType, schemaType, conversionFunc, dbConversionFunc, textSerializer, null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sqlType">SQL type</param>
        /// <param name="fieldType">Field type</param>
        /// <param name="schemaType">Schema type</param>
        /// <param name="conversionFunc">Conversion function</param>
        /// <param name="dbConversionFunc">Function which converts value to its database representation.</param>
        /// <param name="textSerializer">Text serializer used for this type.</param>
        /// <param name="validationFunction">Determines whether passed object value is of this data type.</param>
        public DataType(string sqlType, string fieldType, string schemaType, Func<object, T, CultureInfo, T> conversionFunc,
            Func<T, object, CultureInfo, object> dbConversionFunc, IDataTypeTextSerializer textSerializer, Func<object, CultureInfo, bool> validationFunction)
        {
            SqlType = sqlType;
            FieldType = fieldType;
            SchemaType = schemaType;

            ConversionFunc = conversionFunc;
            DbConversionFunc = dbConversionFunc;
            mTextSerializer = textSerializer;
            IsOfTypeFunction = validationFunction;
            Type = typeof(T);
        }


        /// <summary>
        /// Converts the value to a proper type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture used for conversion of the type</param>
        /// <param name="defaultValue">Default value to return in case the conversion fails</param>
        public override object Convert(object value, CultureInfo culture, object defaultValue)
        {
            return ConversionFunc(value, (T)defaultValue, culture);
        }


        /// <summary>
        /// Converts the value to its database representation.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture used for conversion of the type</param>
        /// <param name="defaultValue">Default value to return in case the conversion fails</param>
        public override object ConvertToDbType(object value, CultureInfo culture, object defaultValue)
        {
            if (DbConversionFunc == null)
            {
                return value;
            }

            return DbConversionFunc((T)value, (T)defaultValue, culture);
        }


        /// <summary>
        /// Uses <see cref="IsOfTypeFunction"/> to determine whether given <paramref name="value"/>
        /// is of this data type.
        /// </summary>
        /// <param name="value">Object to be tested.</param>
        /// <param name="culture">Culture to tests object for.</param>
        /// <returns>True if object is of this data type or if <see cref="IsOfTypeFunction"/> is null.
        /// Otherwise returns false.
        /// </returns>
        public override bool IsValueOfType(object value, CultureInfo culture)
        {
            if (IsOfTypeFunction == null)
            {
                return true;
            }

            return IsOfTypeFunction(value, culture);
        }

        #endregion
    }


    /// <summary>
    /// Data type definition
    /// </summary>
    public abstract class DataType
    {
        #region "Variables"

        private string mSqlValueFormat = "'{0}'";
        private bool mAllowEmpty = true;
        private bool mHasConfigurableDefaultValue = true;
        private bool mAllowAsAliasSource = true;
        private ISet<string> mAllowedObjectTypes;

        /// <summary>
        /// Serializer used to serialize and deserialize values of this data type.
        /// </summary>
        protected IDataTypeTextSerializer mTextSerializer;

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of the group that groups together types with similar behavior, such as integer, decimal, string etc.
        /// </summary>
        public string TypeGroup
        {
            get;
            set;
        }


        /// <summary>
        /// System type alias (e.g. bool for Boolean, etc.)
        /// </summary>
        public string TypeAlias
        {
            get;
            set;
        }


        /// <summary>
        /// System type
        /// </summary>
        public Type Type
        {
            get;
            protected set;
        }


        /// <summary>
        /// Field type
        /// </summary>
        public string FieldType
        {
            get;
            protected set;
        }


        /// <summary>
        /// SQL data type representation
        /// </summary>
        public string SqlType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Type representation in the XML schema of the data
        /// </summary>
        public string SchemaType
        {
            get;
            protected set;
        }


        /// <summary>
        /// SQL value format. Default format is '{0}' to prevent SQL injection
        /// </summary>
        public string SqlValueFormat
        {
            get
            {
                return mSqlValueFormat;
            }
            set
            {
                mSqlValueFormat = value;
            }
        }


        /// <summary>
        /// If true, the data type supports translation of the content
        /// </summary>
        public bool SupportsTranslation
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the data type is considered default. Types marked as default are replaced only by default types in the type mapping tables.
        /// </summary>
        public bool IsDefaultType
        {
            get;
            set;
        }


        /// <summary>
        /// Database type
        /// </summary>
        public object DbType
        {
            get;
            set;
        }


        /// <summary>
        /// Represents database type name
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the data type is hidden
        /// </summary>
        public bool Hidden
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the conversion method
        /// </summary>
        public abstract string ConversionMethod
        {
            get;
            set;
        }


        /// <summary>
        /// Code of the default value for this type
        /// </summary>
        public abstract string DefaultValueCode
        {
            get;
            set;
        }


        /// <summary>
        /// Default value for this data type
        /// </summary>
        public abstract object ObjectDefaultValue
        {
            get;
        }


        /// <summary>
        /// Formatting code of the value for this type
        /// </summary>
        public string CodeValueFormat
        {
            get;
            set;
        }


        /// <summary>
        /// List of special default values recognized by this type and treated as default value by the code
        /// </summary>
        public List<string> SpecialDefaultValues
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the data type allows for nulls in the database
        /// </summary>
        public bool AllowEmpty
        {
            get
            {
                return mAllowEmpty;
            }
            set
            {
                mAllowEmpty = value;
            }
        }


        /// <summary>
        /// If defined, the given format is used to convert the value of this type to a string value. {0} represents the value.
        /// </summary>
        public string StringFormat
        {
            get;
            set;
        }


        /// <summary>
        /// Function which gets a format provider for the given culture
        /// </summary>
        public Func<CultureInfo, IFormatProvider> GetFormatProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the type has a default value
        /// </summary>
        public bool HasConfigurableDefaultValue
        {
            get
            {
                return mHasConfigurableDefaultValue;
            }
            set
            {
                mHasConfigurableDefaultValue = value;
            }
        }


        /// <summary>
        /// Returns true if the field can be used as an alias source
        /// </summary>
        public bool AllowAsAliasSource
        {
            get
            {
                return mAllowAsAliasSource;
            }
            set
            {
                mAllowAsAliasSource = value;
            }
        }


        /// <summary>
        /// When configured, only allows data type for the given object types.
        /// Use case insensitive HashSet for the list of items
        /// </summary>
        public IEnumerable<string> AllowedObjectTypes
        {
            get
            {
                return mAllowedObjectTypes;
            }
            set
            {
                mAllowedObjectTypes = new HashSet<string>(value, StringComparer.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// Gets the <see cref="IDataTypeTextSerializer"/> instance for this data type.
        /// </summary>
        public virtual IDataTypeTextSerializer TextSerializer => mTextSerializer ?? (mTextSerializer = new DefaultDataTypeTextSerializer(FieldType));


        /// <summary>
        /// Indicates whether the data type can be found by <see cref="DataTypeManager.GetDataType(Type)"/>.
        /// </summary>
        internal bool UsableByType { get; set; } = true;


        /// <summary>
        /// Indicates whether this data type can be used as <see cref="TypeEnum.Field"/> type.
        /// </summary>
        internal bool UsableByFieldType { get; set; } = true;


        /// <summary>
        /// Indicates whether this data type can be used as <see cref="TypeEnum.SQL"/> type.
        /// </summary>
        internal bool UsableBySqlType { get; set; } = true;


        /// <summary>
        /// Indicates whether this data type can be used as <see cref="TypeEnum.Schema"/> type.
        /// </summary>
        internal bool UsableBySchemaType { get; set; } = true;


        #endregion


        #region "Size / Precision properties"

        /// <summary>
        /// If true, the data type has variable size
        /// </summary>
        public bool VariableSize
        {
            get;
            set;
        }


        /// <summary>
        /// Default size of the type, if the size is variable
        /// </summary>
        public int DefaultSize
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum size of the type, if the size is variable
        /// </summary>
        public int MaxSize
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum value length
        /// </summary>
        public int MaxValueLength
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the data type has variable precision
        /// </summary>
        public bool VariablePrecision
        {
            get;
            set;
        }


        /// <summary>
        /// Default precision of the type, if the precision is variable
        /// </summary>
        public int DefaultPrecision
        {
            get;
            set;
        } = -1;


        /// <summary>
        /// Maximum precision of the type, if the precision is variable
        /// </summary>
        public int MaxPrecision
        {
            get;
            set;
        }

        #endregion


        #region "Methods"


        /// <summary>
        /// Gets the SQL data type representation
        /// </summary>
        /// <param name="size">Data size</param>
        /// <param name="precision">Data precision</param>
        public string GetSqlType(int size, int precision)
        {
            var type = SqlType;
            if (VariableSize || VariablePrecision)
            {
                string strSize = null;

                if (VariableSize)
                {
                    // Ensure default size
                    if (size <= 0)
                    {
                        size = DefaultSize;
                    }

                    // Check that the size was provided
                    if (size <= 0)
                    {
                        throw new NotSupportedException("[DataType.GetSqlType]: Cannot define the type '" + SqlType + "' without the size being specified explicitly, or via DefaultSize property of the type.");
                    }

                    strSize += size.ToString();
                }

                if (VariablePrecision)
                {
                    // Ensure default precision
                    if (precision < 0)
                    {
                        precision = DefaultPrecision;
                    }

                    // Check that the size was provided
                    if (precision < 0)
                    {
                        throw new NotSupportedException("[DataType.GetSqlType]: Cannot define the type '" + SqlType + "' without the precision being specified explicitly, or via DefaultPrecision property of the type.");
                    }

                    if (strSize != null)
                    {
                        strSize += ",";
                    }
                    strSize += precision.ToString();
                }

                type = String.Format("{0}({1})", type, strSize);
            }

            return type;
        }


        /// <summary>
        /// Gets the SQL value representation for this type
        /// </summary>
        /// <param name="value">Value to convert to SQL value</param>
        public string GetSqlValue(object value)
        {
            var stringValue = SqlHelper.GetSafeQueryString(GetSqlValueString(value), false);

            return String.Format(SqlValueFormat, stringValue);
        }


        /// <summary>
        /// Gets a string representation of the given value on SQL server
        /// </summary>
        /// <param name="value">Value to convert</param>
        private string GetSqlValueString(object value)
        {
            return TableManager.GetValueString(value);
        }


        /// <summary>
        /// Converts the value to a proper type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture used for conversion of the type</param>
        /// <param name="defaultValue">Default value to return in case the conversion fails</param>
        public abstract object Convert(object value, CultureInfo culture, object defaultValue);


        /// <summary>
        /// Converts the value to its database representation.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture used for conversion of the type</param>
        /// <param name="defaultValue">Default value to return in case the conversion fails</param>
        public virtual object ConvertToDbType(object value, CultureInfo culture, object defaultValue)
        {
            return value;
        }


        /// <summary>
        /// Determines whether given <paramref name="value"/> is of this data type.
        /// </summary>
        /// <param name="value">Object to be tested.</param>
        /// <param name="culture">Culture to tests object for.</param>
        /// <returns>True if object is of this data type, otherwise returns false.
        /// </returns>
        public abstract bool IsValueOfType(object value, CultureInfo culture);


        /// <summary>
        /// Gets the code for the default value of this type in C# code
        /// </summary>
        /// <param name="explicitDefaultValue">Explicit default value to use</param>
        /// <param name="returnEmpty">If true, the method returns code even if default value representation is empty</param>
        public string GetDefaultValueCode(string explicitDefaultValue = null, bool returnEmpty = false)
        {
            string defaultValue;
            if (!String.IsNullOrEmpty(explicitDefaultValue) && ((SpecialDefaultValues == null) || !SpecialDefaultValues.Contains(explicitDefaultValue)))
            {
                // Get default value from field
                defaultValue = explicitDefaultValue;

                if (CodeValueFormat != null)
                {
                    defaultValue = String.Format(CodeValueFormat, defaultValue);
                }
                else if ((Type.IsClass && (Type != typeof(string))) || (Type == typeof(Guid)))
                {
                    // Wrap into the constructor for non-value types
                    defaultValue = String.Format("new {0}(\"{1}\")", Type.Name, defaultValue);
                }
            }
            else
            {
                // Get default value from type
                defaultValue = DefaultValueCode;
            }

            // Ensure null in case of no result
            if (String.IsNullOrEmpty(defaultValue))
            {
                defaultValue = returnEmpty ? "null" : null;
            }

            return defaultValue;
        }


        /// <summary>
        /// Gets the string representation of the given value
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="culture">Culture used for conversion to string</param>
        public string GetString(object value, CultureInfo culture)
        {
            if (StringFormat != null)
            {
                return FormatValue(value, culture);
            }

            return ValidationHelper.GetString(value, "", culture);
        }


        /// <summary>
        /// Formats the given value
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="culture">Culture used for format</param>
        protected virtual string FormatValue(object value, CultureInfo culture)
        {
            if ((culture != null) && (GetFormatProvider != null))
            {
                return String.Format(GetFormatProvider(culture), StringFormat, value);
            }

            return String.Format(StringFormat, value);
        }


        /// <summary>
        /// Returns true if the given data type is available for the given object type
        /// </summary>
        /// <param name="objectType">Object type to check</param>
        public bool IsAvailableForObjectType(string objectType)
        {
            return (objectType == null) || (mAllowedObjectTypes == null) || (mAllowedObjectTypes.Contains(objectType));
        }

        #endregion
    }
}