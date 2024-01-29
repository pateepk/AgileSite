using System;
using System.Linq;

using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides extension methods for working with the <c>Enum</c> types.
    /// </summary>
    public static class EnumStringRepresentationExtensions
    {
        /// <summary>
        /// Gets the order of the enum value.
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>
        /// Returns the order of enum item if it is specified using the <see cref="EnumOrderAttribute"/>.
        /// Otherwise returns the int value of the enum value if the enum is based on an int type.
        /// Otherwise returns 0.
        /// </returns>
        public static int GetOrder(this Enum value)
        {
            // Check that the specified type is an enum type
            var enumType = value.GetType();
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified value is not of an enumeration type.", "value");
            }

            var fieldName = Enum.GetName(enumType, value);
            var field = enumType.GetField(fieldName);

            // Try to get the order
            var attribute = (EnumOrderAttribute)field.GetCustomAttributes(typeof(EnumOrderAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                // Return the order
                return attribute.Order;
            }

            // Enum based on integer
            if (enumType.BaseType == typeof(int))
            {
                // Return the int value fo the enum value
                var intValue = Convert.ToInt32(value);
                return intValue;
            }

            return 0;
        }


        #region "String representation"

        /// <summary>
        /// Converts the enum value to it's string representation.
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="value">Enum value</param>
        /// <returns>
        /// Returns the string representation of the enum value if it is specified using the <see cref="EnumStringRepresentationAttribute"/>.
        /// Otherwise returns the enum field name.
        /// </returns>
        public static string ToStringRepresentation<TEnum>(this TEnum value) where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            // Check that the specified type is an enum type
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enumeration type.", "value");
            }

            // Convert
            return (value as Enum).ToStringRepresentation();
        }


        /// <summary>
        /// Converts the enum value to it's string representation.
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>
        /// Returns the string representation of the enum value if it is specified using the <see cref="EnumStringRepresentationAttribute"/>.
        /// Otherwise returns the enum field name.
        /// </returns>
        public static string ToStringRepresentation(this Enum value)
        {
            // Check that the specified type is an enum type
            var enumType = value.GetType();
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified value is not of an enumeration type.", "value");
            }

            var fieldName = Enum.GetName(enumType, value);
            var field = enumType.GetField(fieldName);

            // Try to get the string representation
            var attribute = (EnumStringRepresentationAttribute)field.GetCustomAttributes(typeof(EnumStringRepresentationAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                // Return the string representation
                return attribute.StringRepresentation;
            }

            // Return the enum field name
            return fieldName;
        }


        /// <summary>
        /// Converts the enum value to it's localized string representation.
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <param name="resourcePrefix">Resource prefix</param>
        public static string ToLocalizedString<TEnum>(this TEnum value, string resourcePrefix) 
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            // Check that the specified type is an enum type
            var enumType = value.GetType();
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified value is not of an enumeration type.", "value");
            }

            // Get the string representation
            var stringRepresentation = value.ToStringRepresentation();

            resourcePrefix = resourcePrefix ?? typeof (TEnum).Name;

            // Return the localized string representation
            var localized = CoreServices.Localization.GetString(resourcePrefix + "." + stringRepresentation);
            return localized;
        }


        /// <summary>
        /// Converts the string representation of the enum value to the actual enum value.
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="stringRepresentation">String representation of the enum value</param>
        /// <returns>
        /// Returns the enum value if it is specified using the <see cref="EnumStringRepresentationAttribute"/>.
        /// Otherwise returns the default enum value.
        /// </returns>
        public static TEnum ToEnum<TEnum>(this string stringRepresentation) 
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            // Check that the specified type is an enum type
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enumeration type.", "TEnum");
            }

            TEnum result;
            if (!Enum.TryParse(stringRepresentation, out result))
            {
                // Convert
                return (TEnum)(object)stringRepresentation.ToEnum(enumType);
            }

            return result;
        }


        /// <summary>
        /// Converts the string representation of the enum value to the actual enum value.
        /// </summary>
        /// <param name="stringRepresentation">String representation of the enum value</param>
        /// <param name="enumType">Enum type</param>
        /// <returns>
        /// Returns the enum value if it is specified using the <see cref="EnumStringRepresentationAttribute"/>.
        /// Otherwise returns the default enum value.
        /// </returns>
        private static Enum ToEnum(this string stringRepresentation, Type enumType)
        {
            // Check that the specified type is an enum type
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enumeration type.", "enumType");
            }

            // Try to get the corresponding enum value
            var field = enumType.GetFields().FirstOrDefault(f => f.GetCustomAttributes(typeof(EnumStringRepresentationAttribute), false).Select(a => (EnumStringRepresentationAttribute)a).Any(a => a.StringRepresentation == stringRepresentation));
            if (field != null)
            {
                // Return the corresponding enum value
                return (Enum)field.GetValue(null);
            }

            // Return the default enum value
            var defaultValue = EnumHelper.GetDefaultValue(enumType);
            return defaultValue;
        }

        #endregion
    }
}
