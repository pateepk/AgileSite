using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Core.Internal;


namespace CMS.Core
{
    /// <summary>
    /// Provides information about installable modules.
    /// </summary>
    /// <remarks>
    /// The default implementation does not provide any information about installable modules.
    /// </remarks>
    internal class DefaultInstallableModulesInformationService : IInstallableModulesInformationService
    {
        /// <summary>
        /// Gets set of module names which are not installed and therefore omitted from initialization.
        /// </summary>
        /// <returns>Returns null and therefore all installable modules are omitted from initialization.</returns>
        public ISet<string> GetModuleNamesOmittedFromInitialization()
        {
            return null;
        }
    }
}
