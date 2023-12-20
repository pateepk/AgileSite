using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ShoppingCartCouponCodeInfo), ShoppingCartCouponCodeInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="ShoppingCartCouponCodeInfo"/>.
    /// </summary>
	[Serializable]
    public class ShoppingCartCouponCodeInfo : AbstractInfo<ShoppingCartCouponCodeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.shoppingcartcouponcode";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ShoppingCartCouponCodeInfoProvider), OBJECT_TYPE, "Ecommerce.ShoppingCartCouponCode", "ShoppingCartCouponCodeID", null, null, null, null, null, null, "ShoppingCartID", ShoppingCartInfo.OBJECT_TYPE)
        {
            // Child object types
            // - None

            // Object dependencies
            // - None

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


        #region "Properties"

        /// <summary>
        /// Shopping cart coupon code ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShoppingCartCouponCodeID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ShoppingCartCouponCodeID"), 0);
            }
            set
            {
                SetValue("ShoppingCartCouponCodeID", value);
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
                return ValidationHelper.GetInteger(GetValue("ShoppingCartID"), 0);
            }
            set
            {
                SetValue("ShoppingCartID", value);
            }
        }


        /// <summary>
        /// Shopping cart coupon code code.
        /// </summary>
        [DatabaseField]
        public virtual string CouponCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CouponCode"), String.Empty);
            }
            set
            {
                SetValue("CouponCode", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ShoppingCartCouponCodeInfoProvider.DeleteShoppingCartCouponCodeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ShoppingCartCouponCodeInfoProvider.SetShoppingCartCouponCodeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ShoppingCartCouponCodeInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ShoppingCartCouponCodeInfo"/> class.
        /// </summary>
        public ShoppingCartCouponCodeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ShoppingCartCouponCodeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ShoppingCartCouponCodeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}