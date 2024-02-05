using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    using TypedDataSet = InfoDataSet<WebPartCategoryInfo>;

    /// <summary>
    /// Provides access to information about WebPartCategory.
    /// </summary>
    public class WebPartCategoryInfoProvider : AbstractInfoProvider<WebPartCategoryInfo, WebPartCategoryInfoProvider>, IRelatedObjectCountProvider
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebPartCategoryInfoProvider()
            : base(WebPartCategoryInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        { 
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns all web part categories.
        /// </summary>
        public static ObjectQuery<WebPartCategoryInfo> GetCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets dataset with fist level categories in specified category.
        /// </summary>
        /// <param name="parentCategoryId">ID of category to retrieve sub-categories</param>
        public static TypedDataSet GetCategories(int parentCategoryId)
        {
            return ProviderObject.GetCategoriesInternal(parentCategoryId);
        }


        /// <summary>
        /// Sets the specified WebPartCategory data.
        /// </summary>
        /// <param name="categoryObj">WebPartCategory data object</param>
        public static void SetWebPartCategoryInfo(WebPartCategoryInfo categoryObj)
        {
            ProviderObject.SetInfo(categoryObj);
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static WebPartCategoryInfo GetWebPartCategoryInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the WebPartCategoryInfo structure by the code name.
        /// </summary>
        /// <param name="categoryName">Category code name</param>
        public static WebPartCategoryInfo GetWebPartCategoryInfoByCodeName(string categoryName)
        {
            return ProviderObject.GetInfoByCodeName(categoryName);
        }


        /// <summary>
        /// Returns the WebPartCategoryInfo structure for the specified layout.
        /// </summary>
        /// <param name="categoryId">Id of the category to retrieve</param>
        public static WebPartCategoryInfo GetWebPartCategoryInfoById(int categoryId)
        {
            return ProviderObject.GetInfoById(categoryId);
        }


        /// <summary>
        /// Delete specified WebPartCategory.
        /// </summary>
        /// <param name="webPartCategoryInfo">WebPartCategory object</param>
        public static void DeleteCategoryInfo(WebPartCategoryInfo webPartCategoryInfo)
        {
            ProviderObject.DeleteInfo(webPartCategoryInfo);
        }


        /// <summary>
        /// Delete specified WebPartCategory.
        /// </summary>
        /// <param name="categoryId">Layout id to delete</param>
        public static void DeleteCategoryInfo(int categoryId)
        {
            if (categoryId <= 0)
            {
                throw new Exception("[WebPartCategoryInfoProvider.DeleteCategory]: Cannot delete root web part category.");
            }
            else
            {
                WebPartCategoryInfo webPartCategoryInfo = GetWebPartCategoryInfoById(categoryId);
                DeleteCategoryInfo(webPartCategoryInfo);
            }
        }


        /// <summary>
        /// Returns the DataSet of all the categories on the path to the given category.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        public static TypedDataSet GetCategoryPath(int categoryId)
        {
            return ProviderObject.GetCategoryPathInternal(categoryId);
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
        /// Updates the child webparts count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        public static void UpdateCategoryWebPartChildCount(int originalParentID, int newParentID)
        {
            ProviderObject.UpdateCategoryWebPartChildCountInternal(originalParentID, newParentID);
        }


        /// <summary>
        /// Returns dataset with categories and webparts
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        public static DataSet GetCategoriesAndWebparts(String where, String orderBy, int topN, String columns)
        {
            return ProviderObject.GetCategoriesAndWebpartsInternal(where, orderBy, topN, columns);
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
        /// Returns dataset with categories and webparts
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual DataSet GetCategoriesAndWebpartsInternal(String where, String orderBy, int topN, String columns)
        {
            return ConnectionHelper.ExecuteQuery("cms.webpartcategory.selectallview", null, where, orderBy, 0, columns);
        }


        /// <summary>
        /// Gets dataset with fist level categories in specified category.
        /// </summary>
        /// <param name="parentCategoryId">ID of category to retrieve sub-categories</param>
        protected virtual TypedDataSet GetCategoriesInternal(int parentCategoryId)
        {
            if (parentCategoryId != 0)
            {
                return GetCategories().WhereEquals("CategoryParentID", parentCategoryId).OrderBy("CategoryParentID, CategoryDisplayName").BinaryData(true).TypedResult;
            }

            return GetCategories().WhereNull("CategoryParentID").OrderBy("CategoryDisplayName").BinaryData(true).TypedResult;
        }

        
        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WebPartCategoryInfo info)
        {
            if (info != null)
            {
                // Ensure the code name
                info.Generalized.EnsureCodeName();

                if (info.CategoryID == 0)
                {
                    info.CategoryPath = "";
                    info.CategoryLevel = 0;
                }

                WebPartCategoryInfo parent = null;

                // Category is not root category
                if (info.CategoryParentID != 0)
                {
                    // Update path based on parent category
                    parent = GetInfoById(info.CategoryParentID);
                }
                else if (!info.CategoryPath.EndsWithCSafe("/"))
                {
                    // Insert to the root category if category is not root and doesn't have parent
                    parent = GetInfoByCodeName("/");
                    if (parent != null)
                    {
                        info.CategoryParentID = parent.CategoryID;
                    }
                }

                int originalParent = 0;

                if (info.CategoryID > 0)
                {
                    // Get object form DB because of parent category changing
                    WebPartCategoryInfo wci = GetInfoById(info.CategoryID);
                    if (wci != null)
                    {
                        originalParent = wci.CategoryParentID;
                    }
                }

                // Update the object
                base.SetInfo(info);

                if (parent != null)
                {
                    // Update parent category child count
                    UpdateCategoryChildCount(originalParent, info.CategoryParentID);
                }
            }
            else
            {
                throw new Exception("[WebPartCategoryInfoProvider.SetWebPartCategoryInfo]: No WebPartCategoryInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WebPartCategoryInfo info)
        {
            if (info != null)
            {
                int parentID = info.CategoryParentID;

                // Prepare the parameters
                var parameters = new QueryDataParameters();
                parameters.Add("@ID", info.CategoryID);

                // Sub-categories have to be deleted first
                DataSet categoryDataSet = ConnectionHelper.ExecuteQuery("cms.WebPartCategory.selectSubCategories", parameters);
                // SELECT COUNT(CategoryID) AS Expr1 FROM CMS_WebPartCategory WHERE (CategoryParentID = @ID)

                if (!DataHelper.DataSourceIsEmpty(categoryDataSet))
                {
                    if (ValidationHelper.GetInteger(categoryDataSet.Tables[0].Rows[0].ItemArray[0], 0) != 0) // some sub-categories exists
                    {
                        throw new CheckDependenciesException(info, "Please delete the sub-categories first");
                    }

                    // Deletes category
                    base.DeleteInfo(info);
                }

                // Update parent category child count
                UpdateCategoryChildCount(0, parentID);
            }
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
            parameters.EnsureDataSet<WebPartCategoryInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("CMS.WebPartCategory.SelectCategoryPath", parameters).As<WebPartCategoryInfo>();
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

                    ConnectionHelper.ExecuteQuery("cms.webpartcategory.updatecategorychildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("cms.webpartcategory.updatecategorychildcount", parameters);
            }
        }


        /// <summary>
        /// Updates the child webparts count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        protected virtual void UpdateCategoryWebPartChildCountInternal(int originalParentID, int newParentID)
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

                    ConnectionHelper.ExecuteQuery("cms.webpartcategory.updatecategorywebpartchildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("cms.webpartcategory.updatecategorywebpartchildcount", parameters);
            }
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        protected virtual void RefreshDataCountsInternal()
        {
            ConnectionHelper.ExecuteQuery("cms.webpartcategory.refreshdatacounts", null);
        }

        #endregion
    }
}