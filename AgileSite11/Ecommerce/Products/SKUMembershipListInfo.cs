using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(SKUMembershipListInfo), SKUMembershipListInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// List info for displaying products and product options belonging to membership.
    /// </summary>
    public class SKUMembershipListInfo : AbstractInfo<SKUMembershipListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.skumembershiplist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, SKUInfo.TYPEINFOSKU);

        #endregion


        #region "Constructors"


        /// <summary>
        /// Creates an empty instance of the <see cref="SKUMembershipListInfo"/> class.
        /// </summary>
        public SKUMembershipListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SKUMembershipListInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SKUMembershipListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"


        /// <summary>
        /// Gets the default list of column names for this class.
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return mCombinedColumnNames ?? (mCombinedColumnNames = new List<string> { "SKUName", "SKUPrice", "SKUSiteID", "SKUValidity", "SKUValidFor", "SKUValidUntil", "SKUMembershipGUID", "SKUID", "SKUOptionCategoryID" });
        }


        /// <summary>
        /// Gets the data query for this object type.
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            var query = SKUInfoProvider.GetSKUs()
                .WhereNotNull("SKUMembershipGUID");

            return query;
        }

        #endregion
    }
}