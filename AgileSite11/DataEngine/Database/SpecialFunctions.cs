using System;
using System.Data;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides special functions for other classes.
    /// </summary>
    [Obsolete("Class was not intended for public use and will be removed in the next version.")]
    public static class SpecialFunctions
    {
        /// <summary>
        /// Converts source parameters to query data parameters with type convert (e.g.: "true" as boolean type etc.).
        /// </summary>
        /// <param name="sourceDataRows">Source array of DataRows</param>
        public static QueryDataParameters ConvertDataRowsToParams(params DataRow[] sourceDataRows)
        {
            QueryDataParameters parameters = new QueryDataParameters();

            foreach (DataRow dr in sourceDataRows)
            {
                if (dr != null)
                {
                    for (int i = 0; i < dr.Table.Columns.Count; i++)
                    {
                        parameters.Add(dr.Table.Columns[i].ColumnName, dr[i]);
                    }
                }
            }

            return parameters;
        }


        /// <summary>
        /// Copies data from the given DataClass to the given DataRow.
        /// </summary>
        /// <param name="dc">Source DataClass</param>
        /// <param name="dr">Target DataRow</param>
        public static void CopyDataToDataRow(IDataContainer dc, DataRow dr)
        {
            DataTable destTable = dr.Table;
            foreach (string column in dc.ColumnNames)
            {
                // Find the column in destination
                int colIndex = destTable.Columns.IndexOf(column);
                if (colIndex >= 0)
                {
                    dr[colIndex] = dc.GetValue(column);
                }
            }
        }


        /// <summary>
        /// Copies changed data for the changed columns from the given DataClass to the given DataRow.
        /// </summary>
        /// <param name="dc">Source DataClass</param>
        /// <param name="dr">Target DataRow</param>
        public static void CopyChangedDataToDataRow(IAdvancedDataContainer dc, DataRow dr)
        {
            // If not changed, do not copy anything
            if (!dc.HasChanged)
            {
                return;
            }

            DataTable destTable = dr.Table;
            foreach (string column in dc.ColumnNames)
            {
                if (dc.ItemChanged(column))
                {
                    // Find the column in destination
                    int colIndex = destTable.Columns.IndexOf(column);
                    if (colIndex >= 0)
                    {
                        dr[colIndex] = dc.GetValue(column);
                    }
                }
            }
        }


        /// <summary>
        /// Copies original data for the changed columns from the given DataClass to the given DataRow.
        /// </summary>
        /// <param name="dc">Source DataClass</param>
        /// <param name="dr">Target DataRow</param>
        public static void CopyOriginalDataToDataRow(IAdvancedDataContainer dc, DataRow dr)
        {
            // If not changed, do not copy anything
            if (!dc.HasChanged)
            {
                return;
            }

            DataTable destTable = dr.Table;
            foreach (string column in dc.ColumnNames)
            {
                if (dc.ItemChanged(column))
                {
                    // Find the column in destination
                    int colIndex = destTable.Columns.IndexOf(column);
                    if (colIndex >= 0)
                    {
                        dr[colIndex] = dc.GetOriginalValue(column);
                    }
                }
            }
        }
    }
}