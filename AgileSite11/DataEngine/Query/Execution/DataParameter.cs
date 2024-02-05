using System;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Container for a single query parameter.
    /// </summary>
    public class DataParameter
    {
        #region "Variables"

        private Regex mRegEx;
        private object mValue;

        #endregion


        #region "Properties"

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }


        /// <summary>
        /// Parameter value.
        /// </summary>
        public object Value
        {
            get
            {
                return GetCurrentValue();
            }
            internal set
            {
                mValue = value;
            }
        }


        /// <summary>
        /// Parameter type.
        /// </summary>
        public Type Type
        {
            get;
            set;
        }


        /// <summary>
        /// Regular expression to located the parameter within text
        /// </summary>
        public Regex ParamRegEx
        {
            get
            {
                return mRegEx ?? (mRegEx = GetParameterRegEx());
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataParameter()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public DataParameter(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            name = SqlHelper.GetParameterName(name);

            Name = name;
            mValue = value;
        }


        /// <summary>
        /// Converts the data parameter to string
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0}: {1}", Name, Value);
        }


        /// <summary>
        /// Gets the regular expression for a parameter
        /// </summary>
        private Regex GetParameterRegEx()
        {
            return RegexHelper.GetRegex("(?!\\B'[^']*)" + Name + "\\b(?![^ ']*'\\B)", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
        }


        /// <summary>
        /// Expands the given expression
        /// </summary>
        /// <param name="expression">Expression to expand</param>
        public string Expand(string expression)
        {
            return ExpandInternal(expression, SqlHelper.GetSqlValue(Value));
        }


        /// <summary>
        /// Expands the given expression
        /// </summary>
        /// <param name="expression">Expression to expand</param>
        /// <param name="getValue">Custom function for getting the value</param>
        internal string Expand(string expression, Func<DataParameter, string> getValue)
        {
            if (getValue == null)
            {
                return Expand(expression);
            }

            return ExpandInternal(expression, getValue(this));
        }


        private string ExpandInternal(string expression, string value)
        {
            return ParamRegEx.Replace(expression, value, false);
        }


        /// <summary>
        /// Gets the parameter declaration
        /// </summary>
        internal string GetDeclaration()
        {
            // Use nvarchar and null for unknown types
            var typeName = "nvarchar";
            var sqlValue = DataTypeManager.NULL;

            var val = DataHelper.GetNull(Value);
            if (val != null)
            {
                // Get the data type
                var dataType = DataTypeManager.GetDataType(val.GetType());
                if (dataType != null)
                {
                    // Get type name and convert value
                    typeName = dataType.SqlType;
                    sqlValue = DataTypeManager.GetSqlValue(dataType, val);
                }
            }

            return String.Format("DECLARE {0} {1} = {2};", Name, typeName, sqlValue);
        }


        /// <summary>
        /// Returns true if the parameter equals to another parameter object. Both name and value must match in order to consider two parameters equal.
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // When same instances, consider equal
            if (this == obj)
            {
                return true;
            }

            // Other object must be data parameter as well
            var other = obj as DataParameter;
            if (other == null)
            {
                return false;
            }

            // Name check is case-insensitive, values must match exactly
            return 
                other.Name.EqualsCSafe(Name, true) && 
                Compare(GetValueToCompare(), other.GetValueToCompare()) && 
                Compare(Type, other.Type);
        }


        /// <summary>
        /// Gets the value for the parameter comparison
        /// </summary>
        protected virtual object GetValueToCompare()
        {
            return mValue;
        }


        /// <summary>
        /// Gets the current value for query execution
        /// </summary>
        protected virtual object GetCurrentValue()
        {
            return mValue;
        }


        /// <summary>
        /// Compares two given values and returns true if the values match
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        private bool Compare(object value1, object value2)
        {
            if (value1 == null)
            {
                return (value2 == null);
            }

            return value1.Equals(value2);
        }


        /// <summary>
        /// Gets the hash code of the object
        /// </summary>
        public override int GetHashCode()
        {
            var notNullValue = Value ?? DBNull.Value;
            var notNullType = (object)Type ?? DBNull.Value;

            return 
                Name.ToLowerInvariant().GetHashCode() ^ 
                notNullValue.GetHashCode() ^ 
                notNullType.GetHashCode();
        }


        /// <summary>
        /// Clones the parameter
        /// </summary>
        public virtual DataParameter Clone()
        {
            // Clone the parameter
            var p = new DataParameter(Name, Value);

            p.Type = Type;

            return p;
        }

        #endregion
    }
}