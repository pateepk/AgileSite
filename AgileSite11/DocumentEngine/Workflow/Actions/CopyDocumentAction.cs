﻿using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for document copy action
    /// </summary>
    public class CopyDocumentAction : BaseDocumentAction
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
        /// Indicates if child documents should be included
        /// </summary>
        protected virtual bool IncludeChildren
        {
            get
            {
                return GetResolvedParameter<bool>("IncludeChildren", true);
            }
        }


        /// <summary>
        /// Indicates if document permissions should be copied
        /// </summary>
        protected virtual bool CopyPermissions
        {
            get
            {
                return GetResolvedParameter<bool>("CopyPermissions", false);
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
            // Copy document
            if ((TargetNodeID != 0) && (SourceNode != null))
            {
                // Do not copy permissions across sites
                bool crossSite = !SourceSiteName.EqualsCSafe(TargetSiteName);
                bool copyPermissions = CopyPermissions && !crossSite;

                var tree = Node.TreeProvider;
                var target = DocumentHelper.GetDocument(TargetNodeID, TreeProvider.ALL_CULTURES, tree);
                var settings = new CopyDocumentSettings(SourceNode, target, tree)
                {
                    IncludeChildNodes = IncludeChildren,
                    CopyPermissions = copyPermissions
                };

                DocumentHelper.CopyDocument(settings);
            }
        }
    }
}
