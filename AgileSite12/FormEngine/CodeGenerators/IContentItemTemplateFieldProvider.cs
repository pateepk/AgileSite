using System.Collections.Generic;

namespace CMS.FormEngine
{
    /// <summary>
    /// Describes interface providing fields for code generation template.
    /// </summary>
    internal interface IContentItemTemplateFieldProvider
    {
        /// <summary>
        /// Returns all fields which will be generated in code template.
        /// </summary>
        IEnumerable<FormFieldInfo> GetFields();
    }
}
