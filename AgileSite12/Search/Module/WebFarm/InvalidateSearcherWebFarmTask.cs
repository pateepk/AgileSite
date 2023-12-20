using System;

using CMS.Core;

namespace CMS.Search
{
    /// <summary>
    /// Web farm task used to invalidate searcher.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateSearcherWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the searcher to be invalidated.
        /// </summary>
        public Guid Guid { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="InvalidateSearcherWebFarmTask"/>.
        /// </summary>
        public InvalidateSearcherWebFarmTask()
        {
            TaskTarget = "updatebizformfile";
        }


        /// <summary>
        /// Returns true if the synchronization of the smart search is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return SearchSynchronization.SynchronizeSmartSearch;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="SearchHelper.InvalidateSearcher(Guid)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            SearchHelper.InvalidateSearcher(Guid);
        }
    }
}
