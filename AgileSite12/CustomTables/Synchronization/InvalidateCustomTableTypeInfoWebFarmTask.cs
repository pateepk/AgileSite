using System;

using CMS.Core;

namespace CMS.CustomTables
{
    /// <summary>
    /// Web farm task used to invalidate custom table type info.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateCustomTableTypeInfoWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets class name of the custom table to be invalidated.
        /// </summary>
        public string ClassName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CustomTableItemProvider.InvalidateTypeInfo"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            CustomTableItemProvider.InvalidateTypeInfo(ClassName ?? String.Empty, false);
        }
    }
}
