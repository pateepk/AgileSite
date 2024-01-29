using System;

using CMS.DocumentEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Document import event arguments
    /// </summary>
    public class DocumentImportEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Document being currently imported
        /// </summary>
        public TreeNode Document 
        {
            get; 
            set; 
        }
    }
}