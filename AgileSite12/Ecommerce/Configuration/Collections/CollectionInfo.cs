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

[assembly: RegisterObjectType(typeof(CollectionInfo), CollectionInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="CollectionInfo"/>.
    /// </summary>
	[Serializable]
    public class CollectionInfo : AbstractInfo<CollectionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.collection";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CollectionInfoProvider), OBJECT_TYPE, "Ecommerce.Collection", "CollectionID", "CollectionLastModified", "CollectionGuid", "CollectionName", "CollectionDisplayName", null, "CollectionSiteID", null, null)
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
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE)
                }
            },

            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE)
                },
            },
            EnabledColumn = "CollectionEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Collection ID.
        /// </summary>
        [DatabaseField]
        public virtual int CollectionID
        {
            get
            {
                return GetIntegerValue("CollectionID", 0);
            }
            set
            {
                SetValue("CollectionID", value);
            }
        }


        /// <summary>
        /// The collection name displayed to users on the live site and in the administration interface..
        /// </summary>
        [DatabaseField]
        public virtual string CollectionDisplayName
        {
            get
            {
                return GetStringValue("CollectionDisplayName", String.Empty);
            }
            set
            {
                SetValue("CollectionDisplayName", value);
            }
        }


        /// <summary>
        /// Collection name.
        /// </summary>
        [DatabaseField]
        public virtual string CollectionName
        {
            get
            {
                return GetStringValue("CollectionName", String.Empty);
            }
            set
            {
                SetValue("CollectionName", value);
            }
        }


        /// <summary>
        /// Collection description.
        /// </summary>
        [DatabaseField]
        public virtual string CollectionDescription
        {
            get
            {
                return GetStringValue("CollectionDescription", String.Empty);
            }
            set
            {
                SetValue("CollectionDescription", value, String.Empty);
            }
        }


        /// <summary>
        /// Collection site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CollectionSiteID
        {
            get
            {
                return GetIntegerValue("CollectionSiteID", 0);
            }
            set
            {
                SetValue("CollectionSiteID", value);
            }
        }


        /// <summary>
        /// Collection enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool CollectionEnabled
        {
            get
            {
                return GetBooleanValue("CollectionEnabled", true);
            }
            set
            {
                SetValue("CollectionEnabled", value);
            }
        }


        /// <summary>
        /// Collection guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid CollectionGuid
        {
            get
            {
                return GetGuidValue("CollectionGuid", Guid.Empty);
            }
            set
            {
                SetValue("CollectionGuid", value);
            }
        }


        /// <summary>
        /// Collection last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CollectionLastModified
        {
            get
            {
                return GetDateTimeValue("CollectionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CollectionLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CollectionInfoProvider.DeleteCollectionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CollectionInfoProvider.SetCollectionInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected CollectionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="CollectionInfo"/> class.
        /// </summary>
        public CollectionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CollectionInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CollectionInfo(DataRow dr)
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