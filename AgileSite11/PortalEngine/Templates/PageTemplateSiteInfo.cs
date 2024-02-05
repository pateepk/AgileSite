using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(PageTemplateSiteInfo), PageTemplateSiteInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// PageTemplateSiteInfo data container class.
    /// </summary>
    public class PageTemplateSiteInfo : AbstractInfo<PageTemplateSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.pagetemplatesite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageTemplateSiteInfoProvider), OBJECT_TYPE, "CMS.PageTemplateSite", null, null, null, null, null, null, "SiteID", "PageTemplateID", PageTemplateInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.DESIGN,
            IsBinding = true,
            ImportExportSettings =
            {
                LogExport = false
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
        /// Page template ID.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateID
        {
            get
            {
                return GetIntegerValue("PageTemplateID", 0);
            }
            set
            {
                SetValue("PageTemplateID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PageTemplateSiteInfoProvider.DeletePageTemplateSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PageTemplateSiteInfoProvider.SetPageTemplateSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PageTemplateSiteInfo object.
        /// </summary>
        public PageTemplateSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PageTemplateSiteInfo object from the given DataRow.
        /// </summary>
        public PageTemplateSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}