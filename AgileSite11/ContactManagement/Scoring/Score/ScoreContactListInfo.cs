using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ScoreContactListInfo), ScoreContactListInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Support class for view of contacts in scoring
    /// </summary>
    internal class ScoreContactListInfo : AbstractInfo<ScoreContactListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.scorecontactlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();


        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ScoreContactListInfo object.
        /// </summary>
        public ScoreContactListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ScoreContactListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ScoreContactListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ContactInfo.TYPEINFO);
            typeInfo.AllowDataExport = true;

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                    "ContactID",
                    "ScoreID",
                    "Expiration",
                    "Value"
                   );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.scorecontactrule", "selectall");
        }

        #endregion
    }
}