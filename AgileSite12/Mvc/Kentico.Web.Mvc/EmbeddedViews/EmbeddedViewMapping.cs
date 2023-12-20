using System;
using System.Collections.Generic;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Maps view relative path to view type
    /// </summary>
    internal class EmbeddedViewMapping
    {
        private Dictionary<string, Type> viewTypeMapping;
        private static Lazy<EmbeddedViewMapping> embeddedViewMapping = new Lazy<EmbeddedViewMapping>(LoadMappping);


        public static EmbeddedViewMapping Instance
        {
            get
            {
                return embeddedViewMapping.Value;
            }
        }


        public virtual bool TryGetView(string virtualPath, out Type type)
        {
            return viewTypeMapping.TryGetValue(virtualPath, out type);
        }


        public virtual bool ContainsView(string virtualPath)
        {
            return viewTypeMapping.ContainsKey(virtualPath);
        }


        private static EmbeddedViewMapping LoadMappping()
        {
            var mapping = new EmbeddedViewMapping();
            mapping.viewTypeMapping = EmbeddedViewLoader.LoadMappingTable();

            return mapping;
        }
    }
}
