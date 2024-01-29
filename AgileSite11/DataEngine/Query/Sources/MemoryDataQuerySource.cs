using System;
using System.Data;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data query source as DataSet
    /// </summary>
    public class MemoryDataQuerySource : DataQuerySource
    {
        #region "Variables"

        private DataSet mSourceData;
        private string mDataSourceName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Source data
        /// </summary>
        protected DataSet SourceData
        {
            get
            {
                if (GetSourceData != null)
                {
                    return GetSourceData();
                }

                return mSourceData;
            }
            set
            {
                mSourceData = value;
            }
        }


        /// <summary>
        /// Data source identifier.
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query.
        /// If name is not set, new name is generated. Generated name is not cached, MemoryDataQuerySource must always be non-compatible even with the same instance. 
        /// For more information <see cref="DataQueryBase{T}.HasCompatibleSource"/> method.
        /// </remarks>
        public override string DataSourceName
        {
            get
            {
                return mDataSourceName ?? GetDataSourceName();
            }
            internal set
            {
                mDataSourceName = value;
            }
        }


        /// <summary>
        /// Function to dynamically get the source data at the given moment
        /// </summary>
        protected Func<DataSet> GetSourceData
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceData">Source data</param>
        public MemoryDataQuerySource(DataSet sourceData)
        {
            SourceData = sourceData;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getSourceData">Function to dynamically get the source data at the given moment</param>
        public MemoryDataQuerySource(Func<DataSet> getSourceData)
        {
            GetSourceData = getSourceData;
        }


        /// <summary>
        /// Gets data source identifier. 
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query.
        /// </remarks>
        protected override string GetDataSourceName()
        {
            var data = SourceData;

            // Get source names
            string dataSetName = null;
            string tableName = null;

            if (data != null)
            {
                dataSetName = data.DataSetName;

                if (data.Tables.Count > 0)
                {
                    tableName = data.Tables[0].TableName;
                }
            }

            return String.Format("{0}{1}|{2}|{3}", MEMORY_PREFIX, Guid.NewGuid(), dataSetName, tableName);
        }


        /// <summary>
        /// Executes the query
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        public override DataSet GetData(DataQuerySourceParameters parameters)
        {
            try
            {
                var sourceData = SourceData;

                var maxRecords = parameters.MaxRecords;
                var offset = parameters.Offset;

                // Get parameters
                var p = parameters.Settings;

                string where = p.WhereCondition;
                string orderBy = p.OrderByColumns;
                string columns = p.SelectColumnsList.GetColumns(p.Parameters);

                int topN = p.TopNRecords;

                SqlHelper.HandleEmptyColumns(ref orderBy);
                SqlHelper.HandleEmptyColumns(ref columns);

                where = ExpandExpression(where, p);
                orderBy = ExpandExpression(orderBy, p);

                // If no parameters specified, return complete original data
                var filterEmpty = String.IsNullOrEmpty(where) && String.IsNullOrEmpty(orderBy) && String.IsNullOrEmpty(columns);
                var processRowNumber = (maxRecords > 0) || (topN > 0);

                if (filterEmpty && !processRowNumber)
                {
                    parameters.TotalRecords = DataHelper.GetItemsCount(sourceData);
                    return sourceData;
                }

                // Prepare the list of columns
                string[] cols = null;

                var columnsSet = !String.IsNullOrEmpty(columns);
                if (columnsSet)
                {
                    cols = SqlHelper.ParseColumnList(columns, true).ToArray();
                }

                // Fix where condition for selecting in memory from dataset
                if (where != null)
                {
                    where = FixWhere(where);
                }

                var result = new DataSet(sourceData.DataSetName);

                int totalRecords = 0;

                // Process all tables
                foreach (DataTable dt in sourceData.Tables)
                {
                    DataTable newDt;

                    if (filterEmpty)
                    {
                        // Copy entire table
                        newDt = dt.Copy();
                    }
                    else
                    {
                        // Select results
                        var dv = new DataView(dt, where, orderBy, DataViewRowState.CurrentRows);

                        // Special case for count
                        if (columnsSet && columns.Trim().EqualsCSafe("COUNT(*)", true))
                        {
                            newDt = SqlHelper.CreateScalarTable(dv.Count);
                        }
                        else if (columnsSet && IsAggregatedSumColumn(p))
                        {
                            var value = dt.Compute(columns, where);
                            newDt = SqlHelper.CreateScalarTable(value);
                        }
                        else
                        {
                            newDt = FilterColumns(dv, dt.TableName, p.SelectDistinct, cols);
                        }
                    }

                    totalRecords += newDt.Rows.Count;

                    result.Tables.Add(newDt);
                }

                // Restrict rows
                if (topN > 0)
                {
                    DataHelper.RestrictRows(result, topN);
                    totalRecords = DataHelper.GetItemsCount(result);
                }

                if (maxRecords > 0)
                {
                    DataHelper.RestrictRows(result, offset, maxRecords, ref totalRecords);
                }

                parameters.TotalRecords = totalRecords;

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(
                   String.Format(
@"
Unable to get data from data source

--Parameters:
{0}
",
                        parameters),
                    ex
                );
            }
        }


        /// <summary>
        /// Returns the number of rows in the result queried by this data source if GetData is called with the same parameters
        /// </summary>
        /// <param name="parameters">Source parameters</param>
        public override int GetCount(DataQuerySourceParameters parameters)
        {
            var sourceData = SourceData;

            var maxRecords = parameters.MaxRecords;
            var offset = parameters.Offset;

            // Get parameters
            var p = parameters.Settings;

            string where = p.WhereCondition;

            int topN = p.TopNRecords;

            where = ExpandExpression(where, p);

            // If no parameters specified, return complete original data
            var filterEmpty = String.IsNullOrEmpty(where);
            var restrictRows = (maxRecords > 0) || (topN > 0);

            if (filterEmpty && !restrictRows)
            {
                return parameters.TotalRecords = DataHelper.GetItemsCount(sourceData);
            }

            // Fix where condition for selecting in memory from dataset
            if (where != null)
            {
                where = FixWhere(where);
            }

            int totalRecords = 0;

            // Process all tables
            foreach (DataTable dt in sourceData.Tables)
            {
                totalRecords += filterEmpty ? dt.Rows.Count : dt.Select(where, null, DataViewRowState.CurrentRows).Length;
            }

            // Restrict rows
            if (topN > 0)
            {
                totalRecords = Math.Min(totalRecords, topN);
            }

            var count = totalRecords;

            // Restrict to a specific page
            if (maxRecords > 0)
            {
                count = Math.Min(Math.Max(totalRecords - offset, 0), maxRecords);
            }

            parameters.TotalRecords = totalRecords;

            return count;
        }


        private static string ExpandExpression(string expression, DataQuerySettings settings)
        {
            return settings.Expand(expression, GetValueForDataSet);
        }


        private static string GetValueForDataSet(DataParameter parameter)
        {
            var value = SqlHelper.GetSqlValue(parameter.Value);

            // Transform DateTime value parameter for using in memory data set
            if (parameter.Value is DateTime)
            {
                value = String.Format("#{0}#", value.Trim('\''));
            }
            else if(parameter.Value is Guid)
            {
                value =$"CONVERT({value},'System.Guid')";
            }

            return value;
        }


        /// <summary>
        /// Fixes the where condition to be able to be used with DataTable select
        /// </summary>
        /// <param name="where">Where condition</param>
        private static string FixWhere(string where)
        {
            return where.Replace(" N'", " '").Replace("(N'", "('");
        }


        /// <summary>
        /// Filters columns in given data view. Supports column aliases (ClassID AS ID).
        /// </summary>
        /// <returns>Data table with subset of columns.</returns>
        private static DataTable FilterColumns(DataView dataView, string tableName, bool selectDisting, params string[] columns)
        {
            string[] columnNames = new string[0];
            string[] columnAliases = new string[0];

            // Parse columns
            if (columns != null)
            {
                columnNames = new string[columns.Length];
                columnAliases = new string[columns.Length];

                for (int i = 0; i < columns.Length; i++)
                {
                    SqlHelper.ParseColumn(columns[i], out columnNames[i], out columnAliases[i]);
                }
            }

            for (int i = 0; i < columnAliases.Length; i++)
            {
                if (columnAliases[i] != null)
                {
                    columnAliases[i] = columnAliases[i].TrimStart('[').TrimEnd(']');

                    // Special case. Computed column used TranslationHelper. 
                    // Already present dataset generated from InfoProviderFake and therefore it only needs to be copied.
                    if (columnAliases[i] == TranslationHelper.QUERY_OBJECT_TYPE_COLUMN)
                    {
                        columnNames[i] = TranslationHelper.QUERY_OBJECT_TYPE_COLUMN;
                    }
                }
            }

            // Special case. Computed column used TranslationHelper. 
            // Already present dataset generated from InfoProviderFake and therefore it only needs to be copied.
            var indexOfTranslationHelperComputedQuery = Array.IndexOf(columnAliases, TranslationHelper.QUERY_OBJECT_TYPE_COLUMN);
            if (indexOfTranslationHelperComputedQuery >= 0)
            {
                columnNames[indexOfTranslationHelperComputedQuery] = TranslationHelper.QUERY_OBJECT_TYPE_COLUMN;
            }

            // Get the result table
            var newTable = dataView.ToTable(tableName, selectDisting, columnNames);

            // Switch column names with aliases
            for (int i = 0; i < columnNames.Length; i++)
            {
                if (columnAliases[i] != null)
                {
                    newTable.Columns[columnNames[i]].ColumnName = columnAliases[i];
                }
            }

            return newTable;
        }


        private static bool IsAggregatedSumColumn(IDataQuerySettings settings)
        {
            var column = settings.SelectColumnsList[0] as AggregatedColumn;

            return settings.SelectColumnsList.IsSingleColumn && column != null && column.AggregationType == AggregationType.Sum;
        }

        #endregion
    }
}
