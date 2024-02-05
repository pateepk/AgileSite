using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Localization;

[assembly: RegisterObjectType(typeof(ResourceTranslationInfo), ResourceTranslationInfo.OBJECT_TYPE)]

namespace CMS.Localization
{
    /// <summary>
    /// ResourceTranslationInfo data container class.
    /// </summary>
    public class ResourceTranslationInfo : ResourceTranslationInfoBase<ResourceTranslationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.resourcetranslation";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ResourceTranslationInfoProvider), OBJECT_TYPE, "CMS.ResourceTranslation", "TranslationID", null, null, null, null, null, null, "TranslationStringID", ResourceStringInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("TranslationCultureID", CultureInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            IsBinding = true,
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            SupportsCloning = false,
            RegisterAsChildToObjectTypes = new List<string> { ResourceTranslatedInfo.OBJECT_TYPE },
            ContainsMacros = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion

       
        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ResourceTranslationInfoProvider.DeleteResourceTranslationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ResourceTranslationInfoProvider.SetResourceTranslationInfo(this);
        }


        /// <summary>
        /// Gets the existing object.
        /// </summary>
        protected override BaseInfo GetExisting()
        {
            return ResourceTranslationInfoProvider.GetResourceTranslationInfo(TranslationStringID, TranslationCultureID);
        }
        
        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ResourceTranslationInfo object.
        /// </summary>
        public ResourceTranslationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ResourceTranslationInfo object from the given DataRow.
        /// </summary>
        public ResourceTranslationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}