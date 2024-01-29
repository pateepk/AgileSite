using System;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Methods for accessing Strands Recommender settings.
    /// </summary>
    public static class StrandsSettings
    {
        /// <summary>
        /// Gets boolean indicating if Strands module is enabled. Module is enabled if both API ID and Validation token are set in settings.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static bool IsStrandsEnabled(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return ((GetApiID(siteName) != null) && (GetValidationToken(siteName) != null));
        }


        /// <summary>
        /// Gets Strands API ID from settings or null if not specified.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <returns>API ID if set, null when setting is empty</returns>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetApiID(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            string apiID = SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsAPIID");
            return string.IsNullOrEmpty(apiID) ? null : apiID;
        }


        /// <summary>
        /// Gets Strands Validation token from settings or null if not specified.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <returns>Validation token if set, null when setting is empty</returns>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetValidationToken(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            var token = SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsValidationToken");
            return string.IsNullOrEmpty(token) ? null : token;
        }


        /// <summary>
        /// Gets transformation used to render XML feed.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        /// <exception cref="StrandsException">Transformation code name in settings is not set</exception>
        public static TransformationInfo GetTransformationInfo(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            var transformationCodeName = SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsCatalogTransformation");

            if (string.IsNullOrEmpty(transformationCodeName))
            {
                throw new StrandsException("[StrandsSettings.GetTransformationInfo]: Transformation value in settings is empty.");
            }

            return TransformationInfoProvider.GetTransformation(transformationCodeName);
        }


        /// <summary>
        /// Gets WHERE condition that restricts selection of products.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <returns>WHERE condition if set, empty string when setting is empty</returns>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetWhereCondition(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsCatalogWhereCondition");
        }


        /// <summary>
        /// Gets path to restrict documents selection.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <returns>Path if set, empty string when setting is empty</returns>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetPath(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsPath");
        }


        /// <summary>
        /// Gets document types that will be rendered into Strands XML feed.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <returns>Document types array if set, empty array when setting is empty</returns>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string[] GetDocumentTypes(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsDocumentTypes").Trim(';').Split(';');
        }


        /// <summary>
        /// Gets whether automatic download of catalog performed by Strands Recommender is enabled or not. If true, after setup call, Strands
        /// will automatically download XML feed. Downloading of feed is performed with frequency specified in <see cref="GetAutomaticCatalogUploadFrequency"/>.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static bool IsAutomaticCatalogUploadEnabled(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSStrandsAutomaticCatalogUploadEnabled");
        }


        /// <summary>
        /// Gets frequency of automatic download performed by Strands Recommender of catalog. Usable only when <see cref="IsAutomaticCatalogUploadEnabled"/> is true, 
        /// specifies frequency with which is catalog automatically updated.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetAutomaticCatalogUploadFrequency(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsAutomaticUploadFrequency");
        }


        /// <summary>
        /// Checks whether catalog feed protection is enabled in settings. Protection is enabled when either username or password are set.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static bool IsCatalogFeedAuthenticationEnabled(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return (!string.IsNullOrEmpty(GetCatalogFeedUsername(siteName)) || !string.IsNullOrEmpty(GetCatalogFeedPassword(siteName)));
        }


        /// <summary>
        /// Gets username required to access Strands catalog feed page.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetCatalogFeedUsername(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsCatalogFeedUsername");
        }


        /// <summary>
        /// Gets password required to access Strands catalog feed page.
        /// </summary>
        /// <param name="siteName">Site name where settings are checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteName"/> is null</exception>
        public static string GetCatalogFeedPassword(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return EncryptionHelper.DecryptData(SettingsKeyInfoProvider.GetValue(siteName + ".CMSStrandsCatalogFeedPassword"));
        }
    }
}
