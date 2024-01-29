using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(MVTCombinationInfo), MVTCombinationInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(MVTCombinationInfo), MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MVTCombinationInfo data container class.
    /// </summary>
    public class MVTCombinationInfo : AbstractInfo<MVTCombinationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.MVTCOMBINATION;

        /// <summary>
        /// Object type for document combination
        /// </summary>
        public const string OBJECT_TYPE_DOCUMENTCOMBINATION = PredefinedObjectType.DOCUMENTMVTCOMBINATION;


        /// <summary>
        /// Type information (Web part, zone combinations).
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof (MVTCombinationInfoProvider), OBJECT_TYPE, "OM.MVTCombination", "MVTCombinationID", "MVTCombinationLastModified", "MVTCombinationGUID", "MVTCombinationName", null, null, null, "MVTCombinationPageTemplateID", PageTemplateInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            SupportsCloning = false,
            TypeCondition = new TypeCondition().WhereIsNull("MVTCombinationDocumentID"),
            EnabledColumn = "MVTCombinationEnabled",
            ImportExportSettings =
            {
                LogExport = false
            },
            Feature = FeatureEnum.MVTesting,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "MVTCombinationConversions" }
            }
        };


        /// <summary>
        /// Type information (Combinations containing widgets).
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_DOCUMENT = new ObjectTypeInfo(typeof (MVTCombinationInfoProvider), OBJECT_TYPE_DOCUMENTCOMBINATION, "OM.MVTCombination", "MVTCombinationID", "MVTCombinationLastModified", "MVTCombinationGUID", "MVTCombinationName", null, null, null, "MVTCombinationDocumentID", PredefinedObjectType.DOCUMENTLOCALIZATION)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MVTCombinationPageTemplateID", PageTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            OriginalTypeInfo = TYPEINFO,
            AllowRestore = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            SupportsCloning = false,
            TypeCondition = new TypeCondition().WhereIsNotNull("MVTCombinationDocumentID"),
            EnabledColumn = "MVTCombinationEnabled",
            ImportExportSettings =
            {
                LogExport = false
            },
            Feature = FeatureEnum.MVTesting,
            AllowTouchParent = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "MVTCombinationConversions" }
            }
        };

        #endregion


        #region "Variables"

        private bool? mMVTCombinationEnabledOriginal = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Number of conversions of the current combination
        /// </summary>
        public virtual int MVTCombinationConversions
        {
            get
            {
                return GetIntegerValue("MVTCombinationConversions", 0);
            }
            set
            {
                SetValue("MVTCombinationConversions", value);
            }
        }


        /// <summary>
        /// Unique combination identifier.
        /// </summary>
        public virtual Guid MVTCombinationGUID
        {
            get
            {
                return GetGuidValue("MVTCombinationGUID", Guid.Empty);
            }
            set
            {
                SetValue("MVTCombinationGUID", value);
            }
        }


        /// <summary>
        /// MVT combination ID.
        /// </summary>
        public virtual int MVTCombinationID
        {
            get
            {
                return GetIntegerValue("MVTCombinationID", 0);
            }
            set
            {
                SetValue("MVTCombinationID", value);
            }
        }


        /// <summary>
        /// MVT combination document ID.
        /// </summary>
        public virtual int MVTCombinationDocumentID
        {
            get
            {
                return GetIntegerValue("MVTCombinationDocumentID", 0);
            }
            set
            {
                SetValue("MVTCombinationDocumentID", value, 0);
            }
        }


        /// <summary>
        /// Last modification of the MVT combination.
        /// </summary>
        public virtual DateTime MVTCombinationLastModified
        {
            get
            {
                return GetDateTimeValue("MVTCombinationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MVTCombinationLastModified", value);
            }
        }


        /// <summary>
        /// MVT combination code name.
        /// </summary>
        public virtual string MVTCombinationName
        {
            get
            {
                return GetStringValue("MVTCombinationName", "");
            }
            set
            {
                SetValue("MVTCombinationName", value);
            }
        }


        /// <summary>
        /// MVT combination custom name.
        /// </summary>
        public virtual string MVTCombinationCustomName
        {
            get
            {
                return GetStringValue("MVTCombinationCustomName", "");
            }
            set
            {
                SetValue("MVTCombinationCustomName", value);
            }
        }


        /// <summary>
        /// MVT combination page template ID.
        /// </summary>
        public virtual int MVTCombinationPageTemplateID
        {
            get
            {
                return GetIntegerValue("MVTCombinationPageTemplateID", 0);
            }
            set
            {
                SetValue("MVTCombinationPageTemplateID", value);
            }
        }


        /// <summary>
        /// MVT combination enabled.
        /// </summary>
        public virtual bool MVTCombinationEnabled
        {
            get
            {
                return GetBooleanValue("MVTCombinationEnabled", false);
            }
            set
            {
                SetValue("MVTCombinationEnabled", value);
            }
        }


        /// <summary>
        /// MVT combination enabled - original value.
        /// </summary>
        public virtual bool MVTCombinationEnabledOriginal
        {
            get
            {
                if (mMVTCombinationEnabledOriginal.HasValue)
                {
                    return mMVTCombinationEnabledOriginal.Value;
                }
                else
                {
                    return MVTCombinationEnabled;
                }
            }
            set
            {
                mMVTCombinationEnabledOriginal = value;
            }
        }


        /// <summary>
        /// Indicates whether the MVT combination is a default combination (without any mvt variants).
        /// </summary>
        public virtual bool MVTCombinationIsDefault
        {
            get
            {
                return GetBooleanValue("MVTCombinationIsDefault", false);
            }
            set
            {
                SetValue("MVTCombinationIsDefault", value);
            }
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
                if (MVTCombinationDocumentID > 0)
                {
                    // Combination contaning widgets
                    return TYPEINFO_DOCUMENT;
                }
                else
                {
                    // Combination contaning web parts and zones only
                    return TYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MVTCombinationInfoProvider.DeleteMVTCombinationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MVTCombinationInfoProvider.SetMVTCombinationInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MVTCombinationInfo object.
        /// </summary>
        public MVTCombinationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MVTCombinationInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MVTCombinationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}