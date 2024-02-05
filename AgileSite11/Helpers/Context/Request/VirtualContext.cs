using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

using CMS.Base;

namespace CMS.Helpers
{
    using ItemsDictionary = StringSafeDictionary<object>;
    
    /// <summary>
    /// Stores the virtual context for the current request
    /// </summary>
    public static class VirtualContext
    {
        #region "Constants"

        /// <summary>
        /// Name of the workflow cycle GUID parameter
        /// </summary>
        public const string PARAM_WF_GUID = "wg";


        /// <summary>
        /// Name of the user name parameter
        /// </summary>
        public const string PARAM_USERNAME = "u";


        /// <summary>
        /// Name of the view mode parameter
        /// </summary>
        public const string PARAM_VIEWMODE = "viewmode";


        /// <summary>
        /// Name of the site name parameter
        /// </summary>
        public const string PARAM_SITENAME = "sitename";


        /// <summary>
        /// Name of the preview link parameter
        /// </summary>
        public const string PARAM_PREVIEW_LINK = "pv";


        /// <summary>
        /// Name of the hash parameter
        /// </summary>
        public const string PARAM_HASH = "h";

        #endregion


        #region "Variables"

        /// <summary>
        /// Default URL prefix for the pages virtual context
        /// </summary>
        private static string mVirtualContextPrefix;


        /// <summary>
        /// Default URL separator for the prefix and original URL for pages with virtual context. Default value is "-"
        /// </summary>
        private static string mVirtualContextSeparator;

        #endregion


        #region "Properties"

        /// <summary>
        /// Default URL prefix for the pages virtual context. Default value is "/cmsctx/"
        /// </summary>
        public static string VirtualContextPrefix
        {
            get
            {
                if (mVirtualContextPrefix == null)
                {
                    mVirtualContextPrefix = String.Format("/{0}/", ValidationHelper.GetString(SettingsHelper.AppSettings["CMSVirtualContextPrefix"], "cmsctx").Trim('/'));
                }
                return mVirtualContextPrefix;
            }
            set
            {
                mVirtualContextPrefix = value;
            }
        }


        /// <summary>
        /// Default URL separator for the prefix and original URL for pages with virtual context. Default value is "-"
        /// </summary>
        public static string VirtualContextSeparator
        {
            get
            {
                if (mVirtualContextSeparator == null)
                {
                    mVirtualContextSeparator = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSVirtualContextSeparator"], "-").Trim('/');
                }
                return mVirtualContextSeparator;
            }
            set
            {
                mVirtualContextSeparator = value;
            }
        }


        /// <summary>
        /// Current prefix for the URL in the virtual context
        /// </summary>
        public static string CurrentURLPrefix
        {
            get
            {
                return (string)GetItem("CurrentURLPrefix");
            }
            set
            {
                SetItem("CurrentURLPrefix", value);
            }
        }


        /// <summary>
        /// Returns true if some virtual context properties have been initialized
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return !String.IsNullOrEmpty(CurrentURLPrefix);
            }
        }


        /// <summary>
        /// Returns true if user virtual context properties have been initialized
        /// </summary>
        public static bool IsUserInitialized
        {
            get
            {
                return ItemIsSet(PARAM_USERNAME);
            }
        }


        /// <summary>
        /// Returns true if preview link virtual context properties have been initialized
        /// </summary>
        public static bool IsPreviewLinkInitialized
        {
            get
            {
                return ItemIsSet(PARAM_PREVIEW_LINK);
            }
        }


        /// <summary>
        /// Current items
        /// </summary>
        private static ItemsDictionary CurrentItems
        {
            get
            {
                // Get the current items
                ItemsDictionary items = (ItemsDictionary)AbstractStockHelper<RequestStockHelper>.GetItem("VirtualContext", true);
                if (items == null)
                {
                    // Ensure the items
                    items = new ItemsDictionary();
                    AbstractStockHelper<RequestStockHelper>.Add("VirtualContext", items, true);
                }

                return items;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true, if the given item is set
        /// </summary>
        /// <param name="key">Key to check</param>
        public static bool ItemIsSet(string key)
        {
            return (GetItem(key) != null);
        }


        /// <summary>
        /// Gets the particular item from virtual context
        /// </summary>
        /// <param name="key">Key to get </param>
        public static object GetItem(string key)
        {
            // Get the items collection
            var items = (ItemsDictionary)AbstractStockHelper<RequestStockHelper>.GetItem("VirtualContext", true);
            if (items == null)
            {
                return null;
            }

            // Try to get the item
            return items[key];
        }


        /// <summary>
        /// Sets the particular item in the virtual context
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="value">New value</param>
        public static void SetItem(string key, object value)
        {
            // Special treatment for preview link
            if (key.ToLowerCSafe() == PARAM_PREVIEW_LINK)
            {
                // Preview mode is not available for public user
                var isPublicUser = CMSString.Equals((string)value, "public", true);

                string viewMode = isPublicUser ? "livesite" : "preview";

                CurrentItems[PARAM_VIEWMODE] = viewMode;
                CurrentItems[PARAM_USERNAME] = value;
            }

            CurrentItems[key] = value;
        }


        /// <summary>
        /// Resets the current virtual context
        /// </summary>
        public static void Reset()
        {
            AbstractStockHelper<RequestStockHelper>.Add("VirtualContext", (object)null, true);
            CurrentURLPrefix = null;
        }


        /// <summary>
        /// Removes the virtual context prefix from the given URL
        /// </summary>
        /// <param name="path">Path to modify</param>
        public static string RemoveVirtualContextPrefix(string path)
        {
            return ReplaceVirtualContextPrefix(path, null);
        }


        /// <summary>
        /// Replaces the virtual context prefix from the given URL with a new one
        /// </summary>
        /// <param name="path">Path to modify</param>
        /// <param name="newPrefix">New prefix</param>
        public static string ReplaceVirtualContextPrefix(string path, string newPrefix)
        {
            string prefix = null;
            string result = path;

            // Handle the ~ prefix
            if (path.StartsWithCSafe("~/"))
            {
                prefix = "~";
                result = result.Substring(1);
            }
            else
            {
                // Handle the application path
                string appPath = SystemContext.ApplicationPath;
                if (path.StartsWithCSafe(appPath, true))
                {
                    prefix = appPath.TrimEnd('/');
                    result = result.Substring(prefix.Length);
                }
            }

            if (result.StartsWithCSafe(VirtualContextPrefix, true))
            {
                // Find the separator
                int separatorIndex = result.IndexOfCSafe(String.Format("/{0}/", VirtualContextSeparator));
                if (separatorIndex >= 0)
                {
                    // Join the URL back
                    result = result.Substring(separatorIndex + 1 + VirtualContextSeparator.Length);
                }
            }

            return prefix + newPrefix + result;
        }


        /// <summary>
        /// Returns true if the URL contains virtual context prefix
        /// </summary>
        /// <param name="url">URL to check</param>
        public static bool ContainsVirtualContextPrefix(string url)
        {
            return (url.IndexOfCSafe(VirtualContextPrefix) >= 0) && (url.IndexOfCSafe(String.Format("/{0}/", VirtualContextSeparator)) >= 0);
        }


        /// <summary>
        /// Removes the virtual context prefix from the given URL
        /// </summary>
        /// <param name="path">Path to modify</param>
        /// <param name="values">Collection of the context parameters</param>
        public static string GetVirtualContextPath(string path, NameValueCollection values)
        {
            string newPrefix = GetVirtualContextPrefix(values);

            path = ReplaceVirtualContextPrefix(path, newPrefix);

            return path;
        }


        /// <summary>
        /// Gets the prefix for the virtual context URL
        /// </summary>
        /// <param name="values">Collection of the context parameters</param>
        public static string GetVirtualContextPrefix(NameValueCollection values)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(VirtualContextPrefix);

            bool hash = false;

            // Append the values
            foreach (string name in values.Keys)
            {
                string value = values[name];

                sb.Append(name, "/", value, "/");

                // Detect if hash is needed
                switch (name.ToLowerCSafe())
                {
                    case PARAM_USERNAME:
                    case PARAM_VIEWMODE:
                    case PARAM_PREVIEW_LINK:
                    case PARAM_SITENAME:
                        hash = true;
                        break;
                }
            }

            if (hash)
            {
                // Add the hash
                string hashString = ValidationHelper.GetHashString(sb.ToString(), new HashSettings());

                sb.Append(PARAM_HASH + "/", hashString, "/");
            }

            // Append the separator
            sb.Append(VirtualContextSeparator);

            return sb.ToString();
        }


        /// <summary>
        /// Gets the custom prefix hash for the virtual context URL validation
        /// </summary>
        /// <param name="path">Relative path of the document to get hash for</param>
        public static string AddPreviewHash(string path)
        {
            var basePath = path.StartsWith("~", StringComparison.Ordinal) ? path.TrimStart('~') : URLHelper.RemoveApplicationPath(path);
            basePath = URLHelper.RemoveQuery(basePath).ToLowerCSafe();

            return URLHelper.AddParameterToUrl(path, "uh", ValidationHelper.GetHashString(basePath, new HashSettings()));
        }


        /// <summary>
        /// Gets the additional custom prefix parameters part for the virtual context URL
        /// </summary>
        /// <param name="path">Relative path of the document to get hash for</param>
        public static bool ValidatePreviewHash(string path)
        {
            string hash = QueryHelper.GetString("uh", null);
            if (!String.IsNullOrEmpty(hash))
            {
                path = URLHelper.RemoveParameterFromUrl(path, "uh");
                string compareHash = ValidationHelper.GetHashString(path.TrimStart('~').ToLowerCSafe(), new HashSettings());
                return (hash == compareHash);
            }

            return false;
        }


        /// <summary>
        /// Handles the virtual context for the request.
        /// </summary>
        /// <param name="relativePath">Relative path. If loading succeeded, returns updated virtual path without the context values.</param>
        /// <returns>True, if loading of the virtual context succeeded and the request is a virtual context request, otherwise false.</returns>
        /// <exception cref="InvalidVirtualContextException">When post back under preview link is made -or- link is invalid.</exception>
        public static bool HandleVirtualContext(ref string relativePath)
        {
            // Handle the virtual context
            bool isVirtual = LoadVirtualContextValues(ref relativePath);
            if (isVirtual)
            {
                // Get excluded status for the altered relative path because it no more contains a virtual context prefix
                RequestContext.CurrentExcludedStatus = URLHelper.IsExcludedSystemEnum(relativePath);
            }

            return isVirtual;
        }


        /// <summary>
        /// Handles the virtual context prefixes in the URL.
        /// </summary>
        /// <param name="relativePath">Current relative path. If loading succeeded, returns updated virtual path without the context values.</param>
        /// <returns>True, if loading of the virtual context succeeded and the request is a virtual context request, otherwise false.</returns>
        /// <exception cref="InvalidVirtualContextException">When post back under preview link is made -or- link is invalid.</exception>
        public static bool LoadVirtualContextValues(ref string relativePath)
        {
            string prefix = VirtualContextPrefix;

            // Try to find virtual prefix in standard URL
            if (relativePath.StartsWithCSafe(prefix))
            {
                // Try to find virtual prefix in original URL (when using the rewriting module)
                string originalRelativePath = URLHelper.RemoveApplicationPath(HttpContext.Current.Request.ServerVariables["HTTP_X_ORIGINAL_URL"]);
                if ((originalRelativePath != null) && originalRelativePath.StartsWithCSafe(prefix))
                {
                    relativePath = URLHelper.RemoveQuery(originalRelativePath);
                }

                // Process the virtual prefix
                int nameStart = prefix.Length;
                int nameEnd = relativePath.IndexOf('/', nameStart);

                bool valid = false;
                bool validateHash = false;
                bool previewLink = false;
                int hashStart = -1;

                // Collect the values from the URL
                var values = new NameValueCollection();

                while (nameEnd > nameStart)
                {
                    // Get the next parameter name
                    string name = relativePath.Substring(nameStart, nameEnd - nameStart);
                    if (name == "-")
                    {
                        valid = true;
                        break;
                    }

                    int valueStart = nameEnd + 1;
                    int valueEnd = relativePath.IndexOf('/', valueStart);

                    if (valueEnd < valueStart)
                    {
                        break;
                    }

                    // Get the value
                    string value = relativePath.Substring(valueStart, valueEnd - valueStart);

                    // Handle the hash validation
                    switch (name.ToLowerCSafe())
                    {
                        case PARAM_USERNAME:
                        case PARAM_VIEWMODE:
                        case PARAM_SITENAME:
                            // These items need to validate the hash
                            validateHash = true;
                            break;

                        case PARAM_PREVIEW_LINK:
                            // These items need to validate the hash
                            validateHash = true;
                            previewLink = true;
                            break;

                        case PARAM_HASH:
                            // Hash is the last parameter
                            hashStart = nameStart;
                            break;
                    }

                    // Add context values to set later
                    values.Add(name, value);

                    // Move to the next parameter
                    nameStart = valueEnd + 1;
                    nameEnd = relativePath.IndexOf('/', nameStart);
                }

                if (valid)
                {
                    string currentPrefix = relativePath.Substring(0, nameStart + 1);

                    if (validateHash)
                    {
                        // Validate the hash
                        string hashedPrefix = relativePath.Substring(0, hashStart);
                        string hash = values[PARAM_HASH];

                        var settings = new HashSettings
                        {
                            Redirect = false
                        };

                        bool contextValid = ValidationHelper.ValidateHash(hashedPrefix, hash, settings);

                        // Secured parameters allowed also for global admins
                        if (!contextValid)
                        {
                            contextValid = CMSActionContext.CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
                        }

                        // Postback is not allowed under preview link mode
                        if (previewLink && RequestHelper.IsPostBack())
                        {
                            throw new InvalidVirtualContextException(ResHelper.GetString("virtualcontext.accessdenied"));
                        }

                        // Context is not valid
                        if (!contextValid)
                        {
                            string resStringKey = previewLink ? "virtualcontext.previewlink" : "virtualcontext.accessdenied";
                            throw new InvalidVirtualContextException(ResHelper.GetString(resStringKey));
                        }
                    }

                    // Copy values to virtual context
                    foreach (string name in values.Keys)
                    {
                        string value = values[name];
                        SetItem(name, value);
                    }

                    // Finalize the URL
                    CurrentURLPrefix = currentPrefix;

                    // Modify the current relative path
                    relativePath = relativePath.Substring(currentPrefix.Length);

                    return true;
                }

                // Reset the virtual context
                Reset();
            }

            return false;
        }

        #endregion
    }
}