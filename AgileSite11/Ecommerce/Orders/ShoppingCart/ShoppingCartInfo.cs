using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(ShoppingCartInfo), ShoppingCartInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ShoppingCartInfo data container class.
    /// </summary>
    [Serializable]
    public class ShoppingCartInfo : AbstractInfo<ShoppingCartInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.SHOPPING_CART;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ShoppingCartInfoProvider), OBJECT_TYPE, "Ecommerce.ShoppingCart", "ShoppingCartID", "ShoppingCartLastUpdate", "ShoppingCartGUID", null, null, null, "ShoppingCartSiteID", null, null)
        {
            // Child object types
            // Object dependencies
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ShoppingCartBillingAddressID", AddressInfo.OBJECT_TYPE),
                new ObjectDependency("ShoppingCartShippingAddressID", AddressInfo.OBJECT_TYPE),
                new ObjectDependency("ShoppingCartCompanyAddressID", AddressInfo.OBJECT_TYPE),
                new ObjectDependency("ShoppingCartCurrencyID", CurrencyInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ShoppingCartCustomerID", CustomerInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ShoppingCartPaymentOptionID", PaymentOptionInfo.OBJECT_TYPE),
                new ObjectDependency("ShoppingCartShippingOptionID", ShippingOptionInfo.OBJECT_TYPE),
                new ObjectDependency("ShoppingCartUserID", UserInfo.OBJECT_TYPE)
            },
            // Binding object types
            // - None

            // Export
            // - None

            // Synchronization
            // - None

            // Others
            LogEvents = false,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsCloning = false
        };

        #endregion


        #region "Variables"

        private readonly List<ShoppingCartItemInfo> mCartItems = new List<ShoppingCartItemInfo>();

        private string mShoppingCartCulture;

        private IAddress mShoppingCartBillingAddress;
        private IAddress mShoppingCartShippingAddress;
        private IAddress mShoppingCartCompanyAddress;
        private ContainerCustomData mShoppingCartCustomData;
        private ICouponCodeCollection mCouponCodes;
        private bool mCustomerInitialized;
        private CustomerInfo mCustomer;
        private int mEvaluating;

        #endregion


        #region "Properties - Data class"

        /// <summary>
        /// Shopping cart ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartID
        {
            get
            {
                return GetIntegerValue("ShoppingCartID", 0);
            }
            set
            {
                SetValue("ShoppingCartID", value);
            }
        }


        /// <summary>
        /// Shopping cart site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartSiteID
        {
            get
            {
                return GetIntegerValue("ShoppingCartSiteID", 0);
            }
            set
            {
                SetValue("ShoppingCartSiteID", value, value > 0);
            }
        }


        /// <summary>
        /// Shopping cart shipping option ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartShippingOptionID
        {
            get
            {
                return GetIntegerValue("ShoppingCartShippingOptionID", 0);
            }
            set
            {
                SetValue("ShoppingCartShippingOptionID", value, value > 0);
            }
        }


        /// <summary>
        /// Shopping cart payment option ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartPaymentOptionID
        {
            get
            {
                return GetIntegerValue("ShoppingCartPaymentOptionID", 0);
            }
            set
            {
                SetValue("ShoppingCartPaymentOptionID", value, value > 0);
            }
        }


        /// <summary>
        /// Shopping cart customer ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartCustomerID
        {
            get
            {
                return GetIntegerValue("ShoppingCartCustomerID", 0);
            }
            set
            {
                SetValue("ShoppingCartCustomerID", value, value > 0);
                InvalidateCustomer();
            }
        }


        /// <summary>
        /// Shopping cart contact ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartContactID
        {
            get
            {
                return GetIntegerValue("ShoppingCartContactID", 0);
            }
            set
            {
                SetValue("ShoppingCartContactID", value, value > 0);
            }
        }


        /// <summary>
        /// Shopping cart billing address ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartBillingAddressID
        {
            get
            {
                return GetIntegerValue("ShoppingCartBillingAddressID", 0);
            }
            private set
            {
                SetValue("ShoppingCartBillingAddressID", value, value > 0);
                mShoppingCartBillingAddress = null;
            }
        }


        /// <summary>
        /// Shopping cart shipping address ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartShippingAddressID
        {
            get
            {
                return GetIntegerValue("ShoppingCartShippingAddressID", 0);
            }
            private set
            {
                SetValue("ShoppingCartShippingAddressID", value, value > 0);
                mShoppingCartShippingAddress = null;
            }
        }


        /// <summary>
        /// Shopping cart company address ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartCompanyAddressID
        {
            get
            {
                return GetIntegerValue("ShoppingCartCompanyAddressID", 0);
            }
            private set
            {
                SetValue("ShoppingCartCompanyAddressID", value, value > 0);
                mShoppingCartCompanyAddress = null;
            }
        }


        /// <summary>
        /// Shopping cart currency ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartCurrencyID
        {
            get
            {
                return GetIntegerValue("ShoppingCartCurrencyID", 0);
            }
            set
            {
                SetValue("ShoppingCartCurrencyID", value, value > 0);
            }
        }


        /// <summary>
        /// Shopping cart note.
        /// </summary>
        [DatabaseField]
        public virtual string ShoppingCartNote
        {
            get
            {
                return GetStringValue("ShoppingCartNote", "");
            }
            set
            {
                SetValue("ShoppingCartNote", value);
            }
        }


        /// <summary>
        /// Shopping cart GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ShoppingCartGUID
        {
            get
            {
                return GetGuidValue("ShoppingCartGUID", Guid.Empty);
            }
            set
            {
                SetValue("ShoppingCartGUID", value);
            }
        }


        /// <summary>
        /// Date and time when the shopping cart was last updated.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ShoppingCartLastUpdate
        {
            get
            {
                return GetDateTimeValue("ShoppingCartLastUpdate", DateTime.Now);
            }
            set
            {
                SetValue("ShoppingCartLastUpdate", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Shopping cart custom data.
        /// </summary>
        [RegisterProperty]
        public ContainerCustomData ShoppingCartCustomData
        {
            get
            {
                return mShoppingCartCustomData ?? (mShoppingCartCustomData = new ContainerCustomData(this, "ShoppingCartCustomData"));
            }
        }


        /// <summary>
        /// Owner of the shopping cart. It is null if customer is anonymous or public.
        /// </summary>
        [DatabaseField("ShoppingCartUserID", ValueType = typeof(int))]
        public UserInfo User
        {
            get
            {
                return UserInfoProvider.GetUserInfo(GetValue("ShoppingCartUserID", 0));
            }
            set
            {
                // Check if new user is null or public
                if (value != null && !value.IsPublic())
                {
                    SetValue("ShoppingCartUserID", value.UserID);
                }
                // User is null or public - check if value is already null
                else
                {
                    // Set public user (null value)
                    SetValue("ShoppingCartUserID", null);
                }
                InvalidateCustomer();
            }
        }

        #endregion


        #region "Properties - Info objects"

        /// <summary>
        /// Order data. Loaded when the shopping cart is created from an existing order, otherwise it is null.
        /// </summary>
        public OrderInfo Order
        {
            get;
            set;
        }


        /// <summary>
        /// Customer information.
        /// </summary>
        [RegisterProperty("ShoppingCartCustomer")]
        public CustomerInfo Customer
        {
            get
            {
                if (!mCustomerInitialized)
                {
                    CustomerInfo customer = null;
                    if (User != null && !User.IsPublic())
                    {
                        customer = CustomerInfoProvider.GetCustomerInfoByUserID(User.UserID);
                    }

                    if (customer == null || !customer.Generalized.IsObjectValid)
                    {
                        customer = CustomerInfoProvider.GetCustomerInfo(ShoppingCartCustomerID);
                    }
                    mCustomer = customer;
                    mCustomerInitialized = true;
                }

                return mCustomer;
            }
            set
            {
                var customerId = value?.CustomerID ?? 0;
                SetValue("ShoppingCartCustomerID", customerId, customerId > 0);

                mCustomer = value;
                mCustomerInitialized = (value != null);
            }
        }


        /// <summary>
        /// Gets or sets the current billing address.
        /// </summary>
        [RegisterProperty]
        public IAddress ShoppingCartBillingAddress
        {
            get
            {
                return mShoppingCartBillingAddress ?? (mShoppingCartBillingAddress = AddressInfoProvider.GetAddressInfo(ShoppingCartBillingAddressID));
            }
            set
            {
                // Set id only for AddressInfo objects
                ShoppingCartBillingAddressID = (value as AddressInfo)?.AddressID ?? 0;
                mShoppingCartBillingAddress = value;
            }
        }


        /// <summary>
        /// Gets or sets the current shipping address.
        /// </summary>
        [RegisterProperty]
        public IAddress ShoppingCartShippingAddress
        {
            get
            {
                return mShoppingCartShippingAddress ?? (mShoppingCartShippingAddress = AddressInfoProvider.GetAddressInfo(ShoppingCartShippingAddressID));
            }
            set
            {
                // Set id only for AddressInfo objects
                ShoppingCartShippingAddressID = (value as AddressInfo)?.AddressID ?? 0;
                mShoppingCartShippingAddress = value;
            }
        }


        /// <summary>
        /// Gets or sets the current company address.
        /// </summary>
        [RegisterProperty]
        public IAddress ShoppingCartCompanyAddress
        {
            get
            {
                return mShoppingCartCompanyAddress ?? (mShoppingCartCompanyAddress = AddressInfoProvider.GetAddressInfo(ShoppingCartCompanyAddressID));
            }
            set
            {
                // Set id only for AddressInfo objects
                ShoppingCartCompanyAddressID = (value as AddressInfo)?.AddressID ?? 0;
                mShoppingCartCompanyAddress = value;
            }
        }


        /// <summary>
        /// Selected shipping option. If none is selected and there is only one available, it is selected automatically.
        /// </summary>
        public ShippingOptionInfo ShippingOption
        {
            get
            {
                return GetShippingOption();
            }
        }


        /// <summary>
        /// Selected currency.
        /// </summary>
        public CurrencyInfo Currency
        {
            get
            {
                return CurrencyInfoProvider.GetCurrencyInfo(ShoppingCartCurrencyID);
            }
        }


        /// <summary>
        /// Selected payment method. If none is selected and there is only one available, it is selected automatically.
        /// </summary>
        public PaymentOptionInfo PaymentOption
        {
            get
            {
                return GetPaymentOption();
            }
        }

        #endregion


        #region "Properties - Calculations"

        /// <summary>
        /// Price and discount evaluator.
        /// </summary>
        internal IShoppingCartEvaluator ShoppingCartEvaluator
        {
            get;
            set;
        } = Service.Resolve<IShoppingCartEvaluator>();


        /// <summary>
        /// Table with the shopping cart items.
        /// </summary>
        [RegisterProperty]
        public IEnumerable<ShoppingCartLine> ContentTable
        {
            get
            {
                return ShoppingCartInfoProvider.EvaluateContent(this);
            }
        }


        /// <summary>
        /// DataRowCollection with the summary of the taxes, used as property for MacroDataSource.
        /// </summary>
        [RegisterProperty]
        public ValuesSummary TaxSummary
        {
            get;
            set;
        }


        /// <summary>
        /// Grand total of the shopping cart in shopping cart currency remaining to pay after other payments (e.g. gift card) have been applied.
        /// </summary>
        [RegisterProperty]
        public decimal GrandTotal
        {
            get;
            set;
        }


        /// <summary>
        /// Other payments already applied on the shopping card total price.
        /// </summary>
        /// <remarks>
        /// Typical example of such are gift cards.
        /// </remarks>
        [RegisterProperty]
        public decimal OtherPayments
        {
            get;
            set;
        }


        /// <summary>
        /// Total price of the shopping cart in shopping cart currency. All discounts except for the order discount, taxes and shipping are included.
        /// </summary>
        [RegisterProperty]
        public decimal TotalPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Total price of all shopping cart items in shopping cart currency. Items discounts are included.
        /// </summary>
        [RegisterProperty]
        public decimal TotalItemsPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Discount which is applied to the items total price. It is in shopping cart currency.
        /// </summary>
        [RegisterProperty]
        public decimal OrderDiscount
        {
            get;
            set;
        }


        /// <summary>
        /// The summary of discounts applied to the items total price. It is in shopping cart currency.
        /// </summary>
        [RegisterProperty]
        public ValuesSummary OrderDiscountSummary
        {
            get;
            set;
        }


        /// <summary>
        /// Discount which is applied to the items price. It is in shopping cart currency.
        /// </summary>
        [RegisterProperty]
        public decimal ItemsDiscount
        {
            get
            {
                return CartItems.Sum(item => item.TotalDiscount);
            }
        }


        /// <summary>
        /// Total tax which is applied to all shopping cart items altogether. It is in shopping cart currency.
        /// </summary>
        [RegisterProperty]
        public decimal TotalTax
        {
            get;
            set;
        }


        /// <summary>
        /// Total shipping in shopping cart currency. Shipping free limit is applied.
        /// </summary>
        [RegisterProperty]
        public decimal TotalShipping
        {
            get;
            set;
        }


        /// <summary>
        /// Returns false when any cart calculated value is not up to date.
        /// This applies to prices, discounts, taxes.
        /// </summary>
        /// <remarks>
        /// Call method <see cref="Evaluate"/> to calculate recent values.
        /// </remarks>
        public bool IsRecent()
        {
            var grandTotalBefore = GrandTotal;
            var totalUnitsBefore = TotalUnits;

            Evaluate();

            return GrandTotal == grandTotalBefore && TotalUnits == totalUnitsBefore;
        }


        /// <summary>
        /// Evaluates the price information.
        /// </summary>
        public void Evaluate()
        {
            if (Interlocked.Exchange(ref mEvaluating, 1) == 0)
            {
                try
                {
                    ValidateData();

                    ShoppingCartEvaluator.Evaluate(this);
                }
                finally
                {
                    Interlocked.Exchange(ref mEvaluating, 0);
                }
            }
        }


        /// <summary>
        /// Ensures that shopping cart contains valid data
        /// </summary>
        private void ValidateData()
        {
            // Shipping option
            if (IsInfoMissing(ShippingOption, ShoppingCartShippingOptionID) || (ShippingOption != null && !ShippingOption.ShippingOptionEnabled))
            {
                ShoppingCartShippingOptionID = 0;
            }

            // Payment option
            if (IsInfoMissing(PaymentOption, ShoppingCartPaymentOptionID) || (PaymentOption != null) && !PaymentOption.PaymentOptionEnabled)
            {
                ShoppingCartPaymentOptionID = 0;
            }

            // Customer
            if (IsInfoMissing(Customer, ShoppingCartCustomerID) || (Customer?.CustomerUser != null) && !Customer.CustomerUser.Enabled)
            {
                ShoppingCartCustomerID = 0;
            }
        }


        /// <summary>
        /// Returns True, if the given info object is null and the given ID is equal or greater than zero, otherwise returns False.
        /// </summary>
        /// <param name="info">Info object to check</param>
        /// <param name="infoId">Value of the info object ID column</param>
        private bool IsInfoMissing(BaseInfo info, int infoId)
        {
            return info == null && infoId >= 0;
        }


        private ShippingOptionInfo GetShippingOption()
        {
            var option = ShippingOptionInfoProvider.GetShippingOptionInfo(ShoppingCartShippingOptionID);

            if (option == null && ECommerceContext.OnlyOneShippingOptionAvailableOnSite && IsShippingNeeded)
            {
                // Set the only one available shipping option as selected
                option = ShippingOptionInfoProvider.GetShippingOptions(ShoppingCartSiteID, true).FirstObject;
                ShoppingCartShippingOptionID = option.ShippingOptionID;
            }

            return option;
        }


        private PaymentOptionInfo GetPaymentOption()
        {
            var option = PaymentOptionInfoProvider.GetPaymentOptionInfo(ShoppingCartPaymentOptionID);

            if (option == null && ECommerceContext.OnlyOnePaymentOptionAvailableOnSite)
            {
                // Set the only one available payment option as selected
                option = PaymentOptionInfoProvider.GetPaymentOptions(ShoppingCartSiteID, true).FirstObject;
                ShoppingCartPaymentOptionID = option.PaymentOptionID;
            }

            return option;
        }

        #endregion


        #region "Properties - Others"

        /// <summary>
        /// Gets ID of the registered user the shopping cart belongs to.
        /// </summary>
        /// <remarks>
        /// Returns 0 if user is not assigned or user is public.
        /// </remarks>
        public virtual int ShoppingCartUserID
        {
            get
            {
                if (User == null || User.IsPublic())
                {
                    // Customer is anonymous
                    return 0;
                }

                // Customer is registered
                return User.UserID;
            }
        }


        /// <summary>
        /// List of shopping cart items.
        /// </summary>
        public List<ShoppingCartItemInfo> CartItems
        {
            get
            {
                return mCartItems;
            }
        }


        /// <summary>
        /// List of shopping cart coupon codes.
        /// </summary>
        public ICouponCodeCollection CouponCodes
        {
            get
            {
                if (mCouponCodes == null)
                {
                    mCouponCodes = new CouponCodeCollection();

                    var shoppingCartCodes = ShoppingCartCouponCodeInfoProvider.GetShoppingCartCouponCodes()
                                                                  .WhereEquals("ShoppingCartID", ShoppingCartID)
                                                                  .Column("CouponCode")
                                                                  .GetListResult<string>();

                    foreach (var code in shoppingCartCodes)
                    {
                        mCouponCodes.Add(code, CouponCodeApplicationStatusEnum.NotAppliedInCart);
                    }
                }

                return mCouponCodes;
            }
            internal set
            {
                mCouponCodes = value;
            }
        }


        /// <summary>
        /// Subset of CartItem collection. Bundle items and product options are excluded.
        /// </summary>
        [RegisterProperty]
        public IEnumerable<ShoppingCartItemInfo> CartProducts
        {
            get
            {
                return CartItems.Where(item => !item.IsBundleItem && !item.IsProductOption);
            }
        }


        /// <summary>
        /// Subset of CartItem collection. Bundle items are excluded.
        /// </summary>
        public IEnumerable<ShoppingCartItemInfo> CartContentItems
        {
            get
            {
                return CartItems.Where(item => !item.IsBundleItem);
            }
        }


        /// <summary>
        /// Indicates that customer will be registered after checkout.
        /// </summary>
        [Obsolete("Use member CMS.Core.Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(ShoppingCartSiteID).IsCustomerRegisteredAfterCheckout instead")]
        public bool RegisterAfterCheckout
        {
            get
            {
                return Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(ShoppingCartSiteID).IsCustomerRegisteredAfterCheckout;
            }
            set
            {
                Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(ShoppingCartSiteID).IsCustomerRegisteredAfterCheckout = value;
            }
        }


        /// <summary>
        /// E-mail template code name used for registration after checkout.
        /// </summary>
        [Obsolete("Use member CMS.Core.Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(ShoppingCartSiteID).RegisteredAfterCheckoutTemplate instead")]
        public string RegisterAfterCheckoutTemplate
        {
            get
            {
                return Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(ShoppingCartSiteID).RegisteredAfterCheckoutTemplate;
            }
            set
            {
                Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(ShoppingCartSiteID).RegisteredAfterCheckoutTemplate = value;
            }
        }


        /// <summary>
        /// Indicates that shopping cart has no items (CartItems collection is empty).
        /// </summary>
        [RegisterProperty]
        public bool IsEmpty
        {
            get
            {
                return CartItems.Count == 0;
            }
        }


        /// <summary>
        /// Sum of all shopping cart items' units, bundle items and product options are not included.
        /// </summary>
        [RegisterProperty]
        public int TotalUnits
        {
            get
            {
                return ShoppingCartInfoProvider.GetTotalUnits(this);
            }
        }


        /// <summary>
        /// Payment gateway custom data.
        /// </summary>
        public Hashtable PaymentGatewayCustomData
        {
            get;
            set;
        } = new Hashtable();


        /// <summary>
        /// Total weight of shopping cart items.
        /// </summary>
        [RegisterProperty]
        public double TotalItemsWeight
        {
            get
            {
                return ShoppingCartInfoProvider.CalculateTotalItemsWeight(this);
            }
        }


        /// <summary>
        /// Name of the site to which the shopping cart belongs.
        /// </summary>
        public string SiteName
        {
            get
            {
                var site = SiteInfoProvider.GetSiteInfo(ShoppingCartSiteID);
                return site?.SiteName;
            }
            set
            {
                var site = SiteInfoProvider.GetSiteInfo(value);

                ShoppingCartSiteID = site?.SiteID ?? 0;
            }
        }


        /// <summary>
        /// Indicates if shopping cart object is created from order.
        /// It is set automatically when shopping cart object is created by GetShoppingCartInfoFromOrder method.
        /// </summary>
        public bool IsCreatedFromOrder
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the order to which the shopping cart belongs or from which the shopping cart was created.
        /// </summary>
        public int OrderId
        {
            get;
            set;
        }


        /// <summary>
        /// Other payments summary.
        /// </summary>
        [RegisterProperty]
        public ValuesSummary OtherPaymentsSummary
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if shipping is needed for a shopping cart.
        /// </summary>
        [RegisterProperty]
        public bool IsShippingNeeded
        {
            get
            {
                return ShippingOptionInfoProvider.IsShippingNeeded(this);
            }
        }


        /// <summary>
        /// Indicates if private data were cleared from this shopping cart object.
        /// It is set automatically when shopping cart private data are cleared by ClearShoppingCartPrivateData method.
        /// </summary>
        public bool PrivateDataCleared
        {
            get;
            set;
        }


        /// <summary>
        /// Shopping cart order culture (or culture of the order user created using the shopping cart).
        /// </summary>
        public virtual string ShoppingCartCulture
        {
            get
            {
                if (String.IsNullOrEmpty(mShoppingCartCulture))
                {
                    return LocalizationContext.PreferredCultureCode;
                }

                return mShoppingCartCulture;
            }
            set
            {
                mShoppingCartCulture = value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ShoppingCartInfoProvider.DeleteShoppingCartInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ShoppingCartInfoProvider.SetShoppingCartInfo(this);
        }


        /// <summary>
        /// Register the custom properties
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            // Replace automatically named child collection (ShoppingCartItems of type InfoObjectCollection) with loaded items
            RegisterProperty("ShoppingCartItems", m => m.CartItems);

            // Replace automatic properties with their more complex implementation
            RegisterProperty("Currency", m => m.Currency);
            RegisterProperty("PaymentOption", m => m.PaymentOption);
            RegisterProperty("ShippingOption", m => m.ShippingOption);
        }

        #endregion


        #region "Constructors, serialization"

        /// <summary>
        /// Constructor - Creates an empty ShoppingCartInfo object.
        /// </summary>
        /// <remarks>
        /// To create initialized ShoppingCartInfo ready for shopping use ShoppingCartFactory.CreateCart(siteId) instead.
        /// </remarks>
        public ShoppingCartInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ShoppingCartInfo object from the given DataRow.
        /// </summary>
        public ShoppingCartInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(CartItems), CartItems);
            info.AddValue(nameof(CouponCodes), CouponCodes);
            info.AddValue(nameof(Customer), mCustomer);
            info.AddValue(nameof(ShoppingCartCulture), ShoppingCartCulture);
            info.AddValue(nameof(PrivateDataCleared), PrivateDataCleared);
            info.AddValue(nameof(User), User);
            info.AddValue(nameof(SiteName), SiteName);
            info.AddValue(nameof(ShoppingCartBillingAddress), ShoppingCartBillingAddress);
            info.AddValue(nameof(ShoppingCartShippingAddress), ShoppingCartShippingAddress);
            info.AddValue(nameof(ShoppingCartCompanyAddress), ShoppingCartCompanyAddress);
            info.AddValue(nameof(OtherPaymentsSummary), OtherPaymentsSummary);
            info.AddValue(nameof(IsCreatedFromOrder), IsCreatedFromOrder);
            info.AddValue(nameof(Order), Order);
            info.AddValue(nameof(OrderId), OrderId);
            info.AddValue(nameof(OrderDiscount), OrderDiscount);
            info.AddValue(nameof(OrderDiscountSummary), OrderDiscountSummary);
            info.AddValue(nameof(PaymentGatewayCustomData), PaymentGatewayCustomData);
            info.AddValue(nameof(ShoppingCartCustomData), ShoppingCartCustomData);
            info.AddValue(nameof(ShoppingCartPaymentOptionID), ShoppingCartPaymentOptionID);
            info.AddValue(nameof(ShoppingCartShippingOptionID), ShoppingCartShippingOptionID);
            info.AddValue(nameof(GrandTotal), GrandTotal);
            info.AddValue(nameof(OtherPayments), OtherPayments);
            info.AddValue(nameof(TotalPrice), TotalPrice);
            info.AddValue(nameof(TotalItemsPrice), TotalItemsPrice);
            info.AddValue(nameof(TotalShipping), TotalShipping);
            info.AddValue(nameof(TotalTax), TotalTax);
            info.AddValue(nameof(TaxSummary), TaxSummary);
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected ShoppingCartInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
            mCartItems = (List<ShoppingCartItemInfo>)info.GetValue(nameof(CartItems), typeof(List<ShoppingCartItemInfo>));
            CouponCodes = (ICouponCodeCollection)info.GetValue(nameof(CouponCodes), typeof(ICouponCodeCollection));
            mCustomer = (CustomerInfo)info.GetValue(nameof(Customer), typeof(CustomerInfo));
            mShoppingCartCulture = (string)info.GetValue(nameof(ShoppingCartCulture), typeof(string));

            PrivateDataCleared = (bool)info.GetValue(nameof(PrivateDataCleared), typeof(bool));

            User = (UserInfo)info.GetValue(nameof(User), typeof(UserInfo));
            SiteName = (string)info.GetValue(nameof(SiteName), typeof(string));

            ShoppingCartBillingAddress = (IAddress)info.GetValue(nameof(ShoppingCartBillingAddress), typeof(IAddress));
            ShoppingCartShippingAddress = (IAddress)info.GetValue(nameof(ShoppingCartShippingAddress), typeof(IAddress));
            ShoppingCartCompanyAddress = (IAddress)info.GetValue(nameof(ShoppingCartCompanyAddress), typeof(IAddress));

            IsCreatedFromOrder = (bool)info.GetValue(nameof(IsCreatedFromOrder), typeof(bool));
            Order = (OrderInfo)info.GetValue(nameof(Order), typeof(OrderInfo));
            OrderId = (int)info.GetValue(nameof(OrderId), typeof(int));
            OrderDiscount = (decimal)info.GetValue(nameof(OrderDiscount), typeof(decimal));

            PaymentGatewayCustomData = (Hashtable)info.GetValue(nameof(PaymentGatewayCustomData), typeof(Hashtable));
            mShoppingCartCustomData = (ContainerCustomData)info.GetValue(nameof(ShoppingCartCustomData), typeof(ContainerCustomData));

            ShoppingCartPaymentOptionID = (int)info.GetValue(nameof(ShoppingCartPaymentOptionID), typeof(int));
            ShoppingCartShippingOptionID = (int)info.GetValue(nameof(ShoppingCartShippingOptionID), typeof(int));

            GrandTotal = (decimal)info.GetValue(nameof(GrandTotal), typeof(decimal));
            OtherPayments = (decimal)info.GetValue(nameof(OtherPayments), typeof(decimal));
            TotalPrice = (decimal)info.GetValue(nameof(TotalPrice), typeof(decimal));
            TotalItemsPrice = (decimal)info.GetValue(nameof(TotalItemsPrice), typeof(decimal));
            TotalShipping = (decimal)info.GetValue(nameof(TotalShipping), typeof(decimal));
            TotalTax = (decimal)info.GetValue(nameof(TotalTax), typeof(decimal));

            OrderDiscountSummary = (ValuesSummary)info.GetValue(nameof(OrderDiscountSummary), typeof(ValuesSummary));
            TaxSummary = (ValuesSummary)info.GetValue(nameof(TaxSummary), typeof(ValuesSummary));
            OtherPaymentsSummary = (ValuesSummary)info.GetValue(nameof(OtherPaymentsSummary), typeof(ValuesSummary));
        }

        #endregion


        #region "Public methods - "Calculations and price"

        /// <summary>
        /// Returns remaining amount for free shipping.
        /// Method returns 0 if the cart is null or if there is no valid discount or if free shipping is already applied.
        /// Call this method on an evaluated cart <see cref="ShoppingCartInfo.Evaluate"/>.
        /// The remaining amount is calculated from <see cref="ShoppingCartInfo.TotalItemsPrice"/>.
        /// </summary>
        public decimal CalculateRemainingAmountForFreeShipping()
        {
            return ShoppingCartInfoProvider.CalculateRemainingAmountForFreeShipping(this);
        }

        #endregion


        #region "Public methods - Coupon codes"

        /// <summary>
        /// Applies the specified coupon code to the shopping cart.
        /// </summary>
        /// <param name="couponCode">Shopping cart coupon code to apply</param>
        public bool AddCouponCode(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            if (CouponCodes.Codes.Any(x => ECommerceHelper.CouponCodeComparer.Equals(x.Code, couponCode)))
            {
                return false;
            }

            CouponCodes.Add(couponCode, CouponCodeApplicationStatusEnum.Invalid);

            Evaluate();

            bool isApplied = CouponCodes.IsAppliedInCart(couponCode);

            if (isApplied)
            {
                if (ShoppingCartID > 0)
                {
                    ShoppingCartCouponCodeInfoProvider.SetShoppingCartCouponCodeInfo(new ShoppingCartCouponCodeInfo
                    {
                        ShoppingCartID = ShoppingCartID,
                        CouponCode = couponCode
                    });
                }
            }

            return isApplied;
        }


        /// <summary>
        /// Removes the specified coupon code from the shopping cart.
        /// </summary>
        /// <param name="couponCode">Shopping cart coupon code to remove</param>
        public void RemoveCouponCode(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                throw new ArgumentNullException(nameof(couponCode));
            }

            if (!CouponCodes.Codes.Any(x => ECommerceHelper.CouponCodeComparer.Equals(x.Code, couponCode)))
            {
                return;
            }

            CouponCodes.Remove(couponCode);

            if (ShoppingCartID > 0)
            {
                ShoppingCartCouponCodeInfoProvider.DeleteShoppingCartCouponCodeInfo(ShoppingCartID, couponCode);
            }

            Evaluate();
        }

        #endregion


        #region "Public methods - Shopping cart owner"

        /// <summary>
        /// Sets user in shopping cart (if registered and not anonymous).
        /// </summary>
        /// <param name="customerId">ID of a customer linked with the user.</param>
        public void SetShoppingCartUser(int customerId)
        {
            var customer = CustomerInfoProvider.GetCustomerInfo(customerId);

            if (customer != null && ShoppingCartCustomerID != customerId)
            {
                ShoppingCartCustomerID = customerId;

                if (customer.CustomerIsRegistered)
                {
                    User = UserInfoProvider.GetUserInfo(customer.CustomerUserID) ?? AuthenticationHelper.GlobalPublicUser;
                }
                else
                {
                    User = null;
                }

                SetPreferences(customer);
                Evaluate();
            }
        }


        /// <summary>
        /// Sets preferences for given customer in shopping cart
        /// </summary>
        /// <remarks>
        /// Evaluation is not required because the shopping cart is modified later by adding items.
        /// </remarks>
        private void SetPreferences(CustomerInfo customer)
        {
            var preferences = customer.GetPreferences(ShoppingCartSiteID);

            if (preferences.PaymentOptionID.HasValue)
            {
                ShoppingCartPaymentOptionID = preferences.PaymentOptionID.Value;
            }

            if (preferences.ShippingOptionID.HasValue)
            {
                ShoppingCartShippingOptionID = preferences.ShippingOptionID.Value;
            }
        }


        private void InvalidateCustomer()
        {
            mCustomer = null;
            mCustomerInitialized = false;
        }

        #endregion
    }
}