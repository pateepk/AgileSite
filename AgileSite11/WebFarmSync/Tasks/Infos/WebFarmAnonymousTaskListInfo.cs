using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.WebFarmSync;

[assembly: RegisterObjectType(typeof(WebFarmAnonymousTaskListInfo), "cms.webfarmanonymoustasklist")]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmAnonymousTaskListInfo virtual object.
    /// </summary>
    public class WebFarmAnonymousTaskListInfo : AbstractInfo<WebFarmAnonymousTaskListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmanonymoustasklist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();


        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebFarmAnonymousTaskListInfo object.
        /// </summary>
        public WebFarmAnonymousTaskListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmAnonymousTaskListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WebFarmAnonymousTaskListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, WebFarmTaskInfo.TYPEINFO);
            typeInfo.TypeCondition = new TypeCondition().WhereEquals("TaskIsAnonymous", true);

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "TaskID",
                "TaskType",
                "TaskTextData",
                "TaskCreated",
                "TaskMachineName",
                "TaskErrorMessage"
                );
        }

        #endregion
    }
}