using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.Membership;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(MVTVariantInfo), MVTVariantInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(MVTVariantInfo), MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MVTVariantInfo data container class.
    /// </summary>
    public class MVTVariantInfo : AbstractInfo<MVTVariantInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.MVTVARIANT;

        /// <summary>
        /// Object type for document MVT variant
        /// </summary>
        public const string OBJECT_TYPE_DOCUMENTVARIANT = PredefinedObjectType.DOCUMENTMVTVARIANT;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof (MVTVariantInfoProvider), OBJECT_TYPE, "OM.MVTVariant", "MVTVariantID", "MVTVariantLastModified", "MVTVariantGUID", "MVTVariantName", "MVTVariantDisplayName", null, null, "MVTVariantPageTemplateID", PageTemplateInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            SupportsCloning = false,
            TypeCondition = new TypeCondition().WhereIsNull("MVTVariantDocumentID"),
            ImportExportSettings =
            {
                LogExport = false
            },
            EnabledColumn = "MVTVariantEnabled",
            Feature = FeatureEnum.MVTesting,
            SerializationSettings =
            {
                StructuredFields = new[]
                {
                    new StructuredField<WebPartsStructuredData>("MVTVariantWebParts")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_DOCUMENTVARIANT = new ObjectTypeInfo(typeof (MVTVariantInfoProvider), OBJECT_TYPE_DOCUMENTVARIANT, "OM.MVTVariant", "MVTVariantID", "MVTVariantLastModified", "MVTVariantGUID", "MVTVariantName", "MVTVariantDisplayName", null, null, "MVTVariantDocumentID", PredefinedObjectType.DOCUMENTLOCALIZATION)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MVTVariantPageTemplateID", PageTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            OriginalTypeInfo = TYPEINFO,
            AllowRestore = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            SupportsCloning = false,
            TypeCondition = new TypeCondition().WhereIsNotNull("MVTVariantDocumentID"),
            ImportExportSettings =
            {
                LogExport = false
            },
            EnabledColumn = "MVTVariantEnabled",
            Feature = FeatureEnum.MVTesting,
            AllowTouchParent = false,
            SerializationSettings =
            {
                StructuredFields = new[]
                {
                    new StructuredField<WebPartsStructuredData>("MVTVariantWebParts")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Original value indicating whether the variant is enabled.
        /// </summary>
        protected bool mMVTVariantEnabledOriginal = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// MVT variant ID.
        /// </summary>
        public virtual int MVTVariantID
        {
            get
            {
                return GetIntegerValue("MVTVariantID", 0);
            }
            set
            {
                SetValue("MVTVariantID", value);
            }
        }


        /// <summary>
        /// MVT variant zone ID.
        /// </summary>
        public virtual string MVTVariantZoneID
        {
            get
            {
                return GetStringValue("MVTVariantZoneID", "");
            }
            set
            {
                SetValue("MVTVariantZoneID", value);
            }
        }


        /// <summary>
        /// MVT variant document ID.
        /// </summary>
        public virtual int MVTVariantDocumentID
        {
            get
            {
                return GetIntegerValue("MVTVariantDocumentID", 0);
            }
            set
            {
                SetValue("MVTVariantDocumentID", value, 0);
            }
        }


        /// <summary>
        /// Web part instance GUID.
        /// </summary>
        public virtual Guid MVTVariantInstanceGUID
        {
            get
            {
                return GetGuidValue("MVTVariantInstanceGUID", Guid.Empty);
            }
            set
            {
                SetValue("MVTVariantInstanceGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// MVT variant display name.
        /// </summary>
        public virtual string MVTVariantDisplayName
        {
            get
            {
                return GetStringValue("MVTVariantDisplayName", "");
            }
            set
            {
                SetValue("MVTVariantDisplayName", value);
            }
        }


        /// <summary>
        /// MVT variant description.
        /// </summary>
        public virtual string MVTVariantDescription
        {
            get
            {
                return GetStringValue("MVTVariantDescription", "");
            }
            set
            {
                SetValue("MVTVariantDescription", value);
            }
        }


        /// <summary>
        /// MVT variant page template ID.
        /// </summary>
        public virtual int MVTVariantPageTemplateID
        {
            get
            {
                return GetIntegerValue("MVTVariantPageTemplateID", 0);
            }
            set
            {
                SetValue("MVTVariantPageTemplateID", value);
            }
        }


        /// <summary>
        /// Last modification of the MVT combination.
        /// </summary>
        public virtual bool MVTVariantEnabled
        {
            get
            {
                return GetBooleanValue("MVTVariantEnabled", false);
            }
            set
            {
                SetValue("MVTVariantEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the original value indicating whether the variant is enabled.
        /// </summary>
        public virtual bool MVTVariantEnabledOriginal
        {
            get
            {
                return mMVTVariantEnabledOriginal;
            }
            set
            {
                mMVTVariantEnabledOriginal = value;
            }
        }


        /// <summary>
        /// Unique project identifier.
        /// </summary>
        public virtual Guid MVTVariantGUID
        {
            get
            {
                return GetGuidValue("MVTVariantGUID", Guid.Empty);
            }
            set
            {
                SetValue("MVTVariantGUID", value);
            }
        }


        /// <summary>
        /// MVT variant name.
        /// </summary>
        public virtual string MVTVariantName
        {
            get
            {
                return GetStringValue("MVTVariantName", "");
            }
            set
            {
                SetValue("MVTVariantName", value);
            }
        }


        /// <summary>
        /// MVT variant web parts.
        /// </summary>
        public virtual string MVTVariantWebParts
        {
            get
            {
                return GetStringValue("MVTVariantWebParts", "");
            }
            set
            {
                SetValue("MVTVariantWebParts", value);
            }
        }


        /// <summary>
        /// Last modification of the MVT variant.
        /// </summary>
        public virtual DateTime MVTVariantLastModified
        {
            get
            {
                return GetDateTimeValue("MVTVariantLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MVTVariantLastModified", value);
            }
        }


        /// <summary>
        /// Variant web part/widget instance containing data built from this info object.
        /// </summary>
        public virtual WebPartInstance WebPartInstance
        {
            get;
            set;
        }


        /// <summary>
        /// Variant zone instance containing data built from this info object.
        /// </summary>
        public virtual WebPartZoneInstance WebPartZoneInstance
        {
            get;
            set;
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (MVTVariantDocumentID > 0)
                {
                    return TYPEINFO_DOCUMENTVARIANT;
                }
                else
                {
                    return TYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MVTVariantInfoProvider.DeleteMVTVariantInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MVTVariantInfoProvider.SetMVTVariantInfo(this);
        }


        /// <summary>
        /// Checks if a record with the same column values already exists in the database. Returns true if the set of values is unique.
        /// </summary>
        /// <param name="columns">Columns to check</param>
        public override bool CheckUniqueValues(params string[] columns)
        {
            columns = new string[] { "MVTVariantPageTemplateID", CodeNameColumn };
            return base.CheckUniqueValues(columns);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MVTVariantInfo object.
        /// </summary>
        public MVTVariantInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MVTVariantInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MVTVariantInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the MVT Variant object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return UserInfoProvider.IsAuthorizedPerResource("CMS.MVTest", "Read", siteName, (UserInfo)userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource("CMS.MVTest", "Manage", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}