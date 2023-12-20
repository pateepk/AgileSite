using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing WidgetCategoryInfo management.
    /// </summary>
    public class WidgetCategoryInfoProvider : AbstractInfoProvider<WidgetCategoryInfo, WidgetCategoryInfoProvider>, IRelatedObjectCountProvider
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the WidgetCategoryInfo structure for the specified widget category.
        /// </summary>
        /// <param name="widgetCategoryId">Widget category id</param>
        public static WidgetCategoryInfo GetWidgetCategoryInfo(int widgetCategoryId)
        {
            return ProviderObject.GetInfoById(widgetCategoryId);
        }


        /// <summary>
        /// Returns the WidgetCategoryInfo structure for the specified widget category GUID.
        /// </summary>
        /// <param name="widgetGuid">Widget category Guid</param>
        /// <returns>Widget category info object</returns>
        public static WidgetCategoryInfo GetWidgetCategoryInfo(Guid widgetGuid)
        {
            return ProviderObject.GetInfoByGuid(widgetGuid);
        }


        /// <summary>
        /// Returns the WidgetCategoryInfo structure for the specified widget category name.
        /// </summary>
        /// <param name="categoryName">Widget category name</param>        
        public static WidgetCategoryInfo GetWidgetCategoryInfo(string categoryName)
        {
            return ProviderObject.GetInfoByCodeName(categoryName);
        }


        /// <summary>
        /// Returns all widget categories.
        /// </summary>
        public static ObjectQuery<WidgetCategoryInfo> GetWidgetCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified widget category.
        /// </summary>
        /// <param name="widgetCategory">Widget category to set</param>
        public static void SetWidgetCategoryInfo(WidgetCategoryInfo widgetCategory)
        {
            ProviderObject.SetInfo(widgetCategory);
        }


        /// <summary>
        /// Deletes specified widget category with dependencies.
        /// </summary>
        /// <param name="infoObj">Widget category object</param>
        public static void DeleteWidgetCategoryInfo(WidgetCategoryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified widget category with dependencies.
        /// </summary>
        /// <param name="widgetCategoryId">Widget category ID</param>
        public static void DeleteWidgetCategoryInfo(int widgetCategoryId)
        {
            WidgetCategoryInfo infoObj = GetWidgetCategoryInfo(widgetCategoryId);
            DeleteWidgetCategoryInfo(infoObj);
        }


        /// <summary>
        /// Updates the child categories count of widget category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        public static void UpdateCategoryChildCount(int originalParentID, int newParentID)
        {
            ProviderObject.UpdateCategoryChildCountInternal(originalParentID, newParentID);
        }


        /// <summary>
        /// Updates the child widget count of widget category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        public static void UpdateCategoryWidgetChildCount(int originalParentID, int newParentID)
        {
            ProviderObject.UpdateCategoryWidgetChildCountInternal(originalParentID, newParentID);
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
        protected override void SetInfo(WidgetCategoryInfo info)
        {
            if (info != null)
            {
                // Ensure the code name
                info.Generalized.EnsureCodeName();

                if (info.WidgetCategoryID == 0)
                {
                    info.WidgetCategoryPath = "";
                    info.WidgetCategoryLevel = 0;
                }

                WidgetCategoryInfo parent = null;
                int originalParent = 0;

                // Category is not root category
                if (info.WidgetCategoryParentID != 0)
                {
                    parent = GetWidgetCategoryInfo(info.WidgetCategoryParentID);
                }
                else
                {
                    if (info.WidgetCategoryName != "/")
                    {
                        parent = GetWidgetCategoryInfo("/");
                        if (parent != null)
                        {
                            info.WidgetCategoryParentID = parent.WidgetCategoryID;
                        }
                    }
                }

                // Update or insert object to database
                if (info.WidgetCategoryID > 0)
                {
                    // Get original object from database for updating original parent child count
                    WidgetCategoryInfo wci = GetWidgetCategoryInfo(info.WidgetCategoryID);
                    if (wci != null)
                    {
                        originalParent = wci.WidgetCategoryParentID;
                    }
                }

                base.SetInfo(info);

                if (parent != null)
                {
                    // Update parent category child count
                    UpdateCategoryChildCountInternal(originalParent, info.WidgetCategoryParentID);
                }
            }
            else
            {
                throw new Exception("[WidgetCategoryInfoProvider.SetWidgetCategoryInfo]: No WidgetCategoryInfo object set.");
            }
        }


        /// <summary>
        /// Updates the child categories count of widget category.
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

                    ConnectionHelper.ExecuteQuery("cms.widgetcategory.updatecategorychildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("cms.widgetcategory.updatecategorychildcount", parameters);
            }
        }


        /// <summary>
        /// Updates the child widget count of widget category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        protected virtual void UpdateCategoryWidgetChildCountInternal(int originalParentID, int newParentID)
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

                    ConnectionHelper.ExecuteQuery("cms.widgetcategory.updatecategorywidgetchildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("cms.widgetcategory.updatecategorywidgetchildcount", parameters);
            }
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        protected virtual void RefreshDataCountsInternal()
        {
            ConnectionHelper.ExecuteQuery("cms.widgetcategory.refreshdatacounts", null);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WidgetCategoryInfo info)
        {
            if (info != null)
            {
                // Set connection
                int parentID = info.WidgetCategoryParentID;

                // Get at least one subcategory
                DataSet dsSubcategories = GetWidgetCategories().WhereEquals("WidgetCategoryParentID", info.WidgetCategoryID).TopN(1).Columns("WidgetCategoryID");

                if (!DataHelper.DataSourceIsEmpty(dsSubcategories))
                {
                    // Can't be deleted with sub categories
                    throw new CheckDependenciesException(info, ResHelper.GetString("widgets.subcategoryerror"));
                }
                else
                {
                    // Delete object from database
                    base.DeleteInfo(info);

                    // Update parent category child count
                    UpdateCategoryChildCountInternal(0, parentID);
                }
            }
        }

        #endregion
    }
}