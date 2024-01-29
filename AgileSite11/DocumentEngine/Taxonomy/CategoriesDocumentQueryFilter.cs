using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Taxonomy;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents the state of categories filter data and provides methods to apply filter on <see cref="IDocumentQuery" /> objects.
    /// </summary>
    internal class CategoriesDocumentQueryFilter : IDocumentQueryFilter
    {
        private readonly IEnumerable<string> mCategoryNames;
        private readonly bool mOnlyEnabled;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="categoryNames">Collection of category names.</param>
        /// <param name="onlyEnabled">Indicates if only documents belonging to enabled (sub)categories are to be filtered out.</param>
        public CategoriesDocumentQueryFilter(IEnumerable<string> categoryNames, bool onlyEnabled)
        {
            mCategoryNames = categoryNames;
            mOnlyEnabled = onlyEnabled;
        }


        /// <summary>
        /// Returns <see cref="IWhereCondition"/> object based on current Tags filter inner state.
        /// </summary>
        /// <param name="properties">Query properties representing the query inner state.</param>
        /// <returns>Where Condition.</returns>
        public IWhereCondition GetWhereCondition(DocumentQueryProperties properties)
        {
            var categories = new HashSet<CategoryInfo>();
            var where = new WhereCondition();

            // Check if there are any data for filtering
            if (mCategoryNames.All(String.IsNullOrEmpty))
            {
                return where;
            }

            foreach (var categoryName in mCategoryNames)
            {
                categories.Add(CategoryInfoProvider.GetCategoryInfo(categoryName, properties.SiteName));
            }

            var categoryIDPaths = categories.Where(c => c != null).Select(c => c.CategoryIDPath).ToArray();
            if (categoryIDPaths.Any())
            {
                return where.And(CategoryInfoProvider.GetCategoriesDocumentsWhereCondition(categoryIDPaths, mOnlyEnabled));
            }

            return where.NoResults();
        }
    }
}