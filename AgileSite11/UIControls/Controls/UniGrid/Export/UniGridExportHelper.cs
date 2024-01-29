using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ImportExport;
using CMS.IO;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Helper for exporting data from UniGrid view
    /// </summary>
    public class UniGridExportHelper : DataExportHelper
    {
        #region "Variables"

        private List<BoundField> mBoundFields;

        private bool definitionLoaded;
        private bool mUseGridFilter = true;

        /// <summary>
        /// Holds columns available for export.
        /// </summary>
        protected List<string> mAvailableColumns;

        /// <summary>
        /// Dictionary to cache column indexes
        /// </summary>
        private TwoLevelDictionary<string, string, int> mColumnIndexes = new TwoLevelDictionary<string, string, int>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Holds an instance of UniGrid control.
        /// </summary>
        public UniGrid UniGrid
        {
            get;
            private set;
        }


        /// <summary>
        /// Retrieves data from UniGrid control (applying data properties).
        /// </summary>
        public override DataSet DataSource
        {
            get
            {
                if (mDataSource != null)
                {
                    return mDataSource;
                }

                if (!EnsureGridDefinition())
                {
                    return mDataSource;
                }

                // Save old values
                bool displayPager = UniGrid.Pager.DisplayPager;
                string sortDirect = UniGrid.SortDirect;
                string whereClause = UniGrid.WhereClause;
                string columns = UniGrid.Columns;
                int topN = UniGrid.TopN;
                bool applyPageSize = UniGrid.ApplyPageSize;

                // Set new values
                UniGrid.Pager.DisplayPager = CurrentPageOnly;
                UniGrid.ApplyPageSize = CurrentPageOnly;
                if (!string.IsNullOrEmpty(OrderBy))
                {
                    UniGrid.SortDirect = OrderBy;
                }

                string newWhere = UseGridFilter ? UniGrid.WhereClause : null;
                if (!string.IsNullOrEmpty(WhereCondition))
                {
                    newWhere = SqlHelper.AddWhereCondition(newWhere, WhereCondition);
                }
                UniGrid.WhereClause = newWhere;
                if (ExportRawData)
                {
                    if ((Columns == null) || (Columns.Count == 0))
                    {
                        UniGrid.Columns = string.Empty;
                    }
                    else
                    {
                        UniGrid.Columns = "[" + string.Join("],[", Columns.ToArray()) + "]";
                    }
                }
                if (!CurrentPageOnly)
                {
                    UniGrid.TopN = Records;
                }
                else
                {
                    if (Records >= 0)
                    {
                        UniGrid.TopN = UniGrid.CurrentOffset + Records;
                    }
                }
                try
                {
                    // Load data
                    mDataSource = UniGrid.RetrieveData();
                    // Ensure table name
                    if ((mDataSource != null) && (mDataSource.Tables.Count == 1) && (SafeObjectType != null))
                    {
                        mDataSource.Tables[0].TableName = SafeObjectType;
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ResHelper.GetString("export.errorloadingdata"), ex);
                    throw;
                }
                finally
                {
                    // Return original values
                    UniGrid.Pager.DisplayPager = displayPager;
                    UniGrid.ApplyPageSize = applyPageSize;
                    UniGrid.SortDirect = sortDirect;
                    UniGrid.WhereClause = whereClause;
                    UniGrid.Columns = columns;
                    UniGrid.TopN = topN;
                }

                // Initialize macro resolver
                var resolver = MacroResolver;

                resolver.SetNamedSourceData("WhereCondition", newWhere);
                resolver.SetNamedSourceData("OrderBy", sortDirect);
                resolver.SetNamedSourceData("TotalRecords", DataHelper.GetItemsCount(mDataSource));

                if (!string.IsNullOrEmpty(ObjectType))
                {
                    resolver.SetNamedSourceData("ObjectType", ObjectType);
                }
                return mDataSource;
            }
        }


        /// <summary>
        /// Gets all bound fields from UniGrid.
        /// </summary>
        public List<BoundField> BoundFields
        {
            get
            {
                if (mBoundFields != null)
                {
                    return mBoundFields;
                }

                mBoundFields = new List<BoundField>();
                // Explicitly load grid definition
                if (EnsureGridDefinition() && (UniGrid.GridColumns != null))
                {
                    // Store info about columns
                    // Add only column which has definition and can be exported
                    foreach (var col in UniGrid.GridColumns.Columns.Where(c => c.AllowExport))
                    {
                        // Get field
                        BoundField field = col.Field as BoundField;
                        if ((field != null) && field.Visible)
                        {
                            // Add only visible bound fields
                            mBoundFields.Add(field);
                        }
                    }
                }
                return mBoundFields;
            }
        }


        /// <summary>
        /// Gets a list of columns that are available for export.
        /// </summary>
        public List<string> AvailableColumns
        {
            get
            {
                if (mAvailableColumns == null)
                {
                    if (AllColumns != null)
                    {
                        string[] cols = AllColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        mAvailableColumns = cols.Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
                    }
                    if (UniGrid != null)
                    {
                        if ((mAvailableColumns == null) && (UniGrid.InfoObject != null))
                        {
                            mAvailableColumns = UniGrid.InfoObject.ColumnNames.ToList();
                            ObjectTypeInfo typeInfo = UniGrid.InfoObject.TypeInfo;
                            ExcludeBinaryColumn(typeInfo);
                        }

                        if ((mAvailableColumns == null) && !string.IsNullOrEmpty(UniGrid.Query))
                        {
                            string className = UniGrid.Query.Substring(0, UniGrid.Query.LastIndexOf(".", StringComparison.Ordinal));
                            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(className);
                            if (dci != null)
                            {
                                mAvailableColumns = dci.ColumnNames.ToList();
                                ObjectTypeInfo typeInfo = dci.TypeInfo;
                                ExcludeBinaryColumn(typeInfo);
                            }
                        }

                        if ((mAvailableColumns == null) && !string.IsNullOrEmpty(UniGrid.Columns))
                        {
                            string[] cols = UniGrid.Columns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            mAvailableColumns = cols.Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
                        }
                    }

                    if ((mAvailableColumns == null) && (BoundFields != null))
                    {
                        mAvailableColumns = new List<string>();
                        foreach (BoundField field in BoundFields)
                        {
                            mAvailableColumns.Add(field.DataField);
                        }
                    }
                }
                return mAvailableColumns;
            }
        }


        /// <summary>
        /// Gets or sets macro resolver.
        /// </summary>
        public override MacroResolver MacroResolver
        {
            get
            {
                return mMacroResolver ?? (mMacroResolver = MacroResolver.GetInstance());
            }
            set
            {
                mMacroResolver = value;
            }
        }


        /// <summary>
        /// Name of exported file (without extension).
        /// </summary>
        public override string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(mFileName))
                {
                    if (!string.IsNullOrEmpty(ObjectType))
                    {
                        mFileName = ObjectType.Replace(".", "_");
                    }
                    else if (!string.IsNullOrEmpty(UniGrid.ExportFileName))
                    {
                        mFileName = UniGrid.ExportFileName;
                    }
                    else
                    {
                        return base.FileName;
                    }
                }
                return mFileName;
            }
            set
            {
                mFileName = value;
            }
        }


        /// <summary>
        /// Gets original object type from type info of info object used by UniGrid.
        /// </summary>
        protected string ObjectType
        {
            get
            {
                if ((UniGrid != null) && (UniGrid.InfoObject != null))
                {
                    var typeInfo = UniGrid.InfoObject.TypeInfo;
                    return typeInfo.IsListingObjectTypeInfo ? typeInfo.OriginalObjectType : typeInfo.ObjectType;
                }
                return null;
            }
        }


        /// <summary>
        /// Safe version of object type
        /// </summary>
        protected string SafeObjectType
        {
            get
            {
                if (ObjectType != null)
                {
                    return ObjectType.Replace(".", "_");
                }
                return null;
            }
        }


        /// <summary>
        /// Site name used for export template lookup.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a relative path leading to template.
        /// Priorities of lookup (get) are:
        /// 1.	DataExportTemplatePath/SiteName/ObjectType/Template.xlsx
        /// 2.	DataExportTemplatePath/ObjectType/Template.xlsx
        /// 3.	DataExportTemplatePath/SiteName/Template.xlsx
        /// 4.	DataExportTemplatePath/Template.xlsx
        /// </summary>
        public override string DataExportTemplatePath
        {
            get
            {
                if (mDataExportTemplatePath == null)
                {
                    string dataExportTemplateFolder = FileHelper.GetFullFolderPhysicalPath(DataExportTemplateFolder);
                    string priority1 = dataExportTemplateFolder;
                    string priority2 = dataExportTemplateFolder;
                    string priority3 = dataExportTemplateFolder;
                    string priority4 = dataExportTemplateFolder;

                    bool siteNameDefined = !string.IsNullOrEmpty(SiteName);
                    bool objectTypeDefined = !string.IsNullOrEmpty(ObjectType);

                    if (siteNameDefined)
                    {
                        priority1 = DirectoryHelper.CombinePath(priority1, SiteName);
                        priority3 = DirectoryHelper.CombinePath(priority3, SiteName);
                    }

                    if (objectTypeDefined)
                    {
                        priority1 = DirectoryHelper.CombinePath(priority1, SafeObjectType);
                        priority2 = DirectoryHelper.CombinePath(priority2, SafeObjectType);
                    }

                    priority1 = DirectoryHelper.CombinePath(priority1, DEFAULT_TEMPLATE_NAME);
                    priority2 = DirectoryHelper.CombinePath(priority2, DEFAULT_TEMPLATE_NAME);
                    priority3 = DirectoryHelper.CombinePath(priority3, DEFAULT_TEMPLATE_NAME);
                    priority4 = DirectoryHelper.CombinePath(priority4, DEFAULT_TEMPLATE_NAME);

                    if (siteNameDefined && objectTypeDefined && File.Exists(priority1))
                    {
                        mDataExportTemplatePath = priority1;
                    }
                    else if (objectTypeDefined && File.Exists(priority2))
                    {
                        mDataExportTemplatePath = priority2;
                    }
                    else if (siteNameDefined && File.Exists(priority3))
                    {
                        mDataExportTemplatePath = priority3;
                    }
                    else if (File.Exists(priority4))
                    {
                        mDataExportTemplatePath = priority4;
                    }
                }
                return mDataExportTemplatePath;
            }
            set
            {
                mDataExportTemplatePath = value;
            }
        }


        /// <summary>
        /// Allows to explicitly specify all columns that can be retrieved from UniGrid
        /// Comma-separated value e.g. "ItemID, ItemName"
        /// </summary>
        public string AllColumns
        {
            get
            {
                return UniGrid.AllColumns;
            }
        }


        /// <summary>
        /// Defines whether to export raw table data.
        /// </summary>
        public bool ExportRawData
        {
            get;
            set;
        }


        /// <summary>
        /// Additional where condition.
        /// </summary>
        public string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Whether to use current UniGrid's where condition.
        /// </summary>
        public bool UseGridFilter
        {
            get
            {
                return mUseGridFilter;
            }
            set
            {
                mUseGridFilter = value;
            }
        }


        /// <summary>
        /// Order by clause.
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Allows to explicitly set exported columns.
        /// </summary>
        public List<string> Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether to export just current page.
        /// </summary>
        public bool CurrentPageOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Number of exported records (in case CurrentPageOnly is false).
        /// </summary>
        public int Records
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UniGridExportHelper(UniGrid uniGrid)
        {
            UniGrid = uniGrid;
            ExternalDataBound += UniGridExportHelper_ExternalDataBound;
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Performs external data binding on the data
        /// </summary>
        /// <param name="drv">Source data row</param>
        /// <param name="columnIndex">Column index</param>
        protected object UniGridExportHelper_ExternalDataBound(DataRowView drv, int columnIndex)
        {
            var dt = drv.Row.Table;
            if (!ExportRawData)
            {
                object value = null;

                BoundField bf = BoundFields[columnIndex];
                ExtendedBoundField ebf = bf as ExtendedBoundField;

                if ((ebf != null) && (UniGrid != null))
                {
                    if (!string.IsNullOrEmpty(ebf.ExternalSourceName))
                    {
                        // Get data row view
                        bool dataRowViewMode = (ebf.DataField == UniGrid.ALL);

                        // Apply external data bound event to value
                        value = UniGrid.RaiseExternalDataBound(null, ebf.ExternalSourceName, dataRowViewMode ? drv : drv[ebf.DataField]);

                        // If control is returned, render it
                        if (value is Control)
                        {
                            // Keep control unresolved
                        }
                        else if (((value is string) && !string.IsNullOrEmpty(value.ToString())) || ((value == null) && dataRowViewMode))
                        {
                            // Remove HTML tags
                            var noTags = HTMLHelper.StripTags(ValidationHelper.GetString(value, string.Empty), false, true, string.Empty, string.Empty, string.Empty);

                            // Value from UniGrid should always come encoded (if not, the grid has a possibility of XSS), therefore we need to decode the value first to get raw result
                            value = HTMLHelper.HTMLDecode(noTags);
                        }
                    }
                }

                // Fill with default value
                return value ?? drv[bf.DataField];
            }

            return drv[GetColumnNameByIndex(columnIndex, dt.TableName)];
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// List of registered controls to render
        /// </summary>
        private List<DataExportRenderedControl> mRenderedControls;


        /// <summary>
        /// Gets the exported value for the given 
        /// </summary>
        /// <param name="drv">Data row with the source data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="resultDR">DataRow with the result</param>
        protected override object GetExportedValue(DataRowView drv, string columnName, DataRow resultDR)
        {
            var value = base.GetExportedValue(drv, columnName, resultDR);

            if (value is Control)
            {
                mRenderedControls.Add(new DataExportRenderedControl((Control)value, resultDR, columnName));
                value = null;
            }

            return value;
        }


        /// <summary>
        /// Gets the DataSet with the formatted exported data
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="encode">If true, the values in the DataSet will be encoded</param>
        protected override DataSet GetExportedData(DataSet dataSet, bool encode = true)
        {
            // Clear the cached values
            mColumnIndexes = new TwoLevelDictionary<string, string, int>();
            mRenderedControls = new List<DataExportRenderedControl>();

            var ds = base.GetExportedData(dataSet, encode);

            // Render the controls
            foreach (DataExportRenderedControl c in mRenderedControls)
            {
                Control ctrl = c.Control;
                StringBuilder sb = new StringBuilder();
                StringWriter tw = new StringWriter(sb);
                HtmlTextWriter hw = new HtmlTextWriter(tw);

                string value;

                var tr = ctrl as ObjectTransformation;
                if (tr != null)
                {
                    tr.EncodeOutput = false;
                }

                var tag = ctrl as Tag;
                if (tag != null)
                {
                    tag.EncodeOutput = false;
                }

                try
                {
                    ctrl.RenderControl(hw);

                    value = sb.ToString();
                    value = HTMLHelper.StripTags(ValidationHelper.GetString(value, string.Empty), true, true, string.Empty, string.Empty, string.Empty);
                }
                catch (Exception ex)
                {
                    value = "[Error]: " + ex.Message;
                }

                c.DataRow[c.ColumnName] = value;
            }

            return ds;
        }



        /// <summary>
        /// Removes binary column from columns available for export.
        /// </summary>
        /// <param name="typeInfo">TypeInfo that contains binary column specification</param>
        private void ExcludeBinaryColumn(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                return;
            }

            string binaryCol = typeInfo.BinaryColumn;
            if (!string.IsNullOrEmpty(binaryCol) && mAvailableColumns.Contains(binaryCol))
            {
                mAvailableColumns.Remove(binaryCol);
            }
        }


        /// <summary>
        /// Gets columns that should be exported.
        /// </summary>
        protected override List<string> GetExportedColumns(string tableName)
        {
            if (mExportedColumns == null)
            {
                mExportedColumns = new Hashtable();
            }

            if (mExportedColumns.Contains(tableName))
            {
                return (List<string>)mExportedColumns[tableName];
            }

            List<string> columns;

            if ((Columns != null) && (Columns.Count > 0))
            {
                columns = Columns;
            }
            else
            {
                columns = new List<string>();
                foreach (BoundField field in BoundFields)
                {
                    if (string.IsNullOrEmpty(field.DataField))
                    {
                        continue;
                    }

                    if (ExportRawData && (field is ExtendedBoundField))
                    {
                        if ((field.DataField == UniGrid.ALL) || string.IsNullOrEmpty(field.DataField))
                        {
                            continue;
                        }
                    }
                    columns.Add(BoundFields.IndexOf(field).ToString());
                }
            }

            mExportedColumns[tableName] = columns;

            return columns;
        }


        /// <summary>
        /// Ensures loading grid definition.
        /// </summary>
        /// <returns>Whether definition is ensured</returns>
        private bool EnsureGridDefinition()
        {
            if (!definitionLoaded && (UniGrid != null))
            {
                UniGrid.LoadGridDefinition();
                definitionLoaded = true;
            }
            return definitionLoaded;
        }


        /// <summary>
        /// Gets index of a column corresponding to columns actually selected to export.
        /// </summary>
        /// <param name="tableName">Name of a table</param>
        /// <param name="columnName">Name of a column</param>
        /// <returns>Index of a column</returns>
        protected override int GetColumnIndex(string tableName, string columnName)
        {
            var colIndex = mColumnIndexes[tableName, columnName];
            if (colIndex == 0)
            {
                // Adjust column index by 1, so that 0 is considered as not found value
                colIndex = GetColumnIndexInternal(tableName, columnName) + 1;

                mColumnIndexes[tableName, columnName] = colIndex;
            }

            return colIndex - 1;
        }


        /// <summary>
        /// Gets index of a column corresponding to columns actually selected to export.
        /// </summary>
        /// <param name="tableName">Name of a table</param>
        /// <param name="columnName">Name of a column</param>
        /// <returns>Index of a column</returns>
        protected int GetColumnIndexInternal(string tableName, string columnName)
        {
            if (ExportRawData)
            {
                return base.GetColumnIndex(tableName, columnName);
            }

            return ValidationHelper.GetInteger(columnName, 0);
        }



        /// <summary>
        /// Gets column name by given index from table specified by name.
        /// </summary>
        /// <param name="dataTableColumnIndex">Index of a column</param>
        /// <param name="tableName">Name of a table</param>
        /// <returns>Column name corresponding to given index</returns>
        protected string GetColumnNameByIndex(int dataTableColumnIndex, string tableName)
        {
            var columns = GetExportedColumns(tableName);
            if (columns != null)
            {
                string column = columns[dataTableColumnIndex];
                if (column != null)
                {
                    return column;
                }
            }
            throw new InvalidOperationException("Column was not found.");
        }


        /// <summary>
        /// Gets a caption for given column name.
        /// </summary>
        /// <param name="columnIndex">Index of a column</param>
        /// <param name="tableName">Name of a table</param>
        /// <returns>Caption for a column</returns>
        protected override string GetColumnCaptionText(int columnIndex, string tableName)
        {
            if (ExportRawData)
            {
                string columnName = GetColumnNameByIndex(columnIndex, tableName);
                string name = AvailableColumns.FirstOrDefault(ac => ac == columnName);
                if (name != null)
                {
                    return name;
                }
            }
            else
            {
                return BoundFields[columnIndex].HeaderText;
            }

            throw new InvalidOperationException("Column was not found.");
        }

        #endregion
    }
}