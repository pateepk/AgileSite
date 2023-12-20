using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to invalidate objects.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateObjectWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the object which will be invalidated.
        /// </summary>
        public int ObjectId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ObjectTypeInfo.ObjectInvalidated(int, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Get the type info
            ObjectTypeInfo ti = ObjectTypeManager.GetTypeInfo(TaskTarget);

            // Invalidate the object
            ti?.ObjectInvalidated(ObjectId, false);

        }
    }
}
