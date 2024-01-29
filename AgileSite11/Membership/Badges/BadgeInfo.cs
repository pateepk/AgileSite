using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(BadgeInfo), BadgeInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// BadgeInfo data container class.
    /// </summary>
    public class BadgeInfo : AbstractInfo<BadgeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.badge";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BadgeInfoProvider), OBJECT_TYPE, "CMS.Badge", "BadgeID", "BadgeLastModified", "BadgeGUID", "BadgeName", "BadgeDisplayName", null, null, null, null)
        {
            ModuleName = "cms.badges",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY)
                }
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY)
                }
            },

            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = true,
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Badge id.
        /// </summary>
        public virtual int BadgeID
        {
            get
            {
                return GetIntegerValue("BadgeID", 0);
            }
            set
            {
                SetValue("BadgeID", value);
            }
        }


        /// <summary>
        /// Badge image URL.
        /// </summary>
        public virtual string BadgeImageURL
        {
            get
            {
                return GetStringValue("BadgeImageURL", "");
            }
            set
            {
                SetValue("BadgeImageURL", value);
            }
        }


        /// <summary>
        /// Badge name.
        /// </summary>
        public virtual string BadgeName
        {
            get
            {
                return GetStringValue("BadgeName", "");
            }
            set
            {
                SetValue("BadgeName", value);
            }
        }


        /// <summary>
        /// Badge display name.
        /// </summary>
        public virtual string BadgeDisplayName
        {
            get
            {
                return GetStringValue("BadgeDisplayName", "");
            }
            set
            {
                SetValue("BadgeDisplayName", value);
            }
        }


        /// <summary>
        /// Badge is automatic.
        /// </summary>
        public virtual bool BadgeIsAutomatic
        {
            get
            {
                return GetBooleanValue("BadgeIsAutomatic", false);
            }
            set
            {
                SetValue("BadgeIsAutomatic", value);
            }
        }


        /// <summary>
        /// Badge top limit.
        /// </summary>
        public virtual int BadgeTopLimit
        {
            get
            {
                return GetIntegerValue("BadgeTopLimit", 0);
            }
            set
            {
                SetValue("BadgeTopLimit", value);
            }
        }


        /// <summary>
        /// Badge guid.
        /// </summary>
        public virtual Guid BadgeGUID
        {
            get
            {
                return GetGuidValue("BadgeGUID", Guid.Empty);
            }
            set
            {
                SetValue("BadgeGUID", value);
            }
        }


        /// <summary>
        /// Badge last modified.
        /// </summary>
        public virtual DateTime BadgeLastModified
        {
            get
            {
                return GetDateTimeValue("BadgeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BadgeLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BadgeInfoProvider.DeleteBadgeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BadgeInfoProvider.SetBadgeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BadgeInfo object.
        /// </summary>
        public BadgeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BadgeInfo object from the given DataRow.
        /// </summary>
        public BadgeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}