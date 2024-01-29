namespace CMS.DocumentEngine
{
    /// <summary>
    /// Container for settings used when creating new document language version.
    /// </summary>
    public class NewCultureDocumentSettings : BaseDocumentSettings
    {
        #region "variables"

        private bool mCreateVersion = true;
        private bool mAllowCheckOut = true;
        private bool mClearAttachmentFields = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if new version should be created when document is under a workflow.
        /// </summary>
        public bool CreateVersion
        {
            get
            {
                return mCreateVersion;
            }
            set
            {
                mCreateVersion = value;
            }
        }


        /// <summary>
        /// Indicates if document should be checked out when new version is created and document is under a workflow.
        /// </summary>
        public bool AllowCheckOut
        {
            get
            {
                return mAllowCheckOut;
            }
            set
            {
                mAllowCheckOut = value;
            }
        }


        /// <summary>
        /// Indicates if document attachments should be copied from source culture version.
        /// </summary>
        public bool CopyAttachments
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if field attachments should be cleared. Used in Translation module to ensure correct attachments from external translation.
        /// </summary>
        public bool ClearAttachmentFields
        {
            get
            {
                return mClearAttachmentFields;
            }
            set
            {
                mClearAttachmentFields = value;
            }
        }


        /// <summary>
        /// Indicates if document categories should be copied from source culture version.
        /// </summary>
        public bool CopyCategories
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code of new language version. If not specified DocumentCulture from node instance is used.
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Parametric constructor
        /// </summary>
        /// <param name="node">Document node to create new culture version</param>
        /// <param name="cultureCode">New culture code</param>
        /// <param name="tree">Tree provider to use</param>
        public NewCultureDocumentSettings(TreeNode node, string cultureCode, TreeProvider tree = null) :
            base(node, tree)
        {
            CultureCode = cultureCode;
        }


        /// <summary>
        /// Copying constructor
        /// </summary>
        /// <param name="settings">NewCultureDocumentSettings object used as a pattern.</param>
        public NewCultureDocumentSettings(NewCultureDocumentSettings settings) :
            this(settings.Node, settings.CultureCode, settings.Tree)
        {
            CopyAttachments = settings.CopyAttachments;
            CopyCategories = settings.CopyCategories;
            CreateVersion = settings.CreateVersion;
            AllowCheckOut = settings.AllowCheckOut;
            ClearAttachmentFields = settings.ClearAttachmentFields;
        }

        #endregion
    }
}
