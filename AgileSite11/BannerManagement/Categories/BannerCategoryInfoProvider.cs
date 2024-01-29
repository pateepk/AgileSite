using System;

using CMS.DataEngine;

namespace CMS.BannerManagement
{
    /// <summary>
    /// Class providing BannerCategoryInfo management.
    /// </summary>
    public class BannerCategoryInfoProvider : AbstractInfoProvider<BannerCategoryInfo, BannerCategoryInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public BannerCategoryInfoProvider()
            : base(null, new HashtableSettings
                {
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns object query of all banner categories matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<BannerCategoryInfo> GetBannerCategories(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetBannerCategories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }


        /// <summary>
        /// Returns banner category with specified ID.
        /// </summary>
        /// <param name="categoryId">Banner category ID.</param>        
        public static BannerCategoryInfo GetBannerCategoryInfo(int categoryId)
        {
            return ProviderObject.GetInfoById(categoryId);
        }


        /// <summary>
        /// Returns banner category with specified code name.
        /// </summary>
        /// <param name="categoryCodeName">Banner category code name.</param>        
        public static BannerCategoryInfo GetBannerCategoryInfo(string categoryCodeName)
        {
            return ProviderObject.GetInfoByCodeName(categoryCodeName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified banner category.
        /// </summary>
        /// <param name="categoryObj">Banner category to be set.</param>
        public static void SetBannerCategoryInfo(BannerCategoryInfo categoryObj)
        {
            ProviderObject.SetInfo(categoryObj);
        }


        /// <summary>
        /// Deletes specified banner category.
        /// </summary>
        /// <param name="categoryObj">Banner category to be deleted.</param>
        public static void DeleteBannerCategoryInfo(BannerCategoryInfo categoryObj)
        {
            ProviderObject.DeleteInfo(categoryObj);
        }


        /// <summary>
        /// Deletes banner category with specified ID.
        /// </summary>
        /// <param name="categoryId">Banner category ID.</param>
        public static void DeleteBannerCategoryInfo(int categoryId)
        {
            BannerCategoryInfo categoryObj = GetBannerCategoryInfo(categoryId);
            DeleteBannerCategoryInfo(categoryObj);
        }


        /// <summary>
        /// Returns the query for all banner categories.
        /// </summary>        
        public static ObjectQuery<BannerCategoryInfo> GetBannerCategories()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns object query for all banner categories for specified site ordered by category display name.
        /// </summary>
        /// <param name="siteId">Site ID.</param>        
        public static ObjectQuery<BannerCategoryInfo> GetBannerCategories(int siteId)
        {
            var result = GetBannerCategories().OrderBy("BannerCategoryDisplayName");

            return (siteId >= 0) ? result.OnSite(siteId) : result;
        }


        /// <summary>
        /// Gets banner category info with the specified name. First, banner categories assigned to the
        /// site specified by <paramref name="siteId"/> are searched and then global banner categories.
        /// 
        /// If there is no banner category with this name on specified site or global, null is returned.
        /// </summary>
        /// <param name="bannerCategoryCodeName">Code name.</param>
        /// <param name="siteId">Site to be searched.</param>
        public static BannerCategoryInfo GetBannerCategoryInfoFromSiteOrGlobal(string bannerCategoryCodeName, int siteId)
        {
            return ProviderObject.GetBannerCategoryInfoFromSiteOrGlobalInternal(bannerCategoryCodeName, siteId);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets banner category info with the specified name. First, banner categories assigned to the
        /// site specified by <paramref name="siteId"/> are searched and then global banner categories.
        /// 
        /// If there is no banner category with this name on specified site or global, null is returned.
        /// </summary>
        /// <param name="bannerCategoryCodeName">Code name.</param>
        /// <param name="siteId">Site to be searched.</param>
        protected virtual BannerCategoryInfo GetBannerCategoryInfoFromSiteOrGlobalInternal(string bannerCategoryCodeName, int siteId)
        {
            return GetInfoByCodeName(bannerCategoryCodeName, siteId) ?? GetInfoByCodeName(bannerCategoryCodeName, 0);
        }

        #endregion
    }
}
