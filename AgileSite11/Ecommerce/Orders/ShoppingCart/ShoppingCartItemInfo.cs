using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Data;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ShoppingCartItemInfo), ShoppingCartItemInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Shopping cart item data container class.
    /// </summary>
    [Serializable]
    public class ShoppingCartItemInfo : AbstractInfo<ShoppingCartItemInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.shoppingcartitem";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ShoppingCartItemInfoProvider), OBJECT_TYPE, "ECommerce.ShoppingCartItem", "CartItemID", null, "CartItemGuid", null, null, null, null, "ShoppingCartID", ShoppingCartInfo.OBJECT_TYPE)
        {
            // Child object types
            // - None

            // Object dependencies
            DependsOn = new List<ObjectDependency> { new ObjectDependency("SKUID", SKUInfo.OBJECT_TYPE_SKU, ObjectDependencyEnum.Required) },
            // Binding object types
            // - None

            // Export
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
            SupportsCloning = false,
        };

        #endregion


        #region "Variables"

        // General
        private ShoppingCartInfo mShoppingCart;
        private SKUInfo mSKUInfo;

        // Bundles and product options
        private readonly List<ShoppingCartItemInfo> mProductOptions = new List<ShoppingCartItemInfo>();
        private readonly List<ShoppingCartItemInfo> mBundleItems = new List<ShoppingCartItemInfo>();
        private ContainerCustomData mItemCustomData;

        #endregion


        #region "Properties - Data class"

        /// <summary>
        /// Shopping cart item ID.
        /// </summary>
        [DatabaseField]
        public virtual int CartItemID
        {
            get
            {
                return GetIntegerValue("CartItemID", 0);
            }
            set
            {
                SetValue("CartItemID", value);
            }
        }


        /// <summary>
        /// Shopping cart item unique identifier. Shopping cart item means collection of the product and its selected product options.
        /// </summary>
        [DatabaseField]
        public virtual Guid CartItemGUID
        {
            get
            {
                return GetGuidValue("CartItemGUID", Guid.Empty);
            }
            set
            {
                SetValue("CartItemGUID", value);
            }
        }


        /// <summary>
        /// Unique identifier of the parent shopping cart item. When set current SKU is a product option, otherwise it is a product.
        /// </summary>
        [DatabaseField]
        public virtual Guid CartItemParentGUID
        {
            get
            {
                return GetGuidValue("CartItemParentGUID", Guid.Empty);
            }
            set
            {
                SetValue("CartItemParentGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Unique identifier of parent shopping cart item. If set, this shopping cart item is a bundle item.
        /// </summary>
        [DatabaseField]
        public virtual Guid CartItemBundleGUID
        {
            get
            {
                return GetGuidValue("CartItemBundleGUID", Guid.Empty);
            }
            set
            {
                SetValue("CartItemBundleGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Shopping cart item custom data.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public ContainerCustomData CartItemCustomData
        {
            get
            {
                return mItemCustomData ?? (mItemCustomData = new ContainerCustomData(this, "CartItemCustomData"));
            }
        }


        /// <summary>
        /// Shopping cart ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartID
        {
            get
            {
                int cartId = GetIntegerValue("ShoppingCartID", 0);

                if ((cartId == 0) && (ShoppingCart != null))
                {
                    // Load cart ID from cart
                    cartId = ShoppingCart.ShoppingCartID;
                    SetValue("ShoppingCartID", cartId);
                }

                return cartId;
            }
            set
            {
                SetValue("ShoppingCartID", value);
            }
        }


        /// <summary>
        /// SKU ID.
        /// </summary>
        [DatabaseField]
        public virtual int SKUID
        {
            get
            {
                return GetIntegerValue("SKUID", 0);
            }
            set
            {
                SetValue("SKUID", value);

                // Ensure consistency
                mSKUInfo = null;
            }
        }


        /// <summary>
        /// Amount of the current shopping cart item in the shopping cart. 
        /// </summary>
        [DatabaseField("SKUUnits")]
        public virtual int CartItemUnits
        {
            get
            {
                return GetIntegerValue("SKUUnits", 0);
            }
            set
            {
                SetValue("SKUUnits", value);
            }
        }


        /// <summary> 
        /// It is set only when cart item represents e-product and when the cart item was loaded from the existing e-product order item. 
        /// Then it is date and time until the e-product download links are valid.
        /// </summary>
        [DatabaseField]
        public DateTime CartItemValidTo
        {
            get
            {
                return GetDateTimeValue("CartItemValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CartItemValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// The number of shopping cart item units added automatically to the shopping cart for free.
        /// </summary>
        [DatabaseField]
        public virtual int CartItemAutoAddedUnits
        {
            get
            {
                return GetIntegerValue("CartItemAutoAddedUnits", 0);
            }
            set
            {
                SetValue("CartItemAutoAddedUnits", value);
            }
        }


        /// <summary>
        /// It is set only when cart item represents text product option. 
        /// Then it is the text defined by the customer, e.g. custom label for a T-shirt.
        /// </summary>
        [DatabaseField]
        public virtual string CartItemText
        {
            get
            {
                return GetStringValue("CartItemText", "");
            }
            set
            {
                SetValue("CartItemText", value);
            }
        }

        #endregion


        #region "Properties - Advanced"

        /// <summary>
        /// Order item data. Loaded when the shopping cart is created from an existing order, otherwise it is null.
        /// </summary>
        public OrderItemInfo OrderItem
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the summary of the discounts applied on each unit of this item.
        /// </summary>
        [RegisterProperty]
        public virtual ValuesSummary UnitDiscountSummary
        {
            get;
            set;
        }


        /// <summary>
        /// Unit price of the shopping cart item in shopping cart currency. Catalog-level discounts are reflected.
        /// </summary>
        /// <seealso cref="UnitDiscountSummary"/>
        /// <seealso cref="UnitTotalDiscount"/>
        [RegisterProperty]
        public virtual decimal UnitPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the total price of one unit including options.
        /// </summary>        
        [RegisterProperty]
        public virtual decimal UnitTotalPriceIncludingOptions
        {
            get
            {
                return UnitPrice
                    + ProductOptions.Select(option => option.UnitPrice).Sum();
            }
        }


        /// <summary>
        /// Unit discount applied on the shopping cart item.
        /// </summary>
        /// <remarks>
        /// The value is expressed in the shopping cart currency and includes catalog-level discounts (product coupons, volume and catalog discounts).
        /// For summary of catalog-level discounts see <see cref="UnitDiscountSummary"/>.
        /// </remarks>
        /// <seealso cref="TotalDiscount"/>
        [RegisterProperty]
        public virtual decimal UnitTotalDiscount
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the total price including options.
        /// </summary>        
        [RegisterProperty]
        public virtual decimal TotalPriceIncludingOptions
        {
            get
            {
                return TotalPrice + ProductOptions.Select(option => option.TotalPrice).Sum();
            }
        }


        /// <summary>
        /// Total price of the shopping cart item in shopping cart currency.
        /// </summary>
        [RegisterProperty]
        public virtual decimal TotalPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Total discount applied on the shopping cart item.
        /// </summary>
        /// <remarks>
        /// The value is expressed in the shopping cart currency and does not include catalog-level discounts (product coupons, volume and catalog discounts).
        /// </remarks>
        /// <seealso cref="UnitTotalDiscount"/>
        [RegisterProperty]
        public virtual decimal TotalDiscount
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the summary of the discounts applied on this item.
        /// </summary>
        [RegisterProperty]
        public virtual ValuesSummary DiscountSummary
        {
            get;
            set;
        }


        /// <summary>
        /// Unit weight of the shopping cart item.
        /// </summary>
        [RegisterProperty]
        public virtual double UnitWeight
        {
            get
            {
                return ShoppingCartItemInfoProvider.CalculateUnitWeight(this);
            }
        }


        /// <summary>
        /// Total weight of the shopping cart item.
        /// </summary>
        [RegisterProperty]
        public virtual double TotalWeight
        {
            get
            {
                return ShoppingCartItemInfoProvider.CalculateTotalWeight(this);
            }
        }


        /// <summary>
        /// Data of the shopping cart to which the shopping cart item belongs.
        /// </summary>
        public virtual ShoppingCartInfo ShoppingCart
        {
            get
            {
                return mShoppingCart ?? (mShoppingCart = ECommerceContext.CurrentShoppingCart);
            }
            set
            {
                mShoppingCart = value;
            }
        }


        /// <summary>
        /// Data of the SKU (product) which is represented by the shopping cart item.
        /// </summary>
        public virtual SKUInfo SKU
        {
            get
            {
                if ((mSKUInfo == null) || !mSKUInfo.Generalized.IsObjectValid)
                {
                    mSKUInfo = SKUInfoProvider.GetSKUInfo(SKUID);
                }

                return mSKUInfo;
            }
            set
            {
                mSKUInfo = value;

                // Ensure consistency
                if (mSKUInfo != null)
                {
                    SetValue("SKUID", mSKUInfo.SKUID);
                }
            }
        }


        /// <summary>
        /// List with product options of the shopping cart item.
        /// </summary>
        public virtual List<ShoppingCartItemInfo> ProductOptions
        {
            get
            {
                return mProductOptions;
            }
        }


        /// <summary>
        /// Gets a value indicating whether is accessory product.
        /// </summary>        
        public bool IsAccessoryProduct
        {
            get
            {
                return SKU.IsAccessoryProduct;
            }
        }


        /// <summary>
        /// Gets a value indicating whether is text attribute.
        /// </summary>        
        public bool IsTextAttribute
        {
            get
            {
                return SKU.IsTextAttribute;
            }
        }


        /// <summary>
        /// Gets a value indicating whether is attribute option (e.g. product color).
        /// </summary>        
        public bool IsAttributeOption
        {
            get
            {
                return SKU.IsAttributeOption;
            }
        }


        /// <summary>
        /// Indicates if shopping cart item is representing product or product option.
        /// </summary>
        public bool IsProductOption
        {
            get
            {
                return CartItemParentGUID != Guid.Empty;
            }
        }


        /// <summary>
        /// List of bundle items of the shopping cart item.
        /// </summary>
        public List<ShoppingCartItemInfo> BundleItems
        {
            get
            {
                return mBundleItems;
            }
        }


        /// <summary>
        /// Indicates if shopping cart item is representing bundle item.
        /// </summary>
        public bool IsBundleItem
        {
            get
            {
                return CartItemBundleGUID != Guid.Empty;
            }
        }


        /// <summary>
        /// Shopping cart item which represents product in which the current product option is included. 
        /// If the current item is not product option, returns null.
        /// </summary>
        [RegisterProperty]
        public ShoppingCartItemInfo ParentProduct
        {
            get
            {
                return IsProductOption ? ShoppingCartInfoProvider.GetShoppingCartItem(ShoppingCart, CartItemParentGUID) : null;
            }
        }


        /// <summary>
        /// In case the shopping cart item is a variant, returns its parent product.
        /// If the current item is not variant, returns null;
        /// </summary>
        public SKUInfo VariantParent
        {
            get
            {
                return SKU.IsProductVariant ? SKUInfoProvider.GetSKUInfo(SKU.SKUParentSKUID) : null;
            }
        }


        /// <summary>
        /// Shopping cart item which represents bundle product in which the current bundle item is included. 
        /// If the current item is not bundle item, returns null.
        /// </summary>
        public ShoppingCartItemInfo ParentBundle
        {
            get
            {
                return IsBundleItem ? ShoppingCartInfoProvider.GetShoppingCartItem(ShoppingCart, CartItemBundleGUID) : null;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ShoppingCartItemInfoProvider.DeleteShoppingCartItemInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ShoppingCartItemInfoProvider.SetShoppingCartItemInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ShoppingCartItemInfo object.
        /// </summary>
        public ShoppingCartItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ShoppingCartItemInfo object from the given DataRow.
        /// </summary>
        public ShoppingCartItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            // Create unique identifier of a new shopping cart item
            CartItemGUID = Guid.NewGuid();
        }


        /// <summary>
        /// Constructor - Creates a new ShoppingCartItemInfo object from serialized data.
        /// </summary>
        public ShoppingCartItemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
            // General
            mShoppingCart = (ShoppingCartInfo)info.GetValue(nameof(ShoppingCart), typeof(ShoppingCartInfo));
            mSKUInfo = (SKUInfo)info.GetValue(nameof(SKU), typeof(SKUInfo));
            OrderItem = (OrderItemInfo)info.GetValue(nameof(OrderItem), typeof(OrderItemInfo));

            // Bundles and product options
            mProductOptions = (List<ShoppingCartItemInfo>)info.GetValue(nameof(ProductOptions), typeof(List<ShoppingCartItemInfo>));
            mBundleItems = (List<ShoppingCartItemInfo>)info.GetValue(nameof(BundleItems), typeof(List<ShoppingCartItemInfo>));
            mItemCustomData = (ContainerCustomData)info.GetValue(nameof(CartItemCustomData), typeof(ContainerCustomData));
            CartItemParentGUID = (Guid)info.GetValue(nameof(CartItemParentGUID), typeof(Guid));
            CartItemBundleGUID = (Guid)info.GetValue(nameof(CartItemBundleGUID), typeof(Guid));

            // Discount
            UnitDiscountSummary = (ValuesSummary)info.GetValue(nameof(UnitDiscountSummary), typeof(ValuesSummary));
            TotalDiscount = (decimal)info.GetValue(nameof(TotalDiscount), typeof(decimal));
            DiscountSummary = (ValuesSummary)info.GetValue(nameof(DiscountSummary), typeof(ValuesSummary));

            // Per unit
            UnitTotalDiscount = (decimal)info.GetValue(nameof(UnitTotalDiscount), typeof(decimal));

            // Totals
            TotalPrice = (decimal)info.GetValue(nameof(TotalPrice), typeof(decimal));
            UnitPrice = (decimal)info.GetValue(nameof(UnitPrice), typeof(decimal));
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ShoppingCart), mShoppingCart);
            info.AddValue(nameof(SKU), mSKUInfo);
            info.AddValue(nameof(OrderItem), OrderItem);
            info.AddValue(nameof(ProductOptions), ProductOptions);
            info.AddValue(nameof(BundleItems), BundleItems);
            info.AddValue(nameof(CartItemCustomData), CartItemCustomData);
            info.AddValue(nameof(CartItemParentGUID), CartItemParentGUID);
            info.AddValue(nameof(CartItemBundleGUID), CartItemBundleGUID);
            info.AddValue(nameof(UnitDiscountSummary), UnitDiscountSummary);
            info.AddValue(nameof(TotalDiscount), TotalDiscount);
            info.AddValue(nameof(DiscountSummary), DiscountSummary);
            info.AddValue(nameof(UnitTotalDiscount), UnitTotalDiscount);
            info.AddValue(nameof(TotalPrice), TotalPrice);
            info.AddValue(nameof(UnitPrice), UnitPrice);
        }

        #endregion
    }
}
