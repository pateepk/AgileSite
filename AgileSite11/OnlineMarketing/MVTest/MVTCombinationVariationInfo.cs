using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(MVTCombinationVariationInfo), MVTCombinationVariationInfo.OBJECT_TYPE)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MVTCombinationVariationInfo data container class.
    /// </summary>
    public class MVTCombinationVariationInfo : AbstractInfo<MVTCombinationVariationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.mvtcombinationvariation";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MVTCombinationVariationInfoProvider), OBJECT_TYPE, "OM.MVTCombinationVariation", null, null, null, null, null, null, null, "MVTCombinationID", MVTCombinationInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MVTVariantID", MVTVariantInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            RegisterAsOtherBindingToObjectTypes = new List<string> { MVTVariantInfo.OBJECT_TYPE, MVTVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT },
            RegisterAsBindingToObjectTypes = new List<string>(),
            RegisterAsChildToObjectTypes = new List<string> { MVTCombinationInfo.OBJECT_TYPE, MVTCombinationInfo.OBJECT_TYPE_DOCUMENTCOMBINATION },
            ImportExportSettings = { LogExport = false },
            Feature = FeatureEnum.MVTesting,
            ContainsMacros = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

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

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MVTCombinationVariationInfoProvider.DeleteMVTCombinationVariationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MVTCombinationVariationInfoProvider.SetMVTCombinationVariationInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MVTCombinationVariationInfo object.
        /// </summary>
        public MVTCombinationVariationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MVTCombinationVariationInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MVTCombinationVariationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}