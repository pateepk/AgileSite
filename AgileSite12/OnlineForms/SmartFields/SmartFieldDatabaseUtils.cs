using System.Data;
using System.Linq;

using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Database helper methods for smart fields feature.
    /// </summary>
    internal static class SmartFieldDatabaseUtils
    {
        /// <summary>
        /// Ensures unique filtered non-clustered index on FormContactGUID column for given <paramref name="tableName"/>.
        /// </summary>
        internal static void EnsureContactColumnIndex(string tableName)
        {
            if (!SmartFieldLicenseHelper.HasLicense())
            {
                return;
            }

            var tableManager = new TableManager(null);

            var indexNames = tableManager.GetIndexes(tableName).Tables[0].Rows.OfType<DataRow>().Select(i => i[0]);
            if (indexNames.Contains(GetIndexName(tableName)))
            {
                return;
            }

            tableManager.CreateColumnIndexes(tableName, SmartFieldConstants.CONTACT_COLUMN_NAME, GetIndexDefinitionDataSet(tableName));
        }


        private static DataSet GetIndexDefinitionDataSet(string tableName)
        {
            var table = new DataTable();

            table.Columns.Add("IndexName", typeof(string));
            table.Columns.Add("ColumnName", typeof(string));
            table.Columns.Add("IsDescending", typeof(bool));
            table.Columns.Add("IsStatistics", typeof(bool));
            table.Columns.Add("IsUnique", typeof(bool));
            table.Columns.Add("IsClustered", typeof(bool));
            table.Columns.Add("FilterDefinition", typeof(string));
            table.Columns.Add("ColumnIsIncluded", typeof(bool));

            var dataSet = new DataSet();
            dataSet.Tables.Add(table);

            table.Rows.Add(GetIndexName(tableName), SmartFieldConstants.CONTACT_COLUMN_NAME, false, false, true, false, $"([{SmartFieldConstants.CONTACT_COLUMN_NAME}] IS NOT NULL)", false);

            return dataSet;
        }


        private static string GetIndexName(string tableName)
        {
            return $"IX_{tableName}_{SmartFieldConstants.CONTACT_COLUMN_NAME}";
        }
    }
}
