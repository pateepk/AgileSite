using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterObjectType(typeof(UserMacroIdentityInfo), UserMacroIdentityInfo.OBJECT_TYPE)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// UserMacroIdentityInfo data container class.
    /// </summary>
	[Serializable]
    public class UserMacroIdentityInfo : AbstractInfo<UserMacroIdentityInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.usermacroidentity";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UserMacroIdentityInfoProvider), OBJECT_TYPE, "CMS.UserMacroIdentity", "UserMacroIdentityID", "UserMacroIdentityLastModified", "UserMacroIdentityUserGuid", null, null, null, null, "UserMacroIdentityUserID", PredefinedObjectType.USER)
        {
            ModuleName = "CMS.MacroEngine",
            TouchCacheDependencies = true,
            ContainsMacros = false,
            DefaultData = new DefaultDataSettings(),
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("UserMacroIdentityMacroIdentityID", "cms.macroidentity", ObjectDependencyEnum.NotRequired),
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                IsAutomaticallySelected = true,
                AllowSingleExport = false
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
            },
            LogEvents = true,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// User macro identity ID
        /// </summary>
		[DatabaseField]
        public virtual int UserMacroIdentityID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserMacroIdentityID"), 0);
            }
            set
            {
                SetValue("UserMacroIdentityID", value);
            }
        }


        /// <summary>
        /// User macro identity user ID
        /// </summary>
		[DatabaseField]
        public virtual int UserMacroIdentityUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserMacroIdentityUserID"), 0);
            }
            set
            {
                SetValue("UserMacroIdentityUserID", value);
            }
        }


        /// <summary>
        /// User macro identity macro identity ID
        /// </summary>
		[DatabaseField]
        public virtual int UserMacroIdentityMacroIdentityID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserMacroIdentityMacroIdentityID"), 0);
            }
            set
            {
                SetValue("UserMacroIdentityMacroIdentityID", value, 0);
            }
        }


        /// <summary>
        /// User macro identity user guid
        /// </summary>
		[DatabaseField]
        public virtual Guid UserMacroIdentityUserGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("UserMacroIdentityUserGuid"), Guid.Empty);
            }
            set
            {
                SetValue("UserMacroIdentityUserGuid", value);
            }
        }


        /// <summary>
        /// User macro identity last modified
        /// </summary>
		[DatabaseField]
        public virtual DateTime UserMacroIdentityLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("UserMacroIdentityLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UserMacroIdentityLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UserMacroIdentityInfoProvider.DeleteUserMacroIdentityInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UserMacroIdentityInfoProvider.SetUserMacroIdentityInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected UserMacroIdentityInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty UserMacroIdentityInfo object.
        /// </summary>
        public UserMacroIdentityInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserMacroIdentityInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public UserMacroIdentityInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}