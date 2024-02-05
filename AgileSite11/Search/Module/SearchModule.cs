using System;

using CMS;
using CMS.DataEngine;
using CMS.Search;

[assembly: RegisterModule(typeof(SearchModule))]

namespace CMS.Search
{
    /// <summary>
    /// Represents the Search module.
    /// </summary>
    public class SearchModule : Module
    {
        /// <summary>
        /// Module constructor
        /// </summary>
        public SearchModule()
            : base(new SearchModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            SearchIndexers.Init();
            SearchHandlers.Init();
            SearchSynchronization.Init();

            ImportSpecialActions.Init();
            ExportSpecialActions.Init();

            SearchFieldsHelper.Instance.IncludeContentField.Execute += IsContentField;
            SearchFieldsHelper.Instance.IncludeIndexField.Execute += IsIndexField;

            IndexStatisticsProviders.Instance.Register(SearchIndexInfo.LUCENE_SEARCH_PROVIDER, new LuceneIndexStatisticsProvider());
        }


        /// <summary>
        /// Performs detection of content field for local (Lucene) indexes and for index agnostic detection.
        /// </summary>
        private void IsContentField(object sender, IsContentFieldEventArgs eventArgs)
        {
            var isContentField = eventArgs.SearchSettings.GetFlag(SearchSettings.CONTENT);

            if (eventArgs.Index == null)
            {
                eventArgs.Result |= isContentField;
            }
            else if (eventArgs.Index.IndexProvider.Equals(SearchIndexInfo.LUCENE_SEARCH_PROVIDER, StringComparison.OrdinalIgnoreCase))
            {
                eventArgs.Result = isContentField;
            }
        }


        /// <summary>
        /// Performs detection of field to be included as dedicated index field for local (Lucene) indexes and for index agnostic detection.
        /// </summary>
        private void IsIndexField(object sender, IsIndexFieldEventArgs eventArgs)
        {
            var isIndexField = eventArgs.SearchSettings.GetFlag(SearchSettings.SEARCHABLE);

            if (eventArgs.Index == null)
            {
                eventArgs.Result |= isIndexField;
            }
            else if (eventArgs.Index.IndexProvider.Equals(SearchIndexInfo.LUCENE_SEARCH_PROVIDER, StringComparison.OrdinalIgnoreCase))
            {
                eventArgs.Result = isIndexField;
            }
        }
    }
}