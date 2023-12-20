using System;
using System.Text;

using CMS.Core;

namespace CMS.ContinuousIntegration
{
    internal static class EncodingConfiguration
    {
        private const string APP_SETTINGS_ENCODING_KEY_NAME = "CMSCIEncoding";

        private static Encoding mEncoding;

        private static string mEncodingName;


        /// <summary>
        /// Gets encoding to be used when no <see cref="EncodingName"/> is provided.
        /// </summary>
        public static Encoding DefaultEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }


        /// <summary>
        /// Gets encoding name specified in the application's configuration, if provided.
        /// </summary>
        public static string EncodingName
        {
            get
            {
                return mEncodingName ?? CoreServices.AppSettings[APP_SETTINGS_ENCODING_KEY_NAME];
            }
            internal set
            {
                mEncodingName = value;
            }
        }


        /// <summary>
        /// Gets encoding to be used for repository files content.
        /// </summary>
        /// <exception cref="RepositoryConfigurationException">Thrown when <see cref="EncodingName"/> is specified, but does not represent a valid encoding name.</exception>
        public static Encoding Encoding
        {
            get
            {
                return mEncoding ?? (mEncoding = String.IsNullOrEmpty(EncodingName) ? DefaultEncoding : CreateEncoding(EncodingName));
            }
        }

        /// <summary>
        /// Gets encoding according to <paramref name="encodingName"/>.
        /// </summary>
        /// <exception cref="RepositoryConfigurationException">Thrown when <paramref name="encodingName"/> does not represent a valid encoding name.</exception>
        private static Encoding CreateEncoding(string encodingName)
        {
            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException ex)
            {
                throw new RepositoryConfigurationException($"Encoding '{encodingName}' does not represent a valid encoding name. Please specify a valid name of encoding. Leave blank for defaulting to '{DefaultEncoding.EncodingName}'.", ex);
            }
        }
    }
}
