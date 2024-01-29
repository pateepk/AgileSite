﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace CMS.Core
{
    /// <summary>
    /// Provides loading of application assemblies.
    /// </summary>
    public static class AssemblyDiscoveryHelper
    {
        // Creates lazy object that contains either default implementation of AssemblyDiscovery or instance provided as the only parameter
        private static readonly Func<AssemblyDiscovery, Lazy<AssemblyDiscovery>> LAZY_INITIALIZER =
            assemblyDiscovery => new Lazy<AssemblyDiscovery>(() => assemblyDiscovery ?? new AssemblyDiscovery());

        // Current instance of AssemblyDiscovery.
        private static Lazy<AssemblyDiscovery> mAssemblyDiscovery = LAZY_INITIALIZER(null);


        /// <summary>
        /// Current instance of <see cref="AssemblyDiscovery"/> (or its derived class) used by the helper.
        /// </summary>
        internal static AssemblyDiscovery AssemblyDiscovery
        {
            get
            {
                return mAssemblyDiscovery.Value;
            }

            set
            {
                mAssemblyDiscovery = LAZY_INITIALIZER(value);
            }
        }


        /// <summary>
        /// Returns an enumerable collection of application assemblies.
        /// </summary>
        /// <param name="discoverableOnly">A value indicating whether the discovery process will locate only assemblies decorated with the <see cref="CMS.AssemblyDiscoverableAttribute"/> attribute.</param>
        /// <remarks>
        /// The discovery process looks for assemblies in the directories that the assembly resolver probes.
        /// By default, all the application assemblies are returned, but there are exceptions.
        /// <list type="number">
        /// <item><description>If the <paramref name="discoverableOnly"/> is set to <c>true</c>, assemblies without the <see cref="CMS.AssemblyDiscoverableAttribute"/> attribute are excluded from discovery.</description></item>
        /// <item><description>If there is a file with the "exclude" extension, for example MyCustomAssembly.dll.exclude, the MyCustomAssembly.dll is excluded from discovery.</description></item>
        /// <item><description>Assemblies from the GAC are always excluded from discovery.</description></item>
        /// </list>
        /// </remarks>
        /// <returns>An enumerable collection of application assemblies.</returns>
        public static IEnumerable<Assembly> GetAssemblies(bool discoverableOnly)
        {
            return AssemblyDiscovery.GetAssemblies(discoverableOnly);
        }


        /// <summary>
        /// Returns an enumerable collection of file paths to all assemblies in the directories that the assembly resolver probes.
        /// </summary>
        /// <remarks>
        /// The assembly resolver probes for assemblies in the application directory.
        /// If the application setup includes a list of search paths relative to the application directory, the assembly resolver probes for assemblies only in the specified subdirectories.
        /// A web application is a good example as its setup includes a relative path to the bin subfolder.
        /// </remarks>
        /// <returns>An enumerable collection of file paths to all assemblies in the directories that the assembly resolver probes.</returns>
        public static IEnumerable<string> GetAssembliesFilePaths()
        {
            return AssemblyDiscovery.GetAssembliesFilePaths();
        }


        /// <summary>
        /// Returns true if the given assembly is discoverable.
        /// </summary>
        /// <param name="filePath">Assembly file path</param>
        public static bool IsAssemblyDiscoverable(string filePath)
        {
            return AssemblyDiscovery.IsAssemblyDiscoverable(filePath);
        }
    }
}
