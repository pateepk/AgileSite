using System;

using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Provides formatting for attribute values of Facebook API entities.
    /// </summary>
    public interface IEntityAttributeValueFormatter
    {
        #region "Methods"

        /// <summary>
        /// Formats a Boolean value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        string Format(Boolean value);


        /// <summary>
        /// Formats an Int64 value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        string Format(Int64 value);


        /// <summary>
        /// Formats a Decimal value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        string Format(Decimal value);


        /// <summary>
        /// Formats a DateTime value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        string Format(DateTime value);

        
        /// <summary>
        /// Formats a UserGenderEnum value, and returns it.
        /// </summary>
        /// <param name="value">A value to format.</param>
        /// <returns>A string representation of the specified value.</returns>
        string Format(UserGenderEnum value);

        #endregion
    }

}