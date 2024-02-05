using System;
using System.Collections.Generic;
using System.Globalization;

namespace CMS.Base
{
    /// <summary>
    /// Helper (replace) methods for string class
    /// </summary>
    public class CMSString
    {
        private static readonly HashSet<string> invariantCultureCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "tr", "az" };


        /// <summary>
        /// Compares two strings. Returns an integer that indicates the relationship of the two strings to each other in the sort order.
        /// </summary>
        /// <param name="strA">The first string to compare</param>
        /// <param name="strB">The second string to compare</param>
        /// <param name="ignoreCase">If true, compare is case insensitive</param>
        public static int Compare(String strA, String strB, bool ignoreCase = false)
        {
            if (IsInvariantCompareCulture())
            {
                // Return invariant culture compare 
                StringComparison comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return String.Compare(strA, strB, comp);
            }

#pragma warning disable BH4005 // String comparison methods should not be used without specifying StringComparison.
            return String.Compare(strA, strB, ignoreCase);
#pragma warning restore BH4005 // String comparison methods should not be used without specifying StringComparison.
        }


        /// <summary>
        /// Compares two strings. Returns an integer that indicates the relationship of the two strings to each other in the sort order.
        /// </summary>
        /// <param name="strA">The first string to compare</param>
        /// <param name="strB">The second string to compare</param>
        /// <param name="stringComparison">One of the enumeration values that specifies the rules to use in the comparison</param>
        public static int Compare(String strA, String strB, StringComparison stringComparison)
        {
            return String.Compare(strA, strB, stringComparison);
        }


        /// <summary>
        /// Determines whether two specified String objects have the same value
        /// </summary>
        /// <param name="strA">The first string to compare, or Nothing</param>
        /// <param name="strB">The second string to compare, or Nothing</param>
        public static bool Equals(String strA, String strB)
        {
            return Equals(strA, strB, false);
        }


        /// <summary>
        /// Determines whether two specified String objects have the same value
        /// </summary>
        /// <param name="strA">The first string to compare, or Nothing</param>
        /// <param name="strB">The second string to compare, or Nothing</param>
        /// <param name="ignoreCase">If true, compare is case insensitive</param>
        public static bool Equals(String strA, String strB, bool ignoreCase)
        {
            if (IsInvariantCompareCulture())
            {
                // Return invariant culture compare 
                StringComparison comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return String.Equals(strA, strB, comp);
            }

            StringComparison comparison = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            return String.Equals(strA, strB, comparison);
        }


        /// <summary>
        /// Returns true if <see cref="CultureInfo.InvariantCulture"/> should be used instead of 
        /// <see cref="CultureInfo.CurrentCulture"/> to perform case insensitive operations.
        /// </summary>
        public static bool IsInvariantCompareCulture()
        {
            return IsInvariantCompareCulture(CultureInfo.CurrentCulture);
        }


        /// <summary>
        /// Returns true if <see cref="CultureInfo.InvariantCulture"/> should be used instead of passed culture to perform case insensitive operations.
        /// </summary>
        public static bool IsInvariantCompareCulture(CultureInfo culture)
        {
            return invariantCultureCodes.Contains(culture.TwoLetterISOLanguageName);
        }
    }
}