using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(PaymentOptionInfo), PaymentOptionInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// PaymentOptionInfo data container class.
    /// </summary>
    [Serializable]
    public class PaymentOptionInfo : AbstractInfo<PaymentOptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.paymentoption";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PaymentOptionInfoProvider), OBJECT_TYPE, "ECommerce.PaymentOption", "PaymentOptionID", "PaymentOptionLastModified", "PaymentOptionGUID", "PaymentOptionName", "PaymentOptionDisplayName", null, "PaymentOptionSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("PaymentOptionSucceededOrderStatusID", OrderStatusInfo.OBJECT_TYPE),
                new ObjectDependency("PaymentOptionAuthorizedOrderStatusID", OrderStatusInfo.OBJECT_TYPE),
                new ObjectDependency("PaymentOptionFailedOrderStatusID", OrderStatusInfo.OBJECT_TYPE)
            },
            // Binding object types
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
            ThumbnailGUIDColumn = "PaymentOptionThumbnailGUID",
            HasMetaFiles = true,
            NameGloballyUnique = true,
            SupportsInvalidation = true,
            AssemblyNameColumn = "PaymentOptionAssemblyName",
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
            EnabledColumn = "PaymentOptionEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled  = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// ID of the payment option.
        /// </summary>
        public virtual int PaymentOptionID
        {
            get
            {
                return GetIntegerValue("PaymentOptionID", 0);
            }
            set
            {
                SetValue("PaymentOptionID", value);
            }
        }


        /// <summary>
        /// Code name of the payment option.
        /// </summary>
        public virtual string PaymentOptionName
        {
            get
            {
                return GetStringValue("PaymentOptionName", "");
            }
            set
            {
                SetValue("PaymentOptionName", value);
            }
        }


        /// <summary>
        /// Payment option site ID. Set to 0 for global payment option.
        /// </summary>
        public virtual int PaymentOptionSiteID
        {
            get
            {
                return GetIntegerValue("PaymentOptionSiteID", 0);
            }
            set
            {
                SetValue("PaymentOptionSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Indicates whether the payment option is enabled or not.
        /// </summary>
        public virtual bool PaymentOptionEnabled
        {
            get
            {
                return GetBooleanValue("PaymentOptionEnabled", false);
            }
            set
            {
                SetValue("PaymentOptionEnabled", value);
            }
        }


        /// <summary>
        /// Payment option - Payment gate url.
        /// </summary>
        public string PaymentOptionPaymentGateUrl
        {
            get
            {
                return GetStringValue("PaymentOptionPaymentGateUrl", "");
            }
            set
            {
                SetValue("PaymentOptionPaymentGateUrl", value);
            }
        }


        /// <summary>
        /// Code name of the payment option.
        /// </summary>
        public virtual string PaymentOptionDisplayName
        {
            get
            {
                return GetStringValue("PaymentOptionDisplayName", "");
            }
            set
            {
                SetValue("PaymentOptionDisplayName", value);
            }
        }


        /// <summary>
        /// Payment option description.
        /// </summary>
        public virtual string PaymentOptionDescription
        {
            get
            {
                return GetStringValue("PaymentOptionDescription", "");
            }
            set
            {
                SetValue("PaymentOptionDescription", value);
            }
        }


        /// <summary>
        /// Payment gateway assembly name.
        /// </summary>
        public virtual string PaymentOptionAssemblyName
        {
            get
            {
                return GetStringValue("PaymentOptionAssemblyName", "");
            }
            set
            {
                SetValue("PaymentOptionAssemblyName", value);
            }
        }


        /// <summary>
        /// Payment gateway class name.
        /// </summary>
        public virtual string PaymentOptionClassName
        {
            get
            {
                return GetStringValue("PaymentOptionClassName", "");
            }
            set
            {
                SetValue("PaymentOptionClassName", value);
            }
        }


        /// <summary>
        /// ID of the status which is considered to be "payment successful" status.
        /// </summary>
        public virtual int PaymentOptionSucceededOrderStatusID
        {
            get
            {
                return GetIntegerValue("PaymentOptionSucceededOrderStatusID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("PaymentOptionSucceededOrderStatusID", null);
                }
                else
                {
                    SetValue("PaymentOptionSucceededOrderStatusID", value);
                }
            }
        }


        /// <summary>
        /// ID of the status which is considered to be "payment authorized" status.
        /// </summary>
        public virtual int PaymentOptionAuthorizedOrderStatusID
        {
            get
            {
                return GetIntegerValue("PaymentOptionAuthorizedOrderStatusID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("PaymentOptionAuthorizedOrderStatusID", null);
                }
                else
                {
                    SetValue("PaymentOptionAuthorizedOrderStatusID", value);
                }
            }
        }


        /// <summary>
        /// ID of the status which is considered to be "payment failed" status.
        /// </summary>
        public virtual int PaymentOptionFailedOrderStatusID
        {
            get
            {
                return GetIntegerValue("PaymentOptionFailedOrderStatusID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("PaymentOptionFailedOrderStatusID", null);
                }
                else
                {
                    SetValue("PaymentOptionFailedOrderStatusID", value);
                }
            }
        }


        /// <summary>
        /// Payment option GUID.
        /// </summary>
        public virtual Guid PaymentOptionGUID
        {
            get
            {
                return GetGuidValue("PaymentOptionGUID", Guid.Empty);
            }
            set
            {
                SetValue("PaymentOptionGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime PaymentOptionLastModified
        {
            get
            {
                return GetDateTimeValue("PaymentOptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PaymentOptionLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Allow payment option to be selected if order does not need shipping.
        /// </summary>
        public bool PaymentOptionAllowIfNoShipping
        {
            get
            {
                return GetBooleanValue("PaymentOptionAllowIfNoShipping", false);
            }
            set
            {
                SetValue("PaymentOptionAllowIfNoShipping", value);
            }
        }


        /// <summary>
        /// Payment option thumbnail metafile GUID.
        /// </summary>
        public virtual Guid PaymentOptionThumbnailGUID
        {
            get
            {
                return GetGuidValue("PaymentOptionThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("PaymentOptionThumbnailGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public PaymentOptionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty PaymentOptionInfo object.
        /// </summary>
        public PaymentOptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PaymentOptionInfo object from the given DataRow.
        /// </summary>
        public PaymentOptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PaymentOptionInfoProvider.DeletePaymentOptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PaymentOptionInfoProvider.SetPaymentOptionInfo(this);
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