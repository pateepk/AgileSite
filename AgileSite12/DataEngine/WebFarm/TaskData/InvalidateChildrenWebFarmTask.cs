using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to invalidate children objects.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateChildrenWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the object which children will be invalidated.
        /// </summary>
        public int ParentId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ObjectTypeInfo.ChildrenInvalidated(int, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Get the type info
            ObjectTypeInfo ti = ObjectTypeManager.GetTypeInfo(TaskTarget);

            // Invalidate the children
            ti?.ChildrenInvalidated(ParentId, false);
        }
    }
}
