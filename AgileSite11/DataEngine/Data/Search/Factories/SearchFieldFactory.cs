using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CMS.DataEngine
{
    /// <summary>
    /// Factory for <see cref="ISearchField"/> objects allowing search engines to customize the flags of fields being created.
    /// </summary>
    public class SearchFieldFactory
    {
        private static SearchFieldFactory instance;


        /// <summary>
        /// An event raised upon <see cref="Create"/> call allowing to set the flags of search field being created.
        /// </summary>
        public CreateSearchFieldHandler Creating = new CreateSearchFieldHandler { Name = nameof(SearchFieldFactory) + "." + nameof(Creating) };


        /// <summary>
        /// An event raised upon <see cref="CreatingFromSettings"/> call allowing to map the flags of search field being created.
        /// </summary>
        public CreateSearchFieldFromSettingsHandler CreatingFromSettings = new CreateSearchFieldFromSettingsHandler { Name = nameof(SearchFieldFactory) + "." + nameof(CreatingFromSettings) };


        /// <summary>
        /// Gets the <see cref="SearchFieldFactory"/> instance.
        /// </summary>
        public static SearchFieldFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    Interlocked.CompareExchange(ref instance, new SearchFieldFactory(), null);
                }
                return instance;
            }
        }


        /// <summary>
        /// Initializes a new factory.
        /// </summary>
        private SearchFieldFactory()
        {
            Creating.Execute += SetLocalSearchFieldFlags;
            CreatingFromSettings.Execute += MapLocalSearchFieldFlags;
        }


        /// <summary>
        /// Sets local search field flags (searchable and tokenized).
        /// </summary>
        private void SetLocalSearchFieldFlags(object sender, CreateSearchFieldEventArgs eventArgs)
        {
            switch (eventArgs.CreateOption)
            {
                case CreateSearchFieldOption.SearchableWithTokenizer:
                    eventArgs.SearchField.SetFlag(SearchSettings.TOKENIZED, true);
                    break;

                case CreateSearchFieldOption.SearchableAndRetrievable:
                    eventArgs.SearchField.SetFlag(SearchSettings.SEARCHABLE, true);
                    break;

                case CreateSearchFieldOption.SearchableAndRetrievableWithTokenizer:
                    eventArgs.SearchField.SetFlag(SearchSettings.SEARCHABLE, true);
                    eventArgs.SearchField.SetFlag(SearchSettings.TOKENIZED, true);
                    break;
            }
        }


        /// <summary>
        /// Maps local search field flags (searchable and tokenized).
        /// </summary>
        private void MapLocalSearchFieldFlags(object sender, CreateSearchFieldFromSettingsEventArgs eventArgs)
        {
            var searchField = eventArgs.SearchField;
            var searchSettings = eventArgs.SearchSettings;

            searchField.SetFlag(SearchSettings.SEARCHABLE, searchSettings.GetFlag(SearchSettings.SEARCHABLE));
            searchField.SetFlag(SearchSettings.TOKENIZED, searchSettings.GetFlag(SearchSettings.TOKENIZED));
        }


        /// <summary>
        /// Creates a new search field based on <paramref name="createOption"/>.
        /// </summary>
        /// <param name="fieldName">Name of field to be created.</param>
        /// <param name="dataType">Data type of field.</param>
        /// <param name="createOption">Option indicating which type of field to create.</param>
        /// <returns>Search field for given option.</returns>
        public ISearchField Create(string fieldName, Type dataType, CreateSearchFieldOption createOption)
        {
            var searchField = new SearchField
            {
                FieldName = fieldName,
                DataType = dataType
            };

            var eventArgs = new CreateSearchFieldEventArgs
            {
                SearchField = searchField,
                CreateOption = createOption
            };

            using (Creating.StartEvent(eventArgs))
            {
                return eventArgs.SearchField;
            }
        }


        /// <summary>
        /// Creates a new search field from <paramref name="searchSettings"/>.
        /// </summary>
        /// <param name="searchSettings">Search settings from which to create the field.</param>
        /// <param name="dataType">Data type of field.</param>
        /// <returns>Search field from given search settings.</returns>
        public ISearchField CreateFromSettings(SearchSettingsInfo searchSettings, Type dataType)
        {
            // Use custom field name if set
            var fieldName = string.IsNullOrEmpty(searchSettings.FieldName) ? searchSettings.Name : searchSettings.FieldName;

            var searchField = new SearchField
            {
                FieldName = fieldName,
                DataType = dataType
            };

            var eventArgs = new CreateSearchFieldFromSettingsEventArgs
            {
                SearchField = searchField,
                SearchSettings = searchSettings
            };

            using (CreatingFromSettings.StartEvent(eventArgs))
            {
                return eventArgs.SearchField;
            }
        }
    }
}
