using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(OrderInfo), OrderInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OrderInfo data container class.
    /// </summary>
    [Serializable]
    public class OrderInfo : AbstractInfo<OrderInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ORDER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OrderInfoProvider), OBJECT_TYPE, "ECommerce.Order", "OrderID", "OrderLastModified", "OrderGUID", null, "OrderInvoiceNumber", null, "OrderSiteID", null, null)
        {
            // Child object types
            // Object dependencies
            DependsOn = new List<ObjectDependency>
                                    {
                                        new ObjectDependency("OrderShippingOptionID", ShippingOptionInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                                        new ObjectDependency("OrderCreatedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.RequiredHasDefault),
                                        new ObjectDependency("OrderStatusID", OrderStatusInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                                        new ObjectDependency("OrderCurrencyID", CurrencyInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                                        new ObjectDependency("OrderCustomerID", CustomerInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                                        new ObjectDependency("OrderPaymentOptionID", PaymentOptionInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
                                    },
            // Binding object types
            // - None

            // Synchronization - Off
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },

            // Others
            DeleteObjectWithAPI = true,
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsCloning = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                AllowSingleExport = false,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                }
            },
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Order custom data.
        /// </summary>
        protected ContainerCustomData mOrderCustomData;

        /// <summary>
        /// Payment result.
        /// </summary>
        protected PaymentResultInfo mOrderPaymentResult;

        private OrderAddressInfo mBillingAddress;
        private OrderAddressInfo mShippingAddress;
        private OrderAddressInfo mCompanyAddress;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Order ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderID
        {
            get
            {
                return GetIntegerValue("OrderID", 0);
            }
            set
            {
                SetValue("OrderID", value);
            }
        }


        /// <summary>
        /// Order payment option ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderPaymentOptionID
        {
            get
            {
                return GetIntegerValue("OrderPaymentOptionID", 0);
            }
            set
            {
                SetValue("OrderPaymentOptionID", value, value > 0);
            }
        }


        /// <summary>
        /// Order site ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderSiteID
        {
            get
            {
                return GetIntegerValue("OrderSiteID", 0);
            }
            set
            {
                SetValue("OrderSiteID", value);
            }
        }


        /// <summary>
        /// Order total tax in shopping cart currency.
        /// </summary>
        [DatabaseField]
        public virtual decimal OrderTotalTax
        {
            get
            {
                return GetDecimalValue("OrderTotalTax", 0.0m);
            }
            set
            {
                SetValue("OrderTotalTax", value);
            }
        }


        /// <summary>
        /// Order tax summary.
        /// </summary>
        [DatabaseField]
        public virtual string OrderTaxSummary
        {
            get
            {
                return ValidationHelper.GetString(GetValue("OrderTaxSummary"), string.Empty);
            }
            set
            {
                SetValue("OrderTaxSummary", value, string.Empty);
            }
        }


        /// <summary>
        /// Order invoice number.
        /// </summary>
        [DatabaseField]
        public virtual string OrderInvoiceNumber
        {
            get
            {
                return GetStringValue("OrderInvoiceNumber", "");
            }
            set
            {
                SetValue("OrderInvoiceNumber", value);
            }
        }


        /// <summary>
        /// Order currency ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderCurrencyID
        {
            get
            {
                return GetIntegerValue("OrderCurrencyID", 0);
            }
            set
            {
                SetValue("OrderCurrencyID", value, value > 0);
            }
        }


        /// <summary>
        /// Order shipping option ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderShippingOptionID
        {
            get
            {
                return GetIntegerValue("OrderShippingOptionID", 0);
            }
            set
            {
                SetValue("OrderShippingOptionID", value, value > 0);
            }
        }


        /// <summary>
        /// Order invoice.
        /// </summary>
        [DatabaseField]
        public virtual string OrderInvoice
        {
            get
            {
                return GetStringValue("OrderInvoice", "");
            }
            set
            {
                SetValue("OrderInvoice", value);
            }
        }


        /// <summary>
        /// Order note.
        /// </summary>
        [DatabaseField]
        public virtual string OrderNote
        {
            get
            {
                return GetStringValue("OrderNote", "");
            }
            set
            {
                SetValue("OrderNote", value);
            }
        }


        /// <summary>
        /// Order created by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderCompletedByUserID
        {
            get
            {
                return GetIntegerValue("OrderCreatedByUserID", 0);
            }
            set
            {
                SetValue("OrderCreatedByUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Order customer ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderCustomerID
        {
            get
            {
                return GetIntegerValue("OrderCustomerID", 0);
            }
            set
            {
                SetValue("OrderCustomerID", value);
            }
        }


        /// <summary>
        /// Order status ID.
        /// </summary>
        [DatabaseField]
        public virtual int OrderStatusID
        {
            get
            {
                return GetIntegerValue("OrderStatusID", 0);
            }
            set
            {
                SetValue("OrderStatusID", value);
            }
        }


        /// <summary>
        /// Order grand total.
        /// </summary>
        [DatabaseField]
        public virtual decimal OrderGrandTotal
        {
            get
            {
                return GetDecimalValue("OrderGrandTotal", 0.0m);
            }
            set
            {
                SetValue("OrderGrandTotal", value);
            }
        }


        /// <summary>
        /// Order grand total in the main currency.
        /// </summary>
        [DatabaseField]
        public virtual decimal OrderGrandTotalInMainCurrency
        {
            get
            {
                return GetDecimalValue("OrderGrandTotalInMainCurrency", 0.0m);
            }
            set
            {
                SetValue("OrderGrandTotalInMainCurrency", value);
            }
        }


        /// <summary>
        /// Order total price.
        /// </summary>
        [DatabaseField]
        public virtual decimal OrderTotalPrice
        {
            get
            {
                return GetDecimalValue("OrderTotalPrice", 0.0m);
            }
            set
            {
                SetValue("OrderTotalPrice", value);
            }
        }


        /// <summary>
        /// Order total price in main currency.
        /// </summary>
        [DatabaseField]
        public virtual decimal OrderTotalPriceInMainCurrency
        {
            get
            {
                return GetDecimalValue("OrderTotalPriceInMainCurrency", 0.0m);
            }
            set
            {
                SetValue("OrderTotalPriceInMainCurrency", value);
            }
        }


        /// <summary>
        /// Order total shipping in shopping cart currency.
        /// </summary>
        [DatabaseField]
        public virtual decimal OrderTotalShipping
        {
            get
            {
                return GetDecimalValue("OrderTotalShipping", 0.0m);
            }
            set
            {
                SetValue("OrderTotalShipping", value);
            }
        }


        /// <summary>
        /// Order date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime OrderDate
        {
            get
            {
                return GetDateTimeValue("OrderDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("OrderDate", value);
            }
        }


        /// <summary>
        /// Order payment result.
        /// </summary>
        [DatabaseField]
        public virtual PaymentResultInfo OrderPaymentResult
        {
            get
            {
                if (mOrderPaymentResult == null)
                {
                    var paymentResultXML = ValidationHelper.GetString(GetValue("OrderPaymentResult"), "");
                    if (!string.IsNullOrEmpty(paymentResultXML))
                    {
                        mOrderPaymentResult = new PaymentResultInfo(paymentResultXML);
                    }
                }

                return mOrderPaymentResult;
            }
            set
            {
                ValidatePaymentResult(value);

                var paymentResultXml = ValidationHelper.GetString(value?.GetPaymentResultXml(), "");
                SetValue("OrderPaymentResult", paymentResultXml);

                mOrderPaymentResult = null;
            }
        }


        /// <summary>
        /// Order tracking number.
        /// </summary>
        [DatabaseField]
        public virtual string OrderTrackingNumber
        {
            get
            {
                return GetStringValue("OrderTrackingNumber", "");
            }
            set
            {
                SetValue("OrderTrackingNumber", value);
            }
        }


        /// <summary>
        /// Order GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid OrderGUID
        {
            get
            {
                return GetGuidValue("OrderGUID", Guid.Empty);
            }
            set
            {
                SetValue("OrderGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime OrderLastModified
        {
            get
            {
                return GetDateTimeValue("OrderLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("OrderLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Order is paid.
        /// </summary>
        [DatabaseField]
        public virtual bool OrderIsPaid
        {
            get
            {
                return GetBooleanValue("OrderIsPaid", false);
            }
            set
            {
                SetValue("OrderIsPaid", value);
            }
        }


        /// <summary>
        /// Indicates whether the order payment is authorized.
        /// </summary>
        [RegisterProperty]
        public virtual bool OrderIsAuthorized
        {
            get
            {
                return OrderPaymentResult != null && OrderPaymentResult.PaymentIsAuthorized;
            }
        }


        /// <summary>
        /// Culture of the order.
        /// </summary>
        [DatabaseField]
        public virtual string OrderCulture
        {
            get
            {
                return GetStringValue("OrderCulture", string.Empty);
            }
            set
            {
                SetValue("OrderCulture", value);
            }
        }


        /// <summary>
        /// Order custom data.
        /// </summary>
        [DatabaseField]
        public ContainerCustomData OrderCustomData
        {
            get
            {
                return mOrderCustomData ?? (mOrderCustomData = new ContainerCustomData(this, "OrderCustomData"));
            }
        }


        /// <summary>
        /// Xml content with the summary of the order and multibuy discounts which were applied to the shopping cart item. 
        /// Contains discount name, price and price in main currency.
        /// </summary>
        [DatabaseField]
        public virtual string OrderDiscounts
        {
            get
            {
                return GetStringValue("OrderDiscounts", string.Empty);
            }
            set
            {
                SetValue("OrderDiscounts", value);
            }
        }


        /// <summary>
        /// Order other payments (such as gif codes) XML summary.
        /// </summary>
        [DatabaseField]
        public virtual string OrderOtherPayments
        {
            get
            {
                return ValidationHelper.GetString(GetValue("OrderOtherPayments"), string.Empty);
            }
            set
            {
                SetValue("OrderOtherPayments", value, string.Empty);
            }
        }


        /// <summary>
        /// Order coupon codes XML summary
        /// </summary>
        [DatabaseField]
        public virtual string OrderCouponCodes
        {
            get
            {
                return GetStringValue("OrderCouponCodes", string.Empty);
            }
            set
            {
                SetValue("OrderCouponCodes", value, string.Empty);
            }
        }


        /// <summary>
        /// Returns formatted payment result.
        /// </summary>
        [RegisterProperty]
        public string OrderPaymentFormattedResult
        {
            get
            {
                var paymentResultValue = OrderPaymentResult?.GetFormattedPaymentResultString().Trim();

                if (String.IsNullOrEmpty(paymentResultValue))
                {
                    paymentResultValue = ResHelper.GetString("general.na");
                }

                return paymentResultValue;
            }
        }


        /// <summary>
        /// Gets the current billing address.
        /// </summary>
        [RegisterProperty]
        public OrderAddressInfo OrderBillingAddress
        {
            get
            {
                if ((mBillingAddress == null) || (!mBillingAddress.Generalized.IsObjectValid))
                {
                    mBillingAddress = OrderAddressInfoProvider.GetAddresses()
                                                              .WhereEquals("AddressOrderID", OrderID)
                                                              .WhereEquals("AddressType", (int)AddressType.Billing)
                                                              .TopN(1).FirstOrDefault();
                }

                return mBillingAddress;
            }
        }


        /// <summary>
        /// Gets the current shipping address.
        /// </summary>
        [RegisterProperty]
        public OrderAddressInfo OrderShippingAddress
        {
            get
            {
                if ((mShippingAddress == null) || (!mShippingAddress.Generalized.IsObjectValid))
                {
                    mShippingAddress = OrderAddressInfoProvider.GetAddresses()
                                                               .WhereEquals("AddressOrderID", OrderID)
                                                               .WhereEquals("AddressType", (int)AddressType.Shipping)
                                                               .TopN(1).FirstOrDefault();
                }

                return mShippingAddress;
            }
        }


        /// <summary>
        /// Gets the current company address.
        /// </summary>
        [RegisterProperty]
        public OrderAddressInfo OrderCompanyAddress
        {
            get
            {
                if ((mCompanyAddress == null) || (!mCompanyAddress.Generalized.IsObjectValid))
                {
                    mCompanyAddress = OrderAddressInfoProvider.GetAddresses()
                                                              .WhereEquals("AddressOrderID", OrderID)
                                                              .WhereEquals("AddressType", (int)AddressType.Company)
                                                              .TopN(1).FirstOrDefault();
                }

                return mCompanyAddress;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OrderInfoProvider.DeleteOrderInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OrderInfoProvider.SetOrderInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OrderInfo object.
        /// </summary>
        public OrderInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderInfo object from the given DataRow.
        /// </summary>
        public OrderInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderInfo object from serialized data.
        /// </summary>        
        /// <param name="info">Serialization data</param>
        /// <param name="context">Context</param> 
        public OrderInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
            mOrderCustomData = (ContainerCustomData)info.GetValue("OrderCustomData", typeof(ContainerCustomData));
            mOrderPaymentResult = (PaymentResultInfo)info.GetValue("OrderPaymentResult", typeof(PaymentResultInfo));
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("OrderCustomData", mOrderCustomData);
            info.AddValue("OrderPaymentResult", mOrderPaymentResult);
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            return EcommercePermissions.CheckOrdersPermissions(permission, siteName, userInfo, exceptionOnFailure, base.CheckPermissionsInternal);
        }

        #endregion


        #region "Overrides"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("OrderCustomData", m => m.OrderCustomData);
        }

        #endregion


        #region Order status manipulation

        /// <summary>
        /// Updates <see cref="OrderStatusID"/> property accordingly to given <paramref name="paymentResult"/> and assigned <see cref="PaymentOptionInfo"/> configuration.
        /// </summary>
        public virtual void UpdateOrderStatus(PaymentResultInfo paymentResult)
        {
            if (paymentResult == null)
            {
                throw new ArgumentNullException(nameof(paymentResult));
            }

            ValidatePaymentResult(paymentResult);

            var paymentOption = PaymentOptionInfoProvider.GetPaymentOptionInfo(OrderPaymentOptionID);
            if (paymentOption == null)
            {
                throw new InvalidOperationException("Unable to determine order status. Please set property OrderPaymentOptionID correctly.");
            }

            // Update order payment result
            OrderPaymentResult = paymentResult;

            // Update order status
            if (paymentResult.PaymentIsCompleted)
            {
                UpdateOrderStatus(paymentOption.PaymentOptionSucceededOrderStatusID);

                OrderIsPaid = true;

                // Fire order is paid event for successfully paid order
                EcommerceEvents.OrderPaid.StartEvent(this);
            }
            else if (paymentResult.PaymentIsFailed)
            {
                UpdateOrderStatus(paymentOption.PaymentOptionFailedOrderStatusID);
            }
            else if (paymentResult.PaymentIsAuthorized)
            {
                UpdateOrderStatus(paymentOption.PaymentOptionAuthorizedOrderStatusID);
            }

            SetObject();
        }


        /// <summary>
        /// Assigns order status to order. If can not be used due to global objects settings, looks for site status with the same code name.
        /// </summary>
        /// <param name="statusId">Id of the status</param>
        protected void UpdateOrderStatus(int statusId)
        {
            // Do not change order status
            if (statusId <= 0)
            {
                return;
            }

            var siteName = SiteInfoProvider.GetSiteName(OrderSiteID);

            // Avoid setting global order status when global statuses are not used
            if (!ECommerceSettings.UseGlobalOrderStatus(siteName) && ECommerceSettings.AllowGlobalPaymentMethods(siteName))
            {
                // Get global status
                var origOrderStatus = OrderStatusInfoProvider.GetOrderStatusInfo(statusId);
                if (origOrderStatus != null)
                {
                    if (origOrderStatus.IsGlobal)
                    {
                        // Get site status with the same name
                        var orderStatus = OrderStatusInfoProvider.GetOrderStatusInfo(origOrderStatus.StatusName, siteName);
                        if (orderStatus != null)
                        {
                            // Set site status
                            OrderStatusID = orderStatus.StatusID;
                        }
                    }
                    else
                    {
                        // Set status
                        OrderStatusID = statusId;
                    }
                }
            }
            else
            {
                // Set status
                OrderStatusID = statusId;
            }
        }


        private static void ValidatePaymentResult(PaymentResultInfo paymentResult)
        {
            if (paymentResult == null)
            {
                return;
            }

            if (paymentResult.PaymentIsFailed && paymentResult.PaymentIsCompleted)
            {
                throw new InvalidOperationException("Payment result has properties PaymentIsFailed and PaymentIsCompleted set to true. This is invalid state.");
            }
        }

        #endregion
    }
}