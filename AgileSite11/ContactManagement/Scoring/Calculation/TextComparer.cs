using System;
using System.Collections.Generic;
using System.Globalization;

using CMS.Base;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Compares two strings based on <see cref="TextCompareOperatorEnum"/> condition.
    /// </summary>
    internal static class TextComparer
    {
        private static readonly Dictionary<TextCompareOperatorEnum, Func<string, string, bool>> compareFunctions = new Dictionary<TextCompareOperatorEnum, Func<string, string, bool>>
        {
            {
                TextCompareOperatorEnum.Empty, (value, value2) => string.IsNullOrEmpty(value)
            },
            {
                TextCompareOperatorEnum.EndsWith, (value, value2) => value.EndsWith(value2, StringComparison.CurrentCulture)
            },
            {
                TextCompareOperatorEnum.Equals, (value, value2) => value.EqualsCSafe(value2)
            },  
            {
                TextCompareOperatorEnum.Like, (value, value2) => value.Contains(value2)
            },
            {
                TextCompareOperatorEnum.NotEmpty, (value, value2) => !string.IsNullOrEmpty(value)
            },
            {
                TextCompareOperatorEnum.NotEndsWith, (value, value2) => !value.EndsWith(value2, StringComparison.CurrentCulture)
            },
            {
                TextCompareOperatorEnum.NotEquals, (value, value2) => !value.EqualsCSafe(value2)
            },
            {
                TextCompareOperatorEnum.NotLike, (value, value2) => !value.Contains(value2)
            },
            {
                TextCompareOperatorEnum.NotStartsWith, (value, value2) => !value.StartsWith(value2, StringComparison.CurrentCulture)
            },
            {    
                TextCompareOperatorEnum.StartsWith, (value, value2) => value.StartsWith(value2, StringComparison.CurrentCulture)
            }
        };


        /// <summary>
        /// Checks whether given text in <paramref name="value"/> matches given condition. 
        /// Comparison is case-insensitive and <code>null</code> is considered as empty string.
        /// For example "test text" <see cref="TextCompareOperatorEnum.Like"/> "Test" is true.
        /// </summary>
        /// <returns>True if <paramref name="value"/> matches given <paramref name="compareOperator"/> and <paramref name="value2"/>.</returns>
        /// <exception cref="NotImplementedException"><paramref name="compareOperator"/> is not supported</exception>
        internal static bool Compare(string value, TextCompareOperatorEnum compareOperator, string value2)
        {
            if (!compareFunctions.ContainsKey(compareOperator))
            {
                throw new NotImplementedException();
            }

            return compareFunctions[compareOperator]((value ?? "").ToLower(CultureInfo.CurrentCulture), (value2 ?? "").ToLower(CultureInfo.CurrentCulture));
        }
    }
}
