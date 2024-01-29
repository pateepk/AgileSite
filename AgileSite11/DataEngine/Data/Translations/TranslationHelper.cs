using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    using ConversionsDictionary = SafeDictionary<int, TranslationHelper.ConversionIDs>;


    /// <summary>
    /// Class to provide objects translation interface ID - CodeName.
    /// </summary>
    public class TranslationHelper
    {
        #region "Internal classes"

        /// <summary>
        /// Used to cache indexes of data table columns
        /// </summary>
        private class ColumnsIndexes
        {
            public int SiteIDIndex
            {
                get;
                set;
            }


            public int CodeNameIndex
            {
                get;
                set;
            }


            public int CodeNameIndex2
            {
                get;
                set;
            }


            public int IDIndex
            {
                get;
                set;
            }


            public int ParentIDIndex
            {
                get;
                set;
            }


            public int GroupIDIndex
            {
                get;
                set;
            }


            public int GuidIndex
            {
                get;
                set;
            }


            public Dictionary<string, int> AdditionalFieldsIndex
            {
                get;
                set;
            }
        }


        /// <summary>
        /// Internal structure for wrapping ID, Guid and additional fields from database 
        /// </summary>
        private class ObjectDatabaseWrapper
        {
            private static readonly ObjectDatabaseWrapper mEmpty = new ObjectDatabaseWrapper()
            {
                ID = 0,
                Guid = Guid.Empty
            };


            internal static ObjectDatabaseWrapper EMPTY
            {
                get
                {
                    return mEmpty;
                }
            }


            public int ID
            {
                get;
                set;
            }


            public Guid Guid
            {
                get;
                set;
            }


            public string CodeName
            {
                get;
                set;
            }


            public Dictionary<string, object> AdditionalFields
            {
                get;
                set;
            }
        }


        /// <summary>
        /// Conversion IDs container
        /// </summary>
        internal class ConversionIDs
        {
            /// <summary>
            /// ID
            /// </summary>
            public int ID;


            /// <summary>
            /// Group ID
            /// </summary>
            public int GroupID;


            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="id">ID</param>
            /// <param name="groupId">Group ID</param>
            public ConversionIDs(int id, int groupId)
            {
                ID = id;
                GroupID = groupId;
            }
        }

        #endregion


        #region "Translation data container"

        private class TranslationData
        {
            public string CodeName
            {
                get;
                set;
            }


            public string SiteName
            {
                get;
                set;
            }


            public int ParentID
            {
                get;
                set;
            }


            /// <summary>
            /// Special case - Permissions.
            /// </summary>
            public string Info
            {
                get;
                set;
            }


            public int GroupID
            {
                get;
                set;
            }


            public Guid GUID
            {
                get;
                set;
            }


            public string ObjectType
            {
                get;
                set;
            }


            public Dictionary<string, object> AdditionalFields
            {
                get;
                set;
            }
        }

        #endregion


        #region "Variables"

        private DataTable mTranslationTable;
        private TranslationReferenceLoader mTranslationReferenceLoader;

        // Registered records cached by the composite key [key => DataRow]
        private readonly StringSafeDictionary<DataRow> mRegisteredRecords = new StringSafeDictionary<DataRow>();

        // Registered IDs [className => IDs]
        private readonly StringSafeDictionary<HashSet<int>> mRegisteredIDs = new StringSafeDictionary<HashSet<int>>();

        // If true, all records are registered for lookup
        private bool mAllRecordsRegistered = true;

        // Invalid column index of data set
        private const int INVALID_INDEX = -1;

        // Indicates whether translation table columns are defined with expected column type
        private readonly bool isTranslationTableValid = true;

        // Mapping of object types to the original types [objectType] -> [originalObjectType.ToLower]
        private readonly StringSafeDictionary<string> mOriginalTypes = new StringSafeDictionary<string>();

        // Default values for object types [objectType] -> [id]
        private readonly StringSafeDictionary<int> mDefaultValues = new StringSafeDictionary<int>();

        // ID conversion table [objectType] -> ConversionsDictionary [OldID -> [NewID, NewGroupID]]
        private readonly StringSafeDictionary<ConversionsDictionary> mIDConversion = new StringSafeDictionary<ConversionsDictionary>();

        internal readonly ICollection<string> EMPTY_READ_ONLY_COLLECTION = new List<string>().AsReadOnly();

        /// <summary>
        /// Automatic site name (Get by SiteIDs from DB).
        /// </summary>
        public const string AUTO_SITENAME = "##AUTO##";


        /// <summary>
        /// No site condition for registering records.
        /// </summary>
        public const string NO_SITE = "##NO##";


        /// <summary>
        /// Name of translation table
        /// </summary>
        public const string TRANSLATION_TABLE = "ObjectTranslation";


        /// <summary>
        /// Automatic site ID
        /// </summary>
        public const int AUTO_SITEID = -1;


        /// <summary>
        /// Automatic parent ID.
        /// </summary>
        public const int AUTO_PARENTID = -1;


        /// <summary>
        /// Connection object used to access data.
        /// </summary>
        protected IDataConnection mConnection = null;


        #region "Record column names"

        /// <summary>
        /// Name of column with object's identifier in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_ID_COLUMN = "ID";


        /// <summary>
        /// Name of column with object's parent identifier in a record <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_PARENT_ID_COLUMN = "ParentID";


        /// <summary>
        /// Name of column with object's group identifier in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_GROUP_ID_COLUMN = "GroupID";


        /// <summary>
        /// Name of column with object's class name identifier in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_CLASS_NAME_COLUMN = "ClassName";


        /// <summary>
        /// Name of column with object's class name in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_OBJECT_TYPE_COLUMN = "ObjectType";


        /// <summary>
        /// Name of column with object's code name in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_CODE_NAME_COLUMN = "CodeName";


        /// <summary>
        /// Name of column with object's site name in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_SITE_NAME_COLUMN = "SiteName";


        /// <summary>
        /// Name of column with object's additional information in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_INFO_COLUMN = "Info";


        /// <summary>
        /// Name of column with object's GUID in a record's <see cref="DataRow"/>.
        /// </summary>
        public const string RECORD_GUID_COLUMN = "GUID";


        /// <summary>
        /// Name of the computed column that contains (real) object type of current row. 
        /// Column is used in data query obtaining code names and other information based on ID and object type.
        /// Requested object type can differ from real object type when original and related object types are defined. 
        /// </summary>
        public const string QUERY_OBJECT_TYPE_COLUMN = "CMS_TH_T";


        /// <summary>
        /// Name of column with object's additional fields in a record's <see cref="DataRow"/>.
        /// </summary>
        internal const string RECORD_ADDITIONAL_FIELDS_COLUMN = "AdditionalFields";

        #endregion

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns the translation table.
        /// </summary>
        public DataTable TranslationTable
        {
            get
            {
                return mTranslationTable ?? (mTranslationTable = GetEmptyTable(UseAdditionalFields));
            }
        }


        /// <summary>
        /// ID conversion table [objectType.ToLowerCSafe()] -> hash table [OldID -> NewID]
        /// </summary>
        public Hashtable IDConversion
        {
            get
            {
                return mIDConversion;
            }
        }


        /// <summary>
        /// If true, display name is used instead of the code name.
        /// </summary>
        public bool UseDisplayNameAsCodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether additional fields' data (see <see cref="GetAdditionalFieldNames"/>)
        /// is associated with translation records.
        /// </summary>
        /// <remarks>
        /// Returns false by default. When overriding this member in a derived class,
        /// override the <see cref="GetAdditionalFieldNames"/> method as well.
        /// </remarks>
        internal virtual bool UseAdditionalFields
        {
            get
            {
                return false;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public TranslationHelper()
        {
        }


        /// <summary>
        /// Constructor. Creates the helper initialized with given translation table.
        /// </summary>
        /// <param name="dt">Source data table</param>
        public TranslationHelper(DataTable dt)
        {
            if (dt != null)
            {
                mTranslationTable = dt;

                // Translations made from external data, no records are registered by default
                mAllRecordsRegistered = false;

                if (!EnsureColumns(dt, false))
                {
                    isTranslationTableValid = false;
                    CoreServices.EventLog.LogEvent("E", "TranslationHelper", "InvalidDataTable", "TranslationHelper received a DataTable with wrong typed columns. It will work, but may affect performance." + Environment.NewLine + Environment.StackTrace);
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Determines whether the given translation helper contains any translations or not.
        /// </summary>
        /// <param name="th">TranslationHelper object to examine</param>
        /// <returns>TRUE if there are no translation records</returns>
        public static bool IsEmpty(TranslationHelper th)
        {
            if (th != null)
            {
                return DataHelper.DataSourceIsEmpty(th.TranslationTable);
            }

            return true;
        }


        /// <summary>
        /// Returns empty translation table.
        /// </summary>
        public static DataTable GetEmptyTable()
        {
            return GetEmptyTable(false);
        }


        private static DataTable GetEmptyTable(bool useAdditionalFields)
        {
            DataTable dataTable = new DataTable();
            EnsureColumns(dataTable, useAdditionalFields);
            dataTable.TableName = TRANSLATION_TABLE;
            return dataTable;
        }


        /// <summary>
        /// Ensures the columns in the translation table.
        /// </summary>
        /// <param name="dataTable">Translation table</param>
        /// <param name="useAdditionalFields">Indicates whether column for additional fields should be ensured</param>
        /// <returns>True if all columns were created or already declared with expected data type.</returns>
        private static bool EnsureColumns(DataTable dataTable, bool useAdditionalFields)
        {
            bool columnsAreValid = true;

            columnsAreValid &= EnsureColumn<string>(dataTable, RECORD_CLASS_NAME_COLUMN);
            columnsAreValid &= EnsureColumn<int>(dataTable, RECORD_ID_COLUMN);
            columnsAreValid &= EnsureColumn<string>(dataTable, RECORD_CODE_NAME_COLUMN);
            columnsAreValid &= EnsureColumn<string>(dataTable, RECORD_SITE_NAME_COLUMN);
            columnsAreValid &= EnsureColumn<int>(dataTable, RECORD_PARENT_ID_COLUMN);
            columnsAreValid &= EnsureColumn<string>(dataTable, RECORD_INFO_COLUMN);
            columnsAreValid &= EnsureColumn<int>(dataTable, RECORD_GROUP_ID_COLUMN);
            columnsAreValid &= EnsureColumn<Guid>(dataTable, RECORD_GUID_COLUMN);
            columnsAreValid &= EnsureColumn<string>(dataTable, RECORD_OBJECT_TYPE_COLUMN);

            if (useAdditionalFields)
            {
                columnsAreValid &= EnsureColumn<Dictionary<string, object>>(dataTable, RECORD_ADDITIONAL_FIELDS_COLUMN);
            }

            return columnsAreValid;
        }


        /// <summary>
        /// Ensures the column in the translation table.
        /// </summary>
        /// <typeparam name="T">Column data type</typeparam>
        /// <param name="dataTable">Translation table</param>
        /// <param name="columnName">Column name</param>
        /// <returns>true if column has the correct type defined in <paramref name="dataTable"/>; otherwise false.</returns>
        private static bool EnsureColumn<T>(DataTable dataTable, string columnName)
        {
            var column = dataTable.Columns[columnName];
            if (column == null)
            {
                dataTable.Columns.Add(new DataColumn(columnName, typeof(T)));
                return true;
            }

            if (column.DataType == typeof(T))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Clears the translation table.
        /// </summary>
        public virtual void Clear()
        {
            TranslationTable.Clear();
            IDConversion.Clear();
            mRegisteredRecords.Clear();
            mRegisteredIDs.Clear();

            mAllRecordsRegistered = false;
        }


        /// <summary>
        /// Returns true if the helper contains some records.
        /// </summary>
        public bool HasRecords()
        {
            return (TranslationTable.Rows.Count > 0);
        }


        /// <summary>
        /// Registers the record within given translation table.
        /// </summary>
        /// <param name="id">Object ID</param>
        /// <param name="parameters">Translation record values</param>
        /// <param name="info">Additional record info</param>
        /// <remarks>
        /// If there already was a record already registered for the given object type <paramref name="id"/> and it differs from the current <paramref name="parameters"/>,
        /// the old record is replaced with the new values.
        /// </remarks>
        public void RegisterRecord(int id, TranslationParameters parameters, string info = null)
        {
            RegisterRecord(id, parameters, info, null);
        }


        internal void RegisterRecord(BaseInfo infoObject)
        {
            var generalizedInfo = infoObject.Generalized;
            var typeInfo = infoObject.TypeInfo;

            var parameters = new TranslationParameters(typeInfo)
            {
                Guid = generalizedInfo.ObjectGUID,
                CodeName = generalizedInfo.ObjectCodeName,
                ParentId = generalizedInfo.ObjectParentID,
                GroupId = generalizedInfo.ObjectGroupID,
            };

            if (typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                // ObjectSiteName property returns site name of parent when current object doesn't have site column. 
                // Register site in TranslationHelper record only when current object has site column.
                parameters.SiteName = generalizedInfo.ObjectSiteName;
            }
            
            RegisterRecord(
                generalizedInfo.ObjectID,
                parameters,
                null,
                GetAdditionalFieldsData(infoObject)
            );
        }


        /// <summary>
        /// Registers the record within given translation table.
        /// </summary>
        /// <param name="id">Object ID</param>
        /// <param name="parameters">Translation record values</param>
        /// <param name="info">Additional record info</param>
        /// <param name="additionalFields">Dictionary containing additional record fields and their values</param>
        /// <remarks>
        /// If there already was a record already registered for the given object type <paramref name="id"/> and it differs from the currently registered,
        /// it is replaced with the new <paramref name="parameters"/>.
        /// </remarks>
        internal DataRow RegisterRecord(int id, TranslationParameters parameters, string info, IDictionary<string, object> additionalFields)
        {
            // Automatic site name
            if (parameters.SiteName == AUTO_SITENAME)
            {
                parameters = new TranslationParameters(parameters) { SiteName = null };
            }

            // Check existing record
            var existingRecord = GetRecord(parameters);
            if ((existingRecord != null) && (ValidationHelper.GetInteger(existingRecord[RECORD_ID_COLUMN], 0) == id))
            {
                // If already exists, do not register another
                return existingRecord;
            }

            // Add new record
            DataRow newRow = TranslationTable.NewRow();

            var className = GetClassName(parameters);

            newRow[RECORD_CLASS_NAME_COLUMN] = className;
            newRow[RECORD_ID_COLUMN] = id;
            newRow[RECORD_CODE_NAME_COLUMN] = parameters.CodeName;
            newRow[RECORD_SITE_NAME_COLUMN] = parameters.SiteName;
            newRow[RECORD_OBJECT_TYPE_COLUMN] = parameters.ObjectType;

            if (parameters.ParentId > 0)
            {
                newRow[RECORD_PARENT_ID_COLUMN] = parameters.ParentId;
            }

            if (info != null)
            {
                newRow[RECORD_INFO_COLUMN] = info;
            }

            if (parameters.GroupId > 0)
            {
                newRow[RECORD_GROUP_ID_COLUMN] = parameters.GroupId;
            }

            if (parameters.Guid != Guid.Empty)
            {
                newRow[RECORD_GUID_COLUMN] = parameters.Guid;
            }

            if (UseAdditionalFields)
            {
                newRow[RECORD_ADDITIONAL_FIELDS_COLUMN] = additionalFields ?? new Dictionary<string, object>();
            }

            var oldRecord = GetRecord(parameters.ObjectType, id);
            if (oldRecord != null)
            {
                // Remove old record so it can be replaced by the updated one
                // This is quite a rare case where the object related to the record has been renamed etc.
                DeleteRecord(oldRecord);
            }

            TranslationTable.Rows.Add(newRow);

            if (mAllRecordsRegistered)
            {
                // Cache the record for future use
                var key = GetRecordKey(parameters);

                mRegisteredRecords[key] = newRow;

                // Add registered ID
                var ids = mRegisteredIDs[className];
                if (ids == null)
                {
                    ids = new HashSet<int>();
                    mRegisteredIDs[className] = ids;
                }

                ids.Add(id);
            }

            return newRow;
        }


        /// <summary>
        /// Deletes given translation table record. 
        /// </summary>
        /// <param name="record">Record of translation table to be deleted.</param>
        /// <exception cref="ArgumentException">When <paramref name="record"/> DataRow does not belong to <see cref="TranslationTable"/>.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="record"/> is null.</exception>
        internal void DeleteRecord(DataRow record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (record.Table != TranslationTable)
            {
                throw new ArgumentException("DataRow must belong to translation table of current translation helper instance.", nameof(record));
            }

            RemoveRegisteredRecord(record);

            RemoveRegisteredId(record);

            // Delete row
            record.Delete();
        }


        /// <summary>
        ///  Removes ID from IDs hash table.
        /// </summary>
        /// <param name="record">Record with ID to remove.</param>
        private void RemoveRegisteredId(DataRow record)
        {
            var className = ValidationHelper.GetString(record[RECORD_CLASS_NAME_COLUMN], null);
            if (className != null)
            {
                var id = ValidationHelper.GetInteger(record[RECORD_ID_COLUMN], 0);
                var idsColletion = mRegisteredIDs[className];
                if (idsColletion != null)
                {
                    idsColletion.Remove(id);
                }
            }
        }


        /// <summary>
        /// Removes record references from hash table index.
        /// </summary>
        /// <param name="record">References to this record will be removed.</param>
        private void RemoveRegisteredRecord(DataRow record)
        {
            var keysToDelete = mRegisteredRecords
                .Cast<DictionaryEntry>()
                .Where(pair => pair.Value == record)
                .Select(pair => pair.Key.ToString())
                .ToList();

            keysToDelete.ForEach(key => mRegisteredRecords.Remove(key));
        }


        /// <summary>
        /// Checks if the required data is available for the translation, and throws exception in case something is missing
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">Object ID</param>
        /// <param name="codeName">Object code name</param>
        private static void CheckRequiredDataAvailable(string objectType, int id, string codeName)
        {
            if (id <= 0)
            {
                throw new InvalidOperationException("Missing ID in " + objectType + " (" + codeName + ") data.");
            }

            if (String.IsNullOrEmpty(codeName))
            {
                throw new InvalidOperationException("Missing code name in " + objectType + " (" + id + ") data.");
            }
        }


        /// <summary>
        /// Registers the records from the given table.
        /// </summary>
        /// <param name="dataTable">Table with the records</param>
        /// <param name="objectType">Object type of the target objects</param>
        /// <param name="idColumnName">Name of the column where object IDs are stored</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded object names</param>
        public void RegisterRecords(DataTable dataTable, string objectType, string idColumnName, string siteName, string[] excludedNames = null)
        {
            if ((dataTable == null) || (idColumnName == ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                return;
            }

            // Get new object IDs
            var objectIDs = DataHelper.GetIntegerValues(dataTable, idColumnName);

            RegisterRecords(objectIDs, objectType, siteName, excludedNames);
        }


        /// <summary>
        /// Registers the records from the given list.
        /// </summary>
        /// <param name="objectIDs">IDs to register</param>
        /// <param name="objectType">Object type of the target objects</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded object names</param>
        public void RegisterRecords(IList<int> objectIDs, string objectType, string siteName, string[] excludedNames = null)
        {
            objectIDs = FilterExistingRecords(objectIDs, objectType);

            // Get the translation table
            var data = GetTranslationDataFromDB(objectType, objectIDs, siteName);

            // Register the records
            RegisterTranslationData(data, siteName, excludedNames);
        }


        /// <summary>
        /// Registers the records from the given table.
        /// </summary>
        /// <param name="dt">Table with the records</param>
        /// <param name="objectTypeColumn">Name of the column with dynamic object type value</param>
        /// <param name="idColumnName">Name of the column where object IDs are stored</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded object names</param>
        public void RegisterDynamicRecords(DataTable dt, string objectTypeColumn, string idColumnName, string siteName, string[] excludedNames = null)
        {
            if ((dt == null) || (idColumnName == ObjectTypeInfo.COLUMN_NAME_UNKNOWN) || string.IsNullOrEmpty(objectTypeColumn))
            {
                return;
            }

            if (!DataHelper.DataSourceIsEmpty(dt))
            {
                Dictionary<string, List<int>> dynamicRecords = new Dictionary<string, List<int>>();

                foreach (DataRow row in dt.Rows)
                {
                    // Get dynamic object type
                    string objectType = ValidationHelper.GetString(row[objectTypeColumn], "");
                    if (!string.IsNullOrEmpty(objectType))
                    {
                        // Ensure list of IDs
                        if (!dynamicRecords.ContainsKey(objectType))
                        {
                            dynamicRecords[objectType] = new List<int>();
                        }

                        // Get object ID
                        int objectId = ValidationHelper.GetInteger(row[idColumnName], 0);
                        if (objectId > 0)
                        {
                            dynamicRecords[objectType].Add(objectId);
                        }
                    }
                }

                foreach (var objectType in dynamicRecords.Keys)
                {
                    RegisterRecords(objectType, dynamicRecords[objectType], siteName, excludedNames);
                }
            }
        }


        /// <summary>
        /// Registers the records from the given ID list.
        /// </summary>
        /// <param name="objectType">Object type of the target objects</param>
        /// <param name="objectIDs">List of IDs to register</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded object names</param>
        public void RegisterRecords(string objectType, IList<int> objectIDs, string siteName, string[] excludedNames)
        {
            if (objectIDs == null)
            {
                return;
            }

            // Filter out IDs which are already registered
            objectIDs = FilterExistingRecords(objectIDs, objectType);

            // Get the translation table
            var data = GetTranslationDataFromDB(objectType, objectIDs, siteName);

            // Register the records
            RegisterTranslationData(data, siteName, excludedNames);
        }


        /// <summary>
        /// Registers the code names from the given table.
        /// </summary>
        /// <param name="data">Translation data organized by object ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded object names</param>
        private void RegisterTranslationData(Dictionary<int, TranslationData> data, string siteName, string[] excludedNames)
        {
            // Register records
            StringSafeDictionary<HashSet<int>> parents = new StringSafeDictionary<HashSet<int>>(false);

            // Groups records
            HashSet<int> groups = new HashSet<int>();

            // Register the records and build the lists of parent IDs
            foreach (var item in data)
            {
                int id = item.Key;
                TranslationData record = item.Value;
                if (id > 0)
                {
                    string codeName = record.CodeName;

                    // Exclude excluded code names
                    bool register = true;
                    if ((excludedNames != null) && (excludedNames.Length > 0))
                    {
                        // Check all excluded code names
                        foreach (string name in excludedNames)
                        {
                            if (codeName.StartsWithCSafe(name, true))
                            {
                                register = false;
                                break;
                            }
                        }
                    }

                    if (register)
                    {
                        int parentId = record.ParentID;
                        string info = record.Info;
                        string itemSiteName = record.SiteName;
                        int groupId = record.GroupID;
                        Guid guid = record.GUID;
                        string objectType = record.ObjectType;
                        Dictionary<string, object> additionalFields = record.AdditionalFields;

                        // Register the record
                        var parameters = new TranslationParameters(objectType)
                        {
                            Guid = guid,
                            CodeName = codeName,
                            SiteName = itemSiteName,
                            ParentId = parentId,
                            GroupId = groupId
                        };
                        RegisterRecord(id, parameters, info, additionalFields);

                        // Add parent ID to the list to register
                        if (parentId > 0)
                        {
                            var parentsList = parents[objectType] ?? (parents[objectType] = new HashSet<int>());
                            parentsList.Add(parentId);
                        }

                        if (groupId > 0)
                        {
                            groups.Add(groupId);
                        }
                    }
                }
            }

            foreach (var type in parents.TypedKeys)
            {
                string parentObjectType = GetParentType(type);
                var parentsIds = (parents[type] == null) ? new List<int>() : parents[type].ToList();
                RegisterParentRecords(parentObjectType, parentsIds, siteName, excludedNames);
            }

            // Register group record
            RegisterGroupRecords(groups.ToList(), siteName, excludedNames);
        }


        /// <summary>
        /// Registers the group records to the translation table.
        /// </summary>
        /// <param name="groups">List of groups IDs</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded code names</param>
        private void RegisterGroupRecords(IList<int> groups, string siteName, string[] excludedNames)
        {
            // Register group records
            if ((groups != null) && groups.Count > 0)
            {
                // Get new group IDs
                IList<int> groupIDs = groups;
                groupIDs = FilterExistingRecords(groupIDs, PredefinedObjectType.GROUP);

                // Get the translation table
                var data = GetTranslationDataFromDB(PredefinedObjectType.GROUP, groupIDs, siteName);

                // Register group records
                RegisterTranslationData(data, siteName, excludedNames);
            }
        }


        /// <summary>
        /// Registers the parent records to the translation table.
        /// </summary>
        /// <param name="parentObjectType">Parent object type</param>
        /// <param name="parents">List of parent IDs</param>
        /// <param name="siteName">Site name</param>
        /// <param name="excludedNames">Excluded code names</param>
        private void RegisterParentRecords(string parentObjectType, IList<int> parents, string siteName, string[] excludedNames)
        {
            if ((parents == null) || parents.Count <= 0)
            {
                // No parents provided
                return;
            }

            if (string.IsNullOrEmpty(parentObjectType))
            {
                // No parent object type provided
                return;
            }

            // Get new parent IDs
            IList<int> parentIDs = parents;
            parentIDs = FilterExistingRecords(parentIDs, parentObjectType);

            // Get the translation table
            var data = GetTranslationDataFromDB(parentObjectType, parentIDs, siteName);

            // Register parent records
            RegisterTranslationData(data, siteName, excludedNames);
        }


        /// <summary>
        /// Filters the existing records from the given array.
        /// </summary>
        /// <param name="objectIDs">Existing IDs</param>
        /// <param name="objectType">Object type</param>
        public IList<int> FilterExistingRecords(IList<int> objectIDs, string objectType)
        {
            // When no IDs are incoming, don't filter
            if ((objectIDs == null) || (objectIDs.Count == 0))
            {
                return objectIDs;
            }

            var className = GetClassName(objectType);

            // When all IDs are registered, filter using registered IDs
            if (mAllRecordsRegistered)
            {
                // Get registered IDs
                var ids = mRegisteredIDs[className];
                return ids == null
                    ? objectIDs
                    : GetFilteredIDs(objectIDs, ids);
            }

            // Collect existing IDs from the current translation table
            var existingIDs = new HashSet<int>();

            var idIndex = TranslationTable.Columns.IndexOf("ID");
            var classNameIndex = TranslationTable.Columns.IndexOf("ClassName");

            DataHelper.ForEachRow(
                TranslationTable,
                dr => existingIDs.Add((int)dr[idIndex]),
                dr => (string)dr[classNameIndex] == className);

            return GetFilteredIDs(objectIDs, existingIDs);
        }


        private static IList<int> GetFilteredIDs(IEnumerable<int> ids, HashSet<int> filterOutIds)
        {
            // Build the filtered list
            var filteredIds = new List<int>();

            foreach (var id in ids)
            {
                if (!filterOutIds.Contains(id) && (id > 0))
                {
                    filteredIds.Add(id);
                }
            }

            return filteredIds;
        }


        /// <summary>
        /// Returns true if specified record exists in translation table.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">Object ID</param>
        public bool RecordExists(string objectType, int id)
        {
            return (GetCodeName(objectType, id, null) != null);
        }


        /// <summary>
        /// Returns the code name of the specified object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">Object ID</param>
        public string GetCodeName(string objectType, int id)
        {
            return GetCodeName(objectType, id, "");
        }


        /// <summary>
        /// Gets the record specified by parameters.
        /// </summary>
        /// <remarks>
        /// Object's <see cref="TranslationParameters.Guid"/> has priority over code name. However, if there is <see cref="Guid.Empty"/> provided as <see cref="TranslationParameters.Guid"/>, only <see cref="TranslationParameters.CodeName"/> will be used to identify object.
        /// </remarks>
        /// <param name="parameters">Parameters used to obtain the record.</param>
        public DataRow GetRecord(TranslationParameters parameters)
        {
            var key = GetRecordKey(parameters);

            // Get the record from cache by key
            var result = GetRegisteredRecord(key);
            if ((result != null) || mAllRecordsRegistered)
            {
                return result;
            }

            // Possibility that the record is not registered, try to find record in translation table
            result = TranslationTable.AsEnumerable()
                                     .FirstOrDefault(row => RecordsFilter(row, parameters));

            // Cache the result
            if (result != null)
            {
                mRegisteredRecords[key] = result;
            }

            return result;
        }


        /// <summary>
        /// Return record using the hash table index. 
        /// </summary>
        /// <remaks>
        /// Only for testing purposes outside of TranslationHelper class.
        /// </remaks>
        /// <param name="key">Index key to obtain a record.</param>
        internal DataRow GetRegisteredRecord(string key)
        {
            var result = mRegisteredRecords[key];

            if (result == null)
            {
                return null;
            }

            // Row is in invalid state
            if ((result.RowState & (DataRowState.Deleted | DataRowState.Detached)) != 0)
            {
                mRegisteredRecords.Remove(key);
                return null;
            }

            return result;
        }


        /// <summary>
        /// Returns class name used to identify a database table. 
        /// </summary>
        private string GetClassName(string objectType, ObjectTypeInfo typeInfo = null)
        {
            string originalObjectType = GetOriginalType(objectType, typeInfo);
            string className = GetSafeClassName(originalObjectType);

            return className;
        }


        /// <summary>
        /// Gets the translations XML
        /// </summary>
        /// <param name="includeRoot">If true, the root node is included</param>
        public string GetTranslationsXml(bool includeRoot)
        {
            string translationsXml = String.Empty;

            // Write translations to xml
            using (var writer = new IO.StringWriter())
            {
                TranslationTable.WriteXml(writer, XmlWriteMode.IgnoreSchema, false);

                translationsXml = writer.ToString();
            }

            if (!string.IsNullOrEmpty(translationsXml) && !includeRoot)
            {
                using (var stringReader = new IO.StringReader(translationsXml))
                {
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        xmlReader.MoveToContent();
                        translationsXml = xmlReader.ReadInnerXml();
                    }
                }
            }

            return translationsXml;
        }


        /// <summary>
        /// Returns class name used to identify a database table. 
        /// </summary>
        private string GetClassName(TranslationParameters parameters)
        {
            return GetClassName(parameters.ObjectType, parameters.TypeInfo);
        }


        /// <summary>
        /// Returns hash table index key for given parameters. 
        /// </summary>
        /// <remaks>
        /// Only for testing purposes outside of TranslationHelper class.
        /// </remaks>
        internal string GetRecordKey(TranslationParameters parameters)
        {
            var parentId = parameters.ParentId;

            if ((parameters.ParentId > 0) && (parameters.TypeInfo != null) && parameters.TypeInfo.IsMainObject)
            {
                // Main object types do not need to be referenced using parent as their parent can only be of same type, thus there cannot be 
                // two same objects distinguished by parent ID only. Omitting parentId in this cases prevents unnecessary DB queries.
                parentId = 0;
            }
            return String.Format("{0}|{1}|{2}|{3}|{4}|{5}", GetClassName(parameters), parameters.CodeName, parameters.SiteName, parentId, parameters.GroupId, parameters.Guid);
        }


        private bool RecordsFilter(DataRow recordRow, TranslationParameters parameters)
        {
            if (parameters.ParentId > 0)
            {
                if (!parameters.ParentId.Equals(recordRow[RECORD_PARENT_ID_COLUMN]))
                {
                    return false;
                }
            }

            if (parameters.GroupId > 0)
            {
                if (!parameters.GroupId.Equals(recordRow[RECORD_GROUP_ID_COLUMN]))
                {
                    return false;
                }
            }
            else
            {
                if (!DataHelper.IsEmpty(recordRow[RECORD_GROUP_ID_COLUMN]))
                {
                    return false;
                }
            }

            var className = GetClassName(parameters);

            if (!String.Equals(className, recordRow.Field<string>(RECORD_CLASS_NAME_COLUMN), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // GUID has precedence to code name
            if (parameters.Guid == Guid.Empty)
            {
                if (!parameters.CodeName.Equals(recordRow.Field<string>(RECORD_CODE_NAME_COLUMN), StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            else
            {
                if (!parameters.Guid.Equals(recordRow[RECORD_GUID_COLUMN]))
                {
                    return false;
                }
            }

            if (parameters.SiteName == null)
            {
                if (!DataHelper.IsEmpty(recordRow[RECORD_SITE_NAME_COLUMN]))
                {
                    return false;
                }
            }
            else
            {
                if (!parameters.SiteName.Equals(recordRow.Field<string>(RECORD_SITE_NAME_COLUMN), StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            // Everything checks, keep the row
            return true;
        }


        /// <summary>
        /// Gets the record by ID.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">ID</param>
        public DataRow GetRecord(string objectType, int id)
        {
            var className = GetClassName(objectType);

            if (isTranslationTableValid)
            {
                return TranslationTable.AsEnumerable()
                                       .FirstOrDefault(row => ((id == row.Field<int>(RECORD_ID_COLUMN)) && string.Compare(className, row.Field<string>(RECORD_CLASS_NAME_COLUMN), StringComparison.InvariantCultureIgnoreCase) == 0));
            }

            // Type-safe fall back for invalid DataTables
            return TranslationTable.Select(RECORD_ID_COLUMN + " = " + id + " AND " + RECORD_CLASS_NAME_COLUMN + " = '" + SqlHelper.GetSafeQueryString(className, false) + "'")
                                   .FirstOrDefault();
        }


        /// <summary>
        /// Returns the code name of the specified object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="id">Object ID</param>
        /// <param name="defaultValue">Value to return in case record not found</param>
        public string GetCodeName(string objectType, int id, string defaultValue)
        {
            // Get the record
            var record = GetRecord(objectType, id);
            if ((record == null) || (record[RECORD_CODE_NAME_COLUMN] == DBNull.Value))
            {
                return defaultValue;
            }

            return (string)record[RECORD_CODE_NAME_COLUMN];
        }


        /// <summary>
        /// Returns the object ID for specified record.
        /// </summary>
        /// <remarks>
        /// Object's <see cref="TranslationParameters.Guid"/> has priority over code name. However, if there is <see cref="Guid.Empty"/> provided as <see cref="TranslationParameters.Guid"/>, only <see cref="TranslationParameters.CodeName"/> will be used to identify object.
        /// </remarks>
        /// <param name="parameters">Parameters used to select record</param>
        public int GetID(TranslationParameters parameters)
        {
            var record = GetRecord(parameters);

            return record == null
                ? GetDefaultValue(parameters.ObjectType)
                : (int)record[RECORD_ID_COLUMN];
        }


        /// <summary>
        /// Returns the object ID for specified record calling <see cref="GetID(TranslationParameters)"/>.
        /// If record could not be found, method falls back to <see cref="GetIDFromDB(GetIDParameters,string)"/> and eventually registers DB object in the helper.
        /// If neither attempt results into valid object ID, 0 is returned.
        /// </summary>
        /// <remarks>
        /// Object's <see cref="TranslationParameters.Guid"/> has priority over code name. However, if there is <see cref="Guid.Empty"/> provided as <see cref="TranslationParameters.Guid"/>, only <see cref="TranslationParameters.CodeName"/> will be used to identify object.
        /// <para>
        /// The <see cref="TranslationTable"/> property contains data records that correspond with the database.        
        /// For translation records identified using partially correct parameters (either the code name or GUID is different than the value in the database), 
        /// the actual values are loaded from the database for all fields.        
        /// An index key referencing the correct data is then created from the partial parameters. The index key is used to obtain the record in future calls.
        /// </para>
        /// </remarks>
        /// <param name="siteId">Id of site</param>
        /// <param name="parameters">translation parameters. Property <see cref="TranslationParameters.SiteName"/> is not used and doesn't need to be set.</param>
        public int GetIDWithFallback(TranslationParameters parameters, int siteId)
        {
            // Get destination site name
            string siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);

            var translationParameters = new TranslationParameters(parameters) { SiteName = siteName };

            // Get the ID from registered records
            int id = GetID(translationParameters);

            if (id <= 0)
            {
                // Get the ID from database, if there was no record registered for the object
                var idParameters = new GetIDParameters
                {
                    Guid = parameters.Guid,
                    CodeName = parameters.CodeName,
                    SiteId = siteId,
                    ParentId = parameters.ParentId,
                    GroupId = parameters.GroupId,
                    AdditionalFields = GetAdditionalFieldNames(parameters.TypeInfo)
                };

                var wrappedObject = GetObjectWrapperFromDB(idParameters, parameters.ObjectType);
                if (wrappedObject != ObjectDatabaseWrapper.EMPTY)
                {
                    // Register record with the obtained ID for future use
                    id = wrappedObject.ID;

                    // Update parameters with real code name / GUID. 
                    // Either the code name or GUID could be different than the value in the database.
                    var realTranslationParameters = new TranslationParameters(translationParameters)
                    {
                        CodeName = wrappedObject.CodeName,
                        Guid = wrappedObject.Guid,
                    };
                    var record = RegisterRecord(id, realTranslationParameters, null, wrappedObject.AdditionalFields);

                    // Create reference to new record for original parameters
                    var key = GetRecordKey(translationParameters);
                    mRegisteredRecords[key] = record;
                }
            }

            return id;
        }


        /// <summary>
        /// Adds the ID translation to the translation table.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="oldId">Old ID</param>
        /// <param name="newId">New ID</param>
        /// <param name="newGroupId">New group ID</param>
        public void AddIDTranslation(string objectType, int oldId, int newId, int newGroupId)
        {
            if ((newId <= 0) || (oldId <= 0))
            {
                return;
            }

            // Ensure the conversion table
            string originalObjectType = GetOriginalType(objectType);

            var conversions = EnsureConversionTable(originalObjectType);

            // Cache the ID conversion
            conversions[oldId] = new ConversionIDs(newId, newGroupId);
        }


        /// <summary>
        /// Gets original object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="typeInfo">Object type info</param>
        private string GetOriginalType(string objectType, ObjectTypeInfo typeInfo = null)
        {
            // Try to get from cached values
            var result = mOriginalTypes[objectType];
            if (result == null)
            {
                typeInfo = typeInfo ?? ObjectTypeManager.GetTypeInfo(objectType);
                result = (typeInfo != null) ? typeInfo.OriginalObjectType : objectType;

                mOriginalTypes[objectType] = result.ToLowerCSafe();

                return result;
            }

            return result;
        }


        /// <summary>
        /// Sets default value.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="defaultValue">Default value</param>
        public void SetDefaultValue(string objectType, int defaultValue)
        {
            string originalObjectType = GetOriginalType(objectType);

            mDefaultValues[originalObjectType] = defaultValue;
        }


        /// <summary>
        /// Removes default value.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public void RemoveDefaultValue(string objectType)
        {
            string originalObjectType = GetOriginalType(objectType);

            if (mDefaultValues.ContainsKey(originalObjectType))
            {
                mDefaultValues.Remove(originalObjectType);
            }
        }


        /// <summary>
        /// Gets default value.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <exception cref="ArgumentException">When provided <paramref name="objectType"/> is null or empty.</exception>
        public int GetDefaultValue(string objectType)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new ArgumentException("Object type was not provided.", nameof(objectType));
            }

            string originalObjectType = GetOriginalType(objectType);

            return mDefaultValues[originalObjectType];
        }


        /// <summary>
        /// Gets the new ID for given old ID.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="oldId">Old ID</param>
        /// <param name="codeNameColumn">Code name column of the class</param>
        /// <param name="siteId">Site ID of the required object</param>
        /// <param name="siteIdColumn">Site ID column of the class</param>
        /// <param name="parentIdColumn">Parent ID column</param>
        /// <param name="groupIdColumn">Group ID column</param>
        public int GetNewID(string objectType, int oldId, string codeNameColumn, int siteId, string siteIdColumn, string parentIdColumn, string groupIdColumn)
        {
            int groupId;
            return GetNewID(objectType, oldId, codeNameColumn, siteId, siteIdColumn, parentIdColumn, groupIdColumn, true, out groupId);
        }


        /// <summary>
        /// Gets the new ID for given old ID.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="oldId">Old ID</param>
        /// <param name="codeNameColumn">Code name column of the class</param>
        /// <param name="siteId">Site ID of the required object</param>
        /// <param name="siteIdColumn">Site ID column of the class</param>
        /// <param name="parentIdColumn">Parent ID column</param>
        /// <param name="groupIdColumn">Group ID column</param>
        /// <param name="useDefaultValue">Indicates if default value should be used for ID</param>
        /// <param name="outGroupId">Returns Group ID value</param>
        public int GetNewID(string objectType, int oldId, string codeNameColumn, int siteId, string siteIdColumn, string parentIdColumn, string groupIdColumn, bool useDefaultValue, out int outGroupId)
        {
            outGroupId = 0;

            // If not set or special value, return the previous value
            if (oldId <= 0)
            {
                return oldId;
            }

            var conversions = EnsureConversionTable(objectType);

            // Try to get conversion from existing table
            if (conversions.ContainsKey(oldId))
            {
                var value = conversions[oldId];

                // Output values, convert "not found" ids to 0
                outGroupId = GetOutputId(value.GroupID);

                return GetOutputId(value.ID);
            }

            // If code name column not specified, cannot get the value
            if (codeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return 0;
            }

            // Get the parameters
            var parameters = GetParametersForId(objectType, oldId, siteId);
            if (parameters == null)
            {
                // If parameters can't be prepared, fall back to default value or no output
                return useDefaultValue ? GetDefaultValue(objectType) : 0;
            }

            // Ensure parameter columns (some may be already set explicitly)
            parameters.CodeNameColumn = parameters.CodeNameColumn ?? codeNameColumn;
            parameters.SiteIdColumn = parameters.SiteIdColumn ?? siteIdColumn;
            parameters.ParentIdColumn = parameters.ParentIdColumn ?? parentIdColumn;
            parameters.GroupIdColumn = parameters.GroupIdColumn ?? groupIdColumn;

            // Get new ID from database
            int newId = GetIDFromDB(parameters, objectType);

            // Get default value if not found
            if ((newId <= 0) && useDefaultValue)
            {
                newId = GetDefaultValue(objectType);
            }

            // Save the conversion for later use
            if (newId > 0)
            {
                // Cache the found ID
                conversions[oldId] = new ConversionIDs(newId, parameters.GroupId);
            }
            else
            {
                // Cache as not found ID. When such object is imported in the later process, it overwrites this cached value
                conversions[oldId] = new ConversionIDs(DataHelper.FAKE_ID, DataHelper.FAKE_ID);
            }

            // Output values, convert "not found" ids to 0
            outGroupId = GetOutputId(parameters.GroupId);

            return GetOutputId(newId);
        }


        /// <summary>
        /// Gets the parameters for getting a new object ID based on the given object type, old ID and target site ID
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="oldId">Old object ID</param>
        /// <param name="siteId">Site ID</param>
        private GetIDParameters GetParametersForId(string objectType, int oldId, int siteId)
        {
            // Get the record data
            var record = GetRecord(objectType, oldId);
            if (record == null)
            {
                return null;
            }

            var codename = ValidationHelper.GetString(record[RECORD_CODE_NAME_COLUMN], null);
            var sitename = ValidationHelper.GetString(record[RECORD_SITE_NAME_COLUMN], null);

            // Get new site ID if object site specified
            if (siteId == AUTO_SITEID)
            {
                // Get the site name automatically (site name must be preserved across installations)
                siteId = ProviderHelper.GetId(PredefinedObjectType.SITE, sitename);
            }
            else if (String.IsNullOrEmpty(sitename))
            {
                // Get global object if site name not registered
                siteId = 0;
            }

            string parentIdColumn = null;

            // Parent ID
            var parentId = ValidationHelper.GetInteger(record[RECORD_PARENT_ID_COLUMN], 0);
            if (parentId > 0)
            {
                string originalObjectType = GetOriginalType(objectType);

                // Get new parent ID
                string parentType;

                if (originalObjectType == PredefinedObjectType.PERMISSION)
                {
                    // ### Special case - Permissions ###
                    var permissionType = ValidationHelper.GetString(record[RECORD_OBJECT_TYPE_COLUMN], null);
                    if (String.Equals(permissionType, PredefinedObjectType.CLASSPERMISSION, StringComparison.InvariantCultureIgnoreCase))
                    {
                        parentType = DataClassInfo.OBJECT_TYPE;
                        parentIdColumn = "ClassID";
                    }
                    else if (String.Equals(permissionType, PredefinedObjectType.PERMISSION, StringComparison.InvariantCultureIgnoreCase))
                    {
                        parentType = PredefinedObjectType.RESOURCE;
                        parentIdColumn = "ResourceID";
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown permission type.");
                    }
                }
                else
                {
                    // Get standard parent type
                    parentType = GetParentType(objectType);
                }

                if (!String.IsNullOrEmpty(parentType))
                {
                    parentId = GetNewID(parentType, parentId, null, AUTO_SITEID, null, null, null);

                    // Check if parent was successfully translated
                    if (parentId <= 0)
                    {
                        return null;
                    }
                }
                else
                {
                    parentId = 0;
                }
            }

            // Group ID
            var groupId = ValidationHelper.GetInteger(record[RECORD_GROUP_ID_COLUMN], 0);
            if (groupId > 0)
            {
                groupId = GetNewID(PredefinedObjectType.GROUP, groupId, null, siteId, null, null, null);

                // Group object was not found
                if (groupId <= 0)
                {
                    return null;
                }
            }

            var parameters = new GetIDParameters
            {
                OldId = oldId,
                CodeName = codename,
                SiteId = siteId,
                ParentId = parentId,
                GroupId = groupId,
                ParentIdColumn = parentIdColumn
            };

            return parameters;
        }


        /// <summary>
        /// Ensures the ID translation table for the given object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        private ConversionsDictionary EnsureConversionTable(string objectType)
        {
            string originalObjectType = GetOriginalType(objectType);

            // Ensure the conversion table
            var conversions = mIDConversion[originalObjectType];
            if (conversions == null)
            {
                conversions = new ConversionsDictionary();
                mIDConversion[originalObjectType] = conversions;
            }

            return conversions;
        }


        private int GetOutputId(int id)
        {
            if (DataHelper.IsValidID(id))
            {
                return id;
            }

            return 0;
        }


        /// <summary>
        /// Translates the DataRow column.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteId">Target site ID for objects lookup which are site-related</param>
        /// <returns>Returns true if the translation was successful</returns>
        public bool TranslateColumn(DataRow dr, string columnName, string objectType, int siteId = 0)
        {
            // Get column index
            int colIndex = dr.Table.Columns.IndexOf(columnName);
            if (colIndex < 0)
            {
                return true;
            }

            // Get current value
            object value = dr[colIndex];
            if ((value != null) && (value != DBNull.Value))
            {
                int oldId = ValidationHelper.GetInteger(value, 0);
                if (oldId > 0)
                {
                    // Get new ID
                    int newId = GetNewID(objectType, oldId, null, siteId, null, null, null);

                    // Update the object
                    if (newId > 0)
                    {
                        dr[colIndex] = newId;
                    }
                    else
                    {
                        dr[colIndex] = DBNull.Value;
                    }

                    return (newId > 0);
                }
            }

            return true;
        }


        /// <summary>
        /// Translates the column value with list of IDs of the given info object.
        /// </summary>
        /// <param name="infoObj">Object with the data</param>
        /// <param name="columnName">Column name to translate</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteId">Target site ID for objects lookup which are site-related</param>
        /// <param name="separator">ID separator</param>
        /// <returns>Returns true if the translation was successful</returns>
        public bool TranslateListColumn(GeneralizedInfo infoObj, string columnName, string objectType, int siteId, char separator)
        {
            // Get current value
            object value = infoObj.GetValue(columnName);
            if (value != null)
            {
                // String field - list of IDs
                string[] oldIDs = ((string)value).Split(separator);
                for (int i = 0; i < oldIDs.Length; i++)
                {
                    try
                    {
                        int oldId = ValidationHelper.GetInteger(oldIDs[i].Trim(), 0);
                        if (oldId > 0)
                        {
                            // Get new ID
                            int newId = GetNewID(objectType, oldId, null, siteId, null, null, null);
                            if (newId > 0)
                            {
                                oldIDs[i] = newId.ToString();
                            }
                            else
                            {
                                oldIDs[i] = "";
                            }
                        }
                    }
                    catch
                    {
                        oldIDs[i] = "";
                    }
                }

                // Save translated values to the DataRow
                infoObj.SetValue(columnName, String.Join(separator + "", oldIDs));
            }

            return true;
        }


        /// <summary>
        /// Translates the column value of the given info object.
        /// </summary>
        /// <param name="infoObj">Object with the data</param>
        /// <param name="columnName">Column name to translate</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteId">Target site ID for objects lookup which are site-related</param>
        /// <param name="useDefaultValue">Indicates if default value should be used for new IDs</param>
        /// <param name="required">If false, the column is allowed to be set to null in case the translation is not found.</param>
        /// <returns>Returns true if the translation was successful</returns>
        public bool TranslateColumn(GeneralizedInfo infoObj, string columnName, string objectType, int siteId = 0, bool useDefaultValue = true, bool required = true)
        {
            // For objects with optional site ID column which don't have the site ID set, do not user given site ID for translation
            var ti = infoObj.TypeInfo;

            string siteIdColumn = ti.SiteIDColumn;
            if ((siteIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (infoObj.GetValue(siteIdColumn) == null))
            {
                siteId = 0;
            }
            // ### Special case: page template scope is available only for reusable templates
            else if (ti.ObjectType == PredefinedObjectType.PAGETEMPLATESCOPE)
            {
                siteId = 0;
            }
            else if ((ti.ObjectType == PredefinedObjectType.NODE) || (ti.OriginalObjectType == PredefinedObjectType.DOCUMENT) || (ti.ObjectType == PredefinedObjectType.DOCUMENTCATEGORY))
            {
                // ### Special case: get site identifier automatically for node or document
                siteId = AUTO_SITEID;
            }

            // Get current value
            object value = infoObj.GetValue(columnName);
            if (value != null)
            {
                int oldId = (int)value;
                if (oldId > 0)
                {
                    int groupId;

                    // Get new ID
                    int newId = GetNewID(objectType, oldId, null, siteId, null, null, null, useDefaultValue, out groupId);
                    bool isParentColumn = CMSString.Equals(ti.ParentIDColumn, columnName, true);

                    // Propagate GroupID value from parent object
                    if (isParentColumn && (groupId > 0) && (ti.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                    {
                        infoObj.ObjectGroupID = groupId;
                    }

                    // Update the object if not parent
                    if ((newId > 0) || !isParentColumn)
                    {
                        if (newId > 0)
                        {
                            infoObj.SetValue(columnName, newId);
                        }
                        else if (!required)
                        {
                            infoObj.SetValue(columnName, null);

                            return true;
                        }
                        else
                        {
                            infoObj.SetValue(columnName, null);
                        }
                    }
                    else if (!required)
                    {
                        // Set to null if not required
                        infoObj.SetValue(columnName, null);

                        return true;
                    }

                    return (newId > 0);
                }

                // Set null value
                infoObj.SetValue(columnName, null);
            }

            return true;
        }


        /// <summary>
        /// Translates the column value of the given container.
        /// </summary>
        /// <param name="container">Container with the data</param>
        /// <param name="columnName">Column name to translate</param>
        /// <param name="objectType">Reference object type</param>
        /// <param name="siteId">Target site ID for objects lookup which are site-related</param>
        /// <param name="useDefaultValue">Indicates if default value should be used for new IDs</param>
        /// <param name="required">If false, the column is allowed to be set to null in case the translation is not found.</param>
        /// <returns>Returns true if the translation was successful</returns>
        public bool TranslateColumn(IDataContainer container, string columnName, string objectType, int siteId = 0, bool useDefaultValue = true, bool required = true)
        {
            // Get current value
            object value = container.GetValue(columnName);
            if (value != null)
            {
                int oldId = (int)value;
                if (oldId > 0)
                {
                    int groupId;

                    // Get new ID
                    int newId = GetNewID(objectType, oldId, null, siteId, null, null, null, useDefaultValue, out groupId);

                    // Update the container
                    if (newId > 0)
                    {
                        container.SetValue(columnName, newId);
                    }
                    else if (!required)
                    {
                        container.SetValue(columnName, null);

                        return true;
                    }
                    else
                    {
                        container.SetValue(columnName, null);
                    }

                    return (newId > 0);
                }
                else
                {
                    // Set null value
                    container.SetValue(columnName, null);
                }
            }

            return true;
        }


        /// <summary>
        /// Changes the object code name in the table.
        /// </summary>
        /// <remarks>
        /// Object's <see cref="TranslationParameters.Guid"/> might be set to <see cref="Guid.Empty"/>, however, if there is another object with properly set GUID preset simultaneously,
        /// both objects will be treated as distinct (despite same <see cref="TranslationParameters.CodeName"/> and <see cref="TranslationParameters.SiteName"/> and <see cref="TranslationParameters.ParentId"/> and <see cref="TranslationParameters.GroupId"/>).
        /// </remarks>
        /// <param name="parameters">Parameters that select record which we want to alter.</param>
        /// <param name="newCodeName">New code name.</param>
        public void ChangeCodeName(TranslationParameters parameters, string newCodeName)
        {
            var record = GetRecord(parameters);
            if (record != null)
            {
                record[RECORD_CODE_NAME_COLUMN] = newCodeName;

                // Clear the old record cache
                var oldKey = GetRecordKey(parameters);
                mRegisteredRecords[oldKey] = null;

                // Register a new record cache
                var newKey = GetRecordKey(new TranslationParameters(parameters) { CodeName = newCodeName });
                mRegisteredRecords[newKey] = record;
            }
        }


        /// <summary>
        /// Translates all reference object columns.
        /// </summary>
        /// <param name="infoObj">Object with the data</param>
        /// <param name="siteId">Translate site ID</param>
        /// <param name="parentId">Translate parent ID</param>
        /// <param name="dependencies">Translate dependencies</param>
        /// <param name="targetSiteId">Target site ID for objects lookup which are site-related</param>
        /// <param name="excludeColumns">Columns to exclude</param>
        public string TranslateColumns(GeneralizedInfo infoObj, bool siteId, bool parentId, bool dependencies, int targetSiteId, string excludeColumns)
        {
            string result = "";

            var ti = infoObj.TypeInfo;

            // Translate SiteID
            if (siteId)
            {
                if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    if (!IsExcluded(ti.SiteIDColumn, excludeColumns))
                    {
                        if (!TranslateColumn(infoObj, ti.SiteIDColumn, PredefinedObjectType.SITE, 0, false))
                        {
                            result += ti.SiteIDColumn + ";";
                        }
                    }
                }
            }

            // Translate dependencies
            if (dependencies)
            {
                if (ti.ObjectDependencies != null)
                {
                    foreach (var dep in ti.ObjectDependencies)
                    {
                        // Translate the dependency
                        string columnName = dep.DependencyColumn;
                        if (!IsExcluded(columnName, excludeColumns))
                        {
                            string objectType = infoObj.GetDependencyObjectType(dep);
                            if (!string.IsNullOrEmpty(objectType))
                            {
                                var requiredObj = dep.DependencyType;

                                bool useDefaultValue = (requiredObj == ObjectDependencyEnum.RequiredHasDefault);

                                if (!TranslateColumn(infoObj, columnName, objectType, targetSiteId, useDefaultValue, dep.IsRequired()))
                                {
                                    result += columnName + ";";
                                }
                            }
                        }
                    }
                }
            }

            // Translate parent ID (MUST BE LAST)
            if (parentId)
            {
                if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    if (!IsExcluded(ti.ParentIDColumn, excludeColumns))
                    {
                        if (!TranslateColumn(infoObj, ti.ParentIDColumn, infoObj.ParentObjectType, targetSiteId, false))
                        {
                            result += ti.ParentIDColumn + ";";
                        }
                    }
                }
            }

            // Raise event to translate custom columns
            if (ColumnsTranslationEvents.TranslateColumns.IsBound)
            {
                ColumnsTranslationEvents.TranslateColumns.StartEvent(this, ti.ObjectType, infoObj);
            }

            return result.TrimEnd(';');
        }


        /// <summary>
        /// Gets read-only collection with names of additional fields.
        /// </summary>
        /// <param name="typeInfo">Type info for which to retrieve the list of additional fields.</param>
        /// <returns>Read-only collection of additional field names.</returns>
        /// <remarks>
        /// <para>
        /// This method is called only if <see cref="UseAdditionalFields"/> is true. Returns
        /// empty collection by default.
        /// </para>
        /// <para>
        /// Override this member in a derived class to specify which columns should be retrieved
        /// along with translation record.
        /// </para>
        /// </remarks>
        internal virtual ICollection<string> GetAdditionalFieldNames(ObjectTypeInfo typeInfo)
        {
            return EMPTY_READ_ONLY_COLLECTION;
        }


        /// <summary>
        /// Gets dictionary with data of additional fields [fieldName -> value]
        /// </summary>
        internal virtual IDictionary<string, object> GetAdditionalFieldsData(BaseInfo infoObject)
        {
            return new Dictionary<string, object>();
        }


        /// <summary>
        /// Gets a loader object allowing to instantiate <see cref="Serialization.TranslationReference"/> from various sources.
        /// </summary>
        internal TranslationReferenceLoader TranslationReferenceLoader
        {
            get
            {
                return mTranslationReferenceLoader ?? (mTranslationReferenceLoader = new TranslationReferenceLoader(this));
            }
        }


        /// <summary>
        /// Returns true of the column is excluded.
        /// </summary>
        /// <param name="columnName">Column name to check</param>
        /// <param name="excludedColumns">Excluded column list separated by semicolon</param>
        public static bool IsExcluded(string columnName, string excludedColumns)
        {
            if (excludedColumns == null)
            {
                return false;
            }

            excludedColumns = ";" + excludedColumns + ";";

            return excludedColumns.IndexOfCSafe(";" + columnName + ";", true) >= 0;
        }


        /// <summary>
        /// Gets the dictionary of new IDs for the given list of old IDs [oldId => [ newId, newGroupId ]]
        /// Caches the newly retrieved items in the conversion cache
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="oldIds">List of old object IDs</param>
        /// <param name="siteId">Target site ID</param>
        public SafeDictionary<int, int> GetNewIDs(string objectType, IEnumerable<int> oldIds, int siteId)
        {
            var result = new SafeDictionary<int, int>();

            // Get the info object for meta-data
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
            if (infoObj == null)
            {
                return result;
            }

            var getFromDb = new List<int>();

            var conversions = EnsureConversionTable(objectType);

            // Pre-filter the IDs and use already cached values
            foreach (var oldId in oldIds)
            {
                // Use the existing conversion if found
                var conversion = conversions[oldId];
                if (conversion != null)
                {
                    // Get the output, convert invalid cached ID to 0
                    result[oldId] = GetOutputId(conversion.ID);
                }
                else
                {
                    // Add to the list for processing
                    getFromDb.Add(oldId);
                }
            }

            // Get the parameters for individual IDs
            var parameters = getFromDb.Select(oldId => GetParametersForId(objectType, oldId, siteId)).Where(p => (p != null));

            // Group into groups where we can search items by code name
            var grouped = parameters.GroupBy(p => p.GetGroupingKey());

            foreach (var group in grouped)
            {
                // Get code names to query
                var codeNames = group.Where(item => !String.IsNullOrEmpty(item.CodeName))
                                     .Select(item => item.CodeName)
                                     .Distinct()
                                     .ToList();

                // Dictionary of found CodeName => ID
                var foundIds = new StringSafeDictionary<int>();

                // We can use any of the parameters for the additional parameters
                var sharedParameters = group.FirstOrDefault();
                if (sharedParameters != null)
                {
                    // Get the data by code names
                    var data = GetIDsFromDB(sharedParameters, infoObj, codeNames);

                    // Save mapping of found code names to new IDs
                    data.ForEachRow(dr =>
                    {
                        var codeName = ValidationHelper.GetString(dr[infoObj.CodeNameColumn], "");
                        var id = ValidationHelper.GetInteger(dr[infoObj.TypeInfo.IDColumn], 0);

                        foundIds[codeName] = id;
                    });

                    // Process all items in group and ensure appropriate mappings
                    foreach (var item in group)
                    {
                        // Check if the item was found
                        var foundId = foundIds[item.CodeName];
                        if (foundId > 0)
                        {
                            // Get the output, convert invalid cached ID to 0
                            result[item.OldId] = foundId;

                            // Cache the conversion
                            conversions[item.OldId] = new ConversionIDs(foundId, item.GroupId);
                        }
                        else
                        {
                            // Cache not found conversion
                            conversions[item.OldId] = new ConversionIDs(DataHelper.FAKE_ID, DataHelper.FAKE_ID);
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the object ID from the database.
        /// </summary>
        /// <param name="parameters">Get ID parameters</param>
        /// <param name="objectType">Object type</param>
        public static int GetIDFromDB(GetIDParameters parameters, string objectType)
        {
            return GetObjectWrapperFromDB(parameters, objectType).ID;
        }


        /// <summary>
        /// Returns wrapped object from database containing desired column values
        /// </summary>
        /// <param name="parameters">Database defining parameter values</param>
        /// <param name="objectType">Desired object type</param>
        private static ObjectDatabaseWrapper GetObjectWrapperFromDB(GetIDParameters parameters, string objectType)
        {
            var isCodeNameProvided = !string.IsNullOrEmpty(parameters.CodeName);
            var isGuidProvided = parameters.Guid != Guid.Empty;
            if (!isCodeNameProvided && !isGuidProvided)
            {
                // Neither code name nor GUID provided to identify an object
                return ObjectDatabaseWrapper.EMPTY;
            }

            GeneralizedInfo generalizedInfo = ModuleManager.GetReadOnlyObject(objectType);
            if (generalizedInfo == null)
            {
                // Info object with meta-data could not be retrieved
                return ObjectDatabaseWrapper.EMPTY;
            }

            // If code name column not specified, try to get from info object 
            if (parameters.CodeNameColumn == null)
            {
                parameters.CodeNameColumn = generalizedInfo.CodeNameColumn;
            }

            // If GUID column not specified, try to get from info object 
            if (parameters.GuidColumn == null)
            {
                parameters.GuidColumn = generalizedInfo.TypeInfo.GUIDColumn;
            }

            // Cannot translate if code name column not found and code name provided or GUID column not found and GUID provided
            var isCodeNameColumnValid = isCodeNameProvided
                && (parameters.CodeNameColumn != null)
                && (parameters.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            var isGuidColumnValid = isGuidProvided
                && (parameters.GuidColumn != null)
                && (parameters.GuidColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
            if (!isCodeNameColumnValid && !isGuidColumnValid)
            {
                return ObjectDatabaseWrapper.EMPTY;
            }

            // Create where condition for code name
            WhereCondition codeNameWhereCondition = null;
            if (isCodeNameColumnValid)
            {
                codeNameWhereCondition = CreateCodeNameWhereCondition(parameters);
            }

            // Create where condition for GUID
            WhereCondition guidWhereCondition = null;
            if (isGuidColumnValid)
            {
                guidWhereCondition = new WhereCondition().WhereEquals(parameters.GuidColumn, parameters.Guid);
            }

            // Combine code name and GUID conditions
            if (isCodeNameColumnValid && isGuidColumnValid)
            {
                return GetIDFromDB(codeNameWhereCondition, guidWhereCondition, parameters, generalizedInfo);
            }

            // Select valid condition, code name or GUID
            var validCondition = isCodeNameColumnValid ? codeNameWhereCondition : guidWhereCondition;
            return GetIDFromDB(validCondition, parameters, generalizedInfo);
        }


        /// <summary>
        /// Creates where condition that selects an object based on CodeName provided in <paramref name="parameters"/>.
        /// </summary>
        private static WhereCondition CreateCodeNameWhereCondition(GetIDParameters parameters)
        {
            var codeNameWhereCondition = new WhereCondition();

            if (parameters.CodeNameColumn.Contains(";"))
            {
                // Full code name made of two columns and one column
                AddFullNameWhereCondition(codeNameWhereCondition, parameters);
            }
            else
            {
                // Standard code name
                codeNameWhereCondition.WhereEquals(parameters.CodeNameColumn, parameters.CodeName);
            }

            return codeNameWhereCondition;
        }


        /// <summary>
        /// Selects both object(s) with given GUID (<paramref name="guidWhereCondition"/>) and with given code name (<paramref name="codeNameWhereCondition"/>)
        /// and returns set of column values of first object with matching GUID (if such object exists) or code name (otherwise).
        /// If there are no such objects, 0 is returned.
        /// </summary>
        private static ObjectDatabaseWrapper GetIDFromDB(IWhereCondition codeNameWhereCondition, IWhereCondition guidWhereCondition, GetIDParameters parameters, GeneralizedInfo generalizedInfo)
        {
            // Combine code name and GUID conditions
            var whereCondition = new WhereCondition(new WhereCondition(codeNameWhereCondition).Or(guidWhereCondition));

            // Add where condition for parent
            AddParentWhereCondition(whereCondition, parameters, generalizedInfo);

            var columns = MergeDefaultWithAdditionalColumns(parameters, generalizedInfo.TypeInfo.IDColumn, parameters.GuidColumn, parameters.CodeNameColumn);

            // Select GUID, CodeName and ID 
            var results = generalizedInfo.GetDataQuery(
                    false,
                    s => s.Columns(columns).Where(whereCondition),
                    false).Select(row => new ObjectDatabaseWrapper()
                    {
                        ID = DataHelper.GetIntValue(row, generalizedInfo.TypeInfo.IDColumn, 0),
                        Guid = DataHelper.GetGuidValue(row, parameters.GuidColumn, Guid.Empty),
                        CodeName = DataHelper.GetStringValue(row, parameters.CodeNameColumn, null),
                        AdditionalFields = GetAdditionalFields(parameters, row)
                    }).ToArray();

            if (results.Any())
            {
                return (results.FirstOrDefault(x => x.Guid == parameters.Guid) ?? results.First());
            }

            return ObjectDatabaseWrapper.EMPTY;
        }


        /// <summary>
        /// Selects either object with given GUID or code name (based on <paramref name="singleIdentifierWhereCondition"/>) and returns set of column values of first such object.
        /// If there is no such object, 0 is returned.
        /// </summary>
        private static ObjectDatabaseWrapper GetIDFromDB(WhereCondition singleIdentifierWhereCondition, GetIDParameters parameters, GeneralizedInfo generalizedInfo)
        {
            // Add where condition for parent
            AddParentWhereCondition(singleIdentifierWhereCondition, parameters, generalizedInfo);

            var columns = MergeDefaultWithAdditionalColumns(parameters, generalizedInfo.TypeInfo.IDColumn, parameters.GuidColumn, parameters.CodeNameColumn);

            // Select GUID, CodeName and ID
            var result = generalizedInfo.GetDataQuery(
                        false,
                        s => s.TopN(1).Columns(columns).Where(singleIdentifierWhereCondition),
                        false).Select(row => new ObjectDatabaseWrapper()
                        {
                            ID = DataHelper.GetIntValue(row, generalizedInfo.TypeInfo.IDColumn, 0),
                            Guid = DataHelper.GetGuidValue(row, parameters.GuidColumn, Guid.Empty),
                            CodeName = DataHelper.GetStringValue(row, parameters.CodeNameColumn, null),
                            AdditionalFields = GetAdditionalFields(parameters, row)
                        }).FirstOrDefault();

            return result ?? ObjectDatabaseWrapper.EMPTY;
        }


        private static IEnumerable<string> MergeDefaultWithAdditionalColumns(GetIDParameters parameters, params string[] defaultColumns)
        {
            var mergedColumns = new List<string>(defaultColumns);
            mergedColumns.AddRange(parameters.AdditionalFields);

            return mergedColumns.Where(c => c != ObjectTypeInfo.COLUMN_NAME_UNKNOWN);
        }


        private static Dictionary<string, object> GetAdditionalFields(GetIDParameters parameters, DataRow dataRow)
        {
            return parameters.AdditionalFields
                             .Where(row => GetIndexOf(dataRow.Table.DataSet, row) != INVALID_INDEX)
                             .ToDictionary(i => i, i => dataRow[i]);
        }


        /// <summary>
        /// Gets the object ID from the database.
        /// </summary>
        /// <param name="parameters">Get ID parameters</param>
        /// <param name="infoObj">Info object to use for meta-data and for getting the data</param>
        /// <param name="codeNames">Code names to get</param>
        private static IDataQuery GetIDsFromDB(GetIDParameters parameters, GeneralizedInfo infoObj, IList<string> codeNames)
        {
            var where = new WhereCondition();

            // Standard code name
            where.WhereIn(parameters.CodeNameColumn, codeNames);

            // Add where condition for parent
            AddParentWhereCondition(where, parameters, infoObj);

            var ti = infoObj.TypeInfo;

            return
                infoObj.GetDataQuery(
                    false,
                    s => s.Columns(ti.IDColumn, infoObj.CodeNameColumn).Where(where),
                    false
                );
        }


        private static void AddFullNameWhereCondition(WhereCondition where, GetIDParameters parameters)
        {
            var codeNameColumns = parameters.CodeNameColumn.Split(';');

            var partColumn = codeNameColumns[0];
            var partColumn2 = codeNameColumns[1];

            // Full code name
            if (!parameters.CodeName.Contains(";"))
            {
                throw new InvalidOperationException("Coupled code name column has wrong value! Missing ';' separator.");
            }

            string[] nameParts = parameters.CodeName.Split(';');

            where.Where(
                new WhereCondition()
                    .WhereEquals(partColumn, nameParts[0])
                    .WhereEquals(partColumn2, nameParts[1])
                );
        }


        private static void AddParentWhereCondition(WhereCondition where, GetIDParameters parameters, GeneralizedInfo infoObj)
        {
            var ti = infoObj.TypeInfo;

            // Site ID where condition
            if (parameters.SiteIdColumn == null)
            {
                parameters.SiteIdColumn = ti.SiteIDColumn;
            }

            where.WhereID(parameters.SiteIdColumn, parameters.SiteId);

            // Parent ID column
            if (parameters.ParentIdColumn == null)
            {
                parameters.ParentIdColumn = ti.ParentIDColumn;
            }

            if ((parameters.ParentIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (parameters.ParentId > 0))
            {
                where.WhereEquals(parameters.ParentIdColumn, parameters.ParentId);
            }

            // Group ID column
            if (parameters.GroupIdColumn == null)
            {
                parameters.GroupIdColumn = ti.GroupIDColumn;
            }

            where.WhereID(parameters.GroupIdColumn, parameters.GroupId);
        }


        /// <summary>
        /// Gets the parent object type for specified object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static string GetParentType(string objectType)
        {
            // Get the object
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);

            return (infoObj != null) ? infoObj.ParentObjectType : null;
        }


        /// <summary>
        /// Reads column index for given column name from data set and returns it.
        /// If column name is not instantiated or set to COLUMN_NAME_UNKNOWN, INVALID_INDEX is returned.
        /// </summary>
        /// <param name="dataSet">Set of data (with column names)</param>
        /// <param name="columnName">Name of a column the index is required for</param>
        private static int GetIndexOf(DataSet dataSet, string columnName)
        {
            var index = INVALID_INDEX;
            if (columnName != null && columnName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                index = dataSet.Tables[0].Columns.IndexOf(columnName);
            }

            return index;
        }


        /// <summary>
        /// Gets translation data from database and returns it as a dictionary of [ID] -> [TranslationData] pairs.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectIDs">Array of object IDs</param>
        /// <param name="siteName">Site name</param>
        private Dictionary<int, TranslationData> GetTranslationDataFromDB(string objectType, IList<int> objectIDs, string siteName)
        {
            var result = new Dictionary<int, TranslationData>();
            string originalObjectType = GetOriginalType(objectType);

            // Check if IDs are available
            if ((objectIDs == null) || (objectIDs.Count == 0))
            {
                return result;
            }

            // Optimized access to site name and class name in case of single record
            if (objectIDs.Count == 1)
            {
                int id = objectIDs[0];
                if (id > 0)
                {
                    switch (originalObjectType)
                    {
                        case PredefinedObjectType.SITE:
                        case DataClassInfo.OBJECT_TYPE:
                            {
                                var info = ProviderHelper.GetInfoById(originalObjectType, id);
                                if ((info != null) && !String.IsNullOrEmpty(info.Generalized.ObjectCodeName))
                                {
                                    result[id] = new TranslationData
                                    {
                                        CodeName = info.Generalized.ObjectCodeName,
                                        SiteName = siteName,
                                        ParentID = 0,
                                        Info = null,
                                        GroupID = 0,
                                        GUID = info.Generalized.ObjectGUID,
                                        ObjectType = info.TypeInfo.ObjectType,
                                        AdditionalFields = GetAdditionalFields(info)
                                    };
                                }
                                return result;
                            }
                    }


                }
            }

            // Check if the info object is available
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(originalObjectType);
            if (infoObj == null)
            {
                return result;
            }

            var originalTypeInfo = infoObj.TypeInfo;
            if (originalTypeInfo.IDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return result;
            }

            // Prepare columns
            HashSet<string> columns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            string siteIDColumn = null;

            columns.Add(originalTypeInfo.IDColumn);

            var codeNameColumn = UseDisplayNameAsCodeName ? infoObj.DisplayNameColumn : infoObj.CodeNameColumn;
            if (codeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return result;
            }
            columns.Add(codeNameColumn);

            var allTypeInfos = new[] { originalTypeInfo }
                .Concat(originalTypeInfo.RelatedTypeInfos.Select(t => ObjectTypeManager.GetTypeInfo(t)))
                .Where(t => t != null)
                .ToArray();

            foreach (var typeInfo in allTypeInfos)
            {
                if ((typeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (siteIDColumn == null))
                {
                    siteIDColumn = typeInfo.SiteIDColumn;
                    columns.Add(typeInfo.SiteIDColumn);
                }
                columns.Add(typeInfo.GroupIDColumn);
                columns.Add(typeInfo.ParentIDColumn);
                columns.Add(typeInfo.GUIDColumn);

                if (UseAdditionalFields)
                {
                    columns.AddRange(GetAdditionalFieldNames(typeInfo));
                }
            }

            // Create type column
            var otherTypes = allTypeInfos.Skip(1).Where(t => t.TypeCondition != null);
            if (otherTypes.Any())
            {
                var typeColumn = new StringBuilder();
                typeColumn.Append(QUERY_OBJECT_TYPE_COLUMN, " = CASE ");

                // Iterate through related types
                foreach (var typeInfo in otherTypes)
                {
                    var condition = typeInfo.TypeCondition.GetWhereCondition().ToString(true);
                    typeColumn.Append("WHEN ", condition, " THEN '", typeInfo.ObjectType, "' ");
                }

                typeColumn.Append("ELSE '", originalObjectType, "' END");

                columns.Add(typeColumn.ToString());
            }
            else
            {
                columns.Add(QUERY_OBJECT_TYPE_COLUMN + " = '" + originalObjectType + "'");
            }

            // Prepare where condition
            var where = new WhereCondition().WhereIn(originalTypeInfo.IDColumn, objectIDs);

            // No data should be returned
            if (where.ReturnsNoResults)
            {
                return result;
            }

            // Add site condition
            bool autoSiteName = false;
            if (siteIDColumn != null)
            {
                switch (siteName)
                {
                    case AUTO_SITENAME:
                        // Automatic site name (get by SiteIDs from DB)
                        autoSiteName = true;
                        break;

                    case NO_SITE:
                        // Do not add site name condition
                        siteName = null;
                        break;

                    case null:
                        // Global objects
                        where.WhereNull(siteIDColumn);
                        break;

                    default:
                        // Add site ID condition
                        int siteId = ProviderHelper.GetId(PredefinedObjectType.SITE, siteName);
                        if (siteId > 0)
                        {
                            where.WhereEquals(siteIDColumn, siteId);
                        }
                        break;
                }
            }

            // Get the records
            var q = infoObj.GetDataQuery(
                false,
                s => s
                    .Where(where)
                    .Columns(SqlHelper.MergeColumns(columns)),
                false
            );

            DataSet dataSet = q.Result;
            if (!DataHelper.DataSourceIsEmpty(dataSet))
            {
                var columnsPerType = new Dictionary<string, ColumnsIndexes>();

                // Fill in the result table
                foreach (DataRow dr in dataSet.Tables[0].Rows)
                {
                    var actualObjectType = ValidationHelper.GetString(dr[QUERY_OBJECT_TYPE_COLUMN], originalObjectType);

                    if (!columnsPerType.ContainsKey(actualObjectType))
                    {
                        var ti = ObjectTypeManager.GetTypeInfo(actualObjectType);


                        int siteIdIndex = -1;
                        if (autoSiteName)
                        {
                            siteIdIndex = GetIndexOf(dataSet, siteIDColumn);
                        }

                        // Initialize column indexes
                        int codeNameIndex;
                        int codeNameIndex2 = -1;

                        // Handle coupled codename column
                        if (codeNameColumn.Contains(";"))
                        {
                            var codeNameCols = codeNameColumn.Split(';');
                            codeNameIndex = GetIndexOf(dataSet, codeNameCols[0]);
                            codeNameIndex2 = GetIndexOf(dataSet, codeNameCols[1]);
                        }
                        else
                        {
                            codeNameIndex = GetIndexOf(dataSet, codeNameColumn);
                        }

                        var indexes = new ColumnsIndexes
                        {
                            SiteIDIndex = siteIdIndex,
                            CodeNameIndex = codeNameIndex,
                            CodeNameIndex2 = codeNameIndex2,
                            IDIndex = GetIndexOf(dataSet, ti.IDColumn),
                            ParentIDIndex = GetIndexOf(dataSet, ti.ParentIDColumn),
                            GroupIDIndex = GetIndexOf(dataSet, ti.GroupIDColumn),
                            GuidIndex = GetIndexOf(dataSet, ti.GUIDColumn),
                        };

                        if (UseAdditionalFields)
                        {
                            indexes.AdditionalFieldsIndex = GetAdditionalFieldNames(ti)
                                .Select(field => new
                                {
                                    FieldName = field,
                                    Index = GetIndexOf(dataSet, field)
                                })
                                .Where(pair => pair.Index != INVALID_INDEX)
                                .ToDictionary(pair => pair.FieldName, pair => pair.Index, StringComparer.InvariantCultureIgnoreCase);
                        }

                        columnsPerType.Add(actualObjectType, indexes);
                    }

                    var columnIndexes = columnsPerType[actualObjectType];

                    // Get the ID
                    int id = ValidationHelper.GetInteger(dr[columnIndexes.IDIndex], 0);
                    object codeNameValue;

                    // if coupled codename column
                    if (columnIndexes.CodeNameIndex2 != INVALID_INDEX && columnIndexes.CodeNameIndex2 >= 0)
                    {
                        codeNameValue = dr[columnIndexes.CodeNameIndex] + ";" + dr[columnIndexes.CodeNameIndex];
                    }
                    else
                    {
                        codeNameValue = dr[columnIndexes.CodeNameIndex];
                    }


                    string codeName = Convert.ToString(codeNameValue);

                    // If either ID or code name is not provided, fail
                    CheckRequiredDataAvailable(objectType, id, codeName);

                    // Parent ID
                    var parentId = 0;
                    if (columnIndexes.ParentIDIndex >= 0)
                    {
                        parentId = ValidationHelper.GetInteger(dr[columnIndexes.ParentIDIndex], 0);
                    }

                    // Get site name if automatic
                    if (autoSiteName)
                    {
                        object siteIdValue = dr[columnIndexes.SiteIDIndex];
                        if (siteIdValue == DBNull.Value)
                        {
                            siteName = null;
                        }
                        else
                        {
                            // Get the site translation
                            int siteId = Convert.ToInt32(siteIdValue);
                            siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);
                        }
                    }

                    // Group ID
                    var groupId = 0;
                    if (columnIndexes.GroupIDIndex >= 0)
                    {
                        groupId = ValidationHelper.GetInteger(dr[columnIndexes.GroupIDIndex], 0);
                    }

                    // GUID
                    var guid = Guid.Empty;
                    if (columnIndexes.GuidIndex >= 0)
                    {
                        guid = ValidationHelper.GetGuid(dr[columnIndexes.GuidIndex], Guid.Empty);
                    }

                    // Additional fields
                    Dictionary<string, object> additionalFields = null;
                    if (columnIndexes.AdditionalFieldsIndex != null)
                    {
                        additionalFields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                        foreach (var additionalField in columnIndexes.AdditionalFieldsIndex)
                        {
                            additionalFields.Add(additionalField.Key, dr[additionalField.Value]);
                        }
                    }

                    result[id] = new TranslationData
                    {
                        CodeName = codeName,
                        SiteName = siteName,
                        ParentID = parentId,
                        Info = null,
                        GroupID = groupId,
                        GUID = guid,
                        ObjectType = actualObjectType,
                        AdditionalFields = additionalFields
                    };
                }
            }

            return result;
        }


        private Dictionary<string, object> GetAdditionalFields(BaseInfo info)
        {
            return UseAdditionalFields ? GetAdditionalFieldNames(info.TypeInfo).ToDictionary(it => it, info.GetValue) : null;
        }


        /// <summary>
        /// Gets the safe class name.
        /// </summary>
        /// <param name="className">Class name</param>
        public static string GetSafeClassName(string className)
        {
            if (className == null)
            {
                return null;
            }
            return className.Replace(".", "_");
        }


        #endregion 
    }
}