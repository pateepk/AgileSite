using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for Page builder feature data context.
    /// </summary>
    internal interface IPageBuilderDataContext
    {
        /// <summary>
        /// Gets the page where the Page builder stores and loads data from.
        /// </summary>
        TreeNode Page { get; }


        /// <summary>
        /// Gets the Page builder configuration.
        /// </summary>
        PageBuilderConfiguration Configuration { get; }
    }
}