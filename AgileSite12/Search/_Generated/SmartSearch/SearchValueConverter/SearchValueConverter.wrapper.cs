namespace CMS.Search
{
    
    /// <summary>
    /// Base class for search value converter
    /// </summary>
    public class SearchValueConverter : CMS.Core.StaticWrapper<ISearchValueConverter>
    {
        #region "Methods"

        /// <summary>
        /// Converts the 
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static string ConvertToString(object value)
        {
            return Implementation.ConvertToString(value);
        }


        /// <summary>
        /// Replaces the number expressions within the given condition
        /// </summary>
        /// <param name="condition">Condition to process</param>
        public static string ReplaceNumbers(string condition)
        {
            return Implementation.ReplaceNumbers(condition);
        }


        /// <summary>
        /// Returns string representation of the integer value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Integer value</param>
        public static string IntToString(int value)
        {
            return Implementation.IntToString(value);
        }


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of integer</param>
        public static int StringToInt(string value)
        {
            return Implementation.StringToInt(value);
        }


        /// <summary>
        /// Returns string representation of the double value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Double value</param>
        public static string DoubleToString(double value)
        {
            return Implementation.DoubleToString(value);
        }


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of double</param>
        public static double StringToDouble(string value)
        {
            return Implementation.StringToDouble(value);
        }


        /// <summary>
        /// Returns string representation of the decimal value.
        /// </summary>
        /// <param name="value">Decimal value</param>
        public static string DecimalToString(decimal value)
        {
            return Implementation.DecimalToString(value);
        }


        /// <summary>
        /// Converts search string to the decimal value.
        /// </summary>
        /// <param name="value">String representation of decimal</param>
        public static decimal StringToDecimal(string value)
        {
            return Implementation.StringToDecimal(value);
        }


        /// <summary>
        /// Converts the string representation of the date time from searched document to a date time
        /// </summary>
        /// <param name="input">Input string</param>
        public static System.DateTime StringToDate(string input)
        {
            return Implementation.StringToDate(input);
        }


        /// <summary>
        /// Converts the date time from to its string representation
        /// </summary>
        /// <param name="input">Input date</param>
        public static string DateToString(System.DateTime input)
        {
            return Implementation.DateToString(input);
        }

        #endregion
    }
}
