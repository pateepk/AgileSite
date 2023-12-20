using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export error event arguments
    /// </summary>
    public class ExportErrorEventArgs : ExportBaseEventArgs
    {
        /// <summary>
        /// Exception being thrown by the export process
        /// </summary>
        public Exception Exception 
        {
            get; 
            set; 
        }
    }
}