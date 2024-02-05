using System;

namespace CMS.Core
{
    using TaskAction = Action<string, string[], BinaryData>;

    /// <summary>
    /// Web farm task implementation
    /// </summary>
    public class WebFarmTask
    {
        /// <summary>
        /// Task type.
        /// </summary>
        public string Type
        { 
            get;
            set;
        }


        /// <summary>
        /// Condition which must be met in order to log the task.
        /// </summary>
        public Func<IWebFarmTask, bool> Condition
        { 
            get;
            set;
        }


        /// <summary>
        /// Action which is fired when the web farm task executes.
        /// </summary>
        public TaskAction Execute
        { 
            get;
            set;
        }


        /// <summary>
        /// Indicates if the task modifies memory only. Memory tasks
        /// can be deleted on application start.
        /// </summary>
        public bool IsMemoryTask
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if task number can be optimized by one of predefined ways.
        /// </summary>
        public WebFarmTaskOptimizeActionEnum OptimizationType
        {
            get;
            set;
        }
    }
}