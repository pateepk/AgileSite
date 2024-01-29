using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export event arguments
    /// </summary>
    public class ExportEventArgs : ExportBaseEventArgs
    {
        /// <summary>
        /// Translation helper
        /// </summary>
        public TranslationHelper TranslationHelper 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Exported data. Available only in ExportObjects.After and ExportDocuments.After handler 
        /// </summary>
        public DataSet Data 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Object type which is exported
        /// </summary>
        public string ObjectType 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Flag whether the system exports the site objects or global objects
        /// </summary>
        public bool SiteObjects 
        { 
            get; 
            set; 
        }
    }
}