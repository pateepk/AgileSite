using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using CMS.Helpers;
using CMS.IO;
using CMS.Localization;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides localization script for Page builder feature.
    /// </summary>
    internal sealed class LocalizationScriptProvider : ILocalizationScriptProvider
    {
        private readonly string resourcesFolderPath;

        /// <summary>
        /// Cache minutes for localization script.
        /// </summary>
        internal int CacheMinutes { get; set; } = 10;


        /// <summary>
        /// Creates an instance of <see cref="LocalizationScriptProvider"/> class.
        /// </summary>
        /// <param name="resourcesFolderPath">Folder path where to search for the localizations.</param>
        public LocalizationScriptProvider(string resourcesFolderPath)
        {
            this.resourcesFolderPath = resourcesFolderPath;
        }


        /// <summary>
        /// Gets the script with localization of resource strings.
        /// </summary>
        /// <param name="cultureCode">Culture code of the localization.</param>
        public string Get(string cultureCode)
        {
            IEnumerable<string> filePaths = null;
            return CacheHelper.Cache(() =>
                {
                    filePaths = GetFilePaths(cultureCode);
                    return GetLocalizationScript(filePaths, cultureCode);
                },
                new CacheSettings(CacheMinutes, "Kentico.Scripts.Localization", cultureCode)
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(filePaths.ToArray(), null)
                });
        }


        private string GetLocalizationScript(IEnumerable<string> filePaths, string cultureCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("(function () {");
            sb.AppendLine("window.kentico.localization = window.kentico.localization || {};");
            sb.AppendLine($"window.kentico.localization.culture = \"{HttpUtility.JavaScriptStringEncode(cultureCode)}\";");
            sb.AppendLine("window.kentico.localization.strings = window.kentico.localization.strings || {};");

            var resourcesPaths = string.Join(";", filePaths);
            var manager = new FileResourceManager(resourcesPaths, cultureCode);

            foreach (var key in manager.Resources.TypedKeys)
            {
                sb.AppendLine($"window.kentico.localization.strings[\"{key}\"] = \"{HttpUtility.JavaScriptStringEncode(manager.GetString(key))}\";");
            }

            sb.Append("})();");

            return sb.ToString();
        }


        private IEnumerable<string> GetFilePaths(string cultureCode)
        {
            if (resourcesFolderPath == null || !Directory.Exists(resourcesFolderPath))
            {
                return Enumerable.Empty<string>();
            }

            if (String.IsNullOrEmpty(cultureCode) || cultureCode.Equals(CultureHelper.DefaultUICultureCode, StringComparison.InvariantCultureIgnoreCase))
            {
                cultureCode = String.Empty;
            }

            return LocalizationHelper.GetFolderResourcesFilePaths(resourcesFolderPath, cultureCode);
        }
    }
}