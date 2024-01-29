using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing OptionCategoryInfo management.
    /// </summary>
    public class OptionCategoryInfoProvider : AbstractInfoProvider<OptionCategoryInfo, OptionCategoryInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionCategoryInfoProvider()
            : base(OptionCategoryInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all option categories.
        /// </summary>        
        public static ObjectQuery<OptionCategoryInfo> GetOptionCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns option category with specified ID.
        /// </summary>
        /// <param name="categoryId">Option category ID</param>        
        public static OptionCategoryInfo GetOptionCategoryInfo(int categoryId)
        {
            return ProviderObject.GetInfoById(categoryId);
        }


        /// <summary>
        /// Returns option category with specified name.
        /// </summary>
        /// <param name="categoryName">Option category name</param>                
        /// <param name="siteName">Site name</param>                
        public static OptionCategoryInfo GetOptionCategoryInfo(string categoryName, string siteName)
        {
            return ProviderObject.GetOptionCategoryInfoInternal(categoryName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified option category.
        /// </summary>
        /// <param name="categoryObj">Option category to be set</param>
        public static void SetOptionCategoryInfo(OptionCategoryInfo categoryObj)
        {
            ProviderObject.SetInfo(categoryObj);
        }


        /// <summary>
        /// Deletes specified option category.
        /// </summary>
        /// <param name="categoryObj">Option category to be deleted</param>
        public static void DeleteOptionCategoryInfo(OptionCategoryInfo categoryObj)
        {
            ProviderObject.DeleteInfo(categoryObj);
        }


        /// <summary>
        /// Deletes option category with specified ID.
        /// </summary>
        /// <param name="categoryId">Option category ID</param>
        public static void DeleteOptionCategoryInfo(int categoryId)
        {
            var categoryObj = GetOptionCategoryInfo(categoryId);
            DeleteOptionCategoryInfo(categoryObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns query for all option categories for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        public static ObjectQuery<OptionCategoryInfo> GetOptionCategories(int siteId)
        {
            return ProviderObject.GetOptionCategoriesInternal(siteId);
        }


        /// <summary>
        /// Returns DataSet with all option categories which are assigned to the specified product.
        /// </summary>
        /// <param name="skuId">Product ID</param>
        /// <param name="onlyEnabled">True - only enabled option categories are included, False - all option categories are included</param>
        /// <param name="categoryType">Type of option category. Allows to filter only categories of specific type.</param>        
        public static ObjectQuery<OptionCategoryInfo> GetProductOptionCategories(int skuId, bool onlyEnabled, OptionCategoryTypeEnum? categoryType = null)
        {
            return ProviderObject.GetProductOptionCategoriesInternal(skuId, onlyEnabled, categoryType);
        }


        /// <summary>
        /// Indicates if user is authorized to modify specific option category. 
        /// For global option category: 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific manufacturer: 'EcommerceModify' OR 'ModifyProducts' permission is checked.
        /// </summary>
        /// <param name="categoryObj">Option category to be checked</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedToModifyOptionCategory(OptionCategoryInfo categoryObj, string siteName, IUserInfo user)
        {
            return (categoryObj != null) && IsUserAuthorizedToModifyOptionCategory(categoryObj.IsGlobal, siteName, user);
        }


        /// <summary>
        /// Indicates if user is authorized to modify option categories.
        /// </summary>
        /// <param name="global">For global option categories (global = True): 'EcommerceGlobalModify' permission is checked. 
        /// For site-specific option categories (global = False): 'EcommerceModify' OR 'ModifyProducts' permission is checked.</param>        
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToModifyOptionCategory(bool global, string siteName, IUserInfo user, bool exceptionOnFailure = false)
        {
            var permission = global ? EcommercePermissions.ECOMMERCE_MODIFYGLOBAL : EcommercePermissions.PRODUCTS_MODIFY;

            return ECommerceHelper.IsUserAuthorizedForPermission(permission, siteName, user, exceptionOnFailure);
        }


        /// <summary>
        /// Gets info about how many options are available in specific option categories for given product.
        /// Returns dataset with one table containing OptionCategoryID, AllowAllOptions flag, SelectedOptions count and AllOptions count.
        /// </summary>
        /// <param name="skuId">Product ID to return counts for.</param>
        /// <param name="categoryIds">IDs of categories to get counts for. Use null for all relevant categories.</param>
        public static DataSet GetAllowedOptionsCount(int skuId, IEnumerable<int> categoryIds)
        {
            return ProviderObject.GetAllowedOptionsCountInternal(skuId, categoryIds);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns option category with specified name.
        /// </summary>
        /// <param name="categoryName">Option category name</param>                
        /// <param name="siteName">Site name</param>         
        protected virtual OptionCategoryInfo GetOptionCategoryInfoInternal(string categoryName, string siteName)
        {
            bool searchGlobal = ECommerceSettings.AllowGlobalProductOptions(siteName);

            return GetInfoByCodeName(categoryName, SiteInfoProvider.GetSiteID(siteName), true, searchGlobal);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns query for all option categories for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<OptionCategoryInfo> GetOptionCategoriesInternal(int siteId)
        {
            // Check if site uses site or global product options
            var includeGlobal = ECommerceSettings.AllowGlobalProductOptions(siteId);

            // Get option categories on requested site
            var query = GetOptionCategories().OnSite(siteId, includeGlobal);

            return query;
        }


        /// <summary>
        /// Returns DataSet with all option categories which are assigned to the specified product.
        /// </summary>
        /// <param name="skuId">Product ID</param>
        /// <param name="onlyEnabled">True - only enabled option categories are included, False - all option categories are included</param>
        /// <param name="categoryType">Type of option category. Allows to filter only categories of specific type.</param>
        protected virtual ObjectQuery<OptionCategoryInfo> GetProductOptionCategoriesInternal(int skuId, bool onlyEnabled, OptionCategoryTypeEnum? categoryType = null)
        {
            // Only enabled option categories which belongs to SKU
            var where = new WhereCondition();
            if (onlyEnabled)
            {
                where.WhereTrue("CategoryEnabled");
            }

            where.WhereEquals("SKUID", skuId);

            // Filter according to category type when specified
            if (categoryType.HasValue)
            {
                where.WhereEquals("CategoryType", categoryType.Value.ToStringRepresentation());
            }

            // Join with SKUOptionCategory to retrieve categories in right order
            return GetOptionCategories()
                .Source(s => s.Join<SKUOptionCategoryInfo>("CategoryID", "CategoryID"))
                .Where(where)
                .Columns("COM_OptionCategory.*")
                .OrderBy("SKUCategoryOrder");
        }


        /// <summary>
        /// Gets info about how many options are available in specific option categories for given product.
        /// Returns dataset with one table containing OptionCategoryID, AllowAllOptions flag, SelectedOptions count and AllOptions count.
        /// </summary>
        /// <param name="skuId">Product ID to return counts for.</param>
        /// <param name="categoryIds">IDs of categories to get counts for. Use null for all relevant categories.</param>
        protected virtual DataSet GetAllowedOptionsCountInternal(int skuId, IEnumerable<int> categoryIds)
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@SKUID", skuId);
            parameters.EnsureDataSet<OptionCategoryInfo>();

            // Prepare where condition to filter only requested categories
            var where = new WhereCondition();
            if (categoryIds != null)
            {
                where.WhereIn("SKUCategory.CategoryID", categoryIds.ToList());
            }

            // Get the data            
            return ConnectionHelper.ExecuteQuery("ecommerce.optioncategory.SelectAllowedOptionsCounts", parameters, where.ToString(true));
        }

        #endregion
    }
}