using CMS.Core;
using System;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm task used to invalidate BizForm type info.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateBizFormTypeInfoWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the class to be invalidated.
        /// </summary>
        public string ClassName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="BizFormItemProvider.InvalidateTypeInfo(string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            BizFormItemProvider.InvalidateTypeInfo(ClassName ?? String.Empty, false);
        }
    }
}
