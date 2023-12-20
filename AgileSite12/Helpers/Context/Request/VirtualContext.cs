using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;

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
        /// Name of the culture code parameter
        /// </summary>
        public const string PARAM_CULTURE = "culture";


        /// <summary>
        /// Name of the preview link parameter
        /// </summary>
        public const string PARAM_PREVIEW_LINK = "pv";


        /// <summary>
        /// Name of the parameter which indicates that data in the <see cref="VirtualContext"/> URL belong to Form builder.
        /// </summary>
        public const string PARAM_FORM_BUILDER_URL = "fb";


        /// <summary>
        /// Name of the parameter which indicates form builder preview link expiration.
        /// </summary>
        public const string PARAM_FORM_BUILDER_EXPIRATION = "ts";


        /// <summary>
        /// Name of the parameter which indicates if page is embedded in administration inside an iframe.
        /// </summary>
        public const string PARAM_EMBEDED_IN_ADMINISTRATION = "ea";


        /// <summary>
        /// Name of the hash parameter
        /// </summary>
        public const string PARAM_HASH = "h";


        internal const string PARAM_READONLY_MODE = "readonly";


        /// <summary>
        /// Query string parameter that holds administration url (scheme + domain + appPath)
        /// </summary>
        public const string ADMINISTRATION_DOMAIN_PARAMETER = "administrationUrl";

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


        /// <summary>
        /// Represents period for which the form builder preview link is valid.
        /// </summary>
        internal static readonly TimeSpan FormBuilderLinkExpiration = TimeSpan.FromHours(6);

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
            set => mVirtualContextPrefix = value;
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
            set => mVirtualContextSeparator = value;
        }


        /// <summary>
        /// Current prefix for the URL in the virtual context
        /// </summary>
        public static string CurrentURLPrefix
        {
            get => (string)GetItem("CurrentURLPrefix");
            set => SetItem("CurrentURLPrefix", value);
        }


        /// <summary>
        /// Returns true if some virtual context properties have been initialized
        /// </summary>
        public static bool IsInitialized => !String.IsNullOrEmpty(CurrentURLPrefix);


        /// <summary>
        /// Returns true if preview link virtual context properties have been initialized
        /// </summary>
        public static bool IsPreviewLinkInitialized => ItemIsSet(PARAM_PREVIEW_LINK);


        /// <summary>
        /// Returns true if form builder virtual context properties have been initialized.
        /// </summary>
        public static bool IsFormBuilderLinkInitialized => ItemIsSet(PARAM_FORM_BUILDER_URL);


        /// <summary>
        /// Returns true if virtual context is initialized in readonly mode only
        /// </summary>
        public static bool ReadonlyMode => IsPreviewLinkInitialized && ValidationHelper.GetBoolean(GetItem(PARAM_READONLY_MODE), true);


        /// <summary>
        /// Current items
        /// </summary>
        private static ItemsDictionary CurrentItems
        {
            get
            {
                // Get the current items
                ItemsDictionary items = (ItemsDictionary)RequestStockHelper.GetItem("VirtualContext", true);
                if (items == null)
                {
                    // Ensure the items
                    items = new ItemsDictionary();
                    RequestStockHelper.Add("VirtualContext", items, true);
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
            var items = (ItemsDictionary)RequestStockHelper.GetItem("VirtualContext", true);

            // Try to get the item
            return items?[key];
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
            RequestStockHelper.Add("VirtualContext", (object)null, true);
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

            // Append the values
            foreach (string name in values.Keys)
            {
                string value = values[name];

                sb.Append(name, "/", value, "/");
            }

            // Add the hash
            string hashString = ValidationHelper.GetHashString(sb.ToString(), new HashSettings(""));

            sb.Append(PARAM_HASH + "/", hashString, "/");

            // Append the separator
            sb.Append(VirtualContextSeparator);

            return sb.ToString();
        }


        /// <summary>
        /// Calculates hash for the given <paramref name="path"/> and appends it to the URL.
        /// </summary>
        /// <param name="path">Relative path of the document to get hash for.</param>
        /// <returns>URL passed via parameter <paramref name="path"/> appended with its calculated hash as a query parameter named "uh".</returns>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="path"/> is null or empty.</exception>
        public static string AddPathHash(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            var basePath = path.StartsWith("~", StringComparison.Ordinal) ? path.TrimStart('~') : URLHelper.RemoveApplicationPath(path);
            basePath = URLHelper.RemoveQuery(basePath).ToLowerCSafe();

            return URLHelper.AddParameterToUrl(path, "uh", ValidationHelper.GetHashString(basePath, new HashSettings("")));
        }


        /// <summary>
        /// Validates the hash of the relative path of the document.
        /// </summary>
        /// <param name="path">Relative path of the document to get hash for.</param>
        /// <returns>True if validation succeeded, otherwise false.</returns>
        public static bool ValidatePathHash(string path)
        {
            string hash = QueryHelper.GetString("uh", null);
            if (!String.IsNullOrEmpty(hash))
            {
                path = URLHelper.RemoveParameterFromUrl(path, "uh");
                string compareHash = ValidationHelper.GetHashString(path.TrimStart('~').ToLowerCSafe(), new HashSettings(""));
                return (hash == compareHash);
            }

            return false;
        }


        /// <summary>
        /// Loads and stores the virtual context values for the request.
        /// </summary>
        /// <param name="relativePath">Relative path. If loading succeeded, returns updated virtual path without the context values.</param>
        /// <returns>True, if loading of the virtual context succeeded and the request is a virtual context request, otherwise false.</returns>
        /// <exception cref="InvalidVirtualContextException">When post back under preview link is made -or- link is invalid.</exception>
        public static bool HandleVirtualContext(ref string relativePath)
        {
            // Handle the virtual context
            var values = LoadVirtualContextValues(ref relativePath);

            var isVirtual = values != null;
            if (isVirtual)
            {
                StoreVirtualContextValues(values);

                // Get excluded status for the altered relative path because it no more contains a virtual context prefix
                RequestContext.CurrentExcludedStatus = URLHelper.IsExcludedSystemEnum(relativePath);
            }

            return isVirtual;
        }


        /// <summary>
        /// Loads the virtual context values from the URL into a <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="relativePath">Current relative path. If loading succeeded, returns updated virtual path without the context values.</param>
        /// <returns>Object containing parsed values, if loading of the virtual context succeeded and the request is a virtual context request, otherwise null.</returns>
        /// <exception cref="InvalidVirtualContextException">When post back under preview link is made -or- link is invalid.</exception>
        public static NameValueCollection LoadVirtualContextValues(ref string relativePath)
        {
            string prefix = VirtualContextPrefix;

            // Try to find virtual prefix in standard URL
            if (relativePath.StartsWithCSafe(prefix))
            {
                // Try to find virtual prefix in original URL (when using the rewriting module)
                string originalRelativePath = URLHelper.RemoveApplicationPath(CMSHttpContext.Current.Request.ServerVariables["HTTP_X_ORIGINAL_URL"]);
                if ((originalRelativePath != null) && originalRelativePath.StartsWithCSafe(prefix))
                {
                    relativePath = URLHelper.RemoveQuery(originalRelativePath);
                }

                // Process the virtual prefix
                int nameStart = prefix.Length;
                int nameEnd = relativePath.IndexOf('/', nameStart);

                bool valid = false;
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
                        case PARAM_PREVIEW_LINK:
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

                    // Validate the hash
                    string hashedPrefix = relativePath.Substring(0, hashStart);
                    string hash = values[PARAM_HASH];

                    var settings = new HashSettings("")
                    {
                        Redirect = false
                    };

                    var contextValid = ValidationHelper.ValidateHash(hashedPrefix, hash, settings);
                    if (contextValid)
                    {
                        var userName = values.Get(PARAM_PREVIEW_LINK);
                        if (userName == null)
                        {
                            userName = values.Get(PARAM_USERNAME);
                        }
                        else if (values.Get(PARAM_USERNAME) != null)
                        {
                            throw new InvalidVirtualContextException(CoreServices.Localization.GetString("virtualcontext.accessdenied"));
                        }

                        // User name must be present in the virtual context URL
                        var user = Service.Resolve<IAuthenticationService>().GetUser(userName);
                        contextValid = user != null && user.Enabled;
                    }

                    // Postback is not allowed under preview link mode
                    if (previewLink && RequestHelper.IsPostBack() && IsReadonlyMode(values))
                    {
                        throw new InvalidVirtualContextException(CoreServices.Localization.GetString("virtualcontext.accessdenied"));
                    }

                    // Context is not valid
                    if (!contextValid)
                    {
                        string resStringKey = previewLink ? "virtualcontext.previewlink" : "virtualcontext.accessdenied";
                        throw new InvalidVirtualContextException(CoreServices.Localization.GetString(resStringKey));
                    }

                    // Finalize the URL
                    CurrentURLPrefix = currentPrefix;

                    // Modify the current relative path
                    relativePath = relativePath.Substring(currentPrefix.Length);

                    return values;
                }

                // Reset the virtual context
                Reset();
            }

            return null;
        }


        /// <summary>
        /// Stores key-value pairs into virtual context.
        /// </summary>
        /// <param name="nameValueCollection">Key-value pairs to store.</param>
        public static void StoreVirtualContextValues(NameValueCollection nameValueCollection)
        {
            foreach (string name in nameValueCollection.Keys)
            {
                string value = nameValueCollection[name];
                SetItem(name, value);
            }
        }


        private static bool IsReadonlyMode(NameValueCollection values)
        {
            return ValidationHelper.GetBoolean(values.Get(PARAM_READONLY_MODE), true);
        }


        /// <summary>
        /// Gets preview path based on given path and current initialized context.
        /// </summary>
        /// <param name="path">Application absolute path to be converted.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        /// <param name="cultureCode">Defines a culture code for resulting preview path. If not provided, culture code from current initialized context is used.</param>
        /// <param name="embededInAdministration">Indicates if page is embedded in administration inside an iframe.</param>
        public static string GetPreviewPathFromVirtualContext(string path, bool readonlyMode = true, string cultureCode = null, bool embededInAdministration = false)
        {
            if (!IsPreviewLinkInitialized)
            {
                return path;
            }

            path = AddPathHash(path);

            var userName = GetItem(PARAM_PREVIEW_LINK) as string;
            var linkCultureCode = cultureCode ?? GetItem(PARAM_CULTURE) as string;
            var workflowCycleGuid = ValidationHelper.GetGuid(GetItem(PARAM_WF_GUID), Guid.Empty);
            var param = GetPreviewParameters(userName, linkCultureCode, workflowCycleGuid, readonlyMode, embededInAdministration);
            var virtualContextPath = GetVirtualContextPath(path, param);

            return embededInAdministration ? AddAdministrationDomain(virtualContextPath) : virtualContextPath;
        }


        /// <summary>
        /// Gets the collection of parameters for preview path.
        /// </summary>
        /// <param name="userName">Context of user for the preview.</param>
        /// <param name="cultureCode">Context of culture for the preview.</param>
        /// <param name="workflowCycleGuid">Context of workflow cycle GUID.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        /// <param name="embededInAdministration">Indicates if page is embedded in administration inside an iframe.</param>
        public static NameValueCollection GetPreviewParameters(string userName, string cultureCode, Guid workflowCycleGuid, bool readonlyMode, bool embededInAdministration)
        {
            var param = new NameValueCollection
            {
                { PARAM_PREVIEW_LINK, userName },
                { PARAM_CULTURE, cultureCode },
                { PARAM_WF_GUID, workflowCycleGuid.ToString() }
            };

            if (!readonlyMode)
            {
                param.Add(PARAM_READONLY_MODE, "0");
            }

            if (embededInAdministration)
            {
                param.Add(PARAM_EMBEDED_IN_ADMINISTRATION, "1");
            }

            return param;
        }


        /// <summary>
        /// Returns path containing <see cref="VirtualContext"/> data required by Form builder.
        /// </summary>
        /// <param name="path">Application absolute path to be converted.</param>
        /// <param name="userName">
        /// User under whom the request for returned path is processed.
        /// If parameter is not set then user name stored in <see cref="VirtualContext"/> is used.
        /// </param>
        public static string GetFormBuilderPath(string path, string userName = null)
        {
            path = AddPathHash(path);

            var param = new NameValueCollection
            {
                { PARAM_FORM_BUILDER_URL,  "1" },
                { PARAM_USERNAME, userName ?? GetItem(PARAM_USERNAME) as string },
                { PARAM_FORM_BUILDER_EXPIRATION, GetFormBuilderLinkExpiration(Service.Resolve<IDateTimeNowService>().GetDateTimeNow()) },
                { PARAM_EMBEDED_IN_ADMINISTRATION, "1" }
            };

            var virtualContextPath = GetVirtualContextPath(path, param);

            return AddAdministrationDomain(virtualContextPath);
        }


        /// <summary>
        /// Searches for <see cref="ADMINISTRATION_DOMAIN_PARAMETER"/> parameter in current HTTP request query string
        /// and adds it into the path (if the parameter was found).
        /// </summary>
        /// <param name="path">Application path.</param>
        /// <returns>Application path with administration domain (when applicable).</returns>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        /// <exclude />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string AddAdministrationDomain(string path)
        {
            var administrationDomain = QueryHelper.GetString(ADMINISTRATION_DOMAIN_PARAMETER, string.Empty);

            return string.IsNullOrEmpty(administrationDomain)
                ? path
                : URLHelper.AddParameterToUrl(path, ADMINISTRATION_DOMAIN_PARAMETER, Uri.EscapeDataString(administrationDomain));
        }


        /// <summary>
        /// Returns given <paramref name="dateTime"/> represented in UTC time as number of ticks.
        /// </summary>
        private static string GetFormBuilderLinkExpiration(DateTime dateTime)
        {
            return dateTime.Add(FormBuilderLinkExpiration).ToUniversalTime().Ticks.ToString();
        }

        #endregion
    }
}