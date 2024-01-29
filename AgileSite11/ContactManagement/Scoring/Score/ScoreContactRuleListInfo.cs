using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ScoreContactRuleListInfo), "om.scorecontactrulelist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Support class for view of contact's score detail in scoring
    /// </summary>
    internal class ScoreContactRuleListInfo : AbstractInfo<ScoreContactRuleListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.scorecontactrulelist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ScoreContactRuleListInfo object.
        /// </summary>
        public ScoreContactRuleListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ScoreContactRuleListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ScoreContactRuleListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, RuleInfo.TYPEINFO);
            typeInfo.AllowDataExport = true;

            return typeInfo;
        }


        protected override List<string> GetColumnNames()
        {
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                // View-specific columns
                string[] viewColumns = new string[]{
                        "ScoreID",
                        "ContactID",
                        "Value",
                        "Expiration"
                    };

                // Add columns from base object type
                RuleInfo rule = new RuleInfo();

                CombineColumnNames(viewColumns, rule.ColumnNames);
            }
            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.rule", "selectdatajoined");
        }

        #endregion
    }
}