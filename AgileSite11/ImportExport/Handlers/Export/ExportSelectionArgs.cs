using CMS.Base;
using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Event arguments for events based on object type
    /// </summary>
    public class ExportSelectionArgs : CMSEventArgs
    {
        private bool mSelect = true;
        private bool mIncludeDependingObjects = true;

        private WhereCondition mWhere;


        /// <summary>
        /// Export settings
        /// </summary>
        public SiteExportSettings Settings
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
        /// Where condition.
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
        /// Indicates if site object is being imported
        /// </summary>
        public bool SiteObject
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the selection should be done by the system
        /// </summary>
        public bool Select
        {
            get
            {
                return mSelect;
            }
            set
            {
                mSelect = value;
            }
        }


        /// <summary>
        /// Indicates if the depending object types should be reflected
        /// </summary>
        public bool IncludeDependingObjects
        {
            get
            {
                return mIncludeDependingObjects;
            }
            set
            {
                mIncludeDependingObjects = value;
            }
        }
    }
}