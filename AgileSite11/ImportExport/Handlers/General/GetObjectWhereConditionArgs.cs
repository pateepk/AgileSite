using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Event arguments for events based on object type
    /// </summary>
    public class GetObjectWhereConditionArgs : CMSEventArgs
    {
        private bool mCombineWhereCondition = true;
        private WhereCondition mWhere;


        /// <summary>
        /// Import/Export settings
        /// </summary>
        public AbstractImportExportSettings Settings
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
        /// Indicates if site object type is processed
        /// </summary>
        public bool SiteObjects
        {
            get;
            set;
        }
        

        /// <summary>
        /// Object type where condition
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
        /// Indicates if the where condition should be combined
        /// </summary>
        public bool CombineWhereCondition
        {
            get
            {
                return mCombineWhereCondition;
            }
            set
            {
                mCombineWhereCondition = value;
            }
        }
    }
}