using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service providing an interface between <see cref="ShoppingCartInfo"/> and the calculation pipeline.
    /// </summary>
    public class ShoppingCartAdapterService : IShoppingCartAdapterService
    {
        /// <summary>
        /// Collects information necessary for the calculation from given <see cref="ShoppingCartInfo"/> and stores it in <see cref="CalculationRequest"/>.
        /// </summary>
        /// <param name="cartInfo">Shopping cart to be calculated</param>
        /// <returns><see cref="CalculationRequest"/> filled with relevant information from given <paramref name="cartInfo"/>.</returns>
        public virtual CalculationRequest GetCalculationRequest(ShoppingCartInfo cartInfo)
        {
            if (cartInfo == null)
            {
                throw new ArgumentNullException(nameof(cartInfo));
            }

            if (cartInfo.ShoppingCartSiteID <= 0)
            {
                throw new ArgumentException("Shopping cart site ID has to be assigned.");
            }

            var request = CreateRequest();

            request.User = cartInfo.User;
            request.Customer = cartInfo.Customer;
            request.Site = cartInfo.ShoppingCartSiteID;
            request.Currency = cartInfo.Currency;
            request.PaymentOption = cartInfo.PaymentOption;
            request.BillingAddress = cartInfo.ShoppingCartBillingAddress;
            request.ShippingAddress = cartInfo.ShoppingCartShippingAddress;
            request.ShippingOption = cartInfo.ShippingOption;
            request.CouponCodes = cartInfo.CouponCodes;
            request.TotalItemsWeight = cartInfo.TotalItemsWeight;
            request.RequestCustomData = cartInfo.ShoppingCartCustomData;
            request.OrderId = cartInfo.OrderId;
            request.CalculationDate = GetCalculationDate(cartInfo);
            request.Items = SelectCartItems(cartInfo.CartItems)
                .Select(SelectItem)
                .ToList()
                .AsReadOnly();

            return request;
        }


        /// <summary>
        /// Creates the <see cref="CalculationResult"/> object used to store calculation result (subtotals, price summaries etc.).
        /// <see cref="CalculationResult.Items"/> collection contains <see cref="CalculationResultItem"/> items.
        /// </summary>
        /// <param name="cartInfo">Shopping cart to be calculated.</param>
        /// <returns>Instance of the <see cref="CalculationResult"/>.</returns>
        public virtual CalculationResult GetCalculationResult(ShoppingCartInfo cartInfo)
        {
            if (cartInfo == null)
            {
                throw new ArgumentNullException(nameof(cartInfo));
            }

            var result = CreateResult();
            result.Items = SelectCartItems(cartInfo.CartItems)
                .Select(GetCalculationResultItem)
                .ToList()
                .AsReadOnly();

            return result;
        }


        /// <summary>
        /// Applies result values to given <see cref="ShoppingCartInfo"/>.
        /// Override this method to change the way the calculation result is applied to the shopping cart.
        /// </summary>
        /// <param name="cartInfo">Shopping cart where the result values should be applied</param>
        /// <param name="result">Container with calculation result values</param>
        public virtual void ApplyCalculationResult(ShoppingCartInfo cartInfo, CalculationResult result)
        {
            if (cartInfo == null)
            {
                throw new ArgumentNullException(nameof(cartInfo));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            ApplyResultToItems(cartInfo.CartItems, result.Items);

            cartInfo.TotalShipping = result.Shipping;
            cartInfo.TotalTax = result.Tax;
            cartInfo.TotalItemsPrice = result.Subtotal;
            cartInfo.OrderDiscount = result.OrderDiscount;
            cartInfo.TotalPrice = result.Total;
            cartInfo.OtherPayments = result.OtherPayments;
            cartInfo.GrandTotal = result.GrandTotal;
            cartInfo.CouponCodes.Merge(result.AppliedCouponCodes);

            // Summaries
            cartInfo.OrderDiscountSummary = result.OrderDiscountSummary;
            cartInfo.TaxSummary = result.TaxSummary;
            cartInfo.OtherPaymentsSummary = result.OtherPaymentsApplications;
        }


        /// <summary>
        /// Creates a new instance of <see cref="CalculationRequest"/>.
        /// Override this method to use a derived request implementation.
        /// </summary>
        protected virtual CalculationRequest CreateRequest()
        {
            return new CalculationRequest();
        }


        /// <summary>
        /// Creates a new instance of <see cref="CalculationRequestItem"/>.
        /// Override this method to use a derived request item implementation.
        /// </summary>
        protected virtual CalculationRequestItem CreateRequestItem()
        {
            return new CalculationRequestItem();
        }


        /// <summary>
        /// Creates a new instance of <see cref="CalculationRequestItemOption"/>.
        /// Override this method to use a derived request item option implementation.
        /// </summary>
        protected virtual CalculationRequestItemOption CreateRequestItemOption()
        {
            return new CalculationRequestItemOption();
        }


        /// <summary>
        /// Creates a new instance of <see cref="CalculationResult"/>.
        /// Override this method to use a derived result implementation.
        /// </summary>
        protected virtual CalculationResult CreateResult()
        {
            return new CalculationResult();
        }


        /// <summary>
        /// Creates a new instance of <see cref="CalculationResultItem"/>.
        /// Override this method to use a derived result item implementation.
        /// </summary>
        protected virtual CalculationResultItem CreateResultItem()
        {
            return new CalculationResultItem();
        }


        /// <summary>
        /// Applies the <paramref name="resultItem"/> to the <paramref name="cartItem"/>.
        /// Override this method to change the way the calculation result is applied to the cart item.
        /// </summary>
        /// <param name="cartItem">Shopping cart item to apply result to.</param>
        /// <param name="resultItem">Result of the calculation to be applied</param>
        protected virtual void ApplyResultToItem(ShoppingCartItemInfo cartItem, CalculationResultItem resultItem)
        {
            cartItem.UnitPrice = resultItem.ItemUnitPrice;
            cartItem.UnitTotalDiscount = resultItem.UnitDiscount;
            cartItem.UnitDiscountSummary = resultItem.UnitDiscountSummary;
            cartItem.TotalDiscount = resultItem.ItemDiscount;
            cartItem.DiscountSummary = resultItem.ItemDiscountSummary;
            cartItem.TotalPrice = resultItem.LineSubtotal;
        }


        private void ApplyResultToItems(ICollection<ShoppingCartItemInfo> cartItems, IEnumerable<CalculationResultItem> resultItems)
        {
            if (resultItems != null)
            {
                var accessories = cartItems.SelectMany(item => item.ProductOptions).Where(option => option.IsAccessoryProduct);

                foreach (var pairedItems in cartItems.Concat(accessories)
                    .Join(resultItems, cartItem => cartItem.CartItemGUID, resultItem => resultItem.ItemGuid, Tuple.Create))
                {
                    ApplyResultToItem(pairedItems.Item1, pairedItems.Item2);
                }
            }
        }


        private static DateTime GetCalculationDate(ShoppingCartInfo cartInfo)
        {
            if (!cartInfo.IsCreatedFromOrder)
            {
                return DateTime.Now;
            }

            return cartInfo.Order.OrderDate;
        }


        private static IEnumerable<ShoppingCartItemInfo> SelectCartItems(IEnumerable<ShoppingCartItemInfo> cartItems)
        {
            return cartItems
                .Where(item => !item.IsBundleItem)
                .Where(IsProduct);
        }


        private CalculationRequestItem SelectItem(ShoppingCartItemInfo item)
        {
            var requestItem = CreateRequestItem();
            requestItem.ItemGuid = item.CartItemGUID;
            requestItem.SKU = item.SKU;
            requestItem.Quantity = item.CartItemUnits;
            requestItem.AutoAddedQuantity = item.CartItemAutoAddedUnits;
            requestItem.Options = SelectOptions(item);
            requestItem.ItemCustomData = item.CartItemCustomData;

            if (item.OrderItem != null)
            {
                var unitPrice = item.OrderItem.OrderItemUnitPrice;
                var discounts = new ValuesSummary(item.OrderItem.OrderItemProductDiscounts);
                var prices = new ProductPrices(unitPrice, unitPrice + discounts.Sum(d => d.Value), discounts);

                // Use stored prices from the order
                requestItem.PriceOverride = prices;
            }

            return requestItem;
        }


        private IEnumerable<CalculationRequestItemOption> SelectOptions(ShoppingCartItemInfo item)
        {
            var options = item.ProductOptions
                              .Where(option => !IsProduct(option) && !IsEmptyTextAttribute(option))
                              .Select(GetOption)
                              .ToList()
                              .AsReadOnly();
            return options;
        }


        private CalculationRequestItemOption GetOption(ShoppingCartItemInfo option)
        {
            var result = CreateRequestItemOption();
            result.OptionGuid = option.CartItemGUID;
            result.SKU = option.SKU;

            return result;
        }


        private CalculationResultItem GetCalculationResultItem(ShoppingCartItemInfo item)
        {
            var result = CreateResultItem();
            result.ItemGuid = item.CartItemGUID;

            return result;
        }


        private static bool IsProduct(ShoppingCartItemInfo item)
        {
            var sku = item.SKU;
            if (sku == null)
            {
                return false;
            }

            return !sku.IsProductOption || sku.IsAccessoryProduct;
        }


        private static bool IsEmptyTextAttribute(ShoppingCartItemInfo option)
        {
            var sku = option.SKU;
            if (sku == null)
            {
                return false;
            }

            return sku.IsTextAttribute && String.IsNullOrEmpty(option.CartItemText);
        }
    }
}