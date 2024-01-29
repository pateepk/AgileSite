using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Site related context methods and variables.
    /// </summary>
    [RegisterAllProperties]
    public class TaxonomyContext : AbstractContext<TaxonomyContext>
    {
        #region "Variables"

        private CategoryInfo mCurrentCategory;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current category info object according the URL parameter of the current request. 
        /// It is available when the request contains parameters "category" or "categoryname" with valid value of the category.
        /// </summary>
        public static CategoryInfo CurrentCategory
        {
            get
            {
                return GetCurrentCategory();
            }
            set
            {
                Current.mCurrentCategory = value;
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Returns information on the current category according category ID/name specified as an URL parameter of the current request
        /// </summary> 
        public static CategoryInfo GetCurrentCategory()
        {
            string siteName = SiteContext.CurrentSiteName;

            var c = Current;
            CategoryInfo ci = c.mCurrentCategory;
            if (ci == null)
            {
                // Get all the necessary information from the URL parameters collection
                int categoryId = QueryHelper.GetInteger("categoryid", 0);
                if (categoryId > 0)
                {
                    // Try to get the category info from the DB by the category ID first
                    ci = CategoryInfoProvider.GetCategoryInfo(categoryId);
                }

                if (ci == null)
                {
                    // Try to get the category info from the DB by the category name
                    string categoryName = QueryHelper.GetString("categoryname", "");
                    if (categoryName != "")
                    {
                        ci = CategoryInfoProvider.GetCategoryInfo(categoryName, siteName);
                    }
                }

                // Save the info to the session and request items
                c.mCurrentCategory = ci;
            }

            if (ci != null)
            {
                // Disabled categories and categories from other sites are not visible
                if (!ci.CategoryEnabled || (!ci.IsGlobal && (ci.CategorySiteID != SiteContext.CurrentSiteID)))
                {
                    return null;
                }

                // Global categories denied by settings are not visible
                if (!ci.CategoryIsPersonal && ci.CategoryIsGlobal && !SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAllowGlobalCategories"))
                {
                    return null;
                }
            }

            // Return category info
            return ci;
        }
        
        #endregion
    }
}