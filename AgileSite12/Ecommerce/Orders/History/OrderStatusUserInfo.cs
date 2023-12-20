using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(OrderStatusUserInfo), OrderStatusUserInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OrderStatusUserInfo data container class.
    /// </summary>
    public class OrderStatusUserInfo : AbstractInfo<OrderStatusUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.orderstatususer";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OrderStatusUserInfoProvider), OBJECT_TYPE, "ECommerce.OrderStatusUser", "OrderStatusUserID", null, null, null, null, null, null, "OrderID", OrderInfo.OBJECT_TYPE)
        {
            // Child object types
            // - None

            // Object dependencies
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FromStatusID", OrderStatusInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ToStatusID", OrderStatusInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ChangedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.RequiredHasDefault)
            },
            // Binding object types
            // - None

            // Others
            LogEvents = false,
            AllowTouchParent = false,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsCloning = false,
            ImportExportSettings = { LogExport = false },
            ContainsMacros = false
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// ID of the order.
        /// </summary>
        public virtual int OrderID
        {
            get
            {
                return GetIntegerValue("OrderID", 0);
            }
            set
            {
                SetValue("OrderID", value);
            }
        }


        /// <summary>
        /// To status ID.
        /// </summary>
        public virtual int ToStatusID
        {
            get
            {
                return GetIntegerValue("ToStatusID", 0);
            }
            set
            {
                SetValue("ToStatusID", value);
            }
        }


        /// <summary>
        /// Changed by user ID.
        /// </summary>
        public virtual int ChangedByUserID
        {
            get
            {
                return GetIntegerValue("ChangedByUserID", 0);
            }
            set
            {
                SetValue("ChangedByUserID", value, 0);
            }
        }


        /// <summary>
        /// Note of the order status.
        /// </summary>
        public virtual string Note
        {
            get
            {
                return GetStringValue("Note", "");
            }
            set
            {
                SetValue("Note", value);
            }
        }


        /// <summary>
        /// Date of the change.
        /// </summary>
        public virtual DateTime Date
        {
            get
            {
                return GetDateTimeValue("Date", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("Date", value);
            }
        }


        /// <summary>
        /// Order status user ID.
        /// </summary>
        public virtual int OrderStatusUserID
        {
            get
            {
                return GetIntegerValue("OrderStatusUserID", 0);
            }
            set
            {
                SetValue("OrderStatusUserID", value);
            }
        }


        /// <summary>
        /// From status ID.
        /// </summary>
        public virtual int FromStatusID
        {
            get
            {
                return GetIntegerValue("FromStatusID", 0);
            }
            set
            {
                SetValue("FromStatusID", value, 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OrderStatusUserInfoProvider.DeleteOrderStatusUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OrderStatusUserInfoProvider.SetOrderStatusUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OrderStatusUserInfo object.
        /// </summary>
        public OrderStatusUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderStatusUserInfo object from the given DataRow.
        /// </summary>
        public OrderStatusUserInfo(DataRow dr)
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
            return EcommercePermissions.CheckOrdersPermissions(permission, siteName, userInfo, exceptionOnFailure, base.CheckPermissionsInternal);
        }

        #endregion
    }
}