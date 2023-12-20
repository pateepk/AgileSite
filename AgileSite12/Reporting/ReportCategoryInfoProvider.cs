using System;

using CMS.DataEngine;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing ReportCategoryInfo management.
    /// </summary>
    public class ReportCategoryInfoProvider : AbstractInfoProvider<ReportCategoryInfo, ReportCategoryInfoProvider>, IRelatedObjectCountProvider
    {
        #region "Methods"

        /// <summary>
        /// Returns all report category records.
        /// </summary>
        public static ObjectQuery<ReportCategoryInfo> GetCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ReportCategoryInfo GetReportCategoryInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the ReportCategoryInfo structure for the specified reportCategory.
        /// </summary>
        /// <param name="reportCategoryId">ReportCategory id</param>
        public static ReportCategoryInfo GetReportCategoryInfo(int reportCategoryId)
        {
            return ProviderObject.GetInfoById(reportCategoryId);
        }


        /// <summary>
        /// Returns the ReportCategoryInfo structure for the specified reportCategory.
        /// </summary>
        /// <param name="categoryName">Category name</param>
        public static ReportCategoryInfo GetReportCategoryInfo(string categoryName)
        {
            return ProviderObject.GetInfoByCodeName(categoryName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified reportCategory.
        /// </summary>
        /// <param name="reportCategory">ReportCategory to set</param>
        public static void SetReportCategoryInfo(ReportCategoryInfo reportCategory)
        {
            ProviderObject.SetReportCategoryInfoInternal(reportCategory);
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
        public static void UpdateReportCategoryChildCount(int originalParentID, int newParentID)
        {
            ProviderObject.UpdateReportCategoryChildCountInternal(originalParentID, newParentID);
        }


        /// <summary>
        /// Deletes specified reportCategory.
        /// </summary>
        /// <param name="reportCategoryObj">ReportCategory object</param>
        public static void DeleteReportCategoryInfo(ReportCategoryInfo reportCategoryObj)
        {
            ProviderObject.DeleteReportCategoryInfoInternal(reportCategoryObj);
        }


        /// <summary>
        /// Deletes specified reportCategory.
        /// </summary>
        /// <param name="reportCategoryId">ReportCategory id</param>
        public static void DeleteReportCategoryInfo(int reportCategoryId)
        {
            ReportCategoryInfo reportCategoryObj = GetReportCategoryInfo(reportCategoryId);
            DeleteReportCategoryInfo(reportCategoryObj);
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
        /// Sets (updates or inserts) specified reportCategory.
        /// </summary>
        /// <param name="reportCategory">ReportCategory to set</param>
        protected void SetReportCategoryInfoInternal(ReportCategoryInfo reportCategory)
        {
            if (reportCategory != null)
            {
                // Ensure the code name
                reportCategory.Generalized.EnsureCodeName();

                if (reportCategory.CategoryID == 0)
                {
                    reportCategory.CategoryPath = "";
                    reportCategory.CategoryLevel = 0;
                }

                ReportCategoryInfo parent = null;
                ReportCategoryInfo originalParent = null;

                // Category is not root category
                if (reportCategory.CategoryParentID != 0)
                {
                    parent = GetReportCategoryInfo(reportCategory.CategoryParentID);
                }
                else
                {
                    if (reportCategory.CategoryCodeName != "/")
                    {
                        parent = GetReportCategoryInfo("/");
                        if (parent != null)
                        {
                            // Parent ID
                            reportCategory.CategoryParentID = parent.CategoryID;
                        }
                    }
                }

                if (reportCategory.CategoryID > 0)
                {
                    // Get object form Dabase because of parent category changing
                    originalParent = GetReportCategoryInfo(reportCategory.CategoryID);
                }

                // Store object in database
                SetInfo(reportCategory);

                if (parent != null)
                {
                    // Update parent category child count
                    UpdateCategoryChildCount((originalParent != null) ? originalParent.CategoryParentID : 0, reportCategory.CategoryParentID);
                }
            }
            else
            {
                throw new Exception("[WebPartCategoryInfoProvider.SetWebPartCategoryInfo]: No WebPartCategoryInfo object set.");
            }
        }


        /// <summary>
        /// Updates the child categories count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        protected void UpdateCategoryChildCountInternal(int originalParentID, int newParentID)
        {
            // Update child nodes count if parent changed
            if (newParentID != originalParentID)
            {
                // Update parent child nodes count
                QueryDataParameters parameters = null;

                // Original parent
                if (originalParentID > 0)
                {
                    parameters = new QueryDataParameters();
                    parameters.Add("@CategoryID", originalParentID);

                    ConnectionHelper.ExecuteQuery("reporting.ReportCategory.updatecategorychildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("reporting.ReportCategory.updatecategorychildcount", parameters);
            }
        }


        /// <summary>
        /// Updates the child webparts count of category.
        /// </summary>
        /// <param name="originalParentID">Original parent ID</param>
        /// <param name="newParentID">New parent ID</param>
        protected void UpdateReportCategoryChildCountInternal(int originalParentID, int newParentID)
        {
            // Update child nodes count if parent changed
            if (newParentID != originalParentID)
            {
                // Update parent child nodes count
                QueryDataParameters parameters = null;

                // Original parent
                if (originalParentID > 0)
                {
                    parameters = new QueryDataParameters();
                    parameters.Add("@CategoryID", originalParentID);

                    ConnectionHelper.ExecuteQuery("reporting.reportcategory.updatecategoryreportchildcount", parameters);
                }

                // New parent
                parameters = new QueryDataParameters();
                parameters.Add("@CategoryID", newParentID);

                ConnectionHelper.ExecuteQuery("reporting.reportcategory.updatecategoryreportchildcount", parameters);
            }
        }


        /// <summary>
        /// Deletes specified reportCategory.
        /// </summary>
        /// <param name="reportCategoryObj">ReportCategory object</param>
        protected void DeleteReportCategoryInfoInternal(ReportCategoryInfo reportCategoryObj)
        {
            if (reportCategoryObj != null)
            {
                int parentID = reportCategoryObj.CategoryParentID;

                // Remove category itself
                DeleteInfo(reportCategoryObj);

                UpdateCategoryChildCount(0, parentID);
            }
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        protected void RefreshDataCountsInternal()
        {
            ConnectionHelper.ExecuteQuery("Reporting.ReportCategory.refreshdatacount", null);
        }

        #endregion
    }
}