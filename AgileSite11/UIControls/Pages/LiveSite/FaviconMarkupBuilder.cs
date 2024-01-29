using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

using Directory = CMS.IO.Directory;

namespace CMS.UIControls
{
    /// <summary>
    /// Handles favicon markup creation.
    /// </summary>
    public sealed class FaviconMarkupBuilder
    {
        /// <summary>
        /// Extension of scalable files allowed to be rendered as favicons.
        /// </summary>
        private const string SCALABLE_FAVICON_EXTENSION = "svg";


        /// <summary>
        /// Name of the favicon settings key.
        /// </summary>
        private const string FAVICON_SETTING_NAME = "CMSFaviconPath";


        /// <summary>
        /// Extensions of files allowed to be rendered as favicons.
        /// </summary>
        public static readonly string[] AllowedExtensions =
        {
            "ico",
            "png",
            "gif",
            SCALABLE_FAVICON_EXTENSION
        };


        /// <summary>
        /// Current site name.
        /// </summary>
        private readonly string mSiteName;


        /// <summary>
        /// Initializes a new instance of the <see cref="FaviconMarkupBuilder"/>.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        internal FaviconMarkupBuilder(string siteName)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                throw new ArgumentException("Site name cannot be null nor empty.", "siteName");
            }

            mSiteName = siteName;
        }
        

        /// <summary>
        /// Returns markup with favicon tags.
        /// </summary>
        /// <remarks>Favicon markup is cached per site.</remarks>
        internal string GetCachedFaviconMarkup()
        {
            double cacheMinutes = CacheHelper.CacheMinutes(mSiteName);
            object[] cacheItemNameParts = { "favicon", mSiteName};

            return CacheHelper.Cache(
                setting =>
                {
                    if (setting.Cached)
                    {
                        var dependencies = GetCacheDependencyKeys();
                        setting.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                    }

                    // Get favicon settings key value
                    string faviconPath = SettingsKeyInfoProvider.GetValue(FAVICON_SETTING_NAME, mSiteName);

                    return GetFaviconTags(faviconPath);
                },
                new CacheSettings(cacheMinutes, cacheItemNameParts));
        }


        private string[] GetCacheDependencyKeys()
        {
            // Get dependencies for favicon settings key
            return new[]
            {
                String.Format("cms.settingskey|{0}", FAVICON_SETTING_NAME),
                String.Format("cms.settingskey|{1}|{0}", FAVICON_SETTING_NAME, SiteInfoProvider.GetSiteID(mSiteName))
            };
        }


        /// <summary>
        /// Returns link relation tags for both shortcut icon and icon.
        /// </summary>
        /// <param name="faviconPath">Path to favicon file or directory.</param>
        /// <remarks>If <paramref name="faviconPath"/> is empty, it returns empty string.</remarks>
        private string GetFaviconTags(string faviconPath)
        {
            if (String.IsNullOrWhiteSpace(faviconPath))
            {
                return String.Empty;
            }

            if (FileHelper.DirectoryExists(faviconPath))
            {
                var files = GetFilesInDirectory(faviconPath);
                return BuildMultipleFaviconTags(files);
            }
            return BuildFaviconTags(faviconPath);
        }

        
        /// <summary>
        /// Returns sequence of files with relative path in directory that matches the regex pattern.
        /// </summary>
        private IEnumerable<string> GetFilesInDirectory(string faviconRelativePath)
        {
            string absoluteDirPath = FileHelper.GetFullFolderPhysicalPath(faviconRelativePath);
            string allowedExtensions = String.Join("|", AllowedExtensions);
            string searchPattern = String.Format(@".*\\favicon(-[1-9][0-9]*|)\.({0})$", allowedExtensions);
            var regexPattern = RegexHelper.GetRegex(searchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return Directory.GetFiles(absoluteDirPath)
                .Where(file => regexPattern.IsMatch(file))
                .Select(file => Path.Combine(faviconRelativePath, Path.GetFileName(file)));
        }


        /// <summary>
        /// Returns multiple link tags for both shortcut icon and icon for each file in <paramref name="files"/>.
        /// </summary>
        private string BuildMultipleFaviconTags(IEnumerable<string> files)
        {
            var multipleFaviconTags = new StringBuilder();
            string searchPattern = "[1-9][0-9]*";
            var regex = RegexHelper.GetRegex(searchPattern, RegexOptions.Compiled);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                int result = regex
                    .Match(fileName)
                    .Value
                    .ToInteger(0);
                string sizes = result > 0 ? String.Format("{0}x{0}", result) : null;

                string extension = URLHelper.GetUrlExtension(file);

                // Scalable files should have sizes attribute set to any
                if (IsScalableFormat(extension))
                {
                    sizes = "any";
                }

                multipleFaviconTags.Append(BuildFaviconTags(file, sizes));
            }
            return multipleFaviconTags.ToString();
        }


        /// <summary>
        /// Returns link relation tags for both shortcut icon and icon.
        /// </summary>
        private string BuildFaviconTags(string faviconPath, string sizes = null)
        {
            string faviconUrl = UrlResolver.ResolveUrl(faviconPath);
            string extension = URLHelper.GetUrlExtension(faviconUrl);
            string type = MimeTypeHelper.GetMimetype(extension, null);

            string shortcutIconLink = HTMLHelper.GetLink(faviconUrl, type, rel: "shortcut icon", media: null, title: null, sizes: sizes);
            string iconLink = HTMLHelper.GetLink(faviconUrl, type, rel: "icon", media: null, title: null, sizes: sizes);
            return shortcutIconLink + iconLink;
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="extension"/> is scalable format, <c>false</c> otherwise.
        /// </summary>
        private bool IsScalableFormat(string extension)
        {
            extension = extension.Trim('.');
            return extension.Equals(SCALABLE_FAVICON_EXTENSION, StringComparison.OrdinalIgnoreCase);
        }
    }
}
