using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(MembershipInfo), MembershipInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// Membership data container class.
    /// </summary>
    public class MembershipInfo : AbstractInfo<MembershipInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.membership";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MembershipInfoProvider), OBJECT_TYPE, "CMS.Membership", "MembershipID", "MembershipLastModified", "MembershipGUID", "MembershipName", "MembershipDisplayName", null, "MembershipSiteID", null, null)
        {
            ModuleName = "cms.membership",
            SupportsGlobalObjects = true,
            Feature = FeatureEnum.Membership,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            TouchCacheDependencies = true,
            LogEvents = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Membership object ID.
        /// </summary>
        public virtual int MembershipID
        {
            get
            {
                return GetIntegerValue("MembershipID", 0);
            }
            set
            {
                SetValue("MembershipID", value);
            }
        }


        /// <summary>
        /// Date and time when the membership object was last modified.
        /// </summary>
        public virtual DateTime MembershipLastModified
        {
            get
            {
                return GetDateTimeValue("MembershipLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MembershipLastModified", value);
            }
        }


        /// <summary>
        /// Membership object description.
        /// </summary>
        public virtual string MembershipDescription
        {
            get
            {
                return GetStringValue("MembershipDescription", "");
            }
            set
            {
                SetValue("MembershipDescription", value);
            }
        }


        /// <summary>
        /// Membership object display name.
        /// </summary>
        public virtual string MembershipDisplayName
        {
            get
            {
                return GetStringValue("MembershipDisplayName", "");
            }
            set
            {
                SetValue("MembershipDisplayName", value);
            }
        }


        /// <summary>
        /// Membership object site ID.
        /// </summary>
        public virtual int MembershipSiteID
        {
            get
            {
                return GetIntegerValue("MembershipSiteID", 0);
            }
            set
            {
                SetValue("MembershipSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Membership object unique identifier.
        /// </summary>
        public virtual Guid MembershipGUID
        {
            get
            {
                return GetGuidValue("MembershipGUID", Guid.Empty);
            }
            set
            {
                SetValue("MembershipGUID", value);
            }
        }


        /// <summary>
        /// Membership object code name.
        /// </summary>
        public virtual string MembershipName
        {
            get
            {
                return GetStringValue("MembershipName", "");
            }
            set
            {
                SetValue("MembershipName", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MembershipInfoProvider.DeleteMembershipInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MembershipInfoProvider.SetMembershipInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty Membership object.
        /// </summary>
        public MembershipInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new Membership object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MembershipInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}