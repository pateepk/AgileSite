using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import error event arguments
    /// </summary>
    public class ImportErrorEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Exception being thrown by the import process
        /// </summary>
        public Exception Exception 
        {
            get; 
            set; 
        }
    }
}