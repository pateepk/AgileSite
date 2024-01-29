using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(ShippingOptionInfo), ShippingOptionInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// ShippingOptionInfo data container class.
    /// </summary>
    [Serializable]
    public class ShippingOptionInfo : AbstractInfo<ShippingOptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.shippingoption";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ShippingOptionInfoProvider), OBJECT_TYPE, "Ecommerce.ShippingOption", "ShippingOptionID", "ShippingOptionLastModified", "ShippingOptionGUID", "ShippingOptionName", "ShippingOptionDisplayName", null, "ShippingOptionSiteID", null, null)
        {
            // Object dependencies
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ShippingOptionCarrierID", CarrierInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("ShippingOptionTaxClassID", TaxClassInfo.OBJECT_TYPE)
            },

            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                }
            },

            // Others
            LogEvents = true,
            CheckDependenciesOnDelete = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            ThumbnailGUIDColumn = "ShippingOptionThumbnailGUID",
            HasMetaFiles = true,
            SupportsInvalidation = true,
            SupportsCloneToOtherSite = false,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                },
            },
            EnabledColumn = "ShippingOptionEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        private string mShippingOptionServiceDisplayName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Shipping option site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ShippingOptionSiteID
        {
            get
            {
                return GetIntegerValue("ShippingOptionSiteID", 0);
            }
            set
            {
                SetValue("ShippingOptionSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// ID of shipping carrier used by this shipping option for shipping price calculation.
        /// </summary>
        [DatabaseField]
        public virtual int ShippingOptionCarrierID
        {
            get
            {
                return GetIntegerValue("ShippingOptionCarrierID", 0);
            }
            set
            {
                SetValue("ShippingOptionCarrierID", value, (value > 0));
            }
        }


        /// <summary>
        /// Name (code) of shipping carrier service used by shipping option.
        /// </summary>
        [DatabaseField]
        public virtual string ShippingOptionCarrierServiceName
        {
            get
            {
                return GetStringValue("ShippingOptionCarrierServiceName", "");
            }
            set
            {
                SetValue("ShippingOptionCarrierServiceName", value);
            }
        }


        /// <summary>
        /// Shipping option display name.
        /// </summary>
        [DatabaseField]
        public virtual string ShippingOptionDisplayName
        {
            get
            {
                return GetStringValue("ShippingOptionDisplayName", "");
            }
            set
            {
                SetValue("ShippingOptionDisplayName", value);
            }
        }


        /// <summary>
        /// Shipping option description.
        /// </summary>
        [DatabaseField]
        public virtual string ShippingOptionDescription
        {
            get
            {
                return GetStringValue("ShippingOptionDescription", "");
            }
            set
            {
                SetValue("ShippingOptionDescription", value);
            }
        }


        /// <summary>
        /// ID of the shipping option.
        /// </summary>
        [DatabaseField]
        public virtual int ShippingOptionID
        {
            get
            {
                return GetIntegerValue("ShippingOptionID", 0);
            }
            set
            {
                SetValue("ShippingOptionID", value);
            }
        }


        /// <summary>
        /// Code name of the shipping option.
        /// </summary>
        [DatabaseField]
        public virtual string ShippingOptionName
        {
            get
            {
                return GetStringValue("ShippingOptionName", "");
            }
            set
            {
                SetValue("ShippingOptionName", value);
            }
        }

        /// <summary>
        /// Shipping option tax class ID
        /// </summary>
        [DatabaseField]
        public virtual int ShippingOptionTaxClassID
        {
            get
            {
                return GetIntegerValue("ShippingOptionTaxClassID", 0);
            }
            set
            {
                SetValue("ShippingOptionTaxClassID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates whether the shipping option is enabled or not.
        /// </summary>
        [DatabaseField]
        public virtual bool ShippingOptionEnabled
        {
            get
            {
                return GetBooleanValue("ShippingOptionEnabled", false);
            }
            set
            {
                SetValue("ShippingOptionEnabled", value);
            }
        }


        /// <summary>
        /// ShippingOption GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ShippingOptionGUID
        {
            get
            {
                return GetGuidValue("ShippingOptionGUID", Guid.Empty);
            }
            set
            {
                SetValue("ShippingOptionGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ShippingOptionLastModified
        {
            get
            {
                return GetDateTimeValue("ShippingOptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ShippingOptionLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Shipping option thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ShippingOptionThumbnailGUID
        {
            get
            {
                return GetGuidValue("ShippingOptionThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("ShippingOptionThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Shipping option carrier service display name.
        /// </summary>
        [RegisterProperty]
        public virtual string ShippingOptionServiceDisplayName
        {
            get
            {
                if (String.IsNullOrEmpty(mShippingOptionServiceDisplayName))
                {
                    var provider = CarrierInfoProvider.GetCarrierProvider(ShippingOptionCarrierID);
                    if (provider == null)
                    {
                        return null;
                    }

                    // Get service display name from provider
                    var services = provider.GetServices();
                    if (services != null)
                    {
                        mShippingOptionServiceDisplayName = services.FirstOrDefault(s => s.Key.Equals(ShippingOptionCarrierServiceName, StringComparison.InvariantCultureIgnoreCase)).Value;
                    }
                }

                return mShippingOptionServiceDisplayName;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ShippingOptionInfoProvider.DeleteShippingOptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ShippingOptionInfoProvider.SetShippingOptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ShippingOptionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ShippingOptionInfo object.
        /// </summary>
        public ShippingOptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ShippingOptionInfo object from the given DataRow.
        /// </summary>
        public ShippingOptionInfo(DataRow dr)
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