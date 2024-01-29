using System;
using System.Linq;

using CMS.Core;
using CMS.Helpers;

namespace CMS.CustomTables
{
    /// <summary>
    /// Web farm synchronization for custom table items
    /// </summary>
    internal class CustomTableSynchronization
    {
        #region "Methods"

        /// <summary>
        /// Initializes the tasks for custom table items synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CustomTableTaskType.ClearCustomTableTypeInfos,
                Execute = ClearCustomTableTypeInfos,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = CustomTableTaskType.InvalidateCustomTableTypeInfo,
                Execute = InvalidateCustomTableTypeInfo,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Clears the custom table items
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearCustomTableTypeInfos(string target, string[] data, BinaryData binaryData)
        {
            CustomTableItemProvider.Clear(false);
        }


        /// <summary>
        /// Invalidates the custom table type info
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateCustomTableTypeInfo(string target, string[] data, BinaryData binaryData)
        {
            CustomTableItemProvider.InvalidateTypeInfo(data.FirstOrDefault() ?? "", false);
        }

        #endregion
    }
}
