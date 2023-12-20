using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains utility methods for performing comparisons.
    /// </summary>
    internal static class ComparisonUtils
    {
        /// <summary>
        /// Compares <paramref name="valueA"/> against <paramref name="valueB"/> using the <paramref name="comparisonType"/> specified.
        /// Null string values are treated as empty strings.
        /// </summary>
        public static bool Compare(string valueA, StringFieldComparisonTypes comparisonType, string valueB, StringComparison comparison)
        {
            switch (comparisonType)
            {
                case StringFieldComparisonTypes.Equal:
                    return (String.IsNullOrEmpty(valueA) && String.IsNullOrEmpty(valueB)) || String.Equals(valueA, valueB, comparison);

                case StringFieldComparisonTypes.NotEqual:
                    return !((String.IsNullOrEmpty(valueA) && String.IsNullOrEmpty(valueB)) || String.Equals(valueA, valueB, comparison));

                default:
                    throw new NotSupportedException($"String comparison of type '{nameof(comparisonType)}' is not supported.");
            }
        }


        /// <summary>
        /// Compares <paramref name="valueA"/> against <paramref name="valueB"/> using the <paramref name="comparisonType"/> specified.
        /// </summary>
        public static bool Compare<TNumericValue>(TNumericValue valueA, NumericFieldComparisonTypes comparisonType, TNumericValue valueB) where TNumericValue : IComparable<TNumericValue>
        {
            var comparisonResult = valueA.CompareTo(valueB);

            switch (comparisonType)
            {
                case NumericFieldComparisonTypes.LessThan:
                    return comparisonResult < 0;

                case NumericFieldComparisonTypes.LessThanOrEqual:
                    return comparisonResult <= 0;

                case NumericFieldComparisonTypes.GreaterThan:
                    return comparisonResult > 0;

                case NumericFieldComparisonTypes.GreaterThanOrEqual:
                    return comparisonResult >= 0;

                case NumericFieldComparisonTypes.Equal:
                    return comparisonResult == 0;

                case NumericFieldComparisonTypes.NotEqual:
                    return comparisonResult != 0;

                default:
                    throw new NotSupportedException($"Numeric comparison of type '{nameof(comparisonType)}' is not supported.");
            }
        }
    }
}
