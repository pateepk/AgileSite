using System;
using System.Data;
using System.Linq;
using CMS.DataEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumModeratorInfo management.
    /// </summary>
    public class ForumModeratorInfoProvider : AbstractInfoProvider<ForumModeratorInfo, ForumModeratorInfoProvider>
    {
        /// <summary>
        /// Returns forums ids in which is user moderator.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static DataSet GetUserModeratedForums(int userId)
        {
            return GetForumModerators().WhereEquals("UserID", userId);
        }


        /// <summary>
        /// Returns dataset with forum moderators.
        /// </summary>
        public static DataSet GetGroupForumsModerators(int groupID)
        {
            var groupForum = ForumInfoProvider.GetForums()
                .Column("ForumID")
                .WhereEquals("ForumGroupID", groupID);

            return GetForumModerators().WhereIn("ForumID", groupForum);
        }


        /// <summary>
        /// Returns dataset with forum moderators.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static DataSet GetGroupForumsModerators(string where)
        {
            return GetForumModerators().Where(where);
        }


        /// <summary>
        /// Returns all <see cref="ForumModeratorInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<ForumModeratorInfo> GetForumModerators()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="ForumModeratorInfo"/> binding structure.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="forumId">Forum ID.</param>  
        public static ForumModeratorInfo GetForumModeratorInfo(int userId, int forumId)
        {
            return ProviderObject.GetObjectQuery().TopN(1)
                .WhereEquals("UserID", userId)
                .WhereEquals("ForumID", forumId)
                .FirstOrDefault();
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
        /// Deletes specified <see cref="ForumModeratorInfo"/> binding.
        /// </summary>
        /// <param name="infoObj"><see cref="ForumModeratorInfo"/> object.</param>
        public static void DeleteForumModeratorInfo(ForumModeratorInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
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
    }
}