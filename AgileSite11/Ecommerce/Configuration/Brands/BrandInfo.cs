using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(BrandInfo), BrandInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="BrandInfo"/>.
    /// </summary>
	[Serializable]
    public class BrandInfo : AbstractInfo<BrandInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.brand";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BrandInfoProvider), OBJECT_TYPE, "ecommerce.brand", "BrandID", "BrandLastModified", "BrandGuid", "BrandName", "BrandDisplayName", null, "BrandSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE)
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            ThumbnailGUIDColumn = "BrandThumbnailGUID",
            HasMetaFiles = true,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE)
                }
            },
            EnabledColumn = "BrandEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Brand ID.
        /// </summary>
        [DatabaseField]
        public virtual int BrandID
        {
            get
            {
                return GetIntegerValue("BrandID", 0);
            }
            set
            {
                SetValue("BrandID", value);
            }
        }


        /// <summary>
        /// Brand name.
        /// </summary>
        [DatabaseField]
        public virtual string BrandName
        {
            get
            {
                return GetStringValue("BrandName", String.Empty);
            }
            set
            {
                SetValue("BrandName", value, String.Empty);
            }
        }


        /// <summary>
        /// Brand display name.
        /// </summary>
        [DatabaseField]
        public virtual string BrandDisplayName
        {
            get
            {
                return GetStringValue("BrandDisplayName", String.Empty);
            }
            set
            {
                SetValue("BrandDisplayName", value);
            }
        }


        /// <summary>
        /// Brand description.
        /// </summary>
        [DatabaseField]
        public virtual string BrandDescription
        {
            get
            {
                return GetStringValue("BrandDescription", String.Empty);
            }
            set
            {
                SetValue("BrandDescription", value, String.Empty);
            }
        }


        /// <summary>
        /// Brand homepage.
        /// </summary>
        [DatabaseField]
        public virtual string BrandHomepage
        {
            get
            {
                return GetStringValue("BrandHomepage", String.Empty);
            }
            set
            {
                SetValue("BrandHomepage", value, String.Empty);
            }
        }


        /// <summary>
        /// Brand thumbnail GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid BrandThumbnailGUID
        {
            get
            {
                return GetGuidValue("BrandThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("BrandThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Brand site ID.
        /// </summary>
        [DatabaseField]
        public virtual int BrandSiteID
        {
            get
            {
                return GetIntegerValue("BrandSiteID", 0);
            }
            set
            {
                SetValue("BrandSiteID", value, 0);
            }
        }


        /// <summary>
        /// Brand enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool BrandEnabled
        {
            get
            {
                return GetBooleanValue("BrandEnabled", true);
            }
            set
            {
                SetValue("BrandEnabled", value);
            }
        }


        /// <summary>
        /// Brand guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid BrandGuid
        {
            get
            {
                return GetGuidValue("BrandGuid", Guid.Empty);
            }
            set
            {
                SetValue("BrandGuid", value);
            }
        }


        /// <summary>
        /// Brand last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime BrandLastModified
        {
            get
            {
                return GetDateTimeValue("BrandLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BrandLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BrandInfoProvider.DeleteBrandInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BrandInfoProvider.SetBrandInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected BrandInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="BrandInfo"/> class.
        /// </summary>
        public BrandInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="BrandInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public BrandInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.ECOMMERCE_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.ECOMMERCE_MODIFY, siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}