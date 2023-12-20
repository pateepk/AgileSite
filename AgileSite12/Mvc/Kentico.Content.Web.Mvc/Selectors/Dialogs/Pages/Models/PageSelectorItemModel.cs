using System;
using System.Web.Mvc;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.SiteProvider;

using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.Dialogs.Internal
{
    /// <summary>
    /// Represents a single item in a page selector.
    /// </summary>
    public sealed class PageSelectorItemModel
    {
        /// <summary>
        /// Item name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }


        /// <summary>
        /// Node ID.
        /// </summary>
        [JsonProperty("nodeId")]
        public int NodeId { get; private set; }


        /// <summary>
        /// Node Guid.
        /// </summary>
        [JsonProperty("nodeGuid")]
        public Guid NodeGuid { get; private set; }


        /// <summary>
        /// Document name path.
        /// </summary>
        [JsonProperty("namePath")]
        public string NamePath { get; private set; }


        /// <summary>
        /// Node alias path.
        /// </summary>
        [JsonProperty("nodeAliasPath")]
        public string NodeAliasPath { get; private set; }


        /// <summary>
        /// Page application-relative URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }


        /// <summary>
        /// Item icon class.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; private set; }


        /// <summary>
        /// Indicates whether an item has child items.
        /// </summary>
        [JsonProperty("hasChildNodes")]
        public bool HasChildNodes { get; private set; }


        /// <summary>
        /// Indicates whether the page is exists and the current user has sufficient permissions to access the page.
        /// </summary>
        [JsonProperty("isValid")]
        public bool IsValid { get; private set; } = true;


        /// <summary>
        /// Prevents a default instance of the <see cref="PageSelectorItemModel"/> class from being created.
        /// </summary>
        private PageSelectorItemModel()
        {
        }


        /// <summary>
        /// Creates a new instance of <see cref="PageSelectorItemModel"/> based on a given <paramref name="page"/>.
        /// </summary>
        /// <param name="page">The page acting as a source data.</param>
        /// <param name="urlHelper">URL helper.</param>
        internal static PageSelectorItemModel Create(TreeNode page, UrlHelper urlHelper)
        {
            if (page == null)
            {
                return new PageSelectorItemModel
                {
                    IsValid = false
                };
            }

            var pageType = DataClassInfoProvider.GetDataClassInfo(page.ClassName);
            var url = urlHelper.Kentico().AuthenticateUrl(DocumentURLProvider.GetUrl(page)).ToString();

            var model = new PageSelectorItemModel
            {
                Name = page.DocumentName,
                NodeId = page.NodeID,
                NamePath = page.DocumentNamePath,
                NodeGuid = page.NodeGUID,
                NodeAliasPath = page.NodeAliasPath,
                Icon = (string)pageType?.GetValue("ClassIconClass"),
                HasChildNodes = page.NodeHasChildren,
                Url = url
            };

            // Site root page
            if (page.NodeParentID == 0)
            {
                // Site root has an empty name, get site display name instead
                var site = SiteInfoProvider.GetSiteInfo(page.NodeSiteID);
                if (site != null)
                {
                    model.Name = site.DisplayName;
                }
            }

            return model;
        }
    }
}
