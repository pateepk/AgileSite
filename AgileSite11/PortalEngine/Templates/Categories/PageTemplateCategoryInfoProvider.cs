using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.PortalEngine
{
    using TypedDataSet = InfoDataSet<PageTemplateCategoryInfo>;

    /// <summary>
    /// Provides access to information about page template categories.
    /// </summary>
    public class PageTemplateCategoryInfoProvider : AbstractInfoProvider<PageTemplateCategoryInfo, PageTemplateCategoryInfoProvider>, IRelatedObjectCountProvider
    {
        #region "Methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static PageTemplateCategoryInfo GetPageTemplateCategoryInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets the specified page template category data from info object to DB.
        /// </summary>
        /// <param name="pageTemplateCategory">Page template data object</param>
        public static void SetPageTemplateCategoryInfo(PageTemplateCategoryInfo pageTemplateCategory)
        {
            ProviderObject.SetInfo(pageTemplateCategory);
        }


        /// <summary>
        /// Returns the PageTemplateCategoryInfo structure for the specified CategoryId.
        /// </summary>
        /// <param name="pageTemplateCategoryId">Page CategoryID to use for retrieving the resource data</param>
        public static PageTemplateCategoryInfo GetPageTemplateCategoryInfo(int pageTemplateCategoryId)
        {
            return ProviderObject.GetInfoById(pageTemplateCategoryId);
        }


        /// <summary>
        /// Returns the PageTemplateCategoryInfo structure for the specified CategoryDisplayName.
        /// </summary>
        /// <param name="pageTemplateCategoryName">Category code name to use for retrieving the resource data</param>
        public static PageTemplateCategoryInfo GetPageTemplateCategoryInfo(string pageTemplateCategoryName)
        {
            return ProviderObject.GetInfoByCodeName(pageTemplateCategoryName);
        }


        /// <summary>
        /// Returns all page template categories.
        /// </summary>
        public static ObjectQuery<PageTemplateCategoryInfo> GetPageTemplateCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns list of categories with given ParentCategoryID.
        /// </summary>
        /// <param name="parentCategoryId">Id of parent category</param>
        /// <returns>DataSet with list of categories</returns>
        public static TypedDataSet GetDescendantCategories(int parentCategoryId)
        {
            return ProviderObject.GetDescendantCategoriesInternal(parentCategoryId);
        }


        /// <summary>
        /// Deletes template category.
        /// </summary>
        /// <param name="ptci">PageTemplateCategory object</param>
        public static void DeletePageTemplateCategory(PageTemplateCategoryInfo ptci)
        {
            ProviderObject.DeleteInfo(ptci);
        }


        /// <summary>
        /// Deletes template category defined by id.
        /// </summary>
        /// <param name="pageTemplateCategoryId">Id of category to delete</param>
        public static void DeletePageTemplateCategory(int pageTemplateCategoryId)
        {
            if (pageTemplateCategoryId <= 0)
            {
                throw new Exception("[PageTemplateCategoryInfoProvider.DeletePageTemplateCategory]: Cannot delete root page template category.");
            }

            PageTemplateCategoryInfo ptci = GetPageTemplateCategoryInfo(pageTemplateCategoryId);
            DeletePageTemplateCategory(ptci);
        }


        /// <summary>
        /// Returns list of all template categories with templates count.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <returns>DataSet with list of categories</returns>
        public static TypedDataSet GetCategoriesList(string whereCondition, string orderBy)
        {
            return ProviderObject.GetCategoriesListInternal(whereCondition, orderBy);
        }


        /// <summary>
        /// Returns list of all template categories with templates count for specific site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <returns>DataSet with list of categories</returns>
        public static TypedDataSet GetSiteCategoriesList(int siteId, string whereCondition, string orderBy)
        {
            return ProviderObject.GetSiteCategoriesListInternal(siteId, whereCondition, orderBy);
        }


        /// <summary>
        /// Returns list of all template categories with template allowed for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static TypedDataSet GetCategoriesWithTemplate(int siteId)
        {
            return ProviderObject.GetCategoriesWithTemplateInternal(siteId);
        }


        /// <summary>
        /// Updates the child categories count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        public static void UpdateCategoryChildCount(int originalParentID, int newParentID)
        {
            ProviderObject.UpdateCategoryChildCountInternal(originalParentID, newParentID);
        }


        /// <summary>
        /// Updates the child templates count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        public static void UpdateCategoryTemplateChildCount(int originalParentID, int newParentID)
        {
            ProviderObject.UpdateCategoryTemplateChildCountInternal(originalParentID, newParentID);
        }


        /// <summary>
        /// Gets ad-hoc category info object. If category doesn't exist, creates new one.
        /// </summary>
        public static PageTemplateCategoryInfo GetAdHocCategory()
        {
            return ProviderObject.GetAdHocCategoryInternal();
        }


        /// <summary>
        /// Gets ad-hoc UI category info object. If category doesn't exist, creates new one.
        /// </summary>
        public static PageTemplateCategoryInfo GetAdHocUICategory()
        {
            return ProviderObject.GetAdHocUICategoryInternal();
        }


        /// <summary>
        /// Returns the DataSet of all the categories on the path to the given category.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        public static TypedDataSet GetCategoryPath(int categoryId)
        {
            return ProviderObject.GetCategoryPathInternal(categoryId);
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        public void RefreshObjectsCounts()
        {
            RefreshDataCountsInternal();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(PageTemplateCategoryInfo info)
        {
            if (info != null)
            {
                // Ensure the code name
                info.Generalized.EnsureCodeName();

                if (info.CategoryId == 0)
                {
                    info.CategoryPath = "";
                    info.CategoryLevel = 0;
                }

                PageTemplateCategoryInfo parent = null;
                int originalParent = 0;

                // Category is not root category
                if (info.ParentId != 0)
                {
                    parent = GetInfoById(info.ParentId);
                }
                // Insert to the root category if category is not root and doesn't have parent
                else if (!info.CategoryPath.EndsWithCSafe("/"))
                {
                    parent = GetInfoByCodeName("/");
                    if (parent != null)
                    {
                        info.ParentId = parent.CategoryId;
                    }
                }

                // Update
                if (info.CategoryId > 0)
                {
                    // Get object from database because of parent category changing
                    PageTemplateCategoryInfo ptci = GetInfoById(info.CategoryId);
                    if (ptci != null)
                    {
                        originalParent = ptci.ParentId;
                    }
                }

                base.SetInfo(info);

                if (parent != null)
                {
                    // Update parent category child count
                    UpdateCategoryChildCount(originalParent, info.ParentId);
                }
            }
        }


        /// <summary>
        /// Returns list of categories with given ParentCategoryID.
        /// </summary>
        /// <param name="parentCategoryId">Id of parent category</param>
        /// <returns>DataSet with list of categories</returns>
        protected virtual TypedDataSet GetDescendantCategoriesInternal(int parentCategoryId)
        {
            // parameters preparation
            var parameters = new QueryDataParameters();
            parameters.Add("@ParentCategoryID", parentCategoryId);
            parameters.EnsureDataSet<PageTemplateCategoryInfo>();

            return ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.selectdescendantcategories", parameters).As<PageTemplateCategoryInfo>();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(PageTemplateCategoryInfo info)
        {
            if (info != null)
            {
                int parentID = info.ParentId;

                // Delete object
                base.DeleteInfo(info);

                // Update parent category child count
                UpdateCategoryChildCount(0, parentID);
            }
        }


        /// <summary>
        /// Returns list of all template categories with templates count.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <returns>DataSet with list of categories</returns>
        protected virtual TypedDataSet GetCategoriesListInternal(string whereCondition, string orderBy)
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<PageTemplateCategoryInfo>();

            return ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.selectlist", parameters, whereCondition, orderBy).As<PageTemplateCategoryInfo>();
        }


        /// <summary>
        /// Returns list of all template categories with templates count for specific site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <returns>DataSet with list of categories</returns>
        protected virtual TypedDataSet GetSiteCategoriesListInternal(int siteId, string whereCondition, string orderBy)
        {
            // Prepare parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.EnsureDataSet<PageTemplateCategoryInfo>();

            return ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.selectsitelist", parameters, whereCondition, orderBy).As<PageTemplateCategoryInfo>();
        }


        /// <summary>
        /// Returns list of all template categories with template allowed for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual TypedDataSet GetCategoriesWithTemplateInternal(int siteId)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.EnsureDataSet<PageTemplateCategoryInfo>();

            return ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.selectCategoriesWithAllowedTemplate", parameters, null, "CategoryDisplayName").As<PageTemplateCategoryInfo>();
        }


        /// <summary>
        /// Returns the DataSet of all the categories on the path to the given category.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        protected virtual TypedDataSet GetCategoryPathInternal(int categoryId)
        {
            if (categoryId <= 0)
            {
                return null;
            }

            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@CategoryID", categoryId);
            parameters.EnsureDataSet<PageTemplateCategoryInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("CMS.PageTemplateCategory.SelectCategoryPath", parameters).As<PageTemplateCategoryInfo>();
        }


        /// <summary>
        /// Updates the child categories count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        protected virtual void UpdateCategoryChildCountInternal(int originalParentID, int newParentID)
        {
            // Update child nodes count if parent changed
            if (newParentID != originalParentID)
            {
                // Update parent child nodes count
                QueryDataParameters parameters;

                // Original parent
                if (originalParentID > 0)
                {
                    parameters = new QueryDataParameters();
                    parameters.Add("@CategoryID", originalParentID);

                    ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.updatecategorychildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.updatecategorychildcount", parameters);
            }
        }


        /// <summary>
        /// Updates the child templates count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        protected virtual void UpdateCategoryTemplateChildCountInternal(int originalParentID, int newParentID)
        {
            // Update child nodes count if parent changed
            if (newParentID != originalParentID)
            {
                // Update parent child nodes count
                QueryDataParameters parameters;

                // Original parent
                if (originalParentID > 0)
                {
                    parameters = new QueryDataParameters();
                    parameters.Add("@CategoryID", originalParentID);

                    ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.updatecategorytemplatechildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.updatecategorytemplatechildcount", parameters);
            }
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        protected virtual void RefreshDataCountsInternal()
        {
            ConnectionHelper.ExecuteQuery("cms.pagetemplatecategory.refreshdatacounts", null);
        }


        /// <summary>
        /// Gets ad-hoc category info object. If category doesn't exist, creates new one.
        /// </summary>
        protected virtual PageTemplateCategoryInfo GetAdHocCategoryInternal()
        {
            // Try to get adhoc category name
            var ci = GetPageTemplateCategoryInfo("adhoc");
            if (ci == null)
            {
                ci = new PageTemplateCategoryInfo();
                ci.CategoryName = "AdHoc";
                ci.DisplayName = "Ad-hoc";

                // Parent category ID
                var cp = GetPageTemplateCategoryInfo("/");
                if (cp != null)
                {
                    ci.ParentId = cp.CategoryId;
                }

                SetPageTemplateCategoryInfo(ci);
            }

            return ci;
        }


        /// <summary>
        /// Gets ad-hoc UI category info object. If category doesn't exist, creates new one.
        /// </summary>
        protected virtual PageTemplateCategoryInfo GetAdHocUICategoryInternal()
        {
            // Try to get adhoc category name
            var ci = GetPageTemplateCategoryInfo("adhocui");
            if (ci == null)
            {
                ci = new PageTemplateCategoryInfo();
                ci.CategoryName = "AdHocUI";
                ci.DisplayName = "Ad-hoc";

                // Parent category ID
                var cp = GetPageTemplateCategoryInfo("UITemplates");
                if (cp != null)
                {
                    ci.ParentId = cp.CategoryId;
                }

                SetPageTemplateCategoryInfo(ci);
            }

            return ci;
        }

        #endregion
    }
}