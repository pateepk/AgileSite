using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    internal class PathPrefixRemoval
    {
        /// <summary>
        /// Returns culture code or culture alias defined in URL and removes it from input path.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="siteName">Site name</param>
        internal static string RemoveCulturePrefix(string siteName, ref string path)
        {
            CultureInfo ci = null;
            return RemoveCulturePrefix(siteName, ref path, ref ci);
        }


        internal static string RemoveCulturePrefix(string siteName, ref string path, ref CultureInfo ci)
        {
            // Site name must be defined
            if ((siteName == null) || (path == null))
            {
                return null;
            }

            // Check whether lang prefix is enabled
            if (!URLHelper.UseLangPrefixForUrls(siteName))
            {
                return null;
            }

            // Culture prefix is not present
            string localPath = path.TrimStart('/') + "/";
            int slashIndex = localPath.IndexOfCSafe('/');
            if (slashIndex <= 0)
            {
                return null;
            }

            // Get culture code from URL
            string culture = path.Substring(1, slashIndex);

            // Get all site cultures
            DataSet siteCultures = CultureSiteInfoProvider.GetSiteCultures(siteName);
            if (DataHelper.DataSourceIsEmpty(siteCultures))
            {
                return null;
            }

            // Loop through all site cultures
            foreach (DataRow row in siteCultures.Tables[0].Rows)
            {
                var cultureAlias = row["CultureAlias"].ToString(null);
                var cultureCode = row["CultureCode"].ToString(null);

                // Check whether culture in URL is defined in either site culture or domain alias
                if (cultureCode.EqualsCSafe(culture, true) || cultureAlias.EqualsCSafe(culture, true))
                {
                    ci = CultureInfoProvider.GetCultureInfo(cultureCode);
                    break;
                }
            }

            // None of the site cultures matches
            if (ci == null)
            {
                return null;
            }

            // Remove culture prefix
            path = path.TrimStart('/').Substring(slashIndex);

            return culture;
        }


        internal static bool RemovePathPrefix(string siteName, ref string path)
        {
            if ((siteName == null) || (path == null))
            {
                return false;
            }

            // Get the prefix
            string prefix = SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaultUrlPathPrefix");
            if (prefix == "")
            {
                return false;
            }

            prefix = "/" + prefix.Trim('/') + "/";
            string pathToCompare = path + "/";
            if (!pathToCompare.StartsWithCSafe(prefix, true))
            {
                return false;
            }

            path = path.Substring(prefix.Length - 1);
            return true;
        }
    }
}