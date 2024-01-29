using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Definition of the structured field configuration
    /// </summary>
    public interface IStructuredField
    {
        /// <summary>
        /// Info field name
        /// </summary>
        string FieldName { get; }


        /// <summary>
        /// Creates the object from the given XML value
        /// </summary>
        IStructuredData CreateStructuredValue(string xmlValue);
    }
}
