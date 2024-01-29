using System;

namespace CMS.Core
{
    /// <summary>
    /// Interface for the conversion service
    /// </summary>
    public interface IConversionService
    {
        /// <summary>
        /// Returns the integer representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        int GetInteger(object value, int defaultValue);


        /// <summary>
        /// Returns the boolean representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        bool GetBoolean(object value, bool defaultValue);


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        double GetDouble(object value, double defaultValue, string culture);


        /// <summary>
        /// Returns the Guid representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        Guid GetGuid(object value, Guid defaultValue);


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        string GetString(object value, string defaultValue);


        /// <summary>
        /// Gets the identifier created from the given name
        /// </summary>
        /// <param name="name">Display name</param>
        string GetIdentifier(string name);


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        DateTime GetDateTime(object value, DateTime defaultValue, string culture);


        /// <summary>
        /// Converts the value to specified type. If the value is null, default value is used.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        T GetValue<T>(object value, T defaultValue);


        /// <summary>
        /// Gets the code name created from the given string.
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="replacement">Replacement string for invalid characters</param>
        string GetCodeName(object name, string replacement = null);
    }
}
