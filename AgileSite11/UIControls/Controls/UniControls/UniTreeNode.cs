using System;
using System.Web;

namespace CMS.UIControls
{
    /// <summary>
    /// Universal site map tree node.
    /// </summary>
    public class UniTreeNode : SiteMapNode
    {
        #region "Variables"

        private object mItemData = null;
        private bool mChildNodesLoaded = false;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Property to get and set the inner object.
        /// </summary>
        public object ItemData
        {
            get
            {
                return mItemData;
            }
            set
            {
                mItemData = value;
            }
        }


        /// <summary>
        /// Indicates if child nodes are already loaded.
        /// </summary>
        public bool ChildNodesLoaded
        {
            get
            {
                return mChildNodesLoaded;
            }
            set
            {
                mChildNodesLoaded = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        public UniTreeNode(SiteMapProvider provider, string key)
            : base(provider, key)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        public UniTreeNode(SiteMapProvider provider, string key, string url)
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
        public UniTreeNode(SiteMapProvider provider, string key, string url, string title)
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
        public UniTreeNode(SiteMapProvider provider, string key, string url, string title, string description)
            : base(provider, key, url, title, description)
        {
        }

        #endregion
    }
}