using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for retrieving the page based on given parameters.
    /// </summary>
    internal interface IPageRetriever
    {
        /// <summary>
        /// Retrieves page based on given parameters.
        /// </summary>
        /// <param name="pageIdentifier">Identifier of the page.</param>
        /// <param name="latest">Indicates if configuration should be loaded from latest version of a page if workflow is applied.</param>
        TreeNode Retrieve(int pageIdentifier, bool latest);
    }
}