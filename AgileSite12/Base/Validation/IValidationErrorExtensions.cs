using System;
using System.Globalization;
using System.Linq;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Provides a set of extension methods for <see cref="IValidationError"/>.
    /// </summary>
    public static class IValidationErrorExtensions
    {
        /// <summary>
        /// Returns a localized message with substituted parameters for <paramref name="validationError"/>.
        /// </summary>
        /// <param name="validationError">Validation error whose localized message to retrieve.</param>
        /// <param name="culture">Culture to use for localization.</param>
        /// <returns>Returns a localized message for validation error.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationError"/> is null.</exception>
        /// <seealso cref="ILocalizationService"/>
        /// <remarks>
        /// The default culture is used for messages which are missing localization in specified <paramref name="culture"/>.
        /// </remarks>
        public static string GetMessage(this IValidationError validationError, string culture = null)
        {
            if (validationError == null)
            {
                throw new ArgumentNullException(nameof(validationError));
            }

            var formatProvider = String.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture);

            var localizationService = Service.Resolve<ILocalizationService>();

            var message = String.Format(formatProvider, localizationService.GetString(validationError.MessageKey, culture), validationError.MessageParameters);

            return localizationService.LocalizeString(message, culture);
        }
    }
}
