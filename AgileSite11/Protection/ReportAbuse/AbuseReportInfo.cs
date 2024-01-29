using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Protection;

[assembly: RegisterObjectType(typeof(AbuseReportInfo), AbuseReportInfo.OBJECT_TYPE)]

namespace CMS.Protection
{
    /// <summary>
    /// AbuseReportInfo data container class.
    /// </summary>
    public class AbuseReportInfo : AbstractInfo<AbuseReportInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.abusereport";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AbuseReportInfoProvider), OBJECT_TYPE, "CMS.AbuseReport", "ReportID", null, "ReportGUID", null, "ReportTitle", null, "ReportSiteID", null, null)
                                              {
                                                  TouchCacheDependencies = true,
                                                  LogEvents = true,
                                                  AllowRestore = false,
                                                  ModuleName = "cms.abusereport",
                                                  SupportsCloning = false
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Report User ID.
        /// </summary>
        public virtual int ReportUserID
        {
            get
            {
                return GetIntegerValue("ReportUserID", 0);
            }
            set
            {
                SetValue("ReportUserID", value);
            }
        }


        /// <summary>
        /// Report When.
        /// </summary>
        public virtual DateTime ReportWhen
        {
            get
            {
                return GetDateTimeValue("ReportWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ReportWhen", value);
            }
        }


        /// <summary>
        /// Report GUID.
        /// </summary>
        public virtual Guid ReportGUID
        {
            get
            {
                return GetGuidValue("ReportGUID", Guid.Empty);
            }
            set
            {
                SetValue("ReportGUID", value);
            }
        }


        /// <summary>
        /// Report Status.
        /// </summary>
        public virtual AbuseReportStatusEnum ReportStatus
        {
            get
            {
                return (AbuseReportStatusEnum)GetValue("ReportStatus");
            }
            set
            {
                SetValue("ReportStatus", value);
            }
        }


        /// <summary>
        /// Report Title.
        /// </summary>
        public virtual string ReportTitle
        {
            get
            {
                return GetStringValue("ReportTitle", "");
            }
            set
            {
                SetValue("ReportTitle", value);
            }
        }


        /// <summary>
        /// Report Object Type.
        /// </summary>
        public virtual string ReportObjectType
        {
            get
            {
                return GetStringValue("ReportObjectType", "");
            }
            set
            {
                SetValue("ReportObjectType", value);
            }
        }


        /// <summary>
        /// Report Culture.
        /// </summary>
        public virtual string ReportCulture
        {
            get
            {
                return GetStringValue("ReportCulture", "");
            }
            set
            {
                SetValue("ReportCulture", value);
            }
        }


        /// <summary>
        /// Report Comment.
        /// </summary>
        public virtual string ReportComment
        {
            get
            {
                return GetStringValue("ReportComment", "");
            }
            set
            {
                SetValue("ReportComment", value);
            }
        }


        /// <summary>
        /// Report URL.
        /// </summary>
        public virtual string ReportURL
        {
            get
            {
                return GetStringValue("ReportURL", "");
            }
            set
            {
                SetValue("ReportURL", value);
            }
        }


        /// <summary>
        /// Report Site ID.
        /// </summary>
        public virtual int ReportSiteID
        {
            get
            {
                return GetIntegerValue("ReportSiteID", 0);
            }
            set
            {
                SetValue("ReportSiteID", value);
            }
        }


        /// <summary>
        /// Report ID.
        /// </summary>
        public virtual int ReportID
        {
            get
            {
                return GetIntegerValue("ReportID", 0);
            }
            set
            {
                SetValue("ReportID", value);
            }
        }


        /// <summary>
        /// Report Object ID.
        /// </summary>
        public virtual int ReportObjectID
        {
            get
            {
                return GetIntegerValue("ReportObjectID", 0);
            }
            set
            {
                SetValue("ReportObjectID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AbuseReportInfoProvider.DeleteAbuseReportInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AbuseReportInfoProvider.SetAbuseReportInfo(this);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            bool allowed = false;
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, "Manage", siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AbuseReportInfo object.
        /// </summary>
        public AbuseReportInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AbuseReportInfo object from the given DataRow.
        /// </summary>
        public AbuseReportInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}