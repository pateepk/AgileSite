using CMS.Core;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to clear document field type infos.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearDocumentFieldsTypeInfosWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="DocumentFieldsInfoProvider.Clear"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            DocumentFieldsInfoProvider.Clear(false);
        }
    }
}
