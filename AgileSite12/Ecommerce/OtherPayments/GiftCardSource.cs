using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of the <see cref="IGiftCardSource"/> providing gift card applications for other payments calculation.
    /// </summary>
    internal class GiftCardSource : IGiftCardSource
    {
        private readonly ICurrencyConverterFactory mConverterFactory;
        private readonly ISiteMainCurrencySource mSiteMainCurrencySource;
        private readonly IRoundingServiceFactory mRoundingFactory;


        /// <summary>
        /// Creates a new instance of <see cref="GiftCardSource"/>.
        /// </summary>
        /// <param name="converterFactory">Factory for fetching currency converters by site.</param>
        /// <param name="siteMainCurrencySource">Main currency source.</param>
        /// <param name="roundingFactory"></param>
        public GiftCardSource(ICurrencyConverterFactory converterFactory, ISiteMainCurrencySource siteMainCurrencySource, IRoundingServiceFactory roundingFactory)
        {
            mConverterFactory = converterFactory;
            mSiteMainCurrencySource = siteMainCurrencySource;
            mRoundingFactory = roundingFactory;
        }


        /// <summary>
        /// Returns the gift card collection for the specified <paramref name="data"/>.
        /// Applied gift cards must be running, applicable for the given <see cref="CalculationRequest.User"/> and satisfy the gift card conditions.
        /// Only gift cards satisfying the minimum order amount are returned.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Order amount which is used to filter applicable gift cards. (specified in the calculation currency)</param>
        public IEnumerable<GiftCardApplication> GetGiftCards(CalculatorData data, decimal orderAmount)
        {
            var request = data.Request;

            if (!request.CouponCodes.Codes.Any())
            {
                return Enumerable.Empty<GiftCardApplication>();
            }

            // Calculate order price threshold to the main currency
            var mainCurrency = mSiteMainCurrencySource.GetSiteMainCurrencyCode(request.Site);
            var totalPriceInMainCurrency = mConverterFactory.GetCurrencyConverter(request.Site).Convert(orderAmount, request.Currency.CurrencyCode, mainCurrency);

            var applicableGiftCards = GetApplicableGiftCards(data, totalPriceInMainCurrency);

            return CombineWithCouponCodes(applicableGiftCards, request, orderAmount);
        }


        private bool MeetsMacroCondition(GiftCardInfo giftCard, MacroResolver resolver)
        {
            var condition = giftCard.GiftCardCartCondition;

            return String.IsNullOrEmpty(condition) || resolver.ResolveMacros(condition).ToBoolean(false);
        }


        /// <summary>
        /// Combines the specified <paramref name="giftCards"/> with <see cref="CalculationRequest.CouponCodes"/> 
        /// and returns gift card applications.
        /// </summary>
        /// <param name="giftCards">Gift card collection.</param>
        /// <param name="request">Calculation request data.</param>
        /// <param name="orderPrice">Current order price in calculation currency</param>
        private IEnumerable<GiftCardApplication> CombineWithCouponCodes(IList<GiftCardInfo> giftCards, CalculationRequest request, decimal orderPrice)
        {
            var converter = mConverterFactory.GetCurrencyConverter(request.Site);
            var mainCurrency = mSiteMainCurrencySource.GetSiteMainCurrency(request.Site);

            // Ensure that coupon codes are distinct to prevent multiple application
            var requestCouponCodes = request.CouponCodes.Codes
                                            .Where(x => x.ApplicationStatus != CouponCodeApplicationStatusEnum.GiftCardCorrection)
                                            .GroupBy(y => y.Code, ECommerceHelper.CouponCodeComparer)
                                            .Select(z => z.First())
                                            .ToList();

            var couponCodes = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCodes()
                                                    .WhereIn("GiftCardCouponCodeGiftCardID", giftCards.Select(g => g.GiftCardID).ToList())
                                                    .WhereIn("GiftCardCouponCodeCode", requestCouponCodes.Select(x => x.Code).ToList())
                                                    .ToList();

            var discountedOrderPrice = orderPrice;

            // Iterate in order the coupons were added to cart
            foreach (var couponCode in requestCouponCodes)
            {
                var correspondingCoupon = couponCodes.FirstOrDefault(x => ECommerceHelper.CouponCodeComparer.Equals(x.GiftCardCouponCodeCode, couponCode.Code));
             
                if (correspondingCoupon == null)
                {
                    // Skip processing if the coupon is not found (consistency with other types of discounts)
                    continue;
                }

                decimal correspondingCouponValueInMainCurrency = correspondingCoupon.GiftCardCouponCodeRemainingValue;

                bool isAppliedInOrder = couponCode.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder && couponCode.ValueInMainCurrency.HasValue;

                if (isAppliedInOrder)
                {
                    // Increase coupon code applicable value by already applied amount.
                    correspondingCouponValueInMainCurrency += couponCode.ValueInMainCurrency.Value;
                }
                else
                {                                     
                    if (correspondingCouponValueInMainCurrency <= 0m)
                    {
                        // Skip processing if the coupon is exceeded
                        continue;
                    }
                }

                // Convert the coupon value to the request's currency
                var correspondingCouponValue = converter.Convert(correspondingCouponValueInMainCurrency, mainCurrency.CurrencyCode, request.Currency.CurrencyCode);
                correspondingCouponValue = RoundValue(correspondingCouponValue, request.Site, request.Currency);

                var discountValue = Math.Min(discountedOrderPrice, correspondingCouponValue);

                var discountValueInMainCurrency = converter.Convert(discountValue, request.Currency.CurrencyCode, mainCurrency.CurrencyCode);
                discountValueInMainCurrency = RoundValue(discountValueInMainCurrency, request.Site, mainCurrency);

                discountedOrderPrice -= discountValue;

                var giftCard = giftCards.First(x => x.GiftCardID == correspondingCoupon.GiftCardCouponCodeGiftCardID);

                decimal paymentCorrectionInMainCurrency = 0;
                if (isAppliedInOrder && discountValueInMainCurrency != couponCode.ValueInMainCurrency)
                {
                    paymentCorrectionInMainCurrency = discountValueInMainCurrency - couponCode.ValueInMainCurrency.Value;           
                }

                yield return new GiftCardApplication
                {
                    PaymentName = giftCard.GiftCardDisplayName,
                    PaymentValue = discountValue,
                    PaymentValueInMainCurrency = discountValueInMainCurrency,
                    PaymentCorrectionInMainCurrency = paymentCorrectionInMainCurrency,
                    AppliedCode = couponCode.Code,
                    GiftCard = giftCard
                };
            }          
        }


        private decimal RoundValue(decimal value, int siteId, CurrencyInfo currency)
        {
            var service = mRoundingFactory.GetRoundingService(siteId);

            return service.Round(value, currency);
        }


        private List<GiftCardInfo> GetApplicableGiftCards(CalculatorData data, decimal totalPriceInMainCurrency)
        {
            var request = data.Request;
            int siteID = request.Site;

            var resolver = MacroResolver.GetInstance();
            CartDiscountsFilter.ResolverHelper.PrepareResolver(resolver, data);

            var giftCards = CacheHelper.Cache(() => GetGiftCards(siteID),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "GetOrderGiftCards", siteID)
                {
                    CacheDependency = CacheHelper.GetCacheDependency(new[]
                    {
                        GiftCardInfo.OBJECT_TYPE + "|all"
                    })
                });

            return giftCards.Where(card => card.IsRunningDueDate(request.CalculationDate))
                            .Where(card => MeetsRestriction(card, request))
                            .Where(card => card.GiftCardMinimumOrderPrice <= totalPriceInMainCurrency)
                            .Where(card => MeetsMacroCondition(card, resolver))
                            .ToList();
        }


        private static bool MeetsRestriction(GiftCardInfo giftCard, CalculationRequest request)
        {
            var restriction = giftCard.GiftCardCustomerRestriction;
            var roles = giftCard.GiftCardRoles;
            var user = request.User;
            var site = request.Site;

            return DiscountRestrictionHelper.CheckCustomerRestriction(restriction, roles, user, site);
        }


        /// <summary>
        /// Returns all enabled gift cards that are valid now or will be in the future.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <remarks>
        /// In contrast to <see cref="GiftCardInfoProvider.GetRunningGiftCards"/>, this method also returns gift cards that are not valid yet,
        /// but may be valid at the time the collection is retrieved from the cache.
        /// </remarks>
        private IEnumerable<GiftCardInfo> GetGiftCards(int siteID)
        {
            return GiftCardInfoProvider.GetGiftCards()
                                       .OnSite(siteID)
                                       .WhereTrue("GiftCardEnabled");
        }    
    }
}
