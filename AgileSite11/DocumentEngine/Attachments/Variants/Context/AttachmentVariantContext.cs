using CMS.ResponsiveImages;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Image context for attachment image.
    /// </summary>
    public class AttachmentVariantContext : IVariantContext
    {
        /// <summary>
        /// Parent document
        /// </summary>
        private TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the document alias path.
        /// </summary>
        public string AliasPath
        {
            get
            {
                if (Node == null)
                {
                    return null;
                }

                return Node.NodeAliasPath;
            }
        }


        /// <summary>
        /// Returns the document site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                if (Node == null)
                {
                    return null;
                }

                return Node.NodeSiteName;
            }
        }


        /// <summary>
        /// Returns the document class name.
        /// </summary>
        public string ClassName
        {
            get
            {
                if (Node == null)
                {
                    return null;
                }

                return Node.ClassName;
            }
        }


        /// <summary>
        /// Creates a new instance of the <see cref="AttachmentVariantContext" /> class.
        /// </summary>
        /// <param name="node">Parent page.</param>
        public AttachmentVariantContext(TreeNode node = null)
        {
            Node = node;
        }
    }
}