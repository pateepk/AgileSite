using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Relationships;

[assembly: RegisterObjectType(typeof(RelationshipNameSiteInfo), RelationshipNameSiteInfo.OBJECT_TYPE)]

namespace CMS.Relationships
{
    /// <summary>
    /// RelationshipNameSiteInfo data container class.
    /// </summary>
    public class RelationshipNameSiteInfo : AbstractInfo<RelationshipNameSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.relationshipnamesite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RelationshipNameSiteInfoProvider), OBJECT_TYPE, "CMS.RelationshipNameSite", null, null, null, null, null, null, "SiteID", "RelationshipNameID", RelationshipNameInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            IsBinding = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
           
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Relationship name ID.
        /// </summary>
        public virtual int RelationshipNameID
        {
            get
            {
                return GetIntegerValue("RelationshipNameID", 0);
            }
            set
            {
                SetValue("RelationshipNameID", value);
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
            RelationshipNameSiteInfoProvider.DeleteRelationshipNameSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RelationshipNameSiteInfoProvider.SetRelationshipNameSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RelationshipNameSiteInfo object.
        /// </summary>
        public RelationshipNameSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RelationshipNameSiteInfo object from the given DataRow.
        /// </summary>
        public RelationshipNameSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}