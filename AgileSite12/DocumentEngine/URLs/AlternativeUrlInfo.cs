using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(AlternativeUrlInfo), AlternativeUrlInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Data container class for <see cref="AlternativeUrlInfo"/>.
    /// </summary>
    [Serializable]
    public class AlternativeUrlInfo : AbstractInfo<AlternativeUrlInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.alternativeurl";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlternativeUrlInfoProvider), OBJECT_TYPE, "CMS.AlternativeUrl", "AlternativeUrlID", "AlternativeUrlLastModified", "AlternativeUrlGUID", null, null, null, "AlternativeUrlSiteID", "AlternativeUrlDocumentID", DocumentCultureDataInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.DOCUMENTENGINE,
            TouchCacheDependencies = true,
            ContainsMacros = false,
            SupportsGlobalObjects = false,
            AllowDataExport = false,
            HasMetaFiles = false,
            HasProcesses = false,
            HasScheduledTasks = false,
            IsBinding = false,
            IsMultipleBinding = false,
            IsSiteBinding = false,
            LogEvents = true,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.Complete,
                IsExportable = false
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.TouchParent
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "AlternativeUrlUrl" }
            }
        };


        /// <summary>
        /// Alternative url ID.
        /// </summary>
        [DatabaseField]
        public virtual int AlternativeUrlID
        {
            get
            {
                return GetIntegerValue("AlternativeUrlID", 0);
            }
            set
            {
                SetValue("AlternativeUrlID", value);
            }
        }


        /// <summary>
        /// Alternative url GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid AlternativeUrlGUID
        {
            get
            {
                return GetGuidValue("AlternativeUrlGUID", Guid.Empty);
            }
            set
            {
                SetValue("AlternativeUrlGUID", value);
            }
        }


        /// <summary>
        /// Alternative url document ID.
        /// </summary>
        [DatabaseField]
        public virtual int AlternativeUrlDocumentID
        {
            get
            {
                return GetIntegerValue("AlternativeUrlDocumentID", 0);
            }
            set
            {
                SetValue("AlternativeUrlDocumentID", value);
            }
        }


        /// <summary>
        /// Alternative url site ID.
        /// </summary>
        [DatabaseField]
        public virtual int AlternativeUrlSiteID
        {
            get
            {
                return GetIntegerValue("AlternativeUrlSiteID", 0);
            }
            set
            {
                SetValue("AlternativeUrlSiteID", value);
            }
        }


        /// <summary>
        /// Alternative url url.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual NormalizedAlternativeUrl AlternativeUrlUrl
        {
            get
            {
                return new NormalizedAlternativeUrl(GetStringValue("AlternativeUrlUrl", String.Empty));
            }
            set
            {
                SetValue("AlternativeUrlUrl", value.NormalizedUrl);
            }
        }


        /// <summary>
        /// Alternative url last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime AlternativeUrlLastModified
        {
            get
            {
                return GetDateTimeValue("AlternativeUrlLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AlternativeUrlLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AlternativeUrlInfoProvider.DeleteAlternativeUrlInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AlternativeUrlInfoProvider.SetAlternativeUrlInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected AlternativeUrlInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AlternativeUrlInfo"/> class.
        /// </summary>
        public AlternativeUrlInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AlternativeUrlInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AlternativeUrlInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}