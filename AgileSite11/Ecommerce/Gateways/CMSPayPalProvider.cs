using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;

using CMS.Core;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;

using PayPal;
using PayPal.Api;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing payment using PayPal payment gateway.
    /// </summary>
    public class CMSPayPalProvider : CMSPaymentGatewayProvider, IDirectPaymentGatewayProvider, IDelayedPaymentGatewayProvider
    {
        /// <summary>
        /// Internal class capable of extracting tax.
        /// </summary>
        private class TaxExtractor
        {
            private readonly ShoppingCartInfo mCart;
            private readonly IDictionary<TaxClassInfo, decimal> mProportions;


            /// <summary>
            /// Creates a new instance of <see cref="TaxExtractor"/>.
            /// </summary>
            /// <param name="cart">Shopping cart object</param>
            public TaxExtractor(ShoppingCartInfo cart)
            {
                mCart = cart;
                mProportions = CalculateProportions();
            }


            private IDictionary<TaxClassInfo, decimal> CalculateProportions()
            {
                var result = new Dictionary<TaxClassInfo, decimal>();

                var taxService = Service.Resolve<ITaxClassService>();

                foreach (var taxClassGroup in mCart.CartProducts.Where(item => taxService.GetTaxClass(item.SKU) != null).GroupBy(item => taxService.GetTaxClass(item.SKU)))
                {
                    result.Add(taxClassGroup.Key, taxClassGroup.Sum(i => i.TotalPrice));
                }

                return result;
            }


            /// <summary>
            /// Extracts tax from given <paramref name="subtotal"/> value.
            /// </summary>
            /// <returns>Value without extracted tax.</returns>
            internal decimal ExtractSubTotal(decimal subtotal)
            {
                var taxCalculationRequest = GetTaxCalculationRequest();
                var taxCalculator = GetTaxCalculationService();

                subtotal = subtotal - mCart.TotalTax + taxCalculator.CalculateTaxes(taxCalculationRequest).ShippingTax;

                return subtotal;
            }


            /// <summary>
            /// Extracts tax from given <paramref name="shipping"/> value.
            /// </summary>
            /// <returns>Value without extracted tax.</returns>
            internal decimal ExtractShipping(decimal shipping)
            {
                var taxCalculationRequest = GetTaxCalculationRequest();
                var taxCalculator = GetTaxCalculationService();

                shipping = shipping - taxCalculator.CalculateTaxes(taxCalculationRequest).ShippingTax;

                return shipping;
            }


            /// <summary>
            /// Extracts tax from given <paramref name="orderDiscount"/> value.
            /// </summary>
            /// <returns>Value without extracted tax.</returns>
            internal decimal ExtractOrderDiscount(decimal orderDiscount)
            {
                var total = mProportions.Sum(i => i.Value);
                var taxes = new List<decimal>();

                foreach (var proportion in mProportions)
                {
                    var address = Service.Resolve<ITaxAddressServiceFactory>().GetTaxAddressService(mCart.ShoppingCartSiteID)
                                         .GetTaxAddress(mCart.ShoppingCartBillingAddress, mCart.ShoppingCartShippingAddress, proportion.Key, mCart.Customer);

                    var estimationParams = new TaxEstimationParameters
                    {
                        Currency = mCart.Currency,
                        Address = address,
                        SiteID = mCart.ShoppingCartSiteID
                    };

                    taxes.Add(Service.Resolve<ITaxEstimationService>().ExtractTax(orderDiscount * (proportion.Value / total), proportion.Key, estimationParams));
                }

                orderDiscount = orderDiscount - taxes.Sum();

                return orderDiscount;
            }


            /// <summary>
            /// Extracts tax from given product <paramref name="price"/> value and shopping <paramref name="cartItem"/>.
            /// </summary>
            /// <returns>Value without extracted tax.</returns>
            internal decimal ExtractItemPrice(decimal price, ShoppingCartItemInfo cartItem)
            {
                var taxCalculationRequest = GetTaxCalculationRequest();
                var taxCalculator = GetTaxCalculationService();

                var tax = Service.Resolve<ITaxClassService>().GetTaxClass(cartItem.SKU);
                if (tax == null)
                {
                    return price;
                }

                var proportion = cartItem.TotalPrice / mProportions[tax];

                var taxItem = new TaxItem
                {
                    SKU = cartItem.SKU,
                    Price = proportion * price,
                    Quantity = cartItem.CartItemUnits
                };
                taxCalculationRequest.Items.Add(taxItem);

                price = price - taxCalculator.CalculateTaxes(taxCalculationRequest).ItemsTax;

                return price;
            }


            /// <summary>
            /// Returns implementation of <see cref="ITaxCalculationService"/> for further tax calculation.
            /// </summary>
            private ITaxCalculationService GetTaxCalculationService()
            {
                return Service.Resolve<ITaxCalculationServiceFactory>().GetTaxCalculationService(mCart.ShoppingCartSiteID);
            }


            /// <summary>
            /// Returns instance of <see cref="TaxCalculationRequest"/> needed for further tax calculation.
            /// </summary>
            private TaxCalculationRequest GetTaxCalculationRequest()
            {
                var taxCalculationRequest = new TaxCalculationRequest
                {
                    Shipping = mCart.ShippingOption,
                    ShippingPrice = mCart.TotalShipping,
                    TaxParameters = new TaxCalculationParameters
                    {
                        Customer = mCart.Customer,
                        Currency = mCart.Currency,
                        SiteID = mCart.ShoppingCartSiteID,
                        ShippingAddress = mCart.ShoppingCartShippingAddress,
                        BillingAddress = mCart.ShoppingCartBillingAddress
                    }
                };

                return taxCalculationRequest;
            }
        }


        private const string TRANSACTION_TYPE_SETTING_NAME = "CMSPayPalTransactionType";
        private const int AUTHORIZE_AND_CAPTURE_SETTING_VALUE = 1;
        private const int NEGATIVE_MULTIPLIER = -1;

        private const string CREATED_STATUS = "created";
        private const string AUTHORIZED_STATUS = "authorized";
        private const string COMPLETED_STATUS = "completed";
        private const string REDIRECT_METHOD = "REDIRECT";
        private const string GET_METHOD = "GET";
        private const string APPROVAL_URL = "approval_url";
        private const string SELF_URL = "self";
        private const string ZERO_VALUE = "0.00";
        private const string PAYMENT_INTENT = "sale";
        private const string AUTHORIZE_INTENT = "authorize";
        private const string PAYPAL_PAYMENT_METHOD = "paypal";

        private readonly ISettingServiceFactory mSettingFactory = Service.Resolve<ISettingServiceFactory>();


        private TaxExtractor Extractor
        {
            get
            {
                var extractor = ShoppingCartInfoObj.ShoppingCartCustomData.GetValue("TaxExtractor") as TaxExtractor;
                if (extractor == null)
                {
                    extractor = new TaxExtractor(ShoppingCartInfoObj);
                    ShoppingCartInfoObj.ShoppingCartCustomData.SetValue("TaxExtractor", extractor);
                }

                return extractor;
            }
        }


        /// <summary>
        /// Gets strongly typed payment result.
        /// </summary>
        protected PayPalPaymentResultInfo PayPalPaymentResult => (PayPalPaymentResultInfo)PaymentResult;


        /// <summary>
        /// Gets PayPal gateway compatible culture.
        /// </summary>
        protected CultureInfo Culture => CultureHelper.EnglishCulture;


        #region "Payment"

        /// <summary>
        /// Creates PayPal payment result object.
        /// </summary>
        protected override PaymentResultInfo CreatePaymentResultInfo()
        {
            return new PayPalPaymentResultInfo();
        }


        /// <summary>
        /// Returns payment status name for given paypal response state.
        /// </summary>
        protected virtual string GetPaymentStatusName(string state)
        {
            return $"{{$PayPal.PaymentResult.Status.{state}$}}";
        }


        /// <summary>
        /// Provides shared exception handling for any method connecting to gateway.
        /// </summary>
        /// <remarks>
        /// Any exception occurred during communication is logged into event log as <see cref="EventType.ERROR"/>.
        /// If any exception occurs, <see cref="PayPalPaymentResult"/> is marked as failed.
        /// </remarks>
        /// <param name="action">Method trying to connect to gateway and performing any payment related operation.</param>
        protected void CallGatewayWithExceptionHandling(Action action)
        {
            const string EVENT_CODE = "PayPal payment error";

            try
            {
                action.Invoke();
            }
            catch (PaymentsException e)
            {
                LogEvent(e.Response, EVENT_CODE);

                PayPalPaymentResult.PaymentIsFailed = true;
                ErrorMessage = ResHelper.GetString("paymentgatewayprovider.unexpectederror");
            }
            catch (HttpException e)
            {
                LogEvent(e.Response, EVENT_CODE);

                PayPalPaymentResult.PaymentIsFailed = true;
                ErrorMessage = ResHelper.GetString("paymentgatewayprovider.connectionfailed");
            }
            catch (Exception e)
            {
                var message = EventLogProvider.GetExceptionLogMessage(e);
                LogEvent(message, EVENT_CODE);

                PayPalPaymentResult.PaymentIsFailed = true;
                ErrorMessage = ResHelper.GetString("paymentgatewayprovider.unexpectederror");
            }
        }


        /// <summary>
        /// Processes the payment on external gateway.
        /// </summary>
        public PaymentResultInfo ProcessPayment(IDictionary<string, object> paymentData)
        {
            CheckOrder();

            if (!PayPalPaymentResult.PaymentIsCompleted)
            {
                CallGatewayWithExceptionHandling(() => ProcessPaymentInternal(paymentData));
                UpdateOrderPaymentResult();
            }

            return PayPalPaymentResult;
        }


        /// <summary>
        /// Process the direct payment.
        /// </summary>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="PayPalPaymentResult"/>.
        /// Any occurred error is logged to event log.
        /// </remarks>
        protected virtual void ProcessPaymentInternal(IDictionary<string, object> paymentData)
        {
            var apiContext = Service.Resolve<IPayPalContextProvider>().GetApiContext(ShoppingCartInfoObj.ShoppingCartSiteID);

            var payment = GetPayment(PAYMENT_INTENT);
            var createdPayment = payment.Create(apiContext);

            PayPalPaymentResult.PaymentId = createdPayment.id;
            PayPalPaymentResult.PaymentStatusValue = createdPayment.state;
            PayPalPaymentResult.PaymentStatusName = GetPaymentStatusName(createdPayment.state);
            PayPalPaymentResult.PaymentDetailUrl = GetDetailUrl(createdPayment);
            PayPalPaymentResult.PaymentApprovalUrl = GetApprovalUrl(createdPayment);

            if (CREATED_STATUS.Equals(createdPayment.state, StringComparison.OrdinalIgnoreCase))
            {
                PayPalPaymentResult.PaymentIsFailed = false;
            }
        }


        /// <summary>
        /// Returns whether both gateway and provider shall use delayed payment method.
        /// </summary>
        public override bool UseDelayedPayment()
        {
            if (ShoppingCartInfoObj == null)
            {
                return base.UseDelayedPayment();
            }

            var settingService = mSettingFactory.GetSettingService(ShoppingCartInfoObj.ShoppingCartSiteID);
            return settingService.GetIntegerValue(TRANSACTION_TYPE_SETTING_NAME) == AUTHORIZE_AND_CAPTURE_SETTING_VALUE;
        }

        #endregion


        #region "Private methods"


        private void CheckPaymentId(string paymentId)
        {
            if (paymentId != PayPalPaymentResult.PaymentId)
            {
                throw new SecurityException("Given payment ID does not match order payment ID.");
            }
        }


        private string CreateItemName(ShoppingCartItemInfo cartItem)
        {
            var localizedItemName = ResHelper.LocalizeString(cartItem.SKU.SKUName);

            if (!cartItem.IsProductOption)
            {
                return localizedItemName.ToNullIfEmpty();
            }

            var parentName = cartItem.ParentProduct?.SKU.SKUName ?? string.Empty;
            var localizedParentName = ResHelper.LocalizeString(parentName);

            var optionCategoryName = cartItem.SKU.SKUOptionCategory?.CategoryDisplayName ?? string.Empty;
            var localizedOptionCategoryName = ResHelper.LocalizeString(optionCategoryName);

            return $"{localizedParentName} - {localizedOptionCategoryName} - {localizedItemName}";
        }

        #endregion


        #region "Authorize & Capture payment"

        /// <summary>
        /// Indicates whether the payment is already authorized meaning that payment capture is possible.
        /// </summary>
        public bool IsPaymentAuthorized => PaymentResult.PaymentIsAuthorized;


        /// <summary>
        /// Authorizes a payment.
        /// </summary>
        /// <param name="paymentData">Additional payment data.</param>
        /// <remarks>
        /// Returned object contains transaction identifier which needs to be persisted to allow further manipulation.
        /// </remarks>
        /// <returns>Instance of <see cref="PaymentResultInfo"/>.</returns>
        public PaymentResultInfo AuthorizePayment(IDictionary<string, object> paymentData)
        {
            CheckOrder();

            if (!PayPalPaymentResult.PaymentIsAuthorized)
            {
                CallGatewayWithExceptionHandling(() => AuthorizePaymentInternal(paymentData));
                UpdateOrderPaymentResult();
            }

            return PayPalPaymentResult;
        }


        /// <summary>
        /// Authorizes a payment.
        /// </summary>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="PayPalPaymentResult"/>.
        /// Any occurred error is logged to event log.
        /// </remarks>
        protected virtual void AuthorizePaymentInternal(IDictionary<string, object> paymentData)
        {
            var apiContext = Service.Resolve<IPayPalContextProvider>().GetApiContext(ShoppingCartInfoObj.ShoppingCartSiteID);

            var payment = GetPayment();
            var createdPayment = payment.Create(apiContext);

            PayPalPaymentResult.PaymentId = createdPayment.id;
            PayPalPaymentResult.PaymentStatusValue = createdPayment.state;
            PayPalPaymentResult.PaymentStatusName = GetPaymentStatusName(createdPayment.state);
            PayPalPaymentResult.PaymentDetailUrl = GetDetailUrl(createdPayment);
            PayPalPaymentResult.PaymentApprovalUrl = GetApprovalUrl(createdPayment);

            if (CREATED_STATUS.Equals(createdPayment.state, StringComparison.OrdinalIgnoreCase))
            {
                PayPalPaymentResult.PaymentIsFailed = false;
            }
        }


        /// <summary>
        /// Captures a payment.
        /// </summary>
        /// <remarks>
        /// Previously received transaction identifier is required to perform a capture of previously authorized transaction.
        /// </remarks>
        /// <returns>Instance of <see cref="PaymentResultInfo"/>.</returns>
        public PaymentResultInfo CapturePayment()
        {
            CheckOrder();

            if (PayPalPaymentResult.PaymentIsCompleted)
            {
                return PayPalPaymentResult;
            }

            CallGatewayWithExceptionHandling(CapturePaymentInternal);
            UpdateOrderPaymentResult();

            return PayPalPaymentResult;
        }


        /// <summary>
        /// Captures a payment.
        /// </summary>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="PayPalPaymentResult"/>.
        /// Any occurred error is logged to event log.
        /// </remarks>
        protected virtual void CapturePaymentInternal()
        {
            var apiContext = Service.Resolve<IPayPalContextProvider>().GetApiContext(ShoppingCartInfoObj.ShoppingCartSiteID);

            var authorization = Authorization.Get(apiContext, PayPalPaymentResult.PaymentAuthorizationID);

            var capture = GetCapture();

            capture = authorization.Capture(apiContext, capture);

            PayPalPaymentResult.PaymentTransactionID = capture.id;
            PayPalPaymentResult.PaymentStatusValue = capture.state;
            PayPalPaymentResult.PaymentStatusName = GetPaymentStatusName(capture.state);
            PayPalPaymentResult.PaymentIsCompleted = COMPLETED_STATUS.Equals(capture.state, StringComparison.OrdinalIgnoreCase);

            if (PayPalPaymentResult.PaymentIsCompleted)
            {
                PayPalPaymentResult.PaymentIsFailed = false;
            }
        }


        /// <summary>
        /// Executes payment for given <paramref name="paymentId"/> and <paramref name="payerId"/>.
        /// Both values has to be obtained from payment gateway.
        /// Payment cannot be captured unless its authorization is confirmed.
        /// </summary>
        public void ExecutePayment(string paymentId, string payerId)
        {
            CheckOrder();

            if (PayPalPaymentResult.PaymentIsCompleted || (UseDelayedPayment() && PayPalPaymentResult.PaymentIsAuthorized))
            {
                return;
            }

            CheckPaymentId(paymentId);
            CallGatewayWithExceptionHandling(() => ExecutePaymentInternal(paymentId, payerId));
            UpdateOrderPaymentResult();
        }


        /// <summary>
        /// Executes payment for given <paramref name="paymentId"/> and <paramref name="payerId"/>.
        /// </summary>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="PayPalPaymentResult"/>.
        /// Any occurred error is logged to event log.
        /// </remarks>
        protected virtual void ExecutePaymentInternal(string paymentId, string payerId)
        {
            var executedPayment = RunPaymentExecution(paymentId, payerId);

            if (IsDirectPayment(executedPayment))
            {
                ProcessExecutedDirectPayment(executedPayment);
            }
            else if (IsDelayedPayment(executedPayment))
            {
                ProcessExecutedDelayedPayment(executedPayment);
            }
        }


        /// <summary>
        /// Runs payment execution for <paramref name="paymentId"/> and <paramref name="payerId"/>.
        /// </summary>
        /// <returns>Executed payment.</returns>
        protected virtual Payment RunPaymentExecution(string paymentId, string payerId)
        {
            var paymentExecution = new PaymentExecution
            {
                payer_id = payerId
            };

            var payment = new Payment
            {
                id = paymentId
            };

            var apiContext = Service.Resolve<IPayPalContextProvider>().GetApiContext(ShoppingCartInfoObj.ShoppingCartSiteID);

            return payment.Execute(apiContext, paymentExecution);
        }


        /// <summary>
        /// Sets payment related data from given <paramref name="payment"/> to <see cref="PayPalPaymentResult"/>.
        /// </summary>
        protected virtual void ProcessExecutedDirectPayment(Payment payment)
        {
            var sale = payment.transactions[0].related_resources[0].sale;

            PayPalPaymentResult.PaymentTransactionID = sale.id;
            PayPalPaymentResult.PaymentStatusValue = sale.state;
            PayPalPaymentResult.PaymentStatusName = GetPaymentStatusName(sale.state);
            PayPalPaymentResult.PaymentIsCompleted = COMPLETED_STATUS.Equals(sale.state, StringComparison.OrdinalIgnoreCase);

            if (PayPalPaymentResult.PaymentIsCompleted)
            {
                PayPalPaymentResult.PaymentIsFailed = false;
            }
        }


        /// <summary>
        /// Sets authorization related data from given <paramref name="payment"/> to <see cref="PayPalPaymentResult"/>.
        /// </summary>
        protected virtual void ProcessExecutedDelayedPayment(Payment payment)
        {
            var authorization = payment.transactions[0].related_resources[0].authorization;

            PayPalPaymentResult.PaymentStatusValue = authorization.state;
            PayPalPaymentResult.PaymentStatusName = GetPaymentStatusName(authorization.state);
            PayPalPaymentResult.PaymentIsAuthorized = AUTHORIZED_STATUS.Equals(authorization.state, StringComparison.OrdinalIgnoreCase);
            PayPalPaymentResult.PaymentAuthorizationID = authorization.id;
        }


        /// <summary>
        /// Returns true when given <paramref name="payment"/> has sale intent.
        /// </summary>
        protected static bool IsDirectPayment(Payment payment)
        {
            return PAYMENT_INTENT.Equals(payment?.intent, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns true when given <paramref name="payment"/> has authorize intent.
        /// </summary>
        protected bool IsDelayedPayment(Payment payment)
        {
            return AUTHORIZE_INTENT.Equals(payment?.intent, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns url for approving the authorization from given <paramref name="payment"/>.
        /// </summary>
        protected string GetApprovalUrl(Payment payment)
        {
            return payment.links.FirstOrDefault(
                i => REDIRECT_METHOD.Equals(i.method, StringComparison.OrdinalIgnoreCase)
                && APPROVAL_URL.Equals(i.rel, StringComparison.OrdinalIgnoreCase))?.href;
        }


        /// <summary>
        /// Returns url with payment detail from given <paramref name="payment"/>.
        /// </summary>
        protected string GetDetailUrl(Payment payment)
        {
            return payment.links.FirstOrDefault(
                i => GET_METHOD.Equals(i.method, StringComparison.OrdinalIgnoreCase)
                && SELF_URL.Equals(i.rel, StringComparison.OrdinalIgnoreCase))?.href;
        }


        /// <summary>
        /// Returns <see cref="Payment"/> created from Order used for payment authorization.
        /// </summary>
        /// <param name="paymentIntent">Payment intention, for more information see https://developer.paypal.com/docs/integration/direct/express-checkout/integration-jsv4/advanced-payments-api/create-express-checkout-payments/?mark=intent.</param>
        protected virtual Payment GetPayment(string paymentIntent = AUTHORIZE_INTENT)
        {
            var payment = new Payment
            {
                intent = paymentIntent,
                payer = new Payer
                {
                    payment_method = PAYPAL_PAYMENT_METHOD,
                    payer_info = GetPayerInfo()
                },
                note_to_payer = Order.OrderNote.ToNullIfEmpty(),
                create_time = Order.OrderDate.ToRfcDateTime(),
                redirect_urls = new RedirectUrls
                {
                    cancel_url = GetCancelUrl(),
                    return_url = GetReturnUrl()
                },
                transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        amount = GetTransactionAmount(),
                        invoice_number = Order.OrderInvoiceNumber.ToNullIfEmpty(),
                        item_list = new ItemList
                        {
                            items = GetPaymentItems(),
                            shipping_address = GetShippingAddress()
                        },
                        custom = Order.OrderGUID.ToString()
                    }
                }
            };

            ValidatePayment(payment);

            return payment;
        }


        /// <summary>
        /// Validates given <paramref name="payment"/> before the gateway request is performed.
        /// </summary>
        /// <remarks>
        /// Method ensures that payment object fulfills all validations performed on gateway side.
        /// </remarks>
        /// <see cref="Payment"/>
        protected virtual void ValidatePayment(Payment payment)
        {
            var transaction = payment.transactions[0];

            var total = ValidationHelper.GetDecimal(transaction.amount.details.subtotal, 0m, Culture);
            var calculatedTotal = 0m;

            foreach (var item in transaction.item_list.items)
            {
                var quantity = ValidationHelper.GetDecimal(item.quantity, 0m, Culture);
                var unitPrice = ValidationHelper.GetDecimal(item.price, 0m, Culture);
                calculatedTotal = calculatedTotal + quantity * unitPrice;
            }

            if (total != calculatedTotal)
            {
                transaction.item_list.items.Add(new Item
                {
                    name = "Tax calculation correction",
                    quantity = 1.ToString(Culture),
                    currency = ShoppingCartInfoObj.Currency.CurrencyCode,
                    price = RoundPrice(NEGATIVE_MULTIPLIER * (calculatedTotal - total))
                });
            }
        }


        /// <summary>
        /// Returns <see cref="Amount"/> object providing money amount information required by gateway.
        /// </summary>
        protected internal virtual Amount GetTransactionAmount()
        {
            return new Amount
            {
                currency = ShoppingCartInfoObj.Currency.CurrencyCode,
                total = RoundPrice(ShoppingCartInfoObj.GrandTotal),
                details = new Details
                {
                    subtotal = RoundPrice(GetTransactionAmountSubtotal()),
                    shipping = RoundPrice(GetTransactionAmountShipping()),
                    tax = RoundPrice(ShoppingCartInfoObj.TotalTax),
                    insurance = ZERO_VALUE,
                    handling_fee = ZERO_VALUE,
                    gift_wrap = ZERO_VALUE
                }
            };
        }


        /// <summary>
        /// Returns subtotal value required by the gateway.
        /// </summary>
        /// <seealso cref="Amount"/>
        protected virtual decimal GetTransactionAmountSubtotal()
        {
            var subtotal = ShoppingCartInfoObj.TotalItemsPrice - ShoppingCartInfoObj.OrderDiscountSummary.GetTotalValue() - ShoppingCartInfoObj.OtherPaymentsSummary.GetTotalValue();

            if (mSettingFactory.GetSettingService(ShoppingCartInfoObj.ShoppingCartSiteID).GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES))
            {
                subtotal = Extractor.ExtractSubTotal(subtotal);
            }

            return Math.Max(0, subtotal);
        }


        /// <summary>
        /// Returns shipping value required by the gateway.
        /// </summary>
        /// <seealso cref="Amount"/>
        protected virtual decimal GetTransactionAmountShipping()
        {
            var shipping = ShoppingCartInfoObj.TotalShipping;

            if (mSettingFactory.GetSettingService(ShoppingCartInfoObj.ShoppingCartSiteID).GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES))
            {
                shipping = Extractor.ExtractShipping(shipping);
            }

            return shipping;
        }


        /// <summary>
        /// Returns collection of <see cref="Item"/> created from order items used for payment authorization.
        /// </summary>
        protected internal virtual List<Item> GetPaymentItems()
        {
            var items = new List<Item>();

            foreach (var scItem in ShoppingCartInfoObj.CartItems)
            {
                items.AddRange(GetProductItems(scItem));
            }

            AddOrderDiscountItem(items);
            AddOtherPaymentItem(items);

            return items;
        }


        /// <summary>
        /// Adds item with negative value of <see cref="ShoppingCartInfo.OrderDiscount"/> into given <paramref name="items"/> collection.
        /// </summary>
        protected virtual void AddOrderDiscountItem(List<Item> items)
        {
            var orderDiscount = ShoppingCartInfoObj.OrderDiscountSummary.GetTotalValue();

            if (orderDiscount > 0m)
            {
                if (mSettingFactory.GetSettingService(ShoppingCartInfoObj.ShoppingCartSiteID).GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES))
                {
                    orderDiscount = Extractor.ExtractOrderDiscount(orderDiscount);
                }

                items.Add(GetItem("Order discount", 1, NEGATIVE_MULTIPLIER * orderDiscount));
            }
        }


        /// <summary>
        /// Adds item with negative value of <see cref="ShoppingCartInfo.OtherPayments"/> into given <paramref name="items"/> collection.
        /// </summary>
        protected virtual void AddOtherPaymentItem(List<Item> items)
        {
            var otherPayments = ShoppingCartInfoObj.OtherPaymentsSummary.GetTotalValue();

            if (otherPayments > 0m)
            {
                items.Add(GetItem("Other payments", 1, NEGATIVE_MULTIPLIER * otherPayments));
            }
        }


        /// <summary>
        /// Returns item created from given parameters.
        /// </summary>
        protected Item GetItem(string name, int quantity, decimal price)
        {
            return new Item
            {
                name = name,
                quantity = quantity.ToString(Culture),
                currency = ShoppingCartInfoObj.Currency.CurrencyCode,
                price = RoundPrice(price)
            };
        }


        /// <summary>
        /// Returns item created from given <paramref name="cartItem"/>.
        /// </summary>
        protected internal IEnumerable<Item> GetProductItems(ShoppingCartItemInfo cartItem)
        {
            if (cartItem == null)
            {
                throw new ArgumentNullException(nameof(cartItem));
            }

            if (cartItem.TotalPriceIncludingOptions + cartItem.TotalDiscount > 0)
            {
                var item = GetItem(CreateItemName(cartItem), cartItem.CartItemUnits, GetItemPrice(cartItem));
                item.sku = cartItem.SKU.SKUNumber.ToNullIfEmpty();
                item.description = HTMLHelper.StripTags(cartItem.SKU.SKUDescription).ToNullIfEmpty();

                yield return item;
            }

            if (cartItem.TotalDiscount > 0)
            {
                var name = cartItem.DiscountSummary.GetFormattedDiscountNames();
                var discountItem = GetItem(name, 1, NEGATIVE_MULTIPLIER * cartItem.TotalDiscount);

                yield return discountItem;
            }
        }


        /// <summary>
        /// Returns unit price of one item required by the gateway.
        /// </summary>
        protected virtual decimal GetItemPrice(ShoppingCartItemInfo cartItem)
        {
            var price = (cartItem.TotalPrice + cartItem.TotalDiscount) / cartItem.CartItemUnits;

            if (mSettingFactory.GetSettingService(ShoppingCartInfoObj.ShoppingCartSiteID).GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES))
            {
                price = Extractor.ExtractItemPrice(price, cartItem);
            }

            return price;
        }


        /// <summary>
        /// Returns information about payer created from order used for payment authorization.
        /// </summary>
        protected virtual PayerInfo GetPayerInfo()
        {
            var payer = new PayerInfo
            {
                billing_address = GetBillingAddress(),
                shipping_address = GetShippingAddress()
            };

            var customer = CustomerInfoProvider.GetCustomerInfo(Order.OrderCustomerID);
            if (customer != null)
            {
                payer.email = customer.CustomerEmail.ToNullIfEmpty();
                payer.first_name = customer.CustomerFirstName.ToNullIfEmpty();
                payer.last_name = customer.CustomerLastName.ToNullIfEmpty();
            }

            return payer;
        }


        /// <summary>
        /// Returns billing address created from order used for payment authorization.
        /// </summary>
        protected virtual Address GetBillingAddress()
        {
            if (Order.OrderBillingAddress == null)
            {
                return null;
            }

            var address = new Address
            {
                city = Order.OrderBillingAddress.AddressCity.ToNullIfEmpty(),
                line1 = Order.OrderBillingAddress.AddressLine1.ToNullIfEmpty(),
                line2 = Order.OrderBillingAddress.AddressLine2.ToNullIfEmpty(),
                country_code = CountryInfoProvider.GetCountryInfo(Order.OrderBillingAddress.AddressCountryID)?.CountryTwoLetterCode.ToNullIfEmpty(),
                state = StateInfoProvider.GetStateInfo(Order.OrderBillingAddress.AddressStateID)?.StateCode.ToNullIfEmpty(),
                postal_code = Order.OrderBillingAddress.AddressZip.ToNullIfEmpty(),
                phone = Order.OrderBillingAddress.AddressPhone.ToNullIfEmpty()
            };

            return address;
        }


        /// <summary>
        /// Returns shipping address created from order used for payment authorization.
        /// </summary>
        protected virtual ShippingAddress GetShippingAddress()
        {
            if (Order.OrderShippingAddress == null)
            {
                return null;
            }

            var address = new ShippingAddress
            {
                city = Order.OrderShippingAddress.AddressCity.ToNullIfEmpty(),
                line1 = Order.OrderShippingAddress.AddressLine1.ToNullIfEmpty(),
                line2 = Order.OrderShippingAddress.AddressLine2.ToNullIfEmpty(),
                country_code = CountryInfoProvider.GetCountryInfo(Order.OrderShippingAddress.AddressCountryID)?.CountryTwoLetterCode.ToNullIfEmpty(),
                state = StateInfoProvider.GetStateInfo(Order.OrderShippingAddress.AddressStateID)?.StateCode.ToNullIfEmpty(),
                postal_code = Order.OrderShippingAddress.AddressZip.ToNullIfEmpty(),
                phone = Order.OrderShippingAddress.AddressPhone.ToNullIfEmpty()
            };

            return address;
        }


        /// <summary>
        /// Returns return url for payment request
        /// </summary>
        protected virtual string GetReturnUrl()
        {
            var settingService = mSettingFactory.GetSettingService(Order.OrderSiteID);

            var url = settingService.GetStringValue("CMSPayPalReturnUrl");
            if (!string.IsNullOrEmpty(url))
            {
                url = URLHelper.GetAbsoluteUrl(url);
                url = PaymentOptionInfoProvider.ResolveUrlMacros(url, ShoppingCartInfoObj);
                url = URLHelper.AddParameterToUrl(url, "pporderid", Order.OrderGUID.ToString());
            }

            return url.ToNullIfEmpty();
        }


        /// <summary>
        /// Returns cancellation url for payment request
        /// </summary>
        protected virtual string GetCancelUrl()
        {
            var settingService = mSettingFactory.GetSettingService(Order.OrderSiteID);

            var url = settingService.GetStringValue("CMSPaypalCancelReturnUrl");
            if (!string.IsNullOrEmpty(url))
            {
                url = URLHelper.GetAbsoluteUrl(url);
                url = PaymentOptionInfoProvider.ResolveUrlMacros(url, ShoppingCartInfoObj);
            }

            return url.ToNullIfEmpty();
        }


        /// <summary>
        /// Returns <see cref="Capture"/> created from Order used for payment capture.
        /// </summary>
        protected virtual Capture GetCapture()
        {
            return new Capture
            {
                is_final_capture = true,
                amount = new Amount
                {
                    currency = ShoppingCartInfoObj.Currency.CurrencyCode,
                    total = RoundPrice(ShoppingCartInfoObj.GrandTotal)
                }
            };
        }

        #endregion
    }
}