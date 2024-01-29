using System;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm synchronization for objects
    /// </summary>
    internal class DataTasks
    {
        #region "Variables"

        private static bool? mSynchronizeMetaFiles;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets value that indicates whether file synchronization is enabled.
        /// </summary>
        public static bool SynchronizeMetaFiles
        {
            get
            {
                if (mSynchronizeMetaFiles == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeMetaFiles", "CMSWebFarmSynchronizeMetaFiles", true));
                }

                return mSynchronizeMetaFiles.Value;
            }
            set
            {
                mSynchronizeMetaFiles = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.UpdateMetaFile,
                Execute = UpdateMetaFile,
                Condition = CheckSynchronizeMetaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.DeleteMetaFile,
                Execute = DeleteMetaFile,
                Condition = CheckSynchronizeDeleteMetaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.DictionaryCommand,
                Execute = DictionaryCommand,
                IsMemoryTask = true,
                OptimizationType = WebFarmTaskOptimizeActionEnum.GroupData
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.InvalidateObject,
                Execute = InvalidateObject,
                IsMemoryTask = true,
                OptimizationType = WebFarmTaskOptimizeActionEnum.GroupData
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.InvalidateChildren,
                Execute = InvalidateChildren,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.InvalidateAllObjects,
                Execute = InvalidateAllObjects,
                IsMemoryTask = true
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.ProcessObject,
                Execute = ProcessObject,
                IsMemoryTask = true
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.RemoveReadOnlyObject,
                Execute = RemoveReadOnlyObject,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.ClearReadOnlyObjects,
                Execute = ClearReadOnlyObjects,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.RemoveClassStructureInfo,
                Execute = RemoveClassStructureInfo,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.ClearClassStructureInfos,
                Execute = ClearClassStructureInfos,
                IsMemoryTask = true
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.ClearHashtables,
                Execute = ClearHashtables,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the metafiles is allowed
        /// </summary>
        private static bool CheckSynchronizeMetaFiles(IWebFarmTask task)
        {
            return SynchronizeMetaFiles;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the meta files is allowed
        /// </summary>
        private static bool CheckSynchronizeDeleteMetaFiles(IWebFarmTask task)
        {
            return SynchronizeMetaFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Invalidates object of specified object type and ID
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateObject(string target, string[] data, BinaryData binaryData)
        {
            // Get the type info
            ObjectTypeInfo ti = ObjectTypeManager.GetTypeInfo(target);
            if (ti != null)
            {
                // Invalidate the object
                int objectId = ValidationHelper.GetInteger(data.FirstOrDefault(), 0);
                ti.ObjectInvalidated(objectId, false);
            }
        }


        /// <summary>
        /// Invalidates direct children objects for parent of specified object type and ID
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateChildren(string target, string[] data, BinaryData binaryData)
        {
            // Get the type info
            ObjectTypeInfo ti = ObjectTypeManager.GetTypeInfo(target);
            if (ti != null)
            {
                // Invalidate the children
                int parentId = ValidationHelper.GetInteger(data.FirstOrDefault(), 0);
                ti.ChildrenInvalidated(parentId, false);
            }
        }


        /// <summary>
        /// Invalidates all objects of specified object type
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateAllObjects(string target, string[] data, BinaryData binaryData)
        {
            // Get the type info
            var ti = ObjectTypeManager.GetTypeInfo(target);
            if (ti != null)
            {
                // Invalidate the object
                ti.InvalidateAllObjects(false);
            }
        }


        /// <summary>
        /// Removes object from dictionary collection of specified type
        /// </summary>
        /// <param name="target">Task target(should be empty)</param>
        /// <param name="data">Contains identification of object type, collection type and particular id to remove</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DictionaryCommand(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException($"Expected 3 data arguments, but received {data.Length}");
            }

            var providerIdentification = data[1];
            var itemIdentifier = data[2];

            AbstractProviderDictionary.ProcessWebFarmTask(providerIdentification, itemIdentifier, null);
        }


        /// <summary>
        /// Updates the MetaFile
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateMetaFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 5)
            {
                throw new ArgumentException($"Expected 5 data arguments, but received {data.Length}");
            }

            // Get the parameters
            string siteName = data[0];
            string guid = data[1];
            string fileName = data[2];
            string extension = data[3];
            bool deleteOldFiles = Convert.ToBoolean(data[4]);

            // Save the binary data to the disk
            if (binaryData.Data != null)
            {
                MetaFileInfoProvider.SaveFileToDisk(siteName, guid, fileName, extension, binaryData.Data, deleteOldFiles, true);
            }
            else
            {
                // Drop the cache dependencies
                MetaFileInfoProvider.UpdatePhysicalFileLastWriteTime(siteName, fileName, extension);
                CacheHelper.TouchKey("metafile|" + guid.ToLowerCSafe(), false, false);
            }
        }


        /// <summary>
        /// Deletes the MetaFile
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteMetaFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException($"Expected 3 data arguments, but received {data.Length}");
            }

            // Get the parameters
            string siteName = data[0];
            string fileName = data[1];
            bool deleteDirectory = ValidationHelper.GetBoolean(data[2], false);

            // Delete attachment file
            MetaFileInfoProvider.DeleteFile(siteName, fileName, deleteDirectory, true);
        }


        /// <summary>
        /// Processes provider web farm task
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ProcessObject(string target, string[] data, BinaryData binaryData)
        {
            // Get separator indexes
            int firstIndex = target.IndexOf('|');
            int lastIndex = target.LastIndexOfCSafe('|');

            // Get object type and action name from target (ObjectClassName|ActionName|binaryFlag)
            string className = target.Substring(0, firstIndex);

            // Action name
            string actionName = target.Substring(firstIndex + 1, lastIndex - firstIndex - 1);

            // Try get provider
            var ap = InfoProviderLoader.GetInfoProvider(className);
            if (ap != null)
            {
                ap.ProcessWebFarmTask(actionName, data.FirstOrDefault() ?? "", binaryData.Data);
            }
        }


        /// <summary>
        /// Removes read only object
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemoveReadOnlyObject(string target, string[] data, BinaryData binaryData)
        {
            ModuleManager.RemoveReadOnlyObject(data.FirstOrDefault() ?? "", false);
        }


        /// <summary>
        /// Clears read only objects
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearReadOnlyObjects(string target, string[] data, BinaryData binaryData)
        {
            ModuleManager.ClearReadOnlyObjects(false);
        }


        /// <summary>
        /// Removes class structure info
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemoveClassStructureInfo(string target, string[] data, BinaryData binaryData)
        {
            ClassStructureInfo.Remove(data.FirstOrDefault() ?? "", false);
        }


        /// <summary>
        /// Clears class structure infos
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearClassStructureInfos(string target, string[] data, BinaryData binaryData)
        {
            ClassStructureInfo.Clear(false);
        }


        /// <summary>
        /// Clears the system hashtables
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearHashtables(string target, string[] data, BinaryData binaryData)
        {
            ModuleManager.ClearHashtables(false);
        }

        #endregion
    }
}
