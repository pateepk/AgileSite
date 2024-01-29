using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Methods to work with the Data.
    /// </summary>
    public class DataHelper : CoreMethods
    {
        #region "Constants"

        // Size constants
        private const int SizeByteLimit = 1024; // 1 kB
        private const int SizeKiloByteLimit = 1048576; // 1 MB
        private const int SizeMegaByteLimit = 1073741824; // 1 GB
        private const int DoubleSizeLimit = 20;

        private const string CULTURE_WITH_COMMA_DECIMAL_SEPARATOR = "cs-CZ";
        private const string CULTURE_WITH_DOT_DECIMAL_SEPARATOR = "en-US";

        /// <summary>
        /// Fake ID to use when the ID column requires a value, and needs to be faked
        /// </summary>
        public const int FAKE_ID = int.MaxValue;

        #endregion


        #region "Variables and constants"

        /// <summary>
        /// Action callback.
        /// </summary>
        /// <param name="parameters">Action parameters</param>
        public delegate void ActionCallback(object[] parameters);

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the data container able to read the given data object
        /// </summary>
        /// <param name="data">Data</param>
        public static ISimpleDataContainer GetDataContainer(object data)
        {
            if (data == null)
            {
                return null;
            }

            // Do not convert if already data container
            var dc = data as ISimpleDataContainer;
            if (dc != null)
            {
                return dc;
            }

            var dr = data as DataRow;
            if (dr != null)
            {
                return new DataRowContainer(dr);
            }
            else
            {
                var drv = data as DataRowView;
                if (drv != null)
                {
                    return new DataRowContainer(drv.Row);
                }
            }

            throw new NotSupportedException("[DataHelper.GetDataContainer]: Object of type '" + data.GetType().Name + "' cannot be converted to data container.");
        }


        /// <summary>
        /// Returns data from datarow, datarowview or ISimpleDataContainer objects for specified column.
        /// </summary>
        /// <param name="data">Data container</param>
        /// <param name="columnName">Column name</param>
        public static object GetDataContainerItem(object data, string columnName)
        {
            // Check whether data object exist and column is specified
            if ((data == null) || String.IsNullOrEmpty(columnName))
            {
                return null;
            }

            // Data container
            var container = data as ISimpleDataContainer;
            if (container != null)
            {
                return container.GetValue(columnName);
            }

            // try retype data as datarow view
            DataRowView drv = data as DataRowView;
            DataRow dr;

            // If datarowview object exists, return column data
            if (drv != null)
            {
                if (drv.Row.Table.Columns.Contains(columnName))
                {
                    return drv[columnName];
                }

                return null;
            }

            // Try retype data as datarow and if datarow object exist, return column data
            if ((dr = data as DataRow) != null)
            {
                if (dr.Table.Columns.Contains(columnName))
                {
                    return dr[columnName];
                }

                return null;
            }

            // return null if data can't be retype
            return null;
        }


        /// <summary>
        /// Returns the parent path for the specified path (any kind of path with "/" as a separator)
        /// </summary>
        /// <param name="path">Original path</param>
        public static string GetParentPath(string path)
        {
            if (path == null)
            {
                return null;
            }

            int lastSeparator = path.LastIndexOfCSafe("/");
            if (lastSeparator > 0)
            {
                return path.Substring(0, lastSeparator);
            }

            return "/";
        }


        /// <summary>
        /// Gets the items count in the specified data source.
        /// </summary>
        /// <param name="dataSource">Data source</param>
        public static int GetItemsCount(object dataSource)
        {
            // Get original datasource from grouped datasource
            if (dataSource is GroupedDataSource)
            {
                dataSource = ((GroupedDataSource)dataSource).DataSource;
            }

            if ((dataSource == null) || (dataSource == DBNull.Value))
            {
                return 0;
            }

            // Dataset
            var ds = dataSource as DataSet;
            if (ds != null)
            {
                int tablesCount = ds.Tables.Count;
                if (tablesCount == 1)
                {
                    // Single table - most often
                    return ds.Tables[0].Rows.Count;
                }

                if (tablesCount == 0)
                {
                    // No tables
                    return 0;
                }

                // Check all tables
                int count = 0;

                foreach (DataTable dt in ds.Tables)
                {
                    count += dt.Rows.Count;
                }

                return count;
            }

            // Data table
            var table = dataSource as DataTable;
            if (table != null)
            {
                return table.Rows.Count;
            }

            var collection = dataSource as ICollection;
            if (collection != null)
            {
                return collection.Count;
            }

            // IQueryable will use our CMSQueryProvider execute implementation, that should be optimal.
            var query = dataSource as IQueryable;
            if (query != null)
            {
                return query.Cast<object>().Count();
            }

            // As a last resort, iterate over enumerable to count it.
            var enumerable = dataSource as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.Cast<object>().Count();
            }

            // Return by default 1, for not known object
            return 1;
        }


        /// <summary>
        /// Returns true if the given data source is empty.
        /// </summary>
        /// <param name="dataSource">Data source to check for emptiness</param>
        public static bool DataSourceIsEmpty(DataSet dataSource)
        {
            if (dataSource == null)
            {
                return true;
            }

            int count = dataSource.Tables.Count;
            if (count == 1)
            {
                // Typical DataSet - Most optimized
                return (dataSource.Tables[0].Rows.Count <= 0);
            }
            else if (count <= 0)
            {
                return true;
            }
            else
            {
                // Check all tables for emptiness
                foreach (DataTable dt in dataSource.Tables)
                {
                    if (dt.Rows.Count > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true if the given data source is empty.
        /// </summary>
        /// <param name="dataSource">Data source to check for emptiness</param>
        public static bool DataSourceIsEmpty(object dataSource)
        {
            return (GetItemsCount(dataSource) <= 0);
        }


        /// <summary>
        /// Returns the value from the DataRow field.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returning the value</param>
        /// <returns>Returns true if the value (field) is not found</returns>
        public static bool TryGetDataRowValue(DataRow dr, string columnName, out object value)
        {
            // If no row present, return null
            if (dr == null)
            {
                value = null;
                return false;
            }
            else
            {
                // Get column index
                int colIndex = dr.Table.Columns.IndexOf(columnName);
                if (colIndex < 0)
                {
                    value = null;
                    return false;
                }
                else
                {
                    value = dr[colIndex];
                    return true;
                }
            }
        }


        /// <summary>
        /// Returns the value from the DataRow field, or DBNull.Value if the field does not exist in the datarow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        public static object GetDataRowValue(DataRow dr, string columnName)
        {
            // If no row present, return null
            if (dr == null)
            {
                return DBNull.Value;
            }
            else
            {
                // Get column index
                int colIndex = dr.Table.Columns.IndexOf(columnName);
                if (colIndex < 0)
                {
                    return DBNull.Value;
                }
                else
                {
                    return dr[colIndex];
                }
            }
        }


        /// <summary>
        /// Returns the value from the DataRowView field.
        /// </summary>
        /// <param name="dr">DataRowView with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returning the value</param>
        /// <returns>Returns true if the value (field) is not found</returns>
        public static bool TryGetDataRowViewValue(DataRowView dr, string columnName, out object value)
        {
            if (dr == null)
            {
                value = DBNull.Value;
                return false;
            }
            else
            {
                return TryGetDataRowValue(dr.Row, columnName, out value);
            }
        }


        /// <summary>
        /// Returns the value from the DataRowView field, or null if the field does not exist in the datarow.
        /// </summary>
        /// <param name="dr">DataRowView with the data</param>
        /// <param name="columnName">Column name</param>
        public static object GetDataRowViewValue(DataRowView dr, string columnName)
        {
            if (dr == null)
            {
                return DBNull.Value;
            }
            else
            {
                return GetDataRowValue(dr.Row, columnName);
            }
        }


        /// <summary>
        /// Sets value to data row. If value is null, DBNull will be used.
        /// </summary>
        /// <param name="dr">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public static bool SetDataRowValue(DataRow dr, string columnName, object value)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }

            // Get column index
            int colIndex = dr.Table.Columns.IndexOf(columnName);
            if (colIndex >= 0)
            {
                Type type = dr.Table.Columns[columnName].DataType;

                // Insert directly value for string type
                if (type == typeof(string))
                {
                    dr[colIndex] = value;
                    return true;
                }

                // Insert null for null value
                if (value == DBNull.Value)
                {
                    dr[colIndex] = DBNull.Value;
                    return true;
                }

                // Convert value to target type
                object result = null;
                if (ConvertValue(value, type, ref result))
                {
                    dr[colIndex] = result;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns a DataSet containing single table with single column.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="columnType">Column data type</param>
        public static DataSet GetSingleColumnDataSet(string tableName, string columnName, Type columnType)
        {
            DataSet ds = new DataSet();

            // Create a table
            DataTable dt = new DataTable(tableName);
            dt.Columns.Add(columnName, columnType);

            ds.Tables.Add(dt);

            return ds;
        }


        /// <summary>
        /// Gets DataSet from the given XML string.
        /// </summary>
        /// <param name="xmlData">XML data</param>
        public static DataSet GetDataSetFromXml(string xmlData)
        {
            // If no data given, return null
            if ((xmlData == null) || (xmlData.Trim() == ""))
            {
                return null;
            }

            // Read the XML data
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                ds.TryReadXml(xmlData);

                return ds;
            }
            catch
            {
                if (ds != null)
                {
                    ds.Dispose();
                }
                throw;
            }
        }


        /// <summary>
        /// Returns true, if the given value is empty (null, DBNull, or "").
        /// </summary>
        public static bool IsEmpty(object value)
        {
            return ((value == null) || (value == DBNull.Value) || (value.ToString() == ""));
        }


        /// <summary>
        /// Returns second parameter if the first is null or "".
        /// </summary>
        public static string GetNotEmpty(object value, string defaultValue)
        {
            if (IsEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                return value.ToString();
            }
        }


        /// <summary>
        /// Converts the value to a specific type.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="type">Target type</param>
        public static object ConvertValue(object value, Type type)
        {
            object result = null;
            ConvertValue(value, type, ref result);
            return result;
        }


        /// <summary>
        /// Converts the value to a specific type. Returns true if conversion was possible, otherwise false.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="type">Target type</param>
        /// <param name="result">Result of the conversion</param>
        public static bool ConvertValue(object value, Type type, ref object result)
        {
            if (type == typeof(double))
            {
                double resultDouble = 0;
                if (ValidationHelper.LoadDoubleSystem(ref resultDouble, value))
                {
                    result = resultDouble;
                    return true;
                }
            }
            else if (type == typeof(decimal))
            {
                decimal resultDecimal = 0;
                if (ValidationHelper.LoadDecimalSystem(ref resultDecimal, value))
                {
                    result = resultDecimal;
                    return true;
                }
            }
            else if (type == typeof(DateTime))
            {
                DateTime resultDateTime = DateTime.Now;
                if (ValidationHelper.LoadDateTimeSystem(ref resultDateTime, value))
                {
                    result = resultDateTime;
                    return true;
                }
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpan resultTimeSpan = TimeSpan.Zero;
                if (ValidationHelper.LoadTimeSpanSystem(ref resultTimeSpan, value))
                {
                    result = resultTimeSpan;
                    return true;
                }
            }
            else if (type == typeof(Guid))
            {
                Guid resultGuid = Guid.Empty;
                if (ValidationHelper.LoadGuid(ref resultGuid, value))
                {
                    result = resultGuid;
                    return true;
                }
            }
            if (type == typeof(int))
            {
                int resultInteger = 0;
                if (ValidationHelper.LoadInteger(ref resultInteger, value))
                {
                    result = resultInteger;
                    return true;
                }
            }
            if (type == typeof(long))
            {
                long resultLong = 0;
                if (ValidationHelper.LoadLong(ref resultLong, value))
                {
                    result = resultLong;
                    return true;
                }
            }
            else if (type == typeof(bool))
            {
                bool resultBool = false;
                if (ValidationHelper.LoadBoolean(ref resultBool, value))
                {
                    result = resultBool;
                    return true;
                }
            }
            result = value;
            return false;
        }



        /// <summary>
        /// Converts value to its equivalent string representation using default culture format. Can be used to convert Double, Decimal, DateTime or TimeSpan values.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="type">Type of converted value</param>
        public static String ConvertValueToDefaultCulture(String value, Type type)
        {
            if (type == typeof(double))
            {
                return ValidationHelper.GetDouble(value, 0.0).ToString(CultureHelper.EnglishCulture);
            }
            else if (type == typeof(decimal))
            {
                return ValidationHelper.GetDecimal(value, 0.0M).ToString(CultureHelper.EnglishCulture);
            }
            else if (type == typeof(DateTime))
            {
                return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME).ToString(CultureHelper.EnglishCulture);
            }
            else if (type == typeof(TimeSpan))
            {
                return ValidationHelper.GetTimeSpan(value, TimeSpan.MinValue).ToString("c", CultureHelper.EnglishCulture);
            }

            return value;
        }


        /// <summary>
        /// Returns true if the given value is a valid ID
        /// </summary>
        /// <param name="id">ID to check</param>
        public static bool IsValidID(int id)
        {
            return (id > 0) && (id != FAKE_ID);
        }


        /// <summary>
        /// Gets the first value from the given DataSet.
        /// </summary>
        /// <param name="ds">DataSet with the data</param>
        public static object GetScalarValue(DataSet ds)
        {
            if (!DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0][0];
            }

            return null;
        }


        /// <summary>
        /// Returns true, if the given value is zero (null, DBNull, or 0).
        /// </summary>
        public static bool IsZero(object value)
        {
            if ((value == null) || (value == DBNull.Value))
            {
                return true;
            }
            else
            {
                return ValidationHelper.GetInteger(value, 0) == 0;
            }
        }


        /// <summary>
        /// Returns the object converted to integer or default value when the result of conversion equals zero.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value in case result is zero</param>
        public static int GetNotZero(object value, int defaultValue)
        {
            if (IsZero(value))
            {
                return defaultValue;
            }
            else
            {
                return ValidationHelper.GetInteger(value, defaultValue);
            }
        }


        /// <summary>
        /// Sorts the Data table using given OrderBy expression.
        /// </summary>
        /// <param name="dt">Table to sort</param>
        /// <param name="orderBy">Order by expression</param>
        public static void SortDataTable(DataTable dt, string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                return;
            }

            string[] cols = orderBy.Split(',');
            string newOrderBy = "";

            // Process all columns
            foreach (string col in cols)
            {
                string expr = col.Trim();

                string order = "";
                string colName = expr;

                // Parse the column name
                if (expr.EndsWithCSafe(" DESC", true))
                {
                    colName = expr.Substring(0, expr.Length - 5).Trim();
                    order = " DESC";
                }
                else if (expr.EndsWithCSafe(" ASC", true))
                {
                    colName = expr.Substring(0, expr.Length - 4).Trim();
                    order = " ASC";
                }

                // Special case - newid()
                if (colName.ToLowerCSafe() == "newid()")
                {
                    // Create new ID column
                    colName = CreateUniqueTableColumn(dt, "newid", typeof(Guid));
                    EnsureGUIDs(dt, colName);
                }

                // Add column sort expression if sorting column exists in data table
                if (dt.Columns.Contains(colName) || (dt.Columns.Contains(colName.TrimStart('[').TrimEnd(']'))))
                {
                    if (newOrderBy != "")
                    {
                        newOrderBy += ", ";
                    }
                    newOrderBy += colName + order;
                }
            }

            if (newOrderBy != "")
            {
                dt.DefaultView.Sort = newOrderBy;
            }
        }


        /// <summary>
        /// Creates column with unique name int he given table.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="type">Column data type</param>
        public static string CreateUniqueTableColumn(DataTable dt, string columnName, Type type)
        {
            if (dt == null)
            {
                return null;
            }

            // Get unique column name
            string newColumnName = columnName;

            int index = 0;

            while (dt.Columns.Contains(columnName))
            {
                index++;

                newColumnName = columnName + index.ToString();
            }

            // Add new column
            dt.Columns.Add(new DataColumn(newColumnName, type));

            return newColumnName;
        }


        /// <summary>
        /// Returns the DataView from the given data source.
        /// </summary>
        /// <param name="dataSource">Data source</param>
        public static DataView GetDataView(object dataSource)
        {
            // If null, return null
            if (dataSource == null)
            {
                return null;
            }

            if (dataSource is DataView)
            {
                return (DataView)dataSource;
            }
            else if (dataSource is DataTable)
            {
                return ((DataTable)dataSource).DefaultView;
            }
            else if ((dataSource is DataSet) && (((DataSet)dataSource).Tables.Count > 0))
            {
                return ((DataSet)dataSource).Tables[0].DefaultView;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Returns the DataTable from the given data source.
        /// </summary>
        /// <param name="dataSource">Data source</param>
        public static DataTable GetDataTable(object dataSource)
        {
            if (dataSource == null)
            {
                return null;
            }

            if (dataSource is DataRow)
            {
                return ((DataRow)dataSource).Table;
            }

            if (dataSource is DataRowView)
            {
                return ((DataRowView)dataSource).DataView.Table;
            }

            if ((dataSource is DataSet) && (((DataSet)dataSource).Tables.Count > 0))
            {
                return ((DataSet)dataSource).Tables[0];
            }

            if (dataSource is DataView)
            {
                return ((DataView)dataSource).Table;
            }

            if (dataSource is DataTable)
            {
                return (DataTable)dataSource;
            }

            return null;
        }


        /// <summary>
        /// Returns the string for the specified file size.
        /// </summary>
        /// <param name="size">File size in bytes</param>    
        /// <param name="unit">Unit in which you want the result</param>
        public static string GetSizeString(long size, FileSizeUnitsEnum unit = FileSizeUnitsEnum.Automatic)
        {
            double fSize = size;

            // If automatic select best unit
            if (unit == FileSizeUnitsEnum.Automatic)
            {
                if (size >= SizeMegaByteLimit)
                {
                    unit = FileSizeUnitsEnum.GB;
                }
                else if (size >= SizeKiloByteLimit)
                {
                    unit = FileSizeUnitsEnum.MB;
                }
                else if (size >= SizeByteLimit)
                {
                    unit = FileSizeUnitsEnum.kB;
                }
            }

            // Create output
            switch (unit)
            {
                // Kilobytes
                case FileSizeUnitsEnum.kB:
                    fSize = fSize / 1024;
                    if (fSize < DoubleSizeLimit)
                    {
                        return fSize.ToString("#.#") + " kB";
                    }

                    return Math.Round(fSize, MidpointRounding.AwayFromZero).ToString("#") + " kB";

                // Megabytes
                case FileSizeUnitsEnum.MB:
                    fSize = fSize / (1024 * 1024);
                    if (fSize < DoubleSizeLimit)
                    {
                        return fSize.ToString("#.#") + " MB";
                    }

                    return Math.Round(fSize, MidpointRounding.AwayFromZero).ToString("#") + " MB";

                // Gigabytes
                case FileSizeUnitsEnum.GB:
                    fSize = fSize / (1024 * 1024 * 1024);
                    if (fSize < DoubleSizeLimit)
                    {
                        return fSize.ToString("#.#") + " GB";
                    }

                    return Math.Round(fSize, MidpointRounding.AwayFromZero).ToString("#") + " GB";

                // Bytes
                default:
                    return fSize + " B";
            }
        }


        /// <summary>
        /// Compares two byte arrays.
        /// </summary>
        /// <param name="data1">First array</param>
        /// <param name="data2">Second array</param>
        public static bool CompareByteArrays(byte[] data1, byte[] data2)
        {
            // If both are null, they're equal
            if ((data1 == null) && (data2 == null))
            {
                return true;
            }
            // If either but not both are null, they're not equal
            if ((data1 == null) || (data2 == null))
            {
                return false;
            }
            // Check length
            if (data1.Length != data2.Length)
            {
                return false;
            }

            // Check content
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Merges two data tables into one
        /// </summary>
        /// <param name="dt">Source data table</param>
        /// <param name="destDT">Destination data table</param>
        /// <param name="handleUnique">If true, unique values are processed to maintain only one such record in the result</param>
        private static void MergeTables(DataTable dt, DataTable destDT, bool handleUnique)
        {
            // Find unique column
            if (handleUnique)
            {
                string columnName = null;

                foreach (DataColumn dc in destDT.Columns)
                {
                    if (dc.Unique)
                    {
                        columnName = dc.ColumnName;
                        break;
                    }
                }

                if ((columnName != null) && dt.Columns.Contains(columnName) && (dt.Rows.Count > 0))
                {
                    // Collect unique values
                    var existingItems = new Hashtable();
                    foreach (DataRow dr in destDT.Rows)
                    {
                        object value = dr[columnName];
                        existingItems[value] = true;
                    }

                    // Identify duplicate rows
                    var duplicateRows = new List<DataRow>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        object value = dr[columnName];
                        if (existingItems[value] != null)
                        {
                            duplicateRows.Add(dr);
                        }
                        else
                        {
                            existingItems[value] = true;
                        }
                    }

                    // Remove duplicate rows
                    foreach (DataRow dr in duplicateRows)
                    {
                        dt.Rows.Remove(dr);
                    }

                    dt.AcceptChanges();
                }
            }

            destDT.Merge(dt);
        }


        /// <summary>
        /// Transfers table rows between two tables.
        /// </summary>
        /// <param name="destinationDT">Destination table</param>
        /// <param name="sourceDT">Source table</param>
        /// <param name="where">Where condition for rows</param>
        /// <param name="orderBy">Order BY</param>
        public static DataTable TransferTableRows(DataTable destinationDT, DataTable sourceDT, string where, string orderBy)
        {
            if (DataSourceIsEmpty(sourceDT))
            {
                return destinationDT;
            }

            // Create destination table if not specified
            if (destinationDT == null)
            {
                destinationDT = sourceDT.Clone();
            }

            // Process the rows
            DataRow[] rows = sourceDT.Select(@where, orderBy);

            foreach (DataRow dr in rows)
            {
                destinationDT.ImportRow(dr);
                sourceDT.Rows.Remove(dr);
            }

            sourceDT.AcceptChanges();

            return destinationDT;
        }


        /// <summary>
        /// Sets the table name in the specified DataSet.
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="tableName">Table name</param>
        public static void SetTableName(DataSet ds, string tableName)
        {
            if ((ds != null) && (ds.Tables.Count > 0))
            {
                ds.Tables[0].TableName = tableName;
            }
        }


        /// <summary>
        /// Renames the table in the given DataSet.
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="tableName">Table name</param>
        /// <param name="newTableName">New table name</param>
        public static DataTable RenameTable(DataSet ds, string tableName, string newTableName)
        {
            if (ds == null)
            {
                return null;
            }

            DataTable dt = ds.Tables[tableName];
            if (dt != null)
            {
                dt.TableName = newTableName;
            }

            return dt;
        }


        /// <summary>
        /// Deletes the table rows matching the given where condition.
        /// </summary>
        /// <param name="ds">DataSet with the data</param>
        /// <param name="where">Where condition</param>
        public static void DeleteRows(DataSet ds, string where)
        {
            if ((ds == null) || String.IsNullOrEmpty(@where))
            {
                return;
            }

            // Process all tables
            foreach (DataTable dt in ds.Tables)
            {
                DeleteRows(dt, @where);
            }
        }


        /// <summary>
        /// Keeps only table rows matching the given where condition.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="where">Where condition</param>
        public static void KeepOnlyRows(DataTable dt, string where)
        {
            if ((dt == null) || String.IsNullOrEmpty(where))
            {
                return;
            }

            // Get the rows to keep
            DataRow[] rows = dt.Select(where);

            var deleteRows = dt.Rows.Cast<DataRow>().Except(rows).ToList();

            // Delete the rows
            foreach (DataRow dr in deleteRows)
            {
                dt.Rows.Remove(dr);
            }
        }


        /// <summary>
        /// Deletes the table rows matching the given where condition.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="where">Where condition</param>
        public static void DeleteRows(DataTable dt, string where)
        {
            if ((dt == null) || String.IsNullOrEmpty(where))
            {
                return;
            }

            // Get the rows to delete
            DataRow[] rows = dt.Select(where);

            // Delete the rows
            foreach (DataRow dr in rows)
            {
                dt.Rows.Remove(dr);
            }
        }


        /// <summary>
        /// Converts the table names of given DataSet to lowercase representation.
        /// </summary>
        /// <param name="ds">DataSet to convert</param>
        public static void LowerCaseTableNames(DataSet ds)
        {
            if (ds != null)
            {
                foreach (DataTable dt in ds.Tables)
                {
                    dt.TableName = dt.TableName.ToLowerCSafe();
                }
            }
        }


        /// <summary>
        /// Ensures GUID values in specified column.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="columnName">Column name</param>
        public static void EnsureGUIDs(DataTable dt, string columnName)
        {
            if (dt == null)
            {
                return;
            }

            // Ensure the column
            int colIndex = EnsureColumn(dt, columnName, typeof(Guid));

            // Process all rows
            foreach (DataRow dr in dt.Rows)
            {
                // Ensure GUID
                object value = dr[colIndex];
                if ((value == null) || (value == DBNull.Value))
                {
                    dr[colIndex] = Guid.NewGuid();
                }
            }
        }


        /// <summary>
        /// Sets column values to specified value.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="column">Column to set</param>
        /// <param name="value">Value to set</param>
        public static void SetColumnValues(DataTable dt, string column, object value)
        {
            if (dt.Columns.Contains(column))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dr[column] = value;
                }
            }
        }


        /// <summary>
        /// Ensures specified column.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="columnType">Column type</param>
        public static int EnsureColumn(DataTable dt, string columnName, Type columnType)
        {
            if (dt == null)
            {
                return -1;
            }

            // Ensure the column
            int colIndex = dt.Columns.IndexOf(columnName);
            if (colIndex < 0)
            {
                colIndex = dt.Columns.Count;
                dt.Columns.Add(new DataColumn(columnName, columnType));
            }
            return colIndex;
        }


        /// <summary>
        /// Changes the value in the given table to NULL.
        /// </summary>
        /// <param name="dt">Table to process</param>
        /// <param name="columnName">Column name</param>
        /// <param name="where">Additional where condition</param>
        public static void ChangeValuesToNull(DataTable dt, string columnName, string where)
        {
            ChangeValues(dt, columnName, null, DBNull.Value, @where);
        }


        /// <summary>
        /// Changes the string value in the given table.
        /// </summary>
        /// <param name="dt">Table to process</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <param name="where">Additional where condition</param>
        public static void ChangeStringValues(DataTable dt, string columnName, string value, string where)
        {
            ChangeValues(dt, columnName, null, value, @where);
        }


        /// <summary>
        /// Changes the value in the given table.
        /// </summary>
        /// <param name="dt">Table to process</param>
        /// <param name="columnName">Column name</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <param name="where">Additional where condition</param>
        private static void ChangeValues(DataTable dt, string columnName, object oldValue, object newValue, string where)
        {
            if (dt == null)
            {
                return;
            }

            // Get column index
            int colIndex = dt.Columns.IndexOf(columnName);
            if (colIndex < 0)
            {
                return;
            }

            // Get the rows
            if (oldValue != null)
            {
                if (oldValue is string)
                {
                    where = AddWhereCondition(where, columnName + " = '" + oldValue + "'");
                }
                else
                {
                    where = AddWhereCondition(where, columnName + " = " + oldValue);
                }
            }

            DataRow[] rows = dt.Select(where);

            // Set the values
            foreach (DataRow dr in rows)
            {
                dr[colIndex] = newValue;
            }
        }


        /// <summary>
        /// Adds where condition to the expression.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="condition">Condition to add</param>
        private static string AddWhereCondition(string where, string condition)
        {
            // If condition present, add
            if (!String.IsNullOrEmpty(condition))
            {
                // Add and if previous condition not empty
                if (!String.IsNullOrEmpty(@where))
                {
                    if (@where != condition)
                    {
                        @where = "(" + @where + ") AND (" + condition + ")";
                    }
                }
                else
                {
                    @where = condition;
                }
            }

            return @where;
        }


        /// <summary>
        /// Converts data set to it's pilot.
        /// </summary>
        /// <param name="sourceDs">Source data set</param>
        /// <param name="parameters">Name of new data set columns</param>
        public static DataSet DataSetPivot(DataSet sourceDs, string[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add();
            if ((sourceDs == null) || (sourceDs.Tables.Count == 0) || (sourceDs.Tables[0].Rows.Count == 0))
            {
                return null;
            }
            DataTable sourceDt = sourceDs.Tables[0];

            // Create columns for new dataset
            string firstColumnName = "Column1";

            if (parameters.Length > 0)
            {
                firstColumnName = parameters[0];
            }
            dt.Columns.Add(firstColumnName);

            for (int i = 0; i < sourceDt.Rows.Count; i++)
            {
                string columnName = "Column" + (i + 1);

                if (parameters.Length > (i + 1))
                {
                    columnName = parameters[i + 1];
                }

                dt.Columns.Add(columnName);
            }

            // Fill the rows
            for (int i = 0; i < sourceDt.Columns.Count; i++)
            {
                DataRow dr = dt.NewRow();
                DataColumn column = sourceDt.Columns[i];
                dr[0] = column.Caption;

                for (int k = 0; k < sourceDt.Rows.Count; k++)
                {
                    dr[k + 1] = sourceDt.Rows[k].ItemArray[i].ToString();
                }
                dt.Rows.Add(dr);
            }

            return ds;
        }


        /// <summary>
        /// Changes the boolean value in the given table.
        /// </summary>
        /// <param name="dt">Table to process</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <param name="where">Additional where condition</param>
        public static void ChangeBooleanValues(DataTable dt, string columnName, bool value, string where)
        {
            ChangeValues(dt, columnName, null, value, @where);
        }


        /// <summary>
        /// Changes the boolean value in the given table.
        /// </summary>
        /// <param name="dt">Table to process</param>
        /// <param name="columnName">Column name</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <param name="where">Additional where condition</param>
        public static void ChangeBooleanValues(DataTable dt, string columnName, bool oldValue, bool newValue, string where)
        {
            ChangeValues(dt, columnName, oldValue, newValue, @where);
        }


        /// <summary>
        /// Extends the parameters array for new items.
        /// </summary>
        /// <param name="parameters">Original array</param>
        /// <param name="newItems">New items count</param>
        public static object[] ExtendParameters(object[] parameters, int newItems)
        {
            // For empty parameters return only empty array for new ones
            if (parameters == null)
            {
                return new object[newItems];
            }

            int baseLength = parameters.Length;
            object[] newParameters = new object[baseLength + newItems];

            // Original parameters
            for (int i = 0; i < baseLength; i++)
            {
                newParameters[i] = parameters[i];
            }

            return newParameters;
        }


        /// <summary>
        /// Reads the DataSet from given XML reader (expects "NewDataSet" as a root node).
        /// </summary>
        /// <param name="ds">DataSet to read</param>
        /// <param name="xml">XML to read</param>
        /// <param name="rowCallback">Callback action which should be called after each row is loaded. The DataSet, DataTable a DataRow are added as additional parameters</param>
        /// <param name="rootNode">Name of the root node to check (if null not check is done)</param>
        /// <param name="parameters">Callback parameters</param>
        public static DataSet ReadDataSetFromXml(DataSet ds, XmlReader xml, ActionCallback rowCallback, object[] parameters, string rootNode = "NewDataSet")
        {
            List<string> columns;
            return ReadDataSetFromXml(ds, xml, rowCallback, parameters, rootNode, out columns);
        }


        /// <summary>
        /// Reads the DataSet from given XML reader.
        /// </summary>
        /// <param name="ds">DataSet to read</param>
        /// <param name="xml">XML to read</param>
        /// <param name="rowCallback">Callback action which should be called after each row is loaded. The DataSet, DataTable a DataRow are added as additional parameters</param>
        /// <param name="parameters">Callback parameters</param>
        /// <param name="rootNode">Name of the root node to check (if null not check is done)</param>
        /// <param name="updatedColumns">List of columns present in the xml (not all columns from DataSet have to be also in containing xml), this list is needed for update mode in CMSHierarchyHelper</param>
        /// <param name="cultureName">Name of the culture to use for parsing double, decimal and datetime values. English culture is used if not specified</param>
        public static DataSet ReadDataSetFromXml(DataSet ds, XmlReader xml, ActionCallback rowCallback, object[] parameters, string rootNode, out List<string> updatedColumns, string cultureName = null)
        {
            bool checkRoot = (rootNode != null);

            updatedColumns = new List<string>();

            // Read get first element
            while ((xml.NodeType != XmlNodeType.Element) && !xml.EOF)
            {
                xml.Read();
            }

            if (xml.EOF)
            {
                throw new InvalidOperationException("Unexpected end of file.");
            }

            // Check the start of DataSet
            if (checkRoot && !xml.Name.Equals(rootNode, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($" <{rootNode}> expected but <{xml.Name}> found.");
            }

            bool onlyTable = false;

            if (ds == null)
            {
                ds = new DataSet();
            }
            else
            {
                // Check if xml represents only one table or whole dataset
                foreach (DataTable table in ds.Tables)
                {
                    if (table.TableName.Equals(xml.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        onlyTable = true;
                        break;
                    }
                }
            }

            // Prepare the parameters for callback
            object[] newParameters = ExtendParameters(parameters, 3);
            int indexDS = newParameters.Length - 3;
            int indexDT = newParameters.Length - 2;
            int indexDR = newParameters.Length - 1;

            newParameters[indexDS] = ds;

            // Go inside the DataSet
            if (!onlyTable)
            {
                xml.Read();
            }

            // Load the data
            while (!xml.EOF)
            {
                // Get next DataRow
                while ((xml.NodeType != XmlNodeType.Element) && (xml.NodeType != XmlNodeType.EndElement) && !xml.EOF)
                {
                    xml.Read();
                    if (xml.EOF)
                    {
                        throw new InvalidOperationException("Unexpected end of file.");
                    }
                }

                // End element - end of DataSet or error
                if (xml.NodeType == XmlNodeType.EndElement)
                {
                    if (!checkRoot || xml.Name.Equals(rootNode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // End of Dataset
                        xml.Read();
                        break;
                    }
                    else
                    {
                        throw new InvalidOperationException($"</{rootNode}> expected but </{xml.Name}> found.");
                    }
                }

                // Ensure the table
                string tableName = xml.Name;
                DataTable dt = ds.Tables[tableName];
                if (dt == null)
                {
                    dt = new DataTable();
                    dt.TableName = tableName;
                    ds.Tables.Add(dt);
                }
                newParameters[indexDT] = dt;

                // Create new row
                DataRow dr = dt.NewRow();
                newParameters[indexDR] = dr;

                xml.Read();

                // Read the data
                while (!xml.EOF)
                {
                    // Get next data item
                    while ((xml.NodeType != XmlNodeType.Element) && (xml.NodeType != XmlNodeType.EndElement) && !xml.EOF)
                    {
                        xml.Read();
                        if (xml.EOF)
                        {
                            throw new InvalidOperationException("Unexpected end of file.");
                        }
                    }

                    // End element - end of DataRow or error
                    if (xml.NodeType == XmlNodeType.EndElement)
                    {
                        if (xml.Name.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            dt.Rows.Add(dr);

                            // End of DataRow
                            rowCallback?.Invoke(newParameters);

                            xml.Read();
                            break;
                        }

                        throw new InvalidOperationException($"</{tableName}> expected but <{xml.Name}> found.");
                    }

                    // Ensure the column
                    string columnName = xml.Name;

                    var dc = EnsureTableColumn(dt, columnName, ds);

                    columnName = columnName.ToLowerInvariant();

                    // Add column to updated columns list
                    if (!updatedColumns.Contains(columnName))
                    {
                        updatedColumns.Add(columnName);
                    }

                    // Read the column content
                    if (dc.DataType == typeof(byte[]))
                    {
                        if (!xml.IsEmptyElement)
                        {
                            // Binary data
                            xml.ReadStartElement();

                            // Read content
                            dr[columnName] = XmlHelper.ReadContentAsBase64(xml, 0);

                            xml.ReadEndElement();
                        }
                        else
                        {
                            // Empty binary element is an empty array, if it was null, it would not be there at all
                            dr[columnName] = new byte[0];

                            // Binary data
                            xml.ReadStartElement();
                        }
                    }
                    else
                    {
                        // Other data types
                        Type type = dc.DataType;
                        string content = String.Empty;

                        if (!xml.IsEmptyElement)
                        {
                            xml.ReadStartElement();
                            content = xml.ReadContentAsString();
                            xml.ReadEndElement();
                        }
                        else
                        {
                            xml.ReadStartElement();
                        }

                        dr[columnName] = ParseXmlValue(content, type, columnName, cultureName);
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Ensures that the given table column is present. Creates the column if not. Returns the column object.
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="ds">Source data set where column definition is sought to get proper data type if not defined in table <paramref name="dt"/></param>
        private static DataColumn EnsureTableColumn(DataTable dt, string columnName, DataSet ds)
        {
            var dc = dt.Columns[columnName];
            if (dc == null)
            {
                var dataType = typeof(string);

                // Try to get column's data type from other tables
                foreach (var table in ds.Tables.Cast<DataTable>().Where(t => !t.TableName.Equals(dt.TableName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var column = table.Columns[columnName];
                    if (column != null)
                    {
                        dataType = column.DataType;
                        break;
                    }
                }

                int index = EnsureColumn(dt, columnName, dataType);
                dc = dt.Columns[index];
            }

            return dc;
        }


        /// <summary>
        /// Parses the XML value according to its type.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="type">Object type of the converting column</param>
        /// <param name="columnName">Name of the converting column</param>
        /// <param name="cultureName">Culture to use for parsing double, decimal and datetime values. English culture is used if not specified</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/></exception>
        public static object ParseXmlValue(string value, Type type, string columnName, string cultureName = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var cultureInfo = String.IsNullOrEmpty(cultureName)
                ? CultureHelper.EnglishCulture 
                : CultureHelper.GetCultureInfo(cultureName); 

            if (value.Equals("##null##", StringComparison.OrdinalIgnoreCase))
            {
                return DBNull.Value;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Double:
                    return ParseDoubleXmlValue(value, cultureInfo, columnName);

                case TypeCode.Decimal:
                    return ParseDecimalXmlValue(value, cultureInfo, columnName);

                case TypeCode.DateTime:
                    return ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME, cultureInfo);

                case TypeCode.Boolean:
                    return ValidationHelper.GetBoolean(value, false);

                case TypeCode.Int64:
                    return ValidationHelper.GetLong(value, 0);

                case TypeCode.Int32:
                    return ValidationHelper.GetInteger(value, 0);

                case TypeCode.Object:
                    if (type == typeof(Guid))
                    {
                        if (value == "")
                        {
                            return DBNull.Value;
                        }
                        return ValidationHelper.GetGuid(value, Guid.Empty);
                    }

                    if (type == typeof(TimeSpan))
                    {
                        return ValidationHelper.GetTimeSpan(value, TimeSpan.Zero, cultureInfo);
                    }
                    break;
            }

            return value;
        }


        private static double ParseDoubleXmlValue(string value, CultureInfo culture, string columnName)
        {
            if (IsEmpty(value))
            {
                return 0d;
            }

            double result;
            if (!double.TryParse(value, NumberStyles.Number | NumberStyles.AllowExponent, culture, out result) || double.IsInfinity(result) || double.IsNaN(result))
            {
                throw new FormatException($"The double value is in incorrect format. XML content: <{columnName}>{value}</{columnName}>");
            }

            return result;
        }


        private static decimal ParseDecimalXmlValue(string value, CultureInfo culture, string columnName)
        {
            if (IsEmpty(value))
            {
                return 0m;
            }

            decimal result;
            if (!decimal.TryParse(value, NumberStyles.Number, culture, out result))
            {
                throw new FormatException($"The decimal value is in incorrect format. XML content: <{columnName}>{value}</{columnName}>");
            }

            return result;
        }


        /// <summary>
        /// Writes the DataSet from given XML reader.
        /// </summary>
        /// <param name="ds">DataSet to read</param>
        /// <param name="xml">XML to read</param>
        /// <param name="filePathColumn">Column name with the file path to the binary file</param>
        /// <param name="rowCallback">Callback action which should be called before each row is written. The DataSet, DataTable a DataRow are added as additional parameters</param>
        /// <param name="parameters">Callback parameters</param>
        public static void WriteDataSetToXml(DataSet ds, XmlWriter xml, string filePathColumn, ActionCallback rowCallback, object[] parameters)
        {
            if (ds == null)
            {
                return;
            }

            // Prepare the parameters for callback
            object[] newParameters = ExtendParameters(parameters, 3);

            int indexDS = newParameters.Length - 3;
            int indexDR = newParameters.Length - 1;

            newParameters[indexDS] = ds;

            // Write the DataSet
            xml.WriteStartElement("NewDataSet");

            foreach (DataTable dt in ds.Tables)
            {
                string tableName = dt.TableName;

                // Write the table rows
                foreach (DataRow dr in dt.Rows)
                {
                    // Row callback
                    if (rowCallback != null)
                    {
                        newParameters[indexDR] = dr;
                        rowCallback(newParameters);
                    }

                    xml.WriteStartElement(tableName);

                    // Write the columns
                    int index = 0;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // Write value only if not null
                        object value = dr[index];
                        if (value != DBNull.Value)
                        {
                            string columnName = dc.ColumnName;

                            xml.WriteStartElement(columnName);

                            // Write value
                            if (dc.DataType == typeof(byte[]))
                            {
                                // Write binary data
                                byte[] data = (byte[])value;
                                xml.WriteBase64(data, 0, data.Length);
                            }
                            else if (columnName.EqualsCSafe(filePathColumn, true))
                            {
                                string filePath = ValidationHelper.GetString(value, "");
                                if (!String.IsNullOrEmpty(filePath))
                                {
                                    // Stream from file
                                    XmlHelper.WriteFileAsBase64(xml, filePath);
                                }
                            }
                            else
                            {
                                // Write other values
                                xml.WriteValue(value);
                            }

                            xml.WriteEndElement();
                        }

                        index++;
                    }

                    xml.WriteEndElement();
                }
            }

            xml.WriteEndElement();
        }


        /// <summary>
        /// Gets the size of the given data container.
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="coveredItems">Table of the covered items</param>
        private static int GetDataContainerSize(IDataContainer container, Hashtable coveredItems = null)
        {
            int totalSize = 0;

            // Ensure the covered items
            if (coveredItems == null)
            {
                coveredItems = new Hashtable();
            }

            foreach (string column in container.ColumnNames)
            {
                // Add the value size
                try
                {
                    object value = container.GetValue(column);
                    if ((value != null) && !coveredItems.Contains(value))
                    {
                        totalSize += GetObjectSize(value, coveredItems);
                    }
                }
                catch
                {
                    // Do not log the error if size cannot be retrieved
                }
            }

            return totalSize;
        }


        /// <summary>
        /// Gets the size of the given object.
        /// </summary>
        /// <param name="item">Object</param>
        public static int GetObjectSize(object item)
        {
            return GetObjectSize(item, null);
        }


        /// <summary>
        /// Gets the size of the given object.
        /// </summary>
        /// <param name="item">Object</param>
        /// <param name="coveredItems">Table of the covered items</param>
        private static int GetObjectSize(object item, Hashtable coveredItems)
        {
            int result = 0;
            if ((item == null) || (item == DBNull.Value))
            {
                // NULL
                result = 4;
            }
            else if (item is string)
            {
                // String
                result = ((string)item).Length;
            }
            else if (item is int)
            {
                // Integer
                result = sizeof(int);
            }
            else if (item is bool)
            {
                // Boolean
                result = sizeof(bool);
            }
            else if (item is DateTime)
            {
                // DateTime
                result = 8;
            }
            else if (item is double)
            {
                // Double
                result = sizeof(double);
            }
            else if (item is decimal)
            {
                // Decimal
                result = sizeof(decimal);
            }
            else if (item is byte[])
            {
                // Binary data
                result = ((byte[])item).Length;
            }
            else if (item is object[])
            {
                // Array of objects
                foreach (object obj in (object[])item)
                {
                    result += GetObjectSize(obj);
                }
            }
            else if (item is DataTable)
            {
                // Data table
                result = GetTableSize((DataTable)item);
            }
            else if (item is DataView)
            {
                // Data view
                result = GetViewSize((DataView)item);
            }
            else if (item is DataRow)
            {
                // Data row
                result = GetRowSize((DataRow)item);
            }
            else if (item is IDataContainer)
            {
                // Data container
                result = GetDataContainerSize((IDataContainer)item, coveredItems);
            }
            else
            {
                // Other data
                result = ValidationHelper.GetString(item, "").Length;
            }

            // Add to covered items
            if ((coveredItems != null) && (item != null))
            {
                coveredItems[item] = true;
            }

            return result;
        }


        /// <summary>
        /// Returns the row data size (size of the contained data).
        /// </summary>
        /// <param name="dr">Data row</param>
        public static int GetRowSize(DataRow dr)
        {
            if (dr == null)
            {
                return 0;
            }

            int result = 0;

            // Count all items
            foreach (object item in dr.ItemArray)
            {
                result += GetObjectSize(item);
            }

            return result;
        }


        /// <summary>
        /// Returns the table data size (size of the contained data).
        /// </summary>
        /// <param name="dt">Data table</param>
        public static int GetTableSize(DataTable dt)
        {
            if (dt == null)
            {
                return 0;
            }

            int result = 0;
            int index = 0;

            // Count all rows
            foreach (DataRow dr in dt.Rows)
            {
                result += GetRowSize(dr);

                if (index++ >= 1000)
                {
                    result = (int)(result / 1000.0 * dt.Rows.Count);
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the view data size (size of the contained data).
        /// </summary>
        /// <param name="dv">Data view</param>
        public static int GetViewSize(DataView dv)
        {
            if (dv == null)
            {
                return 0;
            }

            int result = 0;

            // Count all rows
            IEnumerator iterator = dv.GetEnumerator();
            while (iterator.MoveNext())
            {
                DataRowView drv = (DataRowView)iterator.Current;
                result += GetRowSize(drv.Row);
            }

            return result;
        }


        /// <summary>
        /// Converts the standard null value to DBNull.value, keeps the object value if not null.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static object GetDBNull(object value)
        {
            return value ?? DBNull.Value;
        }


        /// <summary>
        /// Converts the DBNull.value to standard null, keeps the object value if not null.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static object GetNull(object value)
        {
            if (value == DBNull.Value)
            {
                value = null;
            }
            return value;
        }


        /// <summary>
        /// Merges two objects implementing the IDictionary interface of type T with key of type K and value of type V.
        /// </summary>
        /// <typeparam name="T">Type of the object to merge</typeparam>
        /// <typeparam name="K">Type of the key</typeparam>
        /// <typeparam name="V">Type of the value</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="updateValue">Indicates if value of item with same key in obj1 should be overwritten</param>
        public static T Merge<T, K, V>(T obj1, T obj2, bool updateValue)
            where T : class, IDictionary<K, V>
        {
            // There is nothing to be merged
            if (obj1 == null)
            {
                return obj2;
            }

            if (obj2 == null)
            {
                return obj1;
            }

            foreach (K key in obj2.Keys)
            {
                // Item with key is present
                if (obj1.ContainsKey(key))
                {
                    // Value should be updated
                    if (updateValue)
                    {
                        obj1[key] = obj2[key];
                    }
                }
                else
                {
                    obj1.Add(key, obj2[key]);
                }
            }

            return obj1;
        }


        /// <summary>
        /// Gets the object as a string (for structured objects the object type and its size).
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="maxLength">Maximum length of the value</param>
        public static string GetObjectString(object value, int maxLength = 500)
        {
            int size;

            return GetObjectString(value, maxLength, out size);
        }


        /// <summary>
        /// Gets the object as a string (for structured objects the object type and its size).
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="maxLength">Maximum length of the value</param>
        /// <param name="size">Returns the size</param>
        public static string GetObjectString(object value, int maxLength, out int size)
        {
            string stringValue;
            size = 0;

            if (value is string)
            {
                // String value
                string str = (string)value;
                size = str.Length;
                stringValue = "\"" + TextHelper.LimitLength(str, maxLength) + "\" (" + GetSizeString(size) + ")";
            }
            else if (value is DataSet)
            {
                // DataSet - listing of table names and sizes
                DataSet ds = (DataSet)value;

                stringValue = "DataSet: ";
                int index = 0;

                foreach (DataTable table in ds.Tables)
                {
                    if (table.Rows.Count > 0)
                    {
                        if (index > 0)
                        {
                            stringValue += ", ";
                        }

                        int tableSize = GetTableSize(table);
                        stringValue += table.TableName + " (" + table.Rows.Count + " [" + table.Columns.Count + "], " + GetSizeString(tableSize) + ")";

                        size += tableSize;
                    }

                    index++;
                }
            }
            else if (value is DataTable)
            {
                // Data table
                DataTable table = (DataTable)value;
                size = GetTableSize(table);
                stringValue = "DataTable: " + table.TableName + " (" + table.Rows.Count + " [" + table.Columns.Count + "], " + GetSizeString(size) + ")";
            }
            else if (value is DataView)
            {
                // Data view
                DataView view = (DataView)value;
                size = GetViewSize(view);

                // Count all rows
                int count = 0;
                IEnumerator iterator = view.GetEnumerator();
                while (iterator.MoveNext())
                {
                    count++;
                }

                stringValue = "DataView: " + view.Table.TableName + " (" + count + " [" + view.Table.Columns.Count + "], " + GetSizeString(size) + ")";
            }
            else if (value is DataRow)
            {
                // Data row
                DataRow row = (DataRow)value;
                size = GetRowSize(row);
                stringValue = "DataRow: [" + row.Table.Columns.Count + "], " + GetSizeString(size);
            }
            else if (value is DataRowView)
            {
                // Data row view
                DataRowView row = (DataRowView)value;
                size = GetRowSize(row.Row);
                stringValue = "DataRowView: [" + row.Row.Table.Columns.Count + "], " + GetSizeString(size);
            }
            else if (value is byte[])
            {
                // Byte array
                byte[] array = (byte[])value;
                size = array.Length;
                stringValue = "System.Byte[" + GetSizeString(size) + "]";
            }
            else if (value is IDataContainer)
            {
                // Data container
                IDataContainer container = (IDataContainer)value;
                size = GetDataContainerSize(container);
                stringValue = value + " (" + GetSizeString(size) + ")";
            }
            else if (value is object[])
            {
                // Object array
                object[] array = (object[])value;
                if (array.Length <= 3)
                {
                    stringValue = "System.Object[" + array.Length + "] { ";
                    int index = 0;

                    foreach (object obj in array)
                    {
                        if (index > 0)
                        {
                            stringValue += ", ";
                        }

                        int objectSize;
                        stringValue += GetObjectString(obj, maxLength, out objectSize);
                        size += objectSize;

                        index++;
                    }

                    stringValue += " }";
                }
                else
                {
                    size = GetObjectSize(array);
                    stringValue = "System.Object[" + array.Length + "] (" + GetSizeString(size) + ")";
                }
            }
            else
            {
                // Convert directly to the string
                stringValue = ((value == null) ? "null" : TextHelper.LimitLength(value.ToString(), maxLength));
            }

            return stringValue;
        }


        /// <summary>
        /// Gets the list of new items in the given list as list separated by semicolon.
        /// </summary>
        /// <param name="oldList">Old list of items separated by separator</param>
        /// <param name="newList">New list of items separated by separator</param>
        public static string GetNewItemsInList(string oldList, string newList)
        {
            return GetNewItemsInList(oldList, newList, ';');
        }


        /// <summary>
        /// Gets the list of new items in the given list as list separated by separator.
        /// </summary>
        /// <param name="oldList">Old list of items separated by separator</param>
        /// <param name="newList">New list of items separated by separator</param>
        /// <param name="separator">Separator</param>
        public static string GetNewItemsInList(string oldList, string newList, char separator)
        {
            if (String.IsNullOrEmpty(oldList))
            {
                return newList;
            }

            if (String.IsNullOrEmpty(newList))
            {
                return null;
            }

            string[] values = oldList.Split(separator);

            // Build hashtable of current values
            Hashtable ht = new Hashtable(values.Length);
            foreach (string value in values)
            {
                ht[value] = true;
            }

            // Go through all values
            string[] newValues = newList.Split(separator);
            if (newValues.Length > 0)
            {
                // Build the selected values string
                StringBuilder sb = new StringBuilder(newValues.Length * 4);

                foreach (string value in newValues)
                {
                    // If not present on the old list, add to the list of new values
                    if (!String.IsNullOrEmpty(value) && !ht.Contains(value))
                    {
                        sb.Append(value, separator);
                    }
                }

                if (sb.Length > 1)
                {
                    return sb.ToString(0, sb.Length - 1);
                }
            }

            return "";
        }


        /// <summary>
        /// Gets the maximum value from the given column.
        /// </summary>
        /// <param name="dt">Data table with the data</param>
        /// <param name="column">Column name</param>
        public static T GetMaximumValue<T>(DataTable dt, string column)
            where T : IComparable
        {
            lock (dt)
            {
                return GetMaximumValue<T>(dt.DefaultView, column);
            }
        }


        /// <summary>
        /// Gets the maximum value from the given column.
        /// </summary>
        /// <param name="dv">Data table with the data</param>
        /// <param name="column">Column name</param>
        public static T GetMaximumValue<T>(DataView dv, string column)
            where T : IComparable
        {
            T maxValue = default(T);
            bool first = true;

            int colIndex = dv.Table.Columns.IndexOf(column);
            if (colIndex >= 0)
            {
                // Go through all rows
                int count = dv.Count;
                for (int i = 0; i < count; i++)
                {
                    DataRowView dr = dv[i];

                    object value = dr[colIndex];
                    if (value != DBNull.Value)
                    {
                        T typedValue = (T)value;
                        if (first)
                        {
                            // Assign first value to the max value
                            maxValue = typedValue;
                            first = false;
                        }
                        else if (typedValue.CompareTo(maxValue) > 0)
                        {
                            // Found larger value
                            maxValue = typedValue;
                        }
                    }
                }
            }

            return maxValue;
        }


        /// <summary>
        /// Marks the duplicate rows in the data table.
        /// </summary>
        /// <param name="dt">Table with the data</param>
        /// <param name="condition">Checks only rows matching the condition</param>
        /// <param name="resultColumn">Name of the column with the result</param>
        /// <param name="columns">Columns to check for duplicity</param>
        public static int MarkDuplicitRows(DataTable dt, string condition, string resultColumn, params string[] columns)
        {
            int duplicities = 0;

            var existingRows = new SafeDictionary<string, DataRowView>();
            var sb = new StringBuilder();

            if (!dt.Columns.Contains(resultColumn))
            {
                dt.Columns.Add(resultColumn, typeof(bool));
            }

            // Make a data view
            DataView dv = new DataView(dt);
            if (!String.IsNullOrEmpty(condition))
            {
                dv.RowFilter = condition;
            }

            // Process all rows
            int count = dv.Count;
            for (int i = 0; i < count; i++)
            {
                DataRowView dr = dv[i];

                // Build the key
                sb.Length = 0;
                foreach (string column in columns)
                {
                    sb.Append("|");
                    sb.Append(dr[column]);
                }
                string key = sb.ToString();

                // Check duplicity
                var existing = existingRows[key];
                if (existing != null)
                {
                    existing[resultColumn] = true;
                    dr[resultColumn] = true;

                    duplicities++;
                }
                else
                {
                    dr[resultColumn] = false;
                }

                // Register current row
                existingRows[key] = dr;
            }

            return duplicities;
        }


        /// <summary>
        /// Gets the minimum value from the given column.
        /// </summary>
        /// <param name="dt">Data table with the data</param>
        /// <param name="column">Column name</param>
        public static T GetMinimumValue<T>(DataTable dt, string column) where T : IComparable
        {
            T maxValue = default(T);
            bool first = true;

            int colIndex = dt.Columns.IndexOf(column);
            if (colIndex >= 0)
            {
                // Go through all rows
                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    object value = dr[colIndex];
                    if (value != DBNull.Value)
                    {
                        T typedValue = (T)value;
                        if (first)
                        {
                            // Assign first value to the max value
                            maxValue = typedValue;
                            first = false;
                        }
                        else if (typedValue.CompareTo(maxValue) < 0)
                        {
                            // Found larger value
                            maxValue = typedValue;
                        }
                    }
                }
            }

            return maxValue;
        }


        /// <summary>
        /// Gets the list of unique values based on given column value.
        /// </summary>
        /// <param name="dt">Table with the data</param>
        /// <param name="column">Column name</param>
        /// <param name="caseSensitive">Case sensitive evaluation?</param>
        public static List<string> GetUniqueValues(DataTable dt, string column, bool caseSensitive)
        {
            var result = new List<string>();
            var found = new Hashtable();

            int colIndex = dt.Columns.IndexOf(column);
            if (colIndex >= 0)
            {
                // Go through all rows
                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    string value = ValidationHelper.GetString(dr[colIndex], null);
                    if (!String.IsNullOrEmpty(value))
                    {
                        // Make case insensitive
                        if (!caseSensitive)
                        {
                            value = value.ToLowerCSafe();
                        }

                        if (!found.Contains(value))
                        {
                            // Add to the result
                            result.Add(value);
                            found.Add(value, true);
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the list of unique rows based on given column value.
        /// </summary>
        /// <param name="dt">Table with the data</param>
        /// <param name="idColumn">ID column name</param>
        public static List<DataRow> GetUniqueRows(DataTable dt, string idColumn)
        {
            var result = new List<DataRow>();
            var found = new ArrayList();

            int colIndex = dt.Columns.IndexOf(idColumn);
            if (colIndex >= 0)
            {
                // Go through all rows
                int count = dt.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    string id = ValidationHelper.GetString(dr[colIndex], null);
                    if (!String.IsNullOrEmpty(id))
                    {
                        if (!found.Contains(id))
                        {
                            // Add to the result
                            result.Add(dr);
                            found.Add(id);
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets value from string that has values separated by separator.
        /// </summary>
        /// <param name="values">String values</param>
        /// <param name="separator">Separator (for example: ';')</param>
        /// <param name="index">Index in split string array</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetPartialValue(string values, string separator, int index, string defaultValue)
        {
            if (!String.IsNullOrEmpty(values))
            {
                // Split input string
                string[] splits = values.Split(new[] { separator }, StringSplitOptions.None);

                // Get the value
                if ((index >= 0) && (index < splits.Length))
                {
                    return splits[index];
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Sets new value to string that is separates by separator.
        /// </summary>
        /// <param name="values">String with values</param>
        /// <param name="newValue">New value</param>
        /// <param name="separator">Separator (for example: ';')</param>
        /// <param name="index">Index in split string array</param>
        /// <returns>String with new value</returns>
        public static string SetPartialValue(string values, string newValue, string separator, int index)
        {
            if (!String.IsNullOrEmpty(values))
            {
                // Split input string
                string[] splits = values.Split(new[] { separator }, StringSplitOptions.None);

                if ((index >= 0) && (index < splits.Length))
                {
                    // Replace new value
                    splits[index] = newValue;
                    // Join substrings separated by separator
                    values = String.Join(separator, splits);
                    return values;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("index", index, "[DataHelper.SetPartialValue]: Index was out of range.");
                }
            }

            return values;
        }


        /// <summary>
        /// Reduces the data set data.
        /// Works in two modes:
        ///  1 - remove records (when calculateAverage is FALSE)
        ///  2 - calculate average values for the intervals (when calculateAverage is TRUE)
        /// </summary>
        /// <param name="ds">The input dataset</param>
        /// <param name="keepEveryXItem">The keep every x-item</param>
        /// <param name="calculateAverage">if true: calculate average value for the interval. If false: pick every X-record</param>
        /// <param name="columnNames">Calculate average values just for given columns (Column1;Column2..). If null, calculate for all columns of types integer and double.</param>
        public static DataSet ReduceDataSetData(DataSet ds, int keepEveryXItem, bool calculateAverage, string columnNames)
        {
            if (keepEveryXItem < 2)
            {
                return ds;
            }

            DataSet filteredDataSet = new DataSet();

            if (!DataSourceIsEmpty(ds))
            {
                filteredDataSet.Tables.Add(ds.Tables[0].Clone());
                int counter = 0;

                // Simple mode - remove rows
                if (!calculateAverage)
                {
                    foreach (DataRow currentRow in ds.Tables[0].Rows)
                    {
                        counter++;

                        if (counter == keepEveryXItem)
                        {
                            filteredDataSet.Tables[0].ImportRow(currentRow);
                            counter = 0;
                        }
                    }
                }
                // Advanced mode - calculate average values
                else
                {
                    // Store column indexes with a flag indicating whether the column is double (true) or int (false)
                    List<KeyValuePair<int, bool>> processCollumnIndexes = new List<KeyValuePair<int, bool>>();
                    string[] columnsNamesArray = null;

                    if (!String.IsNullOrEmpty(columnNames))
                    {
                        columnNames = columnNames.ToLowerCSafe();
                        columnsNamesArray = columnNames.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    // Get all columns which can be averaged
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        if ((columnsNamesArray != null)
                            && (!columnsNamesArray.Contains(column.ColumnName.ToLowerCSafe())))
                        {
                            break;
                        }

                        // Process only int + double values
                        switch (column.DataType.Name)
                        {
                            case "Int16":
                            case "Int32":
                            case "Int64":
                            case "Byte":
                                processCollumnIndexes.Add(new KeyValuePair<int, bool>(column.Ordinal, false));
                                break;

                            case "Decimal":
                            case "Double":
                                processCollumnIndexes.Add(new KeyValuePair<int, bool>(column.Ordinal, true));
                                break;
                        }
                    }

                    // Temporary row used for calculation of average values of the columns
                    DataRow tempRow = filteredDataSet.Tables[0].NewRow();

                    // Initialize int + double values
                    foreach (KeyValuePair<int, bool> index in processCollumnIndexes)
                    {
                        tempRow[index.Key] = 0;
                    }

                    // Loop through all rows and calculate average values for the row intervals
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        DataRow currentRow = ds.Tables[0].Rows[i];

                        // Loop through all columns and add their values to the tempRow
                        foreach (KeyValuePair<int, bool> index in processCollumnIndexes)
                        {
                            // Column is integer
                            if (!index.Value)
                            {
                                // Calculate sum of all rows which are going to be made average
                                tempRow[index.Key] = Convert.ToInt64(tempRow[index.Key]) + Convert.ToInt64((!currentRow.IsNull(index.Key)) ? currentRow[index.Key] : 0);
                            }
                            // Column is double
                            else
                            {
                                // Calculate sum of all rows which are going to be made average
                                tempRow[index.Key] = Convert.ToDouble(tempRow[index.Key]) + Convert.ToDouble((!currentRow.IsNull(index.Key)) ? currentRow[index.Key] : 0);
                            }
                        }

                        counter++;

                        if (counter == keepEveryXItem)
                        {
                            // Copy the current row to the filtered table. The averaged values will be changed afterwards.
                            filteredDataSet.Tables[0].ImportRow(currentRow);

                            DataRow averagedRow = filteredDataSet.Tables[0].Rows[filteredDataSet.Tables[0].Rows.Count - 1];

                            // Loop through all columns and calculate their average values and add the to the final row
                            foreach (KeyValuePair<int, bool> index in processCollumnIndexes)
                            {
                                // Column is integer
                                if (!index.Value)
                                {
                                    // Calculate the average value
                                    averagedRow[index.Key] = Convert.ToInt64(tempRow[index.Key]) / keepEveryXItem;
                                }
                                // Column is double
                                else
                                {
                                    // Calculate the average value
                                    averagedRow[index.Key] = Convert.ToDouble(tempRow[index.Key]) / keepEveryXItem;
                                }
                            }

                            // Reset the counter
                            counter = 0;

                            // Temporary row used for calculation of average values of the columns
                            tempRow = filteredDataSet.Tables[0].NewRow();

                            // Initialize int + double values
                            foreach (KeyValuePair<int, bool> index in processCollumnIndexes)
                            {
                                tempRow[index.Key] = 0;
                            }
                        }
                    }
                }
            }

            return filteredDataSet;
        }


        /// <summary>
        /// Method to convert LINQ result to DataTable
        /// </summary>
        /// <typeparam name="T">Type to convert</typeparam>
        /// <param name="varlist">Variable list</param>
        public static DataTable ConvertToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = null;
            try
            {
                dtReturn = new DataTable();
                // column names   
                PropertyInfo[] oProps = null;

                if (varlist == null)
                {
                    return dtReturn;
                }

                foreach (T rec in varlist)
                {
                    // Use reflection to get property names, to create table, Only first time, others will follow   
                    if (oProps == null)
                    {
                        oProps = rec.GetType().GetProperties();
                        foreach (PropertyInfo pi in oProps)
                        {
                            Type colType = pi.PropertyType;

                            if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                            {
                                colType = colType.GetGenericArguments()[0];
                            }

                            dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                        }
                    }

                    DataRow dr = dtReturn.NewRow();

                    foreach (PropertyInfo pi in oProps)
                    {
                        dr[pi.Name] = pi.GetValue(rec, null) ?? DBNull.Value;
                    }

                    dtReturn.Rows.Add(dr);
                }
                return dtReturn;
            }
            catch
            {
                if (dtReturn != null)
                {
                    dtReturn.Dispose();
                }
                throw;
            }
        }


        /// <summary>
        /// Trims first data table from source to fit into page.
        /// </summary>
        /// <param name="source">Data source</param>
        /// <param name="offset">Index of first record belonging to result page</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records</param>
        public static DataSet TrimDataSetPage(object source, int offset, int pageSize, ref int totalRecords)
        {
            DataSet result = null;
            if (!DataSourceIsEmpty(source))
            {
                // Get table from data source
                DataTable sourceTable = GetDataTable(source);
                if (sourceTable != null)
                {
                    int itemsCount = sourceTable.Rows.Count;

                    // Negative page size means all items on one page
                    if (pageSize <= 0)
                    {
                        pageSize = itemsCount;
                    }

                    bool trimData = (itemsCount > pageSize);

                    // Total records might be set from outside. Overwrite it only when necessary.
                    if (trimData || (totalRecords < 0))
                    {
                        totalRecords = itemsCount;
                    }

                    result = new DataSet();

                    // Table contains more items than can be displayed on the page
                    if (trimData)
                    {
                        // Offset must not be negative
                        if (offset < 0)
                        {
                            offset = 0;
                        }

                        // Prepare result table
                        DataTable dt = sourceTable.Clone();

                        // Get sorted data from source table.
                        DataView dv = sourceTable.DefaultView;

                        int pageEnd = Math.Min(offset + pageSize, dv.Count);

                        // Get only rows for current page
                        for (int i = offset; i < pageEnd; i++)
                        {
                            DataRow dataRow = dt.NewRow();

                            // Copy row data
                            dataRow.ItemArray = dv[i].Row.ItemArray;
                            dt.Rows.Add(dataRow);
                        }
                        result.Tables.Add(dt);
                    }
                    else
                    {
                        // Return source as it is
                        if (source is DataSet)
                        {
                            result = source as DataSet;
                        }
                        else
                        {
                            // Or copy the whole table
                            result.Tables.Add(sourceTable.Copy());
                        }
                    }
                }
            }
            else
            {
                // Empty data source has no records
                totalRecords = 0;
            }
            return result;
        }

        #endregion


        #region "Data row methods"

        /// <summary>
        /// Gets the bool variable from given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public static bool GetBoolValue(DataRow dr, string columnName, bool defaultValue = false)
        {
            object value = GetDataRowValue(dr, columnName);
            return ValidationHelper.GetBoolean(value, defaultValue);
        }


        /// <summary>
        /// Gets the integer variable from given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public static int GetIntValue(DataRow dr, string columnName, int defaultValue = 0)
        {
            object value = GetDataRowValue(dr, columnName);
            return ValidationHelper.GetInteger(value, defaultValue);
        }


        /// <summary>
        /// Gets the guid variable from given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public static Guid GetGuidValue(DataRow dr, string columnName, Guid defaultValue = new Guid())
        {
            object value = GetDataRowValue(dr, columnName);
            return ValidationHelper.GetGuid(value, defaultValue);
        }


        /// <summary>
        /// Gets the string variable from given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetStringValue(DataRow dr, string columnName, string defaultValue = "")
        {
            object value = GetDataRowValue(dr, columnName);
            return ValidationHelper.GetString(value, defaultValue);
        }


        /// <summary>
        /// Gets the DateTime variable from given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public static DateTime GetDateTimeValue(DataRow dr, string columnName, DateTime defaultValue = new DateTime())
        {
            object value = GetDataRowValue(dr, columnName);
            return ValidationHelper.GetDateTime(value, defaultValue);
        }

        #endregion


        #region "Data container methods"

        /// <summary>
        /// Gets the bool variable from given data container.
        /// </summary>
        /// <param name="data">Container with the data</param>
        /// <param name="columnName">Column name</param>
        public static bool GetBoolValue(ISimpleDataContainer data, string columnName)
        {
            return GetBoolValue(data, columnName, false);
        }


        /// <summary>
        /// Gets the bool variable from given data container.
        /// </summary>
        /// <param name="data">Container with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        public static bool GetBoolValue(ISimpleDataContainer data, string columnName, bool defaultValue)
        {
            object value = data.GetValue(columnName);
            return ValidationHelper.GetBoolean(value, defaultValue);
        }


        /// <summary>
        /// Gets the integer variable from given Data container.
        /// </summary>
        /// <param name="data">Container with the data</param>
        /// <param name="columnName">Column name</param>
        public static int GetIntValue(ISimpleDataContainer data, string columnName)
        {
            object value = data.GetValue(columnName);
            return ValidationHelper.GetInteger(value, 0);
        }


        /// <summary>
        /// Gets the guid variable from given data container.
        /// </summary>
        /// <param name="data">Container with the data</param>
        /// <param name="columnName">Column name</param>
        public static Guid GetGuidValue(ISimpleDataContainer data, string columnName)
        {
            object value = data.GetValue(columnName);
            return ValidationHelper.GetGuid(value, Guid.Empty);
        }


        /// <summary>
        /// Gets the string variable from given data container.
        /// </summary>
        /// <param name="data">Container with the data</param>
        /// <param name="columnName">Column name</param>
        public static string GetStringValue(ISimpleDataContainer data, string columnName)
        {
            object value = data.GetValue(columnName);
            return ValidationHelper.GetString(value, null);
        }


        /// <summary>
        /// Gets the DateTime variable from given data container.
        /// </summary>
        /// <param name="data">Container with the data</param>
        /// <param name="columnName">Column name</param>
        public static DateTime GetDateTimeValue(ISimpleDataContainer data, string columnName)
        {
            object value = data.GetValue(columnName);
            return ValidationHelper.GetDateTime(value, DateTime.MinValue);
        }

        #endregion


        #region "Table methods"

        /// <summary>
        /// Gets the array of integer values from the given data table.
        /// </summary>
        /// <param name="dt">Source data table</param>
        /// <param name="columnName">Column name</param>
        public static IList<int> GetIntegerValues(DataTable dt, string columnName)
        {
            if (dt == null)
            {
                return new int[] { };
            }

            var rows = dt.Rows;

            // Prepare the data
            int columnIndex = dt.Columns.IndexOf(columnName);

            // Column hasn't been found
            if (columnIndex == -1)
            {
                throw new Exception("[GetIntegerValues]: Column '" + columnName + "' hasn't been found!");
            }

            int count = rows.Count;

            var result = new List<int>(count);
            var existing = new HashSet<int>();

            // Process all rows
            foreach (DataRow dr in rows)
            {
                // Add the value to the result
                object value = dr[columnIndex];
                if (value != DBNull.Value)
                {
                    int id = Convert.ToInt32(value);
                    if ((id > 0) && existing.Add(id))
                    {
                        result.Add(id);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the array of string values from the given data table.
        /// </summary>
        /// <param name="dt">Source data table</param>
        /// <param name="columnName">Column name</param>
        public static List<string> GetStringValues(DataTable dt, string columnName)
        {
            if (dt == null)
            {
                return new List<string>();
            }

            // Prepare the data
            int columnIndex = dt.Columns.IndexOf(columnName);

            // Column hasn't been found
            if (columnIndex == -1)
            {
                throw new Exception("[GetStringValues]: Column '" + columnName + "' hasn't been found!");
            }

            int count = dt.Rows.Count;

            var result = new List<string>(count);
            var existing = new HashSet<string>();

            // Process all rows
            foreach (DataRow dr in dt.Rows)
            {
                // Add the value to the result
                object value = dr[columnIndex];
                if (value != DBNull.Value)
                {
                    string id = Convert.ToString(value);

                    if (existing.Add(id))
                    {
                        result.Add(id);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the array of the values from the given data table.
        /// </summary>
        /// <param name="dt">Source data table</param>
        /// <param name="columnName">Column name</param>
        public static List<T> GetValues<T>(DataTable dt, string columnName)
        {
            if (dt == null)
            {
                return new List<T>();
            }

            // Prepare the data
            int columnIndex = dt.Columns.IndexOf(columnName);

            // Column hasn't been found
            if (columnIndex == -1)
            {
                throw new Exception("[GetValues]: Column '" + columnName + "' hasn't been found!");
            }

            int count = dt.Rows.Count;

            var result = new List<T>(count);
            var existing = new HashSet<T>();

            // Process all rows
            foreach (DataRow dr in dt.Rows)
            {
                // Add the value to the result
                object value = dr[columnIndex];
                if (value != DBNull.Value)
                {
                    T val = ValidationHelper.GetValue<T>(value);

                    if ((val != null) && existing.Add(val))
                    {
                        result.Add(val);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Re-computes count of child objects in <paramref name="parentObjects"/> data table's <paramref name="childCountColumn"/> column.
        /// </summary>
        /// <param name="parentObjects">Data table containing parent objects.</param>
        /// <param name="idColumn">Identity column name in <paramref name="parentObjects"/> table which the child objects reference.</param>
        /// <param name="childCountColumn">Child count column name where the computed child objects count is stored.</param>
        /// <param name="childObjects">Data table containing child objects.</param>
        /// <param name="parentIdColumn">Foreign key column name in <paramref name="childObjects"/> table which references the parent.</param>
        public static void RecomputeChildCount(DataTable parentObjects, string idColumn, string childCountColumn, DataTable childObjects, string parentIdColumn)
        {
            if (parentObjects.Rows == null)
            {
                return;
            }

            var foreignIdOccurences = GetValuesFrequency(childObjects, parentIdColumn);

            // Assign child count
            foreach (DataRow row in parentObjects.Rows)
            {
                object id = row[idColumn];
                row[childCountColumn] = foreignIdOccurences.ContainsKey(id) ? foreignIdOccurences[id] : 0;
            }
        }


        /// <summary>
        /// Gets frequency of values in specified data table's column.
        /// </summary>
        /// <param name="dt">Data table.</param>
        /// <param name="columnName">Name of column for which the frequency is determined.</param>
        /// <returns>Pairs of (value, occurrences count) in <paramref name="dt"/>'s column named <paramref name="columnName"/>.</returns>
        public static Dictionary<object, int> GetValuesFrequency(DataTable dt, string columnName)
        {
            return GetValuesFrequency(dt.Rows.Cast<DataRow>(), row => row[columnName]);
        }


        /// <summary>
        /// Gets frequency of values in data collection.
        /// </summary>
        /// <typeparam name="TDataContainer">Data container (e.g. DataRow, AbstractInfo).</typeparam>
        /// <param name="dataCollection">Enumerable collection of data.</param>
        /// <param name="valueSelector">Function selecting item (value) within TDataContainer.</param>
        /// <returns>Pairs of (value, occurrences count) in <paramref name="dataCollection"/>.</returns>
        public static Dictionary<object, int> GetValuesFrequency<TDataContainer>(IEnumerable<TDataContainer> dataCollection, Func<TDataContainer, object> valueSelector)
        {
            Dictionary<object, int> frequency = new Dictionary<object, int>();
            if (dataCollection != null)
            {
                foreach (TDataContainer data in dataCollection)
                {
                    object value = valueSelector(data);
                    frequency[value] = frequency.ContainsKey(value) ? frequency[value] + 1 : 1;
                }
            }

            return frequency;
        }

        #endregion


        #region "DataSet methods"

        /// <summary>
        /// Restricts the number of rows to the top N items.
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="topN">Top N items</param>
        public static void RestrictRows(DataSet ds, int topN)
        {
            if (DataSourceIsEmpty(ds) || (topN <= 0))
            {
                return;
            }

            int currentCount = 0;

            // Get to the first table that violates the top N condition
            DataTable dt = null;

            int i;
            for (i = 0; i < ds.Tables.Count; i++)
            {
                dt = ds.Tables[i];
                currentCount += dt.Rows.Count;
                if (currentCount > topN)
                {
                    break;
                }
            }

            // Restrict the row numbers
            if ((dt != null) && (currentCount > topN))
            {
                // Remove the items from current table
                int removeItems = currentCount - topN;
                for (int j = 0; j < removeItems; j++)
                {
                    // Remove last item from default view
                    dt.Rows.Remove(dt.DefaultView[dt.Rows.Count - 1].Row);
                }

                // Remove all the additional tables
                i++;
                while (i < ds.Tables.Count)
                {
                    ds.Tables.RemoveAt(ds.Tables.Count - 1);
                    i++;
                }

                ds.AcceptChanges();
            }
        }


        /// <summary>
        /// Restricts the given DataSet to only given rows starting with the offset, and leaving only maximum defined number of rows.
        /// </summary>
        /// <param name="ds">DataSet with the data</param>
        /// <param name="offset">Starting offset</param>
        /// <param name="maxRecords">Maximum number of the records</param>
        /// <param name="totalRecords">Returns the total number of available records</param>
        /// <returns>Returns the same DataSet object</returns>
        public static DataSet RestrictRows(DataSet ds, int offset, int maxRecords, ref int totalRecords)
        {
            if (DataSourceIsEmpty(ds))
            {
                totalRecords = 0;
                return ds;
            }

            // Indexes of the items to leave
            int start = offset;
            int end = offset + maxRecords;

            int index = 0;
            totalRecords = GetItemsCount(ds);

            // Go through all tables
            foreach (DataTable dt in ds.Tables)
            {
                // Get rows to remove
                var removeRows = new ArrayList();
                foreach (DataRow dr in dt.Rows)
                {
                    if ((index < start) || (index >= end))
                    {
                        removeRows.Add(dr);
                    }

                    index++;
                }

                // Remove the rows
                foreach (DataRow dr in removeRows)
                {
                    dt.Rows.Remove(dr);
                }
            }

            ds.AcceptChanges();

            return ds;
        }


        /// <summary>
        /// Executes the given action for each row in the given DataSet
        /// </summary>
        /// <param name="ds">DataSet with the data</param>
        /// <param name="rowAction">Row action</param>
        /// <param name="rowCondition">Row condition to filter rows</param>
        public static void ForEachRow(DataSet ds, Action<DataRow> rowAction, Func<DataRow, bool> rowCondition = null)
        {
            // No data - no action
            if (DataSourceIsEmpty(ds))
            {
                return;
            }

            // Process all tables
            foreach (DataTable dt in ds.Tables)
            {
                ForEachRow(dt, rowAction, rowCondition);
            }
        }


        /// <summary>
        /// Executes the given action for each row in the given DataSet
        /// </summary>
        /// <param name="dt">Table with the data</param>
        /// <param name="rowAction">Row action</param>
        /// <param name="rowCondition">Row condition to filter rows</param>
        public static void ForEachRow(DataTable dt, Action<DataRow> rowAction, Func<DataRow, bool> rowCondition = null)
        {
            // No data - no action
            if (DataSourceIsEmpty(dt))
            {
                return;
            }

            // Process all rows
            foreach (DataRow dr in dt.Rows)
            {
                if ((rowCondition == null) || rowCondition(dr))
                {
                    rowAction(dr);
                }
            }
        }


        /// <summary>
        /// Creates the union of two Datasets, the values are compared for uniqueness by the given ID column name.
        /// </summary>
        /// <param name="ds1">First DataSet</param>
        /// <param name="ds2">Second DataSet</param>
        /// <param name="idColumn">ID column name</param>
        public static DataSet Union(DataSet ds1, DataSet ds2, string idColumn)
        {
            if (ds1 == null)
            {
                return ds2;
            }
            if (ds2 == null)
            {
                return ds1;
            }

            ArrayList addTables = new ArrayList();

            // Add the new rows
            foreach (DataTable dt in ds2.Tables)
            {
                DataTable destTable = ds1.Tables[dt.TableName];
                if (destTable != null)
                {
                    // Create list of existing IDs
                    ArrayList existingIDs = new ArrayList();
                    foreach (DataRow dr in destTable.Rows)
                    {
                        int id = ValidationHelper.GetInteger(dr[idColumn], 0);
                        if ((id > 0) && !existingIDs.Contains(id))
                        {
                            existingIDs.Add(id);
                        }
                    }

                    // Add new rows
                    foreach (DataRow dr in dt.Rows)
                    {
                        int id = ValidationHelper.GetInteger(dr[idColumn], 0);
                        if ((id > 0) && !existingIDs.Contains(id))
                        {
                            // Add new row
                            destTable.Rows.Add(dr.ItemArray);
                            existingIDs.Add(id);
                        }
                    }
                }
                else
                {
                    // Add full table
                    addTables.Add(dt);
                }
            }

            // Add full tables
            foreach (DataTable dt in addTables)
            {
                ds2.Tables.Remove(dt);
                ds1.Tables.Add(dt);
            }

            return ds1;
        }


        /// <summary>
        /// Transfers tables from source DataSet to the destination DataSet and sets the source table name to specified value.
        /// </summary>
        /// <param name="destinationDS">Destination DataSet</param>
        /// <param name="sourceDS">Source DataSet</param>
        /// <param name="tableName">Source table name</param>
        public static void TransferTable(DataSet destinationDS, DataSet sourceDS, string tableName)
        {
            SetTableName(sourceDS, tableName);
            TransferTables(destinationDS, sourceDS);
        }


        /// <summary>
        /// Transfers tables from source DataSet to the destination DataSet.
        /// </summary>
        /// <param name="destinationDS">Destination DataSet</param>
        /// <param name="sourceDS">Source DataSet</param>
        public static void TransferTables(DataSet destinationDS, DataSet sourceDS)
        {
            if (destinationDS == null)
            {
                throw new Exception("[SqlHelper.TransferTables]: Destination DataSet not specified.");
            }

            if (sourceDS != null)
            {
                // Transfer all tables
                while (sourceDS.Tables.Count > 0)
                {
                    DataTable dt = sourceDS.Tables[0];
                    sourceDS.Tables.Remove(dt);

                    if (!destinationDS.Tables.Contains(dt.TableName))
                    {
                        // Transfer table directly
                        destinationDS.Tables.Add(dt);
                    }
                    else if (dt.Rows.Count > 0)
                    {
                        // Transfer the records
                        DataTable destDT = destinationDS.Tables[dt.TableName];

                        MergeTables(dt, destDT, true);
                    }
                }
            }
        }


        /// <summary>
        /// Makes the given DataSet read-only
        /// </summary>
        /// <param name="ds">DataSet</param>
        public static void LockDataSet(DataSet ds)
        {
            // Do not lock null
            if (ds == null)
            {
                return;
            }

            // Set the object as read-only
            var objDs = ds as IReadOnlyFlag;
            if (objDs != null)
            {
                objDs.IsReadOnly = true;
            }

            EventHandler locked = (sender, e) =>
            {
                string msg = String.Format("DataSet '{0}' cannot be modified, it is read-only.", ds.DataSetName);
                throw new InvalidOperationException(msg);
            };

            // Prevent table modification
            foreach (DataTable t in ds.Tables)
            {
                t.RowChanging += new DataRowChangeEventHandler(locked);
                t.RowDeleted += new DataRowChangeEventHandler(locked);
                t.ColumnChanging += new DataColumnChangeEventHandler(locked);
                t.TableClearing += new DataTableClearEventHandler(locked);
                t.TableNewRow += new DataTableNewRowEventHandler(locked);
            }

            // Prevent table collection modification
            ds.Tables.CollectionChanging += new CollectionChangeEventHandler(locked);
        }

        #endregion
    }
}