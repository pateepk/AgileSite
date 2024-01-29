using System.Collections.Generic;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Provides access to dynamic object types
    /// </summary>
    internal interface ICustomTableTypeProvider
    {
        /// <summary>
        /// Returns all available dynamic types
        /// </summary>
        IEnumerable<string> GetTypes();
    }
}