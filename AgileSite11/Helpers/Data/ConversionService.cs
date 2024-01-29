using System;

using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Service which converts object to specific types
    /// </summary>
    public class ConversionService : IConversionService
    {
        /// <summary>
        /// Returns the integer representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public int GetInteger(object value, int defaultValue)
        {
            return ValidationHelper.GetInteger(value, defaultValue);
        }


        /// <summary>
        /// Returns the boolean representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public bool GetBoolean(object value, bool defaultValue)
        {
            return ValidationHelper.GetBoolean(value, defaultValue);
        }


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public double GetDouble(object value, double defaultValue, string culture)
        {
            return ValidationHelper.GetDouble(value, defaultValue, culture);
        }


        /// <summary>
        /// Returns the decimal representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public decimal GetDecimal(object value, decimal defaultValue, string culture)
        {
            return ValidationHelper.GetDecimal(value, defaultValue, culture);
        }


        /// <summary>
        /// Returns the guid representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public Guid GetGuid(object value, Guid defaultValue)
        {
            return ValidationHelper.GetGuid(value, defaultValue);
        }


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public string GetString(object value, string defaultValue)
        {
            return ValidationHelper.GetString(value, defaultValue);
        }


        /// <summary>
        /// Gets the identifier created from the given name
        /// </summary>
        /// <param name="name">Display name</param>
        public string GetIdentifier(string name)
        {
            return ValidationHelper.GetIdentifier(name);
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public DateTime GetDateTime(object value, DateTime defaultValue, string culture)
        {
            return ValidationHelper.GetDateTime(value, defaultValue, culture);
        }


        /// <summary>
        /// Converts the value to specified type. If the value is null, default value is used.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public T GetValue<T>(object value, T defaultValue)
        {
            return ValidationHelper.GetValue(value, defaultValue);
        }


        /// <summary>
        /// Gets the code name created from the given string.
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="replacement">Replacement string for invalid characters</param>
        public string GetCodeName(object name, string replacement = null)
        {
            return ValidationHelper.GetCodeName(name, replacement);
        }
    }
}
