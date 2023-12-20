using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates extension method for <see cref="IDocumentQuery" /> objects.
    /// </summary>
    public static class TaxonomyDocumentQueryExtensions
    {
        /// <summary>
        /// Retrieves only pages which are assigned to the given <paramref name="tagName"/>. Omitting <paramref name="tagGroupName"/> parameter causes the selection of the tag across all tag groups.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Multiple tags can have the same name when assigned to a different tag group. Omitting <paramref name="tagGroupName"/> parameter causes the selection of the tag across all tag groups.
        /// </para>
        /// <para>
        /// Note:
        /// This method is a data filter and can be used directly only to modify the DocumentQuery (e.g. <c>DocumentHelper.GetDocuments().WithTag("Tag_A")</c>).
        /// It cannot be used as a part of where condition (e.g. <c>DocumentHelper.GetDocuments().WhereContains("DocumentName", "Article").Or().WithTag("Tag_A")</c>).
        /// </para>
        /// </remarks>
        /// <typeparam name="TQuery">Actual type of query being extended.</typeparam>
        /// <typeparam name="TObject">Actual type of TreeNode instance.</typeparam>
        /// <param name="query">Document Query which is being extended.</param>
        /// <param name="tagName">Tag name.</param>
        /// <param name="tagGroupName">Tag group name.</param>
        public static TQuery WithTag<TQuery, TObject>(this IDocumentQuery<TQuery, TObject> query, string tagName, string tagGroupName = null)
            where TQuery : IDocumentQuery<TQuery, TObject>
            where TObject : TreeNode, new()
        {
            if (query == null)
            {
                return default(TQuery);
            }

            var typedQuery = query.GetTypedQuery();

            typedQuery
                .Properties
                .ExternalFilters
                .Add(new TagsDocumentQueryFilter(tagName, tagGroupName));

            return typedQuery;
        }


        /// <summary>
        /// Retrieves all pages which belong to any of the specified categories.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the query is parameterized with <see cref="ObjectQueryBase{TQuery,TObject}.OnSite(SiteInfoIdentifier, bool)" /> method 
        /// the <see cref="DocumentQuery"/> searches site categories first. When no site categories are found it falls back on global categories search.
        /// </para>
        /// <para>
        /// Note:
        /// This method is a data filter and can be used directly only to modify the DocumentQuery (e.g. <c>DocumentHelper.GetDocuments().InCategories("Category_A")</c>).
        /// It cannot be used as a part of where condition (e.g. <c>DocumentHelper.GetDocuments().WhereContains("DocumentName", "Article").Or().InCategories("Category_A")</c>).
        /// </para>
        /// </remarks>
        /// <typeparam name="TQuery">Actual type of query being extended.</typeparam>
        /// <typeparam name="TObject">Actual type of TreeNode instance.</typeparam>
        /// <param name="query">Document Query which is being extended.</param>
        /// <param name="categoryNames">Category names</param>
        public static TQuery InCategories<TQuery, TObject>(this IDocumentQuery<TQuery, TObject> query, params string[] categoryNames)
            where TQuery : IDocumentQuery<TQuery, TObject>
            where TObject : TreeNode, new()
        {
            return InCategories(query, categoryNames, false);
        }


        /// <summary>
        /// Retrieves all pages which belong to any of the specified enabled categories.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the query is parameterized with <see cref="ObjectQueryBase{TQuery,TObject}.OnSite(SiteInfoIdentifier, bool)" /> method 
        /// the <see cref="DocumentQuery"/> searches enabled site categories first. When no site categories are found it falls back on global categories search.
        /// </para>
        /// <para>
        /// Note:
        /// This method is a data filter and can be used directly only to modify the DocumentQuery (e.g. <c>DocumentHelper.GetDocuments().InEnabledCategories("Category_A")</c>).
        /// It cannot be used as a part of where condition (e.g. <c>DocumentHelper.GetDocuments().WhereContains("DocumentName", "Article").Or().InEnabledCategories("Category_A")</c>).
        /// </para>
        /// </remarks>
        /// <typeparam name="TQuery">Actual type of query being extended.</typeparam>
        /// <typeparam name="TObject">Actual type of TreeNode instance.</typeparam>
        /// <param name="query">Document Query which is being extended.</param>
        /// <param name="categoryNames">Category names</param>
        public static TQuery InEnabledCategories<TQuery, TObject>(this IDocumentQuery<TQuery, TObject> query, params string[] categoryNames)
            where TQuery : IDocumentQuery<TQuery, TObject>
            where TObject : TreeNode, new()
        {
            return InCategories(query, categoryNames, true);
        }


        /// <summary>
        /// Retrieves all pages which are in any of the specified categories.
        /// </summary>
        /// <typeparam name="TQuery">Actual type of query being extended.</typeparam>
        /// <typeparam name="TObject">Actual type of TreeNode instance.</typeparam>
        /// <param name="query">Document Query which is being extended.</param>
        /// <param name="categoryNames">Category names</param>
        /// <param name="onlyEnabled">Indicates if only documents belonging to enabled (sub)categories are to be filtered out.</param>
        private static TQuery InCategories<TQuery, TObject>(IDocumentQuery<TQuery, TObject> query, IEnumerable<string> categoryNames, bool onlyEnabled)
            where TQuery : IDocumentQuery<TQuery, TObject>
            where TObject : TreeNode, new()
        {
            if (query == null)
            {
                return default(TQuery);
            }

            var typedQuery = query.GetTypedQuery();

            typedQuery
                .Properties
                .ExternalFilters
                .Add(new CategoriesDocumentQueryFilter(categoryNames, onlyEnabled));

            return typedQuery;
        }
    }
}
