using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing SKUInfo management.
    /// </summary>
    public class SKUInfoProvider : AbstractInfoProvider<SKUInfo, SKUInfoProvider>
    {
        #region "Constructors and variables"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SKUInfoProvider()
            : base(SKUInfo.TYPEINFOSKU, new HashtableSettings
            {
                ID = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all products and product option.
        /// </summary>        
        public static ObjectQuery<SKUInfo> GetSKUs()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns product with specified ID.
        /// </summary>
        /// <param name="skuId">Product ID</param>        
        public static SKUInfo GetSKUInfo(int skuId)
        {
            return ProviderObject.GetSKUInfoInternal(skuId);
        }


        /// <summary>
        /// Returns product with specified GUID.
        /// </summary>
        /// <param name="skuGuid">Product GUID</param>        
        public static SKUInfo GetSKUInfo(Guid skuGuid)
        {
            return ProviderObject.GetSKUInfoInternal(skuGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified product.
        /// </summary>
        /// <param name="skuObj">Product to be set</param>
        public static void SetSKUInfo(SKUInfo skuObj)
        {         
            ProviderObject.SetSKUInfoInternal(skuObj);
        }


        /// <summary>
        /// Deletes specified product.
        /// </summary>
        /// <param name="skuObj">Product to be deleted</param>
        public static void DeleteSKUInfo(SKUInfo skuObj)
        {
            ProviderObject.DeleteSKUInfoInternal(skuObj);
        }


        /// <summary>
        /// Deletes product with specified ID.
        /// </summary>
        /// <param name="skuId">Product ID</param>
        public static void DeleteSKUInfo(int skuId)
        {
            var skuObj = GetSKUInfo(skuId);
            DeleteSKUInfo(skuObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns query for all SKUs for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param> 
        public static ObjectQuery<SKUInfo> GetSKUs(int siteId)
        {
            return ProviderObject.GetSKUsInternal(siteId);
        }


        /// <summary>
        /// Returns query for all product options from specified product option category.
        /// </summary>
        /// <param name="categoryId">Product option category ID</param> 
        /// <param name="onlyEnabled">Indicates if only enabled product options should be selected</param>
        public static ObjectQuery<SKUInfo> GetSKUOptions(int categoryId, bool onlyEnabled)
        {
            return ProviderObject.GetSKUOptionsForProductInternal(0, categoryId, onlyEnabled);
        }


        /// <summary>
        /// Returns query for all product options from specified product option category allowed for product specified by skuId parameter.
        /// </summary>
        /// <param name="skuId">ID of the SKU for which options are to be obtained</param>
        /// <param name="categoryId">Product option category ID</param> 
        /// <param name="onlyEnabled">Indicates if only enabled product options should be selected</param>
        public static ObjectQuery<SKUInfo> GetSKUOptionsForProduct(int skuId, int categoryId, bool onlyEnabled)
        {
            return ProviderObject.GetSKUOptionsForProductInternal(skuId, categoryId, onlyEnabled);
        }


        /// <summary>
        /// Returns query for all bundle items for specified bundle SKU ID.
        /// </summary>
        /// <param name="bundleId">Bundle SKU ID</param>
        public static ObjectQuery<SKUInfo> GetBundleItems(int bundleId)
        {
            return ProviderObject.GetBundleItemsInternal(bundleId);
        }


        /// <summary>
        /// Returns dataset with the products from specified user's wishlist.
        /// </summary>
        /// <param name="userId">ID of the user the wishlist belongs to</param>  
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<SKUInfo> GetWishlistProducts(int userId, int siteId)
        {
            return ProviderObject.GetWishlistProductsInternal(userId, siteId);
        }


        /// <summary>
        /// Returns user friendly URL of the specified product. Does not uses hash tables to crate SKU.
        /// </summary>
        /// <param name="skuGuid">Product unique identifier (GUID)</param>
        /// <param name="skuName">Product name</param>
        /// <param name="siteName">Site Name</param>
        public static string GetSKUUrl(Guid skuGuid, string skuName, string siteName = null)
        {
            return ProviderObject.GetSKUUrlInternal(skuGuid, skuName, siteName);
        }


        /// <summary>
        /// Returns user friendly URL of the specified product. Uses hash tables.
        /// </summary>
        /// <param name="skuID">Product ID</param>
        /// <param name="skuName">Product name</param>
        /// <param name="siteName">Site Name</param>
        public static string GetSKUUrl(int skuID, string skuName, string siteName = null)
        {
            return ProviderObject.GetSKUUrlInternal(skuID, skuName, siteName);
        }


        /// <summary>
        /// Updates products' available items according to the products' units of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart with some products</param>
        /// <exception cref="InvalidOperationException">When strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).</exception>
        public static void UpdateInventory(ShoppingCartInfo cart)
        {
            ProviderObject.UpdateInventoryInternal(cart);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific SKU. 
        /// For global SKU: 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific SKU: 'EcommerceModify' OR 'ModifyProducts' permission is checked.
        /// </summary>
        /// <param name="skuObj">SKU to be checked</param>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedToModifySKU(SKUInfo skuObj, string siteName, IUserInfo user)
        {
            return (skuObj != null) && IsUserAuthorizedToModifySKU(skuObj.IsGlobal, siteName, user);
        }


        /// <summary>
        /// Indicates if user is authorized to modify SKUs.
        /// </summary>
        /// <param name="global">For global SKUs (global = True): 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific SKUs (global = False): 'EcommerceModify' OR 'ModifyProducts' permission is checked.</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails.</param>
        public static bool IsUserAuthorizedToModifySKU(bool global, string siteName, IUserInfo user, bool exceptionOnFailure = false)
        {
            var permission = global ? EcommercePermissions.ECOMMERCE_MODIFYGLOBAL : EcommercePermissions.PRODUCTS_MODIFY;

            return ECommerceHelper.IsUserAuthorizedForPermission(permission, siteName, user, exceptionOnFailure);
        }


        /// <summary>
        /// Checks dependencies, returns true if dependent.
        /// </summary>
        /// <param name="skuId">Product identifier</param>
        public static bool CheckDependencies(int skuId)
        {
            var infoObj = ProviderObject.GetInfoById(skuId);
            if (infoObj != null)
            {
                var variants = VariantHelper.GetVariants(skuId);

                // Check variant dependencies, parent product cannot be deleted if there is a dependency on variant
                foreach (var variant in variants)
                {
                    if (variant.Generalized.CheckDependencies())
                    {
                        return true;
                    }
                }

                return infoObj.Generalized.CheckDependencies();
            }
            return false;
        }


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action to check</param>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            if (feature != FeatureEnum.Ecommerce)
            {
                return true;
            }

            // Parse domain name to remove port etc.
            if (domain != null)
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            }

            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, feature, false);
            if (versionLimitations == 0)
            {
                return true;
            }

            switch (action)
            {
                case ObjectActionEnum.Read:
                case ObjectActionEnum.Edit:
                {
                    return LicenseHelper.CurrentEdition != ProductEditionEnum.Free || GetObjectCountForLicenseCheck(domain) <= versionLimitations;
                }

                case ObjectActionEnum.Insert:
                {
                    return GetObjectCountForLicenseCheck(domain) < versionLimitations;
                }

                default: return false;
            }
        }


        /// <summary>
        /// Returns the amount of objects that are limited by license conditions.
        /// </summary>
        /// <param name="domain">The site domain. If not set, the request domain will be used.</param>
        public static int GetObjectCountForLicenseCheck(string domain = null)
        {
            if (string.IsNullOrEmpty(domain))
            {
                domain = RequestContext.CurrentDomain;
            }

            domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            int siteId = LicenseHelper.GetSiteIDbyDomain(domain);

            if (siteId > 0)
            {
                return ProtectedCacheHelper.Cache(cs => GetObjectCountForLicenseCheckInternal(cs, siteId), ECommerceSettings.ProvidersCacheMinutes, "EcommerceObjectCount" + siteId);
            }

            return 0;
        }


        private static int GetObjectCountForLicenseCheckInternal(CacheSettings cacheSettings, int siteId)
        {
            var objectCount = ProviderObject.GetObjectQuery(false)
                                            .WhereTrue("SKUEnabled")
                                            .WhereNull("SKUOptionCategoryID")
                                            .OnSite(siteId, true)
                                            .GetCount();

            if (cacheSettings.Cached)
            {
                cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(new[]
                {
                    SKUInfo.OBJECT_TYPE_SKU + "|all",
                    SiteInfo.OBJECT_TYPE + "|all"
                });
            }

            return objectCount;
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public static bool CheckLicense(ObjectActionEnum action, string domainName = null)
        {
            domainName = domainName ?? RequestContext.CurrentDomain;

            // Check number of products
            if (!LicenseVersionCheck(domainName, FeatureEnum.Ecommerce, action))
            {
                ProcessLicenseLimitations(domainName);
                return false;
            }

            return true;
        }


        private static void ProcessLicenseLimitations(string domainName)
        {
            var lki = LicenseKeyInfoProvider.GetLicenseKeyInfo(domainName);
            if (lki?.Edition == ProductEditionEnum.Free)
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Ecommerce);
            }
            else
            {
                LicenseHelper.ReportExceededFeatureLimit(FeatureEnum.Ecommerce, domainName);
            }
        }


        /// <summary>
        /// Sorts product options of the specified option category alphabetically (A-Z).
        /// </summary>
        /// <param name="categoryId">ID of option category</param>
        public static void SortProductOptionsAlphabetically(int categoryId)
        {
            ProviderObject.SortProductOptionsAlphabeticallyInternal(categoryId);
        }


        /// <summary>
        /// Returns True if there are any options categories with some enabled product options assigned to the specified SKU, otherwise returns False.
        /// </summary>
        /// <param name="skuId">SKU ID</param>
        public static bool HasSKUEnabledOptions(int skuId)
        {
            return ProviderObject.HasSKUEnabledOptionsInternal(skuId);
        }


        /// <summary>
        /// Indicates if the given SKU is new in the store. By default it is determined based on the CMSStoreProductsAreNewFor setting and SKUInStoreFrom property.
        /// </summary>
        /// <param name="sku">SKU data</param>        
        public static bool IsSKUNew(SKUInfo sku)
        {
            return ProviderObject.IsSKUNewInternal(sku);
        }


        /// <summary>
        /// Indicates if the given SKU can be bought by the customer based on the SKU inventory properties.
        /// </summary>
        /// <param name="sku">SKU data</param>
        public static bool IsSKUAvailableForSale(SKUInfo sku)
        {
            return ProviderObject.IsSKUAvailableForSaleInternal(sku);
        }


        /// <summary>
        /// Indicates the real stock status of SKU based on SKU items available.
        /// </summary>
        /// <param name="sku">SKU data</param>
        public static bool IsSKUInStock(SKUInfo sku)
        {
            return ProviderObject.IsSKUInStockInternal(sku);
        }


        /// <summary>
        /// Returns default value of NeedsShipping flag for given representation of product.
        /// </summary>
        /// <param name="representation">Product representation to get default value for.</param>
        public static bool GetDefaultNeedsShippingFlag(SKUProductTypeEnum representation)
        {
            return ProviderObject.GetDefaultNeedsShippingFlagInternal(representation);
        }


        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns product with specified ID.
        /// </summary>
        /// <param name="skuId">Product ID</param>        
        protected virtual SKUInfo GetSKUInfoInternal(int skuId)
        {
            return GetInfoById(skuId);
        }


        /// <summary>
        /// Returns product with specified GUID.
        /// </summary>
        /// <param name="skuGuid">Product GUID</param>        
        protected virtual SKUInfo GetSKUInfoInternal(Guid skuGuid)
        {
            return GetInfoByGuid(skuGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified product.
        /// </summary>
        /// <param name="skuObj">Product to be set</param>        
        protected virtual void SetSKUInfoInternal(SKUInfo skuObj)
        {
            if (skuObj == null)
            {
                throw new Exception("[SKUInfoProvider.SetSKUInfoInternal]: You must specify the object.");
            }

            // Ensure null value - in case SKUTaxClassID is set to zero or negative value using SetValue
            skuObj.SKUTaxClassID = skuObj.SKUTaxClassID;

            bool newProduct = (skuObj.SKUID <= 0);

            // Set when the product was created, avoid setting when importing product
            if (newProduct && skuObj.Generalized.UpdateTimeStamp)
            {
                skuObj.SetValue("SKUCreated", DateTime.Now);
            }

            // Ensure product type
            if (String.IsNullOrEmpty(skuObj.GetStringValue("SKUProductType", null)))
            {
                skuObj.SKUProductType = SKUProductTypeEnum.Product;
            }

            ClearUnusedFields(skuObj);

            using (var tr = BeginTransaction())
            {
                // If SKU is product and variant tracking has been deactivated -> clear available items for all its variants
                if (skuObj.IsProduct)
                {
                    string originalTrackInventory = ValidationHelper.GetString(skuObj.GetOriginalValue("SKUTrackInventory"), string.Empty);

                    if (TrackInventoryTypeEnum.ByVariants.ToStringRepresentation() == originalTrackInventory)
                    {
                        // Variant tracking has been deactivated
                        if (skuObj.SKUTrackInventory != TrackInventoryTypeEnum.ByVariants)
                        {
                            // Get all variants to clear available items
                            var dsVariants = VariantHelper.GetVariants(skuObj.SKUID);

                            if (!DataHelper.DataSourceIsEmpty(dsVariants))
                            {
                                using (CMSActionContext context = new CMSActionContext())
                                {
                                    context.TouchParent = false;

                                    // Clear SKUAvailableItems for all variants, done through SKUInfo API 'cause of staging
                                    foreach (var variant in dsVariants)
                                    {
                                        variant.SetValue("SKUAvailableItems", null);
                                        SetSKUInfo(variant);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ECommerceSettings.CheapestVariantAdvertising(SiteContext.CurrentSiteName) && skuObj.IsProductVariant)
                {
                    SetLowestPriceToParent(skuObj);
                }

                CleanUpProductTypeDependenciesInternal(skuObj);

                // Check whether fields relevant for search were updated
                // This has to be called before SetInfo is called because it removes records of changes
                bool updateSearchIndex = SearchHelper.SearchFieldChanged(skuObj);

                // Insert or update the product
                SetInfo(skuObj);

                // Update search index
                if (updateSearchIndex)
                {
                    UpdateSearchIndex(skuObj);
                }

                // Commit transaction
                tr.Commit();
            }
        }


        /// <summary>
        /// Deletes specified product.
        /// </summary>
        /// <param name="skuObj">Product to be deleted</param>        
        protected virtual void DeleteSKUInfoInternal(SKUInfo skuObj)
        {
            if (skuObj == null)
            {
                return;
            }

            // Check that SKU object is not used in related objects (order or shopping cart items)
            if (CheckDependencies(skuObj.SKUID))
            {
                // Disable product if user is authorized to modify it
                if (CMSActionContext.CurrentUser != null && skuObj.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, CMSActionContext.CurrentUser))
                {
                    skuObj.SKUEnabled = false;
                    SetSKUInfo(skuObj);
                }

                // Log warning to event log
                CoreServices.EventLog.LogEvent("W", "SKUInfoProvider", "DELETEOBJ", $"Object \"{skuObj.Generalized.TypeInfo.ObjectType}\" with name \"{skuObj.SKUName}\" cannot be deleted because it is used in existing shopping cart or order records.");

                return;
            }

            // Check that there are no SKU variants defined by deleted SKU option category
            if (skuObj.IsProductOption && VariantOptionInfoProvider.GetVariantOptions().TopN(1).WhereEquals("OptionSKUID", skuObj.SKUID).HasResults())
            {
                // Log warning to event log
                CoreServices.EventLog.LogEvent("W", "SKUInfoProvider", "DELETEOBJ", $"Object \"{skuObj.Generalized.TypeInfo.ObjectType}\" with name \"{skuObj.SKUName}\" cannot be deleted because it is used by an existing product variant.");

                return;
            }

            using (var tr = BeginTransaction())
            {
                if (ECommerceSettings.CheapestVariantAdvertising(SiteContext.CurrentSiteName) && skuObj.IsProductVariant)
                {
                    SetLowestPriceToParent(skuObj, true);
                }

                var variants = VariantHelper.GetVariants(skuObj.SKUID);

                using (new CMSActionContext { TouchParent = false })
                {
                    // Delete variants of product and update search index
                    foreach (var variant in variants)
                    {
                        DeleteInfo(variant);
                        UpdateSearchIndex(variant);
                    }
                }

                // Delete MultiBuyDiscounts (buy X get Y) with SKU on Y-side
                var multiBuyDiscountsAppliedOnSKU = new IDQuery(MultiBuyDiscountInfo.OBJECT_TYPE).WhereEquals("MultiBuyDiscountApplyToSKUID", skuObj.SKUID).GetListResult<int>();
                foreach (var discount in multiBuyDiscountsAppliedOnSKU)
                {
                    MultiBuyDiscountInfoProvider.DeleteMultiBuyDiscountInfo(discount);
                }

                // Delete product
                DeleteInfo(skuObj);

                // Update search index of product
                UpdateSearchIndex(skuObj);

                // Commit transaction
                tr.Commit();
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Cleans up product type dependencies like e-product files, bundle relations, Memberships etc.
        /// </summary>
        /// <param name="skuObj">The SKU.</param>        
        protected virtual void CleanUpProductTypeDependenciesInternal(SKUInfo skuObj)
        {
            object originalValue = skuObj.GetOriginalValue("SKUProductType");

            if (originalValue != null)
            {
                SKUProductTypeEnum originalRepresenting = originalValue.ToString().ToEnum<SKUProductTypeEnum>();
                // Product Type has been changed, existing product type dependencies must be removed
                if (skuObj.SKUProductType != originalRepresenting)
                {
                    switch (originalRepresenting)
                    {
                        case SKUProductTypeEnum.Bundle:
                            skuObj.SetValue("SKUBundleItemsCount", null);
                            var bundleProducts = BundleInfoProvider.GetBundles().WhereEquals("BundleID", skuObj.SKUID);

                            foreach (var bi in bundleProducts)
                            {
                                BundleInfoProvider.DeleteBundleInfo(bi);
                            }

                            break;

                        case SKUProductTypeEnum.EProduct:
                            var skuFiles = SKUFileInfoProvider.GetSKUFiles().WhereEquals("FileSKUID", skuObj.SKUID);

                            foreach (var fi in skuFiles)
                            {
                                SKUFileInfoProvider.DeleteSKUFileInfo(fi);
                            }

                            MetaFileInfoProvider.DeleteFiles(skuObj, ObjectAttachmentsCategories.EPRODUCT);
                            break;

                        default:
                            return;
                    }
                }
            }
        }


        /// <summary>
        /// Returns query for all SKUs for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param> 
        protected virtual ObjectQuery<SKUInfo> GetSKUsInternal(int siteId)
        {
            // Check if site uses site or global SKUs
            var includeGlobal = ECommerceSettings.AllowGlobalProducts(siteId);

            // Get SKUs on requested site
            var query = GetSKUs().OnSite(siteId, includeGlobal);

            return query;
        }


        /// <summary>
        /// Returns query for all product options from specified product option category allowed for product specified by skuId parameter.
        /// </summary>
        /// <param name="skuId">ID of the SKU for which options are to be obtained</param>
        /// <param name="categoryId">Product option category ID</param> 
        /// <param name="onlyEnabled">Indicates if only enabled product options should be selected</param>
        protected virtual ObjectQuery<SKUInfo> GetSKUOptionsForProductInternal(int skuId, int categoryId, bool onlyEnabled)
        {
            var query = GetSKUs().WhereEquals("SKUOptionCategoryID", categoryId);

            if (onlyEnabled)
            {
                query.WhereTrue("SKUEnabled");
            }

            if (skuId > 0)
            {
                var productWhere = new WhereCondition();
                productWhere.WhereIn("SKUID", SKUAllowedOptionInfoProvider.GetSKUOptions().Column("OptionSKUID").WhereEquals("SKUID", skuId))
                            .Or()
                            .WhereIn("SKUOptionCategoryID", SKUOptionCategoryInfoProvider.GetSKUOptionCategories().Column("CategoryID").WhereEquals("SKUID", skuId).WhereEqualsOrNull("AllowAllOptions", true));

                query.Where(productWhere);
            }

            return query;
        }


        /// <summary>
        /// Returns query for all bundle items for specified bundle SKU ID.
        /// </summary>
        /// <param name="bundleId">Bundle SKU ID</param>
        protected virtual ObjectQuery<SKUInfo> GetBundleItemsInternal(int bundleId)
        {
            return GetSKUs().WhereIn("SKUID",
                      BundleInfoProvider.GetBundles()
                          .Column("SKUID")
                          .WhereEquals("BundleID", bundleId)
                   );
        }


        /// <summary>
        /// Returns dataset with the products from specified user's wishlist.
        /// </summary>
        /// <param name="userId">ID of the user the wishlist belongs to</param>  
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<SKUInfo> GetWishlistProductsInternal(int userId, int siteId)
        {
            return GetSKUs().WhereIn("SKUID",
                      WishlistItemInfoProvider.GetWishlistItems()
                          .Column("SKUID")
                          .OnSite(siteId)
                          .WhereEquals("UserID", userId)
                   );
        }


        /// <summary>
        /// Returns user friendly URL of the specified product.
        /// </summary>
        /// <param name="skuGuid">Product unique identifier (GUID)</param>
        /// <param name="skuName">Product name</param>
        /// <param name="siteName">Site Name</param>
        protected virtual string GetSKUUrlInternal(Guid skuGuid, string skuName, string siteName)
        {
            var sku = GetSKUInfo(skuGuid);

            return GetPermanentURL(skuName, siteName, sku);
        }


        /// <summary>
        /// Returns user friendly URL of the specified product. Method uses hash tables.
        /// </summary>
        /// <param name="skuID">Product ID</param>
        /// <param name="skuName">Product name</param>
        /// <param name="siteName">Site Name</param>
        protected virtual string GetSKUUrlInternal(int skuID, string skuName, string siteName)
        {
            var sku = GetSKUInfo(skuID);

            return GetPermanentURL(skuName, siteName, sku);
        }


        /// <summary>
        /// Updates products' available items in database according to the products' units of the specified shopping cart.
        /// </summary>
        /// <param name="cart">Shopping cart with some products</param>
        /// <exception cref="InvalidOperationException">When strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).</exception>
        protected virtual void UpdateInventoryInternal(ShoppingCartInfo cart)
        {
            // Do not process
            if (cart == null)
            {
                return;
            }

            // Actualize products available units after purchase                  
            foreach (ShoppingCartItemInfo cartItem in cart.CartItems)
            {
                // Get cart item SKU
                var sku = GetSKUInfo(cartItem.SKUID);
                if ((sku == null) || (sku.SKUTrackInventory == TrackInventoryTypeEnum.Disabled))
                {
                    continue;
                }

                // Calculate the number of items to be removed from inventory
                var orderUnits = cartItem.OrderItem?.OrderItemUnitCount ?? 0;
                var inventoryChange = (cartItem.CartItemUnits - orderUnits);

                // It is bundle
                if (sku.SKUProductType == SKUProductTypeEnum.Bundle)
                {
                    if ((sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundle) || (sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                    {
                        // Update bundle
                        UpdateInventory(sku, inventoryChange, cartItem);
                    }
                    continue;
                }

                // SKU is variant and track by product is enabled
                var trackForVariantProduct = sku.IsProductVariant && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);

                // It is bundle item
                if (cartItem.IsBundleItem && cartItem.ParentBundle?.SKU != null)
                {
                    var itemInventory = cartItem.ParentBundle.SKU.SKUBundleInventoryType;
                    if ((itemInventory == BundleInventoryTypeEnum.RemoveProducts) ||
                        (itemInventory == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                    {
                        // Update bundle item
                        UpdateInventory(sku, inventoryChange, cartItem);

                        if (trackForVariantProduct)
                        {
                            // Update inventory for parent product of variant
                            SKUInfo parent = cartItem.VariantParent;
                            if (parent != null)
                            {
                                UpdateInventory(parent, inventoryChange);
                            }
                        }
                    }

                    continue;
                }

                // Track if this sku is variant and track by variants is enabled
                bool trackForThisVariant = sku.IsProductVariant && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByVariants);
                // Track if this sku is product and track by products is enabled
                bool trackForThisProduct = !sku.IsProductVariant && !sku.IsProductOption && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);
                // Track if this sku is accessory and track by products is enabled
                bool trackForThisAccessory = sku.IsAccessoryProduct && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct);

                if (trackForThisVariant || trackForThisProduct || trackForThisAccessory)
                {
                    UpdateInventory(sku, inventoryChange, cartItem);
                    continue;
                }

                if (trackForVariantProduct)
                {
                    // Update inventory for parent product of variant
                    SKUInfo parent = cartItem.VariantParent;
                    if (parent != null)
                    {
                        UpdateInventory(parent, inventoryChange);
                    }
                }
            }
        }


        /// <summary>
        /// Sorts product options of the specified option category alphabetically (A-Z).
        /// </summary>
        /// <param name="categoryId">Option category ID</param>
        protected virtual void SortProductOptionsAlphabeticallyInternal(int categoryId)
        {
            // Log update object for category object
            var optionCategory = OptionCategoryInfoProvider.GetOptionCategoryInfo(categoryId);
            if (optionCategory == null)
            {
                return;
            }

            var fakeOption = ModuleManager.GetObject(SKUInfo.OBJECT_TYPE_OPTIONSKU);
            if (fakeOption != null)
            {
                // Set parameters which are important for the order
                fakeOption.Generalized.ObjectParentID = categoryId;
                fakeOption.Generalized.ObjectSiteID = optionCategory.CategorySiteID;

                fakeOption.Generalized.SortAlphabetically(true, fakeOption.TypeInfo.OrderColumn, fakeOption.TypeInfo.DisplayNameColumn);
            }

            // Log synchronization for parent
            SynchronizationHelper.LogObjectChange(optionCategory, TaskTypeEnum.UpdateObject);
        }


        /// <summary>
        /// Returns True if there are any options categories with some enabled product options assigned to the specified SKU, otherwise returns False.
        /// </summary>
        /// <param name="skuId">SKU ID</param>
        protected virtual bool HasSKUEnabledOptionsInternal(int skuId)
        {
            // Get all option categories related to this SKU 
            var categoryIDs = SKUOptionCategoryInfoProvider.GetSKUOptionCategories()
                .WhereEquals("SKUID", skuId)
                .Column("CategoryID");

            // Find some enabled option related to given categories in nested query
            var query = GetSKUs()
                    .TopN(1)
                    .WhereTrue("SKUEnabled")
                    .WhereIn("SKUOptionCategoryID", categoryIDs)
                    .Column("SKUID");

            return query.FirstOrDefault() != null;
        }


        /// <summary>
        /// Indicates if the given SKU is new in the store. By default it is determined based on the CMSStoreProductsAreNewFor setting and SKUInStoreFrom property.
        /// </summary>
        /// <param name="sku">SKU data</param>        
        protected virtual bool IsSKUNewInternal(SKUInfo sku)
        {
            if (sku != null)
            {
                int newFor = ECommerceSettings.ProductsAreNewFor(sku.SKUSiteID);

                // Check if product is new
                if (newFor > 0)
                {
                    DateTime newTo = ValidationHelper.GetDateTime(sku.SKUInStoreFrom, DateTimeHelper.ZERO_TIME).AddDays(newFor);

                    return (DateTime.Now <= newTo);
                }
            }

            return false;
        }


        /// <summary>
        /// Indicates if the given SKU can be bought by the customer based on the SKU inventory settings.
        /// </summary>
        /// <param name="sku">SKU data</param>
        protected virtual bool IsSKUAvailableForSaleInternal(SKUInfo sku)
        {
            // SKU is not available
            if ((sku == null) || !sku.SKUEnabled)
            {
                return false;
            }

            // Check if product is sold despite of insufficient stock level
            var ignoreStock = !ValidationHelper.GetBoolean(sku.SKUSellOnlyAvailable, false);
            if (ignoreStock)
            {
                return true;
            }

            // Check variants stock levels if inventory is tracked by variants
            if (sku.IsProduct && (sku.SKUTrackInventory == TrackInventoryTypeEnum.ByVariants))
            {
                // Check if enabled variant with sufficient stock level is present
                var variants = sku.Children["ecommerce.skuvariant"];

                return variants.Any(variant => variant.GetBooleanValue("SKUEnabled", false) && variant.GetIntegerValue("SKUAvailableItems", 0) > 0);
            }

            // Check products inventory
            return (ValidationHelper.GetInteger(sku.SKUAvailableItems, 0) > 0);
        }


        /// <summary>
        /// Indicates the real stock status of SKU based on SKU items available.
        /// </summary>
        /// <param name="sku">SKU data</param>
        protected virtual bool IsSKUInStockInternal(SKUInfo sku)
        {
            return (sku.SKUTrackInventory == TrackInventoryTypeEnum.Disabled) || (sku.SKUAvailableItems > 0);
        }


        /// <summary>
        /// Returns default value of NeedsShipping flag for given representation of product.
        /// </summary>
        /// <param name="representation">Product representation to get default value for.</param>
        protected virtual bool GetDefaultNeedsShippingFlagInternal(SKUProductTypeEnum representation)
        {
            return (representation == SKUProductTypeEnum.Bundle) || (representation == SKUProductTypeEnum.Product);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns user friendly URL of the specified product.
        /// </summary>
        /// <param name="skuName">Product name</param>
        /// <param name="siteName">Site Name</param>
        /// <param name="sku">Product for which URL is created</param>
        private static string GetPermanentURL(string skuName, string siteName, SKUInfo sku)
        {
            if (sku == null)
            {
                return string.Empty;
            }

            var skuGuid = sku.SKUGUID;
            // Get parent name and GUID for variant
            if (sku.IsProductVariant)
            {
                var parent = sku.Parent as SKUInfo;
                if (parent != null)
                {
                    skuGuid = parent.SKUGUID;
                    skuName = parent.SKUName;
                }
            }

            // Ensure site name
            if (string.IsNullOrEmpty(siteName))
            {
                siteName = SiteContext.CurrentSiteName;
            }

            string safeName = URLHelper.GetSafeUrlPart(skuName, siteName);
            string extension = URLHelper.GetFriendlyExtension(siteName);

            // Ensure extension format
            if (extension != "")
            {
                extension = "." + extension.TrimStart('.');
            }

            return "~/getproduct/" + Convert.ToString(skuGuid) + "/" + safeName + extension;
        }


        /// <summary>
        /// Updates the inventory of specified SKU.
        /// </summary>
        /// <param name="sku">The sku.</param>
        /// <param name="inventoryChange">The inventory change.</param>
        /// <param name="cartItem">[Optional] The cart item for specified SKU. Inventory change will be added to cart item's OrderItemUnitCount, to handle multiple UpdateInventory requests.</param>
        /// <exception cref="InvalidOperationException">When strict inventory management is enabled and trying to update inventory below zero to an item which doesn't allow so (<see cref="SKUInfo.SKUSellOnlyAvailable"/> is true).</exception>
        private static void UpdateInventory(SKUInfo sku, int inventoryChange, ShoppingCartItemInfo cartItem = null)
        {
            var stockAfterUpdate = ExecuteUpdateInventoryDbQuery(sku, inventoryChange);

            if (stockAfterUpdate != null && stockAfterUpdate < 0 && sku.SKUSellOnlyAvailable)
            {             
                if (ECommerceSettings.UseStrictInventoryManagement)
                {
                    var message = ResHelper.GetStringFormat("com.inventory.notenoughunits", ResHelper.LocalizeString(sku.SKUName), stockAfterUpdate + inventoryChange);
                    throw new InvalidOperationException(message);
                }

                var warning = $"The number of available items of product '{sku.SKUName}' just got below zero (probably due to concurrent purchase of the same item). Please check the latest orders. To prevent such issues in the future, you can enable strict inventory management.";
                Service.Resolve<IEventLogService>().LogEvent(EventType.WARNING, "Checkout", "UPDATEINVENTORY", warning);
            } 

            sku.Generalized.Invalidate(false);

            // Handle new OrderItemUnitCount for multiple request
            if ((cartItem != null) && (cartItem.OrderItem != null))
            {
                cartItem.OrderItem.OrderItemUnitCount += inventoryChange;
            }
        }


        /// <summary>
        /// Executes an atomic DB query that updates the number of available items of specified <paramref name="sku"/> and returns the number of available items after this operation.
        /// </summary>
        /// <param name="sku">The sku.</param>
        /// <param name="inventoryChange">The inventory change.</param>
        private static int? ExecuteUpdateInventoryDbQuery(SKUInfo sku, int inventoryChange)
        {
            var inventoryChangeParameters = new QueryDataParameters { { "inventoryChange", inventoryChange } };
            var query = new DataQuery(SKUInfo.OBJECT_TYPE_SKU, QueryName.GENERALUPDATE).WhereEquals("SKUID", sku.SKUID);
            var updateExpression = new UpdateQueryExpression(new Dictionary<string, object> { { "SKUAvailableItems", new QueryExpression("SKUAvailableItems - @inventoryChange", inventoryChangeParameters) } });
            var valuesExpression = query.IncludeDataParameters(updateExpression.Parameters, updateExpression.GetExpression());
            query.EnsureParameters().AddMacro(QueryMacros.VALUES, valuesExpression);
            query.EnsureParameters().AddMacro(QueryMacros.OUTPUT, "inserted.[SKUAvailableItems]");

            var result = query.Execute();

            var stockAfterUpdate = result.Tables[0].Rows[0]["SKUAvailableItems"] as int?;

            return stockAfterUpdate;
        }


        /// <summary>
        /// Set unused fields in SKU to null (or default) value
        /// </summary>
        /// <param name="skuObj">SKU</param>
        private static void ClearUnusedFields(SKUInfo skuObj)
        {
            // This is product and inventory tracking for products is disabled
            bool trackByProductDisabled = skuObj.IsProduct && (skuObj.SKUTrackInventory != TrackInventoryTypeEnum.ByProduct);
            // This is product variant and inventory tracking for product variants is disabled
            bool trackByVariantDisabled = skuObj.IsProductVariant && (skuObj.SKUTrackInventory != TrackInventoryTypeEnum.ByVariants);
            // If inventory tracking is disabled for this SKU type set invisible tracking fields to default
            if (trackByProductDisabled || trackByVariantDisabled)
            {
                skuObj.SetValue("SKUAvailableItems", null);
                skuObj.SetValue("SKUReorderAt", null);
            }
            // If inventory tracking is disabled set tracking fields to default
            if (skuObj.SKUTrackInventory == TrackInventoryTypeEnum.Disabled)
            {
                skuObj.SetValue("SKUAvailableInDays", null);
                skuObj.SKUSellOnlyAvailable = false;
            }

            // Product option does not use SKUBundleInventoryType
            if (skuObj.IsProductOption)
            {
                skuObj.SetValue("SKUBundleInventoryType", null);
            }
        }


        /// <summary>
        /// Updates search index of depending nodes.
        /// </summary>
        /// <param name="skuObj">Product data</param>
        private static void UpdateSearchIndex(SKUInfo skuObj)
        {
            if (skuObj == null)
            {
                return;
            }

            // Update search index of depending nodes
            if (SearchHelper.IsSearchTaskCreationAllowed(skuObj.TypeInfo.ObjectClassName))
            {
                // Prepare variables for searching for depending nodes
                TreeProvider tp = new TreeProvider();
                string where = "(SKUID = " + skuObj.SKUID + ")";

                // Get all nodes
                DataSet ds = tp.SelectNodes(TreeProvider.ALL_SITES, TreeProvider.ALL_DOCUMENTS, TreeProvider.ALL_CULTURES, false, null, where, null, -1, false, 0, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS + ", DocumentCheckedOutVersionHistoryID, DocumentPublishedVersionHistoryID");
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    List<SearchTaskCreationParameters> taskParameters = new List<SearchTaskCreationParameters>();

                    // Get node
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        TreeNode node = TreeNode.New(dr["ClassName"].ToString(), dr);
                        if ((node != null) && node.PublishedVersionExists)
                        {
                            taskParameters.Add(new SearchTaskCreationParameters
                            {
                                TaskType = SearchTaskTypeEnum.Update,
                                ObjectType = PredefinedObjectType.DOCUMENT,
                                ObjectField = SearchFieldsConstants.ID,
                                TaskValue = node.GetSearchID(),
                                RelatedObjectID = node.DocumentID
                            });
                        }
                    }

                    // Update search index
                    SearchTaskInfoProvider.CreateTasks(taskParameters, true);
                }
            }
        }


        /// <summary>
        /// Set to the parent the price of the cheapest product variant.
        /// </summary>
        /// <param name="variantInfo">Product variant</param>
        /// <param name="deleting">Flag indicate if product variant is being deleted</param>
        private void SetLowestPriceToParent(SKUInfo variantInfo, bool deleting = false)
        {
            if (!ECommerceActionContext.CurrentSetLowestPriceToParent)
            {
                return;
            }

            var parent = GetSKUInfo(variantInfo.SKUParentSKUID);

            // Get all variants of parent
            var variantsDs = VariantHelper.GetVariants(parent.SKUID);

            if (!DataHelper.DataSourceIsEmpty(variantsDs))
            {
                // Select cheapest variant price from all variants except variant which is being edited/deleted
                DataRow[] variantsOrderedByPrice = variantsDs.Tables[0].Select("SKUID NOT IN (" + variantInfo.SKUID + ") AND SKUEnabled = True", "SKUPrice");

                decimal lowestPrice;

                // Get cheapest variant price from other variants if any exists, otherwise use variant price which is being edited
                if (variantsOrderedByPrice.Length > 0)
                {
                    // Get cheapest variant price
                    decimal otherVariantsLowestPrice = ValidationHelper.GetDecimal(variantsOrderedByPrice[0]["SKUPrice"], parent.SKUPrice);

                    // Decide which price should be used: cheapest price from other variants or variant price which is being edited
                    lowestPrice = (deleting || !variantInfo.SKUEnabled || (otherVariantsLowestPrice < variantInfo.SKUPrice)) ? otherVariantsLowestPrice : variantInfo.SKUPrice;
                }
                else
                {
                    lowestPrice = (variantInfo.SKUEnabled) ? variantInfo.SKUPrice : parent.SKUPrice;
                }

                // If parent product price is different, overwrite it
                if (!parent.SKUPrice.Equals(lowestPrice))
                {
                    parent.SKUPrice = lowestPrice;
                    parent.Generalized.SetObject();
                }
            }
        }

        #endregion
    }
}