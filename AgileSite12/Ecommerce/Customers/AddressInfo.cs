using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.Globalization;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(AddressInfo), AddressInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// AddressInfo data container class.
    /// </summary>
    [Serializable]
    public class AddressInfo : AbstractInfo<AddressInfo>, ISimpleDataContainer
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.address";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AddressInfoProvider), OBJECT_TYPE, "ECommerce.Address", "AddressID", "AddressLastModified", "AddressGUID", null, "AddressName", null, null, "AddressCustomerID", CustomerInfo.OBJECT_TYPE)
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
                                                  CheckDependenciesOnDelete = true,
                                                  TouchCacheDependencies = true,
                                                  AllowRestore = false,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  SupportsGlobalObjects = true,
                                                  SupportsCloneToOtherSite = false,
                                                  ImportExportSettings = { LogExport = false }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Address customer ID.
        /// </summary>
        public virtual int AddressCustomerID
        {
            get
            {
                return GetIntegerValue("AddressCustomerID", 0);
            }
            set
            {
                SetValue("AddressCustomerID", value);
            }
        }


        /// <summary>
        /// Address ZIP code.
        /// </summary>
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
        /// Address name.
        /// </summary>
        public virtual string AddressName
        {
            get
            {
                return GetStringValue("AddressName", "");
            }
            set
            {
                SetValue("AddressName", value);
            }
        }


        /// <summary>
        /// Address personal name.
        /// </summary>
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
            AddressInfoProvider.DeleteAddressInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AddressInfoProvider.SetAddressInfo(this);
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
                switch (columnName.ToLowerCSafe())
                {
                    case "country":
                        return base.TryGetProperty("AddressCountry", out value);

                    case "state":
                        return base.TryGetProperty("AddressState", out value);
                }
            }
            return match;
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                AddressLine1 = ValidationHelper.GetString(p[OBJECT_TYPE + ".address1"], AddressLine1);
                AddressLine2 = ValidationHelper.GetString(p[OBJECT_TYPE + ".address2"], AddressLine2);
                AddressPersonalName = ValidationHelper.GetString(p[OBJECT_TYPE + ".personalname"], AddressPersonalName);
            }

            AddressName = AddressInfoProvider.GetAddressName(this);
            Insert();
        }


        /// <summary>
        /// Returns formatted address text.
        /// </summary>
        public override string ToMacroString()
        {
            return AddressInfoProvider.GetAddressName(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public AddressInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty AddressInfo object.
        /// </summary>
        public AddressInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AddressInfo object from the given DataRow.
        /// </summary>
        public AddressInfo(DataRow dr)
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