using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.TranslationServices;

[assembly: RegisterObjectType(typeof(TranslationSubmissionItemInfo), TranslationSubmissionItemInfo.OBJECT_TYPE)]

namespace CMS.TranslationServices
{
    /// <summary>
    /// TranslationSubmissionItemInfo data container class.
    /// </summary>
    public class TranslationSubmissionItemInfo : AbstractInfo<TranslationSubmissionItemInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.translationsubmissionitem";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TranslationSubmissionItemInfoProvider), OBJECT_TYPE, "CMS.TranslationSubmissionItem", "SubmissionItemID", "SubmissionItemLastModified", "SubmissionItemGUID", null, null, null, null, "SubmissionItemSubmissionID", TranslationSubmissionInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SubmissionItemSubmissionID", TranslationSubmissionInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.Default,
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsCloning = false,
            ModuleName = ModuleName.TRANSLATIONSERVICES,
            ImportExportSettings = { LogExport = true }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the submission this item belongs to.
        /// </summary>
        public virtual int SubmissionItemSubmissionID
        {
            get
            {
                return GetIntegerValue("SubmissionItemSubmissionID", 0);
            }
            set
            {
                SetValue("SubmissionItemSubmissionID", value);
            }
        }


        /// <summary>
        /// GUID of the submission item.
        /// </summary>
        public virtual Guid SubmissionItemGUID
        {
            get
            {
                return GetGuidValue("SubmissionItemGUID", Guid.Empty);
            }
            set
            {
                SetValue("SubmissionItemGUID", value);
            }
        }


        /// <summary>
        /// Name of the submission item.
        /// </summary>
        public virtual string SubmissionItemName
        {
            get
            {
                return GetStringValue("SubmissionItemName", "");
            }
            set
            {
                SetValue("SubmissionItemName", value);
            }
        }


        /// <summary>
        /// Target culture of the submission item.
        /// </summary>
        public virtual string SubmissionItemTargetCulture
        {
            get
            {
                return GetStringValue("SubmissionItemTargetCulture", "");
            }
            set
            {
                SetValue("SubmissionItemTargetCulture", value);
            }
        }


        /// <summary>
        /// Object type of the submission item.
        /// </summary>
        public virtual string SubmissionItemObjectType
        {
            get
            {
                return GetStringValue("SubmissionItemObjectType", "");
            }
            set
            {
                SetValue("SubmissionItemObjectType", value);
            }
        }


        /// <summary>
        /// ID of the object.
        /// </summary>
        public virtual int SubmissionItemObjectID
        {
            get
            {
                return GetIntegerValue("SubmissionItemObjectID", 0);
            }
            set
            {
                SetValue("SubmissionItemObjectID", value);
            }
        }


        /// <summary>
        /// ID of the object.
        /// </summary>
        public virtual int SubmissionItemTargetObjectID
        {
            get
            {
                return GetIntegerValue("SubmissionItemTargetObjectID", 0);
            }
            set
            {
                SetValue("SubmissionItemTargetObjectID", value);
            }
        }


        /// <summary>
        /// Number of words to translate within the submission item.
        /// </summary>
        public virtual int SubmissionItemWordCount
        {
            get
            {
                return GetIntegerValue("SubmissionItemWordCount", 0);
            }
            set
            {
                SetValue("SubmissionItemWordCount", value);
            }
        }


        /// <summary>
        /// Number of characters to translate within the submission item.
        /// </summary>
        public virtual int SubmissionItemCharCount
        {
            get
            {
                return GetIntegerValue("SubmissionItemCharCount", 0);
            }
            set
            {
                SetValue("SubmissionItemCharCount", value);
            }
        }


        /// <summary>
        /// Translated XLIFF document recieved from translation service.
        /// </summary>
        public virtual string SubmissionItemTargetXLIFF
        {
            get
            {
                return GetStringValue("SubmissionItemTargetXLIFF", "");
            }
            set
            {
                SetValue("SubmissionItemTargetXLIFF", value);
            }
        }


        /// <summary>
        /// Submission item last modified date.
        /// </summary>
        public virtual DateTime SubmissionItemLastModified
        {
            get
            {
                return GetDateTimeValue("SubmissionItemLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubmissionItemLastModified", value);
            }
        }


        /// <summary>
        /// Source XLIFF data sent for translation.
        /// </summary>
        public virtual string SubmissionItemSourceXLIFF
        {
            get
            {
                return GetStringValue("SubmissionItemSourceXLIFF", "");
            }
            set
            {
                SetValue("SubmissionItemSourceXLIFF", value);
            }
        }


        /// <summary>
        /// Custom data of submission item.
        /// </summary>
        public virtual string SubmissionItemCustomData
        {
            get
            {
                return GetStringValue("SubmissionItemCustomData", "");
            }
            set
            {
                SetValue("SubmissionItemCustomData", value);
            }
        }


        /// <summary>
        /// ID of the submission item.
        /// </summary>
        public virtual int SubmissionItemID
        {
            get
            {
                return GetIntegerValue("SubmissionItemID", 0);
            }
            set
            {
                SetValue("SubmissionItemID", value);
            }
        }


        /// <summary>
        /// File type of the submission item (XLIFF by default).
        /// </summary>
        public virtual string SubmissionItemType
        {
            get
            {
                return GetStringValue("SubmissionItemType", "XLIFF");
            }
            set
            {
                SetValue("SubmissionItemType", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TranslationSubmissionItemInfoProvider.DeleteTranslationSubmissionItemInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TranslationSubmissionItemInfoProvider.SetTranslationSubmissionItemInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TranslationSubmissionItemInfo object.
        /// </summary>
        public TranslationSubmissionItemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TranslationSubmissionItemInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TranslationSubmissionItemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
