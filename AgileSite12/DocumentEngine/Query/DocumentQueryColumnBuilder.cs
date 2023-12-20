using CMS.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DocumentEngine
{
    internal class DocumentQueryColumnBuilder
    {
        /// <summary>
        /// Priority column for document aliases based on sections count in <see cref="DocumentAliasInfo.AliasURLPath"/>.
        /// </summary>
        public const string AliasURLSectionsCountPriority = "LEN(AliasURLPath) - LEN(REPLACE(AliasURLPath, '/', ''))";


        /// <summary>
        /// Priority column for document aliases based on wildcards count in <see cref="DocumentAliasInfo.AliasWildcardRule"/>.
        /// </summary>
        public const string WildcardCountPriority = "LEN(AliasWildcardRule) - LEN(REPLACE(AliasWildcardRule, '%', ''))";


        /// <summary>
        /// Priority column for document based on sections count in <see cref="PageInfo.DocumentUrlPath"/>.
        /// </summary>
        public const string DocumentURLSectionsCountPriority = "LEN(DocumentURLPath) - LEN(REPLACE(DocumentURLPath, '/', ''))";


        /// <summary>
        /// Gets culture priority column with cultures prioritized by the order in the provided set
        /// </summary>
        /// <param name="cultureColumnName">Name of column with the culture codes</param>
        /// <param name="cultures">Set of culture codes</param>
        public static QueryColumn GetCulturePriorityColumn(string cultureColumnName, ICollection<string> cultures)
        {
            if ((cultures == null) || (cultures.Count == 0))
            {
                return null;
            }

            var filteredCultures = cultures.Where(c => !String.IsNullOrWhiteSpace(c)).ToList();
            var priority = filteredCultures.Count;
            var cases = new HashSet<KeyValuePair<string, string>>();
            foreach (var culture in filteredCultures)
            {
                cases.Add(new KeyValuePair<string, string>(new WhereCondition(cultureColumnName, QueryOperator.Equals, culture).ToString(true), priority.ToString()));
                --priority;
            }

            var columnText = SqlHelper.GetCase(cases, "0");
            return new QueryColumn(columnText);
        }


        /// <summary>
        /// Gets culture priority column with cultures prioritized as follows: current culture code (<paramref name="currentCultureCode"/>), 
        /// default culture code (<paramref name="defaultCultureCode"/>) if parameter (<paramref name="combineWithDefaultCulture"/>) is true.
        /// Otherwise uses only current culture code.
        /// </summary>
        /// <param name="cultureColumnName">Name of column with the culture codes</param>
        /// <param name="currentCultureCode">Code of current culture.</param>
        /// <param name="defaultCultureCode">Code of default culture.</param>
        /// <param name="combineWithDefaultCulture">Indicates if result should contain default culture.</param>
        public static QueryColumn GetCulturePriorityColumn(string cultureColumnName, string currentCultureCode, string defaultCultureCode, bool combineWithDefaultCulture)
        {
            var cultures = combineWithDefaultCulture ? new[] { currentCultureCode, defaultCultureCode } : new[] { currentCultureCode };

            return GetCulturePriorityColumn(cultureColumnName, cultures);
        }


        /// <summary>
        /// Method returns a priority column for a plain and wildcard aliases, where a plain alias has better priority
        /// than an alias with wildcard. Alias with wildcard has some value in column AliasWildcardRule.
        /// </summary>
        public static QueryColumn GetPlainAliasPriority()
        {
            var where = new WhereCondition().WhereEqualsOrNull("AliasWildcardRule", string.Empty);

            var cases = new HashSet<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(where.ToString(true), "1")
            };

            var columnText = SqlHelper.GetCase(cases, "0");
            return new QueryColumn(columnText);
        }
    }
}
