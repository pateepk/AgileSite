using CMS.Base;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Arguments of event represented by <see cref="CreateOrUpdateIndexHandler"/>.
    /// </summary>
    public class CreateOrUpdateIndexEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Azure index to be created or updated.
        /// </summary>
        public Index Index { get; set; }


        /// <summary>
        /// Azure Search service where <see cref="Index"/> is to be created or updated.
        /// </summary>
        public SearchService SearchService { get; set; }
    }
}
