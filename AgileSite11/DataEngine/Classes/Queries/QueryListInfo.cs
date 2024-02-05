using System.Collections.Generic;

using CMS;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(QueryListInfo), QueryListInfo.OBJECT_TYPE_LIST)]

namespace CMS.DataEngine
{
    /// <summary>
    /// Info class providing the list of queries for the query selector
    /// </summary>
    public class QueryListInfo : AbstractInfo<QueryListInfo>
    {
        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE_LIST = "cms.querylist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE_LIST, QueryInfo.TYPEINFO);


        /// <summary>
        /// Query text
        /// </summary>
        public string QueryText
        {
            get; set;
        }


        /// <summary>
        /// Constructor - Creates an empty QueryListInfo object.
        /// </summary>
        public QueryListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery(QueryInfo.OBJECT_TYPE, "selectlist");
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            var columnNames = base.GetColumnNames();
            columnNames.Add("QueryFullName");

            return columnNames;
        }
    }
}
