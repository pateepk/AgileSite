using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(OrderStatusInfo), OrderStatusInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OrderStatusInfo data container class.
    /// </summary>
    public class OrderStatusInfo : AbstractInfo<OrderStatusInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.orderstatus";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OrderStatusInfoProvider), OBJECT_TYPE, "ECommerce.OrderStatus", "StatusID", "StatusLastModified", "StatusGUID", "StatusName", "StatusDisplayName", null, "StatusSiteID", null, null)
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
            CheckDependenciesOnDelete = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
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
            OrderColumn = "StatusOrder",
            EnabledColumn = "StatusEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Order of the status.
        /// </summary>
        public virtual int StatusOrder
        {
            get
            {
                return GetIntegerValue("StatusOrder", 0);
            }
            set
            {
                SetValue("StatusOrder", value);
            }
        }


        /// <summary>
        /// ID of the status.
        /// </summary>
        public virtual int StatusID
        {
            get
            {
                return GetIntegerValue("StatusID", 0);
            }
            set
            {
                SetValue("StatusID", value);
            }
        }


        /// <summary>
        /// Indicates whether the status is enabled or not.
        /// </summary>
        public virtual bool StatusEnabled
        {
            get
            {
                return GetBooleanValue("StatusEnabled", false);
            }
            set
            {
                SetValue("StatusEnabled", value);
            }
        }


        /// <summary>
        /// Display name of the status.
        /// </summary>
        public virtual string StatusDisplayName
        {
            get
            {
                return GetStringValue("StatusDisplayName", "");
            }
            set
            {
                SetValue("StatusDisplayName", value);
            }
        }


        /// <summary>
        /// Code name of the status.
        /// </summary>
        public virtual string StatusName
        {
            get
            {
                return GetStringValue("StatusName", "");
            }
            set
            {
                SetValue("StatusName", value);
            }
        }


        /// <summary>
        /// Status color in hex format (#xxxxxx).
        /// </summary>
        public virtual string StatusColor
        {
            get
            {
                return GetStringValue("StatusColor", "");
            }
            set
            {
                SetValue("StatusColor", value);
            }
        }


        /// <summary>
        /// Order status GUID.
        /// </summary>
        public virtual Guid StatusGUID
        {
            get
            {
                return GetGuidValue("StatusGUID", Guid.Empty);
            }
            set
            {
                SetValue("StatusGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Order status last modified.
        /// </summary>
        public virtual DateTime StatusLastModified
        {
            get
            {
                return GetDateTimeValue("StatusLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("StatusLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if notification should be send to the customer and administrator when order status is changed to this status.
        /// </summary>
        public virtual bool StatusSendNotification
        {
            get
            {
                return GetBooleanValue("StatusSendNotification", false);
            }
            set
            {
                SetValue("StatusSendNotification", value);
            }
        }


        /// <summary>
        /// Order status site ID. Set to 0 for global order status.
        /// </summary>
        public virtual int StatusSiteID
        {
            get
            {
                return GetIntegerValue("StatusSiteID", 0);
            }
            set
            {
                SetValue("StatusSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Order status is paid.
        /// </summary>
        public virtual bool StatusOrderIsPaid
        {
            get
            {
                return GetBooleanValue("StatusOrderIsPaid", false);
            }
            set
            {
                SetValue("StatusOrderIsPaid", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OrderStatusInfoProvider.DeleteOrderStatusInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OrderStatusInfoProvider.SetOrderStatusInfo(this);
        }


        /// <summary>
        /// Inserts cloned status to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Move clone to the end of list
            StatusOrder = GetLastObjectOrder(null);

            base.InsertAsCloneInternal(settings, result, originalObject);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OrderStatusInfo object.
        /// </summary>
        public OrderStatusInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderStatusInfo object from the given DataRow.
        /// </summary>
        public OrderStatusInfo(DataRow dr)
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
            return EcommercePermissions.CheckConfigurationPermissions(permission, siteName, userInfo, exceptionOnFailure, IsGlobal, base.CheckPermissionsInternal);
        }

        #endregion
    }
}