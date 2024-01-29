using CMS.Core;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Web farm synchronization for Form controls
    /// </summary>
    internal class FormEngineWebUISynchronization
    {
        #region "Methods"

        /// <summary>
        /// Initializes the tasks for form resolvers synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = FormEngineWebUITaskType.ClearFormResolvers,
                Execute = ClearFormResolvers,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Clears the BizForm items
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearFormResolvers(string target, string[] data, BinaryData binaryData)
        {
            FormEngineWebUIResolvers.ClearResolvers(false);
        }
 
        #endregion
    }
}
