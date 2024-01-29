using System;
using System.Collections.Generic;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Writes the given object and its hierarchy to the output as XML
    /// </summary>
    internal class ObjectXmlWriter
    {
        private TranslationXmlWriter mTranslationWriter;
        private HashSet<string> bindingDuplicities;

        /// <summary>
        /// Synchronization settings
        /// </summary>
        private SynchronizationObjectSettings Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Writer for the output
        /// </summary>
        private IO.StreamWriter Writer
        {
            get;
            set;
        }


        /// <summary>
        /// Translation XML writer instance
        /// </summary>
        private TranslationXmlWriter TranslationWriter
        {
            get
            {
                return mTranslationWriter ?? (mTranslationWriter = new TranslationXmlWriter(Writer, Settings.TranslationHelper ?? (Settings.TranslationHelper = new TranslationHelper())));
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">Output writer</param>
        /// <param name="settings">Writer settings</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/></exception>
        public ObjectXmlWriter(IO.StreamWriter writer, SynchronizationObjectSettings settings)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            Writer = writer;
            Settings = settings;
        }


        /// <summary>
        /// Writes the object XML
        /// </summary>
        /// <param name="infoObj">Info object</param>
        public void WriteObjectXml(GeneralizedInfo infoObj)
        {
            Writer.WriteLine("<" + Settings.RootName + ">");

            // Process object data
            if (Settings.IncludeObjectData)
            {
                WriteObjectHierarchy(infoObj);
                if (Settings.IncludeTranslations)
                {
                    TranslationWriter.WriteTranslations();
                }
            }

            Writer.WriteLine("</" + Settings.RootName + ">");
        }


        /// <summary>
        /// Writes the object data to the output
        /// </summary>
        /// <param name="infoObj">Info object</param>
        private void WriteObjectHierarchy(GeneralizedInfo infoObj)
        {
            if (infoObj == null)
            {
                throw new ArgumentNullException("infoObj");
            }

            // Ensure open connection for the whole traverse process if the object is connected to DB
            using (var connection = new CMSConnectionScope())
            {
                if (!infoObj.IsDisconnected)
                {
                    connection.Open();
                }

                WriteSingleObject(infoObj);

                // Create initial query from the object and write all objects in its hierarchy
                var typeInfo = infoObj.TypeInfo;
                var parentQuery = new ObjectQuery(typeInfo.ObjectType).WhereEquals(typeInfo.IDColumn, infoObj.ObjectID);

                WriteNestedHierarchy(parentQuery);
            }
        }


        /// <summary>
        /// Writes a single object to the output
        /// </summary>
        /// <param name="infoObj">Info object</param>
        private ObjectTypeInfo WriteSingleObject(GeneralizedInfo infoObj)
        {
            // If the object is category, parent categories must be written first to ensure correct order (parent category first)
            // This could have been written as bulk, but categories themselves aren't too deep and this is handled only for main object, so it should not be a big deal
            var typeInfo = infoObj.TypeInfo;
            if (typeInfo.IsCategory && Settings.IncludeCategories)
            {
                WriteParentCategory(infoObj);
            }

            WriteObjectData(infoObj);

            // If the object is not category, write categories after the object, the order is maintained by the WriteParentCategory method
            if (!typeInfo.IsCategory && Settings.IncludeCategories)
            {
                WriteParentCategory(infoObj);
            }

            return typeInfo;
        }


        /// <summary>
        /// Exports the parent category of the given object
        /// </summary>
        /// <param name="infoObj">Info object</param>
        private void WriteParentCategory(GeneralizedInfo infoObj)
        {
            var typeInfo = infoObj.TypeInfo;

            // Get parent category
            var parentCategoryId = ValidationHelper.GetInteger(infoObj.GetValue(typeInfo.CategoryIDColumn), 0);
            if (parentCategoryId > 0)
            {
                var parentCategory = typeInfo.CategoryObject.GetObject(parentCategoryId);

                // Handle it as first-level object, except for further nested processing
                WriteSingleObject(parentCategory);
            }
        }


        /// <summary>
        /// Traverses the nested hierarchy for the given parent query, traverses children, bindings, etc. based on settings
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteNestedHierarchy(IObjectQuery parentQuery, int currentLevel = 0)
        {
            if (!CheckLevel(currentLevel))
            {
                return;
            }

            WriteChildDependencies(parentQuery, currentLevel);
            WriteChildren(parentQuery, currentLevel);
            WriteBindings(parentQuery, currentLevel);
            WriteOtherBindings(parentQuery, currentLevel);
            WriteMetafiles(parentQuery, currentLevel);
            WriteScheduledTasks(parentQuery, currentLevel);
            WriteProcesses(parentQuery, currentLevel);
        }


        /// <summary>
        /// Writes related processes to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteProcesses(IObjectQuery parentQuery, int currentLevel)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

            if (Settings.IncludeProcesses && typeInfo.HasProcesses)
            {
                var processes = parentQuery.GetProcesses().BinaryData(Settings.EnsureBinaryData);

                WriteObjects(processes, currentLevel);
            }
        }


        /// <summary>
        /// Writes related scheduled tasks to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteScheduledTasks(IObjectQuery parentQuery, int currentLevel)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

            if (Settings.IncludeScheduledTasks && typeInfo.HasScheduledTasks)
            {
                var tasks = parentQuery.GetScheduledTasks().BinaryData(Settings.EnsureBinaryData);

                WriteObjects(tasks, currentLevel);
            }
        }


        /// <summary>
        /// Writes meta files to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteMetafiles(IObjectQuery parentQuery, int currentLevel)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

            if (Settings.IncludeMetafiles && typeInfo.HasMetaFiles)
            {
                var metafiles = parentQuery.GetMetaFiles().BinaryData(Settings.EnsureBinaryData);

                WriteObjects(metafiles, currentLevel);
            }
        }


        /// <summary>
        /// Writes other bindings to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteOtherBindings(IObjectQuery parentQuery, int currentLevel)
        {
            if (Settings.IncludeOtherBindings)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

                foreach (var bindingObjectType in typeInfo.OtherBindingObjectTypes)
                {
                    // Bindings must be allowed to be included in parent DataSet for the given operation
                    var bindingTypeInfo = ObjectTypeManager.GetTypeInfo(bindingObjectType);
                    if (bindingTypeInfo.IncludeToParentDataSet(Settings.Operation) == IncludeToParentEnum.None)
                    {
                        continue;
                    }

                    // Skip site bindings if required
                    if (!Settings.IncludeSiteBindings && bindingTypeInfo.IsSiteBinding)
                    {
                        continue;
                    }

                    var bindings = parentQuery.GetOtherBindings(bindingObjectType).BinaryData(Settings.EnsureBinaryData);

                    WriteObjects(bindings, currentLevel);
                }
            }
        }


        /// <summary>
        /// Writes bindings to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteBindings(IObjectQuery parentQuery, int currentLevel)
        {
            if (Settings.IncludeBindings)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

                foreach (var bindingObjectType in typeInfo.BindingObjectTypes)
                {
                    // Bindings must be allowed to be included in parent DataSet for the given operation
                    var bindingTypeInfo = ObjectTypeManager.GetTypeInfo(bindingObjectType);
                    if (bindingTypeInfo.IncludeToParentDataSet(Settings.Operation) == IncludeToParentEnum.None)
                    {
                        continue;
                    }

                    // Skip site bindings if required
                    if (!Settings.IncludeSiteBindings && bindingTypeInfo.IsSiteBinding)
                    {
                        continue;
                    }

                    var bindings = parentQuery.GetBindings(bindingObjectType).BinaryData(Settings.EnsureBinaryData);

                    WriteObjects(bindings, currentLevel);
                }
            }
        }


        /// <summary>
        /// Writes children to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteChildren(IObjectQuery parentQuery, int currentLevel)
        {
            if (Settings.IncludeChildren)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

                foreach (var childObjectType in typeInfo.ChildObjectTypes)
                {
                    // Children must be allowed to be included in parent DataSet for the given operation
                    var childTypeInfo = ObjectTypeManager.GetTypeInfo(childObjectType);
                    if (childTypeInfo.IncludeToParentDataSet(Settings.Operation) == IncludeToParentEnum.None)
                    {
                        continue;
                    }

                    var children = parentQuery.GetChildren(childObjectType).OrderBy(childTypeInfo.DefaultOrderBy).BinaryData(Settings.EnsureBinaryData);

                    WriteObjects(children, currentLevel);
                }
            }
        }


        /// <summary>
        /// Writes child dependencies to the output
        /// </summary>
        /// <param name="parentQuery">Parent query</param>
        /// <param name="currentLevel">Current level</param>
        private void WriteChildDependencies(IObjectQuery parentQuery, int currentLevel)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(parentQuery.ObjectType);

            if (!String.IsNullOrEmpty(typeInfo.ChildDependencyColumns))
            {
                var dependencyColumns = typeInfo.ChildDependencyColumns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string col in dependencyColumns)
                {
                    var dependencyObjectType = typeInfo.GetObjectTypeForColumn(col);
                    if (dependencyObjectType != null)
                    {
                        var dependencies = parentQuery.GetRelatedObjects(dependencyObjectType, col).BinaryData(Settings.EnsureBinaryData);

                        WriteObjects(dependencies, currentLevel);
                    }
                }
            }
        }


        /// <summary>
        /// Checks if the given level is allowed to be written
        /// </summary>
        /// <param name="currentLevel">Level to check</param>
        private bool CheckLevel(int currentLevel)
        {
            return (Settings.MaxRelativeLevel > currentLevel) || (Settings.MaxRelativeLevel < 0);
        }


        /// <summary>
        /// Writes objects from the given query to the output
        /// </summary>
        /// <param name="objects">List of objects to write</param>
        /// <param name="currentLevel">Current level of processing</param>
        private void WriteObjects(ObjectQuery objects, int currentLevel)
        {
            // Write current objects
            foreach (var obj in objects)
            {
                WriteObjectData(obj);
            }

            WriteNestedHierarchy(objects, currentLevel + 1);
        }


        /// <summary>
        /// Writes the given object to the output
        /// </summary>
        /// <param name="infoObj">Object</param>
        private void WriteObjectData(BaseInfo infoObj)
        {
            if (Settings.IncludeOtherBindings && infoObj.TypeInfo.IsBinding)
            {
                var csi = infoObj.TypeInfo.ClassStructureInfo;
                string key = string.Format("{0}_{1}", infoObj.TypeInfo.ObjectType, GetCompositeID(csi.IDColumn, infoObj));

                if (bindingDuplicities == null)
                {
                    bindingDuplicities = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (bindingDuplicities.Contains(key))
                {
                    return;
                }

                bindingDuplicities.Add(key);
            }

            var objXml = infoObj.ToXML(ObjectHelper.GetSerializationTableName(infoObj), Settings.Binary);

            Writer.WriteLine(objXml);

            // Maintain the list of found foreign keys for the translation helper load at the end
            if (Settings.IncludeTranslations)
            {
                TranslationWriter.RegisterObjectTranslations(infoObj);
            }
        }


        /// <summary>
        /// Returns object ID even if it is composed out of multiple columns.
        /// </summary>
        /// <param name="idColumns">ID columns separated with semicolon</param>
        /// <param name="infoObj">Object to get ID from</param>
        private static string GetCompositeID(string idColumns, BaseInfo infoObj)
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
    }
}
