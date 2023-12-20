using System;

using CMS;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Search.Azure;
using CMS.Base;


[assembly: RegisterModule(typeof(SearchAzureModule))]

namespace CMS.Search.Azure
{
    internal class SearchAzureModule : Module
    {
        /// <summary>
        /// Identifier of <see cref="SearchAzureModule"/>.
        /// </summary>
        public const string MODULE_NAME = "CMS.Search.Azure";


        /// <summary>
        /// Initializes Azure Search module.
        /// </summary>
        public SearchAzureModule()
            : base(MODULE_NAME)
        {
        }


        protected override void OnInit()
        {
            base.OnInit();

            SearchIndexInfo.TYPEINFO.Events.Delete.Before += DeleteAzureIndex;
            SearchEvents.SearchTaskCreationHandler.Execute += LogAzureSearchTasks;
            IndexStatisticsProviders.Instance.Register(SearchIndexInfo.AZURE_SEARCH_PROVIDER, new AzureIndexStatisticsProvider());
            SearchHelper.CreatingDefaultSearchSettings.Execute += SearchFieldsHandlers.CreateDefaultSearchSettings;
            SearchFieldFactory.Instance.Creating.Execute += SearchFieldsHandlers.SetSearchFieldFlags;
            SearchFieldFactory.Instance.CreatingFromSettings.Execute += SearchFieldsHandlers.MapSearchFieldFlags;
            SearchFieldsHelper.Instance.IncludeContentField.Execute += SearchFieldsHandlers.IsContentField;
            SearchFieldsHelper.Instance.IncludeIndexField.Execute += SearchFieldsHandlers.IsIndexField;
        }


        /// <summary>
        /// Deletes index on Azure before it is deleted from the system.
        /// </summary>
        private void DeleteAzureIndex(object sender, ObjectEventArgs e)
        {
            var searchIndex = e.Object as SearchIndexInfo;

            if (searchIndex == null || !searchIndex.IsAzureIndex())
            {
                return;
            }

            var searchService = new SearchService { Name = searchIndex.IndexSearchServiceName, AdminApiKey = searchIndex.IndexAdminKey };
            var searchServiceManager = new SearchServiceManager(searchService);

            try
            {
                searchServiceManager.DeleteIndexIfExists(searchIndex.IndexName);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogEvent(new EventLogInfo
                {
                    EventType = EventType.WARNING,
                    Exception = ex,
                    Source = "Azure Search provider",
                    EventCode = "DELETE INDEX",
                    EventDescription = $"An error occurred when deleting index '{searchIndex.IndexName}' on Azure. If the index exists, it has to be deleted manually."
                });
            }
        }


        /// <summary>
        /// Logs <see cref="SearchTaskAzureInfo"/>s based on <see cref="SearchTaskCreationEventArgs"/>.
        /// </summary>
        private void LogAzureSearchTasks(object sender, SearchTaskCreationEventArgs e)
        {
            foreach (var param in e.Parameters)
            {
                if (!IsAzureSearchTask(param))
                {
                    continue;
                }

                SearchTaskAzureInfoProvider.SetSearchTaskAzureInfo(new SearchTaskAzureInfo
                {
                    SearchTaskAzureAdditionalData = param.TaskValue,
                    SearchTaskAzureInitiatorObjectID = param.RelatedObjectID,
                    SearchTaskAzureMetadata = param.ObjectField,
                    SearchTaskAzureObjectType = param.ObjectType,
                    SearchTaskAzureType = param.TaskType
                });
            }
        }


        /// <summary>
        /// Indicates whether search task is to be logged to be processed by Azure Search task processing engine.
        /// </summary>
        private bool IsAzureSearchTask(SearchTaskCreationParameters searchTaskCreationParameters)
        {
            if (searchTaskCreationParameters.TaskType == SearchTaskTypeEnum.Rebuild)
            {
                var index = SearchIndexInfoProvider.GetSearchIndexInfo(searchTaskCreationParameters.RelatedObjectID);
                if (index == null || !index.IsAzureIndex())
                {
                    return false;
                }
            }
            else if ((searchTaskCreationParameters.TaskType == SearchTaskTypeEnum.Process)
                && String.Equals(searchTaskCreationParameters.ObjectType, PredefinedObjectType.CUSTOMTABLECLASS, StringComparison.OrdinalIgnoreCase))
            {
                // This is an optimization only, the task processing would be void as no custom table Azure index can be defined
                return false;
            }

            return true;
        }
    }
}
