using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class for document synchronization action
    /// </summary>
    public class SynchronizeDocumentAction : BaseDocumentAction
    {
        #region "Parameters"

        /// <summary>
        /// Indicates if child documents should be included.
        /// </summary>
        protected virtual bool IncludeChildren
        {
            get
            {
                return GetResolvedParameter<bool>("IncludeChildren", true);
            }
        }

        
        /// <summary>
        /// Indicates if document tasks should be explicitly logged.
        /// </summary>
        protected virtual bool LogTasks
        {
            get
            {
                return GetResolvedParameter<bool>("LogTasks", false);
            }
        }

        #endregion


        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            // Synchronize document
            if (SourceNode != null)
            {
                // Do not log tasks if not necessary
                if (!DocumentSynchronizationHelper.LogContentChanges(SourceNode.NodeSiteName))
                {
                    return;
                }

                IEnumerable<ISynchronizationTask> tasks = null;

                if (LogTasks)
                {
                    // Get node task
                    var settings = new LogDocumentChangeSettings()
                    {
                        Node = SourceNode,
                        TaskType = TaskTypeEnum.UpdateDocument,
                        LogStaging = true,
                        LogIntegration = false,
                        Tree = SourceNode.TreeProvider,
                        ServerID = SynchronizationInfoProvider.ENABLED_SERVERS,
                        RunAsynchronously = false,
                    };

                    tasks = DocumentSynchronizationHelper.LogDocumentChange(settings);
                    if (tasks != null)
                    {
                        // Get the child documents tasks
                        if (IncludeChildren)
                        {
                            var childSettings = new LogMultipleDocumentChangeSettings()
                            {
                                SiteName = SourceSiteName,
                                NodeAliasPath = SourceAliasPath + "/%",
                                TaskType = TaskTypeEnum.UpdateDocument,
                                LogStaging = true,
                                LogIntegration = false,
                                Tree = Node.TreeProvider,
                                ServerID = SynchronizationInfoProvider.ENABLED_SERVERS,
                                RunAsynchronously = false,
                                CultureCode = Node.DocumentCulture
                            };

                            // Merge tasks
                            var childTasks = DocumentSynchronizationHelper.LogDocumentChange(childSettings);
                            if (childTasks != null)
                            {
                                tasks = tasks.Union(childTasks);
                            }
                        }
                    }

                    // Run the synchronization
                    new StagingTaskRunner(SynchronizationInfoProvider.ENABLED_SERVERS, SourceSite.SiteID).RunSynchronization(tasks.Select(x => x.TaskID));
                }
                else
                {
                    // Get where condition
                    string path = SqlHelper.EscapeQuotes(SourceAliasPath);
                    string where = "TaskNodeAliasPath = N'" + path + "'";
                    if (IncludeChildren)
                    {
                        where = SqlHelper.AddWhereCondition(where, "TaskNodeAliasPath LIKE N'" + SqlHelper.EscapeLikeText(path) + "/%'", "OR");
                    }

                    // Get the tasks
                    int totalRecords = 0;

                    DataSet ds = StagingTaskInfoProvider.SelectTaskList(SourceNode.NodeSiteID, SynchronizationInfoProvider.ENABLED_SERVERS, where, null, 0, "TaskID", 0, 0, ref totalRecords);
                    new StagingTaskRunner(SynchronizationInfoProvider.ENABLED_SERVERS).RunSynchronization(ds);
                }
            }
        }
    }
}
