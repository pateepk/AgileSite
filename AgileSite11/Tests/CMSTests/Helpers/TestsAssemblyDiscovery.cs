using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.Core;
using CMS.IO;

namespace CMS.Tests
{
    /// <summary>
    /// Only allow to discover assemblies referenced by the test assembly (or explicitly included by <see cref="IncludeAssemblyAttribute"/> in the assembly).
    /// </summary>
    internal class TestsAssemblyDiscovery : AssemblyDiscovery
    {
        // Set of assemblies that are allowed for given test(s) run.
        private ISet<string> mAllowedAssemblies;


        /// <summary>
        /// Creates new instance of the <see cref="TestsAssemblyDiscovery"/> and explores list of discoveries allowed for given <paramref name="currentAssembly"/>.
        /// </summary>
        /// <param name="currentAssembly">Assembly the test(s) will be run in (see <see cref="AutomatedTests.SetupTestsAssemblyDiscovery()"/>).</param>
        public TestsAssemblyDiscovery(Assembly currentAssembly)
        {
            mAllowedAssemblies = GetInitializedSetOfAllowedAssemblies(currentAssembly);
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
        public new IEnumerable<string> GetAssembliesFilePaths()
        {
            // Inherited method is available only in assemblies where CMS.Core internals are visible.
            // This method will be available where CMS.Tests internals are visible.
            return base.GetAssembliesFilePaths();
        }


        /// <summary>
        /// Allows exclusion on <paramref name="assemblyName"/> premise.
        /// </summary>
        protected override bool IsExcluded(AssemblyName assemblyName)
        {
            return !mAllowedAssemblies.Contains(assemblyName.FullName);
        }


        /// <summary>
        /// Returns collection of full names of assemblies that are included using <see cref="IncludeAssemblyAttribute"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to search for the <see cref="IncludeAssemblyAttribute"/> in.</param>
        private AssemblyName[] GetIncludedAssemblies(Assembly assembly)
        {
            const string LIBRARY_EXTENSION = ".dll";

            var folderPath = new Uri(Path.GetDirectoryName(assembly.CodeBase)).LocalPath;

            var relativeSearchPath = GetRelativeSearchPath(AppDomain.CurrentDomain);
            var directories = String.IsNullOrEmpty(relativeSearchPath)
                ? new[] { folderPath }
                : relativeSearchPath.Split(new[] { System.IO.Path.PathSeparator }, StringSplitOptions.None)
                                    .Select(path => Path.Combine(folderPath, path))
                                    .Distinct(StringComparer.InvariantCultureIgnoreCase);

            return assembly
                .GetCustomAttributes<IncludeAssemblyAttribute>()
                .SelectMany(include => directories.Select(path => Path.Combine(path, include.AssemblyName + LIBRARY_EXTENSION)))
                .Where(File.Exists)
                .Select(assemblyPath =>
                {
                    try
                    {
                        return AssemblyName.GetAssemblyName(assemblyPath);
                    }
                    catch(BadImageFormatException)
                    {
                        // Ignore error caused by load of native assembly
                        return null;
                    }
                })
                .Where(assemblyName => assemblyName != null)
                .ToArray();
        }


        /// <summary>
        /// Recursively adds all assemblies that are either referenced (in project) or included (by using <see cref="IncludeAssemblyAttribute"/>)
        /// in the <paramref name="assembly"/> into the <paramref name="allowedAssemblyFullNames"/>.
        /// </summary>
        private void AddReferencedAssemblies(Assembly assembly, HashSet<string> allowedAssemblyFullNames)
        {
            var references = assembly.GetReferencedAssemblies();
            var includes = GetIncludedAssemblies(assembly);

            // Select only new assemblies, so they are not loaded twice
            var newReferences = Enumerable
                .Empty<AssemblyName>()
                .Union(references)
                .Union(includes)
                .Where(assemblyName => allowedAssemblyFullNames.Add(assemblyName.FullName))
                .Select(assemblyName => Assembly.Load(assemblyName));            

            try
            {
                foreach(var newAssembly in newReferences)
                {
                    AddReferencedAssemblies(newAssembly, allowedAssemblyFullNames);
                }
            }
            catch
            {
                // Suppress error for assemblies that could not load (no need to consider their references discoverable)
            }
        }


        /// <summary>
        /// Returns set of allowed assemblies that are resolved from references in <paramref name="currentAssembly"/>.
        /// </summary>
        private ISet<string> GetInitializedSetOfAllowedAssemblies(Assembly currentAssembly)
        {
            var allowedAssemblies = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                currentAssembly.FullName
            };

            AddReferencedAssemblies(currentAssembly, allowedAssemblies);
            return allowedAssemblies;
        }


        /// <summary>
        /// Method is called when an exception occurs during an assembly's load.
        /// </summary>
        /// <param name="filePath">Full path to the file that was supposed to contain an assembly.</param>
        /// <param name="exception">Exception that occurs during load.</param>
        /// <remarks>
        /// Invalid operation exception is thrown as this might only happen in case a file of required assembly was not found.
        /// </remarks>
        protected override void OnGetAssemblyFailed(string filePath, Exception exception)
        {
            throw new InvalidOperationException("Could not load assembly: " + filePath, exception);
        }


        /// <summary>
        /// Returns relative paths where the application should look for assemblies.
        /// </summary>
        /// <param name="currentDomain">Current application domain</param>
        /// <remarks>
        /// Due to the way some test runners handle application domains, will read configuration file also if the current domain probes only root directory.
        /// </remarks>
        protected override string GetRelativeSearchPath(AppDomain currentDomain)
        {
            if (String.IsNullOrEmpty(currentDomain.RelativeSearchPath))
            {
                return ReadProbingPathFromConfig(currentDomain.SetupInformation.ConfigurationFile);
            }

            return currentDomain.RelativeSearchPath;
        }
    }
}
