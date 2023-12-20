using System.Web;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Output cache key definition.
    /// </summary>
    public interface IOutputCacheKey
    {
        /// <summary>
        /// Defines a unique name for a cache key.
        /// </summary>
        /// <remarks>Name is used as a prefix for resulting cache key <see cref="GetVaryByCustomString(HttpContextBase, string)"/>.</remarks>
        string Name { get; }


        /// <summary>
        /// Returns cache key specific for the current cache item.
        /// </summary>
        /// <param name="context">An <see cref="HttpContext"/> object that contains information about the current Web request.</param>
        /// <param name="custom">The custom string that specifies which cached response is used to respond to the current request.</param>
        string GetVaryByCustomString(HttpContextBase context, string custom);
    }
}
