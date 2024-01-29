using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents class with support methods for multibuy and product coupon discount sources.
    /// </summary>
    internal abstract class AbstractCartItemDiscountSource
    {
        protected readonly IRoundingServiceFactory mRoundingServiceFactory;
        protected readonly ISiteMainCurrencySource mMainCurrencySource;
        protected readonly ICurrencyConverterFactory mConverterFactory;


        protected AbstractCartItemDiscountSource(IRoundingServiceFactory roundingServiceFactory, ISiteMainCurrencySource mainCurrencySource, ICurrencyConverterFactory converterFactory)
        {
            mRoundingServiceFactory = roundingServiceFactory;
            mMainCurrencySource = mainCurrencySource;
            mConverterFactory = converterFactory;
        }


        protected ICollection<MultiBuyCouponCodeInfo> GetMultiBuyCouponCodes(ICollection<MultiBuyDiscountInfo> discounts, ICollection<string> usedCoupons)
        {
            var codes = new List<MultiBuyCouponCodeInfo>();

            if (usedCoupons.Any())
            {
                codes = MultiBuyCouponCodeInfoProvider.GetMultiBuyCouponCodes()
                   .WhereIn("MultiBuyCouponCodeMultiBuyDiscountID", discounts.Select(d => d.MultiBuyDiscountID).ToList())
                   .WhereIn("MultiBuyCouponCodeCode", usedCoupons)
                   .ToList();
            }

            return codes;
        }


        protected IMultiBuyDiscount GetMultiBuyDiscount(MultiBuyDiscountInfo discount, DiscountsParameters parameters, string appliedCouponCode = null)
        {
            if (!discount.MultiBuyDiscountIsFlat)
            {
                return new MultiBuyDiscount(discount, price => CalculateRelativeDiscount(price, discount, parameters), appliedCouponCode);
            }

            var discountCurrency = mMainCurrencySource.GetSiteMainCurrencyCode(discount.MultiBuyDiscountSiteID);

            var converter = mConverterFactory.GetCurrencyConverter(parameters.SiteID);
            var value = converter.Convert(discount.MultiBuyDiscountValue, discountCurrency, parameters.Currency.CurrencyCode);
            value = RoundValue(value, parameters);

            return new MultiBuyDiscount(discount, value, appliedCouponCode);
        }


        protected static IEnumerable<MultiBuyDiscountInfo> FilterByParameters(IEnumerable<MultiBuyDiscountInfo> discounts, DiscountsParameters parameters)
        {
            return discounts.Where(d => IsValidForDate(d, parameters.DueDate) && MeetsRestriction(d, parameters));
        }


        private static bool IsValidForDate(MultiBuyDiscountInfo discount, DateTime? dueDate)
        {
            // Check time restrictions
            return !dueDate.HasValue || discount.IsValidForDate(dueDate.Value);
        }


        private static bool MeetsRestriction(MultiBuyDiscountInfo discount, DiscountsParameters parameters)
        {
            // Check customer restrictions
            var restriction = discount.MultiBuyDiscountCustomerRestriction;
            var discountRoles = discount.MultiBuyDiscountRoles;

            return DiscountRestrictionHelper.CheckCustomerRestriction(restriction, discountRoles, parameters.User, parameters.SiteID);
        }


        protected static string GetAcceptedCode(MultiBuyDiscountInfo discount, IEnumerable<MultiBuyCouponCodeInfo> coupons, IReadOnlyCouponCodeCollection requestCouponCodes)
        {
            var codes = coupons.Where(c => 
                                    c.MultiBuyCouponCodeMultiBuyDiscountID == discount.MultiBuyDiscountID
                                    && (requestCouponCodes.OrderAppliedCodes.Select(x => x.Code).Contains(c.MultiBuyCouponCodeCode, ECommerceHelper.CouponCodeComparer) 
                                    || !c.UseLimitExceeded))
                                .ToList();

            var alreadyAppliedCode = codes.FirstOrDefault(x => requestCouponCodes.AllAppliedCodes.Select(i => i.Code).Contains(x.MultiBuyCouponCodeCode, ECommerceHelper.CouponCodeComparer));

            if (alreadyAppliedCode != null)
            {
                return alreadyAppliedCode.MultiBuyCouponCodeCode;
            }
            
            return codes.FirstOrDefault()?.MultiBuyCouponCodeCode;
        }


        protected static object GetCacheKeyPart(DiscountsParameters parameters)
        {
            return $"{parameters.SiteID.ObjectID}|{(parameters.Enabled.HasValue ? parameters.Enabled.ToString() : "all")}";
        }


        protected static IEnumerable<MultiBuyDiscountInfo> GetDiscountsForCache(DiscountsParameters parameters, Func<ObjectQuery<MultiBuyDiscountInfo>> getMultiBuyDiscounts)
        {
            var query = getMultiBuyDiscounts();

            // Filter enabled discounts
            if (parameters.Enabled.HasValue)
            {
                query.WhereEquals("MultiBuyDiscountEnabled", parameters.Enabled.Value);
            }

            return query.OrderBy("MultiBuyDiscountPriority").ToList();
        }


        private decimal CalculateRelativeDiscount(decimal price, MultiBuyDiscountInfo discount, DiscountsParameters parameters)
        {
            var rate = discount.MultiBuyDiscountValue / 100m;
            return RoundValue(price * rate, parameters);
        }


        private decimal RoundValue(decimal value, DiscountsParameters parameters)
        {
            var service = mRoundingServiceFactory.GetRoundingService(parameters.SiteID);
            return service.Round(value, parameters.Currency);
        }
    }
}
