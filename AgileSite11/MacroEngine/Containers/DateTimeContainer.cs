using System;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Object encapsulating DateTime objects to be accessible via macro engine.
    /// </summary>
    [Extension(typeof(DateTimeMethods))]
    public class DateTimeContainer : IDataContainer, IConvertible, IFormattable
    {
        #region "Variables"

        private DateTime mDateTime = DateTime.MinValue;

        private List<string> mColumnNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the encapsulated DateTime.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return mDateTime;
            }
        }


        /// <summary>
        /// Gets or sets the culture of the date time (for ToString formatting purposes).
        /// </summary>
        public string Culture
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of DateTimeContainer.
        /// </summary>
        /// <param name="dt">DateTime object to be encapsulated</param>
        public DateTimeContainer(DateTime dt)
        {
            mDateTime = dt;
        }

        #endregion


        #region ISimpleDataContainer Members

        /// <summary>
        /// Gets the value of the column, setter is not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object retval = null;
            TryGetValue(columnName, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                if (mColumnNames == null)
                {
                    mColumnNames = new List<string> { "Year", "Month", "Day", "DayOfWeek", "DayOfYear", "Hour", "Minute", "Second", "Millisecond", "Date", "TimeOfDay" };
                }

                return mColumnNames;
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = null;
            if (DateTime != null)
            {
                switch (columnName.ToLowerCSafe())
                {
                    case "year":
                        value = DateTime.Year;
                        return true;

                    case "month":
                        value = DateTime.Month;
                        return true;

                    case "day":
                        value = DateTime.Day;
                        return true;

                    case "dayofweek":
                        value = DateTime.DayOfWeek;
                        return true;

                    case "dayofyear":
                        value = DateTime.DayOfYear;
                        return true;

                    case "hour":
                        value = DateTime.Hour;
                        return true;

                    case "minute":
                        value = DateTime.Minute;
                        return true;

                    case "second":
                        value = DateTime.Second;
                        return true;

                    case "millisecond":
                        value = DateTime.Millisecond;
                        return true;

                    case "date":
                        value = DateTime.Date;
                        return true;

                    case "timeofday":
                        value = DateTime.TimeOfDay;
                        return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            foreach (string item in ColumnNames)
            {
                if (item.ToLowerCSafe() == columnName.ToLowerCSafe())
                {
                    return true;
                }
            }
            return false;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns string representation of datetime.
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Culture))
            {
                return DateTime.ToString();
            }
            else
            {
                return DateTime.ToString(CultureHelper.GetCultureInfo(Culture)); 
            }
        }


        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        /// The value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">The format to use or a null reference to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. </param>
        /// <param name="formatProvider">The provider to use to format the value or a null reference to obtain the numeric format information from the current locale setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return DateTime.ToString(format, formatProvider);
        }


        /// <summary>
        /// Returns true if date times are same.
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        public override bool Equals(object obj)
        {
            if (obj is DateTime)
            {
                return DateTime.Equals((DateTime)obj);
            }
            return DateTime.Equals(obj);
        }


        /// <summary>
        /// Returns hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return DateTime.GetHashCode();
        }

        #endregion


        #region "IConvertible Methods"

        /// <summary>
        /// Returns the TypeCode for this instance.
        /// </summary>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the specified culture-specific formatting information.
        /// </summary>
        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToBoolean(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using the specified culture-specific formatting information.
        /// </summary>
        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToChar(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToSByte(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToByte(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToInt16(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToUInt16(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToInt32(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToUInt32(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToInt64(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToUInt64(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToSingle(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToDouble(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent Decimal number using the specified culture-specific formatting information.
        /// </summary>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToDecimal(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent DateTime using the specified culture-specific formatting information.
        /// </summary>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToDateTime(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an equivalent String using the specified culture-specific formatting information.
        /// </summary>
        public string ToString(IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToString(provider);
        }


        /// <summary>
        /// Converts the value of this instance to an Object of the specified Type that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)mDateTime).ToType(conversionType, provider);
        }

        #endregion
    }
}