using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Polls;

[assembly: RegisterObjectType(typeof(PollAnswerInfo), PollAnswerInfo.OBJECT_TYPE)]

namespace CMS.Polls
{
    /// <summary>
    /// PollAnswerInfo data container class.
    /// </summary>
    public class PollAnswerInfo : AbstractInfo<PollAnswerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.POLLANSWER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PollAnswerInfoProvider), OBJECT_TYPE, "Polls.PollAnswer", "AnswerID", "AnswerLastModified", "AnswerGUID", null, "AnswerText", null, null, "AnswerPollID", PollInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            LogEvents = true,
            ModuleName = ModuleName.POLLS,
            OrderColumn = "AnswerOrder",
            RegisterAsChildToObjectTypes = new List<string>() { PollInfo.OBJECT_TYPE, PollInfo.OBJECT_TYPE_GROUP },
            EnabledColumn = "AnswerEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SynchronizationSettings = 
            {
                ExcludedStagingColumns = new List<string>
                {
                    "AnswerCount"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Order of current answer.
        /// </summary>
        public virtual int AnswerOrder
        {
            get
            {
                return GetIntegerValue("AnswerOrder", 0);
            }
            set
            {
                SetValue("AnswerOrder", value);
            }
        }


        /// <summary>
        /// Count of the answer.
        /// </summary>
        public virtual int AnswerCount
        {
            get
            {
                return GetIntegerValue("AnswerCount", 0);
            }
            set
            {
                SetValue("AnswerCount", value);
            }
        }


        /// <summary>
        /// ID of the answer.
        /// </summary>
        public virtual int AnswerID
        {
            get
            {
                return GetIntegerValue("AnswerID", 0);
            }
            set
            {
                SetValue("AnswerID", value);
            }
        }


        /// <summary>
        /// Indicates if answer is enabled.
        /// </summary>
        public virtual bool AnswerEnabled
        {
            get
            {
                return GetBooleanValue("AnswerEnabled", false);
            }
            set
            {
                SetValue("AnswerEnabled", value);
            }
        }


        /// <summary>
        /// Answer text.
        /// </summary>
        public virtual string AnswerText
        {
            get
            {
                return GetStringValue("AnswerText", string.Empty);
            }
            set
            {
                SetValue("AnswerText", value);
            }
        }


        /// <summary>
        /// ID of a poll where answer is registered.
        /// </summary>
        public virtual int AnswerPollID
        {
            get
            {
                return GetIntegerValue("AnswerPollID", 0);
            }
            set
            {
                SetValue("AnswerPollID", value);
            }
        }


        /// <summary>
        /// Answer GUID.
        /// </summary>
        public virtual Guid AnswerGUID
        {
            get
            {
                return GetGuidValue("AnswerGUID", Guid.Empty);
            }
            set
            {
                SetValue("AnswerGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime AnswerLastModified
        {
            get
            {
                return GetDateTimeValue("AnswerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AnswerLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if answer is open ended.
        /// </summary>
        public virtual bool AnswerIsOpenEnded
        {
            get
            {
                return !String.IsNullOrEmpty(GetStringValue("AnswerForm", string.Empty));
            }
        }


        /// <summary>
        /// BizForm where open ended answer data are stored.
        /// </summary>
        public virtual string AnswerForm
        {
            get
            {
                return GetStringValue("AnswerForm", string.Empty);
            }
            set
            {
                SetValue("AnswerForm", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Alternative form for open ended answer.
        /// </summary>
        public virtual string AnswerAlternativeForm
        {
            get
            {
                return GetStringValue("AnswerAlternativeForm", string.Empty);
            }
            set
            {
                SetValue("AnswerAlternativeForm", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Indicates if form for open ended answer is hidden if the answer is not selected.
        /// </summary>
        public virtual bool AnswerHideForm
        {
            get
            {
                return GetBooleanValue("AnswerHideForm", false);
            }
            set
            {
                SetValue("AnswerHideForm", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PollAnswerInfoProvider.DeletePollAnswerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PollAnswerInfoProvider.SetPollAnswerInfo(this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Reset answer count
            AnswerCount = 0;

            Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PollAnswerInfo object.
        /// </summary>
        public PollAnswerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PollAnswerInfo object from the given DataRow.
        /// </summary>
        public PollAnswerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            PollInfo pi;
            switch (permission)
            {
                case PermissionsEnum.Read:
                    pi = PollInfoProvider.GetPollInfo(AnswerPollID);
                    if (pi != null)
                    {
                        if ((pi.PollGroupID > 0) && (pi.PollSiteID > 0))
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "read", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                        if (pi.PollSiteID <= 0)
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.POLLS, "globalread", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                    }
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    pi = PollInfoProvider.GetPollInfo(AnswerPollID);
                    if (pi != null)
                    {

                        if ((pi.PollGroupID > 0) && (pi.PollSiteID > 0))
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.GROUPS, "manage", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                        if (pi.PollSiteID <= 0)
                        {
                            return UserInfoProvider.IsAuthorizedPerResource(ModuleName.POLLS, "globalmodify", siteName, (UserInfo)userInfo, exceptionOnFailure);
                        }
                    }
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}