using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.WebFarmSync;

[assembly: RegisterObjectType(typeof(WebFarmTaskListInfo), "cms.webfarmtasklist")]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmTaskListInfo virtual object.
    /// </summary>
    public class WebFarmTaskListInfo : AbstractInfo<WebFarmTaskListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmtasklist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, WebFarmTaskInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebFarmTaskListInfo object.
        /// </summary>
        public WebFarmTaskListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmTaskListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WebFarmTaskListInfo(DataRow dr)
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
            return CombineColumnNames(
                "CMS_WebFarmTask.TaskID",
                "TaskType",
                "TaskTextData",
                "TaskCreated",
                "TaskMachineName",
                "ServerDisplayName",
                "ErrorMessage"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.webfarmtask", "selectforunigrid");
        }

        #endregion
    }
}