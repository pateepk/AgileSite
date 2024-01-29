using System.Collections.Generic;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Container for settings used when deleting document.
    /// </summary>
    public class DeleteDocumentSettings : BaseDocumentSettings
    {
        #region "Variables"

        private HashSet<int> mDeletedNodeIDs;
        private bool mDeleteChildNodes = true;
        private bool mAllowRootDeletion;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether all culture version of the specified document are to be deleted.
        /// </summary>
        public bool DeleteAllCultures
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the history of the specified document is to be deleted.
        /// </summary>
        public bool DestroyHistory
        {
            get;
            set;
        }


        /// <summary>
        /// List of the deleted node IDs for cycles checking.
        /// </summary>
        internal HashSet<int> DeletedNodeIDs
        {
            get
            {
                return mDeletedNodeIDs ?? (mDeletedNodeIDs = new HashSet<int>());
            }
            set
            {
                mDeletedNodeIDs = value;
            }
        }


        /// <summary>
        /// Alternating document.
        /// </summary>
        public TreeNode AlternatingDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Maximal level of deleted document which should be used as source for alternating document.
        /// </summary>
        public int AlternatingDocumentMaxLevel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether document aliases should be copied to the alternating document.
        /// </summary>
        public bool AlternatingDocumentCopyAllPaths
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if root document can be deleted as well. By default at least one culture version of the root document is kept to ensure data integrity.
        /// </summary>
        /// <remarks>
        /// This property can be overriden by <see cref="DocumentActionContext.CurrentAllowRootDeletion"/>.
        /// </remarks>
        internal bool AllowRootDeletion
        {
            get
            {
                return mAllowRootDeletion || DocumentActionContext.CurrentAllowRootDeletion;
            }
            set
            {
                mAllowRootDeletion = value;
            }
        }


        /// <summary>
        /// Indicates if child nodes of the document are deleted when the document is deleted. Used to disable children deletion within TreeNode implementation to handle it within DocumentHelper.
        /// </summary>
        internal bool DeleteChildNodes
        {
            get
            {
                return mDeleteChildNodes;
            }
            set
            {
                mDeleteChildNodes = value;
            }
        }


        /// <summary>
        /// Callback function raised when document has been deleted.
        /// </summary>
        internal DocumentHelper.OnAfterDocumentDeletedEventHandler DeleteCallback;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates and initializes DeleteDocumentSettings object.
        /// </summary>
        /// <param name="node">Document node to delete.</param>
        /// <param name="deleteAllCultures">Delete all culture version of the specified document?</param>
        /// <param name="destroyHistory">Destroy the document history?</param>
        /// <param name="tree"><see cref="TreeProvider"/> to use.</param>
        public DeleteDocumentSettings(TreeNode node, bool deleteAllCultures, bool destroyHistory, TreeProvider tree = null)
            : base(node, tree)
        {
            DeleteAllCultures = deleteAllCultures;
            DestroyHistory = destroyHistory;
        }


        /// <summary>
        /// Copying constructor.
        /// </summary>
        /// <param name="settings">DeleteDocumentSetting object used as a pattern.</param>
        public DeleteDocumentSettings(DeleteDocumentSettings settings)
            : this(settings.Node, settings.DeleteAllCultures, settings.DestroyHistory, settings.Tree)
        {
            DeletedNodeIDs = settings.DeletedNodeIDs;
            AlternatingDocument = settings.AlternatingDocument;
            AlternatingDocumentCopyAllPaths = settings.AlternatingDocumentCopyAllPaths;
            AlternatingDocumentMaxLevel = settings.AlternatingDocumentMaxLevel;
            DeleteChildNodes = settings.DeleteChildNodes;
            AllowRootDeletion = settings.AllowRootDeletion;
            DeleteCallback = settings.DeleteCallback;
        }

        #endregion
    }
}
