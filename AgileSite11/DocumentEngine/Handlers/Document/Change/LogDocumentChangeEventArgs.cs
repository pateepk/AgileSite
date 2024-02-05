using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Log document change event arguments
    /// </summary>
    public class LogDocumentChangeEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Log document change settings
        /// </summary>
        public LogDocumentChangeSettings Settings
        {
            get;
            set;
        }
    }
}