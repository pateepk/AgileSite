using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Compatibility
{
    using TypedDataSet = InfoDataSet<TreeNode>;

    /// <summary>
    /// Provides data in a compatible way with an old API using the new approach of <see cref="MultiDocumentQuery"/> internally.
    /// </summary>
    internal class NodesDataProvider
    {
        /// <summary>
        /// Indicates if tables from result DataSet are merged into a single table.
        /// </summary>
        public bool MergeResults
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if merged results should be sorted.
        /// </summary>
        public bool SortMergedResults
        {
            get;
            set;
        }


        /// <summary>
        /// Base query name to use for the document selection (for the enhanced selection options only).
        /// </summary>
        public string SelectQueryName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if all data including coupled data should be selected. Otherwise only base document data are selected.
        /// </summary>
        public bool SelectAllData
        {
            get;
            set;
        }


        private NodeSelectionParameters Parameters
        {
            get;
            set;
        }


        private List<string> ResolvedClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Custom preferred culture code.
        /// </summary>
        public string PreferredCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Creates instance of the <see cref="NodesDataProvider"/>.
        /// </summary>
        /// <param name="parameters">Parameters to be used to get the data</param>
        public NodesDataProvider(NodeSelectionParameters parameters)
        {
            SortMergedResults = true;
            SelectAllData = true;
            Parameters = parameters;
            ResolvedClassNames = Parameters.GetResolvedClassNames();
        }


        /// <summary>
        /// Gets nodes based on instance parameters.
        /// </summary>
        public TypedDataSet GetDataSet()
        {
            var query = GetQuery();
            var data = query.TypedResult;

            data = SplitDataSetIntoTables(data);
            data = SortDataSet(data);

            return data;
        }


        private TypedDataSet SortDataSet(TypedDataSet data)
        {
            var orderBy = Parameters.OrderBy;
            if (!MergeResults || !SortMergedResults || (orderBy == null) || (data == null))
            {
                return data;
            }

            data.AcceptChanges();
            DataHelper.SortDataTable(data.Tables[0], orderBy);

            return data;
        }


        private IncludeCoupledDataEnum GetIncludeCoupledDataType()
        {
            var classesCount = ResolvedClassNames.Count;
            if ((classesCount > 0) && Parameters.SelectAllData && SelectAllData)
            {
                // Set type of including columns
                return !String.IsNullOrEmpty(Parameters.Columns) ? IncludeCoupledDataEnum.InnerQueryOnly : IncludeCoupledDataEnum.Complete;
            }

            return IncludeCoupledDataEnum.None;
        }


        private MultiDocumentQuery GetQuery()
        {
            var parameters = Parameters;
            var includeCoupledColumnsType = GetIncludeCoupledDataType();
            var aliasPath = parameters.GetNodeAliasPathForSelection();
            var singleNode = parameters.SelectSingleNode;
            var columns = parameters.Columns;
            var where = parameters.Where;
            var relationshipNodeGuid = parameters.RelationshipNodeGUID;
            var relationshipName = parameters.RelationshipName;
            var relationshipSide = parameters.RelationshipSide;
            var selectOnlyPublished = parameters.SelectOnlyPublished;
            var latestVersion = parameters.SelectLatestVersion;

            var selectSingleMode = singleNode && !String.IsNullOrEmpty(aliasPath);
            var classesCount = ResolvedClassNames.Count;

            var topN = selectSingleMode ? 1 : parameters.TopN;

            // Prepare query
            var tree = new TreeProvider();
            var data = tree.SelectNodes()
                           .TopN(topN)
                           .Columns(columns)
                           .Where(where)
                           .InRelationWith(relationshipNodeGuid, relationshipName, relationshipSide)
                           .Types(ResolvedClassNames.ToArray())
                           .Published(selectOnlyPublished)
                           .LatestVersion(latestVersion)
                           .WithCoupledColumns(includeCoupledColumnsType);

            // Use custom query name if set
            var selectQueryName = SelectQueryName;
            if (!String.IsNullOrEmpty(selectQueryName))
            {
                data.QueryName = selectQueryName;
            }

            // For more classes without coupled columns ensure source type column (not using union over all selected types)
            bool selectAllData = parameters.SelectAllData && SelectAllData;

            if (selectAllData && (classesCount > 1) && includeCoupledColumnsType == IncludeCoupledDataEnum.None)
            {
                var col = new QueryColumn(String.Format("'{0}' + [ClassName]", DocumentHelper.DOCUMENT_PREFIX)).As(SystemColumns.SOURCE_TYPE);
                data.AddColumn(col);
            }

            // For more classes with no specific columns use global order by to achieve default sorting by document type first then defined order by or row number
            string orderBy = parameters.OrderBy;
            if (selectAllData && (classesCount > 1) && String.IsNullOrEmpty(columns))
            {
                var order = !String.IsNullOrEmpty(orderBy) ? orderBy : SystemColumns.SOURCE_ROW_NUMBER;
                data.OrderByResultColumns = String.Join(", ", SystemColumns.SOURCE_NUMBER, order).TrimEnd(',');
            }
            else
            {
                data.OrderBy(orderBy);
            }

            // Set node alias path
            if (aliasPath != null)
            {
                data.Path(aliasPath);
            }

            // Set nesting level if alias path contains macros to gather more documents and selecting multiple nodes
            if ((aliasPath == null) || (!singleNode && aliasPath.IndexOfAny(new[] { '%', '_', '[' }) >= 0))
            {
                data.NestingLevel(parameters.MaxRelativeLevel);
            }

            // Set site context
            string siteName = parameters.SiteName;
            if (!TreeProvider.AllSites(siteName))
            {
                data.OnSite(siteName);
            }

            SetCustomPreferredCulture(data);

            TreeProvider.SetQueryCultures(data, parameters.CultureCode, parameters.CombineWithDefaultCulture);

            return data;
        }


        private void SetCustomPreferredCulture(MultiDocumentQuery data)
        {
            data.Properties.PreferredCultureCode = PreferredCultureCode;
        }


        private TypedDataSet GetEmptyDataSet()
        {
            return new TypedDataSet();
        }


        private TypedDataSet SplitDataSetIntoTables(TypedDataSet data)
        {
            if (DataHelper.DataSourceIsEmpty(data))
            {
                RemoveSystemColumns(data);
                return data;
            }

            var selectAllData = Parameters.SelectAllData && SelectAllData;
            var types = ResolvedClassNames;

            // Change only table name in case merging results or less than 2 types are specified or flag to select data without coupled columns was set to false (backward compatibility)
            if (MergeResults || !selectAllData || (types.Count <= 1))
            {
                ChangeTableName(data, selectAllData, types);
                RemoveSystemColumns(data);
                return data;
            }

            var mainTable = data.Tables[0];

            // Get a list of selected columns without system columns
            var selectedColumns = mainTable.Columns
                                        .Cast<DataColumn>()
                                        .Select(t => t.ColumnName)
                                        .ToList();


            List<string> tableColumns = null;
            var allDocumentTypesColumns = ClassStructureInfo.GetColumns(types.ToArray());

            var includeTypesColumns = GetIncludeCoupledDataType() != IncludeCoupledDataEnum.None;
            if (!includeTypesColumns)
            {
                // Remove all document types columns
                tableColumns = selectedColumns.Except(allDocumentTypesColumns, StringComparer.InvariantCultureIgnoreCase).ToList();
            }

            var result = GetEmptyDataSet();

            foreach (var className in types)
            {
                // Get structure
                var structure = ClassStructureInfo.GetClassInfo(className);
                if (structure == null)
                {
                    continue;
                }

                if (includeTypesColumns)
                {
                    // Get list of columns only for current document type
                    var otherTypesColumns = allDocumentTypesColumns.Except(structure.ColumnNames, StringComparer.InvariantCultureIgnoreCase);
                    tableColumns = selectedColumns
                        .Except(otherTypesColumns, StringComparer.InvariantCultureIgnoreCase)
                        .ToList();
                }

                // Create a table
                var view = mainTable.DefaultView;
                view.RowFilter = $"{SystemColumns.SOURCE_TYPE} = '{TreeNodeProvider.GetObjectType(className)}'";
                var table = view.ToTable(className.ToLowerInvariant(), false, tableColumns.Where(x => !SystemColumns.IsSystemColumn(x)).ToArray());
                if (table.Rows.Count > 0)
                {
                    result.Tables.Add(table);
                }
            }

            return result;
        }


        private static void ChangeTableName(TypedDataSet data, bool selectAllData, List<string> types)
        {
            // Get table name from first page type if also coupled columns are requested else use default table name
            var tableName = selectAllData ? types.FirstOrDefault() : String.Empty;
            data.Tables[0].TableName = String.IsNullOrEmpty(tableName) ? TreeNode.OBJECT_TYPE : tableName;
        }


        private static void RemoveSystemColumns(TypedDataSet data)
        {
            foreach (DataTable table in data.Tables)
            {
                var systemColumnNames = table.Columns
                                             .Cast<DataColumn>()
                                             .Select(c => c.ColumnName)
                                             .Where(SystemColumns.IsSystemColumn)
                                             .ToList();

                foreach (var columnName in systemColumnNames)
                {
                    table.Columns.Remove(columnName);
                }
            }
        }
    }
}
