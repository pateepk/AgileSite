using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(OrderItemSKUFileInfo), OrderItemSKUFileInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OrderItemSKUFileInfo data container class.
    /// </summary>
    public class OrderItemSKUFileInfo : AbstractInfo<OrderItemSKUFileInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.orderitemskufile";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OrderItemSKUFileInfoProvider), OBJECT_TYPE, "ECommerce.OrderItemSKUFile", "OrderItemSKUFileID", null, "Token", null, null, null, null, "OrderItemID", OrderItemInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("FileID", SKUFileInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  SupportsCloning = false,
                                                  ImportExportSettings = { LogExport = false }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Unique download identifier.
        /// </summary>
        public virtual Guid Token
        {
            get
            {
                return GetGuidValue("Token", Guid.Empty);
            }
            set
            {
                SetValue("Token", value);
            }
        }


        /// <summary>
        /// Parent order item ID.
        /// </summary>
        public virtual int OrderItemID
        {
            get
            {
                return GetIntegerValue("OrderItemID", 0);
            }
            set
            {
                SetValue("OrderItemID", value);
            }
        }


        /// <summary>
        /// Order item SKU file ID.
        /// </summary>
        public virtual int OrderItemSKUFileID
        {
            get
            {
                return GetIntegerValue("OrderItemSKUFileID", 0);
            }
            set
            {
                SetValue("OrderItemSKUFileID", value);
            }
        }


        /// <summary>
        /// Associated SKU file ID.
        /// </summary>
        public virtual int FileID
        {
            get
            {
                return GetIntegerValue("FileID", 0);
            }
            set
            {
                SetValue("FileID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OrderItemSKUFileInfoProvider.DeleteOrderItemSKUFileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OrderItemSKUFileInfoProvider.SetOrderItemSKUFileInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OrderItemSKUFileInfo object.
        /// </summary>
        public OrderItemSKUFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderItemSKUFileInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public OrderItemSKUFileInfo(DataRow dr)
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