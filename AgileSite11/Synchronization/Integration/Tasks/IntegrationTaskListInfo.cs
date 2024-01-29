using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(IntegrationTaskListInfo), "integration.tasklist")]

namespace CMS.Synchronization
{
    /// <summary>
    /// IntegrationTaskListInfo virtual object.
    /// </summary>
    public class IntegrationTaskListInfo : AbstractInfo<IntegrationTaskListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "integration.tasklist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, IntegrationTaskInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty IntegrationTaskListInfo object.
        /// </summary>
        public IntegrationTaskListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IntegrationTaskListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public IntegrationTaskListInfo(DataRow dr)
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
                // Add columns from base object type
                IntegrationSynchronizationInfo synchronizationInfo = new IntegrationSynchronizationInfo();
                IntegrationConnectorInfo connectorInfo = new IntegrationConnectorInfo();
                IntegrationTaskInfo taskInfo = new IntegrationTaskInfo();

                CombineColumnNames(synchronizationInfo.ColumnNames, connectorInfo.ColumnNames, taskInfo.ColumnNames);
            }

            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("integration.task", "selectlist");
        }

        #endregion
    }
}