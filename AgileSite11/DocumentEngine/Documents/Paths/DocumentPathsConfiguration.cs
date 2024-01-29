using System;
using System.Text;
using System.Linq;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document paths configuration
    /// </summary>
    internal sealed class DocumentPathsConfiguration
    {
        /// <summary>
        /// Name path
        /// </summary>
        public readonly string NamePath;


        /// <summary>
        /// URL path
        /// </summary>
        private readonly string UrlPath;


        /// <summary>
        /// Indicates if name path should be used for URL path
        /// </summary>
        private readonly bool UseNamePathForUrlPath;


        /// <summary>
        /// Creates an instance of <see cref="DocumentPathsConfiguration"/>
        /// </summary>
        /// <param name="useNamePathForUrlPath">Indicates if name path should be used as a source for URL path</param>
        /// <param name="namePath">Document name path</param>
        /// <param name="urlPath">Document URL path</param>
        public DocumentPathsConfiguration(bool useNamePathForUrlPath, string namePath, string urlPath)
        {
            UseNamePathForUrlPath = useNamePathForUrlPath;
            NamePath = namePath;
            UrlPath = urlPath;
        }


        /// <summary>
        /// Gets URL path based on document configuration.
        /// </summary>
        /// <param name="siteName">Document site</param>
        public string GetUrlPath(string siteName)
        {
            return UseNamePathForUrlPath ? UrlPath : GetUrlPathFromNamePath(NamePath, siteName);
        }


        private string GetUrlPathFromNamePath(string namePath, string siteName)
        {
            // This approach is used when document uses custom URL path and that is why generating unique URL path is not required here
            var urlPath = TrimPathPartsToMaximalAllowedLength(namePath);
            return TreePathUtils.GetSafeUrlPath(urlPath, siteName);
        }


        private string TrimPathPartsToMaximalAllowedLength(string path)
        {
            if (String.IsNullOrEmpty(path) || (path == "/"))
            {
                return null;
            }

            var maximalAliasLength = TreePathUtils.MaxAliasLength;
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .Select(p => p.Length > maximalAliasLength ? p.Substring(0, maximalAliasLength) : p);

            return String.Join("/", parts);
        }
    }
}
