using CMS.Core;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to clear document type infos.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearDocumentTypeInfosWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="TreeNodeProvider.Clear"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            TreeNodeProvider.Clear(false);
        }
    }
}
