using CMS.DocumentEngine;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Retrieves page based on virtual context parameters.
    /// </summary>
    internal interface IVirtualContextPageRetriever
    {
        /// <summary>
        /// Retrieves page by its workflow cycle GUID from virtual context.
        /// </summary>
        TreeNode Retrieve();
    }
}
