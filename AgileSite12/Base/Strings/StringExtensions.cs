using System;
using System.Linq;

namespace CMS.Base
{
    /// <summary>
    /// Extensions for the <see cref="string"/> class
    /// </summary>
    public static class StringExtensions
    {
#pragma warning disable BH4000 // String manipulation methods should not be used without specifying CultureInfo.
#pragma warning disable BH4003 // String comparison methods should not be used without specifying StringComparison.
#pragma warning disable BH4004 // String comparison methods should not be used without specifying StringComparison.

        #region "StartsWith & EndsWith"

        /// <summary>
        /// Returns true if the string starts with any of the given strings
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="comparison">Comparison type</param>
        /// <param name="prefixes">Prefixes to check</param>
        public static bool StartsWithAny(this string value, StringComparison comparison, params string[] prefixes)
        {
            return prefixes.Any(prefix => value.StartsWith(prefix, comparison));
        }


        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">The string to compare</param>
        /// <param name="ignoreCase">If true comparison is case insensitive</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="str"/> is null.</exception>
        public static bool StartsWithCSafe(this string value, string str, bool ignoreCase)
        {
            StringComparison comp = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (CMSString.IsInvariantCompareCulture())
            {
                // Return invariant culture compare
                comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return value.StartsWith(str, comp);
            }

            return value.StartsWith(str, comp);
        }


        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">The string to compare</param>
        /// <param name="comparison">Comparison type</param>
        public static bool StartsWithCSafe(this string value, string str, StringComparison comparison)
        {
            return value.StartsWith(str, comparison);
        }


        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">The string to compare</param>
        public static bool StartsWithCSafe(this string value, string str)
        {
            return value.StartsWith(str);
        }


        /// <summary>
        /// Determines whether the ending of this string instance matches the specified string. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">The string to compare</param>
        /// <param name="ignoreCase">If true comparison is case insensitive</param>
        public static bool EndsWithCSafe(this string value, string str, bool ignoreCase)
        {
            StringComparison comp = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (CMSString.IsInvariantCompareCulture())
            {
                // Return invariant culture compare
                comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return value.EndsWith(str, comp);
            }

            return value.EndsWith(str, comp);
        }


        /// <summary>
        /// Determines whether the ending of this string instance matches the specified string. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">The string to compare</param>
        /// <param name="comparison">Comparison type</param>
        public static bool EndsWithCSafe(this string value, string str, StringComparison comparison)
        {
            return value.EndsWith(str, comparison);
        }


        /// <summary>
        /// Determines whether the ending of this string instance matches the specified string when compared using the specified comparison option. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">The string to compare</param>
        public static bool EndsWithCSafe(this string value, string str)
        {
            return value.EndsWith(str);
        }


        /// <summary>
        /// Returns true if the string ends with any of the given strings
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="comparison">Comparison type</param>
        /// <param name="suffixes">Suffixes to check</param>
        public static bool EndsWithAny(this string value, StringComparison comparison, params string[] suffixes)
        {
            return suffixes.Any(suffix => value.EndsWith(suffix, comparison));
        }


        /// <summary>
        /// Converts string to lower case. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="str">String source data</param>
        public static String ToLowerCSafe(this String str)
        {
            if (str == null)
            {
                return null;
            }

            if (CMSString.IsInvariantCompareCulture())
            {
                return str.ToLowerInvariant();
            }

            return str.ToLower();
        }


        /// <summary>
        /// Converts string to upper case. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="str">String source data</param>
        public static String ToUpperCSafe(this String str)
        {
            if (str == null)
            {
                return null;
            }

            if (CMSString.IsInvariantCompareCulture())
            {
                return str.ToUpperInvariant();
            }

            return str.ToUpper();
        }

        #endregion


        #region "Equals & compare"

        /// <summary>
        /// Determines whether two specified String objects have the same value. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="strA">String source data</param>
        /// <param name="strB">String to compare</param>
        public static bool EqualsCSafe(this String strA, String strB)
        {
            return CMSString.Equals(strA, strB);
        }


        /// <summary>
        /// Determines whether two specified String objects have the same value
        /// </summary>
        /// <param name="strA">String source data</param>
        /// <param name="strB">String to compare</param>
        /// <param name="ignoreCase">If true, compare is case insensitive</param>
        public static bool EqualsCSafe(this String strA, String strB, bool ignoreCase)
        {
            return CMSString.Equals(strA, strB, ignoreCase);
        }


        /// <summary>
        /// Determines whether two specified String objects have the same value. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="strA">String source data</param>
        /// <param name="strB">String to compare</param>
        /// <param name="comparison">String comparison options</param>
        public static bool EqualsCSafe(this String strA, String strB, StringComparison comparison)
        {
            return String.Equals(strA, strB, comparison);
        }


        /// <summary>
        /// Determines whether two specified String objects have the same value. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="strA">String source data</param>
        /// <param name="strB">Object to compare</param>
        public static bool EqualsCSafe(this String strA, object strB)
        {
            return CMSString.Equals(strA, strB as String);
        }


        /// <summary>
        /// Compares the current string with another string. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="strA">String source data</param>
        /// <param name="strB">String to compare</param>
        /// <param name="ignoreCase">If true, compare is case insensitive</param>
        public static int CompareToCSafe(this String strA, String strB, bool ignoreCase)
        {
            return CMSString.Compare(strA, strB, ignoreCase);
        }


        /// <summary>
        /// Compares the current string with another string. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="strA">String source data</param>
        /// <param name="strB">String to compare</param>
        public static int CompareToCSafe(this String strA, string strB)
        {
            return CMSString.Compare(strA, strB);
        }

        #endregion


        #region "IndexOf & LastIndexOf"

        /// <summary>
        /// Reports the index of the first occurrence of the specified string in the current String object. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object.</param>
        /// <param name="str">String to find</param>
        /// <param name="ignoreCase">If true, search is case insensitive.</param>
        public static int IndexOfCSafe(this string value, string str, bool ignoreCase)
        {
            StringComparison comp = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (CMSString.IsInvariantCompareCulture())
            {
                // Return invariant culture compare
                comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return value.IndexOf(str, comp);
            }

            return value.IndexOf(str, comp);
        }


        /// <summary>
        /// Reports the index of the first occurrence of the specified char in this instance. The search starts at a specified character position. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">Value object</param>
        /// <param name="chr">Char to find</param>
        /// <param name="index">The search starting position</param>
        public static int IndexOfCSafe(this string value, char chr, int index)
        {
            return value.IndexOf(chr, index);
        }

        /// <summary>
        /// Reports the index of the first occurrence of the specified char in this instance. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">Value object</param>
        /// <param name="chr">Char to find</param>
        public static int IndexOfCSafe(this string value, char chr)
        {
            return value.IndexOf(chr);
        }


        /// <summary>
        /// Reports the index of the first occurrence of the specified string in this instance. The search starts at a specified character position. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">Value object</param>
        /// <param name="str">String to find</param>
        /// <param name="startIndex">The search starting position</param>
        public static int IndexOfCSafe(this string value, string str, int startIndex)
        {
            return value.IndexOf(str, startIndex);
        }


        /// <summary>
        /// Reports the index of the first occurrence of the specified string in the current String object. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">String to find</param>
        public static int IndexOfCSafe(this string value, string str)
        {
            return value.IndexOf(str);
        }


        /// <summary>
        /// Reports the index of the first occurrence of the specified string in this instance. The search starts at a specified character position. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object.</param>
        /// <param name="str">String to find</param>
        /// <param name="startIndex">The search starting position</param>
        /// <param name="ignoreCase">If true, search is case insensitive</param>
        public static int IndexOfCSafe(this string value, string str, int startIndex, bool ignoreCase)
        {
            StringComparison comp = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (CMSString.IsInvariantCompareCulture())
            {
                // Return invariant culture compare
                comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return value.IndexOf(str, startIndex, comp);
            }

            return value.IndexOf(str, startIndex, comp);
        }


        /// <summary>
        /// Reports the index position of the last occurrence of a specified string within this instance. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">String to find</param>
        /// <param name="ignoreCase">True if comparison should be made incasesensitive</param>
        public static int LastIndexOfCSafe(this string value, string str, bool ignoreCase)
        {
            StringComparison comp = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

            if (CMSString.IsInvariantCompareCulture())
            {
                // Return invariant culture compare
                comp = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                return value.LastIndexOf(str, comp);
            }

            return value.LastIndexOf(str, comp);
        }


        /// <summary>
        /// Reports the index position of the last occurrence of a specified char within this instance. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="chr">Char to find</param>
        public static int LastIndexOfCSafe(this string value, char chr)
        {
            return value.LastIndexOf(chr);
        }


        /// <summary>
        /// Reports the index position of the last occurrence of a specified string within this instance. The search starts at a specified character position. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">String to find</param>
        /// <param name="startIndex">The search starting position</param>
        public static int LastIndexOfCSafe(this string value, string str, int startIndex)
        {
            return value.LastIndexOf(str, startIndex);
        }


        /// <summary>
        /// Reports the index position of the last occurrence of a specified string within this instance. The search starts at a specified character position. This method is culture safe - it fixes some problems with different cultures.
        /// </summary>
        /// <param name="value">String object</param>
        /// <param name="str">String to find</param>
        public static int LastIndexOfCSafe(this string value, string str)
        {
            return value.LastIndexOf(str);
        }

        #endregion

#pragma warning restore BH4000 // String manipulation methods should not be used without specifying CultureInfo.
#pragma warning restore BH4003 // String comparison methods should not be used without specifying StringComparison.
#pragma warning restore BH4004 // String comparison methods should not be used without specifying StringComparison.
    }
}