using System;
using System.Text;
using System.Web;

using CMS.IO;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider for metafiles URLs.
    /// </summary>
    public class MetaFileURLProvider : AbstractBaseProvider<MetaFileURLProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns meta file url.
        /// </summary>
        /// <param name="metaFileGuid">Meta file GUID</param>
        /// <param name="fileName">File name without extension</param>
        public static string GetMetaFileUrl(Guid metaFileGuid, string fileName)
        {
            return ProviderObject.GetMetaFileUrlInternal(metaFileGuid, fileName);
        }


        /// <summary>
        /// Returns format of the metafile URL in relative form (with ~). Meaning of the wildcards in returned URL:
        /// - {0} - metafile GUID.
        /// - {1} - extensionless metafile file name.
        /// </summary>
        /// <example>
        /// Example how the returned url can look like:
        /// ~/getmetafile/{0}/{1}.aspx
        /// (.aspx extension can differ based on what is set in settings key CMSFilesFriendlyURLExtension on site specified by <paramref name="siteName"/>)
        /// </example>
        /// <param name="siteName">Sitename used for correct file extension. Null (default) specifies that file extension should be taken from global settings.</param>
        /// <returns>Format of the metafile URL</returns>
        public static string GetMetaFileUrlFormat(string siteName = null)
        {
            return ProviderObject.GetMetaFileUrlFormatInternal(siteName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns meta file url.
        /// </summary>
        /// <param name="metaFileGuid">Meta file GUID</param>
        /// <param name="fileName">File name without extension</param>
        /// <param name="siteName">Sitename used for correct file extension. Null (default) specifies that file extension should be taken from global settings.</param>
        protected virtual string GetMetaFileUrlInternal(Guid metaFileGuid, string fileName, string siteName = null)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            
            return string.Format(GetMetaFileUrlFormat(), metaFileGuid, HttpUtility.UrlEncode(fileName), siteName);
        }


        /// <summary>
        /// Returns format of the metafile URL in relative form (with ~). Meaning of the wildcards in returned URL:
        /// - {0} - metafile GUID.
        /// - {1} - extensionless metafile file name.
        /// </summary>
        /// <example>
        /// Example how the returned url can look like:
        /// ~/getmetafile/{0}/{1}.aspx
        /// (.aspx extension can differ based on what is set in settings key CMSFilesFriendlyURLExtension on site specified by <paramref name="siteName"/>)
        /// </example>
        /// <param name="siteName">Sitename used for correct file extension. Null (default) specifies that file extension should be taken from global settings.</param>
        /// <returns>Format of the metafile URL</returns>
        protected virtual string GetMetaFileUrlFormatInternal(string siteName = null)
        {
            return "~/getmetafile/{0}/{1}" + SettingsKeyInfoProvider.GetFilesUrlExtension(siteName);
        }

        #endregion
    }
}