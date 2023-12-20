using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(ForumModeratorInfo), ForumModeratorInfo.OBJECT_TYPE)]

namespace CMS.Forums
{
    /// <summary>
    /// Forum moderator binding.
    /// </summary>
    public class ForumModeratorInfo : AbstractInfo<ForumModeratorInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.FORUMMODERATOR;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumModeratorInfoProvider), OBJECT_TYPE, "Forums.ForumModerator", null, null, null, null, null, null, null, "ForumID", ForumInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            ModuleName = ModuleName.FORUMS,
            RegisterAsBindingToObjectTypes = new List<string>
            {
                ForumInfo.OBJECT_TYPE,
                ForumInfo.OBJECT_TYPE_GROUP
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// User ID.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// Forum ID.
        /// </summary>
        public virtual int ForumID
        {
            get
            {
                return GetIntegerValue("ForumID", 0);
            }
            set
            {
                SetValue("ForumID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ForumModeratorInfoProvider.DeleteForumModeratorInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumModeratorInfoProvider.SetForumModeratorInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumRoleInfo object.
        /// </summary>
        public ForumModeratorInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumRoleInfo object from the given DataRow.
        /// </summary>
        public ForumModeratorInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}