using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Taxonomy;

[assembly: RegisterObjectType(typeof(TagGroupInfo), TagGroupInfo.OBJECT_TYPE)]

namespace CMS.Taxonomy
{
    /// <summary>
    /// TagGroupInfo data container class.
    /// </summary>
    public class TagGroupInfo : AbstractInfo<TagGroupInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.taggroup";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TagGroupInfoProvider), OBJECT_TYPE, "CMS.TagGroup", "TagGroupID", "TagGroupLastModified", "TagGroupGUID", "TagGroupName", "TagGroupDisplayName", null, "TagGroupSiteID", null, null)
        {
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION)
                },
            },
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },
            LogEvents = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Tag group is ad-hoc.
        /// </summary>
        public virtual bool TagGroupIsAdHoc
        {
            get
            {
                return GetBooleanValue("TagGroupIsAdHoc", false);
            }
            set
            {
                SetValue("TagGroupIsAdHoc", value);
            }
        }


        /// <summary>
        /// Tag group last modified.
        /// </summary>
        public virtual DateTime TagGroupLastModified
        {
            get
            {
                return GetDateTimeValue("TagGroupLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TagGroupLastModified", value);
            }
        }


        /// <summary>
        /// Tag group display name.
        /// </summary>
        public virtual string TagGroupDisplayName
        {
            get
            {
                return GetStringValue("TagGroupDisplayName", "");
            }
            set
            {
                SetValue("TagGroupDisplayName", value);
            }
        }


        /// <summary>
        /// Tag group name.
        /// </summary>
        public virtual string TagGroupName
        {
            get
            {
                return GetStringValue("TagGroupName", "");
            }
            set
            {
                SetValue("TagGroupName", value);
            }
        }


        /// <summary>
        /// Tag group site ID.
        /// </summary>
        public virtual int TagGroupSiteID
        {
            get
            {
                return GetIntegerValue("TagGroupSiteID", 0);
            }
            set
            {
                SetValue("TagGroupSiteID", value);
            }
        }


        /// <summary>
        /// Tag group description.
        /// </summary>
        public virtual string TagGroupDescription
        {
            get
            {
                return GetStringValue("TagGroupDescription", "");
            }
            set
            {
                SetValue("TagGroupDescription", value);
            }
        }


        /// <summary>
        /// Tag group GUID.
        /// </summary>
        public virtual Guid TagGroupGUID
        {
            get
            {
                return GetGuidValue("TagGroupGUID", Guid.Empty);
            }
            set
            {
                SetValue("TagGroupGUID", value);
            }
        }


        /// <summary>
        /// Tag group ID.
        /// </summary>
        public virtual int TagGroupID
        {
            get
            {
                return GetIntegerValue("TagGroupID", 0);
            }
            set
            {
                SetValue("TagGroupID", value);
            }
        }


        /// <summary>
        /// Tag group full name in format [sitename].[taggroupname].
        /// </summary>
        public virtual string TagGroupFullName
        {
            get
            {
                if (TagGroupSiteID > 0)
                {
                    SiteInfo site = SiteInfoProvider.GetSiteInfo(TagGroupSiteID);
                    if (site != null)
                    {
                        return site.SiteName + "." + TagGroupName;
                    }
                }

                return TagGroupName;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TagGroupInfoProvider.DeleteTagGroupInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TagGroupInfoProvider.SetTagGroupInfo(this);
        }


        /// <summary>
        /// Loads the object default data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            TagGroupDescription = "";
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TagGroupInfo object.
        /// </summary>
        public TagGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TagGroupInfo object from the given DataRow.
        /// </summary>
        public TagGroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}