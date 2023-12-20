using System;
using System.Collections.Generic;

using CMS.Core;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing methods and properties for credit payment.
    /// </summary>
    public class CMSCreditPaymentProvider : CMSPaymentGatewayProvider, IDirectPaymentGatewayProvider
    {
        #region "Constants"

        private const decimal NEUTRAL_EXCHANGE_RATE = 1.0m;

        #endregion


        #region "Protected variables"

        /// <summary>
        /// Available credit in main currency.
        /// </summary>
        protected decimal? mAvailableCreditInMainCurrency;


        /// <summary>
        /// Available credit in order currency.
        /// </summary>
        protected decimal? mAvailableCreditInOrderCurrency;


        /// <summary>
        /// Exchange rate.
        /// </summary>
        protected decimal? mExchangeRate;


        /// <summary>
        /// Main currency data object.
        /// </summary>
        protected CurrencyInfo mMainCurrencyObj;


        /// <summary>
        /// Order currency data object.
        /// </summary>
        protected CurrencyInfo mOrderCurrencyObj;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Main currency info object.
        /// </summary>
        public virtual CurrencyInfo MainCurrencyObj
        {
            get
            {
                if (mMainCurrencyObj == null && ShoppingCartInfoObj != null)
                {
                    mMainCurrencyObj = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrency(ShoppingCartInfoObj.ShoppingCartSiteID);
                }

                return mMainCurrencyObj;
            }
        }


        /// <summary>
        /// Order currency info object.
        /// </summary>
        public virtual CurrencyInfo OrderCurrencyObj
        {
            get
            {
                return ShoppingCartInfoObj?.Currency;
            }
        }


        /// <summary>
        /// Available credit in main currency.
        /// </summary>
        public virtual decimal AvailableCreditInMainCurrency
        {
            get
            {
                return (mAvailableCreditInMainCurrency ?? (mAvailableCreditInMainCurrency = GetAvailableCreditInMainCurrency(ShoppingCartInfoObj))).Value;
            }
        }


        /// <summary>
        /// Available credit in order currency.
        /// </summary>
        public virtual decimal AvailableCreditInOrderCurrency
        {
            get
            {
                return (mAvailableCreditInOrderCurrency ?? (mAvailableCreditInOrderCurrency = GetAvailableCreditInOrderCurrency(ShoppingCartInfoObj))).Value;
            }
        }


        /// <summary>
        /// Gets or sets the action which is then called during <see cref="ReloadPaymentData"/> method call.
        /// </summary>
        public Action ReloadAction
        {
            get;
            set;
        }

        #endregion


        private decimal GetExchangeRateFromMainToCartCurrency(ShoppingCartInfo cart)
        {
            if (cart == null)
            {
                return NEUTRAL_EXCHANGE_RATE;
            }

            var rate = NEUTRAL_EXCHANGE_RATE;
            CurrencyConverter.TryGetExchangeRate(false, cart.Currency.CurrencyCode, cart.ShoppingCartSiteID, ref rate);

            return rate > 0 ? rate : NEUTRAL_EXCHANGE_RATE;
        }


        private decimal GetAvailableCreditInOrderCurrency(ShoppingCartInfo cart)
        {
            if (cart == null)
            {
                return 0m;
            }

            var roundingService = GetRoundingService(cart.ShoppingCartSiteID);
            var availableCreditInMainCurrency = GetAvailableCreditInMainCurrency(cart);
            var rate = GetExchangeRateFromMainToCartCurrency(cart);

            return roundingService.Round(availableCreditInMainCurrency / rate, cart.Currency);
        }


        private decimal GetAvailableCreditInMainCurrency(ShoppingCartInfo cart)
        {
            if (cart == null)
            {
                return 0m;
            }

            var siteID = cart.ShoppingCartSiteID;
            var credit = CreditEventInfoProvider.GetTotalCredit(cart.ShoppingCartCustomerID, siteID);

            // Convert global credit to site main currency when using one
            if (ECommerceSettings.UseGlobalCredit(siteID))
            {
                var globalCurrencyCode = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrencyCode(0);
                var siteMainCurrencyCode = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrencyCode(siteID);

                credit = CurrencyConverter.Convert(credit, globalCurrencyCode, siteMainCurrencyCode, siteID);
            }

            return credit;
        }


        private IRoundingService GetRoundingService(int siteID)
        {
            var roundingServiceFactory = Service.Resolve<IRoundingServiceFactory>();
            return roundingServiceFactory.GetRoundingService(siteID);
        }


        /// <summary>
        /// Credit change after payment.
        /// </summary>
        public virtual decimal CreditChangeAfterPayment
        {
            get
            {
                CurrencyInfo mainCurrency;
                decimal credit;

                var siteID = ShoppingCartInfoObj.ShoppingCartSiteID;

                // Check if using global credit
                if (ECommerceSettings.UseGlobalCredit(siteID))
                {
                    // Get main currency for global credit
                    mainCurrency = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrency(0);
                    var siteMainCurrency = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrency(siteID);

                    credit = CurrencyConverter.Convert(Order.OrderGrandTotalInMainCurrency, siteMainCurrency.CurrencyCode, mainCurrency.CurrencyCode, siteID);
                }
                else
                {
                    // Get main currency for site credit
                    mainCurrency = Service.Resolve<ISiteMainCurrencySource>().GetSiteMainCurrency(siteID);

                    credit = Order.OrderGrandTotalInMainCurrency;
                }

                // Round the result
                var roundingService = GetRoundingService(ShoppingCartInfoObj.ShoppingCartSiteID);

                return roundingService.Round(credit, mainCurrency);
            }
        }


        /// <summary>
        /// Indicates whether customer has enough credit to finish payment.
        /// </summary>
        public virtual bool HasCustomerEnoughCredit(ShoppingCartInfo cart)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            var credit = GetAvailableCreditInOrderCurrency(cart);

            return credit >= cart.GrandTotal;
        }


        /// <summary>
        /// Reloads payment data - order and main currencies, their exchange rates, customer available credit.
        /// </summary>
        public void ReloadPaymentData()
        {
            mMainCurrencyObj = null;
            mOrderCurrencyObj = null;
            mAvailableCreditInMainCurrency = null;
            mAvailableCreditInOrderCurrency = null;
            mExchangeRate = null;
            ReloadAction?.Invoke();
        }


        /// <summary>
        /// Process payment.
        /// </summary>        
        public PaymentResultInfo ProcessPayment(IDictionary<string, object> paymentData)
        {
            if (ShoppingCartInfoObj != null)
            {
                // Skip payment when already completed
                if (IsPaymentCompleted)
                {
                    return PaymentResult;
                }

                // Credit available -> finish payment
                if (HasCustomerEnoughCredit(ShoppingCartInfoObj))
                {
                    // Get current order info
                    var orderInfoObj = OrderInfoProvider.GetOrderInfo(OrderId);
                    if (orderInfoObj != null)
                    {
                        // Update customer credit -> create credit event
                        var creditEvent = new CreditEventInfo
                        {
                            EventCreditChange = -CreditChangeAfterPayment,
                            EventCustomerID = ShoppingCartInfoObj.ShoppingCartCustomerID,
                            EventDate = DateTime.Now,
                            EventName = CreditEventInfoProvider.GetCreditEventName(orderInfoObj),
                            EventDescription = CreditEventInfoProvider.GetCreditEventDescription(orderInfoObj),
                            EventSiteID = ECommerceSettings.UseGlobalCredit(ShoppingCartInfoObj.SiteName) ? 0 : ShoppingCartInfoObj.ShoppingCartSiteID
                        };
                        CreditEventInfoProvider.SetCreditEventInfo(creditEvent);

                        // Display info message to user
                        InfoMessage = ResHelper.GetString("CreditPayment.PaymentCompletedMessage");

                        // Update payment result
                        PaymentResult.PaymentTransactionID = creditEvent.EventID.ToString();
                        PaymentResult.PaymentIsCompleted = true;
                        PaymentResult.PaymentStatusName = "{$PaymentGateway.Result.Status.Completed$}";
                        PaymentResult.PaymentStatusValue = "completed";
                        PaymentResult.PaymentDescription = ResHelper.GetString("CreditPayment.PaymentCompleted");

                        // Update payment result in database
                        UpdateOrderPaymentResult();

                        // Display actual available credit                        
                        ReloadPaymentData();
                    }
                    // Unable to update order payment result
                    else
                    {
                        ErrorMessage = String.Format(ResHelper.GetString("PaymentGatewayProvider.ordernotfound"), OrderId);
                    }
                }
                // Credit not available
                else
                {
                    // Display error message to user
                    ErrorMessage = ResHelper.GetString("CreditPayment.NotEnoughCredit");

                    // Update payment result
                    PaymentResult.PaymentIsCompleted = false;
                    PaymentResult.PaymentStatusName = "{$PaymentGateway.Result.Status.Failed$}";
                    PaymentResult.PaymentStatusValue = "failed";
                    PaymentResult.PaymentDescription = ResHelper.GetString("CreditPayment.Result_NotEnoughCredit");

                    // Update payment result in database
                    UpdateOrderPaymentResult();
                }
            }
            else
            {
                // Order data not found
                ErrorMessage = ResHelper.GetString("PaymentGatewayProvider.OrderDataNotFound");
            }

            return PaymentResult;
        }


        /// <summary>
        /// Checks whether current user is authorized to finish payment, if he is not authorized sets corresponding payment gateway error message.
        /// </summary>
        public virtual bool IsUserAuthorizedToFinishPayment(bool internalOrder = false)
        {
            return IsUserAuthorizedToFinishPayment(MembershipContext.AuthenticatedUser, ShoppingCartInfoObj, internalOrder);
        }


        /// <summary>
        /// Checks whether specified user is authorized to finish payment, if he is not authorized sets corresponding payment gateway error message.
        /// </summary>
        /// <param name="user">User attempting to finish payment.</param>
        /// <param name="cart">Cart to be paid for.</param>
        /// <param name="internalOrder">Indicates if payment is done from administration by store admin in the name of customer.</param>
        public override bool IsUserAuthorizedToFinishPayment(UserInfo user, ShoppingCartInfo cart, bool internalOrder = false)
        {
            if (cart == null)
            {
                return false;
            }

            if ((user == null) || (user.IsPublic()))
            {
                ErrorMessage = ResHelper.GetString("CreditPayment.PaymentNotAllowedToNotRegistered");
                return false;
            }

            var customer = cart.Customer;
            if (customer == null)
            {
                ErrorMessage = ResHelper.GetString("com.creditpayment.creditnotexist");
                return false;
            }

            // User can pay only for her own order on the live site
            if (!internalOrder && (customer.CustomerUserID != user.UserID))
            {
                ErrorMessage = ResHelper.GetString("CreditPayment.NotAuthorizedForPayment");
                return false;
            }
        
            // Allow payment only with enough credit
            if (!HasCustomerEnoughCredit(cart))
            {
                ErrorMessage = ResHelper.GetString("CreditPayment.NotEnoughCredit");
                return false;
            }

            return true;
        }
    }
}