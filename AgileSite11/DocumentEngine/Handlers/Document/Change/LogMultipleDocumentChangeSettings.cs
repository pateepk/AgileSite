namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for log multiple document change.
    /// </summary>
    public class LogMultipleDocumentChangeSettings : BaseLogDocumentChangeSettings
    {
        #region "Properties"

        /// <summary>
        /// Site name.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Node alias path.
        /// </summary>
        public string NodeAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code.
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition.
        /// </summary>
        public string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if task data should be kept in the objects.
        /// </summary>
        public bool KeepTaskData
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogMultipleDocumentChangeSettings()
            : base()
        {
            CultureCode = TreeProvider.ALL_CULTURES;
        }

        #endregion
    }
}