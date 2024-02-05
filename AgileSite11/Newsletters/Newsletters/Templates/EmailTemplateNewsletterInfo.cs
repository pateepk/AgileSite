using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(EmailTemplateNewsletterInfo), EmailTemplateNewsletterInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// EmailTemplateNewsletterInfo data container class.
    /// </summary>
    public class EmailTemplateNewsletterInfo : AbstractInfo<EmailTemplateNewsletterInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.emailtemplatenewsletter";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EmailTemplateNewsletterInfoProvider), OBJECT_TYPE, "Newsletter.EmailTemplateNewsletter", null, null, null, null, null, null, null, "NewsletterID", NewsletterInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("TemplateID", EmailTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization
            },
            TouchCacheDependencies = true,
            LogEvents = true,
            ImportExportSettings =
            {
                LogExport = true
            },
            ContainsMacros = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Newsletter e-mail template ID.
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
        /// Newsletter ID.
        /// </summary>
        public virtual int NewsletterID
        {
            get
            {
                return GetIntegerValue("NewsletterID", 0);
            }
            set
            {
                SetValue("NewsletterID", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EmailTemplateNewsletterInfoProvider.DeleteEmailTemplateNewsletterInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EmailTemplateNewsletterInfoProvider.SetEmailTemplateNewsletterInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EmailTemplateNewsletterInfo object.
        /// </summary>
        public EmailTemplateNewsletterInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EmailTemplateNewsletterInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public EmailTemplateNewsletterInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}