using System;
using System.Runtime.Serialization;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class to hold current user information. Extends UserInfo with context specific information.
    /// </summary>
    [Serializable]
    public class CurrentUserInfo : UserInfo
    {
        /// <summary>
        /// User preferred UI Culture code.
        /// </summary>
        public override string PreferredUICultureCode
        {
            get
            {
                return base.PreferredUICultureCode;
            }
            set
            {
                base.PreferredUICultureCode = value;

                // Prevent setting cookie on MVC which disrupts output cache
                if (SystemContext.IsCMSRunningAsMainApplication)
                {
                    CultureHelper.SetPreferredUICultureCode(value);
                }
            }
        }


        /// <summary>
        /// Gets the UserSettings info object for the user.
        /// </summary>
        /// <remarks>
        /// Ensures that user settings will be loaded to the source cached instance.
        /// </remarks>
        internal override UserSettingsInfo UserSettingsInternal
        {
            get
            {
                if (!UserSettingsLoaded)
                {
                    var user = UserInfoProvider.GetUserInfo(UserID);
                    if (user != null)
                    {
                        base.UserSettingsInternal = user.UserSettings;
                    }
                }

                return base.UserSettingsInternal;
            }
            set
            {
                base.UserSettingsInternal = value;
            }
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected CurrentUserInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use UserInfo instead.")]
        public CurrentUserInfo()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceInfo">Source user info</param>
        /// <param name="keepSourceData">If true, source data are kept</param>
        public CurrentUserInfo(UserInfo sourceInfo, bool keepSourceData)
            : base(sourceInfo, keepSourceData)
        {
            UpdatePreferredCultures();
        }


        internal override bool SecurityCollectionsLoadedByInheritedClass(out UserSecurityCollections securityCollections)
        {
            var user = UserInfoProvider.GetUserInfo(UserID);
            if (user != null && !(user is CurrentUserInfo))
            {
                securityCollections = user.EnsureSecurityCollections();
                user.CopySecurityCollections(this);
                return true;
            }

            return base.SecurityCollectionsLoadedByInheritedClass(out securityCollections);
        }


        /// <summary>
        /// Sets the object default values
        /// </summary>
        protected override void LoadDefaultData()
        {
            // Do not load default data as this is only container for existing user
        }


        /// <summary>
        /// Updates the preferred cultures of the user
        /// </summary>
        private void UpdatePreferredCultures()
        {
            string culture;

            // Try to change the preferred culture code
            try
            {
                culture = CultureHelper.GetPreferredCulture();

                var preferred = LocalizationContext.PreferredCultureCode;

                if (!string.IsNullOrEmpty(preferred))
                {
                    // Leave the culture from the database
                    CultureHelper.GetCultureInfo(preferred);
                }
                else if (!string.IsNullOrEmpty(culture))
                {
                    // Get the culture from current browser settings
                    CultureHelper.GetCultureInfo(culture);
                    LocalizationContext.PreferredCultureCode = culture;
                }
                else
                {
                    string defaultCulture = CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName);

                    CultureHelper.GetCultureInfo(defaultCulture);
                    LocalizationContext.PreferredCultureCode = defaultCulture;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CurrentUser", "UPDATECULTURES", ex);
            }

            // Try to change the preferred UI UICulture code
            try
            {
                culture = CultureHelper.GetPreferredUICultureCode();

                if (!string.IsNullOrEmpty(PreferredUICultureCode) && Service.Resolve<ICultureService>().IsUICulture(PreferredUICultureCode))
                {
                    // Leave the UICulture from the database
                }
                else if (!string.IsNullOrEmpty(culture))
                {
                    // Get the UICulture from current browser settings
                    PreferredUICultureCode = culture;
                }
                else
                {
                    PreferredUICultureCode = CultureHelper.DefaultUICultureCode;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CurrentUser", "UPDATECULTURES", ex);
            }
        }
    }
}