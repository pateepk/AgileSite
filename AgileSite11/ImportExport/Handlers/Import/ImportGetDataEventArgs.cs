using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Get import data event arguments
    /// </summary>
    public class ImportGetDataEventArgs : ImportBaseEventArgs
    {
        /// <summary>
        /// Imported data.
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
        /// Empty object which is imported
        /// </summary>
        public BaseInfo Object
        {
            get;
            set;
        }
        

        /// <summary>
        /// Indicates if site object type is imported
        /// </summary>
        public bool SiteObjects
        { 
            get; 
            set; 
        }
          

        /// <summary>
        /// Indicates if data should be get for the selection only
        /// </summary>
        public bool SelectionOnly
        { 
            get; 
            set; 
        }
    }
}