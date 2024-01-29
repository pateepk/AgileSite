using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(CssStylesheetSiteInfo), CssStylesheetSiteInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Css stylesheet site binding info.
    /// </summary>
    public class CssStylesheetSiteInfo : AbstractInfo<CssStylesheetSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.cssstylesheetsite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CssStylesheetSiteInfoProvider), OBJECT_TYPE, "CMS.CssStylesheetSite", null, null, null, null, null, null, "SiteID", "StylesheetID", CssStylesheetInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.DESIGN,
            IsBinding = true,
            ImportExportSettings = { LogExport = false },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.Default,
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
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
        /// Stylesheet ID.
        /// </summary>
        [DatabaseField]
        public virtual int StylesheetID
        {
            get
            {
                return GetIntegerValue("StylesheetID", 0);
            }
            set
            {
                SetValue("StylesheetID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CssStylesheetSiteInfoProvider.DeleteCssStylesheetSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CssStylesheetSiteInfoProvider.SetCssStylesheetSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CssStylesheetSiteInfo object.
        /// </summary>
        public CssStylesheetSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CssStylesheetSiteInfo object from the given DataRow.
        /// </summary>
        public CssStylesheetSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called. 
        /// </summary>
        /// <param name="permission">Permission to convert to string.</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Destroy:
                    return CssStylesheetInfo.PERMISSION_DESTROY_CSS_STYLESHEETS;
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                    return CssStylesheetInfo.PERMISSION_MODIFY_CSS_STYLESHEETS;
                case PermissionsEnum.Read:
                    return CssStylesheetInfo.PERMISSION_READ_CSS_STYLESHEETS;
            }
            return base.GetPermissionName(permission);
        }

        #endregion
    }
}