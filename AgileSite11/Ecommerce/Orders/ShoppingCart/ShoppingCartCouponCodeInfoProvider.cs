using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="ShoppingCartCouponCodeInfo"/> management.
    /// </summary>
    public class ShoppingCartCouponCodeInfoProvider : AbstractInfoProvider<ShoppingCartCouponCodeInfo, ShoppingCartCouponCodeInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="ShoppingCartCouponCodeInfoProvider"/>.
        /// </summary>
        public ShoppingCartCouponCodeInfoProvider()
            : base(ShoppingCartCouponCodeInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the <see cref="ShoppingCartCouponCodeInfo"/> objects.
        /// </summary>
        public static ObjectQuery<ShoppingCartCouponCodeInfo> GetShoppingCartCouponCodes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="ShoppingCartCouponCodeInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ShoppingCartCouponCodeInfo"/> ID.</param>
        public static ShoppingCartCouponCodeInfo GetShoppingCartCouponCodeInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="ShoppingCartCouponCodeInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ShoppingCartCouponCodeInfo"/> to be set.</param>
        public static void SetShoppingCartCouponCodeInfo(ShoppingCartCouponCodeInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="ShoppingCartCouponCodeInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ShoppingCartCouponCodeInfo"/> to be deleted.</param>
        public static void DeleteShoppingCartCouponCodeInfo(ShoppingCartCouponCodeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="ShoppingCartCouponCodeInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ShoppingCartCouponCodeInfo"/> ID.</param>
        public static void DeleteShoppingCartCouponCodeInfo(int id)
        {
            ShoppingCartCouponCodeInfo infoObj = GetShoppingCartCouponCodeInfo(id);
            DeleteShoppingCartCouponCodeInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes specified <see cref="ShoppingCartCouponCodeInfo"/>.
        /// </summary>
        /// <param name="shoppingCartID">ID of shopping cart which has the coupon code applied.</param>
        /// <param name="couponCode">Code of coupon to be deleted.</param>
        public static void DeleteShoppingCartCouponCodeInfo(int shoppingCartID, string couponCode)
        {
            var infoObj = ProviderObject
                .GetObjectQuery()
                .TopN(1)
                .WhereEquals("ShoppingCartID", shoppingCartID)
                .And()
                .WhereEquals("CouponCode", couponCode)
                .FirstOrDefault();

            if (infoObj != null)
            {
                DeleteShoppingCartCouponCodeInfo(infoObj);
            }
        }

        #endregion
    }
}