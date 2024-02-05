using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;

namespace CMS.Tests
{
    /// <summary>
    /// Shared tests configuration
    /// </summary>
    public static class TestsConfig
    {
        private static readonly Lazy<Configuration> mGlobalTestsConfig = new Lazy<Configuration>(GetGlobalTestsConfig);               
        private static readonly Lazy<string> mSolutionFolderPath = new Lazy<string>(GetSolutionFolderPath);


        /// <summary>
        /// The path to the root folder of the solution, if found.
        /// </summary>
        public static string SolutionFolderPath
        {
            get
            {
                return mSolutionFolderPath.Value;
            }
        }


        /// <summary>
        /// Global configuration file for tests (Tests.config located in root directory)
        /// </summary>
        public static Configuration GlobalTestsConfig
        {
            get
            {
                return mGlobalTestsConfig.Value;
            }
        }


        /// <summary>
        /// Gets global configuration file for tests (Tests.config located in root directory)
        /// </summary>
        private static Configuration GetGlobalTestsConfig()
        {
            if (SolutionFolderPath == null)
            {
                return null;
            }

            var efm = new ExeConfigurationFileMap { ExeConfigFilename = Path.Combine(SolutionFolderPath, "Tests.config") };
            return ConfigurationManager.OpenMappedExeConfiguration(efm, ConfigurationUserLevel.None);
        }


        private static bool HasNoParentWithName(this DirectoryInfo directory, string directoryName)
        {
            return ((directory.FullName.IndexOf("\\" + directoryName + "\\", StringComparison.InvariantCultureIgnoreCase) == -1) 
                       && !directory.FullName.EndsWith("\\" + directoryName, StringComparison.InvariantCultureIgnoreCase));
        }


        private static bool ContainsFileWithExtension(this DirectoryInfo directory, string extension)
        {
            var searchPattern = "*" + extension;

            return directory.EnumerateFiles(searchPattern).Any(file => Path.GetExtension(file.FullName) == extension);
        }


        private static bool ContainsFile(this DirectoryInfo directory, string filename)
        {
            return File.Exists(Path.Combine(directory.FullName, filename));
        }


        /// <summary>
        /// Gets a path to the root folder of the solution provided that it contains a test project with current test.
        /// </summary>
        private static string GetSolutionFolderPath()
        {
            try
            {
                var codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var path = Path.GetDirectoryName(codeBase.LocalPath);
                var folder = new DirectoryInfo(path);

                while (folder != null)
                {
                    if (folder.HasNoParentWithName("Debug") && folder.HasNoParentWithName("Release") && (folder.ContainsFileWithExtension(".sln") || folder.ContainsFile("Tests.config")))
                    {
                        return folder.FullName;
                    }
                    folder = folder.Parent;
                }
            }
            catch (SecurityException)
            {
                // Security exception means that the Kentico solution folder was not found and the user is required to provide the folder paths using the configuration file.
            }

            return null;
        }


        /// <summary>
        /// Gets the app setting from test configuration file 
        /// </summary>
        /// <param name="name">App setting name</param>
        public static string GetTestAppSetting(string name)
        {
            var value = ConfigurationManager.AppSettings[name];
            if (value == null)
            {
                // Try global config if not found
                if ((GlobalTestsConfig != null) && GlobalTestsConfig.HasFile)
                {
                    var setting = GlobalTestsConfig.AppSettings.Settings[name];
                    if (setting != null)
                    {
                        value = setting.Value;
                    }
                }
            }

            return value;
        }


        /// <summary>
        /// Gets the connection string of the given name from test configuration file
        /// </summary>
        /// <param name="name">Connection string name</param>
        public static string GetTestConnectionString(string name)
        {
            // Get test connection string from app.config
            var connString = ConfigurationManager.ConnectionStrings[name];
            if (connString != null)
            {
                return connString.ConnectionString;
            }

            // Get test connection string from global config
            if ((GlobalTestsConfig != null) && GlobalTestsConfig.HasFile)
            {
                connString = GlobalTestsConfig.ConnectionStrings.ConnectionStrings[name];
                if (connString != null)
                {
                    return connString.ConnectionString;
                }
            }

            return null;
        }
    }
}
