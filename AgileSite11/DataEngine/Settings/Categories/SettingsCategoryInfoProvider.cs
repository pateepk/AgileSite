using System;
using System.Data;
using System.Linq;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    using TypedDataSet = InfoDataSet<SettingsCategoryInfo>;

    /// <summary>
    /// Class providing Settings category management.
    /// </summary>
    public class SettingsCategoryInfoProvider : AbstractInfoProvider<SettingsCategoryInfo, SettingsCategoryInfoProvider>, IRelatedObjectCountProvider

    {
        #region "Static variables"

        /// <summary>
        /// Info object representing Settings root category.
        /// </summary>
        public static SettingsCategoryInfo mRootCategory = null;

        #endregion


        #region "Public properties"
        
        /// <summary>
        /// Returns root settings category (this property is cached).
        /// </summary>
        public static SettingsCategoryInfo RootCategory
        {
            get
            {
                if (mRootCategory == null)
                {
                    mRootCategory = GetRootSettingsCategoryInfo();
                }
                return mRootCategory;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsCategoryInfoProvider()
            : base(SettingsCategoryInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    Name = true
                })
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Gets root of categories. This is distinguished by CategoryLevel = 0. If more found -> use first.
        /// </summary>
        public static SettingsCategoryInfo GetRootSettingsCategoryInfo()
        {
            return GetSettingsCategories().WhereEquals("CategoryLevel", 0).OrderBy("CategoryID").TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Returns the SettingsCategoryInfo structure for the specified category.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        public static SettingsCategoryInfo GetSettingsCategoryInfo(int categoryId)
        {
            return ProviderObject.GetInfoById(categoryId);
        }


        /// <summary>
        /// Returns the SettingsCategoryInfo structure for the name.
        /// </summary>
        /// <param name="categoryName">CategoryName</param>
        public static SettingsCategoryInfo GetSettingsCategoryInfoByName(string categoryName)
        {
            return ProviderObject.GetInfoByCodeName(categoryName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified category.
        /// </summary>
        /// <param name="category">Settings category to set</param>
        public static void SetSettingsCategoryInfo(SettingsCategoryInfo category)
        {
            ProviderObject.SetInfo(category);
        }


        /// <summary>
        /// Deletes specified category.
        /// </summary>
        /// <param name="categoryObj">Country object</param>
        public static void DeleteSettingsCategoryInfo(SettingsCategoryInfo categoryObj)
        {
            ProviderObject.DeleteInfo(categoryObj);
        }


        /// <summary>
        /// Deletes specified category.
        /// </summary>
        /// <param name="categoryId">Country ID</param>
        public static void DeleteSettingsCategoryInfo(int categoryId)
        {
            SettingsCategoryInfo categoryObj = GetSettingsCategoryInfo(categoryId);
            DeleteSettingsCategoryInfo(categoryObj);
        }


        /// <summary>
        /// Gets all settings categories.
        /// </summary>
        public static ObjectQuery<SettingsCategoryInfo> GetSettingsCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets all categories.
        /// </summary>    
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="topN">Number of data rows which should be included in the result</param>
        /// <param name="columns">Table columns separated by comma which should be included in the result</param>
        public static TypedDataSet GetSettingsCategories(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return ProviderObject.GetSettingsCategoriesInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets DataSet with child setting categories of the specified parent setting category.
        /// </summary>
        /// <param name="parentCategoryId">ID of the parent setting category</param>
        public static TypedDataSet GetChildSettingsCategories(int parentCategoryId)
        {
            return ProviderObject.GetChildSettingsCategoriesInternal(parentCategoryId);
        }


        /// <summary>
        /// Gets DataSet with child setting categories of the specified parent setting category, filtered by where condition.
        /// </summary>
        /// <param name="parentCategoryName">Name of the parent setting category</param>
        /// <param name="where">Where condition</param>
        public static TypedDataSet GetChildSettingsCategories(string parentCategoryName, string where)
        {
            return ProviderObject.GetChildSettingsCategoriesInternal(parentCategoryName, where);
        }


        /// <summary>
        /// Moves specified category up.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        public static void MoveCategoryUp(int categoryId)
        {
            ProviderObject.MoveCategoryUpInternal(categoryId);
        }


        /// <summary>
        /// Moves specified category down.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        public static void MoveCategoryDown(int categoryId)
        {
            ProviderObject.MoveCategoryDownInternal(categoryId);
        }


        /// <summary>
        /// Sets correct SettingsCategoryChildCount to the specified SettingsCategory.
        /// </summary>
        /// <param name="settingsCategoryID">ID of the SettingsCategory</param>
        public static void SetSettingsCategoryChildCount(int settingsCategoryID)
        {
            ProviderObject.SetSettingsCategoryChildCountInternal(settingsCategoryID);
        }


        /// <summary>
        /// Returns last SettingsCategory order for specified parent SettingsCategory.
        /// </summary>
        /// <param name="parentCategoryID">Parent SettingsCategory ID</param>
        public static int GetLastSettingsCategoryOrder(int parentCategoryID)
        {
            return ProviderObject.GetLastSettingsCategoryOrderInternal(parentCategoryID);
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        public static void RefreshDataCounts()
        {
            ProviderObject.RefreshDataCountsInternal();
        }


        /// <summary>
        /// Gets where condition for all parent categories on path except root category.
        /// </summary>
        /// <param name="categoryIdPath">Category ID path</param>
        public static string GetCategoriesOnPathWhereCondition(string categoryIdPath)
        {
            string result = null;

            // Build the list of valid paths
            int index = 0;
            int count = 0;
            while (index < 100)
            {
                if (result != null)
                {
                    result += ", ";
                }
                result += "N'" + SqlHelper.GetSafeQueryString(categoryIdPath, false) + "'";
                count++;

                index++;

                // Get parent to get next value
                categoryIdPath = GetParentPath(categoryIdPath);

                // If root, end building the list
                if (categoryIdPath == "/")
                {
                    break;
                }
            }

            if (count <= 0)
            {
                // No results
                return "1=0";
            }
            else
            {
                if (count > 1)
                {
                    // List of results
                    return "CategoryIDPath IN (" + result + ")";
                }
                else
                {
                    // Single result
                    return "CategoryIDPath = " + result;
                }
            }
        }


        /// <summary>
        /// Gets all the categories on the specified category ID path in the corresponding order.
        /// </summary>
        /// <param name="categoryIdPath">Category ID path</param>
        public static TypedDataSet GetCategoriesOnPath(string categoryIdPath)
        {
            if (string.IsNullOrEmpty(categoryIdPath))
            {
                throw new ArgumentNullException("categoryIdPath");
            }

            var where = GetCategoriesOnPathWhereCondition(categoryIdPath) + " AND (CategoryLevel > 0)";
            var categories = GetSettingsCategories(where, "CategoryLevel");
            return categories;
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
        protected override void SetInfo(SettingsCategoryInfo info)
        {
            if (info == null)
            {
                throw new Exception("[SettingsCategoryInfoProvider.SetSettingsCategoryInfo]: No SettingsCategoryInfo object set.");
            }

            // Save the object
            if (info.CategoryID > 0)
            {
                SettingsCategoryInfo oldInfo = GetInfoById(info.CategoryID, false);
                if (oldInfo != null)
                {
                    int oldParentID = oldInfo.CategoryParentID;
                    int newParentID = info.CategoryParentID;

                    base.SetInfo(info);

                    if (oldParentID != newParentID)
                    {
                        // Update child count
                        SetSettingsCategoryChildCount(oldInfo.CategoryParentID);
                        SetSettingsCategoryChildCount(info.CategoryParentID);
                    }
                }
            }
            else
            {
                // Default values (not null columns)
                info.CategoryChildCount = 0;
                info.CategoryIDPath = "";
                info.CategoryLevel = 0;

                // Insert settings category
                base.SetInfo(info);

                // Update child count
                SetSettingsCategoryChildCount(info.CategoryParentID);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SettingsCategoryInfo info)
        {
            if (info != null)
            {
                // Delete category
                base.DeleteInfo(info);

                // Update child count
                SetSettingsCategoryChildCount(info.CategoryParentID);
            }
        }


        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="topN">Number of data rows which should be included in the result</param>
        /// <param name="columns">Table columns which should be included in the result</param>
        protected virtual TypedDataSet GetSettingsCategoriesInternal(string where, string orderBy, int topN, string columns)
        {
            return GetSettingsCategories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Gets DataSet with child setting categories of the specified parent category.
        /// </summary>
        /// <param name="parentCategoryId">ID of the parent setting category</param>
        protected virtual TypedDataSet GetChildSettingsCategoriesInternal(int parentCategoryId)
        {
            string where = null;

            // Check if looking for non-root category
            if (parentCategoryId > 0)
            {
                where = "CategoryParentID = " + parentCategoryId;
            }
            else
            {
                where = "CategoryParentID IS NULL";
            }

            // Get and return categories ordered by order column
            return GetSettingsCategories().Where(where).OrderBy("CategoryOrder").BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Gets DataSet with child setting categories of the specified parent category.
        /// </summary>
        /// <param name="parentCategoryName">Code name of the parent setting category</param>
        /// <param name="where">Where condition</param>
        protected virtual TypedDataSet GetChildSettingsCategoriesInternal(string parentCategoryName, string where)
        {
            // Check if parent category name is specified
            if (!string.IsNullOrEmpty(parentCategoryName))
            {
                string[] par = new string[1];
                par[0] = parentCategoryName;

                where = SqlHelper.AddWhereCondition(where, "CategoryParentID = (SELECT CategoryID FROM CMS_SettingsCategory WHERE " + SqlHelper.GetWhereCondition("CategoryName", par) + ")");
            }
            else
            {
                // Get root when no parent name specified
                where = SqlHelper.AddWhereCondition(where, "CategoryParentID IS NULL");
            }

            return GetSettingsCategories().Where(where).OrderBy("CategoryOrder").BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Moves specified category up.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        protected virtual void MoveCategoryUpInternal(int categoryId)
        {
            if (categoryId > 0)
            {
                var infoObj = GetInfoById(categoryId);
                if (infoObj != null)
                {
                    infoObj.Generalized.MoveObjectUp();
                }
            }
        }


        /// <summary>
        /// Moves specified category down.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        protected virtual void MoveCategoryDownInternal(int categoryId)
        {
            if (categoryId > 0)
            {
                var infoObj = GetInfoById(categoryId);
                if (infoObj != null)
                {
                    infoObj.Generalized.MoveObjectDown();
                }
            }
        }


        /// <summary>
        /// Sets correct CategoryChildCount to the specified SettingsCategory.
        /// </summary>
        /// <param name="settingsCategoryID">ID of the SettingsCategory</param>
        protected virtual void SetSettingsCategoryChildCountInternal(int settingsCategoryID)
        {
            if (settingsCategoryID > 0)
            {
                SettingsCategoryInfo sci = GetSettingsCategoryInfo(settingsCategoryID);
                if (sci != null)
                {
                    // Get count of child records
                    DataSet ds = GetSettingsCategories("CategoryParentID = " + settingsCategoryID.ToString(), null, 0, "CategoryID");
                    sci.CategoryChildCount = !DataHelper.DataSourceIsEmpty(ds) ? ds.Tables[0].Rows.Count : 0;

                    using (CMSActionContext context = new CMSActionContext())
                    {
                        // Disable logging of tasks
                        context.DisableLogging();

                        SetSettingsCategoryInfo(sci);
                    }
                }
            }
        }


        /// <summary>
        /// Returns last order for specified parent SettingsCategory.
        /// </summary>
        /// <param name="parentCategoryID">Parent SettingsCategory ID</param>
        protected virtual int GetLastSettingsCategoryOrderInternal(int parentCategoryID)
        {
            if (parentCategoryID > 0)
            {
                var orders = GetSettingsCategories().WhereEquals("CategoryParentID", parentCategoryID).OrderByDescending("CategoryOrder").TopN(1).Column("CategoryOrder").GetListResult<int>();
                if (orders.Any())
                {
                    return orders.First();
                }
            }
            return 0;
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        protected virtual void RefreshDataCountsInternal()
        {
            // Execute query
            ConnectionHelper.ExecuteQuery("cms.settingscategory.refreshdatacounts", null);
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Returns the parent path for the specified path (any kind of path with "/" as a separator)
        /// </summary>
        /// <param name="path">Original path</param>
        private static string GetParentPath(string path)
        {
            if (path == null)
            {
                return null;
            }
            else
            {
                int lastSeparator = path.LastIndexOfCSafe("/");
                if (lastSeparator > 0)
                {
                    return path.Substring(0, lastSeparator);
                }
                else
                {
                    return "/";
                }
            }
        }

        #endregion
    }
}