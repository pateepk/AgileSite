using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.EmailEngine;

[assembly: RegisterObjectType(typeof(EmailTemplateInfo), EmailTemplateInfo.OBJECT_TYPE)]

namespace CMS.EmailEngine
{
    /// <summary>
    /// EmailTemplateInfo data container class.
    /// </summary>
    public class EmailTemplateInfo : AbstractInfo<EmailTemplateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.emailtemplate";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailTemplateProvider), OBJECT_TYPE, "CMS.EmailTemplate", "EmailTemplateID", "EmailTemplateLastModified", "EmailTemplateGUID", "EmailTemplateName", "EmailTemplateDisplayName", null, "EmailTemplateSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                    {
                        new ObjectTreeLocation(SITE, DEVELOPMENT),
                        new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                    }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ModuleName = "cms.emailtemplates",
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT),
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            HasMetaFiles = true,
            SupportsLocking = true,
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the e-mail template ID.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateID")]
        public int TemplateID
        {
            get
            {
                return GetIntegerValue("EmailTemplateID", 0);
            }
            set
            {
                SetValue("EmailTemplateID", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template name.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateName")]
        public string TemplateName
        {
            get
            {
                return GetStringValue("EmailTemplateName", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateName", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template type.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateType")]
        public string TemplateType
        {
            get
            {
                string type = GetStringValue("EmailTemplateType", string.Empty);
                if (string.IsNullOrEmpty(type))
                {
                    type = "general";
                }
                return type;
            }
            set
            {
                SetValue("EmailTemplateType", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template display name.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateDisplayName")]
        public string TemplateDisplayName
        {
            get
            {
                return GetStringValue("EmailTemplateDisplayName", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateDisplayName", value);
            }
        }


        /// <summary>
        /// Template description.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateDescription")]
        public string TemplateDescription
        {
            get
            {
                return GetStringValue("EmailTemplateDescription", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template Cc recipients.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateCc")]
        public string TemplateCc
        {
            get
            {
                return GetStringValue("EmailTemplateCc", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateCc", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template Bcc recipients.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateBcc")]
        public string TemplateBcc
        {
            get
            {
                return GetStringValue("EmailTemplateBcc", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateBcc", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail address other than the From address to use to reply to this message.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateReplyTo")]
        public virtual string TemplateReplyTo
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EmailTemplateReplyTo"), string.Empty);
            }
            set
            {
                SetValue("EmailTemplateReplyTo", value, string.Empty);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template From address.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateFrom")]
        public string TemplateFrom
        {
            get
            {
                return GetStringValue("EmailTemplateFrom", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateFrom", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template subject.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateSubject")]
        public string TemplateSubject
        {
            get
            {
                return GetStringValue("EmailTemplateSubject", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateSubject", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template body text.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateText")]
        public string TemplateText
        {
            get
            {
                return GetStringValue("EmailTemplateText", string.Empty);
            }
            set
            {
                SetValue("EmailTemplateText", value);
            }
        }


        /// <summary>
        /// Gets or sets e-mail template body plain text.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplatePlainText")]
        public string TemplatePlainText
        {
            get
            {
                return GetStringValue("EmailTemplatePlainText", string.Empty);
            }
            set
            {
                SetValue("EmailTemplatePlainText", value);
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template site ID.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateSiteID")]
        public int TemplateSiteID
        {
            get
            {
                return GetIntegerValue("EmailTemplateSiteID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("EmailTemplateSiteID", value);
                }
                else
                {
                    SetValue("EmailTemplateSiteID", null);
                }
            }
        }


        /// <summary>
        /// Gets or sets the e-mail template unique identifier.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateGUID")]
        public virtual Guid TemplateGUID
        {
            get
            {
                return GetGuidValue("EmailTemplateGUID", Guid.Empty);
            }
            set
            {
                SetValue("EmailTemplateGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Gets or setsthe date when the object was last modified.
        /// </summary>
        [DatabaseField(ColumnName = "EmailTemplateLastModified")]
        public virtual DateTime TemplateLastModified
        {
            get
            {
                return GetDateTimeValue("EmailTemplateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EmailTemplateLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailTemplateProvider.DeleteEmailTemplate(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailTemplateProvider.SetEmailTemplate(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty EmailTemplateInfo structure.
        /// </summary>
        public EmailTemplateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, initializes the object fromt the DataRow.
        /// </summary>
        /// <param name="dr">Datarow with the object data</param>
        public EmailTemplateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"


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
                    return userInfo.IsAuthorizedPerResource("CMS.EmailTemplates", "Destroy", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}