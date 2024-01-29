using System;
using System.Collections;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Globalization;
using CMS.Base;
using CMS.SiteProvider;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

[assembly: RegisterObjectType(typeof(SiteInfo), SiteInfo.OBJECT_TYPE)]

namespace CMS.SiteProvider
{
    /// <summary>
    /// Represents a site.
    /// </summary>
    public class SiteInfo : AbstractInfo<SiteInfo>, ISiteInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.SITE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SiteInfoProvider), OBJECT_TYPE, "CMS.Site", "SiteID", "SiteLastModified", "SiteGUID", "SiteName", "SiteDisplayName", null, null, null, null)
        {
            SupportsVersioning = false,
            AllowRestore = false,
            SupportsInvalidation = true,
            SupportsCloning = false,
            ImportExportSettings = { LogExport = false, IsExportable = true },
            TouchCacheDependencies = true,
            HasMetaFiles = true,
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "SiteStatus"
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Hash table contains domain aliases of current site [alias.ToLowerCSafe()] -> [DomainAliasInfo]
        /// </summary>
        protected Hashtable mSiteDomainAliases;

        private string mSitePresentationDomain;

        #endregion


        #region "Properties"

        /// <summary>
        /// Combine with default culture?
        /// </summary>
        [DatabaseMapping(false)]
        public bool CombineWithDefaultCulture
        {
            get
            {
                return SiteInfoProvider.CombineWithDefaultCulture(SiteName);
            }
        }


        /// <summary>
        /// Combine files with default culture?
        /// </summary>
        [DatabaseMapping(false)]
        public bool CombineFilesWithDefaultCulture
        {
            get
            {
                return SiteInfoProvider.CombineFilesWithDefaultCulture(SiteName);
            }
        }


        /// <summary>
        /// Indicates whether the site uses multiple cultures.
        /// </summary>
        [RegisterProperty]
        [DatabaseMapping(false)]
        public bool HasMultipleCultures
        {
            get
            {
                DataSet siteCulturesDS = CultureSiteInfoProvider.GetSiteCultures(SiteContext.CurrentSiteName);
                return (!DataHelper.DataSourceIsEmpty(siteCulturesDS) && (siteCulturesDS.Tables[0].Rows.Count > 1));
            }
        }


        /// <summary>
        /// Site domain name.
        /// </summary>
        [DatabaseField("SiteDomainName")]
        public virtual string DomainName
        {
            get
            {
                return GetStringValue("SiteDomainName", String.Empty);
            }
            set
            {
                SetValue("SiteDomainName", value);
            }
        }


        /// <summary>
        /// Presentation URL of the site.
        /// </summary>
        [DatabaseField]
        public virtual string SitePresentationURL
        {
            get
            {
                return GetStringValue("SitePresentationURL", string.Empty);
            }
            set
            {
                SetValue("SitePresentationURL", value);

                // Reset computed presentation domain
                mSitePresentationDomain = null;
            }
        }


        /// <summary>
        /// Domain name which is parsed from the SitePresentationURL property.
        /// </summary>
        public virtual string SitePresentationDomain
        {
            get
            {
                return mSitePresentationDomain ?? (mSitePresentationDomain = GetDomainName(SitePresentationURL));
            }
        }


        /// <summary>
        /// Site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Site default editor stylesheet.
        /// </summary>
        [DatabaseField]
        public virtual int SiteDefaultEditorStylesheet
        {
            get
            {
                return GetIntegerValue("SiteDefaultEditorStylesheet", 0);
            }
            set
            {
                SetValue("SiteDefaultEditorStylesheet", value, (value > 0));
            }
        }


        /// <summary>
        /// Site default stylesheet ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteDefaultStylesheetID
        {
            get
            {
                return GetIntegerValue("SiteDefaultStylesheetID", 0);
            }
            set
            {
                SetValue("SiteDefaultStylesheetID", value, (value > 0));
            }
        }


        /// <summary>
        /// Site description.
        /// </summary>
        [DatabaseField("SiteDescription")]
        public virtual string Description
        {
            get
            {
                return GetStringValue("SiteDescription", "");
            }
            set
            {
                SetValue("SiteDescription", value);
            }
        }


        /// <summary>
        /// Site name.
        /// </summary>
        [DatabaseField]
        public virtual string SiteName
        {
            get
            {
                return GetStringValue("SiteName", "");
            }
            set
            {
                SetValue("SiteName", value);
            }
        }


        /// <summary>
        /// Site default visitor culture.
        /// </summary>
        [DatabaseField("SiteDefaultVisitorCulture")]
        public virtual string DefaultVisitorCulture
        {
            get
            {
                return GetStringValue("SiteDefaultVisitorCulture", "");
            }
            set
            {
                SetValue("SiteDefaultVisitorCulture", value);
            }
        }


        /// <summary>
        /// Site display name.
        /// </summary>
        [DatabaseField("SiteDisplayName")]
        public virtual string DisplayName
        {
            get
            {
                return GetStringValue("SiteDisplayName", "");
            }
            set
            {
                SetValue("SiteDisplayName", value);
            }
        }


        /// <summary>
        /// Site status.
        /// </summary>
        [DatabaseField(ColumnName = "SiteStatus", ValueType = typeof(string))]
        public virtual SiteStatusEnum Status
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SiteStatus"), String.Empty).ToEnum<SiteStatusEnum>();
            }
            set
            {
                SetValue("SiteStatus", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Site GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid SiteGUID
        {
            get
            {
                return GetGuidValue("SiteGUID", Guid.Empty);
            }
            set
            {
                SetValue("SiteGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SiteLastModified
        {
            get
            {
                return GetDateTimeValue("SiteLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SiteLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Hash table contains site's domain aliases. [alias.ToLowerInvariant()] -> [DomainAliasInfo]
        /// </summary>
        public virtual Hashtable SiteDomainAliases
        {
            get
            {
                if (mSiteDomainAliases == null)
                {
                    int sid = SiteID;
                    if (sid > 0)
                    {
                        // Get the aliases
                        DataSet ds = SiteDomainAliasInfoProvider.GetDomainAliases(sid);
                        mSiteDomainAliases = CreateDomainAliasesTable(ds);
                    }
                    else
                    {
                        mSiteDomainAliases = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
                    }
                }
                return mSiteDomainAliases;
            }
            set
            {
                mSiteDomainAliases = value;
            }
        }


        /// <summary>
        /// Returns time zone info for current site.
        /// </summary>
        public virtual TimeZoneInfo SiteTimeZone
        {
            get
            {
                return TimeZoneHelper.GetTimeZoneInfo(this);
            }
        }


        /// <summary>
        /// Indicates whether site is in offline mode.
        /// </summary>
        [DatabaseField]
        public virtual bool SiteIsOffline
        {
            get
            {
                return GetBooleanValue("SiteIsOffline", false);
            }
            set
            {
                SetValue("SiteIsOffline", value);
            }
        }


        /// <summary>
        /// URL redirection in offline mode.
        /// </summary>
        [DatabaseField]
        public virtual string SiteOfflineRedirectURL
        {
            get
            {
                return GetStringValue("SiteOfflineRedirectURL", null);
            }
            set
            {
                SetValue("SiteOfflineRedirectURL", value);
            }
        }


        /// <summary>
        /// Offline message.
        /// </summary>
        [DatabaseField]
        public virtual string SiteOfflineMessage
        {
            get
            {
                return GetStringValue("SiteOfflineMessage", null);
            }
            set
            {
                SetValue("SiteOfflineMessage", value);
            }
        }


        /// <summary>
        /// Indicates whether the site is used only for storing content and is not responsible for live site presentation.
        /// </summary>
        [DatabaseField]
        public bool SiteIsContentOnly
        {
            get
            {
                return GetBooleanValue("SiteIsContentOnly", false);
            }
            set
            {
                SetValue("SiteIsContentOnly", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SiteInfoProvider.DeleteSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SiteInfoProvider.SetSiteInfo(this);
        }

        
        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            Status = SiteStatusEnum.Stopped;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SiteInfo object.
        /// </summary>
        public SiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SiteInfo object from the given DataRow.
        /// </summary>
        public SiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SiteInfo object from the given DataClass.
        /// </summary>
        /// <param name="si">Source site info</param>
        /// <param name="keepSourceData">If true, source data are kept in the object</param>
        public SiteInfo(SiteInfo si, bool keepSourceData)
            : base(TYPEINFO, si.DataClass, keepSourceData)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates hashtable from given dataset.
        /// </summary>
        /// <param name="ds">Dataset contains with document aliases</param>
        /// <returns>Created hashtable.</returns>
        private Hashtable CreateDomainAliasesTable(DataSet ds)
        {
            Hashtable aliases = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Add domain alias
                    string domainAlias = ValidationHelper.GetString(dr["SiteDomainAliasName"], "");
                    aliases[domainAlias] = new SiteDomainAliasInfo(dr);
                }
            }

            return aliases;
        }


        /// <summary>
        /// Returns server date time in dependence on site time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime ConvertServerDateTime(DateTime dateTime)
        {
            return TimeZoneHelper.ConvertToServerDateTime(dateTime, this);
        }


        /// <summary>
        /// Returns site date time in dependence on site time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime ConvertSiteDateTime(DateTime dateTime)
        {
            return TimeZoneHelper.ConvertToSiteDateTime(dateTime, this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Cloned site should be stopped
            Status = SiteStatusEnum.Stopped;
            Insert();
        }


        /// <summary>
        /// Returns domain name of the given URL.
        /// </summary>
        /// <param name="url">Relative or absolute URL</param>
        private static string GetDomainName(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return String.Empty;
            }

            url = URLHelper.AddHTTPToUrl(url);
            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                return uri.Host;
            }

            return String.Empty;
        }

        #endregion
    }
}