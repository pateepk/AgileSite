using System;
using System.Globalization;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides support for formatting and parsing <see cref="DateTime"/> values for usages in url links.
    /// </summary>
    /// <see cref="SecurityHelper"/>
    public static class DateTimeUrlFormatter
    {
        /// <summary>
        /// Date time format used in e-mail confirmation hashing methods.
        /// </summary>
        private const string EMAIL_CONFIRMATION_DATETIME_FORMAT = "ddMMyyyyHHmmss";


        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;


        /// <summary>
        /// Returns given date time in custom culture independent format.
        /// </summary>
        /// <seealso cref="SecurityHelper.GenerateConfirmationEmailHash"/>
        /// <seealso cref="SecurityHelper.ValidateConfirmationEmailHash"/>
        public static string Format(DateTime dateTime)
        {
            if (dateTime == null)
            {
                throw new ArgumentNullException(nameof(dateTime));
            }

            return dateTime.ToString(EMAIL_CONFIRMATION_DATETIME_FORMAT, InvariantCulture);
        }


        /// <summary>
        /// Returns date time parsed from custom culture independent format.
        /// </summary>
        /// <exception cref="FormatException"></exception>
        public static DateTime Parse(string formattedDateTime)
        {
            return DateTime.ParseExact(formattedDateTime, EMAIL_CONFIRMATION_DATETIME_FORMAT, InvariantCulture);
        }


        /// <summary>
        /// Attempts to parse date time in custom culture independent format.
        /// </summary>
        /// <returns>
        /// True when parsing succeeded, false otherwise.
        /// </returns>
        public static bool TryParse(string formattedDateTime, out DateTime dateTime)
        {
            return DateTime.TryParseExact(formattedDateTime, EMAIL_CONFIRMATION_DATETIME_FORMAT, InvariantCulture, DateTimeStyles.None, out dateTime);
        }
    }
}
