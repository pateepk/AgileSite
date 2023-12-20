using System;
using System.Text.RegularExpressions;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Contains a utility method for Unicode characters processing.
    /// </summary>
    internal static class UnicodeUtils
    {
        /// <summary>
        /// Regular expression pattern describing invalid surrogate pair.
        /// </summary>
        private const string INVALID_SURROGATE_PAIR_PATTERN = @"[\uD800-\uDBFF]([^\uDC00-\uDFFF]|$)";


        /// <summary>
        /// Strips invalid surrogate pairs from <paramref name="input"/> string. Pairs are stripped either partially (low surrogate is kept)
        /// or completely depending on whether the low surrogate is a valid character.
        /// </summary>
        /// <param name="input">Input to strip invalid surrogate pairs from.</param>
        /// <returns>Returns string where invalid surrogate pairs are omitted. Returns null if <paramref name="input"/> is null.</returns>
        /// <remarks>
        /// A valid surrogate pair is a pair comprising a high surrogate (Unicode character in range 0xD800-0xDFFF) and a low surrogate (Unicode character in range 0xDC00-0xDFFF).
        /// A high surrogate followed by a character which does not fall within the low surrogate range is considered invalid.
        /// </remarks>
        public static string StripInvalidSurrogatePairs(string input)
        {
            if (input == null)
            {
                return input;
            }

            input = Regex.Replace(input, INVALID_SURROGATE_PAIR_PATTERN, FixInvalidSurrogatePair);

            return input;
        }


        /// <summary>
        /// Fixes invalid surrogate pair (or an unpaired high surrogate at the end of a string) by omitting the high surrogate or both the high and the low surrogate
        /// depending on whether the low surrogate represents a valid character.
        /// </summary>
        private static string FixInvalidSurrogatePair(Match match)
        {
            if (match.Value.Length < 2)
            {
                return "";
            }

            var lowSurrogate = match.Value[1];

            if (Char.IsHighSurrogate(lowSurrogate))
            {
                // If the low surrogate falls within the high surrogate range, returning the low surrogate could produce another surrogate pair when put together with the rest of the resulting string
                return "";
            }

            return lowSurrogate.ToString();
        }
    }
}
