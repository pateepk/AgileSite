using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumModeratorInfo management.
    /// </summary>
    public class ForumModeratorInfoProvider : AbstractInfoProvider<ForumModeratorInfo, ForumModeratorInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns forums ids in which is user moderator.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static DataSet GetUserModeratedForums(int userId)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumModerator.selectall", null, "UserID = " + userId);
        }


        /// <summary>
        /// Returns dataset with forum moderators.
        /// </summary>
        public static DataSet GetGroupForumsModerators(int groupID)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumModerator.selectall", null, "ForumID IN (SELECT ForumID FROM Forums_Forum WHERE ForumGroupID = " + groupID.ToString() + ")");
        }


        /// <summary>
        /// Returns dataset with forum moderators.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        public static DataSet GetGroupForumsModerators(string where, string orderBy)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumModerator.selectall", null, where, orderBy);
        }


        /// <summary>
        /// Returns the ForumModeratorInfo structure for the specified forumModerator.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="forumId">ForumID</param>
        public static ForumModeratorInfo GetForumModeratorInfo(int userId, int forumId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userId);
            parameters.Add("@ForumID", forumId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumModerator.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumModeratorInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumModerator.
        /// </summary>
        /// <param name="forumModerator">ForumModerator to set</param>
        public static void SetForumModeratorInfo(ForumModeratorInfo forumModerator)
        {
            if (forumModerator != null)
            {
                // Check IDs
                if ((forumModerator.UserID <= 0) || (forumModerator.ForumID <= 0))
                {
                    throw new Exception("[ForumModeratorInfoProvider.SetForumModeratorInfo]: Object IDs not set.");
                }

                // Get existing
                ForumModeratorInfo existing = GetForumModeratorInfo(forumModerator.UserID, forumModerator.ForumID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                    //forumModerator.Generalized.UpdateData();
                }
                else
                {
                    forumModerator.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[ForumModeratorInfoProvider.SetForumModeratorInfo]: No ForumModeratorInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified forumModerator.
        /// </summary>
        /// <param name="infoObj">ForumModerator object</param>
        public static void DeleteForumModeratorInfo(ForumModeratorInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified forumModerator.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="forumId">ForumID</param>
        public static void RemoveModeratorFromForum(int userId, int forumId)
        {
            ForumModeratorInfo infoObj = GetForumModeratorInfo(userId, forumId);
            DeleteForumModeratorInfo(infoObj);
        }


        /// <summary>
        /// Add moderator to forum, return moderator Id.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        public static void AddModeratorToForum(int userId, int forumId)
        {
            // Create new binding
            ForumModeratorInfo infoObj = new ForumModeratorInfo();
            infoObj.UserID = userId;
            infoObj.ForumID = forumId;

            // Save to the database
            SetForumModeratorInfo(infoObj);
        }

        #endregion
    }
}