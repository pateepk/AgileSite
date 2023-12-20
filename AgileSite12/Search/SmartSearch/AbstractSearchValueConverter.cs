using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Base class for search value converter
    /// </summary>
    [StaticContract(typeof(ISearchValueConverter), IsLocal = true)]
    public abstract class AbstractSearchValueConverter : ISearchValueConverter
    {
        #region "Properties"

        /// <summary>
        /// Gets the integer replacer regular expression.
        /// </summary>
        protected abstract Regex IntReplacerRegex
        {
            get;
        }


        /// <summary>
        /// Gets the double replacer regular expression.
        /// </summary>
        protected abstract Regex DoubleReplacerRegex
        {
            get;
        }


        /// <summary>
        /// Gets the decimal replacer regular expression.
        /// </summary>
        protected abstract Regex DecimalReplacerRegex
        {
            get;
        }


        /// <summary>
        /// Culture used to convert numbers to string and vice versa. 
        /// </summary>
        protected virtual CultureInfo Culture
        {
            get
            {
                // Search number syntax requires use of '.' character as decimal delimiter. 
                // Invariant culture uses '.' and can't be overridden by user settings. 
                return CultureInfo.InvariantCulture;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts the value to a search string representation.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public virtual string ConvertToString(object value)
        {
            if (value == null)
            {
                return String.Empty;
            }

            string stringValue;

            // Date time
            if (value is DateTime)
            {
                stringValue = DateToString((DateTime)value);
            }
            // Integer
            else if (value is int)
            {
                stringValue = IntToString((int)value);
            }
            // Double
            else if (value is double)
            {
                stringValue = DoubleToString((double)value);
            }
            // Decimal
            else if (value is decimal)
            {
                stringValue = DecimalToString((decimal)value);
            }
            // Bool
            else if (value is bool)
            {
                stringValue = ((bool)value ? "true" : "false");
            }
            else
            {
                stringValue = value.ToString();
            }

            return stringValue;
        }


        /// <summary>
        /// Replaces the number expressions within the given condition
        /// </summary>
        /// <param name="condition">Condition to process</param>
        public virtual string ReplaceNumbers(string condition)
        {
            if (String.IsNullOrEmpty(condition))
            {
                return condition;
            }

            // Process int values in search query  (int)123
            condition = IntReplacerRegex.Replace(condition, IntMatch);

            // Process double values in search query (double)123.456
            condition = DoubleReplacerRegex.Replace(condition, DoubleMatch);

            // Process decimal values in search query (decimal)123.456
            condition = DecimalReplacerRegex.Replace(condition, DecimalMatch);

            return condition;
        }


        /// <summary>
        /// Converts regular integer value to search-friendly integer value.
        /// </summary>
        /// <param name="m">Regex match</param>
        private string IntMatch(Match m)
        {
            return IntToString(ValidationHelper.GetInteger(m.Groups[1].Value, 0, Culture));
        }


        /// <summary>
        /// Converts regular double value to search-friendly double value.
        /// </summary>
        /// <param name="m">Regex match</param>
        private string DoubleMatch(Match m)
        {
            return DoubleToString(ValidationHelper.GetDouble(m.Groups[1].Value, 0, Culture));
        }


        /// <summary>
        /// Converts regular decimal value to search-friendly double value.
        /// </summary>
        /// <param name="m">Regex match</param>
        private string DecimalMatch(Match m)
        {
            return DecimalToString(ValidationHelper.GetDecimal(m.Groups[1].Value, 0m, Culture));
        }


        /// <summary>
        /// Returns string representation of the integer value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Integer value</param>
        public abstract string IntToString(int value);


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of integer</param>
        public abstract int StringToInt(string value);


        /// <summary>
        /// Returns string representation of the double value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Double value</param>
        public abstract string DoubleToString(double value);


        /// <summary>
        /// Returns string representation of the decimal value.
        /// </summary>
        /// <param name="value">Decimal value</param>
        public abstract string DecimalToString(decimal value);


        /// <summary>
        /// Converts search string to the decimal value.
        /// </summary>
        /// <param name="value">String representation of decimal</param>
        public abstract decimal StringToDecimal(string value);


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of double</param>
        public abstract double StringToDouble(string value);


        /// <summary>
        /// Converts the string representation of the date time from searched document to a date time
        /// </summary>
        /// <param name="input">Input string</param>
        public abstract DateTime StringToDate(string input);


        /// <summary>
        /// Converts the date time from to its string representation
        /// </summary>
        /// <param name="input">Input date</param>
        public abstract string DateToString(DateTime input);

        #endregion

    }
}
