using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of catalog discounts.
    /// </summary>
    internal class CatalogDiscountSource : ICatalogDiscountSource
    {
        /// <summary>
        /// Returns the catalog discounts collection which should be applied for the specified <paramref name="sku"/>.
        /// Applied discounts must be running due to <see cref="PriceParameters.CalculationDate"/>, applicable for the given <see cref="PriceParameters.User"/> and satisfy the discount conditions.
        /// </summary>
        /// <param name="sku">The SKU to get discounts for.</param>
        /// <param name="priceParams">Product price calculation parameters</param>
        public IEnumerable<DiscountInfo> GetDiscounts(SKUInfo sku, PriceParameters priceParams)
        {
            var discounts = GetApplicableCatalogDiscounts(priceParams);

            return GetProductDiscountsFilter().Filter(sku, discounts).OfType<DiscountInfo>();
        }


        /// <summary>
        /// Returns the instance of the <see cref="ProductDiscountsFilter"/> used for catalog discount filtering.
        /// </summary>
        protected virtual ProductDiscountsFilter GetProductDiscountsFilter()
        {
            return new ProductDiscountsFilter();
        }


        private IEnumerable<DiscountInfo> GetApplicableCatalogDiscounts(PriceParameters priceParameters)
        {
            int siteID = priceParameters.SiteID;

            var discounts = CacheHelper.Cache(() => GetCatalogDiscounts(siteID),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "GetCatalogDiscountsForApplicator", siteID)
                {
                    CacheDependency = CacheHelper.GetCacheDependency(new[] { DiscountInfo.OBJECT_TYPE + "|all" })
                });

            return discounts.Where(d => d.IsRunningDueDate(priceParameters.CalculationDate))
                            .Where(d => DiscountMetCustomerRestriction(d, priceParameters));
        }


        private IEnumerable<DiscountInfo> GetCatalogDiscounts(int siteID)
        {
            return DiscountInfoProvider.GetDiscounts().OnSite(siteID)
                                       .WhereTrue("DiscountEnabled")
                                       .WhereEquals("DiscountApplyTo", DiscountApplicationEnum.Products.ToStringRepresentation())
                                       .OrderBy("DiscountOrder")
                                       .ToList();          
        }


        private static bool DiscountMetCustomerRestriction(DiscountInfo discount, PriceParameters parameters)
        {
            var restriction = discount.DiscountCustomerRestriction;
            var discountRoles = discount.DiscountRoles;

            return DiscountRestrictionHelper.CheckCustomerRestriction(restriction, discountRoles, parameters.User, parameters.SiteID);
        }
    }
}