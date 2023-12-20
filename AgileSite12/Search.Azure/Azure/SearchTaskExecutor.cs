using System;
using System.Linq;

using CMS;
using CMS.EventLog;
using CMS.Scheduler;

[assembly: RegisterCustomClass(nameof(SearchTaskExecutor), typeof(CMS.Search.Azure.SearchTaskExecutor))]

namespace CMS.Search.Azure
{
    /// <summary>
    /// Processes Azure Search tasks.
    /// </summary>
    public class SearchTaskExecutor : ITask
    {
        private ISearchTaskEngine mSearchTaskEngine;


        /// <summary>   
        /// Search task engine.
        /// </summary>
        internal ISearchTaskEngine SearchTaskEngine
        {
            get
            {
                return mSearchTaskEngine ?? new SearchTaskEngine();
            }
            set
            {
                mSearchTaskEngine = value;
            }
        }


        /// <summary>
        /// Executes all search tasks.
        /// </summary>
        /// <param name="task">Task info.</param>
        /// <returns>Always returns <c>null</c>.</returns>
        public string Execute(TaskInfo task)
        {
            // This query is backed with database index
            var tasks = SearchTaskAzureInfoProvider.GetSearchTaskAzureInfos()
                .OrderByDescending(nameof(SearchTaskAzureInfo.SearchTaskAzurePriority))
                .OrderByAscending(nameof(SearchTaskAzureInfo.SearchTaskAzureID))
                .TopN(SearchManager.TaskProcessingBatchSize);

            while (tasks.Any())
            {
                foreach (var searchTask in tasks)
                {
                    try
                    {
                        SearchTaskEngine.ProcessAzureSearchTask(searchTask);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("Azure Search task processor", "PROCESS", ex);

                        searchTask.SearchTaskAzureErrorMessage = ex.Message;
                        SearchTaskAzureInfoProvider.SetSearchTaskAzureInfo(searchTask);

                        return null;
                    }

                    SearchTaskAzureInfoProvider.DeleteSearchTaskAzureInfo(searchTask);
                }

                tasks.Reset();
            }

            return null;
        }
    }
}
