using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using CMS.Core;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Loads mapping information from assembly about view path and view type.
    /// </summary>
    internal static class EmbeddedViewLoader
    {
        public static Dictionary<string, Type> LoadMappingTable()
        {
            var viewTypeMapping = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            var embeddedViewAssemblies = GetEmbeddedViewAssemblies();

            foreach (var assembly in embeddedViewAssemblies)
            {
                var fullnameTypeMapping = GetWebViewPageFullNameTypeMapping(assembly.Assembly);
                foreach (var viewPath in assembly.Attribute.ViewPaths)
                {
                    var viewVirtualPath = AssemblyViewPathToVirtualPath(viewPath);

                    if (fullnameTypeMapping.TryGetValue(TypeNameFromPath(viewVirtualPath), out Type type))
                    {
                        viewTypeMapping.Add(viewVirtualPath, type);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Embedded view type was not found for view path '{viewVirtualPath}'.");
                    }
                }
            }

            return viewTypeMapping;
        }


        internal static IEnumerable<EmbeddedViewAssembly> GetEmbeddedViewAssemblies()
        {
            return AssemblyDiscoveryHelper.GetAssemblies(discoverableOnly: false)
               .Where(assembly => assembly.GetName().Name.EndsWith(".views", StringComparison.OrdinalIgnoreCase))
               .Select(assembly => new EmbeddedViewAssembly
               {
                   Assembly = assembly,
                   Attribute = assembly.GetCustomAttributes(typeof(EmbeddedViewAssemblyAttribute), false).FirstOrDefault() as EmbeddedViewAssemblyAttribute
               })
               .Where(x => x.Attribute != null);
        }


        internal static Dictionary<string, Type> GetWebViewPageFullNameTypeMapping(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => typeof(WebViewPage).IsAssignableFrom(t)).ToDictionary(key => key.FullName, StringComparer.Ordinal);
        }


        private static string TypeNameFromPath(string path)
        {
            return "ASP._Page" + path.TrimStart('~')
                .Replace(".", "_")
                .Replace("/", "_");
        }


        private static string AssemblyViewPathToVirtualPath(string assemblyViewPath)
        {
            return "~/" + assemblyViewPath.Replace("\\", "/");
        }
    }
}
