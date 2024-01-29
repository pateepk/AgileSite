using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.DataProtection;

[assembly: RegisterObjectType(typeof(ConsentArchiveInfo), ConsentArchiveInfo.OBJECT_TYPE)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Data container class for <see cref="ConsentArchiveInfo"/>.
    /// </summary>
	[Serializable]
    public class ConsentArchiveInfo : AbstractInfo<ConsentArchiveInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.consentarchive";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ConsentArchiveInfoProvider), OBJECT_TYPE, "CMS.ConsentArchive", "ConsentArchiveID", "ConsentArchiveLastModified", "ConsentArchiveGuid", null, null, null, null, "ConsentArchiveConsentID", ConsentInfo.OBJECT_TYPE)
        {
            ModuleName = DataProtectionModule.MODULE_NAME,
            Feature = FeatureEnum.DataProtection,
            TouchCacheDependencies = true,
            LogEvents = true,
            UpdateTimeStamp = false,
            SupportsCloning = false,
            AllowRestore = false,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            }
        };


        /// <summary>
        /// Consent archive ID.
        /// </summary>
        [DatabaseField]
        public virtual int ConsentArchiveID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConsentArchiveID"), 0);
            }
            set
            {
                SetValue("ConsentArchiveID", value);
            }
        }


        /// <summary>
        /// Consent archive guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid ConsentArchiveGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ConsentArchiveGuid"), Guid.Empty);
            }
            set
            {
                SetValue("ConsentArchiveGuid", value);
            }
        }


        /// <summary>
        /// Consent archive last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ConsentArchiveLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ConsentArchiveLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ConsentArchiveLastModified", value);
            }
        }


        /// <summary>
        /// Consent ID.
        /// </summary>
        [DatabaseField]
        public virtual int ConsentArchiveConsentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConsentArchiveConsentID"), 0);
            }
            set
            {
                SetValue("ConsentArchiveConsentID", value);
            }
        }


        /// <summary>
        /// Consent hash.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentArchiveHash
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentArchiveHash"), String.Empty);
            }
            set
            {
                SetValue("ConsentArchiveHash", value);
            }
        }


        /// <summary>
        /// Consent content.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentArchiveContent
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentArchiveContent"), String.Empty);
            }
            set
            {
                SetValue("ConsentArchiveContent", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ConsentArchiveInfoProvider.DeleteConsentArchiveInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ConsentArchiveInfoProvider.SetConsentArchiveInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ConsentArchiveInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ConsentArchiveInfo"/> class.
        /// </summary>
        public ConsentArchiveInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ConsentArchiveInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ConsentArchiveInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}