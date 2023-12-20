using System;

using CMS.Base;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for <see cref="Uri"/> class.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Tries to retrieve <paramref name="relativePath"/> from absolute Uri. Retrieved <paramref name="relativePath"/> consists of <see cref="Uri.AbsolutePath"/> without it's <see cref="SystemContext.ApplicationPath"/> prefix. Query string parameters are omitted.
        /// </summary>
        /// <param name="uri">Uri from which to retrieve relative path.</param>
        /// <param name="relativePath">Retrieved relative path.</param>
        /// <returns><c>true</c> when relative path has been successfully retrieved, otherwise <c>false</c>.</returns>
        public static bool TryGetRelativePath(this Uri uri, out string relativePath)
        {
            relativePath = String.Empty;

            if (!uri.IsAbsoluteUri || !uri.AbsolutePath.StartsWith(SystemContext.ApplicationPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            relativePath = uri.AbsolutePath.Substring(SystemContext.ApplicationPath.Length);
            
            return true;
        }
    }
}
