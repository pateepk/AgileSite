using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides helper methods for working with the <c>Enum</c> types.
    /// </summary>
    public static class EnumHelper
    {
        #region "Default value"

        /// <summary>
        /// Gets the default value fo the specified enum type.
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <returns>
        /// Returns the default enum value, if it is specified using the <see cref="EnumDefaultValueAttribute"/>.
        /// Otherwise returns the first declared enum value.
        /// </returns>
        public static TEnum GetDefaultValue<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            // Check that the specified type is an enum type
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enumeration type.", "TEnum");
            }

            // Get the default value
            return (TEnum)(object)GetDefaultValue(enumType);
        }


        /// <summary>
        /// Gets the default value fo the specified enum type.
        /// </summary>
        /// <param name="enumType">Enum type</param>
        /// <returns>
        /// Returns the default enum value, if it is specified using the <see cref="EnumDefaultValueAttribute"/>.
        /// Otherwise returns the first declared enum value.
        /// </returns>
        public static Enum GetDefaultValue(Type enumType)
        {
            // Check that the specified type is an enum type
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enumeration type.", "enumType");
            }

            // Try to get the default value
            var field = enumType.GetFields().FirstOrDefault(f => f.GetCustomAttributes(typeof(EnumDefaultValueAttribute), false).Any());
            if (field != null)
            {
                // Return the default value
                return (Enum)field.GetValue(null);
            }

            // Return the first declared value
            var firstDeclaredValue = (Enum)Enum.GetValues(enumType).GetValue(0);
            return firstDeclaredValue;
        }

        #endregion


        #region "Enum Category"

        /// <summary>
        /// Gets enums with category attribute, which have given category name.
        /// </summary>
        /// <param name="enumType">Enum type</param>
        /// <param name="categories">Categories</param>
        /// <returns>Return enums with category attribute, with given category name.</returns>
        public static IEnumerable<Enum> GetEnumsByCategories(Type enumType, List<string> categories)
        {
            // Check that the specified type is an enum type
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The specified type is not an enumeration type.", "enumType");
            }

            // Return enums with category attribute, with given category name
            var fields = enumType.GetFields()
                .Where(f => f.GetCustomAttributes(typeof(EnumCategoryAttribute), false)
                    .OfType<EnumCategoryAttribute>()
                    .Any(cat => categories.Contains(cat.Category)))
                .Select(enu => (Enum)enu.GetValue(null));

            return fields;
        }

        #endregion
    }
}
