using System;
using System.Collections.Generic;

using Microsoft.Azure.Search.Models;


namespace CMS.Search.Azure
{
    /// <summary>
    /// Defines methods to compare two <see cref="Field"/> objects if those fields can be merged into one field in Azure index.
    /// </summary>
    internal class FieldPropertiesEqualityComparer : IEqualityComparer<Field>
    {
        /// <summary>
        /// Compares two <see cref="Field"/> objects to decide if those fields can be merged into one field in Azure index.
        /// </summary>
        /// <param name="first">First <see cref="Field"/> object to compare.</param>
        /// <param name="second">Second <see cref="Field"/> object to compare.</param>
        /// <returns>Returns true if two <see cref="Field"/> objects can be merged into one field in Azure index. False otherwise.</returns>
        public bool Equals(Field first, Field second)
        {
            if (first == null && second == null)
            {
                return true;
            }
            else if (first == null | second == null)
            {
                return false;
            }

            return ((first.Name.Equals(second.Name, StringComparison.OrdinalIgnoreCase)) &&
                (first.Type == second.Type) && (first.IsSearchable == second.IsSearchable) &&
                (first.IsSortable == second.IsSortable) && (first.IsFilterable == second.IsFilterable) &&
                (first.IsFacetable == second.IsFacetable) && (first.IsRetrievable == second.IsRetrievable)) &&
                (first.Analyzer == second.Analyzer) && (first.IndexAnalyzer == second.IndexAnalyzer) &&
                (first.IsKey == second.IsKey) && (first.SearchAnalyzer == second.SearchAnalyzer);
        }


        /// <summary>
        /// Returns hash code for <see cref="Field"/> object.
        /// </summary>
        /// <param name="obj"><see cref="Field"/> object to generate hash code from.</param>
        public int GetHashCode(Field obj)
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + (obj.Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name) : 0);
                hash = hash * 23 + (obj.Type != null ? obj.Type.GetHashCode() : 0);
                hash = hash * 23 + obj.IsSearchable.GetHashCode();
                hash = hash * 23 + obj.IsSortable.GetHashCode();
                hash = hash * 23 + obj.IsFilterable.GetHashCode();
                hash = hash * 23 + obj.IsFacetable.GetHashCode();
                hash = hash * 23 + obj.IsRetrievable.GetHashCode();
                hash = hash * 23 + (obj.Analyzer != null ? obj.Analyzer.GetHashCode() : 0);
                hash = hash * 23 + (obj.IndexAnalyzer != null ? obj.Analyzer.GetHashCode() : 0);
                hash = hash * 23 + obj.IsKey.GetHashCode();
                hash = hash * 23 + (obj.SearchAnalyzer != null ? obj.Analyzer.GetHashCode() : 0);

                return hash;
            }
        }
    }
}
