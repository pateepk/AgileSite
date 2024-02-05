using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Newsletters;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(EmailTemplateInfo), EmailTemplateInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// EmailTemplate data container class.
    /// </summary>
    public class EmailTemplateInfo : AbstractInfo<EmailTemplateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.NEWSLETTERTEMPLATE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailTemplateInfoProvider), OBJECT_TYPE, "Newsletter.EmailTemplate", "TemplateID", "TemplateLastModified", "TemplateGUID", "TemplateName", "TemplateDisplayName", null, "TemplateSiteID", null, null)
        {
            SupportsVersioning = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                }
            },

            TouchCacheDependencies = true,
            LogEvents = true,
            ModuleName = ModuleName.NEWSLETTER,
            ThumbnailGUIDColumn = "TemplateThumbnailGUID",
            HasMetaFiles = true,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            MacroCollectionName = "NewsletterEmailTemplate"
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// TemplateID.
        /// </summary>
        [DatabaseField]
        public virtual int TemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TemplateID"), 0);
            }
            set
            {
                SetValue("TemplateID", value);
            }
        }


        /// <summary>
        /// TemplateType.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual EmailTemplateTypeEnum TemplateType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateType"), "").ToEnum<EmailTemplateTypeEnum>();
            }
            set
            {
                SetValue("TemplateType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Template code.
        /// </summary>
        [DatabaseField]
        public virtual string TemplateCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateCode"), "");
            }
            set
            {
                SetValue("TemplateCode", value);
            }
        }


        /// <summary>
        /// TemplateName.
        /// </summary>
        [DatabaseField]
        public virtual string TemplateName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateName"), "");
            }
            set
            {
                SetValue("TemplateName", value);
            }
        }


        /// <summary>
        /// TemplateDisplayName.
        /// </summary>
        [DatabaseField]
        public virtual string TemplateDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateDisplayName"), "");
            }
            set
            {
                SetValue("TemplateDisplayName", value);
            }
        }


        /// <summary>
        /// TemplateSiteID.
        /// </summary>
        [DatabaseField]
        public virtual int TemplateSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TemplateSiteID"), 0);
            }
            set
            {
                SetValue("TemplateSiteID", value);
            }
        }


        /// <summary>
        /// Template GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid TemplateGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("TemplateGUID"), Guid.Empty);
            }
            set
            {
                if (value == Guid.Empty)
                {
                    SetValue("TemplateGUID", null);
                }
                else
                {
                    SetValue("TemplateGUID", value);
                }
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TemplateLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("TemplateLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value == DateTimeHelper.ZERO_TIME)
                {
                    SetValue("TemplateLastModified", null);
                }
                else
                {
                    SetValue("TemplateLastModified", value);
                }
            }
        }


        /// <summary>
        /// Template subject.
        /// </summary>
        [DatabaseField]
        public virtual string TemplateSubject
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateSubject"), "");
            }
            set
            {
                SetValue("TemplateSubject", value);
            }
        }


        /// <summary>
        /// Thumbnail from metafiles (GUID).
        /// </summary>
        [DatabaseField]
        public virtual Guid TemplateThumbnailGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("TemplateThumbnailGUID"), Guid.Empty);
            }
            set
            {
                SetValue("TemplateThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Template description.
        /// </summary>
        [DatabaseField]
        public virtual string TemplateDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateDescription"), String.Empty);
            }
            set
            {
                SetValue("TemplateDescription", value, String.Empty);
            }
        }


        /// <summary>
        /// Template font icon css class.
        /// </summary>
        [DatabaseField]
        public virtual string TemplateIconClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TemplateIconClass"), String.Empty);
            }
            set
            {
                SetValue("TemplateIconClass", value, String.Empty);
            }
        }


        /// <summary>
        /// Indicates whether styles withing the &lt;style&gt; tags will be inlined into the email markup prior sending the email.
        /// </summary>
        [DatabaseField]
        public virtual bool TemplateInlineCSS
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("TemplateInlineCSS"), false);
            }
            set
            {
                SetValue("TemplateInlineCSS", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailTemplateInfoProvider.DeleteEmailTemplateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailTemplateInfoProvider.SetEmailTemplateInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EmailTemplate object.
        /// </summary>
        public EmailTemplateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailTemplate object from the given DataRow.
        /// </summary>
        public EmailTemplateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>ManageTemplates permission name for managing permission type, or base permission name otherwise</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                    return "ManageTemplates";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}