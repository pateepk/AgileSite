using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using Mono.Cecil;

namespace CMS.Core
{
    /// <summary>
    /// Provides loading of application assemblies.
    /// </summary>
    internal class AssemblyDiscovery
    {
#pragma warning disable BH1014 // Do not use System.IO

        #region "Constants and variables"

        /// <summary>
        /// The full name of the AssemblyDiscoverableAttribute class.
        /// </summary>
        private const string ASSEMBLY_DISCOVERABLE_ATTRIBUTE_FULLNAME = "CMS.AssemblyDiscoverableAttribute";

        /// <summary>
        /// A set of file names that will be excluded from discovery.
        /// </summary>
        /// <seealso cref="IsExcludedImplicitly(string)"/>
        private static readonly HashSet<string> mExcludedFileNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            // WIX libraries must be excluded because the installer hosts the application (and therefore the discovery runs) and its payload includes these native libraries.
            "MBAHOST.DLL",
            "MBAPREQ.DLL",

            // All known 3rd party assemblies
            "AjaxControlToolkit.dll",
            "AjaxMin.dll",
            "AngleSharp.dll",
            "AWSSDK.Core.dll",
            "AWSSDK.S3.dll",
            "Castle.Core.dll",
            "Castle.Windsor.dll",
            "CMS.Activities.Loggers.dll",
            "CMS.CKEditor.Web.UI.dll",
            "CMS.DocumentEngine.Web.UI.XmlSerializers.dll",
            "CMS.LicenseProvider.XmlSerializers.dll",
            "CMS.Mvc.dll",
            "DocumentFormat.OpenXml.dll",
            "DotNetOpenAuth.dll",
            "Facebook.dll",
            "GlobalLink.Connect.dll",
            "ITHit.WebDAV.Server.dll",
            "Lucene.Net.v3.dll",
            "Lucene.Net.WordNet.SynExpand.dll",
            "MaxMindGEOIP.dll",
            "Microsoft.Azure.Search.dll",
            "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll",
            "Microsoft.Data.Edm.dll",
            "Microsoft.Data.OData.dll",
            "Microsoft.Data.Services.Client.dll",
            "Microsoft.IdentityModel.dll",
            "Microsoft.Office.Client.Policy.dll",
            "Microsoft.Office.Client.TranslationServices.dll",
            "Microsoft.Office.SharePoint.Tools.dll",
            "Microsoft.Online.SharePoint.Client.Tenant.dll",
            "Microsoft.ProjectServer.Client.dll",
            "Microsoft.Rest.ClientRuntime.Azure.dll",
            "Microsoft.Rest.ClientRuntime.dll",
            "Microsoft.SharePoint.Client.dll",
            "Microsoft.SharePoint.Client.DocumentManagement.dll",
            "Microsoft.SharePoint.Client.Publishing.dll",
            "Microsoft.SharePoint.Client.Runtime.dll",
            "Microsoft.SharePoint.Client.Runtime.Windows.dll",
            "Microsoft.SharePoint.Client.Search.Applications.dll",
            "Microsoft.SharePoint.Client.Search.dll",
            "Microsoft.SharePoint.Client.Taxonomy.dll",
            "Microsoft.SharePoint.Client.UserProfiles.dll",
            "Microsoft.SharePoint.Client.WorkflowServices.dll",
            "Microsoft.Spatial.dll",
            "Microsoft.SqlServer.TransactSql.ScriptDom.dll",
            "Microsoft.Web.Services3.dll",
            "Microsoft.Web.XmlTransform.dll",
            "Microsoft.WindowsAzure.Configuration.dll",
            "Microsoft.WindowsAzure.Storage.dll",
            "Mono.Cecil.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.Rocks.dll",
            "NetSpell.SpellChecker.dll",
            "Newtonsoft.Json.dll",
            "NuGet.Core.dll",
            "OpenPop.dll",
            "PayPal.dll",
            "PDFClown.dll",
            "PreMailer.Net.dll",
            "System.Data.HashFunction.Core.dll",
            "System.Data.HashFunction.CRC.dll",
            "System.Data.HashFunction.Interfaces.dll",
            "System.Net.Http.Formatting.dll",
            "System.Spatial.dll",
            "System.Web.Http.dll",
            "System.Web.Http.WebHost.dll",
            "ThoughtWorks.QRCode.dll",
            "Ude.dll"
        };

        private ICollection<Assembly> mAssemblies;
        private ICollection<Assembly> mDiscoverableAssemblies;

        #endregion


        #region "Properties"

        /// <summary>
        /// A list of application assemblies.
        /// </summary>
        protected ICollection<Assembly> Assemblies
        {
            get
            {
                return mAssemblies ?? (mAssemblies = ExecuteGetAssemblies(false));
            }
        }


        /// <summary>
        /// A list of discoverable application assemblies.
        /// </summary>
        protected ICollection<Assembly> DiscoverableAssemblies
        {
            get
            {
                return mDiscoverableAssemblies ?? (mDiscoverableAssemblies = ExecuteGetAssemblies(true));
            }
        }

        #endregion


        #region "Methods"

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
        public IEnumerable<Assembly> GetAssemblies(bool discoverableOnly)
        {
            return discoverableOnly
                ? DiscoverableAssemblies
                : Assemblies;
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
        public IEnumerable<string> GetAssembliesFilePaths()
        {
            const string LIBRARY_MASK = "*.dll";

            var currentDomain = AppDomain.CurrentDomain;
            var baseDirectory = currentDomain.BaseDirectory;
            var relativeSearchPath = GetRelativeSearchPath(currentDomain);

            IEnumerable<string> directories;
            if (String.IsNullOrEmpty(relativeSearchPath))
            {
                directories = new[] { baseDirectory };
            }
            else
            {
                directories = relativeSearchPath
                    .Split(new[] { Path.PathSeparator }, StringSplitOptions.None)
                    .Select(path => Path.Combine(baseDirectory, path))
                    .Distinct(StringComparer.InvariantCultureIgnoreCase);
            }

            return directories
                .Select(directoryPath => new DirectoryInfo(directoryPath))
                .Where(directory => directory.Exists)
                .SelectMany(directory => directory.GetFiles(LIBRARY_MASK))
                .GroupBy(file => file.Name)
                .Select(fileGroup => fileGroup.First().FullName);
        }


        /// <summary>
        /// Returns true if the given assembly is discoverable.
        /// </summary>
        /// <param name="filePath">Assembly file path</param>
        public bool IsAssemblyDiscoverable(string filePath)
        {
            return AssemblyDefinition
                .ReadAssembly(filePath)
                .CustomAttributes
                .Any(customAttribute => (customAttribute.Constructor.DeclaringType != null)
                    && (customAttribute.Constructor.DeclaringType.FullName == ASSEMBLY_DISCOVERABLE_ATTRIBUTE_FULLNAME));
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns relative paths where the application should look for assemblies.
        /// </summary>
        /// <param name="currentDomain">Current application domain</param>
        /// <remarks>
        /// Only the <see cref="AppDomain.BaseDirectory"/> is relevant if <c>null</c> is returned.
        /// </remarks>
        protected virtual string GetRelativeSearchPath(AppDomain currentDomain)
        {
            return currentDomain.RelativeSearchPath ?? ReadProbingPathFromConfig(currentDomain.SetupInformation.ConfigurationFile);
        }


        /// <summary>
        /// Reads paths where the application should look for assemblies from configuration file on given path.
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file</param>
        /// <remarks>
        /// In subsequently started application domains these paths can be retrieved from <see cref="AppDomain.RelativeSearchPath"/>,
        /// but the default <see cref="AppDomain"/> reads the paths directly from configuration file and does not offer them in the API.
        /// </remarks>
        protected string ReadProbingPathFromConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                // Configuration file name is always provided, despite actual file existence
                return null;
            }

            var doc = new XmlDocument();
            doc.Load(configFilePath);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1");

            var probing = doc.SelectSingleNode("/configuration/runtime/ns:assemblyBinding/ns:probing", namespaceManager);
            return probing?.Attributes?["privatePath"].Value;
        }


        /// <summary>
        /// Returns <c>true</c> if the name of the file is mentioned in <see cref="mExcludedFileNames"/>.
        /// </summary>
        /// <param name="filePath"></param>
        private bool IsExcludedImplicitly(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            return mExcludedFileNames.Contains(fileName);
        }


        /// <summary>
        /// Returns <c>true</c> if a file with exact <paramref name="filePath"/> and ".excluded" suffix exists on disk.
        /// </summary>
        private bool IsExcludedExplicitly(string filePath)
        {
            var hintFilePath = filePath + ".exclude";
            return File.Exists(hintFilePath);
        }


        /// <summary>
        /// Checks if the assembly is excluded (either implicitly by code or explicitly by customer).
        /// </summary>
        private bool IsExcluded(string filePath)
        {
            return IsExcludedImplicitly(filePath) || IsExcludedExplicitly(filePath);
        }


        /// <summary>
        /// Allows exclusion on <paramref name="assemblyName"/> premise.
        /// </summary>
        protected virtual bool IsExcluded(AssemblyName assemblyName)
        {
            return false;
        }


        /// <summary>
        /// If and only if the assembly (specified by <paramref name="assemblyName"/> and <paramref name="filePath"/>) is discoverable,
        /// method loads the assembly and adds it into the <paramref name="assemblies"/> collection.
        /// </summary>
        /// <seealso cref="IsAssemblyDiscoverable(string)"/>
        /// <seealso cref="IsAssemblyDiscoverable(Assembly)"/>
        private void AddOnlyDiscoverableAssembly(ICollection<Assembly> assemblies, Dictionary<string, Assembly> preloadedAssemblies, AssemblyName assemblyName, string filePath)
        {
            Assembly assembly;

            if (preloadedAssemblies.TryGetValue(assemblyName.FullName, out assembly))
            {
                if (IsAssemblyDiscoverable(assembly))
                {
                    assemblies.Add(assembly);
                }
            }
            else if (IsAssemblyDiscoverable(filePath))
            {
                AddAssembly(assemblies, assemblyName);
            }
        }


        /// <summary>
        /// Loads the assembly (specified by <paramref name="assemblyName"/>) and adds it into the <paramref name="assemblies"/> collection.
        /// </summary>
        private void AddAssembly(ICollection<Assembly> assemblies, AssemblyName assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            assemblies.Add(assembly);
        }


        /// <summary>
        /// Returns a read-only collection of application assemblies matching the specified criteria.
        /// </summary>
        /// <param name="onlyDiscoverable">A value indicating whether the discovery process will locate only assemblies decorated with the <see cref="CMS.AssemblyDiscoverableAttribute"/> attribute.</param>
        /// <returns>A read-only collection of application assemblies matching the specified criteria.</returns>
        private ICollection<Assembly> ExecuteGetAssemblies(bool onlyDiscoverable)
        {
            var assemblies = new List<Assembly>();
            var preloadedAssemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .GroupBy(assembly => assembly.FullName)
                .ToDictionary(x => x.Key, x => x.First());

            // Process all found assemblies
            var filePaths = GetAssembliesFilePaths();
            foreach (var filePath in filePaths)
            {
                if (IsExcluded(filePath))
                {
                    continue;
                }

                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(filePath);

                    if (IsExcluded(assemblyName))
                    {
                        continue;
                    }

                    if (onlyDiscoverable)
                    {
                        AddOnlyDiscoverableAssembly(assemblies, preloadedAssemblies, assemblyName, filePath);
                    }
                    else
                    {
                        AddAssembly(assemblies, assemblyName);
                    }
                }
                catch (BadImageFormatException)
                {
                    // Ignore error caused by load of native assembly
                }
                catch (Exception exception)
                {
                    OnGetAssemblyFailed(filePath, exception);

                    // OnGetAssemblyFailed overrides may throw another (wrapped) exception, otherwise throw this one
                    throw;
                }
            }
            assemblies.Sort((a, b) => StringComparer.InvariantCultureIgnoreCase.Compare(a.FullName, b.FullName));

            return assemblies.AsReadOnly();
        }


        /// <summary>
        /// Method is called when an exception occurs during an assembly's load.
        /// </summary>
        /// <param name="filePath">Full path to the file that was supposed to contain an assembly.</param>
        /// <param name="exception">Exception that occurs during load.</param>
        /// <remarks>
        /// Method logs errors using <see cref="DiscoveryError"/> class by default. 
        /// The <see cref="DiscoveryError"/> class logs errors to the EventLog that requires database connection.
        /// To alter this default behavior, override this method.
        /// </remarks>
        protected virtual void OnGetAssemblyFailed(string filePath, Exception exception)
        {
            var error = new DiscoveryError(exception, filePath);
            error.LogEvent();
        }


        /// <summary>
        /// Returns true if the given assembly is discoverable.
        /// </summary>
        /// <param name="assembly">Assembly in question</param>
        private bool IsAssemblyDiscoverable(Assembly assembly)
        {
            return assembly
                .GetCustomAttributesData()
                .Any(customAttribute => (customAttribute.Constructor.DeclaringType != null)
                    && (customAttribute.Constructor.DeclaringType.FullName == ASSEMBLY_DISCOVERABLE_ATTRIBUTE_FULLNAME));
        }

        #endregion

#pragma warning restore BH1014 // Do not use System.IO
    }
}