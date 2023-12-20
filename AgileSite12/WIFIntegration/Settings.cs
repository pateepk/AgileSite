using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Class providing settings of module.
    /// </summary>
    internal class Settings
    {
        #region "Variables"

        private readonly string mSiteName = null;
        private IEnumerable<Uri> mAllowedAudienceUris = null;
        private CertificateValidatorEnum? mCertificateValidator = null;
        private bool? mEnabled = null;
        private string mRealm = null;
        private Uri mIdentityProvider = null;
        private string mTrustedCertificateThumbprint = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Allowed audience of identity provider.
        /// </summary>
        public IEnumerable<Uri> AllowedAudienceURIs
        {
            get
            {
                if (mAllowedAudienceUris == null)
                {
                    Uri uri = null;
                    mAllowedAudienceUris = from url in SettingsKeyInfoProvider.GetValue(mSiteName + ".CMSWIFAllowedAudienceUris").Split(';')
                                           where Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri)
                                           select uri;
                }
                return mAllowedAudienceUris;
            }
        }


        /// <summary>
        /// Selected certificate validator type.
        /// </summary>
        public CertificateValidatorEnum CertificateValidator
        {
            get
            {
                if (!mCertificateValidator.HasValue)
                {
                    mCertificateValidator = (CertificateValidatorEnum)SettingsKeyInfoProvider.GetIntValue(mSiteName + ".CMSWIFCertificateValidator");
                }
                return mCertificateValidator.Value;
            }
        }


        /// <summary>
        /// Indicates whether module is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (!mEnabled.HasValue)
                {
                    var configValue = SettingsHelper.AppSettings["CMSEnableWIF"];
                    mEnabled = ValidationHelper.GetBoolean(configValue, SettingsKeyInfoProvider.GetBoolValue(mSiteName + ".CMSWIFEnabled"));
                }
                return mEnabled.Value;
            }
        }


        /// <summary>
        /// Realm of the application (web site).
        /// </summary>
        public string Realm
        {
            get
            {
                if (string.IsNullOrEmpty(mRealm))
                {
                    mRealm = SettingsKeyInfoProvider.GetValue(mSiteName + ".CMSWIFRealm");
                }
                return mRealm;
            }
        }


        /// <summary>
        /// URL of identity provider.
        /// </summary>
        public Uri IdentityProviderURL
        {
            get
            {
                if (mIdentityProvider == null)
                {
                    string url = SettingsKeyInfoProvider.GetValue(mSiteName + ".CMSWIFIdentityProviderURL");
                    Uri.TryCreate(URLHelper.GetAbsoluteUrl(url), UriKind.Absolute, out mIdentityProvider);
                }
                return mIdentityProvider;
            }
        }


        /// <summary>
        /// Thumbprint of trusted certificate used for secure communication with identity provider.
        /// </summary>
        public string TrustedCertificateThumbprint
        {
            get
            {
                if (string.IsNullOrEmpty(mTrustedCertificateThumbprint))
                {
                    mTrustedCertificateThumbprint = SettingsKeyInfoProvider.GetValue(mSiteName + ".CMSWIFTrustedCertificateThumbprint");
                }
                return mTrustedCertificateThumbprint;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for current site settings.
        /// </summary>
        public Settings()
            : this(SiteContext.CurrentSiteName)
        {
        }


        /// <summary>
        /// Creates instance of settings for given site.
        /// </summary>
        /// <param name="siteName">Name of site.</param>
        public Settings(string siteName)
        {
            mSiteName = siteName;
        }

        #endregion
    }
}
