using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(MVTestListInfo), "om.mvtestlist")]

namespace CMS.OnlineMarketing
{
    internal class MVTestListInfo : AbstractInfo<MVTestListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.mvtestlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MVTestListInfo object.
        /// </summary>
        public MVTestListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MVTestListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MVTestListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, MVTestInfo.TYPEINFO);
            typeInfo.ModuleName = ModuleName.ONLINEMARKETING;

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "MVTestID",
                "MVTestName",
                "MVTestDisplayName",
                "MVTestCulture",
                "MVTestPage",
                "MVTestOpenFrom",
                "MVTestOpenTo",
                "MVTestEnabled",
                "MVTestConversions",
                "MVTestSiteID",
                "HitsValue"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.mvtest", "selectwithhits");
        }

        #endregion
    }
}