using Newtonsoft.Json.Serialization;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides helper methods for serialization.
    /// </summary>
    internal static class SerializerHelper
    {
        /// <summary>
        /// Gets default contract resolver.
        /// </summary>
        public static DefaultContractResolver GetDefaultContractResolver()
        {
            return new CamelCasePropertyNamesContractResolver();
        }
    }
}
