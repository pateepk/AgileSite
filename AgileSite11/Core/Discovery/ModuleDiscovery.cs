using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CMS.Core
{
    /// <summary>
    /// Provides loading of application modules.
    /// </summary>
    public class ModuleDiscovery
    {
        #region "Methods"

        /// <summary>
        /// Returns an enumerable collection of application modules.
        /// </summary>
        /// <remarks>
        /// The discovery process looks for modules in discoverable assemblies (see <see cref="CMS.Core.AssemblyDiscoveryHelper"/> for more information).
        /// The module type is located using the <see cref="RegisterModuleAttribute"/>.
        /// </remarks>
        /// <returns>An enumerable collection of application modules.</returns>
        public IEnumerable<ModuleEntry> GetModules()
        {
            var modules = new List<ModuleEntry>();
            var assemblies = AssemblyDiscoveryHelper.GetAssemblies(discoverableOnly: true);

            foreach (var assembly in assemblies)
            {
                modules.AddRange(GetModules(assembly));
            }

            return modules;
        }


        /// <summary>
        /// Returns an enumerable collection of application modules from the specified assembly.
        /// </summary>
        /// <remarks>
        /// The module type is located using the <see cref="RegisterModuleAttribute"/>.
        /// </remarks>
        /// <param name="assembly">The assembly to retrieve modules from.</param>
        /// <returns>An enumerable collection of application modules from the specified assembly.</returns>
        public IEnumerable<ModuleEntry> GetModules(Assembly assembly)
        {
            var modules = Enumerable.Empty<ModuleEntry>();

            try
            {
                modules = assembly.GetCustomAttributes(typeof (RegisterModuleAttribute), false)
                                    .Cast<RegisterModuleAttribute>()
                                    .Select(attribute =>
                                    {
                                        var instance = ((ModuleEntry)Activator.CreateInstance(attribute.Type));
                                        instance.Assembly = assembly;

                                        return instance;
                                    });
            }
            catch (Exception exception)
            {
                var error = new DiscoveryError(exception, assembly.FullName);
                error.LogEvent();
            }

            return modules;
        }

        #endregion
    }

}