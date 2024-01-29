using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.IO;
using SystemIO = System.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Encapsulates loading of configuration files from embedded resources.
    /// </summary>
    internal class ConfigurationPropertiesLoader
    { 
        private const string PROPERTIES_NAMESPACE = "CMS.PortalEngine.Configuration";
        private const string GROUPS_NAMESPACE = "_Groups";

        private static readonly Lazy<Regex> groupsRegex = new Lazy<Regex>(() => RegexHelper.GetRegex("\\[Group:(\\w+)]", RegexOptions.Compiled));

        private Assembly ExecutingAssembly { get; set; }


        /// <summary>
        /// Creates an instance of the <see cref="ConfigurationPropertiesLoader"/> class.
        /// </summary>
        public ConfigurationPropertiesLoader()
        {
            ExecutingAssembly = Assembly.GetExecutingAssembly();
        }


        /// <summary>
        /// Loads the properties from resource.
        /// </summary>
        /// <param name="resourceNamespace">Resource namespace.</param>
        /// <param name="resourceName">Resource name.</param>
        /// <remarks>Both <paramref name="resourceNamespace"/> and <paramref name="resourceName"/> are case sensitive.</remarks>
        /// <exception cref="SystemIO.FileNotFoundException">Is thrown when requested resource is not found.</exception>
        public string Load(string resourceNamespace, string resourceName)
        {
            var fullResourceName = $"{PROPERTIES_NAMESPACE}.{resourceNamespace}.{resourceName}";
            
            if (!IsExistingResource(fullResourceName))
            {
                throw new SystemIO.FileNotFoundException($"Resource {resourceNamespace}.{resourceName} not found.");
            }

            var resourceStream = ExecutingAssembly.GetManifestResourceStream(fullResourceName);

            string xml;
            using (var reader = StreamReader.New(resourceStream))
            {
                xml = reader.ReadToEnd();
            }

            return ResolvePropertyGroups(xml);
        }
        
        
        private string ResolvePropertyGroups(string xml)
        {
            return groupsRegex.Value.Replace(xml, ResolvePropertyGroup);
        }

        
        private string ResolvePropertyGroup(Match m)
        {
            var name = m.Groups[1].ToString();
            return Load(GROUPS_NAMESPACE, name + ".xml");
        }


        private bool IsExistingResource(string fullResourceName)
        {
            return ExecutingAssembly.GetManifestResourceNames().Contains(fullResourceName);
        }
    }
}
