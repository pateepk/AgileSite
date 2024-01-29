using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(ExternalLoginInfo), ExternalLoginInfo.OBJECT_TYPE)]
    
namespace CMS.Membership
{
    /// <summary>
    /// ExternalLoginInfo data container class.
    /// </summary>
    [Serializable]
    public class ExternalLoginInfo : AbstractInfo<ExternalLoginInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.externallogin";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ExternalLoginInfoProvider), OBJECT_TYPE, "CMS.ExternalLogin", "ExternalLoginID", null, null, null, null, null, null, "UserID", UserInfo.OBJECT_TYPE)
        {
            ModuleName = "cms.users",
            LogIntegration = false,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.Complete
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
            },
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() 
            {
                new ObjectDependency("UserID", "cms.user", ObjectDependencyEnum.Required), 
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// External login ID
        /// </summary>
        [DatabaseField]
        public virtual int ExternalLoginID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ExternalLoginID"), 0);
            }
            set
            {
                SetValue("ExternalLoginID", value);
            }
        }


        /// <summary>
        /// User ID
        /// </summary>
        [DatabaseField]
        public virtual int UserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserID"), 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// Login provider
        /// </summary>
        [DatabaseField]
        public virtual string LoginProvider
        {
            get
            {
                return ValidationHelper.GetString(GetValue("LoginProvider"), String.Empty);
            }
            set
            {
                SetValue("LoginProvider", value, String.Empty);
            }
        }


        /// <summary>
        /// Identity key
        /// </summary>
        [DatabaseField]
        public virtual string IdentityKey
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IdentityKey"), String.Empty);
            }
            set
            {
                SetValue("IdentityKey", value, String.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ExternalLoginInfoProvider.DeleteExternalLoginInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ExternalLoginInfoProvider.SetExternalLoginInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ExternalLoginInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ExternalLoginInfo object.
        /// </summary>
        public ExternalLoginInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ExternalLoginInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ExternalLoginInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}