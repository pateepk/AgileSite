using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ShippingCostInfo management.
    /// </summary>
    public class ShippingCostInfoProvider : AbstractInfoProvider<ShippingCostInfo, ShippingCostInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the query for all shipping costs.
        /// </summary>
        public static ObjectQuery<ShippingCostInfo> GetShippingCosts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of shipping costs for all weight categories in given shipping option. Result is ordered by min weight.
        /// </summary>
        /// <param name="shippingOptionId">Id of the shipping option</param>
        public static ObjectQuery<ShippingCostInfo> GetShippingCosts(int shippingOptionId)
        {
            return ProviderObject.GetShippingCostsInternal(shippingOptionId);
        }


        /// <summary>
        /// Returns shipping cost with specified ID.
        /// </summary>
        /// <param name="shippingCostId">Shipping cost ID</param>        
        public static ShippingCostInfo GetShippingCostInfo(int shippingCostId)
        {
            return ProviderObject.GetInfoById(shippingCostId);
        }


        /// <summary>
        /// Returns appropriate shipping cost for given shipping option and weight.
        /// </summary>
        /// <param name="shippingId">Id of the shipping option</param>
        /// <param name="weight">Weight of delivery</param>
        public static ShippingCostInfo GetShippingCostInfo(int shippingId, double weight)
        {
            return ProviderObject.GetShippingCostInfoInternal(shippingId, weight);
        }


        /// <summary>
        /// Sets (updates or inserts) specified shipping cost.
        /// </summary>
        /// <param name="shippingCostObj">Shipping cost to be set</param>
        public static void SetShippingCostInfo(ShippingCostInfo shippingCostObj)
        {
            ProviderObject.SetInfo(shippingCostObj);
        }


        /// <summary>
        /// Deletes specified shipping cost.
        /// </summary>
        /// <param name="shippingCostObj">Shipping cost to be deleted</param>
        public static void DeleteShippingCostInfo(ShippingCostInfo shippingCostObj)
        {
            ProviderObject.DeleteInfo(shippingCostObj);
        }


        /// <summary>
        /// Deletes shipping cost with specified ID.
        /// </summary>
        /// <param name="shippingCostId">Shipping cost ID</param>
        public static void DeleteShippingCostInfo(int shippingCostId)
        {
            var shippingCostObj = GetShippingCostInfo(shippingCostId);
            DeleteShippingCostInfo(shippingCostObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns dataset of shipping costs for all weight categories in given shipping option. Result is ordered by min weight descending (from heaviest).
        /// </summary>
        /// <param name="shippingOptionId">Id of the shipping option</param>
        protected virtual ObjectQuery<ShippingCostInfo> GetShippingCostsInternal(int shippingOptionId)
        {
            return GetShippingCosts()
                .Where("ShippingCostShippingOptionID", QueryOperator.Equals, shippingOptionId)
                .OrderByDescending("ShippingCostMinWeight");
        }


        /// <summary>
        /// Returns appropriate shipping cost for given shipping option and weight of delivery.
        /// </summary>
        /// <param name="shippingId">Id of the shipping option</param>
        /// <param name="weight">Weight of delivery</param>
        protected virtual ShippingCostInfo GetShippingCostInfoInternal(int shippingId, double weight)
        {
            var costs = CacheHelper.Cache(
                () => GetShippingCosts(shippingId).ToList(),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "ShippingCostInfoProvider", "CostsForShippingOption", shippingId)
                        {
                            GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { 
                                ShippingOptionInfo.OBJECT_TYPE + "|byid|" + shippingId,
                                CurrencyInfo.OBJECT_TYPE + "|all"
                            })
                        });

            return costs.FirstOrDefault(cost => cost.ShippingCostMinWeight <= weight);
        }

        #endregion
    }
}