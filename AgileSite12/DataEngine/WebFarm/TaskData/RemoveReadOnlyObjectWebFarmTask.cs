using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to remove read only objects.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemoveReadOnlyObjectWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets object type to remove.
        /// </summary>
        public string ObjectType { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="ModuleManager.RemoveReadOnlyObject(string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            ModuleManager.RemoveReadOnlyObject(ObjectType, false);
        }
    }
}
