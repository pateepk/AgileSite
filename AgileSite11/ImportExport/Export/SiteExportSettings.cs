using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class representing export settings.
    /// </summary>
    [Serializable]
    public class SiteExportSettings : AbstractImportExportSettings
    {
        #region "Variables"

        private string mTargetPath;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if the files should be processed.
        /// </summary>
        public bool CopyFiles
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Target path for the export package.
        /// </summary>
        public string TargetPath
        {
            get
            {
                // Target path has to be always initialized
                if (mTargetPath == null)
                {
                    throw new Exception("[SiteExportSettings]: TargetPath not initialized!");
                }
                return mTargetPath;
            }
            set
            {
                if (value != null)
                {
                    // Ensure correct ending
                    value = Path.EnsureEndBackslash(value);
                }
                mTargetPath = value;
            }
        }


        /// <summary>
        /// Gets complete target full path with file name.
        /// </summary>
        public string TargetFullPath
        {
            get
            {
                return TargetPath + TargetFileName;
            }
        }


        /// <summary>
        /// If set, only objects modified after this date are exported.
        /// </summary>
        public DateTime TimeStamp
        {
            set;
            get;
        } = DateTimeHelper.ZERO_TIME;


        /// <summary>
        /// Indicates if the ZIP package should be created.
        /// </summary>
        public bool CreatePackage
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Target file name of the export package.
        /// </summary>
        public string TargetFileName
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the export.
        /// </summary>
        public ExportTypeEnum ExportType
        {
            get;
            set;
        } = ExportTypeEnum.Site;


        /// <summary>
        /// Version of the package.
        /// </summary>
        public override string Version
        {
            get
            {
                return base.Version ?? (base.Version = CMSVersion.MainVersion);
            }
            set
            {
                base.Version = value;
            }
        }


        /// <summary>
        /// Hotfix version of the package.
        /// </summary>
        public override string HotfixVersion
        {
            get
            {
                return base.HotfixVersion ?? (base.HotfixVersion = CMSVersion.HotfixVersion);
            }
            set
            {
                base.HotfixVersion = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates export settings.
        /// </summary>
        /// <param name="userInfo">Current user info</param>
        public SiteExportSettings(IUserInfo userInfo)
            : base(userInfo)
        {
            // Initialize progress log and additional information
            EventLogCode = "EXPORT";
            EventLogSource = GetAPIString("ExportSite.EventLogSource", "Export objects");
        }


        /// <summary>
        /// Gets data for serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info, ctxt);

            info.AddValue("TargetFileName", TargetFileName);
            info.AddValue("TargetPath", mTargetPath);
            info.AddValue("SiteId", mSiteId);
            info.AddValue("ExcludedNames", mExcludedNames);
            info.AddValue("CopyFiles", CopyFiles);
            info.AddValue("ExportType", ExportType);
        }


        /// <summary>
        /// Constructor - Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="ctxt">Streaming context</param>
        public SiteExportSettings(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            TargetFileName = (string)info.GetValue("TargetFileName", typeof(string));
            mTargetPath = (string)info.GetValue("TargetPath", typeof(string));
            mExcludedNames = (string[])info.GetValue("ExcludedNames", typeof(string[]));
            CopyFiles = (bool)info.GetValue("CopyFiles", typeof(bool));
            ExportType = (ExportTypeEnum)info.GetValue("ExportType", typeof(ExportTypeEnum));
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Selects the exported objects by their name prefixes
        /// </summary>
        /// <param name="objectTypes">Object types, if null, includes all object types</param>
        /// <param name="codeNamePrefix">Code name prefix</param>
        /// <param name="resourceId">Module ID to select objects that may be assigned to a module</param>
        public void SelectGlobalObjects(IEnumerable<string> objectTypes, string codeNamePrefix, int resourceId = 0)
        {
            objectTypes = objectTypes ?? ImportExportHelper.ObjectTypes.Where(x => !x.IsSite).Select(x => x.ObjectType);

            foreach (var type in objectTypes)
            {
                string objectType = type;

                var infoObj = ModuleManager.GetObject(objectType);
                var codeNameColumn = infoObj.Generalized.CodeNameColumn;

                var q = infoObj.Generalized.GetDataQuery(
                    true,
                    settings =>
                    {
                        settings.Column(codeNameColumn);

                        // Set base object where condition for selection
                        var baseWhere = GetObjectWhereCondition(objectType, false);
                        settings.Where(baseWhere);

                        // Either is assigned to a specific resource, or global and selected by code name
                        var ti = infoObj.TypeInfo;
                        if ((ti.ResourceIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (resourceId > 0))
                        {
                            settings.WhereID(ti.ResourceIDColumn, resourceId);
                            settings.Or();
                        }

                        settings.Where(w =>
                        {
                            // Select by name only objects that are not assigned to resource
                            if (ti.ResourceIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                            {
                                w.WhereID(ti.ResourceIDColumn, resourceId);
                            }

                            // Select by name prefix
                            w.WhereStartsWith(codeNameColumn, codeNamePrefix);
                        });
                    },
                    false
                );

                // Ensure the status for selected objects
                if (!DataHelper.DataSourceIsEmpty(q.Result))
                {
                    EnsureSelectedObjectsExport(objectType, false);
                }

                q.ForEachRow(obj => Select(objectType, (string)obj[codeNameColumn], false));
            }
        }


        /// <summary>
        /// Ensures that the given object type can include selected objects into the export
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObject">Site object</param>
        public void EnsureSelectedObjectsExport(string objectType, bool siteObject)
        {
            if (GetObjectsProcessType(objectType, siteObject) != ProcessObjectEnum.All)
            {
                SetObjectsProcessType(ProcessObjectEnum.Selected, objectType, siteObject);
            }
        }


        /// <summary>
        /// Loads the default selection to the export settings tables.
        /// </summary>
        /// <param name="loadAdditionalSettings">Indicates if additional settings should be loaded</param>
        public void LoadDefaultSelection(bool loadAdditionalSettings = true)
        {
            // Go through all object types in reverse order (import goes in standard order)
            for (int i = ImportExportHelper.ObjectTypes.Count - 1; i >= 0; --i)
            {
                var item = ImportExportHelper.ObjectTypes[i];

                string objectType = item.ObjectType;
                bool siteObject = item.IsSite;

                var e = new ExportLoadSelectionArgs
                {
                    ObjectType = objectType,
                    SiteObject = siteObject,
                    Settings = this
                };

                // Handle the event
                SpecialActionsEvents.ExportLoadDefaultSelection.StartEvent(e);

                if (e.Select)
                {
                    var parameters = new DefaultSelectionParameters
                    {
                        ObjectType = objectType,
                        SiteObjects = siteObject,
                        ExportType = ExportTypeEnum.Default
                    };

                    LoadDefaultSelection(parameters);
                }
            }

            // Ensure other important settings
            bool exportSettings = (ExportType != ExportTypeEnum.None);

            SetSettings(ImportExportHelper.SETTINGS_DOC_HISTORY, exportSettings);
            SetSettings(ImportExportHelper.SETTINGS_DOC_ACLS, exportSettings);
            SetSettings(ImportExportHelper.SETTINGS_DOC_RELATIONSHIPS, exportSettings);
            SetSettings(ImportExportHelper.SETTINGS_EVENT_ATTENDEES, exportSettings);
            SetSettings(ImportExportHelper.SETTINGS_BLOG_COMMENTS, exportSettings);

            // Ensure additional settings
            if (loadAdditionalSettings)
            {
                SetSettings(ImportExportHelper.SETTINGS_CUSTOMTABLE_DATA, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_BIZFORM_DATA, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_FORUM_POSTS, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_BOARD_MESSAGES, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_GLOBAL_FOLDERS, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_SITE_FOLDERS, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_COPY_ASPX_TEMPLATES_FOLDER, exportSettings);
                SetSettings(ImportExportHelper.SETTINGS_COPY_FORUM_CUSTOM_LAYOUTS_FOLDER, exportSettings);
            }
        }


        /// <summary>
        /// Loads the default selection to the import settings tables.
        /// </summary>
        /// <param name="parameters">object containing selection parameters</param>
        public void LoadDefaultSelection(DefaultSelectionParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            try
            {
                // Clear the event log
                EventLog = null;

                // Clear progress log
                if (parameters.ClearProgressLog)
                {
                    ClearProgressLog();
                }

                // Get object type info
                GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(parameters.ObjectType);
                if (infoObj != null)
                {
                    var ti = infoObj.TypeInfo;
                                    
                    // Do not preselect automatically selected object types or object types that are not explicitly exportable (such as objects that are not exportable on their own, but support single object export)
                    if (IsAutomaticallySelected(infoObj) || !ti.ImportExportSettings.IsExportable)
                    {
                        return;
                    }

                    // Code name column of the object type has to be defined
                    if (infoObj.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        throw new Exception("[SiteExportSettings.LoadDefaultSelection]: Missing code name column information for object type '" + parameters.ObjectType + "'.");
                    }

                    // Prepare list for objects and tasks
                    var list = new List<string>();
                    List<int> taskList = null;

                    // Use default object type settings
                    if (parameters.ExportType == ExportTypeEnum.Default)
                    {
                        parameters.ExportType = ExportType;
                    }

                    // Object type should be exported
                    if (parameters.ExportType != ExportTypeEnum.None)
                    {
                        if (parameters.LoadObjects)
                        {
                            bool getData = true;

                            // Get base where condition
                            var where = GetObjectWhereCondition(parameters.ObjectType, parameters.SiteObjects);

                            // Add filter condition
                            if (!string.IsNullOrEmpty(parameters.FilterCurrentWhereCondition))
                            {
                                where = where.And(new WhereCondition(parameters.FilterCurrentWhereCondition));
                            }

                            // Add additional where condition due to site bindings
                            if (parameters.ExportType != ExportTypeEnum.All)
                            {
                                if (!parameters.SiteObjects && (TimeStamp == DateTimeHelper.ZERO_TIME) && (SiteId > 0))
                                {
                                    var siteWhere = new WhereCondition();

                                    if ((ti.BindingObjectTypes != null) && (ti.BindingObjectTypes.Count > 0))
                                    {
                                        // Process all bindings
                                        foreach (string bindingType in ti.BindingObjectTypes)
                                        {
                                            GeneralizedInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingType);
                                            if (bindingObj == null)
                                            {
                                                throw new Exception("[SiteExportSettings.LoadDefaultSelection]: Binding object type '" + parameters.ObjectType + "' not found.");
                                            }

                                            // If binding to site, add where condition for site
                                            var bindingTypeInfo = bindingObj.TypeInfo;
                                            if (bindingTypeInfo.IsSiteBinding)
                                            {
                                                IList<int> objectIDs = null;

                                                // Get the bindings
                                                DataSet bindingsDS =
                                                    bindingObj.GetDataQuery(
                                                        true,
                                                        q => q
                                                            .WhereEquals(bindingTypeInfo.SiteIDColumn, SiteId)
                                                            .Column(bindingTypeInfo.ParentIDColumn),
                                                        false
                                                        ).Result;

                                                if (!DataHelper.DataSourceIsEmpty(bindingsDS))
                                                {
                                                    objectIDs = DataHelper.GetIntegerValues(bindingsDS.Tables[0], bindingTypeInfo.ParentIDColumn);
                                                }

                                                // Prepare the site bindings selection
                                                siteWhere.WhereIn(ti.IDColumn, objectIDs);

                                                // If the site condition is empty, do not attempt to get any data
                                                if (siteWhere.ReturnsNoResults)
                                                {
                                                    getData = false;
                                                }
                                                break;
                                            }
                                        }
                                    }

                                    var e = new ExportSelectionArgs
                                    {
                                        Settings = this,
                                        ObjectType = parameters.ObjectType,
                                        SiteObject = true,
                                        Select = getData,
                                        Where = siteWhere
                                    };

                                    // Handle the event
                                    SpecialActionsEvents.GetSelectionWhereCondition.StartEvent(e);

                                    getData = e.Select;
                                    siteWhere = e.Where;

                                    if (getData)
                                    {
                                        // Add additional where condition due to depending objects
                                        if (e.IncludeDependingObjects)
                                        {
                                            // Get depending object types
                                            siteWhere = GetDependingObjectWhere(parameters.ObjectType, infoObj, siteWhere);
                                        }

                                        // Combine the where conditions
                                        where.Where(siteWhere);
                                    }
                                }
                            }

                            // If time stamp is set, only objects modified after this time stamp are retrieved
                            DateTime timestamp = TimeStamp;

                            // Always include site object
                            if (parameters.ObjectType == SiteInfo.OBJECT_TYPE)
                            {
                                timestamp = DateTimeHelper.ZERO_TIME;
                            }

                            if (getData)
                            {
                                // Get modified objects
                                var q = infoObj.GetModifiedFrom(timestamp);

                                q.ApplySettings(s => s
                                    .Where(where)
                                    .Column(infoObj.CodeNameColumn)
                                    );

                                q.ForEachRow(dr =>
                                {
                                    string codeName = ValidationHelper.GetString(dr[0], "");

                                    list.Add(codeName.ToLowerCSafe());
                                });
                            }
                        }

                        if (parameters.LoadTasks)
                        {
                            taskList = GetExportTaskIds(parameters);
                        }
                    }

                    // Set selected objects
                    if (parameters.LoadObjects)
                    {
                        SetSelectedObjects(list, parameters.ObjectType, parameters.SiteObjects);
                    }

                    // Set selected tasks
                    if (parameters.LoadTasks)
                    {
                        SetSelectedTasks(taskList, parameters.ObjectType, parameters.SiteObjects);
                    }
                }
                else
                {
                    throw new Exception("[SiteExportSettings.LoadDefaultSelection]: Object type '" + parameters.ObjectType + "' not found.");
                }
            }
            catch (Exception ex)
            {
                // Log error
                LogProgressError(null, ex);

                // Write log to the event log
                FinalizeEventLog();

                throw;
            }
        }


        private List<int> GetExportTaskIds(DefaultSelectionParameters parameters)
        {
            var taskList = new List<int>();

            // Get objects tasks
            if (ValidationHelper.GetBoolean(GetSettings(ImportExportHelper.SETTINGS_TASKS), true))
            {
                // Prepare where condition
                int siteId = (parameters.SiteObjects ? SiteId : 0);
                var where = ExportTaskInfoProvider.GetTasksWhereCondition(siteId, parameters.ObjectType, null);

                // Get the data
                var taskObj = new ExportTaskInfo();

                var q = taskObj.Generalized.GetModifiedFrom(TimeStamp, s => s
                    .Where(@where)
                    .Column("TaskID")
                    );

                // Fill in the task list
                q.ForEachRow(dr =>
                {
                    int taskId = (int)dr[0];

                    taskList.Add(taskId);
                });
            }

            return taskList;
        }


        private WhereCondition GetDependingObjectWhere(string objectType, GeneralizedInfo infoObj, WhereCondition siteWhere)
        {
            var where = new WhereCondition(siteWhere);
            var dependingTypes = GetDependingObjectTypes(objectType, true, false);

            foreach (object[] dependingRecord in dependingTypes)
            {
                string rootType = (string)dependingRecord[0];
                if (rootType == objectType)
                {
                    continue;
                }

                bool siteObject = (bool)dependingRecord[1];
                string dependingType = (string)dependingRecord[2];

                // Get depending object type info
                GeneralizedInfo dependingObj = ModuleManager.GetReadOnlyObject(dependingType);
                if (dependingObj == null)
                {
                    throw new Exception("[SiteExportSettings.LoadDefaultSelection]: Depending object type '" + dependingType + "' not found.");
                }

                // Get dependingType dependencies list
                var dependencyColumns = ObjectHelper.GetDependencyColumnNames(dependingType, objectType);
                if (dependencyColumns != null)
                {
                    // Root type - get depending objects
                    bool isChild = (rootType.ToLowerCSafe() != dependingType.ToLowerCSafe());

                    DataSet dependingDS = ExportProvider.GetExportData(this, rootType, siteObject, isChild, true, null);
                    if (!DataHelper.DataSourceIsEmpty(dependingDS))
                    {
                        DataTable dependingDT = ObjectHelper.GetTable(dependingDS, dependingObj);
                        if (dependingDT != null)
                        {
                            IList<int> dependingIDs = new List<int>();

                            foreach (var dependencyColumn in dependencyColumns)
                            {
                                if (dependingDT.Columns.Contains(dependencyColumn))
                                {
                                    // Get list of IDs to export all dependencies
                                    dependingIDs = DataHelper.GetIntegerValues(dependingDT, dependencyColumn).Union(dependingIDs).ToList();
                                }
                            }

                            if (dependingIDs.Count > 0)
                            {
                                // Dependency condition
                                where.Or().WhereIn(infoObj.TypeInfo.IDColumn, dependingIDs);
                            }
                        }
                    }
                }
            }

            return where;
        }


        /// <summary>
        /// Ensures automatic selection for given object type.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="siteObjects">Indicates if object is site object</param>
        public void EnsureAutomaticSelection(GeneralizedInfo infoObj, bool siteObjects)
        {
            if (infoObj != null)
            {
                var ti = infoObj.TypeInfo;

                var e = new ExportLoadSelectionArgs
                {
                    ObjectType = ti.ObjectType,
                    SiteObject = siteObjects,
                    Settings = this,
                    DependencyObjectType = infoObj.ParentObjectType,
                    DependencyIDColumn = ti.ParentIDColumn,
                };

                // Get parent where condition
                if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    e.DependencyObject = ModuleManager.GetReadOnlyObject(infoObj.ParentObjectType);
                }

                // Handle the event
                SpecialActionsEvents.ExportEnsureAutomaticSelection.StartEvent(e);

                var dependencyObject = e.DependencyObject;

                if (e.Select && (dependencyObject != null))
                {
                    var depTypeInfo = dependencyObject.TypeInfo;

                    bool siteObj = (depTypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && e.DependencyIsSiteObject;
                    string[] codeNames = GetSelectedObjectsArray(e.DependencyObjectType, siteObj);

                    // Prepare the selection via code names
                    QueryDataParameters parameters = new QueryDataParameters();

                    using (SelectCondition cond = new SelectCondition(parameters))
                    {
                        cond.PrepareCondition(dependencyObject.Generalized.CodeNameColumn, codeNames);

                        string parentWhere = cond.WhereCondition;

                        // Some data should be selected
                        if (parentWhere != SqlHelper.NO_DATA_WHERE)
                        {
                            // Prepare the child query
                            var childQuery =
                                dependencyObject.Generalized.GetDataQuery(
                                    true,
                                    q => q
                                        .Where(parentWhere, parameters)
                                        .Columns(depTypeInfo.IDColumn),
                                    false
                                 );

                            // Get the data
                            var query =
                                infoObj.GetDataQuery(
                                    true,
                                    q => q
                                        .WhereIn(e.DependencyIDColumn, childQuery)
                                        .Column(infoObj.CodeNameColumn),
                                    false
                                );

                            // Process code names from result
                            var ds = query.Result;

                            if (!DataHelper.DataSourceIsEmpty(ds))
                            {
                                var childCodeNames = DataHelper.GetStringValues(ds.Tables[0], ds.Tables[0].Columns[0].ColumnName);

                                // Set selected objects
                                SetSelectedObjects(childCodeNames, ti.ObjectType, siteObjects);
                            }
                        }
                        else
                        {
                            // Deselect all objects
                            SetSelectedObjects(null, ti.ObjectType, siteObjects);
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Returns the array of object types that depend on specified object type [root type, site object, depending type].
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="child">Seek the child types</param>
        /// <param name="binding">Seek the binding types</param>
        private IEnumerable<object[]> GetDependingObjectTypes(string objectType, bool child, bool binding)
        {
            var list = new List<object[]>();

            // Loop through all object types
            for (int i = ImportExportHelper.ObjectTypes.Count - 1; i >= 0; --i)
            {
                var item = ImportExportHelper.ObjectTypes[i];

                string dependingType = item.ObjectType;
                bool siteObjects = item.IsSite;

                // Do not include excluded depending types
                var dep = ModuleManager.GetReadOnlyObject(dependingType);
                if (dep.TypeInfo.ImportExportSettings.ExcludedDependingType)
                {
                    continue;
                }

                // Process the object
                if (!siteObjects || (SiteId > 0))
                {
                    List<string> types = new List<string>();
                    ObjectHelper.AddDependingObjectTypes(types, objectType, dependingType, child, binding);

                    // Add all types
                    foreach (string type in types)
                    {
                        list.Add(new object[] { dependingType, siteObjects, type });
                    }
                }
            }

            return list;
        }


        /// <summary>
        /// Returns true if the given object type is exported type (root type).
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static bool IsExportedObjectType(string objectType)
        {
            objectType = objectType.ToLowerCSafe();

            // Check all types which are exported
            for (int i = ImportExportHelper.ObjectTypes.Count - 1; i >= 0; --i)
            {
                var item = ImportExportHelper.ObjectTypes[i];

                // Check type
                string exportedType = item.ObjectType.ToLowerCSafe();
                if (exportedType == objectType)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns export settings configuration in XML.
        /// </summary>
        public string GetXML()
        {
            StringBuilder builder = new StringBuilder();
            // Create xml writer
            XmlWriter writer = XmlWriter.Create(builder);

            // <settings> - top element
            writer.WriteStartElement("settings");

            // <configuration ... - contains basic settings as attributes
            writer.WriteStartElement("configuration");
            //      sitename="..."
            writer.WriteAttributeString("sitename", SiteName);
            //      targetfilename="..."
            writer.WriteAttributeString("targetfilename", TargetFileName);
            //      timestamp="..."
            writer.WriteAttributeString("timestamp", TimeStamp.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            //      createpackage="..."
            writer.WriteAttributeString("createpackage", Convert.ToString(CreatePackage));
            //      defaultprocessobjecttype="..."
            writer.WriteAttributeString("defaultprocessobjecttype", ValidationHelper.GetString(Enum.GetName(typeof(ProcessObjectEnum), DefaultProcessObjectType), String.Empty));

            /* Not persistent values, do not save
            //      temporaryfilespath="..."
            //writer.WriteAttributeString("temporaryfilespath", this.TemporaryFilesPath);
            //      evntlogid="..."
            //writer.WriteAttributeString("eventlogid", Convert.ToString(this.EventLogId));
            */

            if (ExcludedNames != null)
            {
                // <excludedNames> - contains excluded object names
                writer.WriteStartElement("excludedNames");
                // Add names
                string names = "";
                foreach (string name in ExcludedNames)
                {
                    // Concatenate names
                    names += name + ";";
                }
                names = names.TrimEnd(';');
                // Write names separated by ';'
                writer.WriteString(names);

                // </excludednames> - excluded object names end element
                writer.WriteEndElement();
            }

            // </configuration>
            writer.WriteEndElement();

            // Write selected global object hashtable
            WriteHashtableXML(writer, mSelectedGlobalObjectsHashtable, "globalSelectedObjects");

            // Write selected site object hashtable
            WriteHashtableXML(writer, mSelectedSiteObjectsHashtable, "siteSelectedObjects");

            // Write additional settings hashtable
            WriteHashtableXML(writer, mSettingsHashtable, "additionalSettings");

            // Write process types of objects hashtable
            WriteHashtableXML(writer, mProcessGlobalObjectsHashtable, "globalProcessedObjects");

            // Write process types of objects hashtable
            WriteHashtableXML(writer, mProcessSiteObjectsHashtable, "siteProcessedObjects");

            // </settings> - settings end element (end of xml content)
            writer.WriteEndElement();

            writer.Flush();

            // Return created xml
            return builder.ToString();
        }


        /// <summary>
        /// Gets XML for given hashtable.
        /// </summary>
        /// <param name="writer">XML writer</param>
        /// <param name="hashtable">Hashtable to process</param>
        /// <param name="tagName">Start element of the XML</param>
        private void WriteHashtableXML(XmlWriter writer, IDictionary hashtable, string tagName)
        {
            if (hashtable != null)
            {
                // Start element
                writer.WriteStartElement(tagName);

                // Add entries from hashtable
                foreach (DictionaryEntry de in hashtable)
                {
                    // Entry tag
                    writer.WriteStartElement("entry");
                    //      type="..." - attribute for value
                    writer.WriteAttributeString("key", ValidationHelper.GetString(de.Key, ""));

                    var list = de.Value as List<string>;
                    if (list != null)
                    {
                        // Concatenate code names from List<string>
                        string codes = list.Join(";");

                        // Write code names separated by ';'
                        writer.WriteString(codes);
                    }
                    else
                    {
                        writer.WriteString(ValidationHelper.GetString(de.Value, ""));
                    }

                    // Entry end element
                    writer.WriteEndElement();
                }

                // End element
                writer.WriteEndElement();
            }
        }


        /// <summary>
        /// Loads export settings configuration from XML.
        /// </summary>
        /// <param name="xml">XML with configuration</param>
        public void LoadFromXML(string xml)
        {
            // Type of the hashtable
            HashtableEnum hashtableType = HashtableEnum.AdditionalSettings;
            bool excludedNames = false;
            // Key name value
            string key = null;

            // Clear hashtables
            mSelectedGlobalObjectsHashtable.Clear();
            mSelectedSiteObjectsHashtable.Clear();

            // Create xml reader
            StringReader sr = new StringReader(xml);
            XmlReader reader = XmlReader.Create(sr);

            // Move to xml content
            reader.MoveToContent();

            // Check that 1st node name is 'settings'
            if (reader.Name.ToLowerCSafe() == "settings")
            {
                // Parse the file
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        // Element node type
                        case XmlNodeType.Element:
                            switch (reader.Name.ToLowerCSafe())
                            {
                                // Configuration element contains basic attributs
                                case "configuration":
                                    if (reader.HasAttributes)
                                    {
                                        for (int i = 0; i < reader.AttributeCount; i++)
                                        {
                                            reader.MoveToAttribute(i);
                                            switch (reader.Name.ToLowerCSafe())
                                            {
                                                // Site name
                                                case "sitename":
                                                    SiteName = reader.Value;
                                                    break;

                                                // Target file name
                                                case "targetfilename":
                                                    TargetFileName = reader.Value;
                                                    break;

                                                // Time stamp
                                                case "timestamp":
                                                    if (ValidationHelper.GetDateTime(reader.Value, DateTimeHelper.ZERO_TIME) != DateTimeHelper.ZERO_TIME)
                                                    {
                                                        TimeStamp = ValidationHelper.GetDateTime(reader.Value, DateTimeHelper.ZERO_TIME);
                                                    }
                                                    break;

                                                // Create package
                                                case "createpackage":
                                                    if (reader.Value.ToLowerCSafe() == "true" || reader.Value.ToLowerCSafe() == "false")
                                                    {
                                                        CreatePackage = Convert.ToBoolean(reader.Value);
                                                    }
                                                    break;

                                                // Excluded names
                                                case "excludednames":
                                                    excludedNames = true;
                                                    break;

                                                // Default process object type
                                                case "defaultprocessobjecttype":
                                                    DefaultProcessObjectType = (ProcessObjectEnum)Enum.Parse(typeof(ProcessObjectEnum), reader.Value, true);
                                                    break;

                                                /* Not persistent values, do not load
                                            //// Temporary files path
                                            //case "temporaryfilespath":
                                            //    this.TemporaryFilesPath = reader.Value;
                                            //    break;

                                            //// Eventlog id
                                            //case "eventlogid":
                                            //    this.EventLogId = Convert.ToInt32(reader.Value);
                                            //    break;
                                            */
                                            }
                                        }
                                        // Move the reader back to the element node
                                        reader.MoveToElement();
                                    }
                                    break;

                                // Global selected objects
                                case "globalselectedobjects":
                                    hashtableType = HashtableEnum.GlobalSelectedObjects;
                                    break;

                                // Site selected objects
                                case "siteselectedobjects":
                                    hashtableType = HashtableEnum.SiteSelectedObjects;
                                    break;

                                // Global processed objects
                                case "globalprocessedobjects":
                                    hashtableType = HashtableEnum.GlobalProcessedObjects;
                                    break;

                                // Site processed objects
                                case "siteprocessedobjects":
                                    hashtableType = HashtableEnum.SiteProcessedObjects;
                                    break;

                                // Additional settings
                                case "additionalsettings":
                                    hashtableType = HashtableEnum.AdditionalSettings;
                                    break;

                                // entry node
                                case "entry":
                                    if (reader.HasAttributes)
                                    {
                                        reader.MoveToFirstAttribute();
                                        if (reader.Name.ToLowerCSafe() == "key")
                                        {
                                            // Get code names type
                                            key = reader.Value;
                                        }
                                        // Return to attribute's owner element
                                        reader.MoveToElement();
                                    }
                                    break;
                            }
                            break;

                        // Text node type (code names separated by ';' or additional settings key or excldued names)
                        case XmlNodeType.Text:
                            if (excludedNames)
                            {
                                string[] names = reader.Value.Split(';');
                                if (names.Length > 0)
                                {
                                    ExcludedNames = names;
                                }
                            }
                            else
                            {
                                switch (hashtableType)
                                {
                                    case HashtableEnum.AdditionalSettings:
                                        mSettingsHashtable[key] = ValidationHelper.GetBoolean(reader.Value, false);
                                        break;

                                    case HashtableEnum.GlobalSelectedObjects:
                                    case HashtableEnum.SiteSelectedObjects:
                                        string[] codes = reader.Value.Split(';');
                                        if (codes.Length > 0 && !DataHelper.IsEmpty(key))
                                        {
                                            List<string> list = new List<string>();
                                            // Fill array list with code names
                                            foreach (string code in codes)
                                            {
                                                list.Add(code);
                                            }

                                            if (list.Count > 0)
                                            {
                                                if (hashtableType == HashtableEnum.GlobalSelectedObjects)
                                                {
                                                    // Add code names of specific type to global selected objects hashtable
                                                    mSelectedGlobalObjectsHashtable.Add(key, list);
                                                }
                                                else
                                                {
                                                    // Add code names of specific type to site selected objects hashtable
                                                    mSelectedSiteObjectsHashtable.Add(key, list);
                                                }
                                            }
                                        }
                                        break;

                                    case HashtableEnum.GlobalProcessedObjects:
                                    case HashtableEnum.SiteProcessedObjects:
                                        if (hashtableType == HashtableEnum.GlobalProcessedObjects)
                                        {
                                            // Add code names of specific type to global processed objects hashtable
                                            mProcessGlobalObjectsHashtable.Add(key, (ProcessObjectEnum)Enum.Parse(typeof(ProcessObjectEnum), reader.Value, true));
                                        }
                                        else
                                        {
                                            // Add code names of specific type to site processed objects hashtable
                                            mProcessSiteObjectsHashtable.Add(key, (ProcessObjectEnum)Enum.Parse(typeof(ProcessObjectEnum), reader.Value, true));
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
        }

        #endregion
    }
}