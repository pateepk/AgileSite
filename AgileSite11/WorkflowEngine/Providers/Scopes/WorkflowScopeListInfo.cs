using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowScopeListInfo), "cms.workflowscopelist")]

namespace CMS.WorkflowEngine
{
    internal class WorkflowScopeListInfo : AbstractInfo<WorkflowScopeListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowscopelist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, WorkflowScopeInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowScopeListInfo object.
        /// </summary>
        public WorkflowScopeListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowScopeListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WorkflowScopeListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        protected override List<string> GetColumnNames()
        {
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                // Union columns from base object type and joined object types
                WorkflowScopeInfo scopeInfo = new WorkflowScopeInfo();
                DataClassInfo classInfo = DataClassInfo.New();
                SiteInfo siteInfo = new SiteInfo();

                CombineColumnNames(scopeInfo.ColumnNames, classInfo.ColumnNames, siteInfo.ColumnNames);
            }
            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.workflow", "selectallscopes");
        }

        #endregion
    }
}