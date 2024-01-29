using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Security;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(OrderItemInfo), OrderItemInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OrderItemInfo data container class.
    /// </summary>
    [Serializable]
    public class OrderItemInfo : AbstractInfo<OrderItemInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.orderitem";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OrderItemInfoProvider), OBJECT_TYPE, "ECommerce.OrderItem", "OrderItemID", "OrderItemLastModified", "OrderItemGUID", null, "OrderItemSKUName", null, null, "OrderItemOrderID", OrderInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("OrderItemSKUID", SKUInfo.OBJECT_TYPE_SKU, ObjectDependencyEnum.Required) },

                                                  // Binding object types
                                                  // - None

                                                  // Synchronization
                                                  // - None

                                                  // Others
                                                  LogEvents = false,
                                                  AllowTouchParent = false,
                                                  TouchCacheDependencies = true,
                                                  AllowRestore = false,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  SupportsVersioning = false,
                                                  SupportsCloning = false,
                                                  ImportExportSettings = { LogExport = false }
                                              };

        #endregion


        #region "Variables"

        private SKUInfo mOrderItemSKU;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Order item custom data.
        /// </summary>
        protected ContainerCustomData mOrderItemCustomData = null;


        /// <summary>
        /// Price of the item per unit, corresponds to the price of the <see cref="ShoppingCartItemInfo.UnitPrice"/>.
        /// </summary>
        public virtual decimal OrderItemUnitPrice
        {
            get
            {
                return GetDecimalValue("OrderItemUnitPrice", 0.0m);
            }
            set
            {
                SetValue("OrderItemUnitPrice", value);
            }
        }


        /// <summary>
        /// Total item price. Corresponds with the <see cref="ShoppingCartItemInfo.TotalPrice"/>.
        /// </summary>
        public virtual decimal OrderItemTotalPrice
        {
            get
            {
                return GetDecimalValue("OrderItemTotalPrice", 0.0m);
            }
            set
            {
                SetValue("OrderItemTotalPrice", value);
            }
        }


        /// <summary>
        /// Total order item price in main currency.
        /// </summary>
        public virtual decimal OrderItemTotalPriceInMainCurrency
        {
            get
            {
                return GetDecimalValue("OrderItemTotalPriceInMainCurrency", 0.0m);
            }
            set
            {
                SetValue("OrderItemTotalPriceInMainCurrency", value);
            }
        }


        /// <summary>
        /// ID of the order.
        /// </summary>
        public virtual int OrderItemOrderID
        {
            get
            {
                return GetIntegerValue("OrderItemOrderID", 0);
            }
            set
            {
                SetValue("OrderItemOrderID", value);
            }
        }


        /// <summary>
        /// ID of the order item.
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
        /// Name of the order item.
        /// </summary>
        public virtual string OrderItemSKUName
        {
            get
            {
                return GetStringValue("OrderItemSKUName", "");
            }
            set
            {
                SetValue("OrderItemSKUName", value);
            }
        }


        /// <summary>
        /// ID of the SKU for this order item.
        /// </summary>
        public virtual int OrderItemSKUID
        {
            get
            {
                return GetIntegerValue("OrderItemSKUID", 0);
            }
            set
            {
                SetValue("OrderItemSKUID", value);
                mOrderItemSKU = null;
            }
        }


        /// <summary>
        /// Number of units within the order.
        /// </summary>
        public virtual int OrderItemUnitCount
        {
            get
            {
                return GetIntegerValue("OrderItemUnitCount", 0);
            }
            set
            {
                SetValue("OrderItemUnitCount", value);
            }
        }


        /// <summary>
        /// Order item unique identifier. Order item means collection of the product and its selected product options.
        /// </summary>
        public virtual Guid OrderItemGUID
        {
            get
            {
                return GetGuidValue("OrderItemGUID", Guid.Empty);
            }
            set
            {
                SetValue("OrderItemGUID", value);
            }
        }


        /// <summary>
        /// Unique identifier of the parent order item. When set current product is a product option, otherwise it is a product.
        /// </summary>
        public virtual Guid OrderItemParentGUID
        {
            get
            {
                return GetGuidValue("OrderItemParentGUID", Guid.Empty);
            }
            set
            {
                SetValue("OrderItemParentGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Unique identifier of parent bundle product. If set, this product is a bundle item.
        /// </summary>
        public virtual Guid OrderItemBundleGUID
        {
            get
            {
                return GetGuidValue("OrderItemBundleGUID", Guid.Empty);
            }
            set
            {
                SetValue("OrderItemBundleGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Order item custom data.
        /// </summary>
        public ContainerCustomData OrderItemCustomData
        {
            get
            {
                return mOrderItemCustomData ?? (mOrderItemCustomData = new ContainerCustomData(this, "OrderItemCustomData"));
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime OrderItemLastModified
        {
            get
            {
                return GetDateTimeValue("OrderItemLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("OrderItemLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }



        /// <summary>
        /// Order item valid to date and time.
        /// </summary>
        public virtual DateTime OrderItemValidTo
        {
            get
            {
                return GetDateTimeValue("OrderItemValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("OrderItemValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if customer is to be notified in advance about expiration of his order item.
        /// </summary>
        public bool OrderItemSendNotification
        {
            get
            {
                return GetBooleanValue("OrderItemSendNotification", false);
            }
            set
            {
                SetValue("OrderItemSendNotification", value);
            }
        }


        /// <summary>
        /// Associated SKU object.
        /// </summary>
        public SKUInfo OrderItemSKU
        {
            get
            {
                return mOrderItemSKU ?? (mOrderItemSKU = SKUInfoProvider.GetSKUInfo(OrderItemSKUID));
            }
            internal set
            {
                mOrderItemSKU = value;
            }
        }


        /// <summary>
        /// Text assigned to the order item.
        /// </summary>
        public virtual string OrderItemText
        {
            get
            {
                return GetStringValue("OrderItemText", "");
            }
            set
            {
                SetValue("OrderItemText", value);
            }
        }


        /// <summary>
        /// Xml content with the summary of the catalog-level discounts which were applied to the shopping cart item. 
        /// Contains whole DiscountsTable from shopping cart item.
        /// </summary>
        public virtual string OrderItemProductDiscounts
        {
            get
            {
                return GetStringValue("OrderItemProductDiscounts", "");
            }
            set
            {
                SetValue("OrderItemProductDiscounts", value);
            }
        }


        /// <summary>
        /// Xml content with the summary of the discounts which were applied to the whole shopping cart item. 
        /// Contains whole <see cref="ShoppingCartItemInfo.DiscountSummary"/> from the <see cref="ShoppingCartItemInfo"/>.
        /// </summary>
        public virtual string OrderItemDiscountSummary
        {
            get
            {
                return GetStringValue("OrderItemDiscountSummary", "");
            }
            set
            {
                SetValue("OrderItemDiscountSummary", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OrderItemInfoProvider.DeleteOrderItemInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OrderItemInfoProvider.SetOrderItemInfo(this);
        }

        #endregion


        #region "Constuctor"

        /// <summary>
        /// Constructor - Creates an empty OrderItemInfo object.
        /// </summary>
        public OrderItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderItemInfo object from the given DataRow.
        /// </summary>
        public OrderItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderItemInfo object from serialized data.
        /// </summary>        
        /// <param name="info">Serialization data</param>
        /// <param name="context">Context</param> 
        public OrderItemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
            mOrderItemCustomData = (ContainerCustomData)info.GetValue("OrderItemCustomData", typeof(ContainerCustomData));
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("OrderItemCustomData", mOrderItemCustomData);
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


        #region "Overrides"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("OrderItemCustomData", m => m.OrderItemCustomData);
        }

        #endregion
    }
}