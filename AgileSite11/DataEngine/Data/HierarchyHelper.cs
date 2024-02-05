using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Linq;

using CMS.Helpers;
using CMS.Base;
using CMS.Core;

using SystemIO = System.IO;

namespace CMS.DataEngine
{
    using TranslationsTable = SafeDictionary<string, List<int>>;

    /// <summary>
    /// Object hierarchy manipulation methods.
    /// </summary>
    public class HierarchyHelper
    {
        #region "Variables"

        private readonly TranslationsTable mTranslations;
        private readonly StringBuilder mResultBuilder;
        private readonly TraverseObjectSettings mSettings;
        private readonly GeneralizedInfo mInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Settings
        /// </summary>
        protected TraverseObjectSettings Settings
        {
            get
            {
                return mSettings;
            }
        }


        /// <summary>
        /// Result builder
        /// </summary>
        protected StringBuilder ResultBuilder
        {
            get
            {
                return mResultBuilder;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor for inheritance
        /// </summary>
        /// <param name="settings">Settings of the serialization</param>
        protected HierarchyHelper(TraverseObjectSettings settings)
        {
            mSettings = settings;
            mResultBuilder = new StringBuilder();
            mTranslations = new TranslationsTable();
        }


        /// <summary>
        /// Creates new instance of CMSHierarchyHelper which can be used to serialize given object.
        /// </summary>
        /// <param name="settings">Settings of the serialization</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        protected HierarchyHelper(TraverseObjectSettings settings, GeneralizedInfo info)
            : this(settings)
        {
            mInfo = info;
        }

        #endregion


        #region "General object traversal methods"

        /// <summary>
        /// Goes through the object structure and calls given delegates.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        public static void TraverseObjectStructure(TraverseObjectSettings settings, GeneralizedInfo info)
        {
            new HierarchyHelper(settings).TraverseObjectStructure(settings, info, 0);
        }


        /// <summary>
        /// Goes through the object structure and calls given delegates.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        protected virtual void TraverseObjectStructure(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            if (info != null)
            {
                using (var scope = new CMSConnectionScope())
                {
                    if (!info.IsDisconnected)
                    {
                        // Ensure open connection for the whole traverse process if the object is connected to DB
                        scope.Open();
                    }

                    // Check if not excluded
                    if (IsExcluded(settings, info))
                    {
                        return;
                    }

                    ExportParentCategories(settings, info, currentLevel);

                    CallObjectCallback(settings, info, currentLevel);

                    ProcessTranslations(settings, info);

                    ExportChildDependencies(settings, info, currentLevel);
                    ExportChildren(settings, info, currentLevel);
                    ExportBindings(settings, info, currentLevel);
                    ExportOtherBindings(settings, info, currentLevel);

                    ExportCategories(settings, info, currentLevel);

                    ExportMetafiles(settings, info, currentLevel);
                    ExportScheduledTasks(settings, info, currentLevel);
                    ExportProcesses(settings, info, currentLevel);
                }
            }
        }


        /// <summary>
        /// Exports the object categories
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportCategories(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;
            var dependencies = ti.ObjectDependencies;

            if (settings.IncludeCategories && (dependencies != null) && dependencies.Any())
            {
                // Append categories if main object is not category
                if ((currentLevel == 0) && !ti.IsCategory)
                {
                    // Add separator, because in array mode, StartCollection callback does not add separator
                    ProcessArraySeparatorMethod(settings);

                    CallStartCollection(settings, "categories", true);
                    ExportCategoryObjects(settings, info);
                    CallEndCollection(settings, "categories", true);
                }
            }
        }


        /// <summary>
        /// Exports the object automation processes
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportProcesses(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;

            if (settings.IncludeProcesses && (ti.HasProcesses) && (info.Processes != null))
            {
                info.Processes.AllowPaging = !settings.DisableCollectionPaging;
                info.Processes.LoadBinaryData = settings.EnsureBinaryData;

                // Append processes
                if (info.Processes.Count > 0)
                {
                    CallStartCollection(settings, "processes");
                    TraverseObjectCollection(settings, info.Processes, currentLevel, false, null);
                    CallEndCollection(settings, "processes");
                }
            }
        }


        /// <summary>
        /// Exports the object scheduled tasks
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportScheduledTasks(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;

            if (settings.IncludeScheduledTasks && (ti.HasScheduledTasks) && (info.ScheduledTasks != null))
            {
                info.ScheduledTasks.AllowPaging = !settings.DisableCollectionPaging;
                info.ScheduledTasks.LoadBinaryData = settings.EnsureBinaryData;

                // Append scheduled tasks
                if (info.ScheduledTasks.Count > 0)
                {
                    CallStartCollection(settings, "tasks");
                    TraverseObjectCollection(settings, info.ScheduledTasks, currentLevel, false, null);
                    CallEndCollection(settings, "tasks");
                }
            }
        }


        /// <summary>
        /// Exports the object metafiles
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportMetafiles(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;

            if (settings.IncludeMetafiles && (ti.HasMetaFiles) && (info.MetaFiles != null))
            {
                info.MetaFiles.AllowPaging = !settings.DisableCollectionPaging;
                info.MetaFiles.LoadBinaryData = settings.EnsureBinaryData;

                // Append metafiles
                if (info.MetaFiles.Count > 0)
                {
                    CallStartCollection(settings, "metafiles");
                    TraverseObjectCollection(settings, info.MetaFiles, currentLevel, false, null);
                    CallEndCollection(settings, "metafiles");
                }
            }
        }


        /// <summary>
        /// Exports the object other bindings
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportOtherBindings(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;

            if (settings.IncludeOtherBindings && (info.OtherBindings != null))
            {
                info.OtherBindings.LoadBinaryData = settings.EnsureBinaryData;

                // Append other binding objects
                if (info.OtherBindings.Count > 0)
                {
                    CallStartCollection(settings, "otherbindings");

                    bool appendSeparator = false;

                    foreach (var item in info.OtherBindings)
                    {
                        GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(item.ObjectType);

                        // Check whether the binding is site and whether it should be included or not
                        if (!settings.IncludeSiteBindings)
                        {
                            if ((infoObj != null) && infoObj.TypeInfo.IsSiteBinding)
                            {
                                continue;
                            }
                        }

                        if (appendSeparator)
                        {
                            ProcessArraySeparatorMethod(settings);
                        }

                        // Get duplicities
                        string key = string.Format("{0}_{1}", ti.OriginalObjectType, item.ObjectType).ToLowerCSafe();

                        var duplicities = settings.BindingDuplicities[key];
                        Func<BaseInfo, bool> objectFilter = null;

                        // Initialize object filter
                        if ((duplicities != null) && (duplicities.Count > 0))
                        {
                            var csi = item.Object.TypeInfo.ClassStructureInfo;
                            objectFilter = (i => !duplicities.Contains(GetCompositeID(csi.IDColumn, i)));
                        }

                        TraverseObjectCollection(settings, item, currentLevel, true, objectFilter);
                        appendSeparator = true;
                    }

                    CallEndCollection(settings, "otherbindings");
                }
            }
        }


        /// <summary>
        /// Exports the object bindings
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportBindings(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;

            if (settings.IncludeBindings && (info.Bindings != null))
            {
                info.Bindings.LoadBinaryData = settings.EnsureBinaryData;

                // Append binding objects
                if (info.Bindings.Count > 0)
                {
                    CallStartCollection(settings, "bindings");

                    bool appendSeparator = false;

                    foreach (var item in info.Bindings)
                    {
                        // Check whether the binding is site and whether it should be included or not
                        if (!settings.IncludeSiteBindings)
                        {
                            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(item.ObjectType);
                            if ((infoObj != null) && infoObj.TypeInfo.IsSiteBinding)
                            {
                                continue;
                            }
                        }

                        if (appendSeparator)
                        {
                            ProcessArraySeparatorMethod(settings);
                        }

                        // Get duplicities
                        string key = string.Format("{0}_{1}", ti.OriginalObjectType, item.ObjectType).ToLowerCSafe();
                        var duplicities = settings.BindingDuplicities[key];

                        // Initialize object filter
                        Func<BaseInfo, bool> objectFilter = null;
                        if ((duplicities != null) && (duplicities.Count > 0))
                        {
                            var csi = item.Object.TypeInfo.ClassStructureInfo;

                            objectFilter = (i => !duplicities.Contains(GetCompositeID(csi.IDColumn, i)));
                        }

                        TraverseObjectCollection(settings, item, currentLevel, true, objectFilter);

                        appendSeparator = true;
                    }

                    CallEndCollection(settings, "bindings");
                }
            }
        }


        /// <summary>
        /// Exports the object children
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportChildren(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            bool levelIsOk = (settings.MaxRelativeLevel > currentLevel) || (settings.MaxRelativeLevel < 0);
            if (settings.IncludeChildren && levelIsOk && (info.Children != null))
            {
                info.Children.LoadBinaryData = settings.EnsureBinaryData;

                // Append collection
                if (info.Children.Count > 0)
                {
                    CallStartCollection(settings, "children");
                    bool appendSeparator = false;

                    foreach (var item in info.Children)
                    {
                        if (appendSeparator)
                        {
                            ProcessArraySeparatorMethod(settings);
                        }

                        TraverseObjectCollection(settings, item, currentLevel, true, null);

                        appendSeparator = true;
                    }

                    CallEndCollection(settings, "children");
                }
            }
        }


        /// <summary>
        /// Exports the object child dependencies
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        /// <param name="currentLevel">Current level of the object tree structure</param>
        private void ExportChildDependencies(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            // Child dependency columns
            if ((info.ChildDependencies != null) && (info.ChildDependencies.Count > 0))
            {
                // Append collection
                CallStartCollection(settings, "childdependencies");
                bool appendSeparator = false;

                foreach (var item in info.ChildDependencies)
                {
                    if (appendSeparator)
                    {
                        ProcessArraySeparatorMethod(settings);
                    }

                    TraverseObjectCollection(settings, item, currentLevel, false, null);

                    appendSeparator = true;
                }

                CallEndCollection(settings, "childdependencies");
            }
        }


        /// <summary>
        /// Exports the object categories if the main object is a category
        /// </summary>
        /// <param name="settings">Traverse settings</param>
        /// <param name="info">Info object</param>
        /// <param name="currentLevel">Current level</param>
        private void ExportParentCategories(TraverseObjectSettings settings, GeneralizedInfo info, int currentLevel)
        {
            var ti = info.TypeInfo;

            // Append categories if main object is category
            if ((currentLevel == 0) && ti.IsCategory && settings.IncludeCategories && (ti.ObjectDependencies != null) && ti.ObjectDependencies.Any())
            {
                CallStartCollection(settings, ti.ObjectType, true);

                ExportCategoryObjects(settings, info);

                CallEndCollection(settings, ti.ObjectType, true);

                ProcessArraySeparatorMethod(settings);
            }
        }


        /// <summary>
        /// Returns true if the object is excluded
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        private static bool IsExcluded(TraverseObjectSettings settings, GeneralizedInfo info)
        {
            var excluded = false;
            if (settings.ExcludedNames != null)
            {
                foreach (string name in settings.ExcludedNames)
                {
                    if (info.ObjectCodeName.StartsWithCSafe(name, true) ||
                        info.ObjectDisplayName.StartsWithCSafe(name, true))
                    {
                        excluded = true;
                        break;
                    }
                }
            }

            return excluded;
        }


        /// <summary>
        /// Processes (logs) the object translations
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="info">GeneralizedInfo to serialize</param>
        private void ProcessTranslations(TraverseObjectSettings settings, GeneralizedInfo info)
        {
            var ti = info.TypeInfo;

            // Site translation
            if ((ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (info.ObjectSiteID > 0))
            {
                ProcessIDMethod(settings, info, ti.SiteIDColumn, PredefinedObjectType.SITE, true);
            }

            // Parent object translation
            if ((ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (info.ObjectParentID > 0))
            {
                ProcessIDMethod(settings, info, ti.ParentIDColumn, info.ParentObjectType, true);
            }

            // Dependency translation
            if (ti.ObjectDependencies != null)
            {
                foreach (var dep in ti.ObjectDependencies)
                {
                    string dependencyColumn = dep.DependencyColumn;
                    string dependencyType = info.GetDependencyObjectType(dep);

                    if (!string.IsNullOrEmpty(dependencyType))
                    {
                        ProcessIDMethod(settings, info, dependencyColumn, dependencyType, dep.IsRequired());
                    }
                }
            }
        }


        /// <summary>
        /// Calls given delegate with required parameters if delegate is not null.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="name">Name of the collection</param>
        protected void CallStartCollection(TraverseObjectSettings settings, string name)
        {
            CallStartCollection(settings, name, false);
        }


        /// <summary>
        /// Calls given delegate with required parameters if delegate is not null.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="name">Name of the collection</param>
        /// <param name="isArray">Determines whether it's the start of an array</param>
        protected void CallStartCollection(TraverseObjectSettings settings, string name, bool isArray)
        {
            ProcessStartCollectionMethod(settings, name, isArray);
        }


        /// <summary>
        /// Calls given delegate with required parameters if delegate is not null.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="name">Name of the collection</param>
        protected void CallEndCollection(TraverseObjectSettings settings, string name)
        {
            CallEndCollection(settings, name, false);
        }


        /// <summary>
        /// Calls given delegate with required parameters if delegate is not null.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="name">Name of the collection</param>
        /// <param name="isArray">Determines whether it's the start of an array</param>
        protected void CallEndCollection(TraverseObjectSettings settings, string name, bool isArray)
        {
            ProcessEndCollectionMethod(settings, name, isArray);
        }


        /// <summary>
        /// Calls item callback if exists.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="isEnd">Determines whether is is the end of the item or the beginning</param>
        protected void CallItemCallback(TraverseObjectSettings settings, bool isEnd)
        {
            ProcessItemMethod(settings, isEnd);
        }


        /// <summary>
        /// Calls object callback if exists.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="infoObj">Info object</param>
        /// <param name="level">Current level</param>
        protected void CallObjectCallback(TraverseObjectSettings settings, GeneralizedInfo infoObj, int level)
        {
            // Raise event to register custom translations
            if (settings.IncludeTranslations && ColumnsTranslationEvents.RegisterRecords.IsBound)
            {
                ColumnsTranslationEvents.RegisterRecords.StartEvent(settings.TranslationHelper, infoObj.TypeInfo.ObjectType, infoObj);
            }

            ProcessObjectMethod(settings, infoObj, level);
        }


        /// <summary>
        /// Exports categories data to given StringBuilder object.
        /// </summary>
        /// <param name="settings">Traverse object object</param>
        /// <param name="info">Info object the categories of which should be exported</param>
        protected bool ExportCategoryObjects(TraverseObjectSettings settings, GeneralizedInfo info)
        {
            bool hasCategories = false;

            var dependencies = info.TypeInfo.ObjectDependencies;
            if (dependencies != null)
            {
                foreach (var dep in dependencies)
                {
                    string dependencyColumn = dep.DependencyColumn;
                    string dependencyType = info.GetDependencyObjectType(dep);
                    if (!string.IsNullOrEmpty(dependencyType))
                    {
                        GeneralizedInfo dependencyObj = ModuleManager.GetObject(dependencyType);
                        if (dependencyObj != null)
                        {
                            int id = ValidationHelper.GetInteger(info.GetValue(dependencyColumn), 0);
                            if (id > 0)
                            {
                                // Add translations
                                ProcessIDMethod(settings, info, dependencyColumn, dependencyType, dep.IsRequired());

                                if (dependencyObj.TypeInfo.IsCategory)
                                {
                                    // Export current category
                                    dependencyObj = dependencyObj.GetObject(id);

                                    if (dependencyObj != null)
                                    {
                                        hasCategories = true;

                                        // Recursively add categories of current category
                                        bool addSeparator = ExportCategoryObjects(settings, dependencyObj);

                                        if (addSeparator)
                                        {
                                            ProcessArraySeparatorMethod(settings);
                                        }

                                        CallItemCallback(settings, false);
                                        CallObjectCallback(settings, dependencyObj, 1);
                                        CallItemCallback(settings, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return hasCategories;
        }


        /// <summary>
        /// Exports given child collection.
        /// </summary>
        /// <param name="settings">Traverse object settings</param>
        /// <param name="collection">InfoObject collection</param>
        /// <param name="currentLevel">Current level of the relationship (parent - child)</param>
        /// <param name="checkInclusion">If true, TypeInfo.IncludeToParentDataSet is called to check whether to include child to result</param>
        /// <param name="objectFilter">Function representing object filter</param>
        protected void TraverseObjectCollection(TraverseObjectSettings settings, IInfoObjectCollection collection, int currentLevel, bool checkInclusion, Func<BaseInfo, bool> objectFilter)
        {
            lock (collection)
            {
                // Save collection settings
                var originalAllowPaging = collection.AllowPaging;
                var originalCheckLicense = collection.CheckLicense;

                try
                {
                    // Disable paging and license check to traverse all data
                    collection.AllowPaging = !settings.DisableCollectionPaging;
                    collection.CheckLicense = false;

                    ProcessStartCollectionMethod(settings, collection.ObjectType, true);

                    bool callSeparator = false;

                    if (!checkInclusion || (collection.TypeInfo.IncludeToParentDataSet(settings.Operation) != IncludeToParentEnum.None))
                    {
                        foreach (BaseInfo item in collection)
                        {
                            GeneralizedInfo generalizedItem = item.Generalized;
                            // Filter object
                            if ((objectFilter != null) && !objectFilter(generalizedItem))
                            {
                                continue;
                            }

                            if (callSeparator)
                            {
                                ProcessArraySeparatorMethod(settings);
                            }

                            CallItemCallback(settings, false);
                            TraverseObjectStructure(settings, generalizedItem, currentLevel + 1);

                            CallItemCallback(settings, true);
                            callSeparator = true;

                            // Keep processed binding IDs
                            if (settings.IncludeOtherBindings && generalizedItem.TypeInfo.IsBinding)
                            {
                                var csi = generalizedItem.TypeInfo.ClassStructureInfo;

                                if (!String.IsNullOrEmpty(csi.IDColumn))
                                {
                                    var parentObj = ModuleManager.GetReadOnlyObject(generalizedItem.ParentObjectType);
                                    if (parentObj != null)
                                    {
                                        string key = string.Format("{0}_{1}", parentObj.TypeInfo.OriginalObjectType, collection.ObjectType).ToLowerCSafe();

                                        var duplicities = settings.BindingDuplicities[key] ?? new HashSet<string>();

                                        duplicities.Add(GetCompositeID(csi.IDColumn, generalizedItem));

                                        settings.BindingDuplicities[key] = duplicities;
                                    }
                                }
                            }
                        }
                    }

                    ProcessEndCollectionMethod(settings, collection.ObjectType, true);
                }
                finally
                {
                    // Restore collection settings
                    collection.AllowPaging = originalAllowPaging;
                    collection.CheckLicense = originalCheckLicense;
                }
            }
        }

        #endregion


        #region "Object export methods"

        /// <summary>
        /// Returns XML representation of given instance of info object.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="infoObj">Info object to export</param>
        /// <returns>XML representation of given instance of info object.</returns>
        public static string ExportObject(TraverseObjectSettings settings, BaseInfo infoObj)
        {
            var hierarchyHelper = new HierarchyHelper(settings, infoObj);

            return hierarchyHelper.ExportObject();
        }


        /// <summary>
        /// Returns XML representation of given instance of info object.
        /// </summary>
        public string ExportObject()
        {
            if (!DataAvailable())
            {
                return "";
            }

            // Process object data
            if (mSettings.IncludeObjectData)
            {
                ExportObjectData();

                // Handle translations
                ProcessTranslations(mSettings, mTranslations, mResultBuilder);
            }

            // Append object metadata if requested
            if (mSettings.IncludeMetadata)
            {
                if ((mResultBuilder.Length > 0) && (mSettings.Format == ExportFormatEnum.JSON))
                {
                    mResultBuilder.Append(", ");
                }

                object obj = GetMetadataSource();

                mResultBuilder.Append(GetMetadata(mSettings, obj));
            }

            // Wrap into root element if needed
            string result = WrapResult(mSettings, mResultBuilder.ToString());

            // If this is atompub we need to wrap the xml into the feed envelope
            if ((mSettings.Format == ExportFormatEnum.ATOM10) || (mSettings.Format == ExportFormatEnum.RSS20))
            {
                using (var memoryStream = new SystemIO.MemoryStream(Encoding.Default.GetBytes(result)))
                using (var xmlReader = new XmlTextReader(memoryStream))
                {
                    SyndicationContent content = SyndicationContent.CreateXmlContent(xmlReader);
                    SyndicationItem item = GetSyndicationItem(content);

                    StringBuilder sb = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings();

                    settings.OmitXmlDeclaration = true;
                    settings.Indent = true;
                    settings.CheckCharacters = false;

                    using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
                    {
                        if (mSettings.Format == ExportFormatEnum.ATOM10)
                        {
                            item.SaveAsAtom10(xmlWriter);
                        }
                        else
                        {
                            item.SaveAsRss20(xmlWriter);
                        }

                        xmlWriter.Flush();
                        result = sb.ToString();
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns true if the data can be exported
        /// </summary>
        protected virtual bool DataAvailable()
        {
            return (mInfo != null);
        }


        /// <summary>
        /// Exports the object data
        /// </summary>
        protected virtual void ExportObjectData()
        {
            if (mInfo != null)
            {
                // Go through the object structure
                TraverseObjectStructure(mSettings, mInfo, 0);
            }
        }


        /// <summary>
        /// Gets the metadata source object
        /// </summary>
        protected virtual object GetMetadataSource()
        {
            object obj = null;

            if (mInfo != null)
            {
                // Ensure clone of the object
                if (!mInfo.IsClone)
                {
                    obj = mInfo.Clone();
                }
                else
                {
                    obj = mInfo;
                }
            }

            return obj;
        }


        /// <summary>
        /// Gets the syndication item created from current object
        /// </summary>
        /// <param name="content">Content</param>
        protected virtual SyndicationItem GetSyndicationItem(SyndicationContent content)
        {
            SyndicationItem item = null;

            if (mInfo != null)
            {
                item = new SyndicationItem(mInfo.ObjectCodeName, content, mSettings.ExportItemURI, mInfo.ObjectID.ToString(), mInfo.ObjectLastModified);
                item.PublishDate = mInfo.ObjectLastModified;
            }

            return item;
        }


        /// <summary>
        /// Handles export to required format.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="obj">Object (TreeNode / Info Object) to process</param>
        /// <param name="currentLevel">Current level within the object tree structure</param>
        protected virtual void ProcessObjectMethod(TraverseObjectSettings settings, ICMSObject obj, int currentLevel)
        {
            // Info object
            GeneralizedInfo info = (GeneralizedInfo)obj;

            // Append main object
            switch (settings.Format)
            {
                case ExportFormatEnum.JSON:
                    {
                        string objResult = info.ToJSON(null, settings.Binary);
                        objResult = objResult.Substring(1, objResult.Length - 2);
                        mResultBuilder.Append(objResult);
                    }
                    break;

                default:
                    mResultBuilder.Append(info.ToXML(ObjectHelper.GetSerializationTableName(info), settings.Binary));
                    break;
            }
        }


        /// <summary>
        /// Handles FK ID (can be used to fill translation helper for example).
        /// </summary>
        /// <param name="settings">Export object settings (not needed in this implementation)</param>
        /// <param name="isEnd">Determines whether its start or end of the item</param>
        protected void ProcessItemMethod(TraverseObjectSettings settings, bool isEnd)
        {
            if (settings.Format == ExportFormatEnum.JSON)
            {
                mResultBuilder.Append(isEnd ? " }" : "{ ");
            }
        }


        /// <summary>
        /// Handles FK ID (can be used to fill translation helper for example).
        /// </summary>
        /// <param name="settings">Export object settings (not needed in this implementation)</param>
        /// <param name="obj">Object (TreeNode / InfoObject) to export</param>
        /// <param name="columnName">Column name of the dependency</param>
        /// <param name="objectType">Object type of the dependency</param>
        /// <param name="required">Determines whether the dependency is required (reflects required flag from TypeInfo).</param>
        protected void ProcessIDMethod(TraverseObjectSettings settings, ICMSObject obj, string columnName, string objectType, bool required)
        {
            if (settings.ProcessIDCallback != null)
            {
                settings.ProcessIDCallback(settings, obj, columnName, objectType, required);
            }
            else
            {
                var id = ValidationHelper.GetInteger(obj.GetValue(columnName), 0);
                AddTranslation(objectType, id, mTranslations);
            }
        }


        /// <summary>
        /// Handles start of the collection export.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="name">Name of the collection</param>
        /// <param name="isArray">Indicates whether it's array collection (important for export to JSON)</param>
        protected void ProcessStartCollectionMethod(TraverseObjectSettings settings, string name, bool isArray)
        {
            if (isArray)
            {
                if (settings.Format == ExportFormatEnum.JSON)
                {
                    mResultBuilder.Append("\"" + name + "\": [");
                }
            }
            else
            {
                AppendNode(settings, mResultBuilder, name, false);
            }
        }


        /// <summary>
        /// Handles array separator of the collection export.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        protected void ProcessArraySeparatorMethod(TraverseObjectSettings settings)
        {
            if (settings.Format == ExportFormatEnum.JSON)
            {
                mResultBuilder.Append(", ");
            }
        }


        /// <summary>
        /// Handles end of the collection export.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="name">Name of the collection</param>
        /// <param name="isArray">Indicates whether it's array collection (important for export to JSON)</param>
        protected void ProcessEndCollectionMethod(TraverseObjectSettings settings, string name, bool isArray)
        {
            if (isArray)
            {
                if (settings.Format == ExportFormatEnum.JSON)
                {
                    mResultBuilder.Append("]");
                }
            }
            else
            {
                AppendNode(settings, mResultBuilder, name, true);
            }
        }


        /// <summary>
        /// Fills translation helper with data from hashtable and appends translations to the result if needed.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="translations">Hashtable with translations</param>
        /// <param name="sb">StringBuilder to append result to</param>
        protected static void ProcessTranslations(TraverseObjectSettings settings, TranslationsTable translations, StringBuilder sb)
        {
            TranslationHelper th = settings.TranslationHelper;

            // Fill TranslationHelper with translations from hashtable
            if (settings.IncludeTranslations && (th == null))
            {
                th = new TranslationHelper();
            }

            if ((translations != null) && (th != null))
            {
                string[] excludedNames = null;
                if (settings.ExcludedNames != null)
                {
                    excludedNames = settings.ExcludedNames.ToArray();
                }

                foreach (string className in translations.Keys)
                {
                    IList<int> ids = translations[className];
                    string siteName = (className == "cms.site" ? null : TranslationHelper.AUTO_SITENAME);
                    th.RegisterRecords(className, ids, siteName, excludedNames);
                }
            }

            // Write translations if requested
            if (settings.IncludeTranslations && (th != null) && !DataHelper.DataSourceIsEmpty(th.TranslationTable))
            {
                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        {
                            sb.Append(", \"translations\": ");
                            sb.Append(th.TranslationTable.ToJSON(false));
                        }
                        break;

                    default:
                        {
                            AppendNode(settings, sb, "translations", false);

                            sb.Append(th.GetTranslationsXml(false));

                            AppendNode(settings, sb, "translations", true);
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Wraps final result into root if needed.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="code">Code to wrap</param>
        protected static string WrapResult(TraverseObjectSettings settings, string code)
        {
            if ((settings.Format == ExportFormatEnum.JSON))
            {
                code = "{ " + code + " }";
            }
            if (!string.IsNullOrEmpty(settings.RootName))
            {
                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        code = "{ \"" + settings.RootName + "\": " + code + " }";
                        break;

                    default:
                        code = "<" + settings.RootName + ">" + code + "</" + settings.RootName + ">";
                        break;
                }
            }
            return code;
        }


        /// <summary>
        /// Appends hierarchy node to the result.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="sb">StringBuilder object with the result</param>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="endTag">Determines whether to append start or end tag</param>
        protected static void AppendNode(TraverseObjectSettings settings, StringBuilder sb, string nodeName, bool endTag)
        {
            if (settings.CreateHierarchy)
            {
                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        if (endTag)
                        {
                            sb.Append(" }");
                        }
                        else
                        {
                            sb.Append(", \"" + nodeName + "\": { ");
                        }
                        break;

                    default:
                        sb.Append("<" + (endTag ? "/" : "") + nodeName + ">");
                        break;
                }
            }
            else
            {
                if (!endTag)
                {
                    if (settings.Format == ExportFormatEnum.JSON)
                    {
                        sb.Append(", ");
                    }
                }
            }
        }


        /// <summary>
        /// Adds given ID into the translation table to correct list according to objectType
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="translationTable">Translation table</param>
        protected static void AddTranslation(string objectType, int id, TranslationsTable translationTable)
        {
            string key = objectType.ToLowerCSafe();
            List<int> ids = translationTable[key] ?? new List<int>();
            if (!ids.Contains(id))
            {
                ids.Add(id);
            }
            translationTable[key] = ids;
        }


        /// <summary>
        /// Returns metadata for given object according to export settings.
        /// </summary>
        /// <param name="settings">Export settings format</param>
        /// <param name="obj">Object to get the metadata of</param>
        protected static string GetMetadata(TraverseObjectSettings settings, object obj)
        {
            if ((obj != null) && (settings != null))
            {
                bool isProperties;
                string colList = GetListOfColumns(obj, out isProperties);
                switch (settings.Format)
                {
                    case ExportFormatEnum.JSON:
                        {
                            Hashtable metadata = new Hashtable();
                            metadata["type"] = obj.GetType().ToString();
                            metadata["columns"] = (isProperties ? "" : colList);
                            metadata["properties"] = (isProperties ? colList : "");

                            string retval = CreateJavaScriptSerializer().Serialize(metadata);
                            retval = "\"metadata\": " + retval;

                            return retval;
                        }

                    default:
                        {
                            XmlWriterSettings xmlSettings = new XmlWriterSettings();
                            xmlSettings.OmitXmlDeclaration = true;
                            xmlSettings.Indent = true;
                            xmlSettings.CheckCharacters = false;

                            StringBuilder sb = new StringBuilder();
                            XmlWriter xml = XmlWriter.Create(sb, xmlSettings);

                            xml.WriteStartElement("metadata");
                            xml.WriteElementString("type", obj.GetType().ToString());
                            xml.WriteElementString("columns", (isProperties ? "" : colList));
                            xml.WriteElementString("properties", (isProperties ? colList : ""));
                            xml.WriteEndElement();

                            xml.Flush();
                            xml.Close();

                            return sb.ToString();
                        }
                }
            }
            return "";
        }


        /// <summary>
        /// Creates a new instance of the <see cref="JavaScriptSerializer"/> class.
        /// </summary>
        private static JavaScriptSerializer CreateJavaScriptSerializer()
        {
            var serializer = new JavaScriptSerializer();
            var length = GetMaxJsonLength();
            if (length > 0)
            {
                serializer.MaxJsonLength = length;
            }

            return serializer;
        }


        /// <summary>
        /// Gets maximum JSON length from the application configuration file.
        /// </summary>
        private static int GetMaxJsonLength()
        {
            return CoreServices.AppSettings["CMSRestMaxJsonLength"].ToInteger(0);
        }


        /// <summary>
        /// Returns list of supported columns/properties
        /// </summary>
        /// <param name="obj">Object to get the metadata of</param>
        /// <param name="isProperties">For properties this param will be true, otherwise (columns) false</param>
        protected static string GetListOfColumns(object obj, out bool isProperties)
        {
            StringBuilder sb = new StringBuilder();
            isProperties = false;
            if (obj is DataRow)
            {
                DataRow dr = (DataRow)obj;
                foreach (DataColumn col in dr.Table.Columns)
                {
                    sb.Append(";" + col.ColumnName);
                }
            }
            else if (obj is DataRowView)
            {
                DataRowView dr = (DataRowView)obj;
                foreach (DataColumn col in dr.DataView.Table.Columns)
                {
                    sb.Append(";" + col.ColumnName);
                }
            }
            else if (obj is IContext)
            {
                IContext context = (IContext)obj;
                foreach (string col in context.Properties)
                {
                    sb.Append(";" + col);
                }
                isProperties = true;
            }
            else if (obj is IHierarchicalObject)
            {
                IHierarchicalObject hc = (IHierarchicalObject)obj;
                foreach (string col in hc.Properties)
                {
                    sb.Append(";" + col);
                }
                isProperties = true;
            }
            else if (obj is IDataContainer)
            {
                IDataContainer dc = (IDataContainer)obj;
                foreach (string col in dc.ColumnNames)
                {
                    sb.Append(";" + col);
                }
            }

            // Return the list of columns/properties
            string result = sb.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Substring(1);
            }
            return result;
        }

        #endregion


        #region "Load object data methods"

        /// <summary>
        /// Loads objects data including collection from a given XML representation (root of the xml has to be ClassTableName of given object).
        /// </summary>
        /// <param name="operation">Operation type</param>
        /// <param name="obj">Object to load</param>
        /// <param name="xml">XML data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="updateOnlySpecifiedColumns">If true, only column contained in the <paramref name="xml"/> are loaded (columns which are not contained are not set to null)</param>
        /// <param name="cultureName">Name of the culture to use for parsing double and datetime values</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        /// <returns>Translation helper</returns>
        public static TranslationHelper LoadObjectFromXML(OperationTypeEnum operation, BaseInfo obj, string xml, bool disconnectObject = true, bool updateOnlySpecifiedColumns = false, string cultureName = null, List<string> excludedColumns = null)
        {
            // Create DataSet from given XML
            DataSet ds = ObjectHelper.GetObjectsDataSet(operation, obj, true);

            if (!updateOnlySpecifiedColumns)
            {
                // Add empty translation table to DataSet
                DataTable translationTable = TranslationHelper.GetEmptyTable();
                ds.Tables.Add(translationTable);
            }

            // Load XML to DataSet
            XmlParserContext xmlContext = new XmlParserContext(null, null, null, XmlSpace.None);
            XmlReaderSettings rs = new XmlReaderSettings();
            rs.ConformanceLevel = ConformanceLevel.Auto;
            rs.CheckCharacters = false;
            XmlReader xmlReader = XmlReader.Create(new XmlTextReader(xml, XmlNodeType.Element, xmlContext), rs);

            List<string> updatedColumns;
            DataHelper.ReadDataSetFromXml(ds, xmlReader, null, null, null, out updatedColumns, cultureName);

            // Create translation helper
            TranslationHelper th = new TranslationHelper(ds.Tables[TranslationHelper.TRANSLATION_TABLE]);

            // Load data from DataSet
            LoadObjectFromDataSet(obj, ds, disconnectObject, (updateOnlySpecifiedColumns ? updatedColumns : null), excludedColumns);
            return th;
        }


        /// <summary>
        /// Loads objects data including collection from a given XML representation.
        /// </summary>
        /// <param name="obj">Object to load</param>
        /// <param name="json">JSON data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="updateOnlySpecifiedColumns">If true, only column contained in the <paramref name="json"/> are loaded (columns which are not contained are not set to null)</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        /// <returns>Translation helper</returns>
        public static TranslationHelper LoadObjectFromJSON(BaseInfo obj, string json, bool disconnectObject = true, bool updateOnlySpecifiedColumns = false, List<string> excludedColumns = null)
        {
            DataSet ds = ObjectHelper.GetObjectsDataSet(OperationTypeEnum.Export, obj, true);

            // Add empty translation table to DataSet
            DataTable translationTable = TranslationHelper.GetEmptyTable();
            ds.Tables.Add(translationTable);

            JavaScriptSerializer serializer = CreateJavaScriptSerializer();
            IDictionary values = (IDictionary)serializer.DeserializeObject(json);

            List<string> columnsToUpdate = LoadDataSetFromJSON(ds, ds.Tables[0].TableName, new object[] { values });
            if (!updateOnlySpecifiedColumns)
            {
                columnsToUpdate = null;
            }

            // Create translation helper
            TranslationHelper th = new TranslationHelper(translationTable);

            // Load data from DataSet
            LoadObjectFromDataSet(obj, ds, disconnectObject, columnsToUpdate, excludedColumns);
            return th;
        }


        /// <summary>
        /// Recursively loads dataset from JSON deserialized object. Returns list of fields contained in the JSON data (for update object purposes).
        /// </summary>
        /// <param name="ds">DataSet to fill the data in</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="values">Values from deserialization</param>
        protected static List<string> LoadDataSetFromJSON(DataSet ds, string tableName, IEnumerable<object> values)
        {
            DataTable dt = ds.Tables[tableName.Replace(".", "_")];
            if (dt != null)
            {
                List<string> result = new List<string>();
                foreach (IDictionary obj in values)
                {
                    DataRow dr = dt.NewRow();
                    foreach (DictionaryEntry val in obj)
                    {
                        var columnName = val.Key.ToString();
                        var isBinary = (dt.Columns.Contains(columnName) && (dt.Columns[columnName].DataType == typeof(byte[])));

                        // Table property, but ignore binary fields (must be updated normally)
                        if ((val.Value is object[]) && !isBinary)
                        {
                            result.AddRange(LoadDataSetFromJSON(ds, val.Key.ToString(), (object[])val.Value));
                        }
                        else
                        {
                            if (val.Value != null)
                            {
                                if (isBinary)
                                {
                                    // Process binary data - convert from object to byte
                                    dr[columnName] = ((object[])val.Value)
                                        .Select(Convert.ToByte).ToArray();
                                }
                                else
                                {
                                    dr[columnName] = val.Value;
                                }
                            }
                            result.Add(columnName.ToLowerCSafe());
                        }
                    }
                    dt.Rows.Add(dr);
                }
                return result;
            }
            return new List<string>();
        }


        /// <summary>
        /// Loads object data including collection from a given data set.
        /// </summary>
        /// <param name="obj">Object to load</param>
        /// <param name="ds">Dataset with data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="columnsToUpdate">List of columns which will be updated</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        public static void LoadObjectFromDataSet(GeneralizedInfo obj, DataSet ds, bool disconnectObject, List<string> columnsToUpdate = null, List<string> excludedColumns = null)
        {
            LoadObjectFromDataSet(obj, ds, disconnectObject, 0, columnsToUpdate, excludedColumns);
        }


        /// <summary>
        /// Loads object data including collection from a given data set.
        /// </summary>
        /// <param name="obj">Object to load</param>
        /// <param name="ds">Dataset with data</param>
        /// <param name="disconnectObject">If true, object collections are disconnected (collections won't load data automatically from the DB, just from dataset)</param>
        /// <param name="id">ID of the object to identify it within the DataSet</param>
        /// <param name="columnsToUpdate">List of columns which will be updated</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        protected static void LoadObjectFromDataSet(GeneralizedInfo obj, DataSet ds, bool disconnectObject, int id, List<string> columnsToUpdate, List<string> excludedColumns)
        {
            if ((obj == null) || DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            // Process root object
            var rootTable = ObjectHelper.GetTable(ds, obj);
            if (rootTable == null)
            {
                return;
            }

            // Make all the collections disconnected (force them empty)
            if (disconnectObject)
            {
                obj.Disconnect();
            }

            var ti = obj.TypeInfo;

            if ((id == 0) && (rootTable.Rows.Count > 0))
            {
                foreach (DataRow dr in rootTable.Rows)
                {
                    // Load the object
                    LoadObjectFromDataRow(obj, dr, columnsToUpdate, excludedColumns);

                    // Check that loaded data matches object type, otherwise try to load next row
                    if (obj.TypeInfo.ObjectType.EqualsCSafe(ti.ObjectType))
                    {
                        break;
                    }
                }

                // Get the ID, if supplied
                id = obj.ObjectID;
            }
            // Decide if root object should be inserted or updated
            bool insert = (id == 0);

            // Object type info may have changed depending on new field values
            ti = obj.TypeInfo;

            // Process children
            foreach (string childObjType in ti.ChildObjectTypes)
            {
                GeneralizedInfo childObj = ModuleManager.GetReadOnlyObject(childObjType);
                if (childObj != null)
                {
                    DataTable childTable = ObjectHelper.GetTable(ds, childObj);

                    if ((childTable != null) && (childTable.Rows.Count > 0))
                    {
                        // Get only collection with correct ID
                        var childTypeInfo = childObj.TypeInfo;

                        // Add required columns
                        if (!childTable.Columns.Contains(childTypeInfo.ParentIDColumn))
                        {
                            childTable.Columns.Add(childTypeInfo.ParentIDColumn);
                        }
                        if (!childTable.Columns.Contains(childTypeInfo.IDColumn))
                        {
                            childTable.Columns.Add(childTypeInfo.IDColumn);
                        }

                        string childWhere = "";
                        // If data are stored in the same table, the parent child relation is defined by filled parent id column
                        if (childTable == rootTable)
                        {
                            childWhere = childTypeInfo.ParentIDColumn + " IS NOT NULL";
                        }

                        var drs = childTable.Select(childWhere, childTypeInfo.DefaultOrderBy);
                        if (drs.Length > 0)
                        {
                            List<BaseInfo> objects = new List<BaseInfo>();

                            foreach (DataRow dr in drs)
                            {
                                // Get new instance of given object
                                childObj = ModuleManager.GetObject(childObjType);

                                // Load the data of the child
                                LoadObjectFromDataRow(childObj, dr, columnsToUpdate, excludedColumns);

                                // Check that loaded data corresponds to loaded object type
                                if (childObj.TypeInfo.ObjectType.EqualsCSafe(childObjType))
                                {
                                    // If new parent is created, child object is created (not updated)
                                    if (insert)
                                    {
                                        childObj.ObjectID = 0;
                                    }

                                    // Don't go into the recursion if ID is not supplied and the object type is same as parent object type
                                    if ((childObj.ObjectID > 0) || (childObjType.EqualsCSafe(ti.ObjectType)))
                                    {
                                        // Init child object and put it to child object type
                                        LoadObjectFromDataSet(childObj, ds, disconnectObject, childObj.ObjectID, columnsToUpdate, excludedColumns);
                                    }

                                    // Add to children collection
                                    objects.Add(childObj);

                                    // Remove from DataSet (it's because not all records have ID, so we have to make sure everything without ID is processed only once)
                                    childTable.Rows.Remove(dr);
                                }
                            }

                            obj.Children[childObjType].Load(objects);
                        }
                    }
                }
            }

            // Process child dependencies
            var dependencies = ti.ObjectDependencies;
            if (dependencies != null)
            {
                var depColumns = ti.ChildDependencyColumns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string depColumn in depColumns)
                {
                    foreach (var dep in dependencies)
                    {
                        string dependencyColumn = dep.DependencyColumn;
                        if (dependencyColumn.ToLowerCSafe() == depColumn.ToLowerCSafe())
                        {
                            string dependencyType = obj.GetDependencyObjectType(dep);
                            if (!string.IsNullOrEmpty(dependencyType))
                            {
                                GeneralizedInfo dependencyObj = ModuleManager.GetReadOnlyObject(dependencyType);
                                if (dependencyObj != null)
                                {
                                    var depTable = ObjectHelper.GetTable(ds, dependencyObj);

                                    if ((depTable != null) && (depTable.Rows.Count > 0))
                                    {
                                        var depTypeInfo = dependencyObj.TypeInfo;

                                        if (!depTable.Columns.Contains(depTypeInfo.IDColumn))
                                        {
                                            depTable.Columns.Add(depTypeInfo.IDColumn);
                                        }

                                        // Get only collection with correct ID
                                        DataRow[] drs = depTable.Select(depTypeInfo.IDColumn + " = " + obj.GetValue(dependencyColumn));
                                        if (drs.Length > 0)
                                        {
                                            List<BaseInfo> objects = new List<BaseInfo>();

                                            foreach (DataRow dr in drs)
                                            {
                                                // Get new instance of given object
                                                dependencyObj = ModuleManager.GetObject(dependencyType);

                                                // Load the data of the child
                                                LoadObjectFromDataRow(dependencyObj, dr, columnsToUpdate, excludedColumns);

                                                // Add to collection collection
                                                objects.Add(dependencyObj);
                                            }

                                            obj.ChildDependencies[dependencyType].Load(objects);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Process bindings
            if (ti.BindingObjectTypes != null)
            {
                foreach (string bindingObjType in ti.BindingObjectTypes)
                {
                    GeneralizedInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingObjType);
                    if (bindingObj != null)
                    {
                        var childTable = ObjectHelper.GetTable(ds, bindingObj);

                        if ((childTable != null) && (childTable.Rows.Count > 0))
                        {
                            var bindingTypeInfo = bindingObj.TypeInfo;

                            if (!childTable.Columns.Contains(bindingTypeInfo.ParentIDColumn))
                            {
                                childTable.Columns.Add(bindingTypeInfo.ParentIDColumn);
                            }

                            // Get only collection with correct ID
                            var drs = childTable.Select(bindingTypeInfo.ParentIDColumn + " = " + id);
                            if (drs.Length > 0)
                            {
                                var objects = new List<BaseInfo>();

                                foreach (DataRow dr in drs)
                                {
                                    // Get new instance of given object
                                    bindingObj = ModuleManager.GetObject(bindingObjType);

                                    // Load the data of the child
                                    LoadObjectFromDataRow(bindingObj, dr, columnsToUpdate, excludedColumns);

                                    // Add to collection collection
                                    objects.Add(bindingObj);
                                }

                                obj.Bindings[bindingObjType].Load(objects);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns object ID even if it is composed out of multiple columns.
        /// </summary>
        /// <param name="idColumns">ID columns separated with semicolon</param>
        /// <param name="infoObj">Object to get ID from</param>
        protected static string GetCompositeID(string idColumns, BaseInfo infoObj)
        {
            if (!String.IsNullOrEmpty(idColumns))
            {
                string[] columns = idColumns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder id = new StringBuilder();
                foreach (string idColumn in columns)
                {
                    id.Append(infoObj.GetValue(idColumn), ",");
                }
                return id.ToString().Trim(',');
            }
            return null;
        }


        /// <summary>
        /// Loads an GeneralizedInfo with values from given DataRow.
        /// </summary>
        /// <param name="container">Container to load</param>
        /// <param name="dr">DataRow with data</param>
        /// <param name="columnsToUpdate">List of columns to update</param>
        /// <param name="excludedColumns">Columns which will be ignored during the data load even if they are in the provided data</param>
        public static void LoadObjectFromDataRow(IDataContainer container, DataRow dr, List<string> columnsToUpdate, List<string> excludedColumns)
        {
            if (container != null)
            {
                bool checkColumns = (columnsToUpdate == null);
                bool hasExcluded = (excludedColumns == null);
                foreach (DataColumn col in dr.Table.Columns)
                {
                    if (container.ContainsColumn(col.ColumnName) && (hasExcluded || !excludedColumns.Contains(col.ColumnName)) && (checkColumns || columnsToUpdate.Contains(col.ColumnName.ToLowerCSafe())))
                    {
                        var isNullValue = ValidationHelper.GetString(dr[col.ColumnName], "").EqualsCSafe("##null##", true);

                        container.SetValue(col.ColumnName, isNullValue ? null : dr[col.ColumnName]);
                    }
                }
            }
        }

        #endregion
    }
}