using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Base;
using CMS.Globalization;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(TaxClassStateInfo), TaxClassStateInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// TaxClassStateInfo data container class.
    /// </summary>
    public class TaxClassStateInfo : AbstractInfo<TaxClassStateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.taxclassstate";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TaxClassStateInfoProvider), OBJECT_TYPE, "ECommerce.TaxClassState", "TaxClassStateID", null, null, null, null, null, null, "TaxClassID", TaxClassInfo.OBJECT_TYPE)
                                              {
                                                  // Child object types
                                                  // - None

                                                  // Object dependencies
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("StateID", StateInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
                                                  // Binding object types
                                                  // - None

                                                  // Others
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  IsBinding = true,
                                                  AllowRestore = false,
                                                  ModuleName = ModuleName.ECOMMERCE,
                                                  Feature = FeatureEnum.Ecommerce,
                                                  ImportExportSettings = { LogExport = false },
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Tax class - state relationship ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaxClassStateID
        {
            get
            {
                return GetIntegerValue("TaxClassStateID", 0);
            }
            set
            {
                SetValue("TaxClassStateID", value);
            }
        }


        /// <summary>
        /// Value of the tax.
        /// </summary>
        [DatabaseField]
        public virtual decimal TaxValue
        {
            get
            {
                return GetDecimalValue("TaxValue", 0);
            }
            set
            {
                SetValue("TaxValue", value);
            }
        }


        /// <summary>
        /// ID of the tax class.
        /// </summary>
        [DatabaseField]
        public virtual int TaxClassID
        {
            get
            {
                return GetIntegerValue("TaxClassID", 0);
            }
            set
            {
                SetValue("TaxClassID", value);
            }
        }


        /// <summary>
        /// ID of the state.
        /// </summary>
        [DatabaseField]
        public virtual int StateID
        {
            get
            {
                return GetIntegerValue("StateID", 0);
            }
            set
            {
                SetValue("StateID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Gets the full name in format TaxClassID.StateID
        /// </summary>
        protected override string ObjectFullName => ObjectHelper.BuildFullName(TaxClassID.ToString(), StateID.ToString());


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject() => TaxClassStateInfoProvider.DeleteTaxClassStateInfo(this);


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject() => TaxClassStateInfoProvider.SetTaxClassStateInfo(this);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TaxClassStateInfo object.
        /// </summary>
        public TaxClassStateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TaxClassStateInfo object from the given DataRow.
        /// </summary>
        public TaxClassStateInfo(DataRow dr)
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