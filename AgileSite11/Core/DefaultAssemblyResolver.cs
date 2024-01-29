using System;
using System.IO;
using System.Reflection;

namespace CMS.Core
{
    /// <summary>
    /// Resolves private assemblies.
    /// </summary>
    /// <remarks>
    /// Private assemblies are located in specific folder and are intended for Kentico only.
    /// Kentico modules reference private assemblies to avoid conflict with customer's assemblies in the bin folder.
    /// Assemblies are organized into subfolders with names that contain assembly name and version.
    /// As the result multiple versions of the same assembly are supported.
    /// </remarks>
    internal sealed class DefaultAssemblyResolver
    {
        /// <summary>
        /// The path to the folder with private assemblies.
        /// </summary>
        private readonly string mBaseFolderPath;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAssemblyResolver"/> class with the specified folder path.
        /// </summary>
        /// <param name="baseFolderPath">The path to the folder with private assemblies.</param>
        public DefaultAssemblyResolver(string baseFolderPath)
        {
            if (String.IsNullOrEmpty(baseFolderPath))
            {
                throw new ArgumentException("The path to the folder with private assemblies is not specified", nameof(baseFolderPath));
            }

            mBaseFolderPath = baseFolderPath;
        }


        /// <summary>
        /// Loads the assembly into an execution context using the specified context.
        /// </summary>
        /// <param name="resolveEventArgs">Data for assembly resolution.</param>
        /// <returns>The assembly matching the specified context; or null, if the assembly cannot be resolved.</returns>
        public Assembly GetAssembly(ResolveEventArgs resolveEventArgs)
        {
            var assemblyName = new AssemblyName(resolveEventArgs.Name);
            var filePath = GetAssemblyFilePath(assemblyName);

            return GetAssemblyFromFile(filePath);
        }


        /// <summary>
        /// Loads the assembly from the specified file into an execution context.
        /// </summary>
        /// <param name="filePath">The path to the assembly.</param>
        /// <returns>The assembly from the specified file; or null, if the assembly does not exist or could not be loaded.</returns>
        private Assembly GetAssemblyFromFile(string filePath)
        {
            Assembly assembly = null;
            try
            {
#pragma warning disable BH1014 // Do not use System.IO
                if (File.Exists(filePath))
#pragma warning restore BH1014 // Do not use System.IO
                {
                    assembly = Assembly.LoadFrom(filePath);
                }
            }
            catch
            {
                assembly = null;
            }

            return assembly;
        }


        /// <summary>
        /// Returns a path to the private assembly with the specified name.
        /// </summary>
        /// <param name="name">The name of the private assembly.</param>
        /// <returns>A path to the private assembly with the specified name.</returns>
        private string GetAssemblyFilePath(AssemblyName name)
        {
            var version = AssemblyVersionHelper.GetDependencyVersion(name);

            var folderName = $"{name.Name}.{version}";
            var fileName = $"{name.Name}.dll";

#pragma warning disable BH1014 // Do not use System.IO
            return Path.Combine(mBaseFolderPath, folderName, fileName);
#pragma warning restore BH1014 // Do not use System.IO
        }
    }
}