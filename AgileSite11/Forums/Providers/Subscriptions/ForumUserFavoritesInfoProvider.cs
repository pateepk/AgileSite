using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumUserFavoritesInfo management.
    /// </summary>
    public class ForumUserFavoritesInfoProvider : AbstractInfoProvider<ForumUserFavoritesInfo, ForumUserFavoritesInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the ForumUserFavoritesInfo structure for the specified forumUserFavorites.
        /// </summary>
        /// <param name="forumUserFavoritesId">ForumUserFavorites id</param>
        public static ForumUserFavoritesInfo GetForumUserFavoritesInfo(int forumUserFavoritesId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", forumUserFavoritesId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumUserFavorites.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumUserFavoritesInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumUserFavorites.
        /// </summary>
        /// <param name="forumUserFavorites">ForumUserFavorites to set</param>
        public static void SetForumUserFavoritesInfo(ForumUserFavoritesInfo forumUserFavorites)
        {
            if (forumUserFavorites != null)
            {
                if (forumUserFavorites.FavoriteID > 0)
                {
                    forumUserFavorites.Generalized.UpdateData();
                }
                else
                {
                    forumUserFavorites.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[ForumUserFavoritesInfoProvider.SetForumUserFavoritesInfo]: No ForumUserFavoritesInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified forumUserFavorites.
        /// </summary>
        /// <param name="infoObj">ForumUserFavorites object</param>
        public static void DeleteForumUserFavoritesInfo(ForumUserFavoritesInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified forumUserFavorites.
        /// </summary>
        /// <param name="forumUserFavoritesId">ForumUserFavorites id</param>
        public static void DeleteForumUserFavoritesInfo(int forumUserFavoritesId)
        {
            ForumUserFavoritesInfo infoObj = GetForumUserFavoritesInfo(forumUserFavoritesId);
            DeleteForumUserFavoritesInfo(infoObj);
        }


        /// <summary>
        /// Returns dataset with user's favorites.
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <param name="siteId">Site ID, if 0 no site is considered</param>
        public static DataSet GetFavorites(int userID, int siteId)
        {
            string whereCond = "Forums_UserFavorites.UserID = " + userID;

            // If site is set and restriction to where condition
            if (siteId > 0)
            {
                whereCond += " AND Forums_UserFavorites.SiteID = " + siteId;
            }

            return GetFavorites(whereCond, null);
        }


        /// <summary>
        /// Gets user-favorites dataset filtered by given WHERE condition and ordered by specified ORDER BY statement.
        /// </summary>
        /// <param name="where">Where condition to use</param>
        /// <param name="orderBy">Order by statement to use</param>        
        public static DataSet GetFavorites(string where, string orderBy)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumUserFavorites.selectalljoined", null, where, orderBy);
        }


        /// <summary>
        /// Adds post to user favorites (if the rercord does not exists yet).
        /// </summary>
        /// <param name="postId">ID of the favorite post</param>
        /// <param name="userId">ID of the user</param>
        /// <param name="favoriteName">Favorite name</param>
        /// <param name="siteId">Site id</param>
        /// <returns>Returns true if forum was added to favorites</returns>
        public static bool AddUserFavoritePost(int postId, int userId, string favoriteName, int siteId)
        {
            DataSet exists = GetFavorites("Forums_UserFavorites.UserID = " + userId + " AND Forums_UserFavorites.PostID = " + postId, null);
            if (DataHelper.DataSourceIsEmpty(exists))
            {
                ForumUserFavoritesInfo favoritePost = new ForumUserFavoritesInfo();
                favoritePost.PostID = postId;
                favoritePost.UserID = userId;
                favoritePost.FavoriteName = favoriteName;
                favoritePost.SiteID = siteId;
                SetForumUserFavoritesInfo(favoritePost);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Adds forum to user favorites (if the rercord does not exists yet).
        /// </summary>
        /// <param name="forumId">ID of the favorite forum</param>
        /// <param name="userId">ID of the user</param>
        /// <param name="favoriteName">Favorite name</param>
        /// <param name="siteId">Site id</param>
        /// <returns>Returns true if forum was added to favorites</returns>
        public static bool AddUserFavoriteForum(int forumId, int userId, string favoriteName, int siteId)
        {
            DataSet exists = GetFavorites("Forums_UserFavorites.UserID = " + userId + " AND Forums_UserFavorites.ForumID = " + forumId, null);
            if (DataHelper.DataSourceIsEmpty(exists))
            {
                ForumUserFavoritesInfo favoritePost = new ForumUserFavoritesInfo();
                favoritePost.ForumID = forumId;
                favoritePost.UserID = userId;
                favoritePost.FavoriteName = favoriteName;
                favoritePost.SiteID = siteId;
                SetForumUserFavoritesInfo(favoritePost);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Updates data for all records given by where condition
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Parameters</param>
        internal static void UpdateData(string updateExpression, string where, QueryDataParameters parameters)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }

        #endregion
    }
}