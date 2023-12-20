using System;
using System.Xml;

using CMS.Base;
using CMS.IO;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains properties for retrieving limits for <see cref="FileUploaderComponent"/>.
    /// </summary>
    /// <remarks>
    /// The limits are determined from application's root web.config for configuration sections
    /// applying to the 'Kentico.FormBuilder/FormFileUploader' location or its parents.
    /// </remarks>
    public class FormFileUploadLimitService : IFormFileUploadLimitService
    {
        private const string WEB_CONFIG_STARTING_LOCATION = FormBuilderRoutes.FILE_UPLOADER_POST_ROUTE_TEMPLATE;
        private const int REQUEST_HEADER_RESERVE_KB = 16;
        private const string MAX_REQUEST_LENGTH_ATTRIBUTE_XPATH = "system.web/httpRuntime[@maxRequestLength]";
        private const string MAX_ALLOWED_CONTENT_LENGTH_ATTRIBUTE_XPATH = "system.webServer/security/requestFiltering/requestLimits[@maxAllowedContentLength]";

        private XmlDocument webConfig;
        private bool limitsLoaded;
        private string mWebConfigPath;
        private long mMaxRequestLength;
        private long mMaxAllowedContentLength;


        /// <summary>
        /// Default value of 'maxRequestLength' web.config attribute.
        /// </summary>
        public const long MAX_REQUEST_LENGTH_DEFAULT_VALUE = 4096;


        /// <summary>
        /// Default value of 'maxAllowedContentLength' web.config attribute.
        /// </summary>
        public const long MAX_ALLOWED_CONTENT_LENGHT_DEFAULT_VALUE = 30000000;


        /// <summary>
        /// Gets the path to the application's web.config file.
        /// </summary>
        internal string WebConfigPath
        {
            get
            {
                if (mWebConfigPath == null)
                {
                    mWebConfigPath = Path.Combine(SystemContext.WebApplicationPhysicalPath, "web.config");
                }

                return mWebConfigPath;
            }
            set
            {
                mWebConfigPath = value;
            }
        }


        /// <summary>
        /// Gets the 'maxRequestLength' attribute raw value in kilobytes from the application's root web.config file.
        /// Returns <see cref="MAX_REQUEST_LENGTH_DEFAULT_VALUE"/> if the value is undefined.
        /// </summary>
        public virtual long MaxRequestLength
        {
            get
            {
                if (!limitsLoaded)
                {
                    LoadLimitsFromWebConfig();
                }

                return mMaxRequestLength;
            }
        }


        /// <summary>
        /// Gets the 'maxAllowedContentLength' attribute raw value in bytes from the application's root web.config file.
        /// Returns <see cref="MAX_ALLOWED_CONTENT_LENGHT_DEFAULT_VALUE"/> if the value is undefined.
        /// </summary>
        public virtual long MaxAllowedContentLength
        {
            get
            {
                if (!limitsLoaded)
                {
                    LoadLimitsFromWebConfig();
                }

                return mMaxAllowedContentLength;
            }
        }


        /// <summary>
        /// Gets the maximal allowed upload file size in kilobytes. 
        /// The limit is the smaller value between <see cref="MaxRequestLength"/> and <see cref="MaxAllowedContentLength"/>
        /// from which additional 16 kBs are subtracted for request headers.
        /// </summary>
        public virtual long MaxAllowedFileSize
        {
            get
            {
                return Math.Min(MaxRequestLength, MaxAllowedContentLength / 1024) - REQUEST_HEADER_RESERVE_KB;
            }
        }


        private void LoadLimitsFromWebConfig()
        {
            LoadWebConfig();

            mMaxRequestLength = GetValueFromWebConfig("maxRequestLength", MAX_REQUEST_LENGTH_ATTRIBUTE_XPATH) ?? MAX_REQUEST_LENGTH_DEFAULT_VALUE;
            mMaxAllowedContentLength = GetValueFromWebConfig("maxAllowedContentLength", MAX_ALLOWED_CONTENT_LENGTH_ATTRIBUTE_XPATH) ?? MAX_ALLOWED_CONTENT_LENGHT_DEFAULT_VALUE;

            limitsLoaded = true;
            webConfig = null;
        }


        private void LoadWebConfig()
        {
            webConfig = new XmlDocument();

            webConfig.LoadXml(File.ReadAllText(WebConfigPath));
        }


        private long? GetValueFromWebConfig(string attributeName, string attributePath)
        {
            XmlNode node;

            node = FindContainingNode(attributePath, WEB_CONFIG_STARTING_LOCATION);

            if (node == null || !Int64.TryParse(node.Attributes[attributeName].Value , out var allowedLength))
            {
                return null;
            }

            return allowedLength;
        }


        private XmlNode FindContainingNode(string attributePath, string startingLocation)
        {
            string locationPathValue = SanitizeXPathAttributeValue(startingLocation);

            XmlNode node;

            do
            {
#pragma warning disable SCS0003 // XPath injection possible in {1} argument passed to '{0}'
                node = webConfig.SelectSingleNode($"configuration/location[@path='{locationPathValue}']/{attributePath}");
                if (node == null)
                {
                    var lastForwardSlashPosition = locationPathValue.LastIndexOf('/');
                    if (lastForwardSlashPosition == -1)
                    {
                        break;
                    }

                    locationPathValue = locationPathValue.Substring(0, lastForwardSlashPosition);
                }

            } while (node == null);

            if (node == null)
            {
                node = webConfig.SelectSingleNode($"configuration/location[not(@path)]/{attributePath}");
            }

            if (node == null)
            {
                node = webConfig.SelectSingleNode($"configuration/{attributePath}");
            }
#pragma warning restore SCS0003 // XPath injection possible in {1} argument passed to '{0}'

            return node;
        }


        private string SanitizeXPathAttributeValue(string input)
        {
            return input.Replace("'", "''");
        }
    }
}
