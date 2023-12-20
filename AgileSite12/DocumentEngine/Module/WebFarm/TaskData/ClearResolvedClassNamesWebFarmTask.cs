using CMS.Core;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to clear resolved class names.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearResolvedClassNamesWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="DocumentTypeHelper.ClearClassNames"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            DocumentTypeHelper.ClearClassNames(false);
        }
    }
}
