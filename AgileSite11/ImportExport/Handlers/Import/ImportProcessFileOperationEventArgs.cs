using System;
using System.Data;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Process file operation event arguments
    /// </summary>
    public class ImportProcessFileOperationEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// File/Folder operation
        /// </summary>
        public FileOperation Operation
        { 
            get; 
            set; 
        }
    }
}