using System;

using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for retrieving the page where is the Page builder used.
    /// </summary>
    /// <remarks>Interface should be used for testing purposes and is not resolved by IoC container.</remarks>
    public interface ICurrentPageRetriever
    {
        /// <summary>
        /// Retrieves current page of a Page builder.
        /// </summary>
        /// <param name="feature">Page builder feature.</param>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="feature"/> is <c>null</c>.</exception>
        TreeNode Retrieve(IPageBuilderFeature feature);
    }
}