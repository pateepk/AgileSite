using System;
using System.Reflection;
using System.Xml;

namespace CMS.Core
{
    /// <summary>
    /// Provides support for determining assembly version from dependencies folder.
    /// </summary>
    internal static class AssemblyVersionHelper
    {
        /// <summary>
        /// Returns dependency version considering possible assembly binding redirect in configuration file.
        /// </summary>
        internal static Version GetDependencyVersion(AssemblyName name)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                var namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1");

                var bindingRedirect = doc.SelectSingleNode($"/configuration/runtime/ns:assemblyBinding/ns:dependentAssembly/ns:assemblyIdentity[@name='{name.Name}']/following-sibling::ns:bindingRedirect", namespaceManager);
                var newVersion = bindingRedirect?.Attributes?["newVersion"].Value;
                var oldVersion = bindingRedirect?.Attributes?["oldVersion"].Value;

                if (string.IsNullOrEmpty(oldVersion) || string.IsNullOrEmpty(newVersion))
                {
                    return name.Version;
                }

                var oldVersions = oldVersion.Split('-');
                if (oldVersions.Length != 2)
                {
                    return name.Version;
                }

                var minimalVersion = new Version(oldVersions[0]);
                var maximalVersion = new Version(oldVersions[1]);

                if (name.Version > minimalVersion && name.Version < maximalVersion)
                {
                    return new Version(newVersion);
                }

                return name.Version;
            }
            catch
            {
                return name.Version;
            }
        }
    }
}
