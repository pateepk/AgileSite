using System.Web;
using System.Web.UI.WebControls;

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Tree node definition for the forum post tree.
    /// </summary>
    public class ForumPostTreeNode : SiteMapNode
    {
        private object mItemData = null;
        private bool mChildNodesLoaded = false;

        private TreeNode mTreeNode = null;

        /// <summary>
        /// Property to get and set the inner TreeNode.
        /// </summary>
        public TreeNode TreeNode
        {
            get
            {
                return mTreeNode;
            }
            set
            {
                mTreeNode = value;
            }
        }


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
        /// Flag saying whether the child nodes are already loaded to the node.
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


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        public ForumPostTreeNode(SiteMapProvider provider, string key)
            : base(provider, key)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">TreeSiteMapProvider</param>
        /// <param name="key">Node key</param>
        /// <param name="url">Node URL</param>
        public ForumPostTreeNode(SiteMapProvider provider, string key, string url)
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
        public ForumPostTreeNode(SiteMapProvider provider, string key, string url, string title)
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
        public ForumPostTreeNode(SiteMapProvider provider, string key, string url, string title, string description)
            : base(provider, key, url, title, description)
        {
        }
    }
}