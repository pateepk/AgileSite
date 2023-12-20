using System.Collections.Generic;

namespace CMS.Core.Internal
{
    /// <summary>
    /// Provides information about installable modules.
    /// </summary>
    /// <remarks>
    /// This interface is for internal use only and shall not be considered as part of public API.
    /// </remarks>
    public interface IInstallableModulesInformationService
    {
        /// <summary>
        /// Gets set of module names which are not installed and therefore omitted from initialization.
        /// </summary>
        /// <returns>Set of module names omitted from initialization. Returns null if all installable modules are omitted from initialization.</returns>
        ISet<string> GetModuleNamesOmittedFromInitialization();
    }
}
