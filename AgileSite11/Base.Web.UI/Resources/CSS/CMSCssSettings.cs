using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.Core;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Settings for the CSS
    /// </summary>
    public class CMSCssSettings
    {
        #region "Variables"

        private bool? mSingleRequest;
        private bool mEnableMinification = true;
        private bool mEnableCompression = true;

        private readonly List<string> mFiles = new List<string>();
        private readonly List<string> mStylesheets = new List<string>();

        private readonly List<string> mComponentFiles = new List<string>();
        private readonly List<int> mContainers = new List<int>();
        private readonly List<int> mLayouts = new List<int>();
        private readonly List<int> mTemplates = new List<int>();
        private readonly List<int> mWebPartLayouts = new List<int>();
        private readonly List<int> mTransformations = new List<int>();
        private readonly List<int> mDeviceLayouts = new List<int>();
        private readonly List<int> mWebParts = new List<int>();

        #endregion


        #region "Properties"

        /// <summary>
        /// List of physical stylesheet files
        /// </summary>
        public List<string> Files
        {
            get
            {
                return mFiles;
            }
        }


        /// <summary>
        /// List of component files
        /// </summary>
        public List<string> ComponentFiles
        {
            get
            {
                return mComponentFiles;
            }
        }


        /// <summary>
        /// List of stylesheet names
        /// </summary>
        public List<string> Stylesheets
        {
            get
            {
                return mStylesheets;
            }
        }


        /// <summary>
        /// List of web part container IDs
        /// </summary>
        public List<int> Containers
        {
            get
            {
                return mContainers;
            }
        }


        /// <summary>
        /// List of layouts
        /// </summary>
        public List<int> Layouts
        {
            get
            {
                return mLayouts;
            }
        }


        /// <summary>
        /// List of page templates
        /// </summary>
        public List<int> Templates
        {
            get
            {
                return mTemplates;
            }
        }


        /// <summary>
        /// List of web part layouts
        /// </summary>
        public List<int> WebPartLayouts
        {
            get
            {
                return mWebPartLayouts;
            }
        }


        /// <summary>
        /// List of web parts
        /// </summary>
        public List<int> WebParts
        {
            get
            {
                return mWebParts;
            }
        }


        /// <summary>
        /// List of transformations
        /// </summary>
        public List<int> Transformations
        {
            get
            {
                return mTransformations;
            }
        }


        /// <summary>
        /// List of device layouts
        /// </summary>
        public List<int> DeviceLayouts
        {
            get
            {
                return mDeviceLayouts;
            }
        }


        /// <summary>
        /// If true, the CSSes are processed within a single request
        /// </summary>
        public bool SingleRequest
        {
            get
            {
                if (mSingleRequest == null)
                {
                    // Load from the settings
                    mSingleRequest = CoreServices.Settings["CMSCombineComponentsCSS"].ToBoolean(false);
                }

                return mSingleRequest.Value;
            }
            set
            {
                mSingleRequest = value;
            }
        }


        /// <summary>
        /// Indicates if resource should be minificated
        /// </summary>
        public bool EnableMinification
        {
            get
            {
                return mEnableMinification;
            }
            set
            {
                mEnableMinification = value;
            }
        }

        /// <summary>
        /// Indicates if resource should be compressed
        /// </summary>
        public bool EnableCompression
        {
            get
            {
                return mEnableCompression;
            }
            set
            {
                mEnableCompression = value;
            }
        }


        /// <summary>
        /// Indicates whether stylesheet code is returned in its dynamic nature (if it uses CSS preprocessor).
        /// </summary>
        public bool ReturnAsDynamic
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether client relative URLs are resolved into absolute URLs in style sheet code.
        /// </summary>
        public bool ResolveCSSClientUrls
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CMSCssSettings()
        {
            ReturnAsDynamic = false;
            ResolveCSSClientUrls = true;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the cache dependencies for the current settings
        /// </summary>
        public List<string> GetCacheDependencies()
        {
            var keys = new List<string>();

            // General CSS dependency key
            keys.Add("css");

            keys.AddRange(GetStylesheetDependencies(Stylesheets));
            keys.AddRange(GetComponentDependencies("cms.pagetemplate|byid|{0}", Templates));
            keys.AddRange(GetComponentDependencies("cms.layout|byid|{0}", Layouts));
            keys.AddRange(GetComponentDependencies("cms.webpartcontainer|byid|{0}", Containers));
            keys.AddRange(GetComponentDependencies("cms.webpart|byid|{0}", WebParts));
            keys.AddRange(GetComponentDependencies("cms.webpartlayout|byid|{0}", WebPartLayouts));
            keys.AddRange(GetComponentDependencies("cms.transformation|byid|{0}", Transformations));
            keys.AddRange(GetComponentDependencies("cms.templatedevicelayout|byid|{0}", DeviceLayouts));

            return keys;
        }


        private IEnumerable<string> GetComponentDependencies(string cacheKeyFormat, List<int> itemIds)
        {
            return itemIds.Select(id => string.Format(cacheKeyFormat, id));
        }


        private IEnumerable<string> GetStylesheetDependencies(List<string> names)
        {
            return names.Select(name => string.Format("cms.cssstylesheet|byname|{0}", name.ToLowerInvariant()));
        }


        /// <summary>
        /// Gets the cache dependencies for the current settings
        /// </summary>
        public List<string> GetFileCacheDependencies()
        {
            var files = new List<string>();

            files.AddRange(GetFilesPhysicalPaths(Files));
            files.AddRange(GetFilesPhysicalPaths(ComponentFiles));

            return files;
        }


        private IEnumerable<string> GetFilesPhysicalPaths(List<string> filePaths)
        {
            return filePaths.Select(filePath => URLHelper.GetPhysicalPath(URLHelper.GetVirtualPath(filePath)));
        }


        /// <summary>
        /// Gets the cache dependency for the current settings
        /// </summary>
        public CMSCacheDependency GetCacheDependency()
        {
            List<string> keys = GetCacheDependencies();
            List<string> files = GetFileCacheDependencies();

            return CacheHelper.GetCacheDependency(files, keys);
        }


        /// <summary>
        /// Loads the CSS settings from the query string
        /// </summary>
        public void LoadFromQueryString()
        {
            // Stylesheets
            Files.AddRange(GetStringValuesFromQueryString("stylesheetfile"));
            Stylesheets.AddRange(GetStringValuesFromQueryString("stylesheetname"));

            // Components
            Containers.AddRange(GetIdsFromQueryString("_containers"));
            Layouts.AddRange(GetIdsFromQueryString("_layouts"));
            Transformations.AddRange(GetIdsFromQueryString("_transformations"));
            WebParts.AddRange(GetIdsFromQueryString("_webparts"));
            WebPartLayouts.AddRange(GetIdsFromQueryString("_webpartlayouts"));
            Templates.AddRange(GetIdsFromQueryString("_templates"));
            DeviceLayouts.AddRange(GetIdsFromQueryString("_devicelayouts"));

            // Additional settings
            EnableMinification = QueryHelper.GetBoolean("enableminification", true);
            EnableCompression = QueryHelper.GetBoolean("enablecompression", true);
            ReturnAsDynamic = QueryHelper.GetBoolean("dynamic", false);
            ResolveCSSClientUrls = QueryHelper.GetBoolean("resolveclienturls", true);
        }


        /// <summary>
        /// Registers the CSS files within current request
        /// </summary>
        public void RegisterCSS(Page page)
        {
            if (SingleRequest)
            {
                // Single request with all CSSes
                string url = CssLinkHelper.GetCssUrl("?" + GetCSSQueryString());
                RegisterCssLink(page, url);
            }
            else
            {
                RegisterCssLink(page, "stylesheetfile", Files);
                RegisterCssLink(page, "stylesheetname", Stylesheets);

                RegisterCssLink(page, "_containers", Containers);
                RegisterCssLink(page, "_layouts", Layouts);
                RegisterCssLink(page, "_templates", Templates);
                RegisterCssLink(page, "_transformations", Transformations);
                RegisterCssLink(page, "_webparts", WebParts);
                RegisterCssLink(page, "_webpartlayouts", WebPartLayouts);
                RegisterCssLink(page, "_devicelayouts", DeviceLayouts);
            }
        }


        private static string EnsureQueryStringToPreventClientCache(string url)
        {
            if (QueryHelper.GetBoolean("clientcache", true) || String.IsNullOrEmpty(url))
            {
                return url;
            }

            // For IE, new link is necessary to prevent cache in preview
            if (BrowserHelper.IsIE())
            {
                return url + "&guid=" + Guid.NewGuid();
            }

            // Prevent cache for preview
            return url + "&clientcache=0";
        }


        private void RegisterCssLink<T>(Page page, string key, List<T> values)
        {
            var url = CssLinkHelper.GetCssUrl("?" + GetCssQueryString(key, values));
            RegisterCssLink(page, url);
        }


        private void RegisterCssLink(Page page, string url)
        {
            url = EnsureQueryStringToPreventClientCache(url);
            CssRegistration.RegisterCssLink(page, url);
        }


        private string GetCssQueryString<T>(string key, List<T> values)
        {
            if ((values == null) || (values.Count == 0))
            {
                return null;
            }

            return string.Format("{0}={1}", key, string.Join(";", values));
        }


        /// <summary>
        /// Gets the query string for the given CSS settings
        /// </summary>        
        private string GetCSSQueryString()
        {
            var items = new List<string>
            {
                // Stylesheets
                GetCssQueryString("stylesheetfile", Files),
                GetCssQueryString("stylesheetname", Stylesheets),

                // Components
                GetCssQueryString("_containers", Containers),
                GetCssQueryString("_layouts", Layouts),
                GetCssQueryString("_templates", Templates),
                GetCssQueryString("_transformations", Transformations),
                GetCssQueryString("_webparts", WebParts),
                GetCssQueryString("_webpartlayouts", WebPartLayouts)
            };

            return string.Join("&", items.Where(x => !string.IsNullOrEmpty(x)));
        }


        private static IEnumerable<string> GetStringValuesFromQueryString(string key)
        {
            // Get the value
            string value = QueryHelper.GetString(key, null);
            if (String.IsNullOrEmpty(value))
            {
                return Enumerable.Empty<string>();
            }

            return value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }


        private static IEnumerable<int> GetIdsFromQueryString(string key)
        {
            return GetStringValuesFromQueryString(key)
                .Select(val => val.ToInteger(0))
                .Where(val => val > 0);
        }


        /// <summary>
        /// Returns true if the settings have some virtual content included
        /// </summary>
        public bool HasVirtualContent()
        {
            if ((mStylesheets != null) && (mStylesheets.Count > 0))
            {
                return true;
            }

            if ((mContainers != null) && (mContainers.Count > 0))
            {
                return true;
            }

            if ((mWebParts != null) && (mWebParts.Count > 0))
            {
                return true;
            }

            if ((mLayouts != null) && (mLayouts.Count > 0))
            {
                return true;
            }

            if ((mTemplates != null) && (mTemplates.Count > 0))
            {
                return true;
            }

            if ((mTransformations != null) && (mTransformations.Count > 0))
            {
                return true;
            }

            if ((mWebPartLayouts != null) && (mWebPartLayouts.Count > 0))
            {
                return true;
            }

            if ((mDeviceLayouts != null) && (mDeviceLayouts.Count > 0))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}