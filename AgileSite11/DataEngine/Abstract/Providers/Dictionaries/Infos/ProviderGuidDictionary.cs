using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider GUID indexed dictionary.
    /// </summary>
    [Obsolete("Use ProviderInfoDictionary<Guid> instead.")]
    public class ProviderGuidDictionary : ProviderInfoDictionary<Guid>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        public ProviderGuidDictionary(string objectType, string columnNames)
            : base(objectType, columnNames)
        {
        }
    }
}