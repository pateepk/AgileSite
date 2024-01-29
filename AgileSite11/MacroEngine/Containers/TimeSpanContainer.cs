using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Object encapsulating TimeSpan objects to be accessible via macro engine.
    /// </summary>
    public class TimeSpanContainer : IDataContainer, IFormattable
    {
        #region "Variables"

        private TimeSpan mTimeSpan = TimeSpan.MinValue;

        private List<string> mColumnNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the encapsulated TimeSpan.
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return this.mTimeSpan;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of TimeSpanContainer.
        /// </summary>
        /// <param name="ts">TimeSpan object to be encapsulated</param>
        public TimeSpanContainer(TimeSpan ts)
        {
            this.mTimeSpan = ts;
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
                    mColumnNames = new List<string> { "Days", "Hours", "Minutes", "Seconds", "Milliseconds", "Ticks", "TotalDays", "TotalHours", "TotalMillisecond", "TotalMinutes", "TotalSeconds" };
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
            if (this.TimeSpan != null)
            {
                switch (columnName.ToLowerCSafe())
                {
                    case "days":
                        value = this.TimeSpan.Days;
                        return true;

                    case "hours":
                        value = this.TimeSpan.Hours;
                        return true;

                    case "minutes":
                        value = this.TimeSpan.Minutes;
                        return true;

                    case "seconds":
                        value = this.TimeSpan.Seconds;
                        return true;

                    case "milliseconds":
                        value = this.TimeSpan.Milliseconds;
                        return true;

                    case "tcks":
                        value = this.TimeSpan.Ticks;
                        return true;

                    case "totaldays":
                        value = this.TimeSpan.TotalDays;
                        return true;

                    case "totalhours":
                        value = this.TimeSpan.TotalHours;
                        return true;

                    case "totalmilliseconds":
                        value = this.TimeSpan.TotalMilliseconds;
                        return true;

                    case "totalminutes":
                        value = this.TimeSpan.TotalMinutes;
                        return true;

                    case "totalseconds":
                        value = this.TimeSpan.TotalSeconds;
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


        /// <summary>
        /// Returns string representation of datetime.
        /// </summary>
        public override string ToString()
        {
            return TimeSpan.ToString();
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
            return TimeSpan.ToString(format, formatProvider);
        }


        /// <summary>
        /// Returns true if time spans are same.
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        public override bool Equals(object obj)
        {
            if (obj is TimeSpan)
            {
                return TimeSpan.Equals((TimeSpan)obj);
            }
            return TimeSpan.Equals(obj);
        }


        /// <summary>
        /// Returns hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return TimeSpan.GetHashCode();
        }
    }
}