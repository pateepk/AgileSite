using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ManufacturerInfo), ManufacturerInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ManufacturerInfo data container class.
    /// </summary>
    public class ManufacturerInfo : AbstractInfo<ManufacturerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.manufacturer";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ManufacturerInfoProvider), OBJECT_TYPE, "ECommerce.Manufacturer", "ManufacturerID", "ManufacturerLastModified", "ManufacturerGUID", "ManufacturerName", "ManufacturerDisplayName", null, "ManufacturerSiteID", null, null)
        {
            // Child object types
            // - None

            // Object dependencies
            // - None

            // Binding object types
            // - None

            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                }
            },

            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            ThumbnailGUIDColumn = "ManufacturerThumbnailGUID",
            HasMetaFiles = true,
            NameGloballyUnique = true,
            SupportsGlobalObjects = true,
            SupportsCloneToOtherSite = false,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                },
            },
            EnabledColumn = "ManufacturerEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Manufacturer enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool ManufacturerEnabled
        {
            get
            {
                return GetBooleanValue("ManufacturerEnabled", false);
            }
            set
            {
                SetValue("ManufacturerEnabled", value);
            }
        }


        /// <summary>
        /// Manufacturer ID.
        /// </summary>
        [DatabaseField]
        public virtual int ManufacturerID
        {
            get
            {
                return GetIntegerValue("ManufacturerID", 0);
            }
            set
            {
                SetValue("ManufacturerID", value);
            }
        }


        /// <summary>
        /// Manufacturer display name.
        /// </summary>
        [DatabaseField]
        public virtual string ManufacturerDisplayName
        {
            get
            {
                return GetStringValue("ManufacturerDisplayName", "");
            }
            set
            {
                SetValue("ManufacturerDisplayName", value);
            }
        }


        /// <summary>
        /// Manufacturer code name.
        /// </summary>
        [DatabaseField]
        public virtual string ManufacturerName
        {
            get
            {
                return GetStringValue("ManufacturerName", "");
            }
            set
            {
                SetValue("ManufacturerName", value);
            }
        }


        /// <summary>
        /// Manufacturer description.
        /// </summary>
        [DatabaseField]
        public virtual string ManufacturerDescription
        {
            get
            {
                return GetStringValue("ManufacturerDescription", "");
            }
            set
            {
                SetValue("ManufacturerDescription", value);
            }
        }


        /// <summary>
        /// Manufacture homepage.
        /// </summary>
        [DatabaseField("ManufactureHomepage")]
        public virtual string ManufacturerHomepage
        {
            get
            {
                return GetStringValue("ManufactureHomepage", "");
            }
            set
            {
                SetValue("ManufactureHomepage", value);
            }
        }


        /// <summary>
        /// Manufacturer GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ManufacturerGUID
        {
            get
            {
                return GetGuidValue("ManufacturerGUID", Guid.Empty);
            }
            set
            {
                SetValue("ManufacturerGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ManufacturerLastModified
        {
            get
            {
                return GetDateTimeValue("ManufacturerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ManufacturerLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Manufacturer site ID. Set to 0 for global manufacturer.
        /// </summary>
        [DatabaseField]
        public virtual int ManufacturerSiteID
        {
            get
            {
                return GetIntegerValue("ManufacturerSiteID", 0);
            }
            set
            {
                SetValue("ManufacturerSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Manufacturer thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ManufacturerThumbnailGUID
        {
            get
            {
                return GetGuidValue("ManufacturerThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("ManufacturerThumbnailGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ManufacturerInfoProvider.DeleteManufacturerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ManufacturerInfoProvider.SetManufacturerInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ManufacturerInfo object.
        /// </summary>
        public ManufacturerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ManufacturerInfo object from the given DataRow.
        /// </summary>
        public ManufacturerInfo(DataRow dr)
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
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.MANUFACTURERS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return ManufacturerInfoProvider.IsUserAuthorizedToModifyManufacturer(IsGlobal, siteName, userInfo, exceptionOnFailure);
                    
                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}