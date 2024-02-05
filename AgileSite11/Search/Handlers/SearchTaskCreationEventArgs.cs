using System.Collections.Generic;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Arguments for event fired when new search task is going to be created.
    /// </summary>
    public class SearchTaskCreationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Collection of <see cref="SearchTaskCreationParameters"/> that are used for creating search tasks.
        /// </summary>
        public ICollection<SearchTaskCreationParameters> Parameters
        {
            get;
            set;
        }
    }
}
