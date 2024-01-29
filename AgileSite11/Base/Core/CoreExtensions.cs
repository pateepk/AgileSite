using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Base extensions
    /// </summary>
    /// <exclude/>
    public static class CoreExtensions
    {
        /// <summary>
        /// Returns the integer representation of an object or default value if not.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="defaultValue">Default value</param>
        public static int ToInteger(this object obj, int defaultValue)
        {
            return CoreServices.Conversion.GetInteger(obj, defaultValue);
        }


        /// <summary>
        /// Returns the boolean representation of an object or default value if not.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="defaultValue">Default value</param>
        public static bool ToBoolean(this object obj, bool defaultValue)
        {
            return CoreServices.Conversion.GetBoolean(obj, defaultValue);
        }


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public static double ToDouble(this object obj, double defaultValue, string culture)
        {
            return CoreServices.Conversion.GetDouble(obj, defaultValue, culture);
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public static DateTime ToDateTime(this object obj, DateTime defaultValue, string culture)
        {
            return CoreServices.Conversion.GetDateTime(obj, defaultValue, culture);
        }


        /// <summary>
        /// Returns the Guid representation of an object or default value if not.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="defaultValue">Default value</param>
        public static Guid ToGuid(this object obj, Guid defaultValue)
        {
            return CoreServices.Conversion.GetGuid(obj, defaultValue);
        }


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="defaultValue">Default value</param>
        public static string ToString(this object obj, string defaultValue)
        {
            return CoreServices.Conversion.GetString(obj, defaultValue);
        }


        /// <summary>
        /// Removes unnecessary zero values at the end of given decimal number.
        /// </summary>
        /// <param name="value">Decimal number.</param>
        public static decimal TrimEnd(this decimal value)
        {
            // Number of zeroes = 28, the maximum precision of decimal type in .NET
            return value / 1.0000000000000000000000000000m;
        }


        /// <summary>
        /// Gets the static property of the given type
        /// </summary>
        /// <param name="type">Parent type</param>
        /// <param name="propertyName">Property name</param>
        public static GenericProperty<PropertyType> StaticProperty<PropertyType>(this Type type, string propertyName)
        {
            var prop = Extension<PropertyType>.GetStaticPropertyForType(type, propertyName);

            return prop;
        }


        /// <summary>
        /// Gets the static property of the given type
        /// </summary>
        /// <param name="type">Parent type</param>
        public static SafeDictionary<string, GenericProperty<PropertyType>> GetStaticProperties<PropertyType>(this Type type)
        {
            var props = Extension<PropertyType>.GetStaticPropertiesForType(type);

            return props;
        }
    }
}
