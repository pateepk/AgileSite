using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(WishlistItemInfo), WishlistItemInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// WishlistItemInfo data container class.
    /// </summary>
    public class WishlistItemInfo : AbstractInfo<WishlistItemInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.wishlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WishlistItemInfoProvider), OBJECT_TYPE, "Ecommerce.Wishlist", null, null, null, null, null, null, "SiteID", "UserID", UserInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("SKUID", SKUInfo.OBJECT_TYPE_SKU, ObjectDependencyEnum.Binding) },
                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = false,
                                                  SupportsInvalidation = true,
                                                  TouchCacheDependencies = true,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  RegisterAsOtherBindingToObjectTypes = new List<string>() { SKUInfo.OBJECT_TYPE_SKU, SKUInfo.OBJECT_TYPE_OPTIONSKU },
                                                  ImportExportSettings = { LogExport = false },
                                                  SynchronizationSettings =
                                                  {
                                                      IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None
                                                  },
                                                  IsSiteBinding = false
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// User ID.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// SKU ID.
        /// </summary>
        public virtual int SKUID
        {
            get
            {
                return GetIntegerValue("SKUID", 0);
            }
            set
            {
                SetValue("SKUID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WishlistItemInfoProvider.DeleteWishlistItemInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WishlistItemInfoProvider.SetWishlistItemInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WishlistItemInfo object.
        /// </summary>
        public WishlistItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WishlistItemInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WishlistItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}