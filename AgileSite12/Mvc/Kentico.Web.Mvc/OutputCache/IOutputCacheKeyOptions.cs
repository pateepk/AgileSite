using System.Collections.Generic;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Options object definition with required vary by values.
    /// </summary>
    public interface IOutputCacheKeyOptions
    {
        /// <summary>
        /// Adds <see cref="IOutputCacheKey"/> to the collection of required vary by values.
        /// </summary>
        /// <param name="outputCacheKey">Output cache item.</param>
        void AddCacheKey(IOutputCacheKey outputCacheKey);


        /// <summary>
        /// Returns collection of <see cref="IOutputCacheKey"/> objects.
        /// </summary>
        IEnumerable<IOutputCacheKey> GetOutputCacheKeys();
    }
}
