using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Container for settings used when copying document.
    /// </summary>
    public class CopyDocumentSettings : BaseDocumentSettings
    {
        #region "Variables"

        private TreeNode mTargetNode;
        private int? mTargetParentNodeId;

        #endregion


        #region "Properties"
        
        /// <summary>
        /// Target node
        /// </summary>
        public TreeNode TargetNode
        {
            get
            {
                if (mTargetParentNodeId != null)
                {
                    mTargetNode = DocumentHelper.GetDocument(mTargetParentNodeId.Value, TreeProvider.ALL_CULTURES, Tree);
                }

                return mTargetNode;
            }
            internal set
            {
                mTargetNode = value;
            }
        }


        /// <summary>
        /// Indicates whether attachments should be copied.
        /// </summary>
        public bool CopyAttachments
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether child nodes should be copied as well.
        /// </summary>
        public bool IncludeChildNodes
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that child nodes are being copied recursively.
        /// </summary>
        public bool ProcessingChildNodes
        {
            get;
            protected set;
        }


        /// <summary>
        /// Owner of the new documents
        /// </summary>
        public int NewDocumentsOwner
        {
            get;
            set;
        }


        /// <summary>
        /// Group of the new documents
        /// </summary>
        public int NewDocumentsGroup
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the document permissions should be copied
        /// </summary>
        public bool CopyPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// New document name
        /// </summary>
        public string NewDocumentName
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true document will be copied only if target site has assigned the culture document has.
        /// </summary>
        public bool CheckSiteCulture
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true SKU bound to copied document will be cloned.
        /// </summary>
        public bool CloneSKU
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public CopyDocumentSettings()
            : this(null, null)
        {
        }


        /// <summary>
        /// Parametric constructor
        /// </summary>
        /// <param name="node">Document node to copy</param>
        /// <param name="target">Target node</param>
        /// <param name="tree">Tree provider to use</param>
        public CopyDocumentSettings(TreeNode node, TreeNode target, TreeProvider tree = null)
            : base(node, tree)
        {
            CopyAttachments = true;
            TargetNode = target;
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="settings">Settings object</param>
        /// <param name="processingChildNodes">Indicates that the instance is created when child nodes are being copied recursively</param>
        public CopyDocumentSettings(CopyDocumentSettings settings, bool processingChildNodes = false)
            : this(settings.Node, settings.TargetNode, settings.Tree)
        {
            mTargetParentNodeId = settings.mTargetParentNodeId;

            ProcessingChildNodes = processingChildNodes;
            IncludeChildNodes = settings.IncludeChildNodes;
            NewDocumentsOwner = settings.NewDocumentsOwner;
            NewDocumentsGroup = settings.NewDocumentsGroup;
            CopyPermissions = settings.CopyPermissions;
            NewDocumentName = settings.NewDocumentName;
            CheckSiteCulture = settings.CheckSiteCulture;
            CloneSKU = settings.CloneSKU;
            CopyAttachments = settings.CopyAttachments;
        }

        #endregion
    }
}
