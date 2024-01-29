using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing base methods and properties for payment gateway management.
    /// </summary>
    public abstract class CMSPaymentGatewayProvider : IPaymentGatewayProvider
    {
        private const string EVENT_SOURCE = "CMSPaymentGatewayProvider";

        #region "Protected variables"

        /// <summary>
        /// Shopping cart object which stores all data during the checkout process.
        /// </summary>
        protected ShoppingCartInfo mShoppingCartInfoObj;


        /// <summary>
        /// Order object which is going to be paid.
        /// </summary>
        protected OrderInfo mOrder;


        /// <summary>
        /// Payment result.
        /// </summary>
        protected PaymentResultInfo mPaymentResult;


        /// <summary>
        /// Order ID.
        /// </summary>
        protected int mOrderId;


        /// <summary>
        /// Indicates whether payment is already completed.
        /// </summary>
        protected bool? mIsPaymentCompleted;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Shopping cart object which stores all data during the checkout process. If OrderId is set it is created from existing order, otherwise it is returned from current shopping cart control.
        /// </summary>
        public ShoppingCartInfo ShoppingCartInfoObj
        {
            get
            {
                if (mShoppingCartInfoObj == null)
                {
                    // Get shopping cart info object from saved order
                    if (OrderId > 0)
                    {
                        mShoppingCartInfoObj = ShoppingCartInfoProvider.GetShoppingCartInfoFromOrder(OrderId);
                    }
                }
                return mShoppingCartInfoObj;
            }
        }


        /// <summary>
        /// Order ID. Set this value when you want to process payment for the existing order outside the checkout process.
        /// </summary>
        public int OrderId
        {
            get
            {
                if (mOrderId == 0)
                {
                    if ((mShoppingCartInfoObj != null) && (mShoppingCartInfoObj.OrderId > 0))
                    {
                        mOrderId = mShoppingCartInfoObj.OrderId;
                    }
                }
                return mOrderId;
            }
            set
            {
                mOrderId = value;
                mShoppingCartInfoObj = null;
            }
        }


        /// <summary>
        /// Order object which is going to be paid.
        /// </summary>
        public OrderInfo Order
        {
            get
            {
                return mOrder ?? (mOrder = OrderInfoProvider.GetOrderInfo(OrderId));
            }
        }


        /// <summary>
        /// Payment result.
        /// </summary>
        protected PaymentResultInfo PaymentResult
        {
            get
            {
                if (mPaymentResult == null)
                {
                    // Create empty payment result
                    mPaymentResult = CreatePaymentResultInfo();

                    if (OrderId > 0)
                    {
                        // Get corresponding order payment result
                        OrderInfo oi = OrderInfoProvider.GetOrderInfo(OrderId);

                        // Load previous payment result only when order payment method 
                        // is equal to payment provider payment method
                        if ((oi != null) && (oi.OrderPaymentResult != null) && (ShoppingCartInfoObj != null)
                            && (oi.OrderPaymentResult.PaymentMethodID == ShoppingCartInfoObj.ShoppingCartPaymentOptionID))
                        {
                            // Load payment result xml definition
                            string xml = ValidationHelper.GetString(oi.GetValue("OrderPaymentResult"), "");
                            if (xml != "")
                            {
                                mPaymentResult.LoadPaymentResultXml(xml);
                            }
                        }
                    }
                }
                return mPaymentResult;
            }
            set
            {
                mPaymentResult = value;
            }
        }


        /// <summary>
        /// Indicates whether payment is already completed.
        /// </summary>
        public virtual bool IsPaymentCompleted
        {
            get
            {
                if (mIsPaymentCompleted == null)
                {
                    mIsPaymentCompleted = ((PaymentResult.PaymentIsCompleted) && (PaymentResult.PaymentMethodID == ShoppingCartInfoObj.ShoppingCartPaymentOptionID));
                }
                return mIsPaymentCompleted.Value;
            }
        }


        /// <summary>
        /// Payment result message displayed to user when payment succeeds.
        /// </summary>
        public string InfoMessage
        {
            get;
            set;
        } = "";


        /// <summary>
        /// Payment result message displayed to user when payment fails.
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        } = "";

        #endregion

        /// <summary>
        /// Validates payment gateway custom data of the current shopping cart step -  payment gateway form data validation is performed by default.
        /// </summary>
        public virtual string ValidateCustomData(IDictionary<string, object> paymentData)
        {
            return string.Empty;
        }


        /// <summary>
        /// Checks whether specified user is authorized to finish payment.
        /// </summary>
        /// <param name="user">User attempting to finish payment.</param>
        /// <param name="cart">Cart to be paid for.</param>
        /// <param name="internalOrder">Indicates if payment is done from administration by store admin in the name of customer.</param>
        public virtual bool IsUserAuthorizedToFinishPayment(UserInfo user, ShoppingCartInfo cart, bool internalOrder = false)
        {
            return true;
        }


        /// <summary>
        /// Returns whether both gateway and provider shall use delayed payment method.
        /// </summary>
        public virtual bool UseDelayedPayment()
        {
            return false;
        }


        /// <summary>
        /// Returns payment gateway provider instance of given generic type.
        /// </summary>
        /// <param name="paymentOptionId">Payment option ID</param>
        public static TProvider GetPaymentGatewayProvider<TProvider>(int paymentOptionId)
            where TProvider : IPaymentGatewayProvider
        {
            var payment = PaymentOptionInfoProvider.GetPaymentOptionInfo(paymentOptionId);
            return ExistsProviderClass(payment) ? CreatePaymentGatewayProviderInstance<TProvider>(payment) : default(TProvider);
        }


        private static bool ExistsProviderClass(PaymentOptionInfo payment)
        {
            return !string.IsNullOrEmpty(payment?.PaymentOptionAssemblyName) && !string.IsNullOrEmpty(payment.PaymentOptionClassName);
        }


        private static TProvider CreatePaymentGatewayProviderInstance<TProvider>(PaymentOptionInfo payment)
        {
            try
            {
                var provider = ClassHelper.GetClass(payment.PaymentOptionAssemblyName, payment.PaymentOptionClassName);
                if (provider is TProvider)
                {
                    return (TProvider)provider;
                }
            }
            catch (Exception ex)
            {
                var message = EventLogProvider.GetExceptionLogMessage(ex);
                LogEvent(message, "EXCEPTION");
            }

            return default(TProvider);
        }


        #region "Protected methods"

        /// <summary>
        /// Logs error with given <paramref name="message"/>, <paramref name="eventCode"/> and <paramref name="eventSource"/> to event log.
        /// </summary>
        protected static void LogEvent(string message, string eventCode, string eventSource = EVENT_SOURCE)
        {
            try
            {
                var userId = 0;
                var userName = "";

                UserInfo user = MembershipContext.AuthenticatedUser;
                if (user != null)
                {
                    userId = user.UserID;
                    userName = user.UserName;
                }

                // Log the event
                EventLogProvider.LogEvent(EventType.ERROR, eventSource, eventCode, message, RequestContext.CurrentURL, userId, userName, siteId: SiteContext.CurrentSiteID);
            }
            catch
            {
                // Unable to log the event
            }
        }


        /// <summary>
        /// Creates payment result object - base PaymentResultInfo object is created by default.
        /// </summary>
        protected virtual PaymentResultInfo CreatePaymentResultInfo()
        {
            return new PaymentResultInfo();
        }


        /// <summary>
        /// Returns payment gateway url.
        /// </summary>        
        protected virtual string GetPaymentGatewayUrl()
        {
            return PaymentOptionInfoProvider.GetPaymentURL(ShoppingCartInfoObj);
        }


        /// <summary>
        /// Adds some additional information to payment result, such as time stamp and payment method name.
        /// </summary>
        protected virtual void AddAdditionalInfoToPaymentResult()
        {
            // Add time stamp
            PaymentResult.PaymentDate = DateTime.Now;

            // Add payment method name
            if (ShoppingCartInfoObj?.PaymentOption != null)
            {
                PaymentResult.PaymentMethodName = ShoppingCartInfoObj.PaymentOption.PaymentOptionDisplayName;
                PaymentResult.PaymentMethodID = ShoppingCartInfoObj.PaymentOption.PaymentOptionID;
            }
        }


        /// <summary>
        /// Updates order payment result in database.
        /// </summary>
        protected internal virtual string UpdateOrderPaymentResult()
        {
            if (OrderId <= 0)
            {
                // Unable to update order payment result - Order was not specified.
                return ResHelper.GetString("PaymentGatewayProvider.ordernotspecified");
            }

            var order = OrderInfoProvider.GetOrderInfo(OrderId);
            if (order == null)
            {
                // Unable to update order payment result - Order was not found.
                return string.Format(ResHelper.GetString("PaymentGatewayProvider.ordernotfound"), OrderId);
            }

            try
            {
                // Add time stamp and payment method
                AddAdditionalInfoToPaymentResult();

                mIsPaymentCompleted = null;

                order.UpdateOrderStatus(PaymentResult);
            }
            catch (Exception ex)
            {
                // Unable to update order payment result - Payment result update failed.
                EventLogProvider.LogException(EVENT_SOURCE, "UpdateOrderPaymentResult", ex);

                // Return error message
                return ResHelper.GetString("PaymentGatewayProvider.PaymentResultUpdateFailed");
            }

            return "";
        }


        /// <summary>
        /// Check, whether Order and Shopping cart objects are present.
        /// <exception cref="InvalidOperationException">In case Order or ShoppingcartInfo object is not set</exception>
        /// </summary>
        protected void CheckOrder()
        {
            if (Order == null || ShoppingCartInfoObj == null)
            {
                throw new InvalidOperationException(ResHelper.GetString("paymentgatewayprovider.paymentdatanotfound"));
            }
        }


        /// <summary>
        /// Transform given <paramref name="price"/> into string representation according to given <paramref name="formatProvider"/> and <paramref name="formatString"/>.
        /// </summary>
        /// <param name="price">Price to transform</param>
        /// <param name="formatProvider">Formatting information</param>
        /// <param name="formatString">Formatting string</param>
        protected string RoundPrice(decimal price, IFormatProvider formatProvider = null, string formatString = "{0:0.00}")
        {
            formatProvider = formatProvider ?? CultureHelper.EnglishCulture.NumberFormat;

            var roundingService = Service.Resolve<IRoundingServiceFactory>().GetRoundingService(ShoppingCartInfoObj.ShoppingCartSiteID);
            return string.Format(formatProvider, formatString, roundingService.Round(price, ShoppingCartInfoObj.Currency));
        }

        #endregion
    }
}