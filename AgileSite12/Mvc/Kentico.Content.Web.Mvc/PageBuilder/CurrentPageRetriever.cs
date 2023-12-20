using System;

using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an instance for retrieving the page where is the Page builder used.
    /// </summary>
    internal class CurrentPageRetriever : ICurrentPageRetriever
    {
        /// <summary>
        /// Retrieves current page of a Page builder.
        /// </summary>
        /// <param name="feature">The Page builder feature.</param>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="feature"/> is <c>null</c>.</exception>
        public TreeNode Retrieve(IPageBuilderFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentException(nameof(feature));
            }

            var data = feature.GetDataContext();
            return data.Page;
        }
    }
}