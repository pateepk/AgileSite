using System.Data;

using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Get export data event arguments
    /// </summary>
    public class ExportGetDataEventArgs : ExportBaseEventArgs
    {
        private WhereCondition mWhere;


        /// <summary>
        /// Translation helper
        /// </summary>
        public TranslationHelper TranslationHelper 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Exported data.
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
        /// Data where condition
        /// </summary>
        public WhereCondition Where
        {
            get
            {
                return mWhere ?? (mWhere = new WhereCondition());
            }
            set
            {
                mWhere = value;
            }
        }
        

        /// <summary>
        /// Indicates if child data should be exported
        /// </summary>
        public bool ChildData
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