using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    internal static class ContentStagingTaskCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="TaskTypeEnum.CreateObject"/> or <see cref="TaskTypeEnum.UpdateObject"/> task 
        /// for the changed <paramref name="baseInfo"/> into the collection.
        /// </summary>
        /// <param name="stagingTaskCollection">Staging task collection.</param>
        /// <param name="baseInfo">Info object.</param>
        public static void AddTaskForChangeObject(this ContentStagingTaskCollection stagingTaskCollection, BaseInfo baseInfo)
        {
            TaskTypeEnum taskType = (baseInfo.Generalized.ObjectID > 0)
                ? TaskTypeEnum.UpdateObject
                : TaskTypeEnum.CreateObject;

            stagingTaskCollection.Add(baseInfo, taskType);
        }


        /// <summary>
        /// Adds <see cref="TaskTypeEnum.DeleteObject"/> task for the <paramref name="baseInfo"/> into the collection.
        /// </summary>
        /// <param name="stagingTaskCollection">Staging task collection.</param>
        /// <param name="baseInfo">Info object.</param>
        public static void AddTaskForDeleteObject(this ContentStagingTaskCollection stagingTaskCollection, BaseInfo baseInfo)
        {
            stagingTaskCollection.Add(baseInfo, TaskTypeEnum.DeleteObject);
        }
    }
}
