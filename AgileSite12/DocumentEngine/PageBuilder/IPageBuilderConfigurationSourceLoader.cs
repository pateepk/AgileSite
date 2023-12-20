using System;

namespace CMS.DocumentEngine.PageBuilder.Internal
{
    /// <summary>
    /// Loads Page builder configuration source.
    /// </summary>
    public interface IPageBuilderConfigurationSourceLoader
    {
        /// <summary>
        /// Loads Page builder configuration source for a page.
        /// </summary>
        /// <param name="page">Page from which the configuration will be loaded.</param>
        /// <returns>Returns the configuration source.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        PageBuilderConfigurationSource Load(TreeNode page);
    }
}
