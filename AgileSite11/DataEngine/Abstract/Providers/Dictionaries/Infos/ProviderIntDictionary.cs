using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider integer indexed dictionary.
    /// </summary>
    [Obsolete("Use ProviderInfoDictionary<int> instead.")]
    public class ProviderIntDictionary : ProviderInfoDictionary<int>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        public ProviderIntDictionary(string objectType, string columnNames)
            : base(objectType, columnNames)
        {
        }
    }
}