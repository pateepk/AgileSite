using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Simple DataClass object.
    /// </summary>
    [Serializable]
    internal class SimpleDataClass : IDataClass, ISerializable
    {
        #region "Variables"

        // Object data.
        private object[] mData;

        // Object original data.
        private object[] mOriginalData;

        // True if there was a change to the ID value (change to the same value is also considered a change)
        private bool mIDWasChanged;

        // Class structure info.
        private ClassStructureInfo mStructureInfo;

        // If true, the object allows updates of only updated fields
        private bool mAllowPartialUpdate = SqlHelper.AllowPartialUpdates;

        private bool mReadOnly;

        #endregion


        #region "Properties"

        /// <summary>
        /// Object class name.
        /// </summary>
        public string ClassName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Object data.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode and value is requested.</exception>
        /// <seealso cref="SetReadOnly"/>
        protected object[] Data
        {
            get
            {
                if (UseOriginalData)
                {
                    return mOriginalData ?? mData;
                }

                return mData;
            }
            set
            {
                ValidateWriteAction(this);

                if (UseOriginalData && (mOriginalData != null))
                {
                    mOriginalData = value;
                }
                
                mData = value;
            }
        }


        /// <summary>
        /// Indicates whether class is in read-only mode.
        /// </summary>
        /// <seealso cref="SetReadOnly"/>
        public bool IsReadOnly
        {
            get
            {
                return mReadOnly;
            }
        }


        /// <summary>
        /// Object ID.
        /// </summary>
        /// <remarks>
        /// Property can be used only for objects with single <see cref="IDColumn"/>.
        /// </remarks>
        public int ID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue(IDColumn), 0);
            }
        }


        /// <summary>
        /// ID column(s).
        /// </summary>
        /// <remarks>Primary key can be identified by more than one column. In this case columns are separated by ';' character.</remarks>
        public string IDColumn
        {
            get
            {
                return StructureInfo.IDColumn;
            }
        }


        /// <summary>
        /// Class structure info.
        /// </summary>
        public ClassStructureInfo StructureInfo
        {
            get
            {
                if (mStructureInfo == null)
                {
                    // Get the class structure info from database
                    mStructureInfo = ClassStructureInfo.GetClassInfo(ClassName);

                    if (mStructureInfo == null)
                    {
                        throw new Exception("[SimpleDataClass.StructureInfo]: Metadata for class '" + ClassName + "' couldn't be found.");
                    }
                }

                return mStructureInfo;
            }
            protected set
            {
                mStructureInfo = value;
            }
        }


        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return StructureInfo.ColumnNames;
            }
        }


        /// <summary>
        /// Number of the object columns.
        /// </summary>
        public int ColumnsCount
        {
            get
            {
                return StructureInfo.ColumnsCount;
            }
        }


        /// <summary>
        /// Column indexer, gets or sets the value in specified column name.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode and value is requested.</exception>
        /// <seealso cref="SetReadOnly"/>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Returns true if the object data changed.
        /// </summary>
        public bool HasChanged
        {
            get
            {
                return DataChanged(null);
            }
        }


        /// <summary>
        /// Returns true if the object is complete (has all columns).
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return StructureInfo.CheckComplete(Data);
            }
        }


        /// <summary>
        /// If true, the object allows partial updates. When ID of the object is set, the object does a full update.
        /// </summary>
        public bool AllowPartialUpdate
        {
            get
            {
                return mAllowPartialUpdate && !mIDWasChanged;
            }
            set
            {
                mAllowPartialUpdate = value;
            }
        }


        /// <summary>
        /// If true, original data is used instead of the actual data for read. Write is modifying original and actual data.
        /// </summary>
        public bool UseOriginalData
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. For inheritance, use constructor SimpleDataClass(dummy)")]
        public SimpleDataClass()
        {
        }


        /// <summary>
        /// Internally visible constructor
        /// </summary>
        /// <param name="dummy">Dummy parameter</param>
        protected SimpleDataClass(bool dummy)
        {
        }


        /// <summary>
        /// Initializes an instance of data class after created by empty constructor
        /// </summary>
        /// <param name="structureInfo">Class structure info</param>
        public virtual void Init(ClassStructureInfo structureInfo)
        {
            ValidateWriteAction(this);

            try
            {
                ClassName = structureInfo.ClassName;
                StructureInfo = structureInfo;

                mData = structureInfo.GetNewData();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create new object of class '" + ClassName + "': " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Object info</param>
        /// <param name="context">Serialization context</param>
        public SimpleDataClass(SerializationInfo info, StreamingContext context)
        {
            mReadOnly = Convert.ToBoolean(ObjectHelper.GetSerializedData(info, "Locked", typeof(bool), false));
            mIDWasChanged = Convert.ToBoolean(ObjectHelper.GetSerializedData(info, "IDWasChanged", typeof(bool), false));
            mData = (object[])ObjectHelper.GetSerializedData(info, "Data", typeof(object[]), null);
            mOriginalData = (object[])ObjectHelper.GetSerializedData(info, "OriginalData", typeof(object[]), null);
            ClassName = Convert.ToString(ObjectHelper.GetSerializedData(info, "ClassName", typeof(string), String.Empty));
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <param name="info">Object info</param>
        /// <param name="context">Serialization context</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Locked", mReadOnly);
            info.AddValue("IDWasChanged", mIDWasChanged);
            info.AddValue("Data", mData);
            info.AddValue("OriginalData", mOriginalData);
            info.AddValue("ClassName", ClassName);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        public virtual void MakeComplete(bool loadFromDb)
        {
            if (loadFromDb)
            {
                if (!IsComplete)
                {
                    // Get the ID
                    int id = ID;
                    if (id <= 0)
                    {
                        throw new Exception("[SimpleDataClass.MakeComplete]: The object is missing the ID field '" + IDColumn + "' value, it cannot be completed.");
                    }

                    // Get the data of the object from DB
                    DataRow dr = SelectData(ClassName, id);
                    if (dr != null)
                    {
                        LoadMissingData(dr);
                    }
                }
            }
            else
            {
                NullMissingData();
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetValue(string columnName, out object value)
        {
            int colIndex = StructureInfo.GetColumnIndex(columnName);
            if (colIndex < 0)
            {
                value = null;
                return false;
            }

            value = Data[colIndex];
            if (SqlHelper.IsMissingOrNull(value))
            {
                value = null;
            }

            return true;
        }


        /// <summary>
        /// Gets the object value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetValue(string columnName)
        {
            int colIndex = StructureInfo.GetColumnIndex(columnName);
            if (colIndex < 0)
            {
                return null;
            }

            object value = Data[colIndex];
            if (SqlHelper.IsMissingOrNull(value))
            {
                value = null;
            }

            return value;
        }


        /// <summary>
        /// Sets the object value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode.</exception>
        /// <seealso cref="SetReadOnly"/>
        public virtual bool SetValue(string columnName, object value)
        {
            ValidateWriteAction(this);

            // Check if the operation is reset of the ID column to zero
            bool idSet = IDColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase);
            bool idReset = (idSet && (ValidationHelper.GetInteger(value, 0) <= 0));

            if (!idReset)
            {
                // Ensure original data to remember changes
                EnsureOriginalData();
            }

            // Set the value
            var result = SetValueInternal(columnName, value);

            if (idReset)
            {
                // When object ID is reset to 0, reset the state of changes (the object no longer relates to an existing object, but is considered a new one)
                ResetChanges();
            }

            if (idSet)
            {
                // Mark ID as changed
                mIDWasChanged = true;
            }

            return result;
        }


        /// <summary>
        /// Sets the object value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        private bool SetValueInternal(string columnName, object value)
        {
            // Convert DBNull.Value to null
            if (value == DBNull.Value)
            {
                value = null;
            }

            // Get column index
            int colIndex = StructureInfo.GetColumnIndex(columnName);

            // Set the value
            if (colIndex >= 0)
            {
                Data[colIndex] = value;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets the DataSet from the object data.
        /// </summary>
        public virtual DataSet GetDataSet()
        {
            DataSet ds = StructureInfo.GetNewDataSet();
            DataTable dt = ds.Tables[0];

            // Allow null in all columns
            foreach (DataColumn dc in dt.Columns)
            {
                dc.AllowDBNull = true;
                dc.ReadOnly = false;
            }

            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            // Fill in the data
            int count = StructureInfo.ColumnsCount;
            for (int i = 0; i < count; i++)
            {
                // Take only values that are not missing
                object value = Data[i];
                if (!SqlHelper.IsMissing(value))
                {
                    dr[i] = DataHelper.GetDBNull(value);
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets the class Data row.
        /// </summary>
        public virtual DataRow GetDataRow()
        {
            return GetDataSet().Tables[0].Rows[0];
        }


        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="loadNullValues">If true, null values are loaded to the object</param>
        public virtual void LoadData(IDataContainer data, bool loadNullValues = true)
        {
            if (data == null)
            {
                return;
            }

            var colNames = ColumnNames;

            // Fill in the data
            int count = StructureInfo.ColumnsCount;
            for (int i = 0; i < count; i++)
            {
                string colName = colNames[i];

                // Load the value
                if (data.ContainsColumn(colName))
                {
                    object value = DataHelper.GetNull(data.GetValue(colName));

                    // Only load non-null values
                    if (loadNullValues || (value != null))
                    {
                        SetData(i, value);
                    }
                }
                else
                {
                    // Only set to missing value if previously not set
                    if (Data[i] == null)
                    {
                        SetData(i, SqlHelper.MISSING_VALUE);
                    }
                }
            }

            if (ContainsExternalData(data))
            {
                // Do not allow partial updates if container contains data from external source
                AllowPartialUpdate = false;
            }
        }


        /// <summary>
        /// Returns true if the given data container contains data from external source
        /// </summary>
        /// <param name="data">Data container to check</param>
        private bool ContainsExternalData(IDataContainer data)
        {
            var drc = data as DataRowContainer;
            var dr = drc?.DataRow;

            if (dr != null)
            {
                if (dr.Table.ContainsExternalData())
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Loads data row of the current class with specified value of column (typically ID).
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="primaryKeyValue">Primary key value</param>
        protected virtual DataRow SelectData(string className, int primaryKeyValue)
        {
            var q = GetDataQuery(null).WhereEquals(IDColumn, primaryKeyValue);

            // Get the data
            DataSet ds = q.Result;

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0];
            }

            return null;
        }


        /// <summary>
        /// Loads the object data by the given primary key value
        /// </summary>
        /// <param name="primaryKeyValue">Primary key value</param>
        public virtual void LoadData(int primaryKeyValue)
        {
            var dr = SelectData(ClassName, primaryKeyValue);
            LoadData(dr.AsDataContainer());
        }


        /// <summary>
        /// Locks the data class as read-only.
        /// </summary>
        /// <seealso cref="SetValue(string, object)"/>
        public void SetReadOnly()
        {
            mReadOnly = true;
        }


        /// <summary>
        /// Loads the null values into the missing fields.
        /// </summary>
        public virtual void NullMissingData()
        {
            // Fill in the data
            int count = StructureInfo.ColumnsCount;
            var data = Data;

            for (int i = 0; i < count; i++)
            {
                // Load only the missing values
                if (SqlHelper.IsMissing(data[i]))
                {
                    SetData(i, null);
                }
            }
        }


        /// <summary>
        /// Loads the missing object data from given DataRow.
        /// </summary>
        /// <param name="dr">Source data</param>
        public virtual void LoadMissingData(DataRow dr)
        {
            if (dr == null)
            {
                return;
            }

            DataTable dt = dr.Table;

            // Fill in the data
            int count = StructureInfo.ColumnsCount;
            var data = Data;
            var cols = dt.Columns;

            for (int i = 0; i < count; i++)
            {
                // Load only the missing values
                if (SqlHelper.IsMissing(data[i]))
                {
                    string colName = ColumnNames[i];

                    // Load the value
                    int colIndex = cols.IndexOf(colName);
                    if (colIndex >= 0)
                    {
                        object value = DataHelper.GetNull(dr[colIndex]);
                        SetData(i, value);
                    }
                }
            }
        }


        /// <summary>
        /// Deletes current record.
        /// </summary>
        /// <param name="preserveData">If true, object data are preserved (it os possible to manipulate with the object further)</param>
        /// <remarks>The method assumes that the primary key is the first column in the DataRow.</remarks>
        public virtual void Delete(bool preserveData = false)
        {
            DataQuery q;

            if (QueryInfoProvider.QueryIsExplicitlyDefined(ClassName, QueryName.DELETE))
            {
                // Support for specific delete query
                q = GetDataQuery(QueryName.DELETE);

                q.Parameters = GetDeleteParameters();
            }
            else
            {
                // Use general delete query for deletion
                q = GetDataQuery(QueryName.GENERALDELETE);

                AddIDWhere(q);
            }

            // Execute the delete query
            q.Execute();
        }


        /// <summary>
        /// Gets the parameters for the delete query
        /// </summary>
        private QueryDataParameters GetDeleteParameters()
        {
            // Prepare the parameters
            QueryDataParameters parameters;

            string[] idColumns = IDColumn.Split(';');
            if (idColumns.Length > 1)
            {
                // Multiple keys, real name parameters
                parameters = new QueryDataParameters();

                foreach (string column in idColumns)
                {
                    // Prepare the ID
                    object id = GetValue(column);
                    if (id == null)
                    {
                        throw new Exception("[SimpleDataClass.Delete]: The object is missing the ID column '" + column + "' value, it cannot be deleted.");
                    }

                    parameters.Add("@" + column, DataHelper.GetDBNull(id));
                }
            }
            else
            {
                // Prepare the ID
                object id = GetValue(IDColumn);
                if (SqlHelper.IsMissing(id))
                {
                    throw new Exception("[SimpleDataClass.Delete]: The object is missing the ID value, it cannot be deleted.");
                }

                // Single key, only ID parameter
                parameters = new QueryDataParameters();
                parameters.Add("@ID", DataHelper.GetDBNull(id));
            }

            return parameters;
        }


        /// <summary>
        /// Adds ID where condition for this object 
        /// </summary>
        /// <param name="q">Data query to manipulate</param>
        private void AddIDWhere(DataQuery q)
        {
            // Prepare the parameters
            var idColumns = IDColumn.Split(';');
            if (idColumns.Length > 1)
            {
                // Add condition for all ID columns
                foreach (string column in idColumns)
                {
                    AddIDWhere(q, column);
                }
            }
            else
            {
                // Add condition for ID column
                AddIDWhere(q, IDColumn);
            }
        }


        /// <summary> 
        /// Inserts current record in the database.
        /// </summary>
        /// <param name="initId">If true, ID of the new object is initialized.</param>
        public virtual void Insert(bool initId = true)
        {
            DataQuery q;

            if (QueryInfoProvider.QueryIsExplicitlyDefined(ClassName, QueryName.INSERT))
            {
                // Support for specific insert query
                q = GetDataQuery(QueryName.INSERT);

                q.Parameters = StructureInfo.ConvertDataToParams(Data, true, true);
            }
            else
            {
                q = GetDataQuery(QueryName.GENERALINSERT);

                // Include object data
                if (!IncludeData(q, true, null, SqlHelper.INSERT_FORMAT))
                {
                    throw new NotSupportedException("[SimpleDataClass.Insert]: Unable to insert object of type '" + ClassName + "'. This object must contain at least one column value.");
                }
            }

            var result = q.Execute();

            // Update the ID of the record
            if (initId)
            {
                UpdateID(result);
            }
        }


        /// <summary>
        /// Updates the ID of the record from the given query result
        /// </summary>
        /// <param name="result">Query result</param>
        private void UpdateID(DataSet result)
        {
            if (result != null)
            {
                // Get the ID of the new record
                int newId = ValidationHelper.GetInteger(DataHelper.GetScalarValue(result), 0);
                if (newId > 0)
                {
                    SetValue(IDColumn, newId);
                }
            }
        }


        /// <summary>
        /// Updates current record.
        /// </summary>
        public virtual void Update()
        {
            DataQuery q;

            var allowPartialUpdate = AllowPartialUpdate;

            if (QueryInfoProvider.QueryIsExplicitlyDefined(ClassName, QueryName.UPDATE))
            {
                // Support for specific update query
                q = GetDataQuery(QueryName.UPDATE);

                q.Parameters = StructureInfo.ConvertDataToParams(Data, allowPartialUpdate, false);
            }
            else
            {
                q = GetDataQuery(QueryName.GENERALUPDATE);

                // Include object data, only execute the query if some data is included
                if (!IncludeData(q, false, allowPartialUpdate ? (Func<string, bool>)ItemChanged : null, SqlHelper.UPDATE_FORMAT))
                {
                    return;
                }

                // Add ID where condition
                AddIDWhere(q);
            }

            q.Execute();
        }


        /// <summary>
        /// Updates or inserts the current record.
        /// </summary>
        /// <param name="existingWhere">Where condition for the existing object</param>
        public virtual void Upsert(WhereCondition existingWhere)
        {
            var q = GetDataQuery(QueryName.GENERALUPSERT);

            // Include object data, only execute the query if some data is included
            if (IncludeData(q, true, null, SqlHelper.INSERT_FORMAT, SqlHelper.UPDATE_FORMAT))
            {
                // Add where condition for existing object
                if (existingWhere != null)
                {
                    q.Where(existingWhere);
                }
                else
                {
                    AddIDWhere(q);
                }

                var result = q.Execute();

                UpdateID(result);
            }
        }


        /// <summary>
        /// Returns true if the object is empty.
        /// </summary>
        public virtual bool IsEmpty()
        {
            return (Data == null);
        }


        /// <summary>
        /// Gets the data query for this class
        /// </summary>
        /// <param name="queryName">Query name</param>
        private DataQuery GetDataQuery(string queryName)
        {
            return new DataQuery(ClassName, queryName);
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool ContainsColumn(string columnName)
        {
            return StructureInfo.ContainsColumn(columnName);
        }


        /// <summary>
        /// Includes the object data to the query. Does not include primary key columns. Returns true if some data was included.
        /// </summary>
        /// <param name="q">Data query</param>
        /// <param name="includeColumns">If true, the columns are included to the query</param>
        /// <param name="valueFormats">Formats for the value expression, {0} represents the column name, {1} represents the parameter. If null, only value parameter is included</param>
        /// <param name="condition">Column condition</param>
        private bool IncludeData(DataQuery q, bool includeColumns, Func<string, bool> condition, params string[] valueFormats)
        {
            // Prepare the parameters
            var csi = StructureInfo;

            int count = ColumnsCount;
            var parameters = q.EnsureParameters();

            var included = false;

            // Prepare values array
            if (valueFormats == null)
            {
                valueFormats = new string[] { null };
            }

            int valuesCount = valueFormats.Length;
            var valueBuilders = new StringBuilder[valuesCount];

            for (int valuesIndex = 0; valuesIndex < valuesCount; valuesIndex++)
            {
                valueBuilders[valuesIndex] = new StringBuilder();
            }

            // Start with no columns
            if (includeColumns)
            {
                q.Columns(SqlHelper.NO_COLUMNS);
            }

            for (int colIndex = 0; colIndex < count; colIndex++)
            {
                var col = csi.ColumnDefinitions[colIndex];
                string columnName = col.ColumnName;

                // Add all columns except for ID column or filtered columns
                if (((condition == null) || condition(columnName)) && !IDColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    // Check for missing value
                    object value = Data[colIndex];

                    // Completely skip missing values
                    if (!SqlHelper.IsMissing(value))
                    {
                        // Add the parameter
                        var par = parameters.AddUnique(columnName, value);

                        par.Type = col.ColumnType;

                        included = true;

                        // Include the column
                        if (includeColumns)
                        {
                            q.AddColumn(new QueryColumn(columnName));
                        }

                        // Include the value
                        var parameterName = par.Name;

                        // Append to all values
                        for (int valuesIndex = 0; valuesIndex < valuesCount; valuesIndex++)
                        {
                            // Append specific value
                            var valueFormat = valueFormats[valuesIndex];
                            var values = valueBuilders[valuesIndex];

                            if (values.Length > 0)
                            {
                                values.Append(", ");
                            }

                            var val = String.Format(valueFormat, columnName, parameterName);

                            values.Append(val);
                        }
                    }
                }
            }

            // Add values to the query
            for (int valuesIndex = 0; valuesIndex < valuesCount; valuesIndex++)
            {
                // Append specific value
                var values = valueBuilders[valuesIndex];
                var macroName = "VALUES";

                if (valuesIndex > 0)
                {
                    macroName += valuesIndex;
                }

                parameters.AddMacro(macroName, values.ToString(), true);
            }

            return included;
        }


        /// <summary>
        /// Adds the ID where condition to the query
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="columnName">Column name</param>
        private void AddIDWhere(DataQuery q, string columnName)
        {
            // Prepare the ID
            object id = GetValue(columnName);
            if (id == null)
            {
                throw new Exception("[SimpleDataClass.AddIDWhere]: The object is missing the ID column '" + columnName + "' value, the operation cannot be performed.");
            }

            q.WhereEquals(columnName, id);
        }

        #endregion


        #region "Change management"

        /// <summary>
        /// Reverts the object changes to the original values.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode.</exception>
        /// <seealso cref="SetReadOnly"/>
        public virtual void RevertChanges()
        {
            ValidateWriteAction(this);

            if (mOriginalData != null)
            {
                mData = mOriginalData;
                mOriginalData = null;
            }
        }


        /// <summary>
        /// Resets the object changes (original values) and keeps the new values.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode.</exception>
        /// <remarks>Reset action can be suppressed by <see cref="CMSActionContext.ResetChanges"/> option.</remarks>
        /// <seealso cref="SetReadOnly"/>
        public virtual void ResetChanges()
        {
            if (!CMSActionContext.CurrentResetChanges)
            {
                return;
            }

            ValidateWriteAction(this);

            mOriginalData = null;
 
            mIDWasChanged = false;
        }


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool ItemChanged(string columnName)
        {
            if (mOriginalData == null)
            {
                return false;
            }

            // Get the column index
            int columnIndex = StructureInfo.GetColumnIndex(columnName);
            if (columnIndex >= 0)
            {
                // Check if the data changed
                return !SqlHelper.ObjectsEqual(mOriginalData[columnIndex], mData[columnIndex]);
            }

            return false;
        }


        /// <summary>
        /// Returns list of column names whose values were changed.
        /// </summary>
        /// <returns>List of column names</returns>
        public virtual List<string> ChangedColumns()
        {
            return ColumnNames.Where(ItemChanged).ToList();
        }


        /// <summary>
        /// Returns the original value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetOriginalValue(string columnName)
        {
            int columnIndex = StructureInfo.GetColumnIndex(columnName);
            if (columnIndex >= 0)
            {
                return (mOriginalData == null) ? mData[columnIndex] : mOriginalData[columnIndex];

            }

            return null;
        }


        /// <summary>
        /// Ensures the object original data to store the object state.
        /// </summary>
        protected virtual void EnsureOriginalData()
        {
            if ((mOriginalData != null) || UseOriginalData)
            {
                return;
            }

            // Clone the data object
            mOriginalData = (object[])mData.Clone();
        }


        /// <summary>
        /// Sets the specific data.
        /// </summary>
        /// <param name="columnIndex">Column index</param>
        /// <param name="value">Value to set</param>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode.</exception>
        /// <seealso cref="SetReadOnly"/>
        protected virtual void SetData(int columnIndex, object value)
        {
            ValidateWriteAction(this);

            if (value == DBNull.Value)
            {
                value = null;
            }

            if ((value != null) && !SqlHelper.IsMissing(value))
            {
                // Ensure the value type
                Type colType = StructureInfo.ColumnDefinitions[columnIndex].ColumnType;
                if (!colType.IsInstanceOfType(value))
                {
                    if (colType == typeof(Guid))
                    {
                        // GUID conversion
                        value = ValidationHelper.GetGuid(value, Guid.Empty);
                    }
                    else
                    {
                        // Other types
                        value = Convert.ChangeType(value, colType, CultureHelper.EnglishCulture);
                    }
                }
            }

            mData[columnIndex] = value;
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        /// <param name="excludedColumns">List of columns excluded from change (separated by ';')</param>
        public virtual bool DataChanged(string excludedColumns)
        {
            if (mOriginalData == null)
            {
                return false;
            }

            var excludedColumnsCollection = excludedColumns?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (excludedColumnsCollection?.Length > 0)
            {
                var excludedColumnSet = new HashSet<string>(excludedColumnsCollection, StringComparer.InvariantCultureIgnoreCase);

                return ColumnNames.Where(column => !excludedColumnSet.Contains(column)).Any(ItemChanged);
            }

            return ColumnNames.Any(ItemChanged);
        }

        #endregion


        #region "Copy methods"

        /// <summary>
        /// Copies the object data to another object.
        /// </summary>
        /// <param name="target">Target for the data.</param>
        /// <remarks>Copying is allowed only for objects with same class name.</remarks>
        public virtual void CopyDataTo(IDataClass target)
        {
            // If either source data not specified, or destination data not specified, do not perform the copying
            if (target == null)
            {
                return;
            }

            ValidateWriteAction(target);

            bool classMatch = CheckTargetClass(target, false);

            var srcData = Data;

            if (classMatch)
            {
                var dataClass = (SimpleDataClass)target;

                // Same classes - Direct copy of the data
                var destData = dataClass.Data;
                
                int count = StructureInfo.ColumnsCount;

                for (int i = 0; i < count; i++)
                {
                    destData[i] = srcData[i];
                }
            }
            else
            {
                // Other classes - Copy through column names and SetValue
                int index = 0;

                foreach (string colName in ColumnNames)
                {
                    // Copy the data
                    if (target.ContainsColumn(colName))
                    {
                        target.SetValue(colName, srcData[index]);
                    }

                    index++;
                }
            }
        }


        /// <summary>
        /// Copies the original data of the DataClass object to another.
        /// </summary>
        /// <param name="target">Destination object.</param>
        /// <remarks>Copying is allowed only for objects of the same type and class name.</remarks>
        public virtual void CopyOriginalDataTo(IDataClass target)
        {
            // If either source data not specified, or destination data not specified, do not perform the copying
            if (target == null)
            {
                return;
            }

            ValidateWriteAction(target);

            // Check the target class
            CheckTargetClass(target, true);

            var destData = (SimpleDataClass)target;

            if (mOriginalData == null)
            {
                destData.mOriginalData = null;

                return;
            }

            // Ensure the original data collection
            destData.EnsureOriginalData();

            var destOriginalData = destData.mOriginalData;
            if (destOriginalData != null)
            {
                var destStructureInfo = destData.StructureInfo;

                int index = 0;

                foreach (string colName in ColumnNames)
                {
                    // Copy the data
                    int destIndex = destStructureInfo.GetColumnIndex(colName);
                    if (destIndex >= 0)
                    {
                        destOriginalData[destIndex] = mOriginalData[index];
                    }

                    index++;
                }
            }
        }


        /// <summary>
        /// Checks if target data class matches this data class
        /// </summary>
        /// <param name="target">Target data class</param>
        /// <param name="throwException">If true, the exception is thrown if classes don't match</param>
        /// <exception cref="Exception">Thrown when <paramref name="throwException"/> is set and class name or type is not equal with current instance.</exception>
        protected virtual bool CheckTargetClass(IDataClass target, bool throwException)
        {
            if (!string.Equals(target.ClassName, ClassName, StringComparison.OrdinalIgnoreCase))
            {
                if (throwException)
                {
                    throw new Exception("[SimpleDataClass.CheckTargetClass]: This operation is only supported between the classes of the same type.");
                }

                return false;
            }

            if (target is SimpleDataClass)
            {
                return true;
            }

            if (throwException)
            {
                throw new Exception("[SimpleDataClass.CheckTargetClass]: Target data class must be of the same type as source.");
            }

            return false;
        }


        /// <summary>
        /// Validates write action against read only flag.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when object is in read-only mode.</exception>
        /// <seealso cref="SetReadOnly"/>
        private static void ValidateWriteAction(IDataClass dataClass)
        {
            if (dataClass.IsReadOnly)
            {
                throw new NotSupportedException("[SimpleDataClass] This object is read-only and cannot be modified.");
            }
        }

        #endregion
    }
}