using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(SiteDomainAliasInfo), SiteDomainAliasInfo.OBJECT_TYPE)]

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site domain alias.
    /// </summary>
    public class SiteDomainAliasInfo : AbstractInfo<SiteDomainAliasInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.sitedomainalias";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SiteDomainAliasInfoProvider), OBJECT_TYPE, "CMS.SiteDomainAlias", "SiteDomainAliasID", "SiteDomainLastModified", "SiteDomainGUID", "SiteDomainAliasName", "SiteDomainAliasName", null, null, "SiteID", SiteInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the site domain alias id.
        /// </summary>
        [DatabaseField]
        public virtual int SiteDomainAliasID
        {
            get
            {
                return GetIntegerValue("SiteDomainAliasID", 0);
            }
            set
            {
                SetValue("SiteDomainAliasID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site domain alias name.
        /// </summary>
        [DatabaseField]
        public virtual string SiteDomainAliasName
        {
            get
            {
                return GetStringValue("SiteDomainAliasName", "");
            }
            set
            {
                SetValue("SiteDomainAliasName", value);
            }
        }


        /// <summary>
        /// Gets or sets the site id.
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
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site default visitor culture.
        /// </summary>
        [DatabaseField]
        public virtual string SiteDefaultVisitorCulture
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
        /// Site domain GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid SiteDomainGUID
        {
            get
            {
                return GetGuidValue("SiteDomainGUID", Guid.Empty);
            }
            set
            {
                SetValue("SiteDomainGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SiteDomainLastModified
        {
            get
            {
                return GetDateTimeValue("SiteDomainLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SiteDomainLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Object Default alias path.
        /// </summary>
        [DatabaseField]
        public virtual string SiteDomainDefaultAliasPath
        {
            get
            {
                return GetStringValue("SiteDomainDefaultAliasPath", "");
            }
            set
            {
                SetValue("SiteDomainDefaultAliasPath", value);
            }
        }


        /// <summary>
        /// Object Redirect URL.
        /// </summary>
        [DatabaseField]
        public virtual string SiteDomainRedirectUrl
        {
            get
            {
                return GetStringValue("SiteDomainRedirectUrl", "");
            }
            set
            {
                SetValue("SiteDomainRedirectUrl", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SiteDomainAliasInfoProvider.DeleteSiteDomainAliasInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SiteDomainAliasInfoProvider.SetSiteDomainAliasInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SiteDomainAlias object.
        /// </summary>
        public SiteDomainAliasInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SiteDomainAlias object from the given DataRow.
        /// </summary>
        public SiteDomainAliasInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}