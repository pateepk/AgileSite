namespace CMS.Search
{
    
    /// <summary>
    /// Base class for search value converter
    /// </summary>
    public interface ISearchValueConverter
    {
        #region "Methods"

        /// <summary>
        /// Converts the 
        /// </summary>
        /// <param name="value">Value to convert</param>
        string ConvertToString(object value);


        /// <summary>
        /// Replaces the number expressions within the given condition
        /// </summary>
        /// <param name="condition">Condition to process</param>
        string ReplaceNumbers(string condition);


        /// <summary>
        /// Returns string representation of the integer value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Integer value</param>
        string IntToString(int value);


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of integer</param>
        int StringToInt(string value);


        /// <summary>
        /// Returns string representation of the double value. Returned value has always uniform format.
        /// </summary>
        /// <param name="value">Double value</param>
        string DoubleToString(double value);


        /// <summary>
        /// Converts search string to the integer value.
        /// </summary>
        /// <param name="value">String representation of double</param>
        double StringToDouble(string value);


        /// <summary>
        /// Returns string representation of the decimal value.
        /// </summary>
        /// <param name="value">Decimal value</param>
        string DecimalToString(decimal value);


        /// <summary>
        /// Converts search string to the decimal value.
        /// </summary>
        /// <param name="value">String representation of decimal</param>
        decimal StringToDecimal(string value);


        /// <summary>
        /// Converts the string representation of the date time from searched document to a date time
        /// </summary>
        /// <param name="input">Input string</param>
        System.DateTime StringToDate(string input);


        /// <summary>
        /// Converts the date time from to its string representation
        /// </summary>
        /// <param name="input">Input date</param>
        string DateToString(System.DateTime input);

        #endregion
    }
}
