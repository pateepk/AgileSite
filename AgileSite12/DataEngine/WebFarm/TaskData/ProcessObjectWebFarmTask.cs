using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to process an object's data of given object type <see cref="ObjectType"/>.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ProcessObjectWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the object type.
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets object's data to synchronize.
        /// </summary>
        public string ObjectData { get; set; }


        /// <summary>
        /// Gets or sets name of the action to be executed upon object of given <see cref="ObjectType"/> with given <see cref="ObjectData"/>.
        /// </summary>
        public string ActionName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="IWebFarmProvider.ProcessWebFarmTask(string, string, byte[])"/> method on provider of given <see cref="ObjectType"/> while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            var ap = InfoProviderLoader.GetInfoProvider(ObjectType);

            ap?.ProcessWebFarmTask(ActionName, ObjectData, TaskBinaryData.Data);
        }
    }
}
