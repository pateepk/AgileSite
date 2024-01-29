using System;
using System.Globalization;

using CMS.Helpers;
using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Provides culture-sensitive formatting for attribute values of Facebook API entities.
    /// </summary>
    public sealed class EntityAttributeValueFormatter : IEntityAttributeValueFormatter
    {
        #region "Public methods"

        /// <summary>
        /// Formats a Boolean value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        public string Format(bool value)
        {
            return ResHelper.GetString(value ? "general.yes" : "general.no", CultureInfo.InvariantCulture.Name);
        }


        /// <summary>
        /// Formats an Int64 value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        public string Format(long value)
        {
            return value.ToString("N", CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Formats a Decimal value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        public string Format(decimal value)
        {
            return value.ToString("N", CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Formats a DateTime value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        public string Format(DateTime value)
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }

        
        /// <summary>
        /// Formats a UserGenderEnum value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        public string Format(UserGenderEnum value)
        {
            switch (value)
            {
                case UserGenderEnum.Male: return ResHelper.GetString("general.male", CultureInfo.InvariantCulture.Name);
                case UserGenderEnum.Female: return ResHelper.GetString("general.female", CultureInfo.InvariantCulture.Name);
            }

            return ResHelper.GetString("general.unknown", CultureInfo.InvariantCulture.Name);
        }

        #endregion
    }

}