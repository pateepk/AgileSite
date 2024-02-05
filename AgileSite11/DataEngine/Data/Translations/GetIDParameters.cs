using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Parameters wrapper for the TranslationHelper.GetID function
    /// </summary>
    public class GetIDParameters
    {
        /// <summary>
        /// Old object ID
        /// </summary>
        public int OldId
        {
            get;
            set;
        }


        /// <summary>
        /// Code name
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Code name column
        /// </summary>
        public string CodeNameColumn
        {
            get;
            set;
        }


        /// <summary>
        /// GUID
        /// </summary>
        public Guid Guid
        {
            get;
            set;
        }


        /// <summary>
        /// GUID column
        /// </summary>
        public string GuidColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID
        /// </summary>
        public int SiteId
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID column
        /// </summary>
        public string SiteIdColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Parent ID
        /// </summary>
        public int ParentId
        {
            get;
            set;
        }


        /// <summary>
        /// Parent ID column
        /// </summary>
        public string ParentIdColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Group ID
        /// </summary>
        public int GroupId
        {
            get;
            set;
        }


        /// <summary>
        /// Group ID column
        /// </summary>
        public string GroupIdColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Additional fields to be loaded
        /// </summary>
        internal ICollection<string> AdditionalFields
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public GetIDParameters()
        {
            AdditionalFields = new string[0];
        }

        /// <summary>
        /// Gets the grouping key for the parameters. The grouping key contains all "parent" IDs which may group the underlying GUIDs and Code names into a common set (parent ID, group ID, site ID, and the column names)
        /// </summary>
        internal string GetGroupingKey()
        {
            return String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", ParentId, ParentIdColumn, GroupId, GroupIdColumn, SiteId, SiteIdColumn, CodeNameColumn);
        }
    }
}