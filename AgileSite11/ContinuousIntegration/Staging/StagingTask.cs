using System.Diagnostics;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Staging task to be performed for given info object.
    /// </summary>
    [DebuggerDisplay("{Object.TypeInfo.ObjectType} - {Object.Generalized.ObjectCodeName}")]
    public class StagingTask
    {
        /// <summary>
        /// Creates instance of staging task DTO.
        /// </summary>
        /// <param name="baseInfo">Base info.</param>
        /// <param name="taskType">Type of the staging </param>
        public StagingTask(BaseInfo baseInfo, TaskTypeEnum taskType)
        {
            Object = baseInfo;
            TaskType = taskType;
        }

        /// <summary>
        /// Type of the staging task to be performed.
        /// </summary>
        public TaskTypeEnum TaskType { get; private set; }


        /// <summary>
        /// Object data.
        /// </summary>
        public BaseInfo Object { get; private set; }
    }
}
