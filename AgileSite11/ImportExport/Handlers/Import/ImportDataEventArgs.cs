using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import data event arguments
    /// </summary>
    public class ImportDataEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Original import package data. Available only in ImportObjects.After handler
        /// </summary>
        public DataSet Data 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Object type which is imported
        /// </summary>
        public string ObjectType 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Flag whether the system processes the site objects or global objects data
        /// </summary>
        public bool SiteObjects 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Flag whether at least one parent object has been imported.
        /// </summary>
        public bool ParentImported
        {
            get;
            set;
        }
    }
}