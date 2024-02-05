using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    using ClassTable = StringSafeDictionary<ClassStructureInfo>;
    

    /// <summary>
    /// Class information.
    /// </summary>
    [Serializable]
    public class ClassStructureInfo : ISerializable
    {
        #region "Variables"
        
        // Regular expression for the schema columns list.
        private static readonly CMSRegex RegExSchemaColumns = new CMSRegex("<xs:element name=\"([\\w\\.]+)\"", true);

        // Regular expression for the schema types list.
        private static readonly CMSRegex RegExSchemaTypes = new CMSRegex("type=\"([0-9A-Z:]+)\"|base=\"([0-9A-Z:]+)\"[^<]*<xs:maxLength value=\"(\\d+)\"", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Class structure infos [className] -> [ClassInfo]
        private static readonly CMSStatic<ClassTable> mClassInfos = new CMSStatic<ClassTable>(() => new ClassTable());

        // List of column names.
        private List<string> mColumnNames;

        // ID column name(s)
        private string mIDColumn;

        #endregion


        #region "Properties"

        /// <summary>
        /// Class structure infos [className] -> [ClassInfo]
        /// </summary>
        private static ClassTable ClassInfos
        {
            get
            {
                return mClassInfos;
            }
        }


        /// <summary>
        /// Columns count.
        /// </summary>
        public int ColumnsCount
        {
            get
            {
                return ColumnDefinitions.Count;
            }
        }


        /// <summary>
        /// Column definitions.
        /// </summary>
        public List<ColumnDefinition> ColumnDefinitions
        {
            get;
            protected set;
        }


        /// <summary>
        /// Column indexes dictionary [columnName] -> [columnIndex]
        /// </summary>
        protected StringSafeDictionary<int> ColumnIndexes
        {
            get;
            set;
        }


        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return mColumnNames ?? (mColumnNames = GetColumnNames());
            }
        }


        /// <summary>
        /// List of binary column names
        /// </summary>
        public List<string> BinaryColumns
        {
            get;
            protected set;
        }


        /// <summary>
        /// List of string column names
        /// </summary>
        public List<string> StringColumns
        {
            get;
            protected set;
        }


        /// <summary>
        /// Class name.
        /// </summary>
        public string ClassName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Table name
        /// </summary>
        public string TableName
        {
            get;
            set;
        }


        /// <summary>
        /// ID column name(s).
        /// </summary>
        /// <remarks>Primary key can be identified by more than one column. In this case columns are separated by ';' character.</remarks>
        public string IDColumn
        {
            get
            {
                return mIDColumn ?? (mIDColumn = GetIDColumn());
            }
            protected internal set
            {
                mIDColumn = value;
            }
        }


        /// <summary>
        /// Returns true if this class has some binary columns
        /// </summary>
        public bool HasBinaryColumns
        {
            get
            {
                return BinaryColumns.Count > 0;
            }
        }
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ClassStructureInfo()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="xmlSchema">Class XML schema</param>
        /// <param name="tableName">Table name</param>
        public ClassStructureInfo(string className, string xmlSchema, string tableName)
            : this()
        {
            ClassName = className;
            TableName = tableName;

            LoadColumns(xmlSchema);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ClassStructureInfo(SerializationInfo info, StreamingContext context)
        {
            StringColumns = new List<string>();
            BinaryColumns = new List<string>();

            ColumnDefinitions = (List<ColumnDefinition>)info.GetValue("ColumnDefinitions", typeof(List<ColumnDefinition>));
            ColumnIndexes = (StringSafeDictionary<int>)info.GetValue("ColumnIndexes", typeof(StringSafeDictionary<int>));

            ClassName = info.GetString("ClassName");
            IDColumn = info.GetString("IDColumn");
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ColumnDefinitions", ColumnDefinitions);
            info.AddValue("ColumnIndexes", ColumnIndexes);
            info.AddValue("ClassName", ClassName);
            info.AddValue("IDColumn", IDColumn);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets underlying database table's indexes. Returns null when information regarding indexes is not available.
        /// </summary>
        public ITableIndexes GetTableIndexes()
        {
            var dci = DataClassInfoProvider.GetDataClassInfo(ClassName);
            if (dci == null)
            {
                return null;
            }

            var connectionString = !String.IsNullOrEmpty(dci.ClassConnectionString) ? dci.ClassConnectionString : null;
            var tm = new TableManager(connectionString);

            var tableIndexes = tm.GetTableIndexes(dci.ClassTableName);

            return tableIndexes;
        }


        /// <summary>
        /// Gets the ID column name
        /// </summary>
        private string GetIDColumn()
        {
            string idColumn = null;

            // Load from the class schema
            var ci = DataClassInfoProvider.GetDataClassInfo(ClassName);
            if ((ci != null) && !String.IsNullOrEmpty(ci.ClassTableName))
            {
                // Get primary key name by table manager
                // We can't use primary key from XML schema because in some cases DataAdapter retrieves unique clustered key instead of the actual primary key
                var tm = new TableManager(ci.ClassConnectionString);

                idColumn = tm.GetTablePKName(ci.ClassTableName);
            }

            return idColumn ?? "";
        }


        /// <summary>
        /// Gets the list of column names
        /// </summary>
        private List<string> GetColumnNames()
        {
            return ColumnDefinitions.Select(columnDefinition => columnDefinition.ColumnName).ToList();
        }


        /// <summary>
        /// Gets new data array for object of the given class.
        /// </summary>
        public object[] GetNewData()
        {
            return new object[ColumnsCount];
        }


        /// <summary>
        /// Sets all the items in the data to missing values.
        /// </summary>
        /// <param name="data">Data to set</param>
        public void SetAllMissing(object[] data)
        {
            int count = ColumnsCount;
            for (int i = 0; i < count; i++)
            {
                data[i] = SqlHelper.MISSING_VALUE;
            }
        }


        /// <summary>
        /// Checks whether the given data is complete (has all columns set to some value).
        /// </summary>
        /// <param name="data">Data to check</param>
        public bool CheckComplete(object[] data)
        {
            int count = ColumnsCount;
            for (int i = 0; i < count; i++)
            {
                if (data[i] == SqlHelper.MISSING_VALUE)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Gets new data structure for class data as a DataSet.
        /// </summary>
        public virtual DataSet GetNewDataSet()
        {
            var ds = new DataSet();
            
            // Set all the columns to allow nulls
            var dt = new DataTable(TableName);

            foreach (var col in ColumnDefinitions)
            {
                dt.Columns.Add(col.ColumnName, col.ColumnType);
            }

            ds.Tables.Add(dt);

            return ds;
        }


        /// <summary>
        /// Gets the column index.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <returns>Returns column index if exists; otherwise -1.</returns>
        public int GetColumnIndex(string columnName)
        {
            int colIndex;
            if (ColumnIndexes.TryGetValue(columnName, out colIndex))
            {
                return colIndex;
            }

            return -1;
        }


        /// <summary>
        /// Gets the column type.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public Type GetColumnType(string columnName)
        {
            int colIndex = GetColumnIndex(columnName);
            if (colIndex >= 0)
            {
                return ColumnDefinitions[colIndex].ColumnType;
            }

            return null;
        }


        /// <summary>
        /// Returns true if the data class has the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return (GetColumnIndex(columnName) >= 0);
        }


        /// <summary>
        /// Returns the object data converted to the query parameters.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="allowMissing">If true, the process allows missing values</param>
        /// <param name="nullForMissing">If true, missing values are replaced by NULLs</param>
        public QueryDataParameters ConvertDataToParams(object[] data, bool allowMissing, bool nullForMissing)
        {
            int count = ColumnsCount;
            var parameters = new QueryDataParameters();

            for (int i = 0; i < count; i++)
            {
                var col = ColumnDefinitions[i];
                string columnName = col.ColumnName;

                // Check for missing value
                object value = data[i];
                
                // Handle missing values
                if (SqlHelper.IsMissing(value))
                {
                    if (!allowMissing)
                    {
                        // Object must be complete in order to do this
                        throw new Exception("The value for field '" + columnName + "' is missing. You need to initialize the field before executing this operation.");
                    }
                    
                    if (nullForMissing)
                    {
                        // Use null values
                        value = null;
                    }
                }

                // Add the parameter
                parameters.Add("@" + columnName, DataHelper.GetDBNull(value), col.ColumnType);
            }

            return parameters;
        }
        
        
        /// <summary>
        /// Loads the columns from given XML schema.
        /// </summary>
        /// <param name="xmlSchema">XML schema</param>
        private void LoadColumns(string xmlSchema)
        {
            InitCollections();

            // No XML schema - no columns
            if (String.IsNullOrEmpty(xmlSchema))
            {
                return;
            }

            // Match the column names
            var columns = RegExSchemaColumns.Matches(xmlSchema);
            var types = RegExSchemaTypes.Matches(xmlSchema);

            if (columns.Count > 2)
            {
                int count = columns.Count;

                int index = 0;

                // Distribute the columns
                for (int i = 2; i < count; i++)
                {
                    string colName = columns[i].Groups[1].ToString();

                    // Get the type
                    Match type = types[index];
                    var schemaType = type.Groups[1].ToString();

                    var colType = DataTypeManager.GetSystemType(TypeEnum.Schema, schemaType);

                    RegisterColumn(colName, colType);

                    index++;
                }
            }
        }


        /// <summary>
        /// Registers the given column within the structure info
        /// </summary>
        /// <param name="colName">Column name</param>
        /// <param name="colType">Column type</param>
        protected void RegisterColumn(string colName, Type colType)
        {
            int index = ColumnDefinitions.Count;

            ColumnDefinitions.Add(new ColumnDefinition(colName, colType));

            if (colType == typeof (string))
            {
                StringColumns.Add(colName);
            }
            else if (colType == typeof (byte[]))
            {
                BinaryColumns.Add(colName);
            }
            
            ColumnIndexes[colName] = index;
        }


        /// <summary>
        /// Initializes the inner collections for structure
        /// </summary>
        protected void InitCollections()
        {
            ColumnIndexes = new StringSafeDictionary<int>();
            ColumnDefinitions = new List<ColumnDefinition>();
            BinaryColumns = new List<string>();
            StringColumns = new List<string>();
        }


        /// <summary>
        /// Gets the list of columns of the given type
        /// </summary>
        /// <param name="type">Type to match</param>
        public List<string> GetColumns(Type type)
        {
            var q = from c in ColumnDefinitions where c.ColumnType == type select c.ColumnName;

            return q.ToList();
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Returns the class info for specified class.
        /// </summary>
        /// <param name="className">Class name</param>
        public static ClassStructureInfo GetClassInfo(string className)
        {
            if (String.IsNullOrEmpty(className))
            {
                return null;
            }

            // Try to get from hashtable
            ClassStructureInfo result;
            if (!ClassInfos.TryGetValue(className, out result))
            {
                result = DataClassInfoProvider.GetClassStructureInfoFromDB(className);

                ClassInfos[className] = result;
            }

            return result;
        }


        /// <summary>
        /// Clear the class infos and properties lists of all object types.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        internal static void Clear(bool logTask)
        {
            ClassInfos.Clear();

            BaseInfo.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(DataTaskType.ClearClassStructureInfos);
            }
        }


        /// <summary>
        /// Removes the specified class structure definition.
        /// </summary>
        /// <param name="className">ClassName to remove</param>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        public static void Remove(string className, bool logTask)
        {
            ClassInfos.Remove(className);

            if (logTask)
            {
                WebFarmHelper.CreateTask(DataTaskType.RemoveClassStructureInfo, null, className);
            }
        }


        /// <summary>
        /// Gets the columns for the listed class names
        /// </summary>
        /// <param name="classNames">List of class names to get</param>
        public static List<string> GetColumns(params string[] classNames)
        {
            var allColumns = new List<string>();

            foreach (var className in classNames)
            {
                var csi = GetClassInfo(className);
                if (csi != null)
                {
                    var columns = csi.ColumnDefinitions.Select(column => column.ColumnName);
                    allColumns.AddRange(columns);
                }
            }

            return allColumns;
        }


        /// <summary>
        /// Combines class structure definitions to the one
        /// </summary>
        /// <param name="source">Source of the data (database table or view)</param>
        /// <param name="structures">List of structure definitions to combine</param>
        public static ClassStructureInfo Combine(string source, params ClassStructureInfo[] structures)
        {
            var combined = new ClassStructureInfo(null, null, source);
            foreach (var structure in structures)
            {
                structure.ColumnDefinitions.ForEach(d => combined.RegisterColumn(d.ColumnName, d.ColumnType));
            }

            return combined;
        }

        #endregion


        /// <summary>
        /// Gets the XML schema of the DataSet represented by this structure
        /// </summary>
        public string GetXmlSchema()
        {
            var ds = GetNewDataSet();

            return ds.GetXmlSchema();
        }
    }
}