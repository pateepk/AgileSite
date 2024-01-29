using System;
using System.Data;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
            ImportExportEvents.GetImportData.After += GetImportData_After;

        }


        private static void GetImportData_After(object sender, ImportGetDataEventArgs e)
        {
            var settings = e.Settings;
            // Sort categories to preserve its order from older packages where TypeInfo was not correctly set (CM-4782)
            if (settings.IsLowerVersion("8.2", "46"))
            {
                var objectType = e.ObjectType;
                var selectionOnly = e.SelectionOnly;
                var data = e.Data;

                if (objectType.EqualsCSafe(CategoryInfo.OBJECT_TYPE, true) && !selectionOnly)
                {
                    // Sort categories
                    SortCategories(data, CategoryInfo.OBJECT_TYPE, CategoryInfo.TYPEINFO.ImportExportSettings.OrderBy);
                }

                if (objectType.EqualsCSafe(UserInfo.OBJECT_TYPE, true) && !selectionOnly)
                {
                    // Sort user's categories
                    SortCategories(data, CategoryInfo.OBJECT_TYPE, CategoryInfo.TYPEINFOUSERCATEGORY.ImportExportSettings.OrderBy);
                }
            }
        }


        private static void SortCategories(DataSet data, string objectType, string orderBy)
        {
            DataTable dataTable = null;
            if (!ImportExportHelper.GetDataTable(data, objectType, ref dataTable))
            {
                return;
            }

            DataHelper.SortDataTable(dataTable, orderBy);

            // Update table in data set
            data.Tables.Remove(dataTable.TableName);
            data.Tables.Add(dataTable.DefaultView.ToTable());
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var parameters = e.Parameters;
            var existing = parameters.ExistingObject;

            using (new ImportSpecialCaseContext(settings))
            {
                if (infoObj.TypeInfo.OriginalObjectType == CategoryInfo.OBJECT_TYPE)
                {
                    if (!parameters.SkipObjectUpdate && (existing != null))
                    {
                        var category = (CategoryInfo)infoObj;
                        var existingCategory = (CategoryInfo)existing;

                        // Keep existing hierarchy of categories
                        category.CategoryParentID = existingCategory.CategoryParentID;
                        category.CategoryIDPath = existingCategory.CategoryIDPath;
                        category.CategoryNamePath = existingCategory.CategoryNamePath;
                        category.CategoryLevel = existingCategory.CategoryLevel;
                        category.CategoryOrder = existingCategory.CategoryOrder;
                    }
                }
            }
        }

        #endregion
    }
}