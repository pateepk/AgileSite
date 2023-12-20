using CMS.DataEngine;


namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides methods for working with <see cref="TaxClassInfo"/> and its data.
    /// </summary>
    public class TaxClassInfoProvider : AbstractInfoProvider<TaxClassInfo, TaxClassInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public TaxClassInfoProvider()
            : base(TaxClassInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all tax classes.
        /// </summary>
        /// <returns>All tax class query.</returns>
        public static ObjectQuery<TaxClassInfo> GetTaxClasses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a tax class with the specified ID.
        /// </summary>
        /// <param name="classId">ID of the tax class.</param>
        /// <returns>Tax class object with the ID.</returns>
        public static TaxClassInfo GetTaxClassInfo(int classId)
        {
            return ProviderObject.GetInfoById(classId);
        }


        /// <summary>
        /// Returns a tax class with the specified code name on the specified site.
        /// </summary>
        /// <param name="className">Code name of the tax class.</param>               
        /// <param name="siteName">Name of the site with the tax class.</param>                
        /// <returns>Tax class object with the name.</returns>
        public static TaxClassInfo GetTaxClassInfo(string className, string siteName)
        {
            return ProviderObject.GetTaxClassInfoInternal(className, siteName);
        }


        /// <summary>
        /// Creates or updates the specified tax class in the database.
        /// </summary>
        /// <param name="classObj">Tax class to be set.</param>
        public static void SetTaxClassInfo(TaxClassInfo classObj)
        {
            ProviderObject.SetInfo(classObj);
        }


        /// <summary>
        /// Deletes the specified tax class.
        /// </summary>
        /// <param name="classObj">Tax class to be deleted.</param>
        public static void DeleteTaxClassInfo(TaxClassInfo classObj)
        {
            ProviderObject.DeleteInfo(classObj);
        }


        /// <summary>
        /// Deletes a tax class with the specified ID.
        /// </summary>
        /// <param name="classId">ID of the tax class that is deleted.</param>
        public static void DeleteTaxClassInfo(int classId)
        {
            var classObj = GetTaxClassInfo(classId);
            DeleteTaxClassInfo(classObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns all tax classes on the specified site.
        /// </summary>
        /// <param name="siteId">ID of the site with the tax classes.</param>        
        /// <returns>Site's tax class query.</returns>       
        public static ObjectQuery<TaxClassInfo> GetTaxClasses(int siteId)
        {
            return ProviderObject.GetTaxClassesInternal(siteId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns a tax class with the specified code name on the specified site.
        /// </summary>
        /// <param name="className">Code name of the tax class.</param>               
        /// <param name="siteName">Name of the site with the tax class.</param>                
        /// <returns>Tax class object with the name.</returns>
        protected virtual TaxClassInfo GetTaxClassInfoInternal(string className, string siteName)
        {
            // Ensure site ID 
            int siteId = ECommerceHelper.GetSiteID(siteName, ECommerceSettings.USE_GLOBAL_TAX_CLASSES);

            return GetInfoByCodeName(className, siteId, true);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns all tax classes on the specified site.
        /// </summary>
        /// <param name="siteId">ID of the site with the tax classes.</param>        
        /// <returns>Site's tax class query.</returns>       
        protected virtual ObjectQuery<TaxClassInfo> GetTaxClassesInternal(int siteId)
        {
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_TAX_CLASSES);

            return GetTaxClasses().OnSite(siteId);
        }

        #endregion
    }
}