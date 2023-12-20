using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.Globalization;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(OrderAddressInfo), OrderAddressInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OrderAddressInfo data container class.
    /// </summary>
    [Serializable]
    public class OrderAddressInfo : AbstractInfo<OrderAddressInfo>, ISimpleDataContainer
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.orderaddress";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OrderAddressInfoProvider), OBJECT_TYPE, "ECommerce.OrderAddress", "AddressID", "AddressLastModified", "AddressGUID", null, "AddressPersonalName", null, null, "AddressOrderID", OrderInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>()
                                                                           {
                                                                               new ObjectDependency("AddressCountryID", CountryInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                                                                               new ObjectDependency("AddressStateID", StateInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
                                                                           },
                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = true,
                                                  TouchCacheDependencies = true,
                                                  AllowRestore = false,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  SupportsCloneToOtherSite = false,
                                                  SupportsInvalidation = true,
                                                  ImportExportSettings = { LogExport = false }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Address order ID.
        /// </summary>
        [DatabaseField]
        public virtual int AddressOrderID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AddressOrderID"), 0);
            }
            set
            {
                SetValue("AddressOrderID", value);
            }
        }


        /// <summary>
        /// Address type.
        /// </summary>
        [DatabaseField]
        public virtual int AddressType
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AddressType"), 0);
            }
            set
            {
                SetValue("AddressType", value);
            }
        }


        /// <summary>
        /// Address ZIP code.
        /// </summary>
        [DatabaseField]
        public virtual string AddressZip
        {
            get
            {
                return GetStringValue("AddressZip", "");
            }
            set
            {
                SetValue("AddressZip", value);
            }
        }


        /// <summary>
        /// Address state ID.
        /// </summary>
        [DatabaseField]
        public virtual int AddressStateID
        {
            get
            {
                return GetIntegerValue("AddressStateID", 0);
            }
            set
            {
                SetValue("AddressStateID", value, (value > 0));
            }
        }


        /// <summary>
        /// Address phone.
        /// </summary>
        [DatabaseField]
        public virtual string AddressPhone
        {
            get
            {
                return GetStringValue("AddressPhone", "");
            }
            set
            {
                SetValue("AddressPhone", value);
            }
        }


        /// <summary>
        /// Address country ID.
        /// </summary>
        [DatabaseField]
        public virtual int AddressCountryID
        {
            get
            {
                return GetIntegerValue("AddressCountryID", 0);
            }
            set
            {
                SetValue("AddressCountryID", value, (value > 0));
            }
        }


        /// <summary>
        /// Address ID.
        /// </summary>
        [DatabaseField]
        public virtual int AddressID
        {
            get
            {
                return GetIntegerValue("AddressID", 0);
            }
            set
            {
                SetValue("AddressID", value);
            }
        }


        /// <summary>
        /// Address personal name.
        /// </summary>
        [DatabaseField]
        public virtual string AddressPersonalName
        {
            get
            {
                return GetStringValue("AddressPersonalName", "");
            }
            set
            {
                SetValue("AddressPersonalName", value);
            }
        }


        /// <summary>
        /// Address line 1.
        /// </summary>
        [DatabaseField]
        public virtual string AddressLine1
        {
            get
            {
                return GetStringValue("AddressLine1", "");
            }
            set
            {
                SetValue("AddressLine1", value);
            }
        }


        /// <summary>
        /// Address line 2.
        /// </summary>
        [DatabaseField]
        public virtual string AddressLine2
        {
            get
            {
                return GetStringValue("AddressLine2", "");
            }
            set
            {
                SetValue("AddressLine2", value);
            }
        }


        /// <summary>
        /// Address city.
        /// </summary>
        [DatabaseField]
        public virtual string AddressCity
        {
            get
            {
                return GetStringValue("AddressCity", "");
            }
            set
            {
                SetValue("AddressCity", value);
            }
        }


        /// <summary>
        /// Address GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid AddressGUID
        {
            get
            {
                return GetGuidValue("AddressGUID", Guid.Empty);
            }
            set
            {
                SetValue("AddressGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Date and time when the address was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime AddressLastModified
        {
            get
            {
                return GetDateTimeValue("AddressLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AddressLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OrderAddressInfoProvider.DeleteAddressInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OrderAddressInfoProvider.SetAddressInfo(this);
        }


        /// <summary>
        /// Returns value of property (additional support for Country and State properties).
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetProperty(string columnName, out object value)
        {
            bool match = base.TryGetProperty(columnName, out value);
            if (!match)
            {
                switch (columnName.ToLowerInvariant())
                {
                    case "country":
                        return base.TryGetProperty("AddressCountry", out value);

                    case "state":
                        return base.TryGetProperty("AddressState", out value);
                }
            }
            return match;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public OrderAddressInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty OrderAddressInfo object.
        /// </summary>
        public OrderAddressInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OrderAddressInfo object from the given DataRow.
        /// </summary>
        public OrderAddressInfo(DataRow dr)
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