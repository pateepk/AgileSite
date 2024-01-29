using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.Core;
using CMS.Core.Internal;

[assembly: RegisterImplementation(typeof(IInstallableModulesInformationService), typeof(CMS.Modules.InstallableModulesInformationService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]


namespace CMS.Modules
{
    /// <summary>
    /// Provides information about installable modules.
    /// </summary>
    /// <remarks>
    /// The <see cref="IInstallableModulesInformationService"/> could be implemented directly by <see cref="InstallableModulesManager"/>,
    /// but it is a singleton and it is not possible to register interface implementation while providing your own singleton instance (new singleton instance
    /// would be created).
    /// </remarks>
    internal class InstallableModulesInformationService : IInstallableModulesInformationService
    {
        /// <summary>
        /// Gets set of module names which are not installed and therefore omitted from initialization.
        /// </summary>
        /// <returns>Set of module names omitted from initialization. Returns null if all installable modules are omitted from initialization.</returns>
        public ISet<string> GetModuleNamesOmittedFromInitialization()
        {
            return InstallableModulesManager.Current.GetModuleNamesOmittedFromInitialization();
        }
    }
}
