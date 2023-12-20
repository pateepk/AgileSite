using CMS.Core;
using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to invalidate document type infos.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateDocumentTypeInfoWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the class file to be invalidated.
        /// </summary>
        public string ClassName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="TreeNodeProvider.InvalidateTypeInfo(string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            TreeNodeProvider.InvalidateTypeInfo(ClassName ?? String.Empty, false);
        }
    }
}
