using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS;
using CMS.Base;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.Search;
using CMS.Search.Lucene3;

using Lucene.Net.Documents;

[assembly: RegisterImplementation(typeof(ISearchValueConverter), typeof(LuceneSearchValueConverter), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Helper methods for search condition syntax
    /// </summary>
    public class LuceneSearchValueConverter : AbstractSearchValueConverter
    {
        #region "Variables"

        /// <summary>
        /// Maximal value of double value.
        /// </summary>
        private const double SEARCH_DOUBLE_MAX = 9999999999.9999999999;

        private Regex mIntReplacerRegex;
        private Regex mDoubleReplacerRegex;
        private Regex mDecimalReplacerRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the integer replacer regular expression. Replaces integer values in following format (int)123.
        /// </summary>
        protected override Regex IntReplacerRegex
        {
            get
            {
                return mIntReplacerRegex ?? (mIntReplacerRegex = RegexHelper.GetRegex("\\(int\\)([-]*\\d+)"));
            }
        }


        /// <summary>
        /// Gets the double replacer regular expression. Replaces double values in following format (double)123.456.
        /// </summary>
        protected override Regex DoubleReplacerRegex
        {
            get
            {
                return mDoubleReplacerRegex ?? (mDoubleReplacerRegex = RegexHelper.GetRegex("\\(double\\)([-]*\\d+[.]*\\d*([eE][+-]?\\d+)?)"));
            }
        }


        /// <summary>
        /// Gets the double replacer regular expression.
        /// </summary>
        protected override Regex DecimalReplacerRegex
        {
            get
            {
                return mDecimalReplacerRegex ?? (mDecimalReplacerRegex = RegexHelper.GetRegex("\\(decimal\\)([-]*\\d+[.]*\\d*)"));
            }
        }

        #endregion
        

        #region "Methods"

        /// <summary>
        /// Returns string representation of the integer value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Integer value</param>
        public override string IntToString(int value)
        {
            // Set negative flag
            bool isNegative = (value < 0);

            // If current value is negative convert it to the positive area
            if (isNegative)
            {
                value = value + Int32.MaxValue;
            }

            // Format integer value
            string result = String.Format(Culture, "{0:0000000000}", value);

            // Add sign value
            if (isNegative)
            {
                result = "0" + result;
            }
            else
            {
                result = "1" + result;
            }

            return result;
        }


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of integer</param>
        public override int StringToInt(string value)
        {
            int result = 0;

            // Check whether input is defined
            if (String.IsNullOrEmpty(value))
            {
                return result;
            }

            // Get sign flag
            bool isNegative = value.StartsWithCSafe("0");

            // Remove sign value
            value = value.Substring(1);

            // Try parse
            if (Int32.TryParse(value, NumberStyles.Any, Culture, out result))
            {
                // Ensure negative numbers
                if (isNegative)
                {
                    result = result - Int32.MaxValue;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns string representation of the double value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Double value</param>
        public override string DoubleToString(double value)
        {
            // Set negative flag
            bool isNegative = (value < 0.0);

            // If current value is negative convert it to the positive area
            if (isNegative)
            {
                value = SEARCH_DOUBLE_MAX + value;
            }

            // Format double value
            string result = String.Format(Culture, "{0:0000000000.0000000000}", value);

            // Add sign value
            if (isNegative)
            {
                result = "0" + result;
            }
            else
            {
                result = "1" + result;
            }

            return result;
        }


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of double</param>
        public override double StringToDouble(string value)
        {
            double result = 0;

            // Check whether input is defined
            if (String.IsNullOrEmpty(value))
            {
                return result;
            }

            // Get sign flag
            bool isNegative = value.StartsWithCSafe("0");

            // Remove sign value
            value = value.Substring(1);

            // Try parse
            if (Double.TryParse(value, NumberStyles.Any, Culture, out result))
            {
                // Ensure negative numbers
                if (isNegative)
                {
                    result = result - SEARCH_DOUBLE_MAX;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns string representation of the decimal value.
        /// </summary>
        /// <param name="value">Decimal value</param>
        public override string DecimalToString(decimal value)
        {
            var isNegative = value < 0;

            // Convert to positive 
            if (isNegative)
            {
                value = -value;
            }

            // Format string
            string number = value.ToString(Culture);

            // Trim zeros or append '.' (123.25600 -> 123.256, 123 -> 123.)
            number = number.Contains(".") ? number.TrimEnd('0') : number + '.';

            // Create builder for whole string representation with sign placeholder, add capacity for sign, scale prefix and 'X' suffix
            var builder = new StringBuilder("S", number.Length + 4);
            
            // Append scale prefix - number of digits before decimal point (123.123 -> 03123.123)
            builder.Append(number.IndexOf('.').ToString("00"));

            // Append number
            builder.Append(number);

            // Ensure correct string comparison of negative numbers
            if (isNegative)
            {
                EnsureNegativeDecimal(builder);

                // Append char with bigger value than digits to ensure correct comparison of digits after decimal point
                builder.Append('X');
            }

            // Set sign value
            builder[0] = isNegative ? '0' : '1';

            return builder.ToString();
        }


        /// <summary>
        /// Converts search string to the decimal value.
        /// </summary>
        /// <param name="value">String representation of decimal</param>
        public override decimal StringToDecimal(string value)
        {
            decimal result = 0m;

            // Check whether input is defined
            if (String.IsNullOrEmpty(value))
            {
                return result;
            }

            // Get sign flag
            bool isNegative = value.StartsWithCSafe("0");

            // Remove sign value and scale prefix
            value = value.Substring(3);

            // Ensure negative numbers
            if (isNegative)
            {
                var b = new StringBuilder(value);
                EnsureNegativeDecimal(b);
                value = b.ToString().TrimEnd('X');
            }

            // Try parse
            if (Decimal.TryParse(value, NumberStyles.Any, Culture, out result))
            {
                // Ensure negative numbers
                if (isNegative)
                {
                    result = -result;
                }
            }

            return result;
        }


        /// <summary>
        /// Converts the string representation of the date time from searched document to a date time
        /// </summary>
        /// <param name="input">Input string</param>
        public override DateTime StringToDate(string input)
        {
            return DateTools.StringToDate(input);
        }


        /// <summary>
        /// Converts the date time from to its string representation
        /// </summary>
        /// <param name="input">Input date</param>
        public override string DateToString(DateTime input)
        {
            return DateTools.DateToString(input, DateTools.Resolution.SECOND);
        }
        
        #endregion


        #region "Helper methods"

        /// <summary>
        /// Converts string representation of negative decimal number to comparable format. (123.4 -> 876.5)
        /// </summary>
        /// <param name="builder">String builder with representation of decimal (without sign value)</param>
        private void EnsureNegativeDecimal(StringBuilder builder)
        {
            for (int i = 0; i < builder.Length; i++)
            {
                char c = builder[i];

                if ((c >= '0') && (c <= '9'))
                {
                    builder[i] = (char)('0' + ('9' - c));
                }
            }
        }

        #endregion

    }
}
