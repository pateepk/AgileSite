using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(EmailWidgetTemplateInfo), EmailWidgetTemplateInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// EmailWidgetTemplateInfo data container class.
    /// </summary>
    [Serializable]
    public class EmailWidgetTemplateInfo : AbstractInfo<EmailWidgetTemplateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.emailwidgettemplate";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailWidgetTemplateInfoProvider), OBJECT_TYPE, "Newsletter.EmailWidgetTemplate", "EmailWidgetTemplateID", null, null, null, null, null, null, "EmailWidgetID", EmailWidgetInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.NEWSLETTER,
            TouchCacheDependencies = true,
            IsBinding = true,
            ContainsMacros = false,
            LogEvents = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("TemplateID", EmailTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Email widget template ID
        /// </summary>
		[DatabaseField]
        public virtual int EmailWidgetTemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailWidgetTemplateID"), 0);
            }
            set
            {
                SetValue("EmailWidgetTemplateID", value);
            }
        }


        /// <summary>
        /// Email widget ID
        /// </summary>
		[DatabaseField]
        public virtual int EmailWidgetID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("EmailWidgetID"), 0);
            }
            set
            {
                SetValue("EmailWidgetID", value);
            }
        }


        /// <summary>
        /// Template ID
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailWidgetTemplateInfoProvider.DeleteEmailWidgetTemplateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailWidgetTemplateInfoProvider.SetEmailWidgetTemplateInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected EmailWidgetTemplateInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty EmailWidgetTemplateInfo object.
        /// </summary>
        public EmailWidgetTemplateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailWidgetTemplateInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public EmailWidgetTemplateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type.</param>
        /// <returns>ManageTemplates permission name for managing permission type, or base permission name otherwise.</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Modify:
                    return "ManageTemplates";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}