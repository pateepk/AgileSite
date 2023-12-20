using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for base document action
    /// </summary>
    abstract public class BaseDocumentAction : DocumentWorkflowAction
    {
        #region "Variables"

        private TreeNode mSourceNode = null;
        private SiteInfo mSourceSite = null;
        private string mSourceCulture = null;
        
        #endregion


        #region "Parameters"

        /// <summary>
        /// Source alias path
        /// </summary>
        protected virtual string SourceAliasPath
        {
            get
            {
                return GetResolvedParameter<string>("SourceAliasPath", Node.NodeAliasPath);
            }
        }


        /// <summary>
        /// Source site name
        /// </summary>
        protected virtual string SourceSiteName
        {
            get
            {
                return GetResolvedParameter<string>("SourceSiteName", Node.NodeSiteName);
            }
        }


        /// <summary>
        /// Source document culture code
        /// </summary>
        protected virtual string SourceCulture
        {
            get
            {
                if (mSourceCulture == null)
                {
                    mSourceCulture = Node.DocumentCulture;
                }

                return mSourceCulture;
            }
            set
            {
                mSourceCulture = value;
            }
        }
        

        /// <summary>
        /// Specifies if return the default culture document when specified culture not found.
        /// </summary>
        protected virtual bool SourceCombineWithDefaultCulture
        {
            get;
            set;
        }
        

        /// <summary>
        /// Source site
        /// </summary>
        protected virtual SiteInfo SourceSite
        {
            get
            {
                return mSourceSite ?? (mSourceSite = SiteInfoProvider.GetSiteInfo(SourceSiteName));
            }
        }


        /// <summary>
        /// Node to be copied
        /// </summary>
        protected virtual TreeNode SourceNode
        {
            get
            {
                if (mSourceNode == null)
                {
                    if (Node.NodeAliasPath.EqualsCSafe(SourceAliasPath))
                    {
                        mSourceNode = Node;
                    }
                    else
                    {
                        mSourceNode = DocumentHelper.GetDocument(SourceSiteName, SourceAliasPath, SourceCulture, SourceCombineWithDefaultCulture, null, null, null, TreeProvider.ALL_LEVELS, false, null, Node.TreeProvider);
                    }
                }
                return mSourceNode;
            }
        }

        #endregion
    }
}
