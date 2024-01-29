using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Newsletters;
using CMS.WebAnalytics;
using CMS.Scheduler;

[assembly: RegisterObjectType(typeof(IssueInfo), IssueInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(IssueInfo), IssueInfo.OBJECT_TYPE_VARIANT)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Issue data container class.
    /// </summary>
    public class IssueInfo : AbstractInfo<IssueInfo>
    {
        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NEWSLETTERISSUE;

        /// <summary>
        /// Object type for variant
        /// </summary>
        public const string OBJECT_TYPE_VARIANT = PredefinedObjectType.NEWSLETTERISSUEVARIANT;


        private IInfoObjectCollection mABTestVariants;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IssueInfoProvider), OBJECT_TYPE, "Newsletter.Issue", "IssueID", "IssueLastModified", "IssueGUID", null, "IssueDisplayName", null, "IssueSiteID", "IssueNewsletterID", NewsletterInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("IssueTemplateID", EmailTemplateInfo.OBJECT_TYPE)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING)
                },
                ExcludedStagingColumns = new List<string>
                {
                    "IssueStatus",
                    "IssueOpenedEmails",
                    "IssueBounces",
                    "IssueSentEmails",
                    "IssueUnsubscribed",
                    "IssueMailoutTime",
                    "IssueScheduledTaskID"
                }
            },
            LogEvents = true, // This value is overridden in NewsletterModule.Init by app setting
            TouchCacheDependencies = true,
            DeleteObjectWithAPI = true,
            SupportsVersioning = true,
            ModuleName = ModuleName.NEWSLETTER,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.Complete,
                AllowSingleExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING)
                }
            },
            TypeCondition = new TypeCondition().WhereIsNull("IssueVariantOfIssueID"),
            HasMetaFiles = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields =
                {
                    "IssueDisplayName"
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "IssueStatus",
                    "IssueOpenedEmails",
                    "IssueBounces",
                    "IssueSentEmails",
                    "IssueUnsubscribed",
                    "IssueMailoutTime",
                    "IssueScheduledTaskID"
                },
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("IssueWidgets")
                }
            },
            SupportsInvalidation = true
        };


        /// <summary>
        /// Additional info for A/B test issue
        /// </summary>
        public static ObjectTypeInfo TYPEINFOVARIANT = new ObjectTypeInfo(typeof(IssueInfoProvider), OBJECT_TYPE_VARIANT, "Newsletter.Issue", "IssueID", "IssueLastModified", "IssueGUID", null, "IssueDisplayName", null, "IssueSiteID", "IssueVariantOfIssueID", OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("IssueTemplateID", EmailTemplateInfo.OBJECT_TYPE),
                new ObjectDependency("IssueScheduledTaskID", TaskInfo.OBJECT_TYPE),
                new ObjectDependency("IssueNewsletterID", NewsletterInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ExcludedStagingColumns = new List<string>
                {
                    "IssueStatus",
                    "IssueOpenedEmails",
                    "IssueBounces",
                    "IssueSentEmails",
                    "IssueUnsubscribed",
                    "IssueMailoutTime",
                    "IssueScheduledTaskID"
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            DeleteObjectWithAPI = true,
            SupportsVersioning = true,
            ModuleName = ModuleName.NEWSLETTER,
            TypeCondition = new TypeCondition()
                            .WhereIsNotNull("IssueVariantOfIssueID")
                            .WhereEquals("IssueIsABTest", "1"),
            MacroCollectionName = "Newsletter.IssueVariant",
            HasMetaFiles = true,
            OriginalTypeInfo = TYPEINFO,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields =
                {
                    "IssueDisplayName",
                    "IssueVariantName"
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "IssueStatus",
                    "IssueOpenedEmails",
                    "IssueBounces",
                    "IssueSentEmails",
                    "IssueUnsubscribed",
                    "IssueMailoutTime",
                    "IssueScheduledTaskID"
                },
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("IssueWidgets")
                }
            },
            SupportsInvalidation = true
        };

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return IssueVariantOfIssueID != 0
                    ? TYPEINFOVARIANT
                    : TYPEINFO;
            }
        }


        /// <summary>
        /// Issue display name.
        /// </summary>
        [DatabaseField]
        public virtual string IssueDisplayName
        {
            get
            {
                return GetStringValue("IssueDisplayName", String.Empty);
            }
            set
            {
                SetValue("IssueDisplayName", value);
            }
        }


        /// <summary>
        /// IssueUnsubscribed.
        /// </summary>
        [DatabaseField]
        public virtual int IssueUnsubscribed
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueUnsubscribed"), 0);
            }
            set
            {
                SetValue("IssueUnsubscribed", value);
            }
        }


        /// <summary>
        /// IssueText.
        /// </summary>
        [DatabaseField]
        public virtual string IssueText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueText"), string.Empty);
            }
            set
            {
                SetValue("IssueText", value);
            }
        }


        /// <summary>
        /// IssueSubject.
        /// </summary>
        [DatabaseField]
        public virtual string IssueSubject
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueSubject"), string.Empty);
            }
            set
            {
                SetValue("IssueSubject", value);
            }
        }


        /// <summary>
        /// IssueID.
        /// </summary>
        [DatabaseField]
        public virtual int IssueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueID"), 0);
            }
            set
            {
                SetValue("IssueID", value);
            }
        }


        /// <summary>
        /// IssueSentEmails.
        /// </summary>
        [DatabaseField]
        public virtual int IssueSentEmails
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueSentEmails"), 0);
            }
            set
            {
                SetValue("IssueSentEmails", value);
            }
        }


        /// <summary>
        /// IssueTemplateID.
        /// </summary>
        [DatabaseField]
        public virtual int IssueTemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueTemplateID"), 0);
            }
            set
            {
                SetValue("IssueTemplateID", value, (value > 0));
            }
        }


        /// <summary>
        /// IssueNewsletterID.
        /// </summary>
        [DatabaseField]
        public virtual int IssueNewsletterID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueNewsletterID"), 0);
            }
            set
            {
                SetValue("IssueNewsletterID", value);
            }
        }


        /// <summary>
        /// Issue site ID.
        /// </summary>
        [DatabaseField]
        public virtual int IssueSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueSiteID"), 0);
            }
            set
            {
                SetValue("IssueSiteID", value);
            }
        }


        /// <summary>
        /// IssueMailoutTime.
        /// </summary>
        [DatabaseField]
        public virtual DateTime IssueMailoutTime
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("IssueMailoutTime"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("IssueMailoutTime", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Issue GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid IssueGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("IssueGUID"), Guid.Empty);
            }
            set
            {
                SetValue("IssueGUID", value, (value != Guid.Empty));
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime IssueLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("IssueLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("IssueLastModified", value, value != DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets the number of opened e-mails by subscribers with this newsletter issue.
        /// </summary>
        /// <value>The total number of opened e-mails containing this issue of newsletter. Default is 0</value>        
        [DatabaseField]
        public virtual int IssueOpenedEmails
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueOpenedEmails"), 0);
            }
            set
            {
                SetValue("IssueOpenedEmails", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of bounced e-mails for this issue.
        /// </summary>
        [DatabaseField]
        public virtual int IssueBounces
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueBounces"), 0);
            }
            set
            {
                SetValue("IssueBounces", value, value > 0);
            }
        }


        /// <summary>
        /// Gets or sets the status for this issue.
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public virtual IssueStatusEnum IssueStatus
        {
            get
            {
                return (IssueStatusEnum)ValidationHelper.GetInteger(GetValue("IssueStatus"), 0);
            }
            set
            {
                SetValue("IssueStatus", (int)value, (int)value > 0);
            }
        }


        /// <summary>
        /// A/B test flag, set to TRUE for main A/B test issue and its variants.
        /// </summary>
        [DatabaseField]
        public virtual bool IssueIsABTest
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IssueIsABTest"), false);
            }
            set
            {
                SetValue("IssueIsABTest", value);
            }
        }


        /// <summary>
        /// ID of parent issue (in case of A/B testing)
        /// </summary>
        [DatabaseField]
        public virtual int IssueVariantOfIssueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueVariantOfIssueID"), 0);
            }
            set
            {
                SetValue("IssueVariantOfIssueID", value, value > 0);
            }
        }


        /// <summary>
        /// Name of the A/B test variant. Use <see cref="IssueExtensions.GetVariantName(IssueInfo)"/> extension method instead to get variant name properly.
        /// </summary>
        [DatabaseField]
        public virtual string IssueVariantName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueVariantName"), string.Empty);
            }
            set
            {
                SetValue("IssueVariantName", value);
            }
        }


        /// <summary>
        /// Sender user-friendly name.
        /// </summary>
        [DatabaseField]
        public virtual string IssueSenderName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueSenderName"), string.Empty);
            }
            set
            {
                SetValue("IssueSenderName", value);
            }
        }


        /// <summary>
        /// Sender e-mail address.
        /// </summary>
        [DatabaseField]
        public virtual string IssueSenderEmail
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueSenderEmail"), string.Empty);
            }
            set
            {
                SetValue("IssueSenderEmail", value);
            }
        }


        /// <summary>
        /// Gets or sets ID of scheduled task for sending issue.
        /// </summary>
        [DatabaseField]
        public virtual int IssueScheduledTaskID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("IssueScheduledTaskID"), 0);
            }
            set
            {
                SetValue("IssueScheduledTaskID", value, value > 0);
            }
        }


        /// <summary>
        /// Gets if the issue is variant of an A/B test issue.
        /// </summary>
        public virtual bool IssueIsVariant
        {
            get
            {
                return (IssueIsABTest && IssueVariantOfIssueID > 0);
            }
        }


        /// <summary>
        /// Collection of all A/B test variants of the main newsletter issue.
        /// </summary>
        public virtual IInfoObjectCollection ABTestVariants
        {
            get
            {
                if (mABTestVariants == null)
                {
                    var col = new InfoObjectCollection(PredefinedObjectType.NEWSLETTERISSUE);

                    col.Where = new WhereCondition().WhereTrue("IssueIsABTest").WhereEquals("IssueVariantOfIssueID", ObjectID);

                    mABTestVariants = col;
                }
                return mABTestVariants;
            }
        }


        /// <summary>
        /// Indicates if the object versioning is supported.
        /// </summary>
        protected override bool SupportsVersioning
        {
            get
            {
                return base.SupportsVersioning && !IssueIsABTest;
            }
            set
            {
                base.SupportsVersioning = value;
            }
        }


        /// <summary>
        /// Enables or disabled automatic adding of UTM parameters into email links targeting current site.
        /// </summary>
        [DatabaseField]
        public bool IssueUseUTM
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IssueUseUTM"), false);
            }
            set
            {
                SetValue("IssueUseUTM", value);
            }
        }


        /// <summary>
        /// Gets or sets UTM source for the issue.
        /// UTM source is optionally added to all links targeting current site in the issue as 'utm_source' query parameter.
        /// </summary>
        [DatabaseField]
        public virtual string IssueUTMSource
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueUTMSource"), string.Empty);
            }
            set
            {
                SetValue("IssueUTMSource", value);
            }
        }


        /// <summary>
        /// Gets or sets UTM campaign for the issue.
        /// UTM campaign is optionally added to all links targeting current site in the issue as 'utm_campaign' query parameter.
        /// </summary>
        [DatabaseField]
        public string IssueUTMCampaign
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueUTMCampaign"), string.Empty);
            }
            set
            {
                SetValue("IssueUTMCampaign", value);
            }
        }


        /// <summary>
        /// Gets configuration of issue widgets.
        /// </summary>
        [DatabaseField]
        public string IssueWidgets
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssueWidgets"), string.Empty);
            }
            set
            {
                SetValue("IssueWidgets", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets preheader text used as a preview text in email clients.
        /// </summary>
        [DatabaseField]
        public string IssuePreheader
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssuePreheader"), string.Empty);
            }
            set
            {
                SetValue("IssuePreheader", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets plain text of the issue.
        /// </summary>
        [DatabaseField]
        public string IssuePlainText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IssuePlainText"), string.Empty);
            }
            set
            {
                SetValue("IssuePlainText", value, !string.IsNullOrEmpty(value));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty Issue object.
        /// </summary>
        public IssueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new Issue object from the given DataRow.
        /// </summary>
        public IssueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IssueInfoProvider.DeleteIssueInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IssueInfoProvider.SetIssueInfo(this);
        }


        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            NewsletterTasksManager.DeleteMailoutTask(IssueGUID, IssueSiteID);

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>AuthorIssues permission name for managing permission type, or base permission name otherwise</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                    return "AuthorIssues";

                default:
                    return base.GetPermissionName(permission);
            }
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource("CMS.Newsletter", "Destroy", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Inserts cloned issues into the DB.
        /// If a cloned issue has ABTestVariants (it is the main issue of an AB tested issue), another issues will be cloned automatically as they are children of this issue.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Reset issue as new
            IssueStatus = IssueStatusEnum.Idle;
            IssueBounces = 0;
            IssueOpenedEmails = 0;
            IssueSentEmails = 0;
            IssueMailoutTime = DateTimeHelper.ZERO_TIME;
            IssueUnsubscribed = 0;

            if (IssueIsVariant)
            {
                // Explicitly creates a scheduled task for each A/B test variant issue. 
                // Needs to insert the issue first, otherwise the task would be created with an incorrect name (no IssueID to use in TaskName), which can cause conflicts.
                IssueInfoProvider.SetIssueInfo(this);
                IssueScheduledTaskID = CreateScheduledTask(this);
            }
            else
            {
                // Sets IssueScheduledTaskID for issues that aren't A/B test variants.
                IssueScheduledTaskID = 0;
            }

            IssueInfoProvider.SetIssueInfo(this);
        }


        private int CreateScheduledTask(IssueInfo issue)
        {

            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            // Creates a new scheduled task
            TaskInfo task = NewsletterTasksManager.CreateMailoutTask(issue, DateTime.Now, false);
            TaskInfoProvider.SetTaskInfo(task);
            return task.TaskID;
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            IssueUnsubscribed = 0;
            IssueSentEmails = 0;
        }

        #endregion
    }
}