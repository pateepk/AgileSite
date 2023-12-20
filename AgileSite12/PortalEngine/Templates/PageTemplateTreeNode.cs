using System;
using System.Web;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Tree node representation for the page template.
    /// </summary>
    public class PageTemplateTreeNode : SiteMapNode
    {
        /// <summary>
        /// Property to get and set the inner object.
        /// </summary>
        public object ItemData
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        public PageTemplateTreeNode(SiteMapProvider provider, string key)
            : base(provider, key)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        public PageTemplateTreeNode(SiteMapProvider provider, string key, string url)
            : base(provider, key, url)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        /// <param name="title">Node title</param>
        public PageTemplateTreeNode(SiteMapProvider provider, string key, string url, string title)
            : base(provider, key, url, title)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        /// <param name="title">Node title</param>
        /// <param name="description">Node description</param>
        public PageTemplateTreeNode(SiteMapProvider provider, string key, string url, string title, string description)
            : base(provider, key, url, title, description)
        {
        }
    }
}