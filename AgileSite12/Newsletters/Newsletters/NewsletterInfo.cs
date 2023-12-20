using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.DataEngine;
using CMS.Newsletters;
using CMS.WebAnalytics;
using CMS.MacroEngine;

[assembly: RegisterObjectType(typeof(NewsletterInfo), NewsletterInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Newsletter data container class.
    /// </summary>
    public class NewsletterInfo : AbstractInfo<NewsletterInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NEWSLETTER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(NewsletterInfoProvider), OBJECT_TYPE, "Newsletter.Newsletter", "NewsletterID", "NewsletterLastModified", "NewsletterGUID", "NewsletterName", "NewsletterDisplayName", null, "NewsletterSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>
            { 
                new ObjectDependency("NewsletterSubscriptionTemplateID", EmailTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("NewsletterUnsubscriptionTemplateID", EmailTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("NewsletterDynamicScheduledTaskID", TaskInfo.OBJECT_TYPE), 
                new ObjectDependency("NewsletterOptInTemplateID", EmailTemplateInfo.OBJECT_TYPE) 
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.NEWSLETTER,
            Feature = FeatureEnum.Newsletters,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "NewsletterDynamicScheduledTaskID"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the newsletter dynamic scheduled task ID.
        /// </summary>
        [DatabaseField]
        public virtual int NewsletterDynamicScheduledTaskID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewsletterDynamicScheduledTaskID"), 0);
            }
            set
            {
                SetValue("NewsletterDynamicScheduledTaskID", value, value > 0);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter source.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterSource
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterSource"), string.Empty);
            }
            set
            {
                SetValue("NewsletterSource", value);
            }
        }


        /// <summary>
        /// Gets or sets the email communication type.
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public virtual EmailCommunicationTypeEnum NewsletterType
        {
            get
            {
                return (EmailCommunicationTypeEnum)GetIntegerValue("NewsletterType", 0);
            }
            set
            {
                SetValue("NewsletterType", (int)value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter site ID.
        /// </summary>
        [DatabaseField]
        public virtual int NewsletterSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewsletterSiteID"), 0);
            }
            set
            {
                SetValue("NewsletterSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter name.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterName"), string.Empty);
            }
            set
            {
                SetValue("NewsletterName", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter subscription template ID.
        /// </summary>
        [DatabaseField]
        public virtual int NewsletterSubscriptionTemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewsletterSubscriptionTemplateID"), 0);
            }
            set
            {
                SetValue("NewsletterSubscriptionTemplateID", value);
            }
        }


        /// <summary>
        /// Gets or sets the subject of a dynamic newsletter.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterDynamicSubject
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterDynamicSubject"), string.Empty);
            }
            set
            {
                SetValue("NewsletterDynamicSubject", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter ID.
        /// </summary>
        [DatabaseField]
        public virtual int NewsletterID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewsletterID"), 0);
            }
            set
            {
                SetValue("NewsletterID", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter display name.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterDisplayName"), string.Empty);
            }
            set
            {
                SetValue("NewsletterDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter sender name.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterSenderName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterSenderName"), string.Empty);
            }
            set
            {
                SetValue("NewsletterSenderName", value);
            }
        }


        /// <summary>
        /// Gets or sets the URL of a dynamic newsletter.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterDynamicURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterDynamicURL"), string.Empty);
            }
            set
            {
                SetValue("NewsletterDynamicURL", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail address of a newsletter sender.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterSenderEmail
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterSenderEmail"), string.Empty);
            }
            set
            {
                SetValue("NewsletterSenderEmail", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter unsubscription template ID.
        /// </summary>
        [DatabaseField]
        public virtual int NewsletterUnsubscriptionTemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewsletterUnsubscriptionTemplateID"), 0);
            }
            set
            {
                SetValue("NewsletterUnsubscriptionTemplateID", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter unsubscription URL.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterUnsubscribeUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterUnsubscribeUrl"), string.Empty);
            }
            set
            {
                SetValue("NewsletterUnsubscribeUrl", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter base URL.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterBaseUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterBaseUrl"), string.Empty);
            }
            set
            {
                SetValue("NewsletterBaseUrl", value);
            }
        }


        /// <summary>
        /// Gets or sets the newsletter unique identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid NewsletterGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("NewsletterGUID"), Guid.Empty);
            }
            set
            {
                SetValue("NewsletterGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time the object was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime NewsletterLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("NewsletterLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("NewsletterLastModified", value, value != DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets or sets whether the newsletter should use double opt-in.
        /// </summary>
        [DatabaseField]
        public virtual bool NewsletterEnableOptIn
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NewsletterEnableOptIn"), false);
            }
            set
            {
                SetValue("NewsletterEnableOptIn", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the template which will be used for double opt-in confirmation.
        /// </summary>
        [DatabaseField]
        public virtual int NewsletterOptInTemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewsletterOptInTemplateID"), 0);
            }
            set
            {
                SetValue("NewsletterOptInTemplateID", value, value > 0);
            }
        }


        /// <summary>
        /// Gets or sets whether subscription confirmation should be sent after double opt-in e-mail.
        /// </summary>
        [DatabaseField]
        public virtual bool NewsletterSendOptInConfirmation
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NewsletterSendOptInConfirmation"), false);
            }
            set
            {
                SetValue("NewsletterSendOptInConfirmation", value);
            }
        }


        /// <summary>
        /// Gets or sets the URL of the double opt-in page.
        /// </summary>
        [DatabaseField]
        public virtual string NewsletterOptInApprovalURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterOptInApprovalURL"), string.Empty);
            }
            set
            {
                SetValue("NewsletterOptInApprovalURL", value);
            }
        }


        /// <summary>
        /// Gets or sets whether tracking of opened emails carrying newsletter issues is enabled.
        /// </summary>
        /// <value>A flag that determines if tracking is turned on for this newsletter. Default is false.</value>
        [DatabaseField]
        public virtual bool NewsletterTrackOpenEmails
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NewsletterTrackOpenEmails"), false);
            }
            set
            {
                SetValue("NewsletterTrackOpenEmails", value);
            }
        }


        /// <summary>
        /// Gets or sets whether tracking of clicked links in newsletter issues is enabled.
        /// </summary>
        /// <value>A flag that determines if tracking is turned on for this newsletter. Default is false.</value>
        [DatabaseField]
        public virtual bool NewsletterTrackClickedLinks
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NewsletterTrackClickedLinks"), false);
            }
            set
            {
                SetValue("NewsletterTrackClickedLinks", value);
            }
        }


        /// <summary>
        /// Gets or sets whether activities logging is enabled for this particular newsletter.
        /// </summary>
        /// <value>A flag that determines if activities logging is turned on for this newsletter. Default is false.</value>
        [DatabaseField]
        public virtual bool NewsletterLogActivity
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NewsletterLogActivity"), false);
            }
            set
            {
                SetValue("NewsletterLogActivity", value);
            }
        }


        /// <summary>
        /// Gets or sets the email addresses where draft emails are sent.
        /// </summary>
        /// <value>Semicolon separated list of email addresses.</value>
        [DatabaseField]
        public virtual string NewsletterDraftEmails
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NewsletterDraftEmails"), string.Empty);
            }
            set
            {
                SetValue("NewsletterDraftEmails", value);
            }
        }


        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            NewsletterInfoProvider.DeleteNewsletterInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            NewsletterInfoProvider.SetNewsletterInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            bool copySubscribers = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                copySubscribers = ValidationHelper.GetBoolean(p["newsletter.newsletter" + ".subscribers"], true);
            }

            if (copySubscribers)
            {
                if (settings.ExcludedOtherBindingTypes.Contains(SubscriberNewsletterInfo.OBJECT_TYPE))
                {
                    settings.ExcludedOtherBindingTypes.Remove(SubscriberNewsletterInfo.OBJECT_TYPE);
                }
            }
            else
            {
                settings.ExcludedOtherBindingTypes.Add(SubscriberNewsletterInfo.OBJECT_TYPE);
            }

            // Clone scheduled task
            if (NewsletterDynamicScheduledTaskID > 0)
            {
                var oldTask = TaskInfoProvider.GetTaskInfo(NewsletterDynamicScheduledTaskID);
                if (oldTask != null)
                {
                    // Create new task
                    var newTask = NewsletterTasksManager.CreateOrUpdateDynamicNewsletterTask(this, SchedulingHelper.DecodeInterval(oldTask.TaskInterval));

                    // Save the task
                    TaskInfoProvider.SetTaskInfo(newTask);

                    NewsletterDynamicScheduledTaskID = newTask.TaskID;
                }
            }

            Insert();
        }


        /// <summary>
        /// Gets the nice name of the newsletter based on its type
        /// </summary>
        [MacroMethod]
        public string GetNiceName()
        {
            return TypeHelper.GetNiceObjectTypeName(OBJECT_TYPE + "." + NewsletterType);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty Newsletter object.
        /// </summary>
        public NewsletterInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new Newsletter object from the given DataRow.
        /// </summary>
        public NewsletterInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>Configure permission name for managing permission type, or base permission name otherwise</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                    return "Configure";
                
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
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return NewsletterHelper.CheckLicense(action, domainName);
        }

        #endregion
    }
}