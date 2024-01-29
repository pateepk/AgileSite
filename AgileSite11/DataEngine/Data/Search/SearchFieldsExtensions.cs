using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Extension methods for working with <see cref="ISearchFields"/> collection.
    /// </summary>
    public static class SearchFieldsExtensions
    {
        /// <summary>
        /// Indicates whether content field should be stored in the index
        /// </summary>
        private static readonly BoolAppSetting mStoreContentField = new BoolAppSetting("CMSSearchStoreContentField");


        /// <summary>
        /// Adds system search fields to <paramref name="searchFields"/>.
        /// </summary>
        /// <remarks>
        /// The system search fields are fields of name <see cref="SearchFieldsConstants.TYPE"/>, <see cref="SearchFieldsConstants.ID"/>, <see cref="SearchFieldsConstants.SITE"/>, <see cref="SearchFieldsConstants.CREATED"/>,
        /// <see cref="SearchFieldsConstants.CULTURE"/> and <see cref="SearchFieldsConstants.INDEX"/>.
        /// </remarks>
        public static void AddSystemFields(this ISearchFields searchFields)
        {
            var searchFieldsFactory = SearchFieldFactory.Instance;

            searchFields.Add(searchFieldsFactory.Create(SearchFieldsConstants.TYPE, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), null);
            searchFields.Add(searchFieldsFactory.Create(SearchFieldsConstants.ID, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), null);
            searchFields.Add(searchFieldsFactory.Create(SearchFieldsConstants.SITE, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), null);
            searchFields.Add(searchFieldsFactory.Create(SearchFieldsConstants.CREATED, typeof(DateTime), CreateSearchFieldOption.SearchableAndRetrievable), null);
            searchFields.Add(searchFieldsFactory.Create(SearchFieldsConstants.CULTURE, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), null);
            searchFields.Add(searchFieldsFactory.Create(SearchFieldsConstants.INDEX, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), null);
        }


        /// <summary>
        /// Inserts index field into search fields collection if <see cref="SearchFieldsHelper.IsIndexField"/> is true.
        /// </summary>
        /// <param name="searchFields">Search fields collection.</param>
        /// <param name="infoObj">Searchable object whose field represented by <paramref name="searchSetting"/> is to be processed.</param>
        /// <param name="index">Search index for which the field is being processed.</param>
        /// <param name="searchSetting">Search settings of DataClass field.</param>
        /// <param name="dataType">Defines <see cref="Type"/> of value stored in the added field.</param>
        public static void AddIndexField(this ISearchFields searchFields, ISearchable infoObj, ISearchIndexInfo index, SearchSettingsInfo searchSetting, Type dataType)
        {
            if (!SearchFieldsHelper.Instance.IsIndexField(index, searchSetting))
            {
                return;
            }

            var searchField = SearchFieldFactory.Instance.CreateFromSettings(searchSetting, dataType);

            searchFields.Add(searchField, () => infoObj.GetValue(searchSetting.Name));
        }


        /// <summary>
        /// Appends search field value into designated content field if <see cref="SearchFieldsHelper.IsContentField"/> is true.
        /// </summary>
        /// <param name="searchFields">Search fields collection.</param>
        /// <param name="infoObj">Searchable object whose field represented by <paramref name="searchSetting"/> is to be processed.</param>
        /// <param name="index">Search index for which the field is being processed.</param>
        /// <param name="searchSetting">Search settings of DataClass field.</param>
        /// <param name="stripTags">Indicates if HTML tags should be removed from content.</param>
        public static void AddContentField(this ISearchFields searchFields, ISearchable infoObj, ISearchIndexInfo index, SearchSettingsInfo searchSetting, bool stripTags = false)
        {
            if (!SearchFieldsHelper.Instance.IsContentField(index, searchSetting))
            {
                return;
            }

            searchFields.AddToContentField(() => searchFields.PrepareContentValue(infoObj.GetValue(searchSetting.Name), stripTags));
        }


        /// <summary>
        /// Ensures <see cref="SearchFieldsConstants.CONTENT"/> field in collection. 
        /// Field is created if it's not already present.
        /// </summary>
        /// <param name="fields">Search fields collection</param>
        public static ISearchField EnsureContentField(this ISearchFields fields)
        {
            return fields.AddToContentField(() => String.Empty);
        }


        /// <summary>
        /// Appends data to <see cref="SearchFieldsConstants.CONTENT"/> field. 
        /// Field is created if it's not already present.
        /// </summary>
        /// <param name="fields">Search fields collection</param>
        /// <param name="getValueFunc">Function that returns data to append</param>
        public static ISearchField AddToContentField(this ISearchFields fields, Func<object> getValueFunc)
        {
            var createOption = mStoreContentField ? CreateSearchFieldOption.SearchableAndRetrievableWithTokenizer : CreateSearchFieldOption.SearchableWithTokenizer;
            var searchField = SearchFieldFactory.Instance.Create(SearchFieldsConstants.CONTENT, typeof(string), createOption);

            return fields.Add(searchField, getValueFunc);
        }
    }
}
