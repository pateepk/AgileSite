using System;
using System.Collections.Generic;

using CMS.CMSImportExport;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Documents import event arguments
    /// </summary>
    public class DocumentsImportEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// List of imported document original IDs
        /// </summary>
        public List<int> ImportedDocumentIDs
        {
            get;
            set;
        }

        
        /// <summary>
        /// List of imported node original IDs
        /// </summary>
        public List<int> ImportedNodeIDs
        {
            get;
            set;
        }
    }
}