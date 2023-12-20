using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to execute dictionary web farm task.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DictionaryCommandWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets task text data.
        /// </summary>
        public string TaskTextData { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AbstractProviderDictionary.ProcessWebFarmTask(string, string, byte[])"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            AbstractProviderDictionary.ProcessWebFarmTask(TaskTarget, TaskTextData, null);
        }
    }
}
