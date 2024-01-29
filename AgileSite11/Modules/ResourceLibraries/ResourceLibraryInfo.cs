using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;
using CMS.Core;

[assembly: RegisterObjectType(typeof(ResourceLibraryInfo), ResourceLibraryInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// ResourceLibraryInfo data container class.
    /// </summary>
    [Serializable]
    public class ResourceLibraryInfo : AbstractInfo<ResourceLibraryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.resourcelibrary";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ResourceLibraryInfoProvider), OBJECT_TYPE, "CMS.ResourceLibrary", "ResourceLibraryID", null, null, null, null, null, null, "ResourceLibraryResourceID", ResourceInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.CMS,
            TouchCacheDependencies = true,
            AllowDataExport = true,
            ImportExportSettings = {
                AllowSingleExport = false,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false,
                IsExportable = false,
            },
            SupportsVersioning = false,
            SupportsCloning = false,
            SupportsLocking = false,
            SupportsCloneToOtherSite = false,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            AllowRestore = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "ResourceLibraryPath"
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Resource library ID
        /// </summary>
        [DatabaseField]
        public virtual int ResourceLibraryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ResourceLibraryID"), 0);
            }
            set
            {
                SetValue("ResourceLibraryID", value);
            }
        }


        /// <summary>
        /// Resource library resource ID
        /// </summary>
        [DatabaseField]
        public virtual int ResourceLibraryResourceID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ResourceLibraryResourceID"), 0);
            }
            set
            {
                SetValue("ResourceLibraryResourceID", value);
            }
        }


        /// <summary>
        /// Gets or sets the physical path to a dll library within the application directory. 
        /// </summary>
        [DatabaseField]
        public virtual string ResourceLibraryPath
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ResourceLibraryPath"), String.Empty);
            }
            set
            {
                SetValue("ResourceLibraryPath", value, String.Empty);
            }
        }
        
        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ResourceLibraryInfoProvider.DeleteResourceLibraryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ResourceLibraryInfoProvider.SetResourceLibraryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ResourceLibraryInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ResourceLibraryInfo object.
        /// </summary>
        public ResourceLibraryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ResourceLibraryInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ResourceLibraryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}