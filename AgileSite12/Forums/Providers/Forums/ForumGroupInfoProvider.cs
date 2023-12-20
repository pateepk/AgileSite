using System;
using System.Collections.Generic;
using System.Data;

using CMS.ContinuousIntegration;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumGroupInfo management.
    /// </summary>
    public class ForumGroupInfoProvider : AbstractInfoProvider<ForumGroupInfo, ForumGroupInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ForumGroupInfoProvider()
            : base(ForumGroupInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns a query for all the ForumGroupInfo objects.
        /// </summary>
        public static ObjectQuery<ForumGroupInfo> GetForumGroups()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ForumGroupInfo GetForumGroupInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the ForumGroupInfo structure for the specified forumGroup.
        /// </summary>
        /// <param name="forumGroupId">ForumGroup id</param>
        public static ForumGroupInfo GetForumGroupInfo(int forumGroupId)
        {
            return ProviderObject.GetInfoById(forumGroupId);
        }


        /// <summary>
        /// Returns or create AdHoc group info.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ForumGroupInfo GetAdHocGroupInfo(int siteId)
        {
            ForumGroupInfo fgi = null;

            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
            if (si != null)
            {
                fgi = GetForumGroupInfo("AdHocForumGroup", siteId);

                if (fgi != null)
                {
                    return fgi;
                }

                fgi = new ForumGroupInfo();

                fgi.GroupID = 0;
                fgi.GroupDisplayName = "AdHoc forum group";
                fgi.GroupName = "AdHocForumGroup";
                fgi.GroupBaseUrl = "";
                fgi.GroupUnsubscriptionUrl = "";
                fgi.GroupSiteID = siteId;

                SetForumGroupInfo(fgi);
            }

            return fgi;
        }


        /// <summary>
        /// Returns the ForumGroupInfo structure for the specified forumGroup.
        /// </summary>
        /// <param name="forumGroupName">ForumGroup name</param>
        /// <param name="siteId">Site id</param>
        /// <param name="communityGroupId">Community group id</param>
        public static ForumGroupInfo GetForumGroupInfo(string forumGroupName, int siteId, int communityGroupId)
        {
            return ProviderObject.GetInfoByCodeName(forumGroupName, siteId, communityGroupId);
        }


        /// <summary>
        /// Returns the ForumGroupInfo structure for the specified forumGroup.
        /// </summary>
        /// <param name="forumGroupName">ForumGroup name</param>
        /// <param name="siteId">Site ID</param>
        public static ForumGroupInfo GetForumGroupInfo(string forumGroupName, int siteId)
        {
            return GetForumGroupInfo(forumGroupName, siteId, 0);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumGroup.
        /// </summary>
        /// <param name="forumGroup">ForumGroup to set</param>
        public static void SetForumGroupInfo(ForumGroupInfo forumGroup)
        {
            ProviderObject.SetInfo(forumGroup);
        }


        /// <summary>
        /// Deletes specified forumGroup.
        /// </summary>
        /// <param name="forumGroupObj">ForumGroup object</param>
        public static void DeleteForumGroupInfo(ForumGroupInfo forumGroupObj)
        {
            ProviderObject.DeleteInfo(forumGroupObj);
        }


        /// <summary>
        /// Deletes specified forumGroup.
        /// </summary>
        /// <param name="forumGroupId">ForumGroup id</param>
        public static void DeleteForumGroupInfo(int forumGroupId)
        {
            ForumGroupInfo forumGroupObj = GetForumGroupInfo(forumGroupId);
            DeleteForumGroupInfo(forumGroupObj);
        }


        /// <summary>
        /// Moves group up in the order sequence (up = smaller GroupOrder = sooner in the navigation).
        /// </summary>
        /// <param name="forumGroupId">Group ID to move</param>
        public static void MoveGroupUp(int forumGroupId)
        {
            ProviderObject.MoveGroupUpInternal(forumGroupId);
        }


        /// <summary>
        /// Moves group down in the order sequence (down = larger GroupOrder = later in the navigation).
        /// </summary>
        /// <param name="forumGroupId">Group ID to move</param>
        public static void MoveGroupDown(int forumGroupId)
        {
            ProviderObject.MoveGroupDownInternal(forumGroupId);
        }


        /// <summary>
        /// Gets double opt-in interval for the site. It's time interval in hours within which user has to approve her subscription.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int DoubleOptInInterval(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSForumOptInInterval");
        }


        /// <summary>
        /// Gets setting value indicating if double opt-in confirmation emails should be sent
        /// </summary>
        public static bool SendOptInConfirmation(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSForumEnableOptInConfirmation");
        }


        /// <summary>
        /// Gets setting value indicating if double opt-in should be enabled
        /// </summary>
        public static bool EnableDoubleOptIn(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSForumEnableOptIn");
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ForumGroupInfo info)
        {
            bool insert = (info.GroupID <= 0);

            base.SetInfo(info);

            // Do not init objects order when restoring forum group from the CI repository
            if (insert && !RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                info.Generalized.InitObjectsOrder();
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ForumGroupInfo info)
        {
            if (info != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(info.GroupSiteID);
                if (si != null)
                {
                    // Delete attachments related to the current forum
                    ForumAttachmentInfoProvider.DeleteFiles("AttachmentPostID IN (SELECT PostID FROM Forums_ForumPost WHERE PostForumID IN (SELECT ForumID FROM Forums_Forum WHERE ForumGroupID = " + info.GroupID.ToString() + "))", si.SiteName);
                }

                // Delete object
                base.DeleteInfo(info);

                ProviderHelper.ClearHashtables(ForumInfo.OBJECT_TYPE, true);
                ForumInfoProvider.ClearLicCheck();

                // Update user counts
                UserInfoProvider.UpdateUserCounts(ActivityPointsEnum.ForumPost, 0, 0);

                // Update search index
                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    // Get forums related to this forum group
                    DataSet result = ForumInfoProvider.GetForums().WhereEquals("ForumGroupID", info.GroupID).Column("ForumDocumentID");
                    if (!DataHelper.DataSourceIsEmpty(result))
                    {
                        List<SearchTaskCreationParameters> taskParameters = new List<SearchTaskCreationParameters>();
                        foreach (DataRow dr in result.Tables[0].Rows)
                        {
                            // Get node related to this forum
                            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                            TreeNode node = tree.SelectSingleDocument((int)dr["ForumDocumentID"]);

                            // Get node document ID
                            if ((node != null) && node.PublishedVersionExists)
                            {
                                taskParameters.Add(new SearchTaskCreationParameters()
                                {
                                    TaskType = SearchTaskTypeEnum.Update,
                                    ObjectType = PredefinedObjectType.DOCUMENT,
                                    ObjectField = SearchFieldsConstants.ID,
                                    TaskValue = node.GetSearchID(),
                                    RelatedObjectID = node.DocumentID
                                });
                            }
                        }

                        SearchTaskInfoProvider.CreateTasks(taskParameters, true);
                    }

                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, ForumInfo.OBJECT_TYPE, "postforumgroupid", info.GroupID.ToString(), 0);
                }
            }
        }


        /// <summary>
        /// Moves group up in the order sequence (up = smaller GroupOrder = sooner in the navigation).
        /// </summary>
        /// <param name="forumGroupId">Group ID to move</param>
        protected void MoveGroupUpInternal(int forumGroupId)
        {
            ForumGroupInfo fgi = GetForumGroupInfo(forumGroupId);
            if (fgi != null)
            {
                fgi.Generalized.MoveObjectUp();
            }
        }


        /// <summary>
        /// Moves group down in the order sequence (down = larger GroupOrder = later in the navigation).
        /// </summary>
        /// <param name="forumGroupId">Group ID to move</param>
        protected void MoveGroupDownInternal(int forumGroupId)
        {
            ForumGroupInfo fgi = GetForumGroupInfo(forumGroupId);
            if (fgi != null)
            {
                fgi.Generalized.MoveObjectDown();
            }
        }

        #endregion
    }
}