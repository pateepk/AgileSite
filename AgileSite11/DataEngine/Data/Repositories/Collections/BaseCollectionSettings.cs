using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Settings for the base object collection
    /// </summary>
    public abstract class BaseCollectionSettings
    {
        private WhereCondition mWhereCondition;


        /// <summary>
        /// Index of the collection in repository
        /// </summary>
        public int Index
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the collection
        /// </summary>
        public string Name
        {
            get;
            set;
        }



        /// <summary>
        /// Object type
        /// </summary>
        protected internal abstract string ObjectTypeInternal
        {
            get;
        }


        /// <summary>
        /// Nice name of the collection
        /// </summary>
        public string NiceName
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition.
        /// </summary>
        public WhereCondition WhereCondition
        {
            get
            {
                return mWhereCondition ?? (mWhereCondition = new WhereCondition());
            }
            set
            {
                mWhereCondition = value;
            }
        }


        /// <summary>
        /// Dynamic where condition
        /// </summary>
        public Func<string> DynamicWhere
        {
            get;
            set;
        }


        /// <summary>
        /// Order by clause
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Top N items to get
        /// </summary>
        public int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to get from database
        /// </summary>
        public string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Name column
        /// </summary>
        public string NameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Collection name</param>
        public BaseCollectionSettings(string name)
        {
            Name = name;
            SiteID = -1;
        }
    }
}