using System.Collections.Generic;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Handler which belongs to an event that is fired when new search task is going to be created.
    /// </summary>
    public class SearchTaskCreationHandler : SimpleHandler<SearchTaskCreationHandler, SearchTaskCreationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="parameters">Collection of search task creation parameters.</param>
        public SearchTaskCreationEventArgs StartEvent(ICollection<SearchTaskCreationParameters> parameters)
        {
            SearchTaskCreationEventArgs e = new SearchTaskCreationEventArgs
            {
                Parameters = parameters,
            };

            return StartEvent(e);
        }
    }
}
