using System.ComponentModel;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing access to E-commerce related settings.
    /// </summary>
    public static class ECommerceSettings
    {
        #region "Constants"

        /// <summary>
        /// Represents CMSStoreAllowAnonymousCustomers e-commerce setting key name.
        /// </summary>
        public const string ALLOW_ANONYMOUS_CUSTOMERS = "CMSStoreAllowAnonymousCustomers";

        /// <summary>
        /// Represents CMSShoppingCartURL e-commerce setting key name.
        /// </summary>
        public const string SHOPPINGCART_URL = "CMSShoppingCartURL";

        /// <summary>
        /// Represents CMSWishlistURL e-commerce setting key name.
        /// </summary>
        public const string WISHLIST_URL = "CMSWishlistURL";

        /// <summary>
        /// Represents CMSDefaultProductImageURL e-commerce setting key name.
        /// </summary>
        public const string DEFAULT_PRODUCT_IMAGE_URL = "CMSDefaultProductImageURL";

        /// <summary>
        /// Represents CMSStoreRedirectToShoppingCart e-commerce setting key name.
        /// </summary>
        public const string REDIRECT_TO_SHOPPINGCART = "CMSStoreRedirectToShoppingCart";

        /// <summary>
        /// Represents CMSStoreInvoiceTemplate e-commerce setting key name.
        /// </summary>
        public const string INVOICE_TEMPLATE = "CMSStoreInvoiceTemplate";

        /// <summary>
        /// Represents CMSStoreDefaultCountryName e-commerce setting key name.
        /// </summary>
        public const string DEFAULT_COUNTRY_NAME = "CMSStoreDefaultCountryName";

        /// <summary>
        /// Represents CMSStoreShowTaxRegistrationID e-commerce setting key name.
        /// </summary>
        public const string SHOW_TAX_REGISTRATION_ID = "CMSStoreShowTaxRegistrationID";

        /// <summary>
        /// Represents CMSStoreShowOrganizationID e-commerce setting key name.
        /// </summary>
        public const string SHOW_ORGANIZATION_ID = "CMSStoreShowOrganizationID";

        /// <summary>
        /// Represents CMSStoreSendEmailsFrom e-commerce setting key name.
        /// </summary>
        public const string SEND_EMAILS_FROM = "CMSStoreSendEmailsFrom";

        /// <summary>
        /// Represents CMSStoreSendEmailsTo e-commerce setting key name.
        /// </summary>
        public const string SEND_EMAILS_TO = "CMSStoreSendEmailsTo";

        /// <summary>
        /// Represents CMSStoreUseExtraCompanyAddress e-commerce setting key name.
        /// </summary>
        public const string USE_EXTRA_COMPANYADDRESS = "CMSStoreUseExtraCompanyAddress";

        /// <summary>
        /// Represents CMSStoreSendOrderNotification e-commerce setting key name.
        /// </summary>
        public const string SEND_ORDER_NOTIFICATION = "CMSStoreSendOrderNotification";

        /// <summary>
        /// Represents CMSStoreSendPaymentNotification e-commerce setting key name.
        /// </summary>
        public const string SEND_PAYMENT_NOTIFICATION = "CMSStoreSendPaymentNotification";

        /// <summary>
        /// Represents CMSStoreCheckoutProcess e-commerce setting key name.
        /// </summary>
        public const string CHECKOUT_PROCESS = "CMSStoreCheckoutProcess";

        /// <summary>
        /// Represents CMSStoreRequireCompanyInfo e-commerce setting key name.
        /// </summary>
        public const string REQUIRE_COMPANYINFO = "CMSStoreRequireCompanyInfo";

        /// <summary>
        /// Represents CMSStoreApplyTaxBasedOn e-commerce setting key name.
        /// </summary>
        public const string APPLY_TAXES_BASED_ON = "CMSStoreApplyTaxesBasedOn";

        /// <summary>
        /// Represents CMSStoreWeightFormattingString e-commerce setting key name.
        /// </summary>
        public const string WEIGHT_FORMATTING_STRING = "CMSStoreWeightFormattingString";

        /// <summary>
        /// Represents CMSStoreMassUnit e-commerce setting key name.
        /// </summary>
        public const string MASS_UNIT = "CMSStoreMassUnit";

        /// <summary>
        /// Represents CMSStoreUseCustomerCultureForEmails e-commerce settings key name.
        /// </summary>
        public const string USE_CUSTOMER_CULTURE_FOR_EMAILS = "CMSStoreUseCustomerCultureForEmails";

        /// <summary>
        /// Represents CMSStoreAllowGlobalProducts e-commerce setting key name.
        /// </summary>
        public const string ALLOW_GLOBAL_PRODUCTS = "CMSStoreAllowGlobalProducts";

        /// <summary>
        /// Represents CMSStoreAllowGlobalProductOptions e-commerce setting key name.
        /// </summary>
        public const string ALLOW_GLOBAL_PRODUCT_OPTIONS = "CMSStoreAllowGlobalProductOptions";

        /// <summary>
        /// Represents CMSStoreAllowGlobalManufacturers e-commerce setting key name.
        /// </summary>
        public const string ALLOW_GLOBAL_MANUFACTURERS = "CMSStoreAllowGlobalManufacturers";

        /// <summary>
        /// Represents CMSStoreAllowGlobalSuppliers e-commerce setting key name.
        /// </summary>
        public const string ALLOW_GLOBAL_SUPPLIERS = "CMSStoreAllowGlobalSuppliers";

        /// <summary>
        /// Represents CMSStoreAllowGlobalDepartments e-commerce setting key name.
        /// </summary>
        public const string ALLOW_GLOBAL_DEPARTMENTS = "CMSStoreAllowGlobalDepartments";

        /// <summary>
        /// Represents CMSStoreAllowGlobalPaymentMethods e-commerce setting key name.
        /// </summary>
        public const string ALLOW_GLOBAL_PAYMENT_METHODS = "CMSStoreAllowGlobalPaymentMethods";

        /// <summary>
        /// Represents CMSStoreUseGlobalTaxClasses e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_TAX_CLASSES = "CMSStoreUseGlobalTaxClasses";

        /// <summary>
        /// Represents CMSStoreUseGlobalCurrencies e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_CURRENCIES = "CMSStoreUseGlobalCurrencies";

        /// <summary>
        /// Represents CMSStoreUseGlobalExchangeRates e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_EXCHANGE_RATES = "CMSStoreUseGlobalExchangeRates";

        /// <summary>
        /// Represents CMSStoreUseGlobalCredit e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_CREDIT = "CMSStoreUseGlobalCredit";

        /// <summary>
        /// Represents CMSStoreUseGlobalOrderStatus e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_ORDER_STATUS = "CMSStoreUseGlobalOrderStatus";

        /// <summary>
        /// Represents CMSStoreUseGlobalPublicStatus e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_PUBLIC_STATUS = "CMSStoreUseGlobalPublicStatus";

        /// <summary>
        /// Represents CMSStoreUseGlobalInternalStatus e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_INTERNAL_STATUS = "CMSStoreUseGlobalInternalStatus";

        /// <summary>
        /// Represents CMSStoreUseGlobalInvoice e-commerce setting key name.
        /// </summary>
        public const string USE_GLOBAL_INVOICE = "CMSStoreUseGlobalInvoice";

        /// <summary>
        /// Pattern (macro expression) used to generate an invoice number.
        /// </summary>
        public const string INVOICE_NUMBER_PATTERN = "CMSStoreInvoiceNumberPattern";

        /// <summary>
        /// Represents CMSStoreEProductsReminder e-commerce setting key name.
        /// </summary>
        public const string EPRODUCTS_REMINDER = "CMSStoreEProductsReminder";

        /// <summary>
        /// Represents CMSStoreRegistrationConversionName e-commerce setting key name.
        /// </summary>
        public const string REGISTRATION_CONVERSION_NAME = "CMSStoreRegistrationConversionName";

        /// <summary>
        /// Represents CMSStoreRegistrationConversionValue e-commerce setting key name.
        /// </summary>
        public const string REGISTRATION_CONVERSION_VALUE = "CMSStoreRegistrationConversionValue";

        /// <summary>
        /// Represents CMSStoreOrderConversionName e-commerce setting key name.
        /// </summary>
        public const string ORDER_CONVERSION_NAME = "CMSStoreOrderConversionName";

        /// <summary>
        /// Represents CMSStoreOrderConversionValue e-commerce setting key name.
        /// </summary>
        public const string ORDER_CONVERSION_VALUE = "CMSStoreOrderConversionValue";

        /// <summary>
        /// Represents CMSStoreAddToShoppingCartConversionName e-commerce setting key name.
        /// </summary>
        public const string ADD_TO_CART_CONVERSION_NAME = "CMSStoreAddToShoppingCartConversionName";

        /// <summary>
        /// Represents CMSStoreAddToShoppingCartConversionValue e-commerce setting key name.
        /// </summary>
        public const string ADD_TO_CART_CONVERSION_VALUE = "CMSStoreAddToShoppingCartConversionValue";

        /// <summary>
        /// Represents CMSStoreProductsStartingPath e-commerce setting key name.
        /// </summary>
        public const string PRODUCTS_STARTING_PATH = "CMSStoreProductsStartingPath";

        /// <summary>
        /// Represents CMSStoreAllowProductsWithoutDocuments e-commerce setting key name.
        /// </summary>
        public const string ALLOW_PRODUCTS_WITHOUT_DOCUMENTS = "CMSStoreAllowProductsWithoutDocuments";

        /// <summary>
        /// Represents CMSStoreProductsTree e-commerce setting key name.
        /// </summary>
        public const string PRODUCTS_TREE = "CMSStoreProductsTree";

        /// <summary>
        /// Represents CMSStoreProductsAreNewFor e-commerce setting key name.
        /// </summary>
        public const string PRODUCTS_ARE_NEW_FOR = "CMSStoreProductsAreNewFor";

        /// <summary>
        /// Represents CMSStoreRelatedProductsRelationshipName e-commerce setting key name.
        /// </summary>
        public const string RELATED_PRODUCTS_RELATIONSHIP_NAME = "CMSStoreRelatedProductsRelationshipName";

        /// <summary>
        /// Represents CMSStoreNewProductStatus e-commerce setting key name.
        /// </summary>
        public const string NEW_PRODUCT_STATUS = "CMSStoreNewProductStatus";

        /// <summary>
        /// Represents CMSStoreDisplayProductsInSectionsTree e-commerce setting key name.
        /// </summary>
        public const string DISPLAY_PRODUCTS_IN_SECTIONS_TREE = "CMSStoreDisplayProductsInSectionsTree";

        /// <summary>
        /// Represents CMSCheapestVariantAdvertising e-commerce setting key name.
        /// </summary>
        public const string CHEAPEST_VARIANT_ADVERTISING = "CMSCheapestVariantAdvertising";

        /// <summary>
        /// Represents CMSStoreAutoRegisterCustomer e-commerce setting key name.
        /// </summary>
        public const string AUTOMATIC_REGISTRATION_CUSTOMER = "CMSStoreAutoRegisterCustomer";

        /// <summary>
        /// Represents CMSStoreAutoRegistrationEmailTemplate e-commerce setting key name.
        /// </summary>
        public const string AUTOMATIC_REGISTRATION_EMAIL_TEMPLATE = "CMSStoreAutoRegistrationEmailTemplate";

        /// <summary>
        /// Represents CMSShoppingCartExpirationPeriod e-commerce setting key name.
        /// </summary>
        public const string SHOPPING_CART_EXPIRATION_PERIOD = "CMSShoppingCartExpirationPeriod";

        /// <summary>
        /// Represents CMSMarkShoppingCartAsAbandonedPeriod e-commerce setting key name.
        /// </summary>
        public const string MARK_SHOPPING_CART_AS_ABANDONED_PERIOD = "CMSMarkShoppingCartAsAbandonedPeriod";

        /// <summary>
        /// Represents name of setting key determining wheteher the product price includes tax or not.
        /// </summary>
        internal const string INCLUDE_TAX_IN_PRICES = "CMSIncludeTaxInPrices";
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if meta file should be used for product image. Value is taken from webconfig key CMSUseMetaFileForProductImage.
        /// Default value is true.
        /// </summary>
        public static bool UseMetaFileForProductImage
        {
            get
            {
                return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseMetaFileForProductImage"], true);
            }
        }


        /// <summary>
        /// Time (minutes) used for caches used in ecommerce providers. Default value is 10 minutes.
        /// </summary>
        public static double ProvidersCacheMinutes
        {
            get
            {
                return 10;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates if customers need to register in your site so that they can make the purchase.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowAnonymousCustomers(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_ANONYMOUS_CUSTOMERS, siteIdentifier);
        }


        /// <summary>
        /// Gets the URL of the shopping cart page.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string ShoppingCartURL(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(SHOPPINGCART_URL, siteIdentifier);
        }


        /// <summary>
        /// Gets the URL of the wish list page.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string WishListURL(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(WISHLIST_URL, siteIdentifier);
        }


        /// <summary>
        /// Gets the URL of the default product image which is used when image of the product is not specified.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string DefaultProductImageURL(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(DEFAULT_PRODUCT_IMAGE_URL, siteIdentifier);
        }


        /// <summary>
        /// Indicates if user will be redirected to shopping cart after adding product to cart.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool RedirectToShoppingCart(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(REDIRECT_TO_SHOPPINGCART, siteIdentifier);
        }


        /// <summary>
        /// Gets HTML template used to generate an invoice (or receipt) after an order is finished and saved.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string InvoiceTemplate(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(INVOICE_TEMPLATE, siteIdentifier);
        }


        /// <summary>
        /// Gets default country, choose your country or country where you sell the most. All taxes are applied based on their values in the default country unless the customer specifies their country or state during the checkout process.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string DefaultCountryName(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(DEFAULT_COUNTRY_NAME, siteIdentifier);
        }


        /// <summary>
        /// Indicates if organization ID field (e.g. VAT registration ID) should be displayed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool ShowTaxRegistrationID(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(SHOW_TAX_REGISTRATION_ID, siteIdentifier);
        }


        /// <summary>
        /// Indicates if organization ID field should be displayed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool ShowOrganizationID(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(SHOW_ORGANIZATION_ID, siteIdentifier);
        }


        /// <summary>
        /// Gets an e-mail address the e-commerce notification e-mails are sent from.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string SendEmailsFrom(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(SEND_EMAILS_FROM, siteIdentifier);
        }


        /// <summary>
        /// Gets site-related e-mail address (e.g. merchant's e-mail address) the e-commerce notification e-mails are sent to.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string SendEmailsTo(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(SEND_EMAILS_TO, siteIdentifier);
        }


        /// <summary>
        /// Indicates if the option of providing company address is available in the check out process.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseExtraCompanyAddress(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_EXTRA_COMPANYADDRESS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if e-mail notifications are sent after an order is finished and saved.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool SendOrderNotification(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(SEND_ORDER_NOTIFICATION, siteIdentifier);
        }


        /// <summary>
        /// Indicates if e-mail notifications are sent after an order payment is completed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool SendPaymentNotification(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(SEND_PAYMENT_NOTIFICATION, siteIdentifier);
        }


        /// <summary>
        /// Gets the checkout process used in the store shopping cart.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string CheckoutProcess(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(CHECKOUT_PROCESS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if it is compulsory to provide company account information in the check out process.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool RequireCompanyInfo(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(REQUIRE_COMPANYINFO, siteIdentifier);
        }


        /// <summary>
        /// Indicates whether the tax is applied based on shipping or billing address.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static ApplyTaxBasedOnEnum ApplyTaxesBasedOn(SiteInfoIdentifier siteIdentifier)
        {
            string val = SettingsKeyInfoProvider.GetValue(APPLY_TAXES_BASED_ON, siteIdentifier);

            return val.ToEnum<ApplyTaxBasedOnEnum>();
        }


        /// <summary>
        /// Gets the format used to display product weight. Use {0} expression to insert the weight into the formatting string.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string WeightFormattingString(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(WEIGHT_FORMATTING_STRING, siteIdentifier);
        }


        /// <summary>
        /// Gets the mass unit code used by the system to represent weights.
        /// </summary>
        public static string MassUnit()
        {
            return SettingsKeyInfoProvider.GetValue(MASS_UNIT, 0);
        }


        /// <summary>
        /// Indicates if emails to customer will be sent in shopping cart culture. 
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseCustomerCultureForEmails(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_CUSTOMER_CULTURE_FOR_EMAILS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global products are allowed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowGlobalProducts(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_GLOBAL_PRODUCTS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global product options are allowed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowGlobalProductOptions(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_GLOBAL_PRODUCT_OPTIONS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global manufacturers are allowed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowGlobalManufacturers(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_GLOBAL_MANUFACTURERS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global suppliers are allowed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowGlobalSuppliers(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_GLOBAL_SUPPLIERS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global departments are allowed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowGlobalDepartments(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_GLOBAL_DEPARTMENTS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global payment methods are allowed.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowGlobalPaymentMethods(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_GLOBAL_PAYMENT_METHODS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global tax classes are used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalTaxClasses(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_TAX_CLASSES, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global currencies are used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalCurrencies(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_CURRENCIES, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global exchange rates are used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalExchangeRates(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_EXCHANGE_RATES, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global credit is used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalCredit(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_CREDIT, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global order statuses are used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalOrderStatus(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_ORDER_STATUS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global public statuses are used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalPublicStatus(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_PUBLIC_STATUS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global internal statuses are used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalInternalStatus(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_INTERNAL_STATUS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global invoice is used.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool UseGlobalInvoice(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(USE_GLOBAL_INVOICE, siteIdentifier);
        }


        /// <summary>
        /// Pattern (macro expression) for generating of invoice number.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string InvoiceNumberPattern(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(INVOICE_NUMBER_PATTERN, siteIdentifier);
        }


        /// <summary>
        /// Gets number of days before e-product expiration when reminder is to be sent.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static int EProductsReminder(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetIntValue(EPRODUCTS_REMINDER, siteIdentifier);
        }


        /// <summary>
        /// Path within the content tree where sub-tree of product sections starts.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string ProductsStartingPath(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(PRODUCTS_STARTING_PATH, siteIdentifier);
        }


        /// <summary>
        /// Indicates whether it is possible to create products without document data.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AllowProductsWithoutDocuments(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(ALLOW_PRODUCTS_WITHOUT_DOCUMENTS, siteIdentifier);
        }


        /// <summary>
        /// Returns the mode of displaying tree in the products administration UI.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static ProductsTreeModeEnum ProductsTree(SiteInfoIdentifier siteIdentifier)
        {
            string val = SettingsKeyInfoProvider.GetValue(PRODUCTS_TREE, siteIdentifier);

            return val.ToEnum<ProductsTreeModeEnum>();
        }


        /// <summary>
        /// Number of days for which created products are marked as 'new products' in the store. 
        /// Days are counted from the product 'In store from' property.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static int ProductsAreNewFor(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetIntValue(PRODUCTS_ARE_NEW_FOR, siteIdentifier);
        }


        /// <summary>
        /// Relationship name to be used for related products.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string RelatedProductsRelationshipName(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(RELATED_PRODUCTS_RELATIONSHIP_NAME, siteIdentifier);
        }


        /// <summary>
        /// Product public status which represents the status of the product which is evaluated as a new product 
        /// (according to the 'Products are new for' setting and 'In store from' product property).        
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string NewProductStatus(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(NEW_PRODUCT_STATUS, siteIdentifier);
        }


        /// <summary>
        /// Indicates if products will be displayed in products sections tree. This setting has no effect when not using sections tree.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool DisplayProductsInSectionsTree(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(DISPLAY_PRODUCTS_IN_SECTIONS_TREE, siteIdentifier);
        }


        /// <summary>
        /// Indicates if global objects of given type are allowed/used on the site specified by site name.
        /// </summary>
        /// <param name="siteIdentifier">Name or ID of the site.</param>
        /// <param name="objectType">Type of object to get settings value for.</param>
        public static bool AllowGlobalObjects(SiteInfoIdentifier siteIdentifier, string objectType)
        {
            string keyName = GetGlobalKeyForType(objectType);

            return SettingsKeyInfoProvider.GetBoolValue(keyName, siteIdentifier);
        }


        /// <summary>
        /// Indicates if ecommerce object of given type can be combined site and global within one site. 
        /// False means that site can use only global or only site records.
        /// </summary>
        /// <param name="objectType">Type of ecommerce object to get settings value for.</param>
        public static bool AllowCombineSiteAndGlobal(string objectType)
        {
            switch (objectType)
            {
                case SKUInfo.OBJECT_TYPE_SKU:
                case OptionCategoryInfo.OBJECT_TYPE:
                case SKUInfo.OBJECT_TYPE_OPTIONSKU:
                case ManufacturerInfo.OBJECT_TYPE:
                case SupplierInfo.OBJECT_TYPE:
                case DepartmentInfo.OBJECT_TYPE:
                case ShippingOptionInfo.OBJECT_TYPE:
                case PaymentOptionInfo.OBJECT_TYPE:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Indicates if the system updates the product price, visible in the product listing, automatically with the price of the cheapest product variant.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool CheapestVariantAdvertising(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(CHEAPEST_VARIANT_ADVERTISING, siteIdentifier);
        }


        /// <summary>
        /// Indicates if customer is automatically registered after checkout process and email is send.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static bool AutomaticCustomerRegistration(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetBoolValue(AUTOMATIC_REGISTRATION_CUSTOMER, siteIdentifier);
        }


        /// <summary>
        /// Returns email template code name for automatic customer registration email notification.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string AutomaticRegistrationEmailTemplateCodeName(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(AUTOMATIC_REGISTRATION_EMAIL_TEMPLATE, siteIdentifier);
        }


        /// <summary>
        /// Number of days before the shopping cart is expired.
        /// Default value is 30 days.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static int ShoppingCartExpirationPeriod(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetIntValue(SHOPPING_CART_EXPIRATION_PERIOD, siteIdentifier);
        }


        /// <summary>
        /// Number of hours after which are shopping carts marked as abandoned.
        /// Default value is 4 hours.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static double MarkShoppingCartAsAbandonedPeriod(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetDoubleValue(MARK_SHOPPING_CART_AS_ABANDONED_PERIOD, siteIdentifier);
        }


        /// <summary>
        /// Returns settings key name allowing global object of given object type.
        /// </summary>
        /// <param name="objectType">Object type name</param>
        public static string GetGlobalKeyForType(string objectType)
        {
            switch (objectType)
            {
                case SKUInfo.OBJECT_TYPE_SKU:
                    return ALLOW_GLOBAL_PRODUCTS;

                case OptionCategoryInfo.OBJECT_TYPE:
                case SKUInfo.OBJECT_TYPE_OPTIONSKU:
                    return ALLOW_GLOBAL_PRODUCT_OPTIONS;

                case ManufacturerInfo.OBJECT_TYPE:
                    return ALLOW_GLOBAL_MANUFACTURERS;

                case SupplierInfo.OBJECT_TYPE:
                    return ALLOW_GLOBAL_SUPPLIERS;

                case DepartmentInfo.OBJECT_TYPE:
                    return ALLOW_GLOBAL_DEPARTMENTS;

                case PaymentOptionInfo.OBJECT_TYPE:
                    return ALLOW_GLOBAL_PAYMENT_METHODS;

                case TaxClassInfo.OBJECT_TYPE:
                    return USE_GLOBAL_TAX_CLASSES;

                case CurrencyInfo.OBJECT_TYPE:
                    return USE_GLOBAL_CURRENCIES;

                case ExchangeRateInfo.OBJECT_TYPE:
                case ExchangeTableInfo.OBJECT_TYPE:
                    return USE_GLOBAL_EXCHANGE_RATES;

                case CreditEventInfo.OBJECT_TYPE:
                    return USE_GLOBAL_CREDIT;

                case OrderStatusInfo.OBJECT_TYPE:
                    return USE_GLOBAL_ORDER_STATUS;

                case PublicStatusInfo.OBJECT_TYPE:
                    return USE_GLOBAL_PUBLIC_STATUS;

                case InternalStatusInfo.OBJECT_TYPE:
                    return USE_GLOBAL_INTERNAL_STATUS;

                default:
                    throw new InvalidEnumArgumentException($"Settings key allowing usage of global object of type {objectType} not found.");
            }
        }


        /// <summary>
        /// Indicates whether prices on given <paramref name="site"/> already include tax.
        /// </summary>
        /// <param name="site">Code name or ID of the site. Use null or empty string for global setting.</param>
        public static bool IsTaxIncludedInPrices(SiteInfoIdentifier site)
        {
            return SettingsKeyInfoProvider.GetBoolValue(INCLUDE_TAX_IN_PRICES, site);
        }

        #endregion


        #region "Conversions"

        /// <summary>
        /// Name of the conversion that will be logged when a user successfully registers on the website through the checkout process.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string RegistrationConversionName(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(REGISTRATION_CONVERSION_NAME, siteIdentifier);
        }


        /// <summary>
        /// Number that will be recorded along with the Registration conversion when it is logged. 
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string RegistrationConversionValue(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(REGISTRATION_CONVERSION_VALUE, siteIdentifier);
        }


        /// <summary>
        /// Name of the conversion that will be logged when a user completes an order.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string OrderConversionName(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(ORDER_CONVERSION_NAME, siteIdentifier);
        }


        /// <summary>
        /// Number that will be recorded along with the Order conversion when it is logged. 
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string OrderConversionValue(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(ORDER_CONVERSION_VALUE, siteIdentifier);
        }


        /// <summary>
        /// Name of the conversion that will be logged when a user adds a product to the shopping cart.
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string AddToCartConversionName(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(ADD_TO_CART_CONVERSION_NAME, siteIdentifier);
        }


        /// <summary>
        /// Number that will be recorded along with the specified conversion when a product is added to the shopping cart. 
        /// </summary>
        /// <param name="siteIdentifier">Code name or ID of the site. Use null or empty string for global setting</param>
        public static string AddToCartConversionValue(SiteInfoIdentifier siteIdentifier)
        {
            return SettingsKeyInfoProvider.GetValue(ADD_TO_CART_CONVERSION_VALUE, siteIdentifier);
        }

        #endregion
    }
}