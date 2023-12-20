using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Represents the page template filter context.
    /// </summary>
    public class PageTemplateFilterContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="PageTemplateFilterContext"/>.
        /// </summary>
        /// <param name="parentPage">The parent of the page that is to be created.</param>
        /// <param name="pageType">The page type of the page that is to be created.</param>
        /// <param name="culture">The culture of the page that is to be created.</param>
        internal PageTemplateFilterContext(TreeNode parentPage, string pageType, string culture)
        {
            ParentPage = parentPage;
            PageType = pageType;
            Culture = culture;
        }


        /// <summary>
        /// Gets the culture of the page that is to be created. If the page already exists, returns the culture of the existing page.
        /// </summary>
        public string Culture { get; }


        /// <summary>
        /// Gets the page type of the page that is to be created. If the page already exists, returns the page type of the existing page.
        /// </summary>
        public string PageType { get; }


        /// <summary>
        /// Gets the parent of the page that is to be created. If the page already exists, returns the parent of the existing page.
        /// </summary>
        public TreeNode ParentPage { get; }
    }
}
