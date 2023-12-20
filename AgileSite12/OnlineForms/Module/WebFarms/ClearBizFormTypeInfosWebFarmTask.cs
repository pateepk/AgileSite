using CMS.Core;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm task used to clear the bizForm item type info.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearBizFormTypeInfosWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="BizFormItemProvider.Clear(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            BizFormItemProvider.Clear(false);
        }
    }
}
