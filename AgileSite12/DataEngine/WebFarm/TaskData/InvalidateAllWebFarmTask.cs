using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to invalidate all objects.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateAllWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ObjectTypeInfo.InvalidateAllObjects(bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Get the type info
            var ti = ObjectTypeManager.GetTypeInfo(TaskTarget);

            // Invalidate the object
            ti?.InvalidateAllObjects(false);
        }
    }
}
