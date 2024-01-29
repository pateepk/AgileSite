using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ModuleLicenses;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(ModuleLicenseKeyInfo), ModuleLicenseKeyInfo.OBJECT_TYPE)]
    
namespace CMS.ModuleLicenses
{
    /// <summary>
    /// ModuleLicenseKeyInfo data container class.
    /// </summary>
	[Serializable]
    public class ModuleLicenseKeyInfo : AbstractInfo<ModuleLicenseKeyInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.modulelicensekey";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ModuleLicenseKeyInfoProvider), OBJECT_TYPE, "CMS.ModuleLicenseKey", "ModuleLicenseKeyID", "ModuleLicenseKeyLastModified", "ModuleLicenseKeyGuid", null, "ModuleLicenseKeyLicense", null, null, "ModuleLicenseKeyResourceID", ResourceInfo.OBJECT_TYPE)
        {
			ModuleName = "CMS.ModuleLicenses",
			TouchCacheDependencies = true,
            SupportsCloning = false,
            ContinuousIntegrationSettings =
            {
                Enabled = false
            },
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            ContainsMacros = false,
            SupportsVersioning = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Module license key ID
        /// </summary>
        [DatabaseField]
        public virtual int ModuleLicenseKeyID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ModuleLicenseKeyID"), 0);
            }
            set
            {
                SetValue("ModuleLicenseKeyID", value);
            }
        }


        /// <summary>
        /// Module license key guid
        /// </summary>
        [DatabaseField]
        public virtual Guid ModuleLicenseKeyGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ModuleLicenseKeyGuid"), Guid.Empty);
            }
            set
            {
                SetValue("ModuleLicenseKeyGuid", value);
            }
        }


        /// <summary>
        /// Module license key last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime ModuleLicenseKeyLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ModuleLicenseKeyLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ModuleLicenseKeyLastModified", value);
            }
        }


        /// <summary>
        /// Module license key data
        /// </summary>
        [DatabaseField]
        public virtual string ModuleLicenseKeyLicense
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ModuleLicenseKeyLicense"), String.Empty);
            }
            set
            {
                SetValue("ModuleLicenseKeyLicense", value);
            }
        }


        /// <summary>
        /// Module license key resource ID
        /// </summary>
        [DatabaseField]
        public virtual int ModuleLicenseKeyResourceID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ModuleLicenseKeyResourceID"), 0);
            }
            set
            {
                SetValue("ModuleLicenseKeyResourceID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ModuleLicenseKeyInfoProvider.DeleteModuleLicenseKeyInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ModuleLicenseKeyInfoProvider.SetModuleLicenseKeyInfo(this);
        }

        #endregion


        #region "Constructors"

		/// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected ModuleLicenseKeyInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ModuleLicenseKeyInfo object.
        /// </summary>
        public ModuleLicenseKeyInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ModuleLicenseKeyInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ModuleLicenseKeyInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}