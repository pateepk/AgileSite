using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(ABVariantDataInfo), ABVariantDataInfo.OBJECT_TYPE)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents a materialized A/B variant. 
    /// </summary>
    public class ABVariantDataInfo : AbstractInfo<ABVariantDataInfo>
    {
        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ABVARIANTDATA;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ABVariantDataInfoProvider), OBJECT_TYPE, "OM.ABVariantData", "ABVariantID", null, "ABVariantGUID", null, "ABVariantDisplayName", null, null, "ABVariantTestID", PredefinedObjectType.ABTEST)
        {
            TouchCacheDependencies = true,
            SupportsCloning = false,
            SupportsVersioning = false,
            AllowDataExport = false,
            AllowTouchParent = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false, 
IsExportable = false
            },
            Feature = FeatureEnum.ABTesting,
            ContainsMacros = false
        };


        /// <summary>
        /// Variant data ID.
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
        /// Indicates whether the variant is the original one.
        /// </summary>
        [DatabaseField]
        public virtual bool ABVariantIsOriginal
        {
            get
            {
                return GetBooleanValue("ABVariantIsOriginal", false);
            }
            set
            {
                SetValue("ABVariantIsOriginal", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ABVariantDataInfoProvider.DeleteABVariantDataInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ABVariantDataInfoProvider.SetABVariantDataInfo(this);
        }


        /// <summary>
        /// Constructor - Creates an empty ABVariantDataInfo object.
        /// </summary>
        public ABVariantDataInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ABVariantDataInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ABVariantDataInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}