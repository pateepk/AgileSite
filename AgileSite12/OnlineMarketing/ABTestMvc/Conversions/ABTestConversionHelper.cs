using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Helper class for working with A/B test conversions.
    /// </summary>
    internal static class ABTestConversionHelper
    {
        /// <summary>
        /// Separator ('|') is used to join base conversion name from its definition and related item identifier (if specified).
        /// </summary>
        public const string CONVERSION_FULLNAME_SEPARATOR = "|";


        /// <summary>
        /// Create full name of <paramref name="conversionName"/> using given <paramref name="relatedItemIdentifier"/>.
        /// </summary>
        /// <param name="conversionName">Name of conversion which full name to return.</param>
        /// <param name="relatedItemIdentifier">Identifier of a item related to conversion.</param>
        /// <returns>Full name of conversion.</returns>
        public static string GetConversionFullName(string conversionName, string relatedItemIdentifier)
        {
            return String.IsNullOrEmpty(relatedItemIdentifier)
                ? conversionName
                : conversionName + CONVERSION_FULLNAME_SEPARATOR + relatedItemIdentifier;
        }


        /// <summary>
        /// Returns base conversion name if given name <paramref name="conversionFullName"/> contains related item identifier.
        /// </summary>
        /// <param name="conversionFullName">Conversion name which may contain related item identifier</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="conversionFullName"/> is null.</exception>
        public static string GetConversionOriginalName(string conversionFullName)
        {
            if (conversionFullName == null)
            {
                throw new ArgumentNullException(nameof(conversionFullName));
            }

            return conversionFullName.Split(ABTestConversionHelper.CONVERSION_FULLNAME_SEPARATOR[0])[0];
        }
    }
}
