using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.ContinuousIntegration;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.Taxonomy
{
    using TypedDataSet = InfoDataSet<CategoryInfo>;

    /// <summary>
    /// Class providing CategoryInfo management.
    /// </summary>
    public class CategoryInfoProvider : AbstractInfoProvider<CategoryInfo, CategoryInfoProvider>
    {
        #region "Constants"

        private const int IDPATH_COLUMN_LENGTH = 450;

        #endregion


        #region "Variables"

        private static readonly CMSLazy<int> mCategoryIDLength = new CMSLazy<int>(() => ValidationHelper.GetInteger(SettingsHelper.AppSettings["CategoryIDLength"], 8));
        private static int? mMaxCategoryLevel;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Length of the category ID, 8 by default.
        /// </summary>
        public static int CategoryIDLength
        {
            get
            {
                return mCategoryIDLength.Value;
            }
        }


        /// <summary>
        /// Returns maximal available level of the category.
        /// </summary>
        public static int MaxCategoryLevel
        {
            get
            {
                if (!mMaxCategoryLevel.HasValue)
                {
                    mMaxCategoryLevel = (IDPATH_COLUMN_LENGTH / (CategoryIDLength + 1)) - 2;
                }
                return mMaxCategoryLevel.Value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CategoryInfoProvider()
            : base(CategoryInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the categoryInfo objects.
        /// </summary>
        public static ObjectQuery<CategoryInfo> GetCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a query for all categories matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<CategoryInfo> GetCategories(string where, string orderBy, int topN = 0, string columns = null)
        {
            return GetCategories().Where(where).OrderBy(orderBy).TopN(topN).Column(columns);
        }


        /// <summary>
        /// Returns category with specified ID.
        /// </summary>
        /// <param name="categoryId">Category ID.</param>        
        public static CategoryInfo GetCategoryInfo(int categoryId)
        {
            return ProviderObject.GetInfoById(categoryId);
        }


        /// <summary>
        /// Returns category with specified name.
        /// </summary>
        /// <param name="categoryName">Category name.</param>                
        /// <param name="siteName">Site name.</param>                
        public static CategoryInfo GetCategoryInfo(string categoryName, string siteName)
        {
            return ProviderObject.GetCategoryInfoInternal(categoryName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified category.
        /// </summary>
        /// <param name="categoryObj">Category to be set.</param>
        public static void SetCategoryInfo(CategoryInfo categoryObj)
        {
            ProviderObject.SetInfo(categoryObj);
        }


        /// <summary>
        /// Deletes specified category.
        /// </summary>
        /// <param name="categoryObj">Category to be deleted.</param>
        public static void DeleteCategoryInfo(CategoryInfo categoryObj)
        {
            ProviderObject.DeleteInfo(categoryObj);
        }


        /// <summary>
        /// Deletes category with specified ID.
        /// </summary>
        /// <param name="categoryId">Category ID.</param>
        public static void DeleteCategoryInfo(int categoryId)
        {
            CategoryInfo categoryObj = GetCategoryInfo(categoryId);
            DeleteCategoryInfo(categoryObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all categories for specified site matching the specified parameters.
        /// </summary>       
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        /// <param name="siteId">Site ID.</param>            
        public static ObjectQuery<CategoryInfo> GetCategories(string where, string orderBy, int topN, string columns, int siteId)
        {
            where = SqlHelper.AddWhereCondition(where, ProviderObject.GetSiteWhereCondition(siteId), "AND");

            return GetCategories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }


        /// <summary>
        /// Returns a query for all subcategories for specified site matching the specified parameters.
        /// </summary>
        /// <param name="categoryId">ID of the category to get child categories for.</param>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        /// <param name="siteId">Site ID.</param>
        public static ObjectQuery<CategoryInfo> GetChildCategories(int categoryId, string where = null, string orderBy = null, int topN = 0, string columns = null, int siteId = 0)
        {
            // Add category parent condition
            where = SqlHelper.AddWhereCondition(where, "ISNULL(CategoryParentID, 0) = " + categoryId);

            return GetCategories(where, orderBy, topN, columns, siteId);
        }


        /// <summary>
        /// Returns dataset of all categories base on complete where condition from TreeProvider.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by condition.</param>
        /// <param name="topN">Top N condition.</param>
        public static TypedDataSet GetDocumentCategories(string where = null, string orderBy = null, int topN = 0)
        {
            return ProviderObject.GetDocumentCategoriesInternal(where, orderBy, topN);
        }


        /// <summary>
        /// Gets count of the documents related to the specified category.
        /// </summary>
        /// <param name="categoryId">ID of the category.</param>
        public static int GetCategoryCount(int categoryId)
        {
            return ProviderObject.GetCategoryCountInternal(categoryId);
        }


        /// <summary>
        /// Moves specified category up within the parent category.
        /// </summary>
        /// <param name="categoryId">ID of the category</param>
        public static void MoveCategoryUp(int categoryId)
        {
            ProviderObject.MoveCategoryUpInternal(categoryId);
        }


        /// <summary>
        /// Moves specified category down within the parent category.
        /// </summary>
        /// <param name="categoryId">ID of the category.</param>
        public static void MoveCategoryDown(int categoryId)
        {
            ProviderObject.MoveCategoryDownInternal(categoryId);
        }


        /// <summary>
        /// Returns maximum order value for given category siblings.
        /// </summary>
        /// <param name="category">Category to get max order for.</param>
        public static int GetLastCategoryOrder(CategoryInfo category)
        {
            return ProviderObject.GetLastCategoryOrderInternal(category);
        }


        /// <summary>
        /// Returns where condition usable for filtering documents by categories.
        /// </summary>
        /// <param name="categoriesIDPaths">Categories ID paths.</param>
        /// <param name="onlyEnabled">Indicates if only documents belonging to enabled (sub)categories are to be filtered out.</param>
        public static IWhereCondition GetCategoriesDocumentsWhereCondition(IEnumerable<string> categoriesIDPaths, bool onlyEnabled)
        {
            return ProviderObject.GetCategoriesDocumentsWhereConditionInternal(categoriesIDPaths, onlyEnabled);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns category with specified name.
        /// </summary>
        /// <param name="categoryName">Category name.</param>                
        /// <param name="siteName">Site name.</param>         
        protected virtual CategoryInfo GetCategoryInfoInternal(string categoryName, string siteName)
        {
            // Search for global categories if site categories not found
            bool searchGlobal = SettingsKeyInfoProvider.GetBoolValue("CMSAllowGlobalCategories", siteName);

            return GetInfoByCodeName(categoryName, SiteInfoProvider.GetSiteID(siteName), true, searchGlobal);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(CategoryInfo info)
        {
            if (info != null)
            {
                // Update
                if (info.CategoryID > 0)
                {
                    CategoryInfo oldInfo = GetInfoById(info.CategoryID, false);
                    if (oldInfo != null)
                    {
                        // Check if category was moved
                        bool parentIdChanged = oldInfo.CategoryParentID != info.CategoryParentID;

                        // Do not change the order when restoring a category from the CI repository
                        if (parentIdChanged && !RepositoryActionContext.CurrentIsRestoreOperationRunning)
                        {
                            // Place category at the end (will be updated after save)
                            info.CategoryOrder = GetLastCategoryOrderInternal(info) + 1;
                        }

                        if (info.ItemChanged("CategoryDisplayName") || parentIdChanged)
                        {
                            // Update name path
                            info.Generalized.UpdatePathColumn("CategoryParentID", "CategoryNamePath", "CategoryDisplayName", true, false);
                        }
                    }
                }
                // Insert
                else
                {
                    // Default values (not null columns)
                    info.CategoryIDPath = "";
                    info.CategoryLevel = 0;

                    // Get correct name path
                    info.Generalized.UpdatePathColumn("CategoryParentID", "CategoryNamePath", "CategoryDisplayName", false, false);

                    // Do not change the order when restoring a category from the CI repository
                    if (!RepositoryActionContext.CurrentIsRestoreOperationRunning)
                    {
                        // Get max category order
                        info.CategoryOrder = GetLastCategoryOrderInternal(info) + 1;
                    }

                    // Ensure code name for correct validation
                    info.Generalized.EnsureCodeName();
                }

                // Save changes
                base.SetInfo(info);

                // Paths were updated, flush hash tables
                ClearHashtables(true);

                // If category indexing is enabled create search task
                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, CategoryInfo.OBJECT_TYPE, "update", info.CategoryID.ToString(), info.CategoryID);
                }
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(CategoryInfo info)
        {
            base.DeleteInfo(info);

            // Child categories were removed from DB.
            ClearHashtables(true);

            // If category indexing is enabled create search task
            if (SearchIndexInfoProvider.SearchEnabled)
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, CategoryInfo.OBJECT_TYPE, "delete", info.CategoryID.ToString(), info.CategoryID);
            }
        }

        #endregion


        /// <summary>
        /// Gets count of the documents related to the specified category.
        /// </summary>
        /// <param name="categoryId">ID of the category.</param>
        protected virtual int GetCategoryCountInternal(int categoryId)
        {
            int count = 0;

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@CategoryID", categoryId);

            // Get the number of documents
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.documentcategory.documentsincategory", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                count = ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0);
            }
            return count;
        }


        /// <summary>
        /// Returns dataset of all categories base on complete where condition from TreeProvider.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by condition.</param>
        /// <param name="topN">Top N condition.</param>
        protected virtual TypedDataSet GetDocumentCategoriesInternal(string where, string orderBy, int topN)
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<CategoryInfo>();

            return ConnectionHelper.ExecuteQuery("cms.category.selectDocumentsCategories", parameters, where, orderBy, topN).As<CategoryInfo>();
        }


        /// <summary>
        /// Moves specified category up within the parent category.
        /// </summary>
        /// <param name="categoryId">ID of the category</param>
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


        #region "Internal methods - Advanced"

        /// <summary>
        /// Moves specified category down within the parent category.
        /// </summary>
        /// <param name="categoryId">ID of the category.</param>
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
        /// Returns maximum order value for given category siblings.
        /// </summary>
        /// <param name="category">Category to get max order for.</param>
        protected virtual int GetLastCategoryOrderInternal(CategoryInfo category)
        {
            // Get max category order
            string where = GetWhereCondition(category);
            DataSet ds = GetCategories().Where(where).OrderByDescending("CategoryOrder").Column("CategoryOrder AS MaxOrder").TopN(1);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Place category at the end of siblings
                return ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["MaxOrder"], 0);
            }

            return 0;
        }


        /// <summary>
        /// Returns where condition usable for filtering documents by categories.
        /// </summary>
        /// <param name="categoriesIDPaths">Categories ID paths.</param>
        /// <param name="onlyEnabled">Indicates if only documents belonging to enabled (sub)categories are to be filtered out.</param>
        protected virtual IWhereCondition GetCategoriesDocumentsWhereConditionInternal(IEnumerable<string> categoriesIDPaths, bool onlyEnabled)
        {
            var categoryWhere = new WhereCondition();
            var onlyEnabledWhere = new WhereCondition();

            foreach (string categoryIDPath in categoriesIDPaths)
            {
                categoryWhere = categoryWhere.Or().WhereStartsWith("CategoryIDPath", categoryIDPath);
            }

            if (onlyEnabled)
            {
                onlyEnabledWhere = onlyEnabledWhere
                    .WhereTrue("CategoryEnabled")
                    .WhereNotExists(
                        GetCategories().From(new QuerySourceTable("CMS_Category", "pc"))
                                       .Column("CategoryID")
                                       .WhereFalse("pc.CategoryEnabled")
#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                                       .WhereLike("CMS_Category.CategoryIDPath".AsColumn(), "pc.CategoryIDPath+'/%'".AsExpression())
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                    );
            }

            var documentWhere = new ObjectQuery(PredefinedObjectType.DOCUMENTCATEGORY)
                .Column("DocumentID")
                .WhereIn("CategoryID",
                    GetCategories().Column("CategoryID")
                                   .Where(categoryWhere)
                                   .Where(onlyEnabledWhere));

            return new WhereCondition().WhereIn("DocumentID", documentWhere);
        }

        #endregion


        #region "Private methods"

        private string GetSiteWhereCondition(int siteId)
        {
            var siteWhere = "";

            // Allow global objects if global objects are requested
            var allowGlobal = siteId <= 0;

            // Allow global objects according to the site settings
            var siteName = SiteInfoProvider.GetSiteName(siteId);
            if (!string.IsNullOrEmpty(siteName))
            {
                allowGlobal = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAllowGlobalCategories");
            }

            var siteIdcolumn = TypeInfo.SiteIDColumn;
            if (allowGlobal)
            {
                // Include global objects
                siteWhere = siteIdcolumn + " IS NULL";

                if (siteId > 0)
                {
                    // Include site objects
                    siteWhere += " OR " + siteIdcolumn + " = " + siteId;
                }
            }
            else if (siteId > 0)
            {
                // Only site objects
                siteWhere = siteIdcolumn + " = " + siteId;
            }

            return siteWhere;
        }


        /// <summary>
        /// Returns where condition usable for filtering categories of same type under the same parent.
        /// </summary>
        /// <param name="category">Category info</param>
        private static string GetWhereCondition(CategoryInfo category)
        {
            return string.Format("ISNULL(CategoryParentID, 0) = {0} AND (ISNULL(CategorySiteID, 0) = {1}) AND (ISNULL(CategoryUserID, 0) = {2})", category.CategoryParentID, category.CategorySiteID, category.CategoryUserID);
        }

        #endregion
    }
}