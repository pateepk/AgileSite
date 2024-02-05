using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowHistoryListInfo), "cms.workflowhistorylist")]

namespace CMS.WorkflowEngine
{
    internal class WorkflowHistoryListInfo : AbstractInfo<WorkflowHistoryListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowhistorylist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, WorkflowHistoryInfo.TYPEINFO);

        #endregion

        
        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowHistoryListInfo object.
        /// </summary>
        public WorkflowHistoryListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowHistoryListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WorkflowHistoryListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.workflowhistory", "selectdocumenthistory");
        }

        #endregion
    }
}