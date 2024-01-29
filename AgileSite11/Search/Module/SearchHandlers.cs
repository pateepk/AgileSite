using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Base;
using CMS.SiteProvider;
using CMS.WebFarmSync;

namespace CMS.Search
{
    /// <summary>
    /// Synchronization handlers
    /// </summary>
    internal class SearchHandlers
    {
        /// <summary>
        /// Initializes the synchronization handlers
        /// </summary>
        public static void Init()
        {
            ApplicationEvents.PostStart.Execute += ProcessSearchTasks;

            SiteEvents.Delete.After += SiteDeletion_After;

            ObjectEvents.Delete.After += CreateSearchTask;
            ObjectEvents.Insert.After += CreateSearchTask;
            ObjectEvents.Update.After += CreateSearchTask;

            SearchIndexInfo.TYPEINFO.Events.Update.Before += CheckIndexValidity;

            // If site is added or removed from index mark it as outdated
            SearchIndexSiteInfo.TYPEINFO.Events.Insert.After += MarkSearchAsOutdatedBySiteInfo;
            SearchIndexSiteInfo.TYPEINFO.Events.Delete.After += MarkSearchAsOutdatedBySiteInfo;

            // If culture is added or removed from index mark it as outdated
            SearchIndexCultureInfo.TYPEINFO.Events.Insert.After += MarkSearchAsOutdatedByCultureInfo;
            SearchIndexCultureInfo.TYPEINFO.Events.Delete.After += MarkSearchAsOutdatedByCultureInfo;

            DataClassInfo.TYPEINFO.Events.Update.Before += CheckSearchSettingsValidity;

            DataClassInfo.TYPEINFO.Events.Insert.Before += SetupSearchSettings;

            WebFarmServerInfo.TYPEINFO.Events.Delete.After += DeleteOrphanedSearchTasks;
        }


        private static void DeleteOrphanedSearchTasks(object sender, ObjectEventArgs e)
        {
            var serverName = e.Object.Generalized.ObjectCodeName;
            if (!String.IsNullOrEmpty(serverName))
            {
                // Delete all search task for removed web farm server
                SearchTaskInfoProvider.DeleteQuery().WhereEquals("SearchTaskServerName", serverName).Execute();
            }
        }


        private static void MarkSearchAsOutdatedByCultureInfo(object sender, ObjectEventArgs e)
        {
            SearchIndexCultureInfo cultureInfo = (SearchIndexCultureInfo)e.Object;

            if (cultureInfo == null)
            {
                return;
            }

            MarkSearchIndexAsOutdated(cultureInfo.IndexID);
        }


        private static void MarkSearchAsOutdatedBySiteInfo(object sender, ObjectEventArgs e)
        {
            SearchIndexSiteInfo indexSiteInfo = (SearchIndexSiteInfo)e.Object;
            if (indexSiteInfo == null)
            {
                return;
            }

            MarkSearchIndexAsOutdated(indexSiteInfo.IndexID);
        }


        private static void MarkSearchIndexAsOutdated(int indexId)
        {
            var index = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);
            if (index == null)
            {
                return;
            }

            if ((SearchIndexInfoProvider.GetIndexStatus(index) == IndexStatusEnum.READY) && !index.IndexIsOutdated)
            {
                index.IndexIsOutdated = true;
                index.Update();
            }
        }


        private static void CheckSearchSettingsValidity(object sender, ObjectEventArgs e)
        {
            var dataClass = (DataClassInfo)e.Object;
            if (dataClass == null)
            {
                return;
            }

            if (!dataClass.ClassSearchEnabled || !dataClass.AnyItemChanged("ClassSearchTitleColumn", "ClassSearchContentColumn", "ClassSearchImageColumn", "ClassSearchCreationDateColumn", "ClassSearchSettings"))
            {
                return;
            }

            var indexTypes = GetRelevantIndexTypes(dataClass);

            // Get relevant index candidates
            var indexIds = SearchIndexInfoProvider.GetIndexIDs(indexTypes);
            var className = dataClass.ClassName;

            e.CallWhenFinished(() =>
            {
                UpdateOutdatedIndexes(indexIds, className);
            });
        }


        private static List<string> GetRelevantIndexTypes(DataClassInfo dataClass)
        {
            string className = dataClass.ClassName;
            List<string> indexTypes = new List<string>
            {
                SearchHelper.GENERALINDEX
            };

            if (dataClass.ClassIsDocumentType)
            {
                // Pages indexes
                indexTypes.AddRange(new[]
                {
                    PredefinedObjectType.DOCUMENT,
                    SearchHelper.DOCUMENTS_CRAWLER_INDEX
                });
            }
            else if (dataClass.ClassIsCustomTable)
            {
                // Custom table indexes
                indexTypes.Add(PredefinedObjectType.CUSTOMTABLECLASS);
            }
            else if (dataClass.ClassIsForm)
            {
                // On-line form indexes
                indexTypes.Add(SearchHelper.ONLINEFORMINDEX);
            }
            else if (className.Equals(PredefinedObjectType.FORUMPOST, StringComparison.InvariantCultureIgnoreCase))
            {
                // Forum indexes
                indexTypes.Add(PredefinedObjectType.FORUM);
            }
            else if (SearchIndexers.IndexerTypes.Contains(className, StringComparer.InvariantCultureIgnoreCase))
            {
                // Other specific indexes
                indexTypes.Add(className);
            }
            return indexTypes;
        }


        private static void UpdateOutdatedIndexes(List<int> indexIds, string className)
        {
            var indexObjects = SearchIndexInfoProvider.GetSearchIndexes()
                                                      .WhereIn("IndexID", indexIds)
                                                      .WhereEqualsOrNull("IndexIsOutdated", false);


            foreach (var searchIndexInfo in indexObjects)
            {
                // Check only 'Ready' indexes for validity, others indexes don't need set Outdated flag because they are not working (new, error, etc.)
                if (SearchIndexInfoProvider.GetIndexStatus(searchIndexInfo) != IndexStatusEnum.READY)
                {
                    continue; 
                }
                
                var settings = searchIndexInfo.IndexSettings;
                if (settings == null)
                {
                    continue;
                }

                if (SearchIndexers.GetIndexer(searchIndexInfo.IndexType).IsClassNameRelevantToIndex(className, settings))
                {
                    searchIndexInfo.IndexIsOutdated = true;
                    searchIndexInfo.Update();
                }
            }
        }


        private static void SetupSearchSettings(object sender, ObjectEventArgs e)
        {
            var dataClass = (DataClassInfo)e.Object;
            if (dataClass == null)
            {
                return;
            }

            if (String.IsNullOrEmpty(dataClass.ClassSearchSettings) && dataClass.ClassIsForm)
            {
                // Set default search settings if form has no search settings specified
                dataClass.ClassSearchEnabled = true;
                dataClass.ClassSearchSettings = SearchHelper.GetDefaultSearchSettings(dataClass);
                dataClass.ClassSearchCreationDateColumn = "FormInserted";
            }
        }


        private static void CheckIndexValidity(object sender, ObjectEventArgs e)
        {
            SearchIndexInfo index = (SearchIndexInfo)e.Object;
            if (index == null)
            {
                return;
            }

            if (index.IndexIsOutdated || (SearchIndexInfoProvider.GetIndexStatus(index) != IndexStatusEnum.READY))
            {
                // Index is already outdated
                return;
            }

            var changedColumns = index.ChangedColumns();
            var indexingFields = new[]
            {
                "IndexAnalyzerType",
                "IndexSettings",
                "IndexStopWordsFile",
                "IndexCustomAnalyzerAssemblyName",
                "IndexCustomAnalyzerClassName",
                "IndexCrawlerUserName",
                "IndexCrawlerFormsUserName",
                "IndexCrawlerUserPassword",
                "IndexCrawlerDomain"
            };

            // Check if columns relevant to indexed content changed
            if (changedColumns.Intersect(indexingFields).Any())
            {
                index.IndexIsOutdated = true;
            }
        }


        /// <summary>
        /// Processes the search tasks
        /// </summary>
        private static void ProcessSearchTasks(object sender, EventArgs eventArgs)
        {
            SearchTaskInfoProvider.ProcessTasks();
        }


        /// <summary>
        /// Executes after the site deletion process
        /// </summary>
        private static void SiteDeletion_After(object sender, SiteDeletionEventArgs e)
        {
            using (var ctx = new CMSActionContext())
            {
                // Enable smart search indexer
                ctx.EnableSmartSearchIndexer = true;

                // Process search tasks
                SearchTaskInfoProvider.ProcessTasks(true);
            }
        }


        /// <summary>
        /// Executes then search task is supposed to be created
        /// </summary>
        [ExcludeFromDebug]
        private static void CreateSearchTask(object sender, ObjectEventArgs e)
        {
            // Update smart search general indexes
            GeneralizedInfo obj = e.Object;

            var ti = obj.TypeInfo;
            if (!ti.SupportsSearch || !CMSActionContext.CurrentCreateSearchTask)
            {
                return;
            }

            // Check whether search is enabled for specified object type
            string objectType = ti.ObjectType;
            if (SearchIndexInfoProvider.IsObjectTypeIndexed(objectType))
            {
                // Create task
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, objectType, ti.IDColumn, e.Object.GetSearchID(), obj.ObjectID, true);
            }
        }
    }
}
