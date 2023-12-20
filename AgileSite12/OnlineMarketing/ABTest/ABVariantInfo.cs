using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(ABVariantInfo), ABVariantInfo.OBJECT_TYPE)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// ABVariantInfo data container class.
    /// </summary>
    public class ABVariantInfo : AbstractInfo<ABVariantInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ABVARIANT;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ABVariantInfoProvider), OBJECT_TYPE, "OM.ABVariant", "ABVariantID", "ABVariantLastModified", "ABVariantGUID", "ABVariantName", "ABVariantDisplayName", null, "ABVariantSiteID", "ABVariantTestID", "om.abtest")
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            MaxCodeNameLength = 50,
            ImportExportSettings = { LogExport = false },
            Feature = FeatureEnum.ABTesting,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Variant path.
        /// </summary>
        [DatabaseField]
        public virtual string ABVariantPath
        {
            get
            {
                return GetStringValue("ABVariantPath", "");
            }
            set
            {
                SetValue("ABVariantPath", value);
            }
        }


        /// <summary>
        /// Last modification of variant.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ABVariantLastModified
        {
            get
            {
                return GetDateTimeValue("ABVariantLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ABVariantLastModified", value);
            }
        }


        /// <summary>
        /// Variant's test ID.
        /// </summary>
        [DatabaseField]
        public virtual int ABVariantTestID
        {
            get
            {
                return GetIntegerValue("ABVariantTestID", 0);
            }
            set
            {
                SetValue("ABVariantTestID", value);
            }
        }


        /// <summary>
        /// Variant code name.
        /// </summary>
        [DatabaseField]
        public virtual string ABVariantName
        {
            get
            {
                return GetStringValue("ABVariantName", "");
            }
            set
            {
                SetValue("ABVariantName", value);
            }
        }


        /// <summary>
        /// Variant display name.
        /// </summary>
        [DatabaseField]
        public virtual string ABVariantDisplayName
        {
            get
            {
                return GetStringValue("ABVariantDisplayName", "");
            }
            set
            {
                SetValue("ABVariantDisplayName", value);
            }
        }


        /// <summary>
        /// Variant ID.
        /// </summary>
        [DatabaseField]
        public virtual int ABVariantID
        {
            get
            {
                return GetIntegerValue("ABVariantID", 0);
            }
            set
            {
                SetValue("ABVariantID", value);
            }
        }


        /// <summary>
        /// Variant GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ABVariantGUID
        {
            get
            {
                return GetGuidValue("ABVariantGUID", Guid.Empty);
            }
            set
            {
                SetValue("ABVariantGUID", value);
            }
        }


        /// <summary>
        /// Site ID of test variant.
        /// </summary>
        [DatabaseField]
        public virtual int ABVariantSiteID
        {
            get
            {
                return GetIntegerValue("ABVariantSiteID", 0);
            }
            set
            {
                SetValue("ABVariantSiteID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ABVariantInfoProvider.DeleteABVariantInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ABVariantInfoProvider.SetABVariantInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ABVariantInfo object.
        /// </summary>
        public ABVariantInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ABVariantInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ABVariantInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}