using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.TranslationServices;

[assembly: RegisterObjectType(typeof(TranslationSubmissionInfo), TranslationSubmissionInfo.OBJECT_TYPE)]

namespace CMS.TranslationServices
{
    /// <summary>
    /// TranslationSubmissionInfo data container class.
    /// </summary>
    public class TranslationSubmissionInfo : AbstractInfo<TranslationSubmissionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.translationsubmission";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TranslationSubmissionInfoProvider), OBJECT_TYPE, "CMS.TranslationSubmission", "SubmissionID", "SubmissionLastModified", "SubmissionGUID", null, "SubmissionName", null, "SubmissionSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                    new ObjectDependency("SubmissionServiceID", TranslationServiceInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                    new ObjectDependency("SubmissionSubmittedByUserID", UserInfo.OBJECT_TYPE)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            SupportsCloning = false,
            ModuleName = ModuleName.TRANSLATIONSERVICES,
            ImportExportSettings = { IsExportable = false }
        };

        #endregion


        #region "Variables"

        private HashSet<string> mSubmissionTargetCultures;

        #endregion


        #region "Properties"

        /// <summary>
        /// List of target cultures of the submission separated by ';'.
        /// </summary>
        public virtual string SubmissionTargetCulture
        {
            get
            {
                return GetStringValue("SubmissionTargetCulture", "");
            }
            set
            {
                SetValue("SubmissionTargetCulture", value);
            }
        }


        /// <summary>
        /// List of target cultures of the submission.
        /// </summary>
        [DatabaseMapping(false)]
        public HashSet<string> SubmissionTargetCultures
        {
            get
            {
                return mSubmissionTargetCultures ?? (mSubmissionTargetCultures = new HashSet<string>(SubmissionTargetCulture.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase));
            }
        }


        /// <summary>
        /// Submission ticket (unique ID on the translation service side).
        /// </summary>
        public virtual string SubmissionTicket
        {
            get
            {
                return GetStringValue("SubmissionTicket", "");
            }
            set
            {
                SetValue("SubmissionTicket", value);
            }
        }


        /// <summary>
        /// Submission GUID.
        /// </summary>
        public virtual Guid SubmissionGUID
        {
            get
            {
                return GetGuidValue("SubmissionGUID", Guid.Empty);
            }
            set
            {
                SetValue("SubmissionGUID", value);
            }
        }


        /// <summary>
        /// ID of the submission.
        /// </summary>
        public virtual int SubmissionID
        {
            get
            {
                return GetIntegerValue("SubmissionID", 0);
            }
            set
            {
                SetValue("SubmissionID", value);
            }
        }


        /// <summary>
        /// Site ID of the submission.
        /// </summary>
        public virtual int SubmissionSiteID
        {
            get
            {
                return GetIntegerValue("SubmissionSiteID", 0);
            }
            set
            {
                SetValue("SubmissionSiteID", value);
            }
        }


        /// <summary>
        /// ID of the user who submitted submission to translation.
        /// </summary>
        public virtual int SubmissionSubmittedByUserID
        {
            get
            {
                return GetIntegerValue("SubmissionSubmittedByUserID", 0);
            }
            set
            {
                SetValue("SubmissionSubmittedByUserID", value);
            }
        }


        /// <summary>
        /// Name of the submission.
        /// </summary>
        public virtual string SubmissionName
        {
            get
            {
                return GetStringValue("SubmissionName", "");
            }
            set
            {
                SetValue("SubmissionName", value);
            }
        }


        /// <summary>
        /// Price of the submission.
        /// </summary>
        public virtual double SubmissionPrice
        {
            get
            {
                return GetDoubleValue("SubmissionPrice", 0);
            }
            set
            {
                SetValue("SubmissionPrice", value);
            }
        }


        /// <summary>
        /// Deadline of the translation.
        /// </summary>
        public virtual DateTime SubmissionDeadline
        {
            get
            {
                return GetDateTimeValue("SubmissionDeadline", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubmissionDeadline", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Date of the submission (when it was created).
        /// </summary>
        public virtual DateTime SubmissionDate
        {
            get
            {
                return GetDateTimeValue("SubmissionDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubmissionDate", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Submission last modified date.
        /// </summary>
        public virtual DateTime SubmissionLastModified
        {
            get
            {
                return GetDateTimeValue("SubmissionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubmissionLastModified", value);
            }
        }


        /// <summary>
        /// Source culture of the submission.
        /// </summary>
        public virtual string SubmissionSourceCulture
        {
            get
            {
                return GetStringValue("SubmissionSourceCulture", "");
            }
            set
            {
                SetValue("SubmissionSourceCulture", value);
            }
        }


        /// <summary>
        /// ID of the translation service provider.
        /// </summary>
        public virtual int SubmissionServiceID
        {
            get
            {
                return GetIntegerValue("SubmissionServiceID", 0);
            }
            set
            {
                SetValue("SubmissionServiceID", value);
            }
        }


        /// <summary>
        /// Priority of the submission.
        /// </summary>
        public virtual int SubmissionPriority
        {
            get
            {
                return GetIntegerValue("SubmissionPriority", 0);
            }
            set
            {
                SetValue("SubmissionPriority", value);
            }
        }


        /// <summary>
        /// Status of the translation.
        /// </summary>
        public virtual TranslationStatusEnum SubmissionStatus
        {
            get
            {
                return (TranslationStatusEnum)GetIntegerValue("SubmissionStatus", 0);
            }
            set
            {
                SetValue("SubmissionStatus", (int)value);
            }
        }


        /// <summary>
        /// Status message of the translation.
        /// </summary>
        public virtual string SubmissionStatusMessage
        {
            get
            {
                return GetStringValue("SubmissionStatusMessage", "");
            }
            set
            {
                SetValue("SubmissionStatusMessage", value);
            }
        }


        /// <summary>
        /// Instructions for translators.
        /// </summary>
        public virtual string SubmissionInstructions
        {
            get
            {
                return GetStringValue("SubmissionInstructions", "");
            }
            set
            {
                SetValue("SubmissionInstructions", value);
            }
        }


        /// <summary>
        /// Indicates if binary files were included in the XLIFF.
        /// </summary>
        public virtual bool SubmissionTranslateAttachments
        {
            get
            {
                return GetBooleanValue("SubmissionTranslateAttachments", false);
            }
            set
            {
                SetValue("SubmissionTranslateAttachments", value);
            }
        }


        /// <summary>
        /// Number of items within the submission.
        /// </summary>
        public virtual int SubmissionItemCount
        {
            get
            {
                return GetIntegerValue("SubmissionItemCount", 0);
            }
            set
            {
                SetValue("SubmissionItemCount", value);
            }
        }


        /// <summary>
        /// Number of words to translate within the submission.
        /// </summary>
        public virtual int SubmissionWordCount
        {
            get
            {
                return GetIntegerValue("SubmissionWordCount", 0);
            }
            set
            {
                SetValue("SubmissionWordCount", value);
            }
        }


        /// <summary>
        /// Number of character to translate within the submission.
        /// </summary>
        public virtual int SubmissionCharCount
        {
            get
            {
                return GetIntegerValue("SubmissionCharCount", 0);
            }
            set
            {
                SetValue("SubmissionCharCount", value);
            }
        }


        /// <summary>
        /// Custom field used to pass parameters with submission to providers, this field does not correspond to any DB field.
        /// </summary>
        public virtual object SubmissionParameter
        {
            get;
            set;
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TranslationSubmissionInfoProvider.DeleteTranslationSubmissionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TranslationSubmissionInfoProvider.SetTranslationSubmissionInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            var result = base.SetValue(columnName, value);

            // Special columns treatment
            switch (columnName.ToLowerCSafe())
            {
                // Clear target cultures
                case "submissiontargetculture":
                    mSubmissionTargetCultures = null;
                    break;
            }

            return result;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TranslationSubmissionInfo object.
        /// </summary>
        public TranslationSubmissionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TranslationSubmissionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TranslationSubmissionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets service representation of the source service culture code
        /// </summary>
        public string GetServiceSourceCulture()
        {
            return TranslationServiceHelper.GetCultureCode(SubmissionSourceCulture, TranslationCultureMappingDirectionEnum.SystemToService);
        }


        /// <summary>
        /// Gets service representation of the target service culture codes
        /// </summary>
        public List<string> GetServiceTargetCultures()
        {
            return SubmissionTargetCultures.Select(c => TranslationServiceHelper.GetCultureCode(c, TranslationCultureMappingDirectionEnum.SystemToService)).ToList();
        }

        #endregion
    }
}
