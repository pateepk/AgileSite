using System;
using System.Linq;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Encapsulates functionality detecting paths excluded from adding X-Frame-Options HTTP header.
    /// </summary>
    internal class XFrameOptionsExcludedPathDetector
    {
        private const string CLICKJACKINGHASH_QUERY_PARAM_NAME = "clickjackinghash";
        private const string REFRESHTOKEN_PARAM_NAME = "refreshtoken";

        private static string[] mXFrameOptionsExcluded;
        private readonly bool mRedirect;


        /// <summary>
        /// Specifies files which don't have specified X-Frame-Options HTTP header.
        /// </summary>
        private static string[] XFrameOptionsExcluded
        {
            get
            {
                if (mXFrameOptionsExcluded == null)
                {
                    string excluded = SettingsHelper.AppSettings["CMSXFrameOptionsExcluded"];
                    if (!String.IsNullOrEmpty(excluded))
                    {
                        excluded = excluded.ToLowerInvariant();
                        mXFrameOptionsExcluded = excluded.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        // Insert empty value for to indicate that mXFrameOptionsExcluded is set
                        mXFrameOptionsExcluded = new string[] { };
                    }
                }

                return mXFrameOptionsExcluded;
            }
        }


        /// <summary>
        /// Creates an instance of the <see cref="XFrameOptionsExcludedPathDetector"/> class.
        /// </summary>
        public XFrameOptionsExcludedPathDetector()
        {
            mRedirect = true;
        }


        /// <summary>
        /// Creates an instance of the <see cref="XFrameOptionsExcludedPathDetector"/> class.
        /// </summary>
        /// <param name="redirect">Indicates whether hash validation should redirect to an error page in case of invalid hash.</param>
        internal XFrameOptionsExcludedPathDetector(bool redirect) 
            : this()
        {
            mRedirect = redirect;
        }


        /// <summary>
        /// Returns whether given path is excluded from adding X-Frame-Options HTTP header.
        /// </summary>
        /// <param name="path">Rewritten path or path prefix starting with /.</param>
        /// <param name="url">Original URL including query string.</param>
        public bool IsExcluded(string path, Uri url)
        {
            if (path == null)
            {
                return false;
            }

            return IsExcludedByPath(path) || IsExcludedByClickjackingHash(url);
        }


        private bool IsExcludedByClickjackingHash(Uri url)
        {
            // Special handling for preview pages
            var clickjackingHash = URLHelper.GetQueryValue(url.Query, CLICKJACKINGHASH_QUERY_PARAM_NAME);
            if (String.IsNullOrEmpty(clickjackingHash))
            {
                return false;
            }

            var queryStringStaticPart = URLHelper.RemoveUrlParameters(url.Query, CLICKJACKINGHASH_QUERY_PARAM_NAME, REFRESHTOKEN_PARAM_NAME);

            var hashSettings = new HashSettings(RequestContext.UserName)
            {
                Redirect = mRedirect
            };

            return ValidationHelper.ValidateHash(queryStringStaticPart, clickjackingHash, hashSettings);
        }


        private static bool IsExcludedByPath(string path)
        {
            return XFrameOptionsExcluded.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
        }
    }
}
