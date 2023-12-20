using System.Data;

using CMS.CMSImportExport;
using CMS.Helpers;

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