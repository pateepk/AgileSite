using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Base;
using CMS.Polls;

[assembly: RegisterObjectType(typeof(PollListInfo), PollListInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(PollListInfo), PollListInfo.OBJECT_TYPE_GROUP)]

namespace CMS.Polls
{
    internal class PollListInfo : AbstractInfo<PollListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "polls.polllist";


        /// <summary>
        /// Object type for group
        /// </summary>
        public const string OBJECT_TYPE_GROUP = "polls.grouppolllist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, PollInfo.TYPEINFO);


        /// <summary>
        /// Type information for group.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOGROUP = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE_GROUP, PollInfo.TYPEINFOGROUP);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PollListInfo object.
        /// </summary>
        public PollListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PollListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public PollListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                PollInfo pollInfo = new PollInfo();

                CombineColumnNames(pollInfo.ColumnNames, new List<String>() { "AnswerCount" });
            }
            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("polls.poll", "selectallwithanswercount");
        }

        #endregion
    }
}