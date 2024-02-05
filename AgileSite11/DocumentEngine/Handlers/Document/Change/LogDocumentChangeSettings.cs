namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for log single document change.
    /// </summary>
    public class LogDocumentChangeSettings : BaseLogDocumentChangeSettings
    {
        #region "Properties"

        /// <summary>
        /// Tree node.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }

        #endregion
    }
}