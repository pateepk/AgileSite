using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Single export object selection event arguments
    /// </summary>
    public class SingleExportSelectionEventArgs : ExportBaseEventArgs
    {
        /// <summary>
        /// Exported object
        /// </summary>
        public BaseInfo InfoObject
        { 
            get; 
            set; 
        }


        /// <summary>
        /// List of selected objects (code names)
        /// </summary>
        public List<string> SelectedObjects
        {
            get;
            set;
        }
    }
}