using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

using CMS.IO;
using CMS.Base;
using CMS.EventLog;
using CMS.DataEngine;

using SystemIO = System.IO;

namespace CMS.Synchronization
{
    /// <summary>
    /// Object that implements IStagingTaskData and is used for sending staging task related data from source server to target.
    /// </summary>
    [Serializable]
    public class StagingTaskData : IStagingTaskData
    {
        #region "Variables"

        private string mSystemVersion;
        private TaskGroupInfo[] mTaskGroups;

        #endregion


        #region "Properties"

        /// <summary>
        /// Type of task, for example update, delete, add to site etc...
        /// </summary>
        public TaskTypeEnum TaskType
        {
            get;
            set;
        }


        /// <summary>
        /// Object type of object for which the given staging task was created.
        /// </summary>
        public string TaskObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Data of object, which will be recreated on target server.
        /// </summary>
        public string TaskData
        {
            get;
            set;
        }


        /// <summary>
        /// Binary data of object which will be recreated on target server.
        /// </summary>
        public string TaskBinaryData
        {
            get;
            set;
        }


        /// <summary>
        /// List of tasks servers separated by ';', eg. ';server1;server2;server3;'.
        /// </summary>
        public string TaskServers
        {
            get;
            set;
        }


        /// <summary>
        /// From which Kentico version is this object sent.
        /// Version should be the same on source and target.
        /// If not set, current version is returned.
        /// </summary>
        public string SystemVersion
        {
            get
            {
                if (String.IsNullOrEmpty(mSystemVersion))
                {
                    mSystemVersion = CMSVersion.MainVersion;
                }

                return mSystemVersion;
            }
            set
            {
                mSystemVersion = value;
            }
        }


        /// <summary>
        /// Guid of user who has synchronized the task.
        /// </summary>
        public Guid UserGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Name of user who has synchronized the task.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }


        /// <summary>
        /// Work labels under which object encapsulated within staging task got modified.
        /// </summary>
        public TaskGroupInfo[] TaskGroups
        {
            get
            {
                return mTaskGroups ?? (mTaskGroups = new TaskGroupInfo[0]);
            }
            set
            {
                mTaskGroups = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Parameterless constructor, for creating StagingTaskData.
        /// No properties are initialized except SystemVersion.
        /// </summary>
        public StagingTaskData() { }


        /// <summary>
        /// Creates StagingTaskData from serialized StagingTaskData.
        /// Should be used on target server.
        /// All properties are initialized from serialized StagingTaskData.
        /// </summary>
        /// <param name="serializedStagingTaskData">Serialized StagingTaskData from which this object will be initialized</param>
        public StagingTaskData(string serializedStagingTaskData)
        {
            Deserialize(serializedStagingTaskData);
        }


        /// <summary>
        /// Creates StagingTaskData from serialized StagingTaskInfo.
        /// Should be used before sending data from source to target.
        /// All properties are initialized, UserName and UserGuid are equal to CurrentUser.
        /// </summary>
        /// <param name="sti">StagingTaskInfo from which this object will be initialized</param>
        public StagingTaskData(StagingTaskInfo sti)
        {
            TaskType = sti.TaskType;
            TaskData = sti.TaskData;
            TaskObjectType = sti.TaskObjectType;
            TaskServers = sti.TaskServers;
            UserName = CMSActionContext.CurrentUser.UserName;
            UserGuid = CMSActionContext.CurrentUser.UserGUID;
            TaskBinaryData = SynchronizationHelper.GetObjectBinaryXml(OperationTypeEnum.Synchronization, sti.TaskObjectType, sti.TaskObjectID, sti.TaskType);

            // Get work labels for given staging task
            TaskGroups = TaskGroupInfoProvider.GetTaskGroups()
                .WhereIn("TaskGroupID", TaskGroupTaskInfoProvider.GetTaskGroupTasks().WhereEquals("TaskID", sti.TaskID).Column("TaskGroupID"))
                .ToArray();
        }

        #endregion


        #region "IStagingTaskData methods"

        /// <summary>
        /// Serializes current StagingTaskData to string, for sending via staging.
        /// </summary>
        /// <returns>Serializesd current StagingTaskData</returns>
        public string Serialize()
        {
            string serializedObj;

            using (var ms = new SystemIO.MemoryStream())
            {
                SoapFormatter sp = new SoapFormatter();
                sp.Serialize(ms, this);
                ms.Position = 0;
                using (StreamReader sr = StreamReader.New(ms))
                {
                    serializedObj = sr.ReadToEnd();
                }
            }

            return serializedObj;
        }

        #endregion


        #region "Helper Methods"

        /// <summary>
        /// Deserializes string, to fill properties of this StagingTaskData.
        /// </summary>
        /// <param name="serializedStagingTaskData">Serialized StagingTaskData</param>
        private void Deserialize(string serializedStagingTaskData)
        {
            IStagingTaskData std;

            using (var ms = new SystemIO.MemoryStream())
            {
                using (StreamWriter sw = StreamWriter.New(ms))
                {
                    sw.Write(serializedStagingTaskData);
                    sw.Flush();
                    ms.Position = 0;

                    SoapFormatter sp = new SoapFormatter
                    {
                        Binder = StagingTaskDataBinder.Instance,
                    };

                    object deserialized = null;
                    try
                    {
                        deserialized = sp.Deserialize(ms);
                    }
                    catch (InvalidOperationException e)
                    {
                        EventLogProvider.LogException("Staging", "STAGINGTASKDATADESERIALIZATION", e);
                        throw;
                    }

                    std = (IStagingTaskData)deserialized;
                }
            }

            TaskType = std.TaskType;
            TaskServers = std.TaskServers;
            TaskObjectType = std.TaskObjectType;
            TaskBinaryData = std.TaskBinaryData;
            TaskData = std.TaskData;
            UserName = std.UserName;
            UserGuid = std.UserGuid;
            SystemVersion = std.SystemVersion;
            TaskGroups = std.TaskGroups;
        }


        /// <summary>
        /// Sets the current user's guid and username to <see cref="UserGuid"/> and <see cref="UserName"/>
        /// of current StagingTaskData instance.
        /// </summary>
        /// <param name="user">User to be set, if null <see cref="CMSActionContext.CurrentUser"/> will be used</param>
        public void SetCurrentUser(IUserInfo user = null)
        {
            user = user ?? CMSActionContext.CurrentUser;

            UserName = user.UserName;
            UserGuid = user.UserGUID;
        }

        #endregion
    }


    /// <summary>
    /// Provides a type to which deserialization is to be bound.
    /// </summary>
    internal class StagingTaskDataBinder : SerializationBinder
    {
        private readonly Type stagingTaskDataType = typeof(StagingTaskData);
        private readonly Type taskGroupInfoType = typeof(TaskGroupInfo);
        private readonly Type synchronizationTypeEnumType = typeof(SynchronizationTypeEnum);
        private readonly Type iDataClassType = typeof(IDataClass);
        private readonly Type objectType = typeof(object);
        private readonly Type guidType = typeof(Guid);

        private readonly string synchronizationAssemblyName;
        private readonly string dataEngineAssemblyName;
        private readonly string systemAssemblyName;

        private static readonly Lazy<StagingTaskDataBinder> mInstance = new Lazy<StagingTaskDataBinder>(() => new StagingTaskDataBinder());


        /// <summary>
        /// Gets the current instance of the <see cref="StagingTaskDataBinder"/> class.
        /// </summary>
        public static StagingTaskDataBinder Instance => mInstance.Value;


        /// <summary>
        /// Initializes a new instance of <see cref="StagingTaskDataBinder"/>.
        /// </summary>
        private StagingTaskDataBinder()
        {
            synchronizationAssemblyName = stagingTaskDataType.Assembly.FullName;
            dataEngineAssemblyName = synchronizationTypeEnumType.Assembly.FullName;
            systemAssemblyName = objectType.Assembly.FullName;
        }


        /// <summary>
        /// Returns a type to be bound to for deserialization of staging data.
        /// </summary>
		/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object. </param>
		/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object. </param>
        /// <exception cref="InvalidOperationException">Thrown when, <paramref name="assemblyName"/> and <paramref name="typeName"/> are not supported by the staging.</exception>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (systemAssemblyName.Equals(assemblyName, StringComparison.Ordinal))
            {
                if (typeName.Equals(objectType.FullName, StringComparison.Ordinal))
                {
                    return objectType;
                }

                if (typeName.Equals(guidType.FullName, StringComparison.Ordinal))
                {
                    return guidType;
                }
            }

            if (dataEngineAssemblyName.Equals(assemblyName, StringComparison.Ordinal))
            {
                if (synchronizationTypeEnumType.FullName.Equals(typeName, StringComparison.Ordinal))
                {
                    return synchronizationTypeEnumType;
                }

                if (iDataClassType.IsAssignableFrom(synchronizationTypeEnumType.Assembly.GetType(typeName)))
                {
                    // When object is assignable from IDataClass (SimpleDataClass) then return null thus deserializer will use the type from the input params
                    return null;
                }
            }

            if (synchronizationAssemblyName.Equals(assemblyName, StringComparison.Ordinal))
            {
                if (stagingTaskDataType.FullName.Equals(typeName, StringComparison.Ordinal))
                {
                    return stagingTaskDataType;
                }

                if (taskGroupInfoType.FullName.Equals(typeName, StringComparison.Ordinal))
                {
                    return taskGroupInfoType;
                }
            }

            throw new InvalidOperationException($"Type '{typeName}' from assembly '{assemblyName}' cannot be bound to.");
        }
    }
}
