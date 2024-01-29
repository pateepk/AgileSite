using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebServices
{
    /// <summary>
    /// Provides utility methods for REST.
    /// </summary>
    public class RESTServiceHelper : AbstractHelper<RESTServiceHelper>
    {
        /// <summary>
        /// Returns hash string to be used for given URL path and query instead of authentication.
        /// </summary>
        /// <param name="pathAndQuery">URL absolute path and query for which to get the hash (resolved virtual path and query string originating from <see cref="BaseRESTService.RewriteRESTUrl"/>).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathAndQuery"/> is null.</exception>
        /// <remarks>
        /// The produced hash is invariant of the <paramref name="pathAndQuery"/> trailing slash and optional query string parameters
        /// <c>offset</c> and <c>maxrecords</c>.
        /// </remarks>
        public static string GetUrlPathAndQueryHash(string pathAndQuery)
        {
            return HelperObject.GetUrlPathAndQueryHashInternal(pathAndQuery);
        }


        /// <summary>
        /// Tests whether hash computed for <paramref name="pathAndQuery"/> matches <paramref name="expectedHash"/>.
        /// </summary>
        /// <param name="pathAndQuery">URL absolute path and query for which to compute the hash (resolved virtual path and query string originating from <see cref="BaseRESTService.RewriteRESTUrl"/>
        /// or <see cref="Uri.PathAndQuery"/> of the request being validated).</param>
        /// <param name="expectedHash">Hash to perform the computed hash matching against.</param>
        /// <returns>Return true if <paramref name="pathAndQuery"/> produces the same hash as <paramref name="expectedHash"/> prescribes. Otherwise returns false.</returns>
        /// <remarks>
        /// The produced hash is invariant of the <paramref name="pathAndQuery"/> trailing slash and optional query string parameters
        /// <c>offset</c> and <c>maxrecords</c>.
        /// </remarks>
        /// <seealso cref="GetUrlPathAndQueryHash"/>
        public static bool IsUrlPathAndQueryHashValid(string pathAndQuery, string expectedHash)
        {
            return HelperObject.IsUrlPathAndQueryHashValidInternal(pathAndQuery, expectedHash);
        }


        /// <summary>
        /// Tries to parse an absolute URL of a REST service endpoint and splits the absolute path to absolute path prefix and relative REST path.
        /// </summary>
        /// <param name="absoluteUrl">Absolute URL identifying REST service endpoint.</param>
        /// <param name="absolutePathPrefix">When this method returns, contains absolute path prefix of the absolute path (the path portion preceding '/rest'). Contains null if parsing fails.</param>
        /// <param name="relativeRestPath">When this method returns, contains relative path within the REST endpoint (e.g. '/rest/all'). Contains null if parsing fails.</param>
        /// <returns>Returns true upon successful parsing, otherwise false.</returns>
        /// <example>
        /// Parses a URL in format <c>http://example.com/web/virtual/path/rest/om.activitytype?format=xml</c> to <c>/web/virtual/path</c> prefix and <c>/rest/om.activitytype</c> REST path.
        /// </example>
        public static bool TryParseRestUrlPath(string absoluteUrl, out string absolutePathPrefix, out string relativeRestPath)
        {
            return HelperObject.TryParseRestUrlPathInternal(absoluteUrl, out absolutePathPrefix, out relativeRestPath);
        }


        /// <summary>
        /// Returns hash string to be used for given URL path and query instead of authentication.
        /// </summary>
        /// <param name="pathAndQuery">URL absolute path and query for which to get the hash (resolved virtual path and query string originating from <see cref="BaseRESTService.RewriteRESTUrl"/>).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathAndQuery"/> is null.</exception>
        /// <remarks>
        /// The produced hash is invariant of the <paramref name="pathAndQuery"/> trailing slash and optional query string parameters
        /// <c>offset</c> and <c>maxrecords</c>.
        /// </remarks>
        protected virtual string GetUrlPathAndQueryHashInternal(string pathAndQuery)
        {
            if (pathAndQuery == null)
            {
                throw new ArgumentNullException(nameof(pathAndQuery));
            }
            
            // Remove paging parameters from hash
            pathAndQuery = URLHelper.RemoveParameterFromUrl(pathAndQuery, "offset");
            pathAndQuery = URLHelper.RemoveParameterFromUrl(pathAndQuery, "maxrecords");
            pathAndQuery = NormalizeUrlPathTrailingSlash(pathAndQuery);

            return ValidationHelper.GetHashString(pathAndQuery, new HashSettings());
        }


        /// <summary>
        /// Normalizes URL by removing trailing slash from the end of the URL path. The URL must be normalized in order to be compatible with the way the
        /// <see cref="System.ServiceModel.OperationContext.IncomingMessageHeaders"/> represents path. 
        /// </summary>
        /// <param name="pathAndQuery">URL path and query to normalize.</param>
        /// <returns>URL which does not contain a trailing slash.</returns>
        /// <remarks>
        /// Normalizes URL paths like <c>/RESTService.svc/cms.query/?domain=mydomain</c> to <c>/RESTService.svc/cms.query?domain=mydomain</c> by trimming the trailing path slash.
        /// </remarks>
        private static string NormalizeUrlPathTrailingSlash(string pathAndQuery)
        {
            var path = URLHelper.RemoveQuery(pathAndQuery);
            if (path.EndsWith("/", StringComparison.Ordinal))
            {
                var query = URLHelper.GetQuery(pathAndQuery);

                return path.TrimEnd('/') + query;
            }

            return pathAndQuery;
        }


        /// <summary>
        /// Tests whether hash computed for <paramref name="pathAndQuery"/> matches <paramref name="expectedHash"/>.
        /// </summary>
        /// <param name="pathAndQuery">URL absolute path and query for which to compute the hash (resolved virtual path and query string originating from <see cref="BaseRESTService.RewriteRESTUrl"/>
        /// or <see cref="Uri.PathAndQuery"/> of the request being validated).</param>
        /// <param name="expectedHash">Hash to perform the computed hash matching against.</param>
        /// <returns>Return true if <paramref name="pathAndQuery"/> produces the same hash as <paramref name="expectedHash"/> prescribes. Otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathAndQuery"/> or <paramref name="expectedHash"/> is null.</exception>
        /// <remarks>
        /// The produced hash is invariant of the <paramref name="pathAndQuery"/> trailing slash and optional query string parameters
        /// <c>offset</c> and <c>maxrecords</c>.
        /// </remarks>
        /// <seealso cref="GetUrlPathAndQueryHash"/>
        protected virtual bool IsUrlPathAndQueryHashValidInternal(string pathAndQuery, string expectedHash)
        {
            if (expectedHash == null)
            {
                throw new ArgumentNullException(nameof(expectedHash));
            }

            return GetUrlPathAndQueryHash(pathAndQuery).Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Tries to parse an absolute URL of a REST service endpoint and splits the absolute path to absolute path prefix and relative REST path.
        /// </summary>
        /// <param name="absoluteUrl">Absolute URL identifying REST service endpoint.</param>
        /// <param name="absolutePathPrefix">When this method returns, contains absolute path prefix of the absolute path (the path portion preceding '/rest'). Contains null if parsing fails.</param>
        /// <param name="relativeRestPath">When this method returns, contains relative path within the REST endpoint (e.g. '/rest/all'). Contains null if parsing fails.</param>
        /// <returns>Returns true upon successful parsing, otherwise false.</returns>
        /// <example>
        /// Parses a URL in format <c>http://example.com/web/virtual/path/rest/om.activitytype?format=xml</c> to <c>/web/virtual/path</c> prefix and <c>/rest/om.activitytype</c> REST path.
        /// </example>
        protected virtual bool TryParseRestUrlPathInternal(string absoluteUrl, out string absolutePathPrefix, out string relativeRestPath)
        {
            Uri uri;
            if (Uri.TryCreate(absoluteUrl, UriKind.Absolute, out uri))
            {
                string absolutePath = uri.AbsolutePath;
                int relateiveRestPathStartIndex = absolutePath.IndexOf("/rest", StringComparison.OrdinalIgnoreCase);
                if (relateiveRestPathStartIndex >= 0)
                {
                    absolutePathPrefix = absolutePath.Substring(0, relateiveRestPathStartIndex);
                    relativeRestPath = absolutePath.Substring(relateiveRestPathStartIndex);

                    return true;
                }
            }

            absolutePathPrefix = null;
            relativeRestPath = null;

            return false;
        }
    }
}
