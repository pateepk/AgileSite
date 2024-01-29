using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(DepartmentInfo), DepartmentInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// DepartmentInfo data container class.
    /// </summary>
    public class DepartmentInfo : AbstractInfo<DepartmentInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.department";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DepartmentInfoProvider), OBJECT_TYPE, "ECommerce.Department", "DepartmentID", "DepartmentLastModified", "DepartmentGUID", "DepartmentName", "DepartmentDisplayName", null, "DepartmentSiteID", null, null)
        {
            // Child object types
            // - None

            // Object dependencies
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("DepartmentDefaultTaxClassID", TaxClassInfo.OBJECT_TYPE)
            },
            Extends = new List<ExtraColumn>()
            {
                new ExtraColumn(DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, "ClassSKUDefaultDepartmentID"), 
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
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            NameGloballyUnique = true,
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
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Code name of the department.
        /// </summary>
        [DatabaseField]
        public virtual string DepartmentName
        {
            get
            {
                return GetStringValue("DepartmentName", "");
            }
            set
            {
                SetValue("DepartmentName", value);
            }
        }


        /// <summary>
        /// ID of the department.
        /// </summary>
        [DatabaseField]
        public virtual int DepartmentID
        {
            get
            {
                return GetIntegerValue("DepartmentID", 0);
            }
            set
            {
                SetValue("DepartmentID", value);
            }
        }


        /// <summary>
        /// Display name of the department.
        /// </summary>
        [DatabaseField]
        public virtual string DepartmentDisplayName
        {
            get
            {
                return GetStringValue("DepartmentDisplayName", "");
            }
            set
            {
                SetValue("DepartmentDisplayName", value);
            }
        }


        /// <summary>
        /// Department default tax class ID.
        /// </summary>
        [DatabaseField]
        public virtual int DepartmentDefaultTaxClassID
        {
            get
            {
                return GetIntegerValue("DepartmentDefaultTaxClassID", 0);
            }
            set
            {
                SetValue("DepartmentDefaultTaxClassID", value, value > 0);
            }
        }


        /// <summary>
        /// Department GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid DepartmentGUID
        {
            get
            {
                return GetGuidValue("DepartmentGUID", Guid.Empty);
            }
            set
            {
                SetValue("DepartmentGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DepartmentLastModified
        {
            get
            {
                return GetDateTimeValue("DepartmentLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("DepartmentLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Department site ID. Set to 0 for global department.
        /// </summary>
        [DatabaseField]
        public virtual int DepartmentSiteID
        {
            get
            {
                return GetIntegerValue("DepartmentSiteID", 0);
            }
            set
            {
                SetValue("DepartmentSiteID", value, value > 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject() => DepartmentInfoProvider.DeleteDepartmentInfo(this);


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject() => DepartmentInfoProvider.SetDepartmentInfo(this);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DepartmentInfo object.
        /// </summary>
        public DepartmentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DepartmentInfo object from the given DataRow.
        /// </summary>
        public DepartmentInfo(DataRow dr)
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