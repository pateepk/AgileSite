using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider string indexed dictionary.
    /// </summary>
    [Obsolete("Use ProviderInfoDictionary<string> instead.")]
    public class ProviderStringDictionary : ProviderInfoDictionary<string>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnNames">Column names included in the object key (list of columns separated by semicolon)</param>
        public ProviderStringDictionary(string objectType, string columnNames)
            : base(objectType, columnNames, StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}