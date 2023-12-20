using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Localization;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(CultureSiteInfo), CultureSiteInfo.OBJECT_TYPE)]

namespace CMS.SiteProvider
{
    /// <summary>
    /// CultureSiteInfo data container class.
    /// </summary>
    public class CultureSiteInfo : AbstractInfo<CultureSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CULTURESITE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CultureSiteInfoProvider), OBJECT_TYPE, "CMS.CultureSite", null, null, null, null, null, null, "SiteID", "CultureID", CultureInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsBinding = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
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
        /// Culture ID.
        /// </summary>
        public virtual int CultureID
        {
            get
            {
                return GetIntegerValue("CultureID", 0);
            }
            set
            {
                SetValue("CultureID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CultureSiteInfoProvider.DeleteCultureSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CultureSiteInfoProvider.SetCultureSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CultureSiteInfo object.
        /// </summary>
        public CultureSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CultureSiteInfo object from the given DataRow.
        /// </summary>
        public CultureSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}