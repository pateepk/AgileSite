using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Performs a case-insensitive, culture invariant comparison of strings with brackets removed by <see cref="SqlHelper.RemoveSquareBrackets"/>.
    /// </summary>
    internal class BracketInvariantStringComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Compares two strings to decide if they are same without brackets.
        /// </summary>
        /// <param name="x">The first string to compare.</param>
        /// <param name="y">The second string to compare.</param>
        /// <returns>True if two strings are equal with removed brackets; otherwise, false.</returns>
        public bool Equals(string x, string y)
            => ReferenceEquals(x, y) || StringComparer.InvariantCultureIgnoreCase.Equals(SqlHelper.RemoveSquareBrackets(x), SqlHelper.RemoveSquareBrackets(y));


        /// <summary>
        /// Gets the hash code for the specified string with removed brackets.
        /// </summary>
        /// <param name="obj">A string.</param>
        /// <returns>Hash code calculated from the value of the obj parameter.</returns>
        public int GetHashCode(string obj) => StringComparer.InvariantCultureIgnoreCase.GetHashCode(SqlHelper.RemoveSquareBrackets(obj));
    }
}