using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ManufacturerInfo management.
    /// </summary>
    public class ManufacturerInfoProvider : AbstractInfoProvider<ManufacturerInfo, ManufacturerInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ManufacturerInfoProvider()
            : base(ManufacturerInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
                    Name = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all manufacturers.
        /// </summary>
        public static ObjectQuery<ManufacturerInfo> GetManufacturers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns manufacturer with specified ID.
        /// </summary>
        /// <param name="manufacturerId">Manufacturer ID</param>        
        public static ManufacturerInfo GetManufacturerInfo(int manufacturerId)
        {
            return ProviderObject.GetInfoById(manufacturerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified manufacturer.
        /// </summary>
        /// <param name="manufacturerObj">Manufacturer to be set</param>
        public static void SetManufacturerInfo(ManufacturerInfo manufacturerObj)
        {
            ProviderObject.SetInfo(manufacturerObj);
        }


        /// <summary>
        /// Deletes specified manufacturer.
        /// </summary>
        /// <param name="manufacturerObj">Manufacturer to be deleted</param>
        public static void DeleteManufacturerInfo(ManufacturerInfo manufacturerObj)
        {
            ProviderObject.DeleteInfo(manufacturerObj);
        }


        /// <summary>
        /// Deletes manufacturer with specified ID.
        /// </summary>
        /// <param name="manufacturerId">Manufacturer ID</param>
        public static void DeleteManufacturerInfo(int manufacturerId)
        {
            ManufacturerInfo manufacturerObj = GetManufacturerInfo(manufacturerId);
            DeleteManufacturerInfo(manufacturerObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all manufacturers for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>   
        /// <param name="onlyEnabled">True - only enable manufacturers are returned.
        /// False - both enabled and disabled manufacturers are returned.</param>
        public static ObjectQuery<ManufacturerInfo> GetManufacturers(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetManufacturersInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns manufacturer with specified code name.
        /// </summary>
        /// <param name="manufacturerName">Manufacturer code name</param>        
        /// <param name="siteName">Manufacturer site name</param>        
        public static ManufacturerInfo GetManufacturerInfo(string manufacturerName, string siteName)
        {
            return ProviderObject.GetManufacturerInfoInternal(manufacturerName, siteName);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific manufacturer. 
        /// For global manufacturer: 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific manufacturer: 'EcommerceModify' OR 'ModifyManufacturers' permission is checked.
        /// </summary>
        /// <param name="manufacturer">Manufacturer to be checked</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedToModifyManufacturer(ManufacturerInfo manufacturer, string siteName, IUserInfo user)
        {
            return (manufacturer != null) && IsUserAuthorizedToModifyManufacturer(manufacturer.IsGlobal, siteName, user);
        }


        /// <summary>
        /// Indicates if user is authorized to modify manufacturers.
        /// </summary>
        /// <param name="global">For global manufacturers (global = True): 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific manufacturers (global = False): 'EcommerceModify' OR 'ModifyManufacturers' permission is checked.</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToModifyManufacturer(bool global, string siteName, IUserInfo user, bool exceptionOnFailure = false)
        {
            var permission = global ? EcommercePermissions.ECOMMERCE_MODIFYGLOBAL : EcommercePermissions.MANUFACTURERS_MODIFY;

            return ECommerceHelper.IsUserAuthorizedForPermission(permission, siteName, user, exceptionOnFailure);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all manufacturers for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>   
        /// <param name="onlyEnabled">True - only enable manufacturers are returned.
        /// False - both enabled and disabled manufacturers are returned.</param>
        protected virtual ObjectQuery<ManufacturerInfo> GetManufacturersInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global manufacturers
            var includeGlobal = ECommerceSettings.AllowGlobalManufacturers(siteId);

            // Get manufacturers on requested site
            var query = GetManufacturers().OnSite(siteId, includeGlobal);

            if (onlyEnabled)
            {
                query.WhereTrue("ManufacturerEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns manufacturer with specified code name.
        /// </summary>
        /// <param name="manufacturerName">Manufacturer code name</param>        
        /// <param name="siteName">Manufacturer site name</param>        
        protected virtual ManufacturerInfo GetManufacturerInfoInternal(string manufacturerName, string siteName)
        {
            // Search for global manufacturer if site manufacturer not found
            bool searchGlobal = ECommerceSettings.AllowGlobalManufacturers(siteName);

            return GetInfoByCodeName(manufacturerName, SiteInfoProvider.GetSiteID(siteName), true, searchGlobal);
        }

        #endregion
    }
}