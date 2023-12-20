using Newtonsoft.Json;

namespace CMS.Core
{
    /// <summary>
    /// Represents a type of a web farm task which has a condition to be met in order to log such type of task and an associated action to be executed upon task processing.
    /// </summary>
    public abstract class WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets task target.
        /// </summary>
        [JsonIgnore]
        public string TaskTarget { get; set; }


        /// <summary>
        /// Gets or sets target path of file transported by task.
        /// </summary>
        [JsonIgnore]
        public string TaskFilePath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the binary representation of data.
        /// </summary>
        [JsonIgnore]
        public BinaryData TaskBinaryData { get; set; }


        /// <summary>
        /// Action which is invoked when the web farm task executes.
        /// </summary>
        public abstract void ExecuteTask();


        /// <summary>
        /// Condition which must be met in order to log the task.
        /// </summary>
        public virtual bool ConditionMethod()
        {
            return true;
        }
    }
}
