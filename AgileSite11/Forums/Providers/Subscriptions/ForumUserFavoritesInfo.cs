using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(ForumUserFavoritesInfo), ForumUserFavoritesInfo.OBJECT_TYPE)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumUserFavoritesInfo data container class.
    /// </summary>
    public class ForumUserFavoritesInfo : AbstractInfo<ForumUserFavoritesInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "forums.forumuserfavorites";


        /// <summary>
        /// Type information.
        /// </summary>        
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumUserFavoritesInfoProvider), OBJECT_TYPE, "Forums.ForumUserFavorites", "FavoriteID", "FavoriteLastModified", "FavoriteGUID", null, "FavoriteName", null, "SiteID", "UserID", UserInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ForumID", ForumInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("PostID", ForumPostInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            ModuleName = ModuleName.FORUMS,
            IsBinding = true,
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                IsExportable = true,
                IsAutomaticallySelected = true
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


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
        /// Favorite name.
        /// </summary>
        public virtual string FavoriteName
        {
            get
            {
                return GetStringValue("FavoriteName", "");
            }
            set
            {
                SetValue("FavoriteName", value);
            }
        }


        /// <summary>
        /// Post ID.
        /// </summary>
        public virtual int PostID
        {
            get
            {
                return GetIntegerValue("PostID", 0);
            }
            set
            {
                SetValue("PostID", value, 0);
            }
        }


        /// <summary>
        /// Favorite ID.
        /// </summary>
        public virtual int FavoriteID
        {
            get
            {
                return GetIntegerValue("FavoriteID", 0);
            }
            set
            {
                SetValue("FavoriteID", value);
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
                SetValue("ForumID", value, 0);
            }
        }


        /// <summary>
        /// Favorite GUID.
        /// </summary>
        public virtual Guid FavoriteGUID
        {
            get
            {
                return GetGuidValue("FavoriteGUID", Guid.Empty);
            }
            set
            {
                SetValue("FavoriteGUID", value);
            }
        }


        /// <summary>
        /// Favorite last modified.
        /// </summary>
        public virtual DateTime FavoriteLastModified
        {
            get
            {
                return GetDateTimeValue("FavoriteLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FavoriteLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        protected override ICollection<string> GetCacheDependencies()
        {
            var result = base.GetCacheDependencies();

            result.Add("forums.forumuserfavorites|byuserid|" + UserID);

            return result;
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ForumUserFavoritesInfoProvider.DeleteForumUserFavoritesInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumUserFavoritesInfoProvider.SetForumUserFavoritesInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumUserFavoritesInfo object.
        /// </summary>
        public ForumUserFavoritesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumUserFavoritesInfo object from the given DataRow.
        /// </summary>
        public ForumUserFavoritesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}