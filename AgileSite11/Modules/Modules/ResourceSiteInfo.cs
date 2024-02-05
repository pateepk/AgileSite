using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(ResourceSiteInfo), ResourceSiteInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// ResourceSiteInfo data container class.
    /// </summary>
    public class ResourceSiteInfo : AbstractInfo<ResourceSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.resourcesite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ResourceSiteInfoProvider), OBJECT_TYPE, "CMS.ResourceSite", null, null, null, null, null, null, "SiteID", "ResourceID", ResourceInfo.OBJECT_TYPE)
        {
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
        /// Resource ID.
        /// </summary>
        public virtual int ResourceID
        {
            get
            {
                return GetIntegerValue("ResourceID", 0);
            }
            set
            {
                SetValue("ResourceID", value);
            }
        }


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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ResourceSiteInfoProvider.DeleteResourceSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ResourceSiteInfoProvider.SetResourceSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ResourceSiteInfo object.
        /// </summary>
        public ResourceSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ResourceSiteInfo object from the given DataRow.
        /// </summary>
        public ResourceSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}