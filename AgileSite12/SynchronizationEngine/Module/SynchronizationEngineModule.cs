using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Synchronization;

[assembly: RegisterModule(typeof(SynchronizationEngineModule))]

namespace CMS.Synchronization
{
    /// <summary>
    /// Represents the Synchronization Engine module.
    /// </summary>
    public class SynchronizationEngineModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SynchronizationEngineModule()
            : base(new SynchronizationEngineModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes synchronization module
        /// </summary>
        protected override void OnInit()
        {
            IntegrationHelper.Init();
            base.OnInit();
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("ProcessTask", ProcessTask);
        }


        /// <summary>
        /// Process synchronization task optionally with children
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object ProcessTask(object[] parameters)
        {
            // Prepare staging task data
            var stagingTaskData = new StagingTaskData
            {
                TaskType = (TaskTypeEnum)parameters[1],
                TaskObjectType = ValidationHelper.GetString(parameters[2], null),
                TaskData = ValidationHelper.GetString(parameters[3], null),
                TaskBinaryData = ValidationHelper.GetString(parameters[4], null),
                TaskGroups = TaskGroupInfoProvider.GetTaskGroups().WhereIn("TaskGroupCodeName", (parameters[8] as IEnumerable<string>)?.ToList()).TypedResult.ToArray()
            };
            stagingTaskData.SetCurrentUser(UserInfoProvider.GetUserInfo(ValidationHelper.GetString(parameters[7], null)));

            // Call ProcessTask method
            var man = SyncManager.GetInstance();
            man.OperationType = (OperationTypeEnum)parameters[0];
            man.SiteName = ValidationHelper.GetString(parameters[6], null);

            return man.ProcessTask(stagingTaskData, ValidationHelper.GetBoolean(parameters[5], true), null);
        }
    }
}