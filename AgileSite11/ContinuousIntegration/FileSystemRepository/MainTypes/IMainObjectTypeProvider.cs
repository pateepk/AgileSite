using System.Collections.Generic;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Provides access to main object types.
    /// Main object type is type without a parent type.
    /// </summary>
    internal interface IMainObjectTypeProvider
    {
        /// <summary>
        /// Returns set of supported main object types.
        /// </summary>
        ISet<string> GetObjectTypes();
    }
}
