using System;
using System.Collections.Generic;
using System.Globalization;

using CMS.Helpers;

namespace CMS.DataEngine.Serialization
{
    using ConversionFunction = Func<string, object, object>;

    /// <summary>
    /// Provides method for converting character string to requested type.
    /// </summary>
    internal static class StringConverter
    {

        #region "Constant"

        // Template text for error message
        private const string ERROR_MESSAGE_TEMPLATE = "Conversion '{0}' to type '{1}' failed.";

        // Conversion functions for system types that are sensitive for culture settings
        private static readonly Dictionary<Type, ConversionFunction> conversionFunctions = new Dictionary<Type, ConversionFunction>
        {
            { typeof(int), GetInteger },
            { typeof(long), GetLong },
            { typeof(double), GetDouble },
            { typeof(float), GetFloat },
            { typeof(decimal), GetDecimal },
            { typeof(DateTime), GetDateTime },
            { typeof(bool), GetBoolean },
            { typeof(Guid), GetGuid }
        };

        #endregion


        #region "Private methods"

        /// <summary>
        /// Delegate for generic TryParse method.
        /// </summary>
        private delegate bool TryParse<T>(string str, NumberStyles numberStyles, IFormatProvider formatProvider, out T value);


        /// <summary>
        /// Converts given string representation to a number.
        /// </summary>
        /// <param name="value">String representation of a number.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails.</param>
        /// <param name="numberStyles">Determines the styles ("characters") permitted in numeric string (i.e. whitespace, sign, decimal point etc.).</param>
        /// <param name="parseFunc">Function for converting string to number.</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetNumber<T>(string value, object defaultValue, NumberStyles numberStyles, TryParse<T> parseFunc)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            T val;
            if (!parseFunc(value, numberStyles, CultureHelper.EnglishCulture, out val))
            {
                throw new InvalidCastException(String.Format(ERROR_MESSAGE_TEMPLATE, value, typeof(T).Name));
            }

            return val;
        }


        /// <summary>
        /// Converts given string value to <see cref="int"/>.
        /// </summary>
        /// <param name="value">String representation of an <see cref="int"/>.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails.</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetInteger(string value, object defaultValue)
        {
            return GetNumber<int>(value, defaultValue, NumberStyles.Integer, int.TryParse);
        }


        /// <summary>
        /// Converts given string value to <see cref="long"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="long"/>.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails.</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetLong(string value, object defaultValue)
        {
            return GetNumber<long>(value, defaultValue, NumberStyles.Integer, long.TryParse);
        }


        /// <summary>
        /// Converts given string value to <see cref="double"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="double"/>.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetDouble(string value, object defaultValue)
        {
            return GetNumber<double>(value, defaultValue, NumberStyles.Float, double.TryParse);
        }


        /// <summary>
        /// Converts given string value to <see cref="float"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="float"/>.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetFloat(string value, object defaultValue)
        {
            return GetNumber<float>(value, defaultValue, NumberStyles.Float, float.TryParse);
        }


        /// <summary>
        /// Converts given string value to <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="decimal"/>.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetDecimal(string value, object defaultValue)
        {
            return GetNumber<decimal>(value, defaultValue, NumberStyles.Float, decimal.TryParse);
        }


        /// <summary>
        /// Converts given string value to <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="DateTime"/> value.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        /// <remarks>Conversion takes into account machine's timezone.</remarks>
        private static object GetDateTime(string value, object defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            DateTime val;            
            if (!DateTime.TryParse(value, CultureHelper.EnglishCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out val))
            {
                throw new InvalidCastException(String.Format(ERROR_MESSAGE_TEMPLATE, value, "DateTime"));
            }

            // Parsed DateTime converted to machine-specific timezone
            return val;
        }


        /// <summary>
        /// Converts given string value to <see cref="bool"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="bool"/> value.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetBoolean(string value, object defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            value = value.Trim();
            if (value == "0")
            {
                return false;
            }
            if (value == "1")
            {
                return true;
            }

            bool val;
            if (!bool.TryParse(value, out val))
            {
                throw new InvalidCastException(String.Format(ERROR_MESSAGE_TEMPLATE, value, "Boolean"));
            }

            return val;
        }


        /// <summary>
        /// Converts given string value to <see cref="Guid"/>.
        /// </summary>
        /// <param name="value">String representation of a <see cref="Guid"/> value.</param>
        /// <param name="defaultValue">Default value to return if the conversion fails</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        private static object GetGuid(string value, object defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            Guid val;
            if (!Guid.TryParse(value, out val))
            {
                throw new InvalidCastException(String.Format(ERROR_MESSAGE_TEMPLATE, value, "Guid"));
            }

            return val;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Converts given string value to type specified by <see cref="DataType"/>.
        /// </summary>
        /// <param name="value">String representation of a value to conversion.</param>
        /// <param name="dataType">Describes the type of returned object.</param>
        /// <exception cref="InvalidCastException">Thrown when conversion fails.</exception>
        public static object GetValue(string value, DataType dataType)
        {
            ConversionFunction conversionFunction;            
            if (conversionFunctions.TryGetValue(dataType.Type, out conversionFunction))
            {
                return conversionFunction(value, dataType.ObjectDefaultValue);
            }

            return dataType.Convert(value, CultureHelper.EnglishCulture, dataType.ObjectDefaultValue);
        }

        #endregion
    }
}
