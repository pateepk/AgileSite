using System;

using CMS.Helpers;
using CMS.Search;

namespace CMS.Scheduler
{
    /// <summary>
    /// Class used by scheduler to execute the task.
    /// </summary>
    public class SearchIndexOptimizer : ITask
    {
        /// <summary>
        /// Optimalize search indexes.
        /// </summary>
        /// <param name="task">Task to start</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                string indexName = DataHelper.GetNotEmpty(task.TaskData, "##all##");
                int indexID = 0;
                
                if (indexName != "##all##")
                {
                    SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexName);
                    if (indexInfo != null)
                    {
                        indexID = indexInfo.IndexID;
                    }
                }

                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Optimize, null, null, indexName, indexID);
                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}