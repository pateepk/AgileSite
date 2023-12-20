using CMS.Core;

namespace CMS.Search
{
    /// <summary>
    /// Web farm task used to run smart search indexer.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RunSmartSearchIndexerWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RunSmartSearchIndexerWebFarmTask"/>.
        /// </summary>
        public RunSmartSearchIndexerWebFarmTask()
        {
            TaskTarget = "RunSmartSearchIndexer";
        }


        /// <summary>
        /// Returns true if the synchronization of the smart search is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return SearchSynchronization.SynchronizeSmartSearch;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="SearchTaskInfoProvider.ProcessTasks()"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            SearchTaskInfoProvider.ProcessTasks();
        }
    }
}
