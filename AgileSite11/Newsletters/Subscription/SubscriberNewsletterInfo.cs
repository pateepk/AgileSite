using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(SubscriberNewsletterInfo), SubscriberNewsletterInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// SubscriberNewsletterInfo data container class.
    /// </summary>
    public class SubscriberNewsletterInfo : AbstractInfo<SubscriberNewsletterInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.SUBSCRIBERTONEWSLETTER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SubscriberNewsletterInfoProvider), OBJECT_TYPE, "Newsletter.SubscriberNewsletter", "SubscriberNewsletterID", null, null, null, null, null, null, "SubscriberID", SubscriberInfo.OBJECT_TYPE)
                                              {
                                                  DependsOn = new List<ObjectDependency>
                                                    {
                                                        new ObjectDependency("NewsletterID", NewsletterInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
                                                    },
                                                  IsBinding = true,
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  SupportsVersioning = false,
                                                  AllowRestore = false,
                                                  RegisterAsBindingToObjectTypes = new List<string> { SubscriberInfo.OBJECT_TYPE, SubscriberInfo.OBJECT_TYPE_CONTACTGROUP, SubscriberInfo.OBJECT_TYPE_CONTACT },
                                                  ImportExportSettings = { LogExport = false, AllowSingleExport = false },
                                                  IncludeToVersionParentDataSet = false,
                                                  ContainsMacros = false,
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates when subscriber was subscribed to the newsletter.
        /// </summary>
        public virtual DateTime SubscribedWhen
        {
            get
            {
                return GetDateTimeValue("SubscribedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscribedWhen", value);
            }
        }


        /// <summary>
        /// ID of the newsletter.
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


        /// <summary>
        /// ID of the Subscriber.
        /// </summary>
        public virtual int SubscriberID
        {
            get
            {
                return GetIntegerValue("SubscriberID", 0);
            }
            set
            {
                SetValue("SubscriberID", value);
            }
        }


        /// <summary>
        /// Indicates if subscription is allowed.
        /// </summary>
        public virtual bool SubscriptionApproved
        {
            get
            {
                return GetBooleanValue("SubscriptionApproved", true);
            }
            set
            {
                SetValue("SubscriptionApproved", value);
            }
        }


        /// <summary>
        /// Gets or sets when subscription was approved.
        /// </summary>
        public virtual DateTime SubscriptionApprovedWhen
        {
            get
            {
                return GetDateTimeValue("SubscriptionApprovedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriptionApprovedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Hash of subscription approval.
        /// </summary>
        public virtual string SubscriptionApprovalHash
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubscriptionApprovalHash"), String.Empty);
            }
            set
            {
                SetValue("SubscriptionApprovalHash", value);
            }
        }


        /// <summary>
        /// Subscriber newsletter ID.
        /// </summary>
        public virtual int SubscriberNewsletterID
        {
            get
            {
                return GetIntegerValue("SubscriberNewsletterID", 0);
            }
            set
            {
                SetValue("SubscriberNewsletterID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SubscriberNewsletterInfoProvider.DeleteSubscriberNewsletterInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SubscriberNewsletterInfoProvider.SetSubscriberNewsletterInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SubscriberNewsletterInfo object.
        /// </summary>
        public SubscriberNewsletterInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SubscriberNewsletterInfo object from the given DataRow.
        /// </summary>
        public SubscriberNewsletterInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}