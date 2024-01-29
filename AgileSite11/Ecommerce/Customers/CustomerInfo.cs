using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(CustomerInfo), CustomerInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// CustomerInfo data container class.
    /// </summary>
    [Serializable]
    public class CustomerInfo : AbstractInfo<CustomerInfo>
    {
        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CUSTOMER;

        private UserInfo mCustomerUser;
        private OrdersCollection mAllOrders;
        private readonly Dictionary<int, OrdersCollection> mOrdersOnSites = new Dictionary<int, OrdersCollection>();
        private InfoObjectCollection<SKUInfo> mPurchasedProducts;
        private InfoObjectCollection<SKUInfo> mWishlist;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CustomerInfoProvider), OBJECT_TYPE, "ECommerce.Customer", "CustomerID", "CustomerLastModified", "CustomerGUID", null, "CustomerLastName", null, "CustomerSiteID", null, null)
        {
            // Child object types
            // Object dependencies
            DependsOn = new List<ObjectDependency>
                                    {
                                        new ObjectDependency("CustomerUserID", UserInfo.OBJECT_TYPE)
                                    },
            // Binding object types
            // - None

            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                }
            },

            // Others
            LogEvents = true,
            CheckDependenciesOnDelete = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsInvalidation = true,
            SupportsGlobalObjects = true,
            SupportsCloneToOtherSite = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                LogExport = true,
                AllowSingleExport = false,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                },
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Customer e-mail.
        /// </summary>
        public virtual string CustomerEmail
        {
            get
            {
                return GetStringValue("CustomerEmail", "");
            }
            set
            {
                SetValue("CustomerEmail", value);
            }
        }


        /// <summary>
        /// Customer fax.
        /// </summary>
        public virtual string CustomerFax
        {
            get
            {
                return GetStringValue("CustomerFax", "");
            }
            set
            {
                SetValue("CustomerFax", value);
            }
        }


        /// <summary>
        /// ID of the customer's user account. If set, it says that the customer is registered.
        /// </summary>
        public virtual int CustomerUserID
        {
            get
            {
                return GetIntegerValue("CustomerUserID", 0);
            }
            set
            {
                SetValue("CustomerUserID", value, (value > 0));
                mCustomerUser = null;
            }
        }


        /// <summary>
        /// Registered user information. It is null for anonymous customer.
        /// </summary>
        public virtual UserInfo CustomerUser
        {
            get
            {
                if (CustomerIsRegistered && ((mCustomerUser == null) || !mCustomerUser.Generalized.IsObjectValid))
                {
                    mCustomerUser = UserInfoProvider.GetUserInfo(CustomerUserID);
                }

                return mCustomerUser;
            }
        }


        /// <summary>
        /// Customer ID.
        /// </summary>
        public virtual int CustomerID
        {
            get
            {
                return GetIntegerValue("CustomerID", 0);
            }
            set
            {
                SetValue("CustomerID", value);

                // Invalidate collections
                mOrdersOnSites.Clear();
                mPurchasedProducts = null;
                mWishlist = null;
            }
        }


        /// <summary>
        /// Customer last name.
        /// </summary>
        public virtual string CustomerLastName
        {
            get
            {
                return GetStringValue("CustomerLastName", "");
            }
            set
            {
                SetValue("CustomerLastName", value);
            }
        }


        /// <summary>
        /// Customer phone.
        /// </summary>
        public virtual string CustomerPhone
        {
            get
            {
                return GetStringValue("CustomerPhone", "");
            }
            set
            {
                SetValue("CustomerPhone", value);
            }
        }


        /// <summary>
        /// Customer first name.
        /// </summary>
        public virtual string CustomerFirstName
        {
            get
            {
                return GetStringValue("CustomerFirstName", "");
            }
            set
            {
                SetValue("CustomerFirstName", value);
            }
        }


        /// <summary>
        /// Customer company name.
        /// </summary>
        public virtual string CustomerCompany
        {
            get
            {
                return GetStringValue("CustomerCompany", "");
            }
            set
            {
                SetValue("CustomerCompany", value);
            }
        }


        /// <summary>
        /// Customer GUID.
        /// </summary>
        public virtual Guid CustomerGUID
        {
            get
            {
                return GetGuidValue("CustomerGUID", Guid.Empty);
            }
            set
            {
                SetValue("CustomerGUID", value);
            }
        }


        /// <summary>
        /// Customer tax registration ID.
        /// </summary>
        public virtual string CustomerTaxRegistrationID
        {
            get
            {
                return GetStringValue("CustomerTaxRegistrationID", "");
            }
            set
            {
                SetValue("CustomerTaxRegistrationID", value, "");
            }
        }


        /// <summary>
        /// Customer organization ID.
        /// </summary>
        public virtual string CustomerOrganizationID
        {
            get
            {
                return GetStringValue("CustomerOrganizationID", "");
            }
            set
            {
                SetValue("CustomerOrganizationID", value, "");
            }
        }


        /// <summary>
        /// Date and time when the customer was created.
        /// </summary>
        public virtual DateTime CustomerCreated
        {
            get
            {
                return GetDateTimeValue("CustomerCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CustomerCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Date and time when the customer was last modified.
        /// </summary>
        public virtual DateTime CustomerLastModified
        {
            get
            {
                return GetDateTimeValue("CustomerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CustomerLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Customer site ID.
        /// </summary>
        public virtual int CustomerSiteID
        {
            get
            {
                return GetIntegerValue("CustomerSiteID", 0);
            }
            set
            {
                SetValue("CustomerSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Indicates if customer has login (meaning CustomerUserID is set).
        /// </summary>
        [RegisterProperty]
        public virtual bool CustomerIsRegistered
        {
            get
            {
                return (CustomerUserID > 0);
            }
        }


        /// <summary>
        /// Returns customer first name followed by last name and by company name when available. Format is "John Doe (My company)".
        /// </summary>
        [RegisterProperty]
        public virtual string CustomerInfoName
        {
            get
            {
                string customerName = string.Format("{0} {1}", CustomerFirstName, CustomerLastName);

                // Show customer name and company in brackets, if company specified
                if (!string.IsNullOrEmpty(CustomerCompany))
                {
                    return string.Format("{0} ({1})", customerName, CustomerCompany);
                }

                return customerName;
            }
        }


        /// <summary>
        /// Indicates if customer has company information (company name, org id or tax id) filled.
        /// </summary>
        [RegisterProperty]
        public virtual bool CustomerHasCompanyInfo
        {
            get
            {
                return !string.IsNullOrEmpty(CustomerCompany) ||
                       !string.IsNullOrEmpty(CustomerOrganizationID) ||
                       !string.IsNullOrEmpty(CustomerTaxRegistrationID);
            }
        }

        #endregion


        #region "Collections"

        /// <summary>
        /// Collection of customer orders made on current site.
        /// </summary>
        public virtual OrdersCollection Orders
        {
            get
            {
                int siteId = SiteContext.CurrentSiteID;

                // Create new collection for this site when not present
                if (!mOrdersOnSites.ContainsKey(siteId))
                {
                    var orders = new OrdersCollection(CustomerID, siteId);
                    mOrdersOnSites.Add(siteId, orders);

                    return orders;
                }

                return mOrdersOnSites[siteId];
            }
        }


        /// <summary>
        /// Collection of all customer orders.
        /// </summary>
        public OrdersCollection AllOrders
        {
            get
            {
                return mAllOrders ?? (mAllOrders = new OrdersCollection(CustomerID, -1));
            }
        }


        /// <summary>
        /// Collection of all customer purchased products from all sites.
        /// </summary>
        public virtual InfoObjectCollection<SKUInfo> PurchasedProducts
        {
            get
            {
                if (mPurchasedProducts == null)
                {
                    // Create collection of purchased products from all sites.
                    mPurchasedProducts = new InfoObjectCollection<SKUInfo>
                                             {
                                                 Where = new WhereCondition().WhereIn("SKUID", new IDQuery<OrderItemInfo>("OrderItemSKUID")
                                                                                                  .WhereIn("OrderItemOrderID",
                                                                                                    new IDQuery<OrderInfo>()
                                                                                                        .WhereTrue("OrderIsPaid")
                                                                                                        .WhereEquals("OrderCustomerID", CustomerID))),
                                                 OrderByColumns = "SKUName",
                                                 SiteID = -1
                                             };
                }

                return mPurchasedProducts;
            }
        }


        /// <summary>
        /// Collection of all customer wishlist items from all sites. It is empty, if customer is not registered.
        /// </summary>
        public virtual InfoObjectCollection<SKUInfo> Wishlist
        {
            get
            {
                if (mWishlist == null)
                {
                    // Create collection of wishlist items from all sites
                    mWishlist = new InfoObjectCollection<SKUInfo>
                                    {
                                        Where = new WhereCondition().WhereIn("SKUID", new IDQuery<WishlistItemInfo>("COM_Wishlist.SKUID")
                                                                                            .WhereIn("COM_Wishlist.UserID",
                                                                                                new IDQuery<CustomerInfo>("CustomerUserID")
                                                                                                    .WhereEquals("CustomerID", CustomerID))),
                                        OrderByColumns = "SKUName",
                                        SiteID = -1
                                    };
                }

                return mWishlist;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CustomerInfoProvider.DeleteCustomerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CustomerInfoProvider.SetCustomerInfo(this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Reset user assignment
            CustomerUserID = 0;

            Insert();
        }

        #endregion


        #region "Constructors and internals"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public CustomerInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CustomerInfo object.
        /// </summary>
        public CustomerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CustomerInfo object from the given DataRow.
        /// </summary>
        public CustomerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Registers properties of the object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Orders", m => m.Orders);
            RegisterProperty("AllOrders", m => m.AllOrders);
            RegisterProperty("PurchasedProducts", m => m.PurchasedProducts);
            RegisterProperty("Wishlist", m => m.Wishlist);
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
            return EcommercePermissions.CheckCustomersPermissions(permission, siteName, userInfo, exceptionOnFailure, base.CheckPermissionsInternal);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            ModuleCommands.OnlineMarketingRemoveCustomer(CustomerID);

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Returns customer informative name taken from CustomerInfoName property.
        /// </summary>
        public override string ToMacroString()
        {
            return CustomerInfoName;
        }


        /// <summary>
        /// Gets customer's preferences on the site specified by <paramref name="site"/> parameter.
        /// </summary>
        /// <param name="site">Identifier of the site for which the preferences are obtained.</param>
        /// <returns>Customer's preferences on given site.
        /// Returns <see cref="CustomerPreferences.Unknown"/> when preferences not found.</returns>
        public CustomerPreferences GetPreferences(SiteInfoIdentifier site)
        {
            var provider = Service.Resolve<ICustomerPreferencesProvider>();

            return provider != null ? provider.GetPreferences(this, site) : CustomerPreferences.Unknown;
        }

        #endregion
    }
}