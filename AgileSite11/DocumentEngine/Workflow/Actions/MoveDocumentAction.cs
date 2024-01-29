using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for document move action
    /// </summary>
    public class MoveDocumentAction : BaseDocumentAction
    {
        #region "Variables"

        private int? mTargetNodeId = null;

        #endregion


        #region "Parameters"

        /// <summary>
        /// Target alias path
        /// </summary>
        protected virtual string TargetAliasPath
        {
            get
            {
                return GetResolvedParameter<string>("TargetAliasPath", null);
            }
        }


        /// <summary>
        /// Target site name
        /// </summary>
        protected virtual string TargetSiteName
        {
            get
            {
                return GetResolvedParameter<string>("TargetSiteName", Node.NodeSiteName);
            }
        }


        /// <summary>
        /// Indicates if only child documents should be moved
        /// </summary>
        protected virtual bool MoveOnlyChildren
        {
            get
            {
                return GetResolvedParameter<bool>("MoveOnlyChildren", true);
            }
        }


        /// <summary>
        /// Indicates if document permissions should be preserved
        /// </summary>
        protected virtual bool PreservePermissions
        {
            get
            {
                return GetResolvedParameter<bool>("PreservePermissions", false);
            }
        }


        /// <summary>
        /// Target parent node ID
        /// </summary>
        protected int TargetNodeID
        {
            get
            {
                if (mTargetNodeId == null)
                {
                    mTargetNodeId = TreePathUtils.GetNodeIdByAliasPath(TargetSiteName, TargetAliasPath);
                }

                return mTargetNodeId.Value;
            }
        }

        #endregion


        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            // Move document
            if ((TargetNodeID != 0) && (SourceNode != null))
            {
                // Do not copy permissions across sites
                bool crossSite = !SourceSiteName.EqualsCSafe(TargetSiteName);
                bool preservePermissions = PreservePermissions && !crossSite;
                var tree = Node.TreeProvider;
                var parent = DocumentHelper.GetDocument(TargetNodeID, TreeProvider.ALL_CULTURES, tree);

                if (MoveOnlyChildren)
                {
                    // Move only children
                    foreach (var doc in SourceNode.Children)
                    {
                        DocumentHelper.MoveDocument(doc, parent, tree, preservePermissions);
                    }
                }
                else
                {
                    DocumentHelper.MoveDocument(SourceNode, parent, tree, preservePermissions);
                }

                // Refresh node to ensure fresh instance when moved
                RefreshNode();
            }
        }
    }
}
