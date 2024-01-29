using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportSubscriptionInfo), ReportSubscriptionInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// ReportSubscriptionInfo data container class.
    /// </summary>
    public class ReportSubscriptionInfo : AbstractInfo<ReportSubscriptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.REPORTSUBSCRIPTION;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ReportSubscriptionInfoProvider), OBJECT_TYPE, "Reporting.ReportSubscription", "ReportSubscriptionID", "ReportSubscriptionLastModified", "ReportSubscriptionGUID", null, "ReportSubscriptionSubject", null, "ReportSubscriptionSiteID", "ReportSubscriptionReportID", ReportInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>
            { 
                new ObjectDependency("ReportSubscriptionGraphID", ReportGraphInfo.OBJECT_TYPE),
                new ObjectDependency("ReportSubscriptionTableID", ReportTableInfo.OBJECT_TYPE),
                new ObjectDependency("ReportSubscriptionValueID", ReportValueInfo.OBJECT_TYPE),
                new ObjectDependency("ReportSubscriptionUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            ModuleName = ModuleName.REPORTING,
            ImportExportSettings =
            {
                LogExport = true
            },
            EnabledColumn = "ReportSubscriptionEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        private ContainerCustomData mReportSubscriptionSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of subscription's report.
        /// </summary>
        public virtual int ReportSubscriptionReportID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionReportID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionReportID", value);
            }
        }


        /// <summary>
        /// ID of subscription's graph object.
        /// </summary>
        public virtual int ReportSubscriptionGraphID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionGraphID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionGraphID", value, 0);
            }
        }


        /// <summary>
        /// CustomData used for load/store settings which can be used for customization
        /// </summary>
        public virtual ContainerCustomData ReportSubscriptionSettings
        {
            get
            {
                if (mReportSubscriptionSettings == null)
                {
                    mReportSubscriptionSettings = new ContainerCustomData(this, "ReportSubscriptionSettings");
                }
                return mReportSubscriptionSettings;
            }
        }


        /// <summary>
        /// ID of subscription's table object.
        /// </summary>
        public virtual int ReportSubscriptionTableID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionTableID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionTableID", value, 0);
            }
        }


        /// <summary>
        /// ID of subscription's user.
        /// </summary>
        public virtual int ReportSubscriptionUserID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionUserID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionUserID", value, 0);
            }
        }


        /// <summary>
        /// ID of subscription's site.
        /// </summary>
        public virtual int ReportSubscriptionSiteID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionSiteID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionSiteID", value, 0);
            }
        }


        /// <summary>
        /// ID of subscription's value object.
        /// </summary>
        public virtual int ReportSubscriptionValueID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionValueID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionValueID", value, 0);
            }
        }


        /// <summary>
        /// Condition for sending report.
        /// </summary>
        public virtual string ReportSubscriptionCondition
        {
            get
            {
                return GetStringValue("ReportSubscriptionCondition", "");
            }
            set
            {
                SetValue("ReportSubscriptionCondition", value);
            }
        }


        /// <summary>
        /// Email of subscribed user.
        /// </summary>
        public virtual string ReportSubscriptionEmail
        {
            get
            {
                return GetStringValue("ReportSubscriptionEmail", "");
            }
            set
            {
                SetValue("ReportSubscriptionEmail", value);
            }
        }


        /// <summary>
        /// If true, only reports with empty dataset are used.
        /// </summary>
        public virtual bool ReportSubscriptionOnlyNonEmpty
        {
            get
            {
                return GetBooleanValue("ReportSubscriptionOnlyNonEmpty", true);
            }
            set
            {
                SetValue("ReportSubscriptionOnlyNonEmpty", value);
            }
        }


        /// <summary>
        /// Condition for sending report.
        /// </summary>
        public virtual string ReportSubscriptionSubject
        {
            get
            {
                return GetStringValue("ReportSubscriptionSubject", "");
            }
            set
            {
                SetValue("ReportSubscriptionSubject", value);
            }
        }


        /// <summary>
        /// Subscription object ID.
        /// </summary>
        public virtual int ReportSubscriptionID
        {
            get
            {
                return GetIntegerValue("ReportSubscriptionID", 0);
            }
            set
            {
                SetValue("ReportSubscriptionID", value);
            }
        }


        /// <summary>
        /// Indicates whether subscription is enabled.
        /// </summary>
        public virtual bool ReportSubscriptionEnabled
        {
            get
            {
                return GetBooleanValue("ReportSubscriptionEnabled", false);
            }
            set
            {
                SetValue("ReportSubscriptionEnabled", value);
            }
        }


        /// <summary>
        /// Subscription last modification time.
        /// </summary>
        public virtual DateTime ReportSubscriptionLastModified
        {
            get
            {
                return GetDateTimeValue("ReportSubscriptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ReportSubscriptionLastModified", value);
            }
        }


        /// <summary>
        /// Date of next post.
        /// </summary>
        public virtual DateTime ReportSubscriptionNextPostDate
        {
            get
            {
                return GetDateTimeValue("ReportSubscriptionNextPostDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ReportSubscriptionNextPostDate", value);
            }
        }


        /// <summary>
        /// Date of subscription's last post.
        /// </summary>
        public virtual DateTime ReportSubscriptionLastPostDate
        {
            get
            {
                return GetDateTimeValue("ReportSubscriptionLastPostDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ReportSubscriptionLastPostDate", value);
            }
        }


        /// <summary>
        /// Subscription report's parameters.
        /// </summary>
        public virtual string ReportSubscriptionParameters
        {
            get
            {
                return GetStringValue("ReportSubscriptionParameters", "");
            }
            set
            {
                SetValue("ReportSubscriptionParameters", value);
            }
        }


        /// <summary>
        /// Subscription object unique identifier.
        /// </summary>
        public virtual Guid ReportSubscriptionGUID
        {
            get
            {
                return GetGuidValue("ReportSubscriptionGUID", Guid.Empty);
            }
            set
            {
                SetValue("ReportSubscriptionGUID", value);
            }
        }


        /// <summary>
        /// Subscription's interval. It shows how often report will be sent.
        /// </summary>
        public virtual string ReportSubscriptionInterval
        {
            get
            {
                return GetStringValue("ReportSubscriptionInterval", "");
            }
            set
            {
                SetValue("ReportSubscriptionInterval", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ReportSubscriptionInfoProvider.DeleteReportSubscriptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ReportSubscriptionInfoProvider.SetReportSubscriptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportSubscriptionInfo object.
        /// </summary>
        public ReportSubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportSubscriptionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ReportSubscriptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
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
            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                ReportSubscriptionEmail = ValidationHelper.GetString(p[PredefinedObjectType.REPORTSUBSCRIPTION + ".email"], ReportSubscriptionEmail);
            }

            Insert();
        }

        #endregion
    }
}
