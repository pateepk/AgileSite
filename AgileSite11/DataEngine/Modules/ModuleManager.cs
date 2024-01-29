using System;
using System.Data;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Manages the list of modules within application
    /// </summary>
    public class ModuleManager : ModuleEntryManager
    {
        #region "Static variables"

        // Table of read only objects (for purposes of data direction).
        private static readonly ConcurrentDictionary<string, BaseInfo> mReadOnlyObjects = new ConcurrentDictionary<string, BaseInfo>(StringComparer.OrdinalIgnoreCase);

        #endregion


        #region "Get object methods"

        /// <summary>
        /// Removes object structures.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        public static void RemoveReadOnlyObject(string objectType, bool logTask)
        {
            BaseInfo baseInfo;
            mReadOnlyObjects.TryRemove(objectType, out baseInfo);

            if (logTask)
            {
                WebFarmHelper.CreateTask(DataTaskType.RemoveReadOnlyObject, null, objectType);
            }
        }


        /// <summary>
        /// Clears object structures.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged</param>
        public static void ClearReadOnlyObjects(bool logTask)
        {
            mReadOnlyObjects.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(DataTaskType.ClearReadOnlyObjects);
            }
        }


        /// <summary>
        /// Gets the cached object representation of the given class name.
        /// </summary>
        /// <param name="className">Class name</param>
        public static BaseInfo GetReadOnlyObjectByClassName(string className)
        {
            var dci = DataClassInfoProvider.GetDataClassInfo(className);
            if (dci != null)
            {
                var objectType = string.IsNullOrEmpty(dci.ClassDefaultObjectType) ? dci.ClassName : dci.ClassDefaultObjectType;
                return GetReadOnlyObject(objectType);
            }

            return null;
        }


        /// <summary>
        /// Gets the cached object instance of given type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static BaseInfo GetReadOnlyObject(string objectType)
        {
            return GetReadOnlyObject(objectType, false);
        }


        /// <summary>
        /// Gets the cached object instance of given type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="exceptionIfNotFound">If true, an exception is fired if the given object type is not found</param>
        public static BaseInfo GetReadOnlyObject(string objectType, bool exceptionIfNotFound)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                return null;
            }

            var result = mReadOnlyObjects.GetOrAdd(objectType, (objectTypeKey) =>
            {
                // Get the new empty object and cache
                var emptyObject = GetEmptyObject(objectTypeKey);

                if (emptyObject == null)
                {
                    // Cache empty info to prevent further attempts to load
                    return InfoHelper.EmptyInfo;
                }

                // Lock the object for further modifications
                emptyObject.SetReadOnly();
                EnsureTypeInfo(emptyObject);

                return emptyObject;
            });

            // Treat empty info cached value as null
            if (result is NotImplementedInfo)
            {
                result = null;
            }

            // Throw exception if necessary
            if (exceptionIfNotFound && (result == null))
            {
                throw new InvalidOperationException($"Object type '{objectType}' not found.");
            }

            return result;
        }


        /// <summary>
        /// Materializes type info to avoid lazy loading of it's properties after the object is published.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectTypeInfo EnsureTypeInfo(BaseInfo baseInfo)
        {
            return baseInfo.TypeInfo;
        }


        /// <summary>
        /// Gets a new object of the given type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="throwIfNotFound">If true, the method throws an exception in case the object type was not found</param>
        public static BaseInfo GetObject(string objectType, bool throwIfNotFound = false)
        {
            return GetObject(null, objectType, throwIfNotFound);
        }


        /// <summary>
        /// Gets a new object of the given type created from the given DataRow.
        /// </summary>
        /// <param name="objectRow">Object DataRow</param>
        /// <param name="objectType">Object type</param>
        /// <param name="throwIfNotFound">If true, the method throws an exception in case the object type was not found</param>
        public static BaseInfo GetObject(DataRow objectRow, string objectType, bool throwIfNotFound = false)
        {
            return GetObject(new LoadDataSettings(objectRow, objectType)
            {
                ThrowIfNotFound = throwIfNotFound
            });
        }


        /// <summary>
        /// Gets a new object of the given type created using the given settings.
        /// </summary>
        /// <param name="settings">Object settings</param>
        public static BaseInfo GetObject(LoadDataSettings settings)
        {
            var infoObj = GetReadOnlyObject(settings.ObjectType, settings.ThrowIfNotFound);
            if (infoObj == null)
            {
                return null;
            }

            // Create new object using the empty instance
            return infoObj.Generalized.NewObject(settings);
        }


        private static BaseInfo GetEmptyObject(string objectType)
        {
            objectType = objectType.ToLowerCSafe();

            // Create new object
            var result = ObjectTypeManager.GetEmptyObject(objectType);
            if (result == null)
            {
                // Go through all modules
                foreach (ModuleInfo module in Modules)
                {
                    var entry = module.Module as Module;
                    if (entry != null)
                    {
                        // Get the object
                        result = entry.GetObject(objectType);
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion


        #region "Other module methods"

        /// <summary>
        /// Processes the specified command.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action</param>
        [Obsolete("Use module specific method if available or change dependency for module you can't access directly.")]
        public static bool CheckModuleLicense(string moduleName, string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            Module entry = GetModule(moduleName);
            if (entry != null)
            {
                return entry.CheckLicense(domain, feature, action);
            }

            return true;
        }


        /// <summary>
        /// Gets the module entry for specified module.
        /// </summary>
        /// <param name="name">Module name</param>
        public static Module GetModule(string name)
        {
            // Get the module entry
            ModuleInfo info = GetModuleInfo(name);
            if (info != null)
            {
                return info.Module as Module;
            }

            return null;
        }


        /// <summary>
        /// Clears all the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public static void ClearHashtables(bool logTasks = true)
        {
            ClearModules();
            ProviderHelper.ClearAllHashtables(false);
            ClearReadOnlyObjects(false);

            // Log web farm task
            if (logTasks)
            {
                WebFarmHelper.CreateTask(DataTaskType.ClearHashtables);
            }
        }


        private static void ClearModules()
        {
            // Go through all the modules
            foreach (ModuleInfo module in Modules)
            {
                // Clear module hashtables
                var entry = module.Module as Module;
                if (entry != null)
                {
                    entry.Clear(false);
                }
            }
        }


        /// <summary>
        /// Gets the value of context property.
        /// </summary>
        /// <param name="name">Context name</param>
        /// <param name="propertyName">Property name</param>
        public static object GetContextProperty(string name, string propertyName)
        {
            var prop = typeof(IContext).StaticProperty<IContext>(name);
            if (prop != null)
            {
                var context = prop.Value;
                if (context != null)
                {
                    return context.GetProperty(propertyName);
                }
            }

            return null;
        }

        #endregion
    }
}
