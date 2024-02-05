using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing SupplierInfo management.
    /// </summary>
    public class SupplierInfoProvider : AbstractInfoProvider<SupplierInfo, SupplierInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SupplierInfoProvider()
            : base(SupplierInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
                    Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all suppliers.
        /// </summary>
        public static ObjectQuery<SupplierInfo> GetSuppliers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns supplier with specified ID.
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>        
        public static SupplierInfo GetSupplierInfo(int supplierId)
        {
            return ProviderObject.GetInfoById(supplierId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified supplier.
        /// </summary>
        /// <param name="supplierObj">Supplier to be set</param>
        public static void SetSupplierInfo(SupplierInfo supplierObj)
        {
            ProviderObject.SetInfo(supplierObj);
        }


        /// <summary>
        /// Deletes specified supplier.
        /// </summary>
        /// <param name="supplierObj">Supplier to be deleted</param>
        public static void DeleteSupplierInfo(SupplierInfo supplierObj)
        {
            ProviderObject.DeleteInfo(supplierObj);
        }


        /// <summary>
        /// Deletes supplier with specified ID.
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        public static void DeleteSupplierInfo(int supplierId)
        {
            var supplierObj = GetSupplierInfo(supplierId);
            DeleteSupplierInfo(supplierObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all suppliers for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>   
        /// <param name="onlyEnabled">True - only enable suppliers are returned.
        /// False - both enabled and disabled suppliers are returned.</param>
        public static ObjectQuery<SupplierInfo> GetSuppliers(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetSuppliersInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns suppliers with specified code name.
        /// </summary>
        /// <param name="supplierName">Supplier code name</param>        
        /// <param name="siteName">Supplier site name</param>        
        public static SupplierInfo GetSupplierInfo(string supplierName, string siteName)
        {
            return ProviderObject.GetSupplierInfoInternal(supplierName, siteName);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific supplier. 
        /// For global supplier: 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific supplier: 'EcommerceModify' OR 'ModifySuppliers' permission is checked.
        /// </summary>
        /// <param name="supplier">Supplier to be checked</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedToModifySupplier(SupplierInfo supplier, string siteName, IUserInfo user)
        {
            return (supplier != null) && IsUserAuthorizedToModifySupplier(supplier.IsGlobal, siteName, user);
        }


        /// <summary>
        /// Indicates if user is authorized to modify suppliers.
        /// </summary>
        /// <param name="global">For global suppliers (global = True): 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific suppliers (global = False): 'EcommerceModify' OR 'ModifySuppliers' permission is checked.</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToModifySupplier(bool global, string siteName, IUserInfo user, bool exceptionOnFailure = false)
        {
            var permission = global ? EcommercePermissions.ECOMMERCE_MODIFYGLOBAL : EcommercePermissions.SUPPLIERS_MODIFY;

            return ECommerceHelper.IsUserAuthorizedForPermission(permission, siteName, user, exceptionOnFailure);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all suppliers for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>   
        /// <param name="onlyEnabled">True - only enable suppliers are returned.
        /// False - both enabled and disabled suppliers are returned.</param>
        protected virtual ObjectQuery<SupplierInfo> GetSuppliersInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global suppliers
            var includeGlobal = ECommerceSettings.AllowGlobalSuppliers(siteId);

            // Get suppliers on requested site
            var query = GetSuppliers().OnSite(siteId, includeGlobal);

            if (onlyEnabled)
            {
                query.WhereTrue("SupplierEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns suppliers with specified code name.
        /// </summary>
        /// <param name="supplierName">Supplier code name</param>        
        /// <param name="siteName">Supplier site name</param>        
        protected virtual SupplierInfo GetSupplierInfoInternal(string supplierName, string siteName)
        {
            // Search for global supplier if site supplier not found
            bool searchGlobal = ECommerceSettings.AllowGlobalSuppliers(siteName);

            return GetInfoByCodeName(supplierName, SiteInfoProvider.GetSiteID(siteName), true, searchGlobal);
        }

        #endregion
    }
}