using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WebPartContainerSiteInfo), WebPartContainerSiteInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// WebPartContainerSiteInfo data container class.
    /// </summary>
    public class WebPartContainerSiteInfo : AbstractInfo<WebPartContainerSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webpartcontainersite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPartContainerSiteInfoProvider), OBJECT_TYPE, "CMS.WebPartContainerSite", null, null, null, null, null, null, "SiteID", "ContainerID", WebPartContainerInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.DESIGN,
            IsBinding = true,
            ImportExportSettings = { LogExport = false },
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
        /// Container ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContainerID
        {
            get
            {
                return GetIntegerValue("ContainerID", 0);
            }
            set
            {
                SetValue("ContainerID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebPartContainerSiteInfoProvider.DeleteWebPartContainerSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebPartContainerSiteInfoProvider.SetWebPartContainerSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebPartContainerSiteInfo object.
        /// </summary>
        public WebPartContainerSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebPartContainerSiteInfo object from the given DataRow.
        /// </summary>
        public WebPartContainerSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}