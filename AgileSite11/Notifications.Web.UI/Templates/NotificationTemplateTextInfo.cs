using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Notifications.Web.UI;

[assembly: RegisterObjectType(typeof(NotificationTemplateTextInfo), NotificationTemplateTextInfo.OBJECT_TYPE)]

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Notification template text info data container class.
    /// </summary>
    public class NotificationTemplateTextInfo : AbstractInfo<NotificationTemplateTextInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "notification.templatetext";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(NotificationTemplateTextInfoProvider), OBJECT_TYPE, "Notification.TemplateText", "TemplateTextID", "TemplateTextLastModified", "TemplateTextGUID", null, null, null, null, "TemplateID", NotificationTemplateInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("GatewayID", NotificationGatewayInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            MacroCollectionName = "CMS.NotificationTemplateText",
            AllowRestore = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Template text ID.
        /// </summary>
        public virtual int TempalateTextID
        {
            get
            {
                return GetIntegerValue("TempalateTextID", 0);
            }
            set
            {
                SetValue("TempalateTextID", value);
            }
        }


        /// <summary>
        /// Template text GUID.
        /// </summary>
        public virtual Guid TempalateTextGUID
        {
            get
            {
                return GetGuidValue("TempalateTextGUID", Guid.Empty);
            }
            set
            {
                SetValue("TempalateTextGUID", value);
            }
        }


        /// <summary>
        /// Template text last modified.
        /// </summary>
        public virtual DateTime TempalateTextLastModified
        {
            get
            {
                return GetDateTimeValue("TempalateTextLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TempalateTextLastModified", value);
            }
        }


        /// <summary>
        /// Gateway ID.
        /// </summary>
        public virtual int GatewayID
        {
            get
            {
                return GetIntegerValue("GatewayID", 0);
            }
            set
            {
                SetValue("GatewayID", value);
            }
        }


        /// <summary>
        /// Template plain text.
        /// </summary>
        public virtual string TemplatePlainText
        {
            get
            {
                return GetStringValue("TemplatePlainText", "");
            }
            set
            {
                SetValue("TemplatePlainText", value);
            }
        }


        /// <summary>
        /// Template subject.
        /// </summary>
        public virtual string TemplateSubject
        {
            get
            {
                return GetStringValue("TemplateSubject", "");
            }
            set
            {
                SetValue("TemplateSubject", value);
            }
        }


        /// <summary>
        /// Template ID.
        /// </summary>
        public virtual int TemplateID
        {
            get
            {
                return GetIntegerValue("TemplateID", 0);
            }
            set
            {
                SetValue("TemplateID", value);
            }
        }


        /// <summary>
        /// Template HTML text.
        /// </summary>
        public virtual string TemplateHTMLText
        {
            get
            {
                return GetStringValue("TemplateHTMLText", "");
            }
            set
            {
                SetValue("TemplateHTMLText", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            NotificationTemplateTextInfoProvider.DeleteNotificationTemplateTextInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            NotificationTemplateTextInfoProvider.SetNotificationTemplateTextInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty NotificationTemplateTextInfo object.
        /// </summary>
        public NotificationTemplateTextInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new NotificationTemplateTextInfo object from the given DataRow.
        /// </summary>
        public NotificationTemplateTextInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}