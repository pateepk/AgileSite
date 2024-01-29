using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Protection;

[assembly: RegisterObjectType(typeof(BadWordInfo), BadWordInfo.OBJECT_TYPE)]

namespace CMS.Protection
{
    /// <summary>
    /// BadWordInfo data container class.
    /// </summary>
    public class BadWordInfo : AbstractInfo<BadWordInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "badwords.word";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BadWordInfoProvider), OBJECT_TYPE, "BadWords.Word", "WordID", "WordLastModified", "WordGUID", null, "WordExpression", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            MacroCollectionName = "CMS.BadWord",
            ModuleName = "cms.badwords",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY)
                },
            },
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "WordExpression" }
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Word expression.
        /// </summary>
        public virtual string WordExpression
        {
            get
            {
                return GetStringValue("WordExpression", string.Empty);
            }
            set
            {
                SetValue("WordExpression", value);
            }
        }


        /// <summary>
        /// Word action.
        /// </summary>
        public virtual BadWordActionEnum WordAction
        {
            get
            {
                return (BadWordActionEnum)ValidationHelper.GetInteger(GetValue("WordAction"), 0);
            }
            set
            {
                switch (value)
                {
                    case BadWordActionEnum.None:
                        SetValue("WordAction", DBNull.Value);
                        break;

                    default:
                        SetValue("WordAction", Convert.ToInt32(value));
                        break;
                }
            }
        }


        /// <summary>
        /// Word ID.
        /// </summary>
        public virtual int WordID
        {
            get
            {
                return GetIntegerValue("WordID", 0);
            }
            set
            {
                SetValue("WordID", value);
            }
        }


        /// <summary>
        /// Word last modified.
        /// </summary>
        public virtual DateTime WordLastModified
        {
            get
            {
                return GetDateTimeValue("WordLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WordLastModified", value);
            }
        }


        /// <summary>
        /// Word GUID.
        /// </summary>
        public virtual Guid WordGUID
        {
            get
            {
                return GetGuidValue("WordGUID", Guid.Empty);
            }
            set
            {
                SetValue("WordGUID", value);
            }
        }


        /// <summary>
        /// Word replacement.
        /// </summary>
        public virtual string WordReplacement
        {
            get
            {
                return GetStringValue("WordReplacement", null);
            }
            set
            {
                SetValue("WordReplacement", value);
            }
        }


        /// <summary>
        /// Indicates if word is global (all cultures).
        /// </summary>
        public virtual bool WordIsGlobal
        {
            get
            {
                return GetBooleanValue("WordIsGlobal", false);
            }
            set
            {
                SetValue("WordIsGlobal", value);
            }
        }


        /// <summary>
        /// Indicates if word is regular expression.
        /// </summary>
        public virtual bool WordIsRegularExpression
        {
            get
            {
                return GetBooleanValue("WordIsRegularExpression", false);
            }
            set
            {
                SetValue("WordIsRegularExpression", value);
            }
        }


        /// <summary>
        /// Indicates if word should match whole word.
        /// </summary>
        public virtual bool WordMatchWholeWord
        {
            get
            {
                return GetBooleanValue("WordMatchWholeWord", false);
            }
            set
            {
                SetValue("WordMatchWholeWord", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BadWordInfoProvider.DeleteBadWordInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BadWordInfoProvider.SetBadWordInfo(this);
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
                case PermissionsEnum.Read:
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, "UseBadWords", siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BadWordInfo object.
        /// </summary>
        public BadWordInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BadWordInfo object from the given DataRow.
        /// </summary>
        public BadWordInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}