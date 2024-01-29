using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce.AuthorizeNetDataContracts;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;

using Address = CMS.Ecommerce.AuthorizeNetDataContracts.Address;
using CreditCard = CMS.Ecommerce.AuthorizeNetDataContracts.CreditCard;
using Order = CMS.Ecommerce.AuthorizeNetDataContracts.Order;
using Payment = CMS.Ecommerce.AuthorizeNetDataContracts.Payment;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing payment using Authorize.NET payment gateway.
    /// </summary>
    public class CMSAuthorizeNetProvider : CMSPaymentGatewayProvider, IDirectPaymentGatewayProvider, IDelayedPaymentGatewayProvider
    {
        private const string ERROR_CODE = "Authorize.NET payment error";
        private const string API_LOGINID_SETTING_NAME = "CMSAuthorizeNETAPILogin";
        private const string API_TRANSACTIONKEY_SETTING_NAME = "CMSAuthorizeNETTransactionKey";
        private const string TRANSACTION_TYPE_SETTING_NAME = "CMSAuthorizeNETTransactionType";
        private const int AUTHORIZE_ONLY_SETTING_VALUE = 1;

        private readonly ISettingServiceFactory mSettingFactory = Service.Resolve<ISettingServiceFactory>();

        
        /// <summary>
        /// Gets strongly typed payment result.
        /// </summary>
        protected AuthorizeNetPaymentResultInfo AuthorizeNetPaymentResult => (AuthorizeNetPaymentResultInfo)PaymentResult;


        /// <summary>
        /// Creates Authorize.Net payment result object.
        /// </summary>
        protected override PaymentResultInfo CreatePaymentResultInfo()
        {
            return new AuthorizeNetPaymentResultInfo();
        }


        /// <summary>
        /// Indicates whether the payment is already authorized meaning that payment capture is possible.
        /// </summary>
        public bool IsPaymentAuthorized => PaymentResult.PaymentIsAuthorized;


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
            return settingService.GetIntegerValue(TRANSACTION_TYPE_SETTING_NAME) == AUTHORIZE_ONLY_SETTING_VALUE;
        }


        /// <summary>
        /// Validates credit card <paramref name="paymentData"/> submitted by user.
        /// </summary>
        /// <param name="paymentData">Data to validate</param>
        /// <returns>Error messages if <paramref name="paymentData"/> are not valid.</returns>
        public override string ValidateCustomData(IDictionary<string, object> paymentData)
        {
            var sb = new StringBuilder();

            if (!MatchesRegex(paymentData?[AuthorizeNetParameters.CARD_NUMBER], "^[0-9]{13,16}$"))
            {
                sb.AppendLine(ResHelper.GetString("authorizenetform.errorcreditcardnumber"));
            }

            if (!MatchesRegex(paymentData?[AuthorizeNetParameters.CARD_CCV], "^[0-9]{3,4}$"))
            {
                sb.AppendLine(ResHelper.GetString("authorizenetform.errorcreditcardccv"));
            }

            // Check format of expiration
            if (!IsExpirationValid(paymentData?[AuthorizeNetParameters.CARD_EXPIRATION]))
            {
                sb.AppendLine(ResHelper.GetString("authorizenetform.errorcreditcardexpiration"));
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns whether given <paramref name="data"/> matches <paramref name="regexPattern"/>.
        /// </summary>
        /// <param name="data">Object containing data to check</param>
        /// <param name="regexPattern">Regular expression pattern to match</param>
        protected virtual bool MatchesRegex(object data, string regexPattern)
        {
            var regex = RegexHelper.GetRegex(regexPattern);

            return regex.IsMatch(ValidationHelper.GetString(data, ""));
        }


        /// <summary>
        /// Validates whether <paramref name="expiration"/> is valid.
        /// </summary>
        /// <param name="expiration">Object containing expiration</param>
        /// <returns>Whether expiration is valid.</returns>
        protected virtual bool IsExpirationValid(object expiration)
        {
            var expirationValue = ValidationHelper.GetDateTime(expiration, DateTimeHelper.ZERO_TIME);

            return !DateTime.Equals(expirationValue, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Proceses the payment in external gateway directly.
        /// </summary>
        /// <param name="paymentData">Additional payment data containing credit card data.</param>
        /// <returns>
        /// Instance of <see cref="PaymentResultInfo"/>.
        /// </returns>
        /// <seealso cref="CMSPaymentGatewayProvider.CreatePaymentResultInfo"/>
        public PaymentResultInfo ProcessPayment(IDictionary<string, object> paymentData)
        {
            CheckOrder();

            ProcessPaymentInternal(paymentData);

            UpdateOrderPaymentResult();

            return AuthorizeNetPaymentResult;
        }


        /// <summary>
        /// Proceses the payment in external gateway directly.
        /// </summary>
        /// <param name="paymentData">Additional payment data containing credit card data.</param>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="AuthorizeNetPaymentResult"/>.
        /// Any occured error is logged to event log.
        /// </remarks>
        protected virtual void ProcessPaymentInternal(IDictionary<string, object> paymentData)
        {
            var authorizeAndCaptureRequest = GetPaymentRequest(paymentData, TransactionType.AuthCaptureTransaction);

            var response = ProcessRequest(authorizeAndCaptureRequest);

            // Error occured while processing request on gateway
            if (TryProcessError(response))
            {
                AuthorizeNetPaymentResult.PaymentIsFailed = true;
                return;
            }

            // Process response results
            var transactionResponse = response.TransactionRespone.TransactionResponse;

            AuthorizeNetPaymentResult.PaymentIsFailed = false;
            AuthorizeNetPaymentResult.PaymentIsCompleted = true;
            AuthorizeNetPaymentResult.PaymentTransactionID = transactionResponse.TransId;
            AuthorizeNetPaymentResult.AuthorizationCode = transactionResponse.AuthCode;
            UpdatePaymentInfo(response.TransactionRespone);
        }


        /// <summary>
        /// Authorizes a payment.
        /// </summary>
        /// <param name="paymentData">Additional payment data containing credit card data.</param>
        /// <remarks>
        /// Returned object contains transaction identifier which needs to be persisted to allow further manipulation.
        /// </remarks>
        /// <returns>Instance of <see cref="PaymentResultInfo"/>.</returns>
        public PaymentResultInfo AuthorizePayment(IDictionary<string, object> paymentData)
        {
            CheckOrder();

            AuthorizePaymentInternal(paymentData);

            UpdateOrderPaymentResult();

            return AuthorizeNetPaymentResult;
        }


        /// <summary>
        /// Authorizes a payment.
        /// </summary>
        /// <param name="paymentData">Additional payment data containing credit card data.</param>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="AuthorizeNetPaymentResult"/>.
        /// Any occured error is logged to event log.
        /// </remarks>
        protected virtual void AuthorizePaymentInternal(IDictionary<string, object> paymentData)
        {
            var authorizePaymentRequest = GetPaymentRequest(paymentData, TransactionType.AuthOnlyTransaction);

            var response = ProcessRequest(authorizePaymentRequest);

            // Error occured while processing request on gateway
            if (TryProcessError(response))
            {
                AuthorizeNetPaymentResult.PaymentIsFailed = true;
                return;
            }

            // Process response results
            var transactionResponse = response.TransactionRespone.TransactionResponse;

            AuthorizeNetPaymentResult.PaymentIsFailed = false;
            AuthorizeNetPaymentResult.PaymentIsAuthorized = true;
            AuthorizeNetPaymentResult.PaymentAuthorizationID = transactionResponse.TransId;
            AuthorizeNetPaymentResult.AuthorizationCode = transactionResponse.AuthCode;
            UpdatePaymentInfo(response.TransactionRespone);
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

            // Check whether order is authorized
            if (!mOrder.OrderIsAuthorized || string.IsNullOrWhiteSpace(mOrder.OrderPaymentResult.PaymentAuthorizationID))
            {
                AuthorizeNetPaymentResult.PaymentIsFailed = true;
                AuthorizeNetPaymentResult.PaymentIsCompleted = false;

                ErrorMessage = $"{ResHelper.GetString("authorizenet.provider.capturenotauthorizedpayment")}";

                LogEvent(ErrorMessage, ERROR_CODE);

                return AuthorizeNetPaymentResult;
            }

            CapturePaymentInternal();

            UpdateOrderPaymentResult();

            return AuthorizeNetPaymentResult;
        }


        /// <summary>
        /// Captures a payment.
        /// </summary>
        /// <remarks>
        /// Method stores the gateway response data into <see cref="AuthorizeNetPaymentResult"/>.
        /// Any occured error is logged to event log.
        /// </remarks>
        protected virtual void CapturePaymentInternal()
        {
            var capturePaymentRequest = GetCapturePaymentRequest();

            var response = ProcessRequest(capturePaymentRequest);

            // Error occured while processing request on gateway
            if (TryProcessError(response))
            {
                AuthorizeNetPaymentResult.PaymentIsFailed = true;
                return;
            }

            // Process response results
            AuthorizeNetPaymentResult.PaymentIsFailed = false;
            AuthorizeNetPaymentResult.PaymentIsCompleted = true;
            AuthorizeNetPaymentResult.PaymentTransactionID = response.TransactionRespone.TransactionResponse.TransId;
            UpdatePaymentInfo(response.TransactionRespone);
        }


        /// <summary>
        /// Get information about payment status and payment description.
        /// </summary>
        /// <param name="response">Response from which to retrieve information</param>
        protected virtual void UpdatePaymentInfo(CreateTransactionResponse response)
        {
            AuthorizeNetPaymentResult.PaymentStatusName = GetTransactionResponseName(response.TransactionResponse.ResponseCode);
            AuthorizeNetPaymentResult.PaymentStatusValue = ValidationHelper.GetString(response.TransactionResponse.ResponseCode, "", CultureInfo.InvariantCulture);

            var sbDescription = new StringBuilder();
            sbDescription.AppendLine(GetMainMessageTextRepresentation(response.MainMessages.MainMessage, true));

            if (response.TransactionResponse.MessagesMultiple != null)
            {
                foreach (var message in response.TransactionResponse.MessagesMultiple)
                {
                    sbDescription.AppendLine($"{message.Code}: {message.Description}");
                }
            }

            AuthorizeNetPaymentResult.PaymentDescription = sbDescription.ToString();
        }


        /// <summary>
        /// Serializes and send payment request to Authorize.NET gateway, then receive and deserialize returned response.
        /// </summary>
        /// <param name="request">Request object</param>
        /// <returns>Instance of <see cref="ResponseResult"/>.</returns>
        /// <remarks>
        /// Any occured error is logged to event log and stored in <see cref="AuthorizeNetPaymentResult"/>.
        /// </remarks>
        protected virtual ResponseResult ProcessRequest(CreateTransactionRequest request)
        {
            var responseResult = new ResponseResult();

            // Check whether gateway URL is set
            var paymentGatewayURL = GetPaymentGatewayUrl();
            if (String.IsNullOrWhiteSpace(paymentGatewayURL))
            {
                ErrorMessage = $"{ResHelper.GetString("authorizenet.provider.gatewayurl.error")}";
                LogEvent(ErrorMessage, ERROR_CODE);
                AuthorizeNetPaymentResult.PaymentIsFailed = true;

                return responseResult;
            }

            try
            {
                // Send request to Authorize.NET gateway
                string xmlResponse;
                using (var webClient = new WebClient())
                {
                    xmlResponse = webClient.UploadString(paymentGatewayURL, "POST", SerializeRequest(request));
                }

                // If specific kind of error occures, ErrorResponse is returned
                if (xmlResponse.Contains("<ErrorResponse"))
                {
                    responseResult.ErrorResponse = DeserializeResponse<ErrorResponse>(xmlResponse);
                }
                else
                {
                    // CreateTransactionResponse is returned - Deserialize it
                    responseResult.TransactionRespone = DeserializeResponse<CreateTransactionResponse>(xmlResponse);
                }
            }
            catch (Exception e)
            {
                LogEvent(EventLogProvider.GetExceptionLogMessage(e), ERROR_CODE);
                AuthorizeNetPaymentResult.PaymentIsFailed = true;

                ErrorMessage = $"{ResHelper.GetString("paymentgatewayprovider.unexpectederror")}";
            }

            return responseResult;
        }


        /// <summary>
        /// Process XML response and return object containing data from response.
        /// </summary>
        /// <param name="response">Response in XML format</param>
        protected T DeserializeResponse<T>(string response)
        {
            return Service.Resolve<IDataContractSerializerService<T>>().Deserialize(response);
        }


        /// <summary>
        /// Check whether response from gateway is <see cref="ErrorResponse"/> or <see cref="TransactionResponse"/> containing errors. Create error message and description.
        /// </summary>
        /// <param name="response">Deserialized response from gateway</param>
        /// <returns>False if request doen't contain any errors, otherwise true.</returns>
        /// <remarks>
        /// Any occured error is logged to event log and stored in <see cref="AuthorizeNetPaymentResult"/>.
        /// </remarks>
        protected virtual bool TryProcessError(ResponseResult response)
        {
            // No error in response
            var transactionResponse = response.TransactionRespone;
            if ((transactionResponse != null) && (transactionResponse.MainMessages.ResultCode == MainMessagesResultCode.Ok) && (transactionResponse.TransactionResponse.Errors == null))
            {
                return false;
            }

            // Response is ErrorResponse
            if (response.ErrorResponse != null)
            {
                ErrorMessage = $"{ResHelper.GetString("paymentgatewayprovider.unexpectederror")}";

                var mainErrorMessage = GetMainMessageTextRepresentation(response.ErrorResponse.MainMessages.MainMessage, true);
                AuthorizeNetPaymentResult.PaymentDescription = mainErrorMessage;
                LogEvent(mainErrorMessage, ERROR_CODE);
            }

            // Response is TransactionResponse containing errors
            if (transactionResponse != null)
            {
                var sbMessage = new StringBuilder();
                var sbDescription = new StringBuilder();

                sbDescription.AppendLine(GetMainMessageTextRepresentation(transactionResponse.MainMessages.MainMessage, true));

                if (transactionResponse.TransactionResponse.Errors != null)
                {
                    foreach (var error in transactionResponse.TransactionResponse.Errors)
                    {
                        sbMessage.AppendLine($"{error.ErrorText}");
                        sbDescription.AppendLine($"{error.ErrorCode}: {error.ErrorText}");
                    }
                }
                else
                {
                    // Main message shall be used, only if there is no detailed description.
                    sbMessage.AppendLine(GetMainMessageTextRepresentation(transactionResponse.MainMessages.MainMessage));
                }

                ErrorMessage = sbMessage.ToString();
                AuthorizeNetPaymentResult.PaymentDescription = sbDescription.ToString();
            }

            return true;
        }


        /// <summary>
        /// Get string representation of information in <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Message from which to get information</param>
        /// <param name="includeCode">Whether to include code number</param>
        protected virtual string GetMainMessageTextRepresentation(MainMessage message, bool includeCode = false)
        {
            return includeCode ? $"{message.Code}: {message.Text}" : $"{message.Text}";
        }


        /// <summary>
        /// Returns transaction response name for given code.
        /// </summary>
        /// <param name="code">Code of transaction response</param>
        protected virtual string GetTransactionResponseName(TransactionResponseCode code)
        {
            return $"{{$authorizenet.provider.transactionresponsename.{code.ToStringRepresentation()}$}}";
        }


        /// <summary>
        /// Creates request object for performing payment.
        /// </summary>
        /// <param name="paymentData">Additional payment data containing credit card data.</param>
        /// <param name="transactionType"><see cref="TransactionType.AuthCaptureTransaction"/> for direct payment request. <see cref="TransactionType.AuthOnlyTransaction"/> for authorize payment request</param>
        /// <returns>Instance of <see cref="CreateTransactionRequest"/>.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="paymentData"/>is null or doesn't contain proper data <see cref="AuthorizeNetParameters"/>.> </exception>
        protected virtual CreateTransactionRequest GetPaymentRequest(IDictionary<string, object> paymentData, TransactionType transactionType)
        {
            if ((paymentData == null) || !paymentData.ContainsKey(AuthorizeNetParameters.CARD_EXPIRATION) || !paymentData.ContainsKey(AuthorizeNetParameters.CARD_NUMBER) || !paymentData.ContainsKey(AuthorizeNetParameters.CARD_CCV))
            {
                throw new InvalidOperationException(ResHelper.GetString("paymentgatewayprovider.paymentdatanotfound"));
            }

            var request = new CreateTransactionRequest();
            request.MerchantAuthentication = GetMerchantAuthentication();
            request.RefId = ValidationHelper.GetString(Order.OrderID, "", CultureInfo.InvariantCulture);

            request.TransactionRequest = new TransactionRequest();
            request.TransactionRequest.TransactionType = transactionType;
            request.TransactionRequest.Amount = RoundPrice(Order.OrderGrandTotal);
            request.TransactionRequest.Payment = GetPayment(paymentData);
            request.TransactionRequest.Order = GetOrder();
            request.TransactionRequest.Tax = GetPartialAmount(RoundPrice(Order.OrderTotalTax));

            var shipOption = ShippingOptionInfoProvider.GetShippingOptionInfo(Order.OrderShippingOptionID);
            var shipAmount = RoundPrice(Order.OrderTotalShipping);
            request.TransactionRequest.Shipping = GetPartialAmount(shipAmount, shipOption?.ShippingOptionDisplayName.ToNullIfEmpty(), shipOption?.ShippingOptionDescription.ToNullIfEmpty());

            request.TransactionRequest.PONumber = ValidationHelper.GetString(Order.OrderID, "", CultureInfo.InvariantCulture);
            request.TransactionRequest.BillTo = GetAddress(Order.OrderBillingAddress);
            request.TransactionRequest.ShipTo = GetAddress(Order.OrderShippingAddress);
            request.TransactionRequest.Customer = GetCustomer();

            return request;
        }


        /// <summary>
        /// Creates requet object for capturing authorized payment.
        /// </summary>
        /// <returns>Instance of <see cref="CreateTransactionRequest"/>.</returns>
        protected virtual CreateTransactionRequest GetCapturePaymentRequest()
        {
            var request = new CreateTransactionRequest();

            request.MerchantAuthentication = GetMerchantAuthentication();

            request.RefId = ValidationHelper.GetString(Order.OrderID, "", CultureInfo.InvariantCulture);

            request.TransactionRequest = new TransactionRequest();
            request.TransactionRequest.TransactionType = TransactionType.PriorAuthCaptureTransaction;
            request.TransactionRequest.Amount = RoundPrice(Order.OrderGrandTotal);
            request.TransactionRequest.RefTransId = Order.OrderPaymentResult.PaymentAuthorizationID;
            request.TransactionRequest.Order = GetOrder();

            return request;
        }


        /// <summary>
        /// Serializes given request object.
        /// </summary>
        /// <param name="request">Request to serialize</param>
        /// <returns>XML representation of given request.</returns>
        protected string SerializeRequest<T>(T request)
        {
            var serializerService = Service.Resolve<IDataContractSerializerService<T>>();

            return serializerService.Serialize(request);
        }


        /// <summary>
        /// Creates authenttication for payment request.
        /// </summary>
        /// <returns>Instance of <see cref="MerchantAuthentication"/>.</returns>
        protected virtual MerchantAuthentication GetMerchantAuthentication()
        {
            var settingService = mSettingFactory.GetSettingService(ShoppingCartInfoObj.ShoppingCartSiteID);

            var result = new MerchantAuthentication()
            {
                Name = settingService.GetStringValue(API_LOGINID_SETTING_NAME),
                TransactionKey = settingService.GetStringValue(API_TRANSACTIONKEY_SETTING_NAME)
            };

            return result;
        }


        /// <summary>
        /// Creates customer part of payment request.
        /// </summary>
        /// <returns>Instance of <see cref="Customer"/>.</returns>
        protected virtual Customer GetCustomer()
        {
            var result = new Customer();

            var customer = CustomerInfoProvider.GetCustomerInfo(Order.OrderCustomerID);
            if (customer != null)
            {
                result.Id = ValidationHelper.GetString(customer.CustomerID, "", CultureInfo.InvariantCulture);
                result.Email = customer.CustomerEmail.ToNullIfEmpty();
            }

            return result;
        }


        /// <summary>
        /// Creates order part of payment request.
        /// </summary>
        /// <returns>Instance of <see cref="Order"/>.</returns>
        protected virtual Order GetOrder()
        {
            return new Order()
            {
                InvoiceNumber = TextHelper.LimitLength(Order.OrderInvoiceNumber, 20, "").ToNullIfEmpty(),
                Description = TextHelper.LimitLength(Order.OrderNote, 255, "").ToNullIfEmpty()
            };

        }


        /// <summary>
        /// Creates payment part of payment request.
        /// </summary>
        /// <param name="paymentData">Credit card data</param>
        /// <returns>Instance of <see cref="Payment"/>.</returns>
        protected virtual Payment GetPayment(IDictionary<string, object> paymentData)
        {
            var result = new Payment();

            result.CreditCard = new CreditCard();
            var expDate = ValidationHelper.GetDateTime(paymentData[AuthorizeNetParameters.CARD_EXPIRATION], DateTimeHelper.ZERO_TIME);
            result.CreditCard.ExpirationDate = string.Format(CultureHelper.EnglishCulture.DateTimeFormat, "{0:yyyy-MM}", expDate);
            result.CreditCard.CardNumber = ValidationHelper.GetString(paymentData[AuthorizeNetParameters.CARD_NUMBER], "");
            result.CreditCard.CardCode = ValidationHelper.GetString(paymentData[AuthorizeNetParameters.CARD_CCV], "");

            return result;
        }


        /// <summary>
        /// Creates partial amount part of payment request. Partial amount represents data about tax or shipping.
        /// </summary>
        /// <param name="amount">Amount of tax or shipping</param>
        /// <param name="name">Name of partial amount</param>
        /// <param name="description">Description of partial amount</param>
        /// <returns>Instance of <see cref="PartialAmount"/>.</returns>
        protected virtual PartialAmount GetPartialAmount(string amount, string name = null, string description = null)
        {
            var result = new PartialAmount();

            result.Amount = amount;
            result.Name = name;
            result.Description = description;

            return result;
        }


        /// <summary>
        /// Creates address for payment request. Address represents billing or shipping.
        /// </summary>
        /// <param name="address">Source of billing or shipping address</param>
        /// <returns>Instance of <see cref="AuthorizeNetDataContracts.Address"/>.</returns>
        protected virtual Address GetAddress(IAddress address)
        {
            if (address == null)
            {
                return null;
            }

            var result = new Address();
            result.FirstName = address.AddressPersonalName.ToNullIfEmpty();

            result.AddressLine = address.AddressLine1.ToNullIfEmpty();
            if (!string.IsNullOrEmpty(address.AddressLine2))
            {
                result.AddressLine += $", {address.AddressLine2}";
            }

            result.City = address.AddressCity.ToNullIfEmpty();
            result.Zip = address.AddressZip.ToNullIfEmpty();
            result.Country = CountryInfoProvider.GetCountryInfo(address.AddressCountryID)?.CountryThreeLetterCode.ToNullIfEmpty();
            result.State = StateInfoProvider.GetStateInfo(address.AddressStateID)?.StateCode.ToNullIfEmpty();

            return result;
        }
    }
}