using System.Web;

using CMS.Helpers;
using CMS.Base;
using CMS.Core;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// E-commerce context.
    /// </summary>
    [RegisterAllProperties]
    public class ECommerceContext : AbstractContext<ECommerceContext>
    {
        #region "Variables"

        private CountryInfo mCurrentCustomerCountry;
        private StateInfo mCurrentCustomerState;

        private ShoppingCartInfo mCurrentShoppingCart;
        private CurrencyInfo mCurrentCurrency;

        private SKUInfo mCurrentProduct;
        private ManufacturerInfo mCurrentManufacturer;
        private SupplierInfo mCurrentSupplier;
        private InternalStatusInfo mCurrentProductInternalStatus;
        private PublicStatusInfo mCurrentProductPublicStatus;
        private DepartmentInfo mCurrentProductDepartment;

        private bool? mMoreCurrenciesUsedOnSite;

        private bool? mOnlyOneShippingOptionsAvailableOnSite;
        private bool? mOnlyOnePaymentOptionsAvailableOnSite;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true, if more than one currency is used and enabled on current site, otherwise false.
        /// </summary>
        public static bool MoreCurrenciesUsedOnSite
        {
            get
            {
                if (!Current.mMoreCurrenciesUsedOnSite.HasValue)
                {
                    var currencies = CurrencyInfoProvider.GetCurrencies(SiteContext.CurrentSiteID, true)
                                                             .Columns("CurrencyID")
                                                             .TopN(2);

                    Current.mMoreCurrenciesUsedOnSite = currencies.Count > 1;
                }

                return Current.mMoreCurrenciesUsedOnSite.Value;
            }
        }


        /// <summary>
        /// Returns true, if there is only one shipping option available on current site, otherwise false.
        /// </summary>
        public static bool OnlyOneShippingOptionAvailableOnSite
        {
            get
            {
                if (!Current.mOnlyOneShippingOptionsAvailableOnSite.HasValue)
                {
                    var shippingOptions = ShippingOptionInfoProvider.GetShippingOptions(SiteContext.CurrentSiteID, true)
                        .Columns("ShippingOptionID")
                        .TopN(2);

                    Current.mOnlyOneShippingOptionsAvailableOnSite = shippingOptions.Count == 1;
                }

                return Current.mOnlyOneShippingOptionsAvailableOnSite.Value;
            }
        }


        /// <summary>
        /// Returns true, if there is only one payment option available on current site, otherwise false.
        /// </summary>
        public static bool OnlyOnePaymentOptionAvailableOnSite
        {
            get
            {
                if (!Current.mOnlyOnePaymentOptionsAvailableOnSite.HasValue)
                {
                    var paymentOptions = PaymentOptionInfoProvider.GetPaymentOptions(SiteContext.CurrentSiteID, true)
                        .Columns("PaymentOptionID")
                        .TopN(2);

                    Current.mOnlyOnePaymentOptionsAvailableOnSite = paymentOptions.Count == 1;
                }

                return Current.mOnlyOnePaymentOptionsAvailableOnSite.Value;
            }
        }


        /// /// <summary>
        /// Indicates if exchange rate from global main currency is missing in the last valid exchange table. Returns false if site uses
        /// global currencies or global exchange rates or code of site main currency is the same as code of global main currency.
        /// </summary>
        public static bool IsExchangeRateFromGlobalMainCurrencyMissing
        {
            get
            {
                var siteMainCode = CurrencyInfoProvider.GetMainCurrencyCode(SiteContext.CurrentSiteID);
                var globalMainCode = CurrencyInfoProvider.GetMainCurrencyCode(0);

                decimal rate = 1;
                return !CurrencyConverter.TryGetExchangeRate(globalMainCode, siteMainCode, SiteContext.CurrentSiteID, ref rate);
            }
        }


        /// <summary>
        /// Customer from current shopping cart.
        /// </summary>
        public static CustomerInfo CurrentCustomer
        {
            get
            {
                return CurrentShoppingCart?.Customer;
            }
        }


        /// <summary>
        /// Current customer country.
        /// </summary>
        public static CountryInfo CurrentCustomerCountry
        {
            get
            {
                // Load if not available
                if (Current.mCurrentCustomerCountry == null)
                {
                    // Get country from current customer
                    var customer = CurrentCustomer;
                    if (customer != null)
                    {
                        var customerAddress = ECommerceHelper.GetLastUsedOrDefaultAddress(customer.CustomerID);
                        // Omit if default address is returned
                        if (customerAddress.AddressID > 0)
                        {
                            Current.mCurrentCustomerCountry = CountryInfoProvider.GetCountryInfo(customerAddress.AddressCountryID);
                        }
                    }
                }

                return Current.mCurrentCustomerCountry;
            }
            set
            {
                Current.mCurrentCustomerCountry = value;
            }
        }


        /// <summary>
        /// Current customer state.
        /// </summary>
        public static StateInfo CurrentCustomerState
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentCustomerState == null)
                {
                    // Get state from current customer
                    var customer = CurrentCustomer;
                    if (customer != null)
                    {
                        var customerAddress = ECommerceHelper.GetLastUsedOrDefaultAddress(customer.CustomerID);
                        Current.mCurrentCustomerState = StateInfoProvider.GetStateInfo(customerAddress.AddressStateID);
                    }
                }

                return Current.mCurrentCustomerState;
            }
            set
            {
                Current.mCurrentCustomerState = value;
            }
        }


        /// <summary>
        /// Returns the current Shopping cart info.
        /// </summary>
        public static ShoppingCartInfo CurrentShoppingCart
        {
            get
            {
                if ((HttpContext.Current != null) && (Current.mCurrentShoppingCart == null))
                {
                    DebugHelper.SetContext("CurrentShoppingCart");

                    var user = MembershipContext.AuthenticatedUser;
                    var siteId = SiteContext.CurrentSiteID;
                    Current.mCurrentShoppingCart = Service.Resolve<ICurrentShoppingCartService>().GetCurrentShoppingCart(user, siteId);

                    DebugHelper.ReleaseContext();
                }

                return Current.mCurrentShoppingCart;
            }
            set
            {
                if ((HttpContext.Current != null) && (HttpContext.Current.Session != null))
                {
                    // Set context
                    Current.mCurrentShoppingCart = value;

                    Service.Resolve<ICurrentShoppingCartService>().SetCurrentShoppingCart(value);
                }
            }
        }


        /// <summary>
        /// Current currency.
        /// </summary>
        public static CurrencyInfo CurrentCurrency
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentCurrency == null)
                {
                    // Try to get currency from shopping cart, if this fails, then return main currency
                    if (CurrentShoppingCart?.Currency != null)
                    {
                        Current.mCurrentCurrency = CurrentShoppingCart.Currency;
                    }
                    else
                    {
                        Current.mCurrentCurrency = CurrencyInfoProvider.GetMainCurrency(SiteContext.CurrentSiteID);
                    }

                }

                return Current.mCurrentCurrency;
            }
            set
            {
                Current.mCurrentCurrency = value;
            }
        }


        /// <summary>
        /// Current manufacturer object according the URL parameter of the current request.
        /// It is available when the request contains parameters "manufacturerId" or "manufacturerName" with valid value of the manufacturer.
        /// </summary>
        public static ManufacturerInfo CurrentManufacturer
        {
            get
            {
                if (Current.mCurrentManufacturer == null)
                {
                    ManufacturerInfo man = null;
                    // Try to get by ID
                    int manId = QueryHelper.GetInteger("manufacturerid", 0);
                    if (manId > 0)
                    {
                        man = ManufacturerInfoProvider.GetManufacturerInfo(manId);
                    }

                    // Try to get by name
                    if (man == null)
                    {
                        string manName = QueryHelper.GetString("manufacturername", "");
                        if (manName != "")
                        {
                            man = ManufacturerInfoProvider.GetManufacturerInfo(manName, SiteContext.CurrentSiteName);
                        }
                    }

                    // Save for later use
                    Current.mCurrentManufacturer = man;
                }

                return Current.mCurrentManufacturer;
            }
            set
            {
                Current.mCurrentManufacturer = value;
            }
        }


        /// <summary>
        /// Current product.
        /// </summary>
        public static SKUInfo CurrentProduct
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentProduct == null)
                {
                    // Get SKUID from PageInfo and get the SKUInfo object
                    PageInfo pi = DocumentContext.CurrentPageInfo;
                    if (pi != null)
                    {
                        Current.mCurrentProduct = SKUInfoProvider.GetSKUInfo(pi.NodeSKUID);
                    }
                }

                return Current.mCurrentProduct;
            }
            set
            {
                Current.mCurrentProduct = value;

                CurrentProductDepartment = null;
                CurrentProductSupplier = null;
                CurrentProductInternalStatus = null;
                CurrentProductPublicStatus = null;
            }
        }


        /// <summary>
        /// Current product department.
        /// </summary>
        public static DepartmentInfo CurrentProductDepartment
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentProductDepartment == null)
                {
                    // Get department from current product
                    SKUInfo skuInfo = CurrentProduct;
                    if (skuInfo != null)
                    {
                        Current.mCurrentProductDepartment = DepartmentInfoProvider.GetDepartmentInfo(skuInfo.SKUDepartmentID);
                    }
                }

                return Current.mCurrentProductDepartment;
            }
            set
            {
                Current.mCurrentProductDepartment = value;
            }
        }


        /// <summary>
        /// Current product supplier.
        /// </summary>
        public static SupplierInfo CurrentProductSupplier
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentSupplier == null)
                {
                    // Get supplier from current product
                    SKUInfo skuInfo = CurrentProduct;
                    if (skuInfo != null)
                    {
                        Current.mCurrentSupplier = SupplierInfoProvider.GetSupplierInfo(skuInfo.SKUSupplierID);
                    }
                }

                return Current.mCurrentSupplier;
            }
            set
            {
                Current.mCurrentSupplier = value;
            }
        }


        /// <summary>
        /// Current product internal status.
        /// </summary>
        public static InternalStatusInfo CurrentProductInternalStatus
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentProductInternalStatus == null)
                {
                    // Get internal status from current product
                    SKUInfo skuInfo = CurrentProduct;
                    if (skuInfo != null)
                    {
                        Current.mCurrentProductInternalStatus = InternalStatusInfoProvider.GetInternalStatusInfo(skuInfo.SKUInternalStatusID);
                    }
                }

                return Current.mCurrentProductInternalStatus;
            }
            set
            {
                Current.mCurrentProductInternalStatus = value;
            }
        }


        /// <summary>
        /// Current product public status.
        /// </summary>
        public static PublicStatusInfo CurrentProductPublicStatus
        {
            get
            {
                // Load when unknown
                if (Current.mCurrentProductPublicStatus == null)
                {
                    // Get public status from current product
                    SKUInfo skuInfo = CurrentProduct;
                    if (skuInfo != null)
                    {
                        Current.mCurrentProductPublicStatus = PublicStatusInfoProvider.GetPublicStatusInfo(skuInfo.SKUPublicStatusID);
                    }
                }
                return Current.mCurrentProductPublicStatus;
            }
            set
            {
                Current.mCurrentProductPublicStatus = value;
            }
        }

        #endregion


        #region "Context methods - Advanced"

        /// <summary>
        /// Checks the specified ecommerce permission for current user.
        /// </summary>
        /// <param name="permissionName">Permission name to be checked</param>
        public static bool IsUserAuthorizedForPermission(string permissionName)
        {
            return ECommerceHelper.IsUserAuthorizedForPermission(permissionName, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific customer.
        /// 'EcommerceModify' OR 'ModifyCustomers' permission is checked.
        /// </summary>
        public static bool IsUserAuthorizedToModifyCustomer()
        {
            return CustomerInfoProvider.IsUserAuthorizedToModifyCustomer(SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify discounts.
        /// </summary>
        public static bool IsUserAuthorizedToModifyDiscount()
        {
            return DiscountInfoProvider.IsUserAuthorizedToModifyDiscount(SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if current user is authorized to modify specific manufacturer.
        /// For global manufacturer: 'EcommerceGlobalModify' permission is checked.
        /// For site-specific manufacturer: 'EcommerceModify' OR 'ModifyManufacturers' permission is checked.
        /// </summary>
        /// <param name="manufacturerObj">Manufacturer to be checked</param>
        public static bool IsUserAuthorizedToModifyManufacturer(ManufacturerInfo manufacturerObj)
        {
            return ManufacturerInfoProvider.IsUserAuthorizedToModifyManufacturer(manufacturerObj, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify manufacturers.
        /// </summary>
        /// <param name="global">For global manufacturers (global = True): 'EcommerceGlobalModify' permission is checked.
        /// For site-specific manufacturers (global = False): 'EcommerceModify' OR 'ModifyManufacturers' permission is checked.</param>
        public static bool IsUserAuthorizedToModifyManufacturer(bool global)
        {
            return ManufacturerInfoProvider.IsUserAuthorizedToModifyManufacturer(global, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific option category.
        /// For global option category: 'EcommerceGlobalModify' permission is checked.
        /// For site-specific manufacturer: 'EcommerceModify' OR 'ModifyProducts' permission is checked.
        /// </summary>
        /// <param name="categoryObj">Option category to be checked</param>
        public static bool IsUserAuthorizedToModifyOptionCategory(OptionCategoryInfo categoryObj)
        {
            return OptionCategoryInfoProvider.IsUserAuthorizedToModifyOptionCategory(categoryObj, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify option categories.
        /// </summary>
        /// <param name="global">For global option categories (global = True): 'EcommerceGlobalModify' permission is checked.
        /// For site-specific option categories (global = False): 'EcommerceModify' OR 'ModifyProducts' permission is checked.</param>
        public static bool IsUserAuthorizedToModifyOptionCategory(bool global)
        {
            return OptionCategoryInfoProvider.IsUserAuthorizedToModifyOptionCategory(global, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific SKU.
        /// For global SKU: 'EcommerceGlobalModify' permission is checked.
        /// For site-specific SKU: 'EcommerceModify' OR 'ModifyProducts' permission is checked.
        /// </summary>
        /// <param name="skuObj">SKU to be checked</param>
        public static bool IsUserAuthorizedToModifySKU(SKUInfo skuObj)
        {
            return SKUInfoProvider.IsUserAuthorizedToModifySKU(skuObj, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify SKUs.
        /// </summary>
        /// <param name="global">For global SKUs (global = True): 'EcommerceGlobalModify' permission is checked.
        /// For site-specific SKUs (global = False): 'EcommerceModify' OR 'ModifyProducts' permission is checked.</param>
        public static bool IsUserAuthorizedToModifySKU(bool global)
        {
            return SKUInfoProvider.IsUserAuthorizedToModifySKU(global, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific supplier.
        /// For global supplier: 'EcommerceGlobalModify' permission is checked.
        /// For site-specific supplier: 'EcommerceModify' OR 'ModifySuppliers' permission is checked.
        /// </summary>
        /// <param name="supplierObj">Supplier to be checked</param>
        public static bool IsUserAuthorizedToModifySupplier(SupplierInfo supplierObj)
        {
            return SupplierInfoProvider.IsUserAuthorizedToModifySupplier(supplierObj, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Indicates if user is authorized to modify suppliers.
        /// </summary>
        /// <param name="global">For global suppliers (global = True): 'EcommerceGlobalModify' permission is checked.
        /// For site-specific suppliers (global = False): 'EcommerceModify' OR 'ModifySuppliers' permission is checked.</param>
        public static bool IsUserAuthorizedToModifySupplier(bool global)
        {
            return SupplierInfoProvider.IsUserAuthorizedToModifySupplier(global, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
        }

        #endregion
    }
}