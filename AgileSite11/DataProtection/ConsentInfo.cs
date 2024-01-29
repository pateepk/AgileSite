using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(ConsentInfo), ConsentInfo.OBJECT_TYPE)]

namespace CMS.DataProtection
{
    /// <summary>
    /// Data container class for <see cref="ConsentInfo"/>.
    /// </summary>
	[Serializable]
    public class ConsentInfo : AbstractInfo<ConsentInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.consent";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ConsentInfoProvider), OBJECT_TYPE, "CMS.Consent", "ConsentID", "ConsentLastModified", "ConsentGuid", "ConsentName", "ConsentDisplayName", null, null, null, null)
        {
            ModuleName = DataProtectionModule.MODULE_NAME,
            Feature = FeatureEnum.DataProtection,
            TouchCacheDependencies = true,
            LogEvents = true,
            SupportsCloning = false,
            AllowRestore = false,
            ImportExportSettings =
            {
                AllowSingleExport = false,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("ConsentContent")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled =  true
            }
        };


        /// <summary>
        /// Consent ID.
        /// </summary>
        [DatabaseField]
        public virtual int ConsentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ConsentID"), 0);
            }
            set
            {
                SetValue("ConsentID", value);
            }
        }


        /// <summary>
        /// Code name of the Consent.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentName"), String.Empty);
            }
            set
            {
                SetValue("ConsentName", value);
            }
        }


        /// <summary>
        /// Consent display name.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentDisplayName"), String.Empty);
            }
            set
            {
                SetValue("ConsentDisplayName", value);
            }
        }


        /// <summary>
        /// Consent content.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentContent
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentContent"), String.Empty);
            }
            set
            {
                SetValue("ConsentContent", value);
            }
        }


        /// <summary>
        /// Consent GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ConsentGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ConsentGuid"), Guid.Empty);
            }
            set
            {
                SetValue("ConsentGuid", value);
            }
        }


        /// <summary>
        /// Consent last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ConsentLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ConsentLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ConsentLastModified", value);
            }
        }


        /// <summary>
        /// Consent hash
        /// </summary>
		[DatabaseField]
        public virtual string ConsentHash
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConsentHash"), String.Empty);
            }
            internal set
            {
                SetValue("ConsentHash", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ConsentInfoProvider.DeleteConsentInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ConsentInfoProvider.SetConsentInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ConsentInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ConsentInfo"/> class.
        /// </summary>
        public ConsentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ConsentInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ConsentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}