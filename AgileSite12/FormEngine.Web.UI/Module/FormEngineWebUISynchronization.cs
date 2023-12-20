using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Web farm synchronization for Form controls
    /// </summary>
    internal class FormEngineWebUISynchronization
    {
        /// <summary>
        /// Initializes the tasks for form resolvers synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<ClearFormResolversWebFarmTask>(true);
        }
    }
}
