using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(CreditEventInfo), CreditEventInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// CreditEventInfo data container class.
    /// </summary>
    public class CreditEventInfo : AbstractInfo<CreditEventInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.creditevent";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CreditEventInfoProvider), OBJECT_TYPE, "ECommerce.CreditEvent", "EventID", "EventCreditLastModified", "EventCreditGUID", null, "EventName", null, "EventSiteID", "EventCustomerID", CustomerInfo.OBJECT_TYPE)
        {
            // Child object types
            // - None

            // Object dependencies
            // - None

            // Binding object types
            // - None

            // Others
            LogEvents = false,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsCloning = false,
            SupportsGlobalObjects = true,
            SupportsCloneToOtherSite = false,
            ImportExportSettings = { LogExport = false },
            ContainsMacros = false
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Event name.
        /// </summary>
        public virtual string EventName
        {
            get
            {
                return GetStringValue("EventName", "");
            }
            set
            {
                SetValue("EventName", value);
            }
        }


        /// <summary>
        /// ID of the customer the event belongs to.
        /// </summary>
        public virtual int EventCustomerID
        {
            get
            {
                return GetIntegerValue("EventCustomerID", 0);
            }
            set
            {
                SetValue("EventCustomerID", value);
            }
        }


        /// <summary>
        /// Event ID.
        /// </summary>
        public virtual int EventID
        {
            get
            {
                return GetIntegerValue("EventID", 0);
            }
            set
            {
                SetValue("EventID", value);
            }
        }


        /// <summary>
        /// Event description.
        /// </summary>
        public virtual string EventDescription
        {
            get
            {
                return GetStringValue("EventDescription", "");
            }
            set
            {
                SetValue("EventDescription", value);
            }
        }


        /// <summary>
        /// Event date.
        /// </summary>
        public virtual DateTime EventDate
        {
            get
            {
                return GetDateTimeValue("EventDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EventDate", value);
            }
        }


        /// <summary>
        /// Event credit change.
        /// </summary>
        public virtual decimal EventCreditChange
        {
            get
            {
                return GetDecimalValue("EventCreditChange", 0.0m);
            }
            set
            {
                SetValue("EventCreditChange", value);
            }
        }


        /// <summary>
        /// Event credit GUID.
        /// </summary>
        public virtual Guid EventCreditGUID
        {
            get
            {
                return GetGuidValue("EventCreditGUID", Guid.Empty);
            }
            set
            {
                SetValue("EventCreditGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Date and time when the credit event was last modified.
        /// </summary>
        public virtual DateTime EventCreditLastModified
        {
            get
            {
                return GetDateTimeValue("EventCreditLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EventCreditLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Credit event site ID. Set to 0 for global credit event.
        /// </summary>
        public virtual int EventSiteID
        {
            get
            {
                return GetIntegerValue("EventSiteID", 0);
            }
            set
            {
                SetValue("EventSiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CreditEventInfoProvider.DeleteCreditEventInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CreditEventInfoProvider.SetCreditEventInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CreditEventInfo object.
        /// </summary>
        public CreditEventInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CreditEventInfo object from the given DataRow.
        /// </summary>
        public CreditEventInfo(DataRow dr)
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
            return EcommercePermissions.CheckCustomersPermissions(permission, siteName, userInfo, exceptionOnFailure, base.CheckPermissionsInternal);
        }

        #endregion
    }
}