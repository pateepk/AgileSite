using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ProductListInfo), "ecommerce.productlist")]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ProductListInfo virtual object.
    /// </summary>
    internal class ProductListInfo : AbstractInfo<ProductListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.productlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ProductListInfo object.
        /// </summary>
        public ProductListInfo()
            : base(TYPEINFO)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, SKUInfo.TYPEINFOSKU);
            typeInfo.TypeCondition = new TypeCondition().WhereIsNull("SKUOptionCategoryID");

            return typeInfo;
        }

        #endregion
    }
}