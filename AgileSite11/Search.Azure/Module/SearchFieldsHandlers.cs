using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Search.Azure
{
    internal static class SearchFieldsHandlers
    {
        /// <summary>
        /// All flags except the <see cref="AzureSearchFieldFlags.CONTENT"/> flag are mapped.
        /// </summary>
        private static readonly string[] mappedSearchFieldFlags =
        {
            AzureSearchFieldFlags.RETRIEVABLE,
            AzureSearchFieldFlags.SEARCHABLE,
            AzureSearchFieldFlags.FACETABLE,
            AzureSearchFieldFlags.FILTERABLE,
            AzureSearchFieldFlags.SORTABLE
        };


        /// <summary>
        /// Sets default values for Azure specific search field flags.
        /// </summary>
        public static void CreateDefaultSearchSettings(object sender, CreateDefaultSearchSettingsEventArgs eventArgs)
        {
            var searchSettings = eventArgs.SearchSettings;

            searchSettings.SetFlag(AzureSearchFieldFlags.CONTENT, eventArgs.DataType == typeof(string)); 
            searchSettings.SetFlag(AzureSearchFieldFlags.RETRIEVABLE, (eventArgs.DataType != typeof(Guid)) && (eventArgs.DataType != typeof(String)));
            searchSettings.SetFlag(AzureSearchFieldFlags.SEARCHABLE, IsAzureSearchableType(eventArgs.DataType));
        }


        /// <summary>
        /// Sets Azure specific flags of a system search field being created.
        /// </summary>
        public static void SetSearchFieldFlags(object sender, CreateSearchFieldEventArgs eventArgs)
        {
            var isKey = eventArgs.SearchField.FieldName.Equals(SearchFieldsConstants.ID, StringComparison.OrdinalIgnoreCase);
            if (isKey)
            {
                eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.SORTABLE, true);
            }

            switch (eventArgs.CreateOption)
            {
                case CreateSearchFieldOption.SearchableWithTokenizer:
                    eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.SEARCHABLE, IsAzureSearchableType(eventArgs.SearchField.DataType));
                    break;

                case CreateSearchFieldOption.SearchableAndRetrievable:
                    eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.RETRIEVABLE, true);
                    eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.FILTERABLE, !isKey);
                    break;

                case CreateSearchFieldOption.SearchableAndRetrievableWithTokenizer:
                    eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.RETRIEVABLE, true);
                    eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.FILTERABLE, !isKey);
                    eventArgs.SearchField.SetFlag(AzureSearchFieldFlags.SEARCHABLE, IsAzureSearchableType(eventArgs.SearchField.DataType));
                    break;
            }
        }


        public static bool IsAzureSearchableType(Type type)
        {
            return (type == typeof(string)) || typeof(IEnumerable<string>).IsAssignableFrom(type);
        }


        /// <summary>
        /// Maps Azure specific search settings flags to search field flags.
        /// </summary>
        public static void MapSearchFieldFlags(object sender, CreateSearchFieldFromSettingsEventArgs eventArgs)
        {
            var searchField = eventArgs.SearchField;
            var searchSettings = eventArgs.SearchSettings;

            foreach (var flag in mappedSearchFieldFlags)
            {
                searchField.SetFlag(flag, searchSettings.GetFlag(flag));
            }
        }


        /// <summary>
        /// Performs detection of content field for Azure indexes and for index agnostic detection.
        /// </summary>
        public static void IsContentField(object sender, IsContentFieldEventArgs eventArgs)
        {
            var isContentField = eventArgs.SearchSettings.GetFlag(AzureSearchFieldFlags.CONTENT);

            if (eventArgs.Index == null)
            {
                eventArgs.Result |= isContentField;
            }
            else if (eventArgs.Index.IndexProvider.Equals(SearchIndexInfo.AZURE_SEARCH_PROVIDER, StringComparison.OrdinalIgnoreCase))
            {
                eventArgs.Result = isContentField;
            }
        }


        /// <summary>
        /// Performs detection of field to be included as dedicated index field for Azure indexes and for index agnostic detection.
        /// </summary>
        public static void IsIndexField(object sender, IsIndexFieldEventArgs eventArgs)
        {
            var isIndexField = mappedSearchFieldFlags.Any(flag => eventArgs.SearchSettings.GetFlag(flag));

            if (eventArgs.Index == null)
            {
                eventArgs.Result |= isIndexField;
            }
            else if(eventArgs.Index.IndexProvider.Equals(SearchIndexInfo.AZURE_SEARCH_PROVIDER, StringComparison.OrdinalIgnoreCase))
            {
                eventArgs.Result = isIndexField;
            }
                
        }
    }
}
