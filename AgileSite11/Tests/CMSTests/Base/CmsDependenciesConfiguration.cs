using System;
using System.IO;
using System.Reflection;

namespace CMS.Tests
{
    /// <summary>
    /// Represents a configuration of the test environment related to CMS dependencies.
    /// </summary>
    /// <remarks>
    /// CMS dependencies are third-party assemblies that Kentico requires to run.
    /// They are kept in a special folder to prevent version conflicts.
    /// The runtime is configured to look in this folder when a referenced assembly is not found.
    /// This configuration allows developers to use different versions of third-party assemblies then Kentico does.
    /// </remarks>
    internal sealed class CmsDependenciesConfiguration
    {
        /// <summary>
        /// The name of the folder with CMS dependencies.
        /// </summary>
        private const string mDependenciesFolderName = "CMSDependencies";


        /// <summary>
        /// The name of the setting with the path to the folder that contains CMS dependencies.
        /// </summary>
        private const string mDependenciesSettingName = "CMSTestDependenciesFolderPath";


        /// <summary>
        /// The message of the exception that is thrown when the folder with CMS dependencies is not found.
        /// </summary>
        private const string mExceptionMessage = @"The folder with CMS dependencies was not found. Tests require these dependencies to run correctly.
There are several options how to fix this problem when working with the CMS solution:
1. Place the folder with the test project into the folder that contains the CMS solution.
2. Add the CMSTestDependenciesFolderPath application setting to the configuration file of the test project. The dependencies are located in the CMS/CMSDependencies subfolder of the folder that contains the CMS solution.";


        /// <summary>
        /// The path to the folder with CMS dependencies.
        /// </summary>
        private readonly string mDependenciesFolderPath;


        /// <summary>
        /// Gets the path to the folder with CMS dependencies.
        /// </summary>
        public string DependenciesFolderPath
        {
            get
            {
                return mDependenciesFolderPath;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CmsDependenciesConfiguration"/> class.
        /// </summary>
        /// <param name="dependenciesFolderPath">The path to the folder with CMS dependencies.</param>
        private CmsDependenciesConfiguration(string dependenciesFolderPath)
        {
            mDependenciesFolderPath = dependenciesFolderPath;
        }


        /// <summary>
        /// Detects the path to the folder with CMS dependencies.
        /// </summary>
        /// <remarks>
        /// This method uses the following strategies to locate the folder with CMS dependencies:
        /// <list type="bullet">
        /// <item><description>Look for the CMSTestDependenciesFolderPath application setting in the test configuration file.</description></item>
        /// <item><description>Look for the CMSDependencies subfolder in the folder where the test assembly is located. This strategy is intended for projects that use the Kentico.Libraries NuGet package.</description></item>
        /// <item><description>Look for the CMS/CMSDependencies subfolder in the CMS solution folder. This is the original strategy for test projects that are a part of the CMS solution.</description></item>
        /// </list>
        /// </remarks>
        /// <returns>A configuration object with a detected path to an existing folder with CMS dependencies.</returns>
        /// <exception cref="DirectoryNotFoundException">The folder with CMS dependencies was not found.</exception>
        public static CmsDependenciesConfiguration Create()
        {
            var providers = new Func<string>[]
            {
                GetDependenciesFolderPathFromConfiguration,
                GetDependenciesFolderPathFromCodeBase,
                GetDependenciesFolderPathFromSolution
            };

            try
            {
                foreach (var provider in providers)
                {
                    var dependenciesFolderPath = provider();

                    if (!String.IsNullOrEmpty(dependenciesFolderPath) && Directory.Exists(dependenciesFolderPath))
                    {
                        return new CmsDependenciesConfiguration(dependenciesFolderPath);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new DirectoryNotFoundException(mExceptionMessage, exception);
            }

            // Indicates that CMSDependencies folder wasn't defined
            return new CmsDependenciesConfiguration(String.Empty);
        }


        private static string GetDependenciesFolderPathFromConfiguration()
        {
            return TestsConfig.GetTestAppSetting(mDependenciesSettingName);
        }


        private static string GetDependenciesFolderPathFromCodeBase()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codeBase = assembly.CodeBase;
            var filePath = assembly.Location;

            if (codeBase.StartsWith((Uri.UriSchemeFile), StringComparison.OrdinalIgnoreCase))
            {
                filePath = GetFilePathFromCodeBase(codeBase);
            }

            return Path.Combine(Path.GetDirectoryName(filePath), mDependenciesFolderName);
        }


        private static string GetDependenciesFolderPathFromSolution()
        {
            if (TestsConfig.SolutionFolderPath != null)
            {
                return Path.Combine(TestsConfig.SolutionFolderPath, "CMS", mDependenciesFolderName);
            }

            return null;
        }


        private static string GetFilePathFromCodeBase(string codeBase)
        {
            int start = Uri.UriSchemeFile.Length + Uri.SchemeDelimiter.Length;

            if (codeBase[start] == '/') // Third slash means a local path
            {
                // Handle Windows drive specifications
                if (codeBase[start + 2] == ':')
                {
                    ++start;
                }
                // else leave the last slash so path is absolute
            }
            else // It's either a Windows drive specification or a share
            {
                if (codeBase[start + 1] != ':')
                {
                    start -= 2; // Back up to include two slashes
                }
            }

            return codeBase.Substring(start);
        }
    }
}
