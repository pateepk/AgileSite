using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(SKUListInfo), "ecommerce.skulist")]

namespace CMS.Ecommerce
{
    /// <summary>
    /// SKUListInfo virtual object.
    /// </summary>
    internal class SKUListInfo : AbstractInfo<ProductListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.skulist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, SKUInfo.TYPEINFOSKU);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SKUListInfo object.
        /// </summary>
        public SKUListInfo()
            : base(TYPEINFO)
        {
        }

        #endregion
    }
}