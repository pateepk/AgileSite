using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DocumentEngine;
using CMS.Helpers;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Encapsulates methods providing page data for page selector.
    /// </summary>
    internal class PageSelectorPagesRepository
    {
        /// <summary>
        /// Gets the page data of the given <paramref name="nodeGuid"/>. 
        /// </summary>
        /// <param name="nodeGuid">Node GUID.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="culture">Culture code.</param>
        public virtual TreeNode GetPage(Guid nodeGuid, string siteName, string culture)
        {
            return GetBaseDocumentQuery(siteName, culture)
              .WhereEquals("NodeGUID", nodeGuid)
              .TopN(1)
              .FirstOrDefault();
        }


        /// <summary>
        /// Gets the page data of the given <paramref name="nodeAliasPath"/>. 
        /// </summary>
        /// <param name="nodeAliasPath">The path to the root node of the dialog.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="culture">Culture.</param>
        /// <returns></returns>
        public virtual TreeNode GetPage(string nodeAliasPath, string siteName, string culture)
        {
            return GetBaseDocumentQuery(siteName, culture)
                .Path(nodeAliasPath)
                .TopN(1)
                .FirstOrDefault();
        }


        /// <summary>
        /// Provides data about child pages of a given <paramref name="parentId"/>.
        /// </summary>
        /// <param name="parentId">ID of a parent page.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="culture">Culture code.</param>
        public virtual IEnumerable<TreeNode> GetChildPages(int parentId, string siteName, string culture)
        {
            return GetBaseDocumentQuery(siteName, culture)
                .WhereEquals("NodeParentID", parentId)
                .OrderBy("NodeOrder")
                .ToList();
        }


        private DocumentQuery GetBaseDocumentQuery(string siteName, string culture)
        {
            return new DocumentQuery()
              .OnSite(siteName)
              .Culture(culture, CultureHelper.GetDefaultCultureCode(siteName))
              .CombineWithAnyCulture()
              .LatestVersion()
              .CheckPermissions();
        }
    }
}
