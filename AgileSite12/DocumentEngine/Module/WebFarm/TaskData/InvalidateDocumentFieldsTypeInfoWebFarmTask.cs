using CMS.Core;
using CMS.DocumentEngine.Internal;
using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm task used to invalidate document fields.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class InvalidateDocumentFieldsTypeInfoWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the class file which data fields will be invalidated.
        /// </summary>
        public string ClassName { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="DocumentFieldsInfoProvider.InvalidateTypeInfo(string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            DocumentFieldsInfoProvider.InvalidateTypeInfo(ClassName ?? String.Empty, false);
        }
    }
}
