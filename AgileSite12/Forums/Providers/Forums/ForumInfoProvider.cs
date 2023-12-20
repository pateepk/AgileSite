using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.ContinuousIntegration;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.Forums
{
    /// <summary>
    /// Class providing ForumInfo management.
    /// </summary>
    public class ForumInfoProvider : AbstractInfoProvider<ForumInfo, ForumInfoProvider>
    {
        #region "Constants and variables"

        private const string CLEAR_CACHED_FORUMS_COUNT_ACTION_NAME = "ClearCachedForumsCount";

        private static readonly Hashtable licForums = new Hashtable();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ForumInfoProvider()
            : base(ForumInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Methods"


        /// <summary>
        /// Returns a query for all the ForumInfo objects.
        /// </summary>
        public static ObjectQuery<ForumInfo> GetForums()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ForumInfo GetForumInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the number of message boards related to the specified document.
        /// </summary>
        /// <param name="documentId">Document ID to return number of boards for</param> 
        public static int GetForumsCount(int documentId)
        {
            return ProviderObject.GetForumsCountInternal(documentId);
        }


        /// <summary>
        /// Returns the ForumInfo structure for the specified forum.
        /// </summary>
        /// <param name="forumId">Forum id</param>
        public static ForumInfo GetForumInfo(int forumId)
        {
            return ProviderObject.GetInfoById(forumId);
        }


        /// <summary>
        /// Returns the ForumInfo structure for the specified forum.
        /// </summary>
        /// <param name="forumName">Forum name</param>
        /// <param name="siteId">Site ID</param>
        public static ForumInfo GetForumInfo(string forumName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(forumName, siteId);
        }


        /// <summary>
        /// Returns the ForumInfo structure for the specified forum.
        /// </summary>
        /// <param name="forumName">Forum name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Community group ID</param>
        public static ForumInfo GetForumInfo(string forumName, int siteId, int groupId)
        {
            return ProviderObject.GetInfoByCodeName(forumName, siteId, groupId);
        }


        /// <summary>
        /// Returns the ForumInfo structure for the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static ForumInfo GetForumInfoByDocument(int documentId)
        {
            return ProviderObject.GetForumInfoByDocumentInternal(documentId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forum.
        /// </summary>
        /// <param name="forum">Forum to set</param>
        public static void SetForumInfo(ForumInfo forum)
        {
            if (forum != null)
            {
                // Insert new item
                if (forum.ForumID <= 0)
                {
                    // Check license for insertion
                    if (!LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Forums, ObjectActionEnum.Insert))
                    {
                        LicenseHelper.GetAllAvailableKeys(FeatureEnum.Forums);
                    }
                }

                ProviderObject.SetInfo(forum);
            }
        }


        /// <summary>
        /// Deletes specified forum.
        /// </summary>
        /// <param name="forumObj">Forum object</param>
        public static void DeleteForumInfo(ForumInfo forumObj)
        {
            ProviderObject.DeleteInfo(forumObj);
        }


        /// <summary>
        /// Deletes specified forum.
        /// </summary>
        /// <param name="forumId">Forum id</param>
        public static void DeleteForumInfo(int forumId)
        {
            ForumInfo forumObj = GetForumInfo(forumId);
            DeleteForumInfo(forumObj);
        }


        /// <summary>
        /// Moves forum up in the order sequence (up = smaller ForumOrder = sooner in the navigation).
        /// </summary>
        /// <param name="forumId">Forum ID to move</param>
        public static void MoveForumUp(int forumId)
        {
            ProviderObject.MoveForumUpInternal(forumId);
        }


        /// <summary>
        /// Moves forum down in the order sequence (down = larger ForumpOrder = later in the navigation).
        /// </summary>
        /// <param name="forumId">Forum ID to move</param>
        public static void MoveForumDown(int forumId)
        {
            ProviderObject.MoveForumDownInternal(forumId);
        }


        /// <summary>
        /// Update threads, posts, last user and last time values.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        internal static void UpdateForumCounts(int forumId)
        {
            ProviderObject.UpdateForumCountsInternal(forumId);
        }


        /// <summary>
        /// Updates PostQuestionSolved field in threads. Sets true in threads where
        /// at least one post is marked as is answer (according to the forum AnswerLimit settings).
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="postIdPath">Post id path</param>
        internal static void UpdateQuestionSolved(int forumId, string postIdPath = null)
        {
            ProviderObject.UpdateQuestionSolvedInternal(forumId, postIdPath);
        }


        /// <summary>
        /// Adds specified role to the forum.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="forumId">Forum ID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void AddRoleToForum(int roleId, int forumId, int permissionId)
        {
            ForumRoleInfoProvider.AddRoleToForum(roleId, forumId, permissionId);
        }


        /// <summary>
        /// Removes the forum role assignment.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="forumId">Forum ID</param>
        /// <param name="permissionId">Permission ID</param>
        public static void RemoveRoleFromForum(int roleId, int forumId, int permissionId)
        {
            ForumRoleInfoProvider.RemoveRoleFromForum(roleId, forumId, permissionId);
        }


        /// <summary>
        /// Returns true if role is allowed for given forum.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="forumId">Forum ID</param>
        public static bool IsRoleAllowedForForum(int roleId, int forumId)
        {
            return ProviderObject.IsRoleAllowedForForumInternal(roleId, forumId);
        }


        /// <summary>
        /// Add moderator to forum, return moderator Id.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        public static void AddModerator(int userId, int forumId)
        {
            ForumModeratorInfoProvider.AddModeratorToForum(userId, forumId);
        }


        /// <summary>
        /// Remove moderator from forum id.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        public static void RemoveModerator(int userId, int forumId)
        {
            ForumModeratorInfoProvider.RemoveModeratorFromForum(userId, forumId);
        }


        /// <summary>
        /// Returns DataSet with moderators(User) to specified forum.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        public static DataSet GetModerators(int forumId)
        {
            return ProviderObject.GetModeratorsInternal(forumId);
        }


        /// <summary>
        /// Returns forum id's to moderator assigned by user id.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static DataSet GetModerator(int userId)
        {
            return ProviderObject.GetModeratorInternal(userId);
        }


        /// <summary>
        /// License version check.
        /// </summary>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            // Get limitations
            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, feature, (action != ObjectActionEnum.Insert));
            if (versionLimitations == 0)
            {
                return true;
            }

            if (domain != null)
            {
                domain = URLHelper.RemoveWWW(domain.ToLowerInvariant());
            }

            if (feature != FeatureEnum.Forums)
            {
                return true;
            }

            if (licForums[domain] == null)
            {
                licForums[domain] = GetForums().OnSite(LicenseHelper.GetSiteIDbyDomain(domain)).GetCount();
            }

            try
            {
                // Try add
                if (action == ObjectActionEnum.Insert)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(licForums[domain], -1) + 1)
                    {
                        return false;
                    }
                }

                // Get status
                if (action == ObjectActionEnum.Edit)
                {
                    if (versionLimitations < ValidationHelper.GetInteger(licForums[domain], 0))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                ClearLicCheck();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Clear license limitations table.
        /// </summary>
        public static void ClearLicCheck()
        {
            licForums.Clear();

            ProviderObject.CreateWebFarmTask(CLEAR_CACHED_FORUMS_COUNT_ACTION_NAME, null);
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public static bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null)
        {
            domainName = domainName ?? RequestContext.CurrentDomain;

            if (!LicenseVersionCheck(domainName, FeatureEnum.Forums, action))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Forums);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Refresh threads and posts count.
        /// </summary>
        /// <param name="forumName">Forum name</param>
        /// <param name="siteId">Site ID</param>
        public static void RefreshDataCount(string forumName, int siteId)
        {
            // Get forum info
            ForumInfo fi = GetForumInfo(forumName, siteId);

            if (fi != null)
            {
                UpdateForumCounts(fi.ForumID);
            }
        }


        /// <summary>
        /// Add security where condition to the existing where condition.
        /// </summary>
        /// <param name="where">Existing where condition</param>
        /// <param name="communityGroupId">Community group ID</param>
        /// <returns>Returns where condition</returns>
        public static string CombineSecurityWhereCondition(string where, int communityGroupId)
        {
            return ProviderObject.CombineSecurityWhereConditionInternal(where, communityGroupId);
        }


        /// <summary>
        /// Indicates whether specified user has assigned specific permission for the specified forum.
        /// </summary>
        /// <param name="forumId">Forum id</param>
        /// <param name="forumGroupId">ID of the Forum group of the forum</param>
        /// <param name="permissionName">Name of the permission to check</param>
        /// <param name="permissionScope">Permission to check</param>
        /// <param name="user">User info</param>
        public static bool IsAuthorizedPerForum(int forumId, int forumGroupId, string permissionName, SecurityAccessEnum permissionScope, UserInfo user)
        {
            return ProviderObject.IsAuthorizedPerForumInternal(forumId, forumGroupId, permissionName, permissionScope, user);
        }


        /// <summary>
        /// Returns site name with dependence on selected forum id.
        /// </summary>
        /// <param name="forumId">Forum Id</param>
        public static string GetForumSiteName(int forumId)
        {
            string siteName = null;

            // Get post site name
            ForumInfo fi = GetForumInfo(forumId);
            if (fi != null)
            {
                int siteId = fi.ForumSiteID;
                if (siteId <= 0)
                {
                    ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(fi.ForumGroupID);
                    if (fgi != null)
                    {
                        siteId = fgi.GroupSiteID;
                    }
                }

                // Get site name
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
                if (si != null)
                {
                    siteName = si.SiteName;
                }
            }

            return siteName;
        }


        /// <summary>
        /// Returns IDs of forums with specified name.
        /// </summary>
        /// <param name="name">Forum name, may contain wildcard e.g. "MyForum_*"</param>
        /// <param name="siteId">Site ID</param>        
        public static IEnumerable<int> GetForumIdsByForumName(string name, int siteId)
        {
            if (!name.Contains("*"))
            {
                // Single forum
                ForumInfo fi = GetForumInfo(name, siteId);
                if (fi != null)
                {
                    var result = new List<int>();
                    result.Add(fi.ForumID);

                    return result;
                }
            }
            else
            {
                // Wildcard, prepare the like expression
                name = SqlHelper.EscapeLikeQueryPatterns(name).Replace("*", "%");

#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                DataSet ds = GetForums().WhereEquals("ForumSiteID", siteId).WhereLike("ForumName", name).Column("ForumID");
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Build the list from result
                    var result = new List<int>();

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Add each forum ID
                        result.Add((int)dr["ForumID"]);
                    }

                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns search documents 
        /// </summary>
        /// <param name="sii">Search index info</param>
        /// <param name="where">Where condition for the documents</param>
        /// <param name="lastId">Last post ID that was processed</param>
        public static List<SearchDocument> GetSearchDocuments(SearchIndexInfo sii, WhereCondition where, int lastId)
        {
            // Check where condition
            if (String.IsNullOrEmpty(where.WhereCondition))
            {
                return null;
            }

            int batchSize = 10;
            if (sii != null)
            {
                batchSize = sii.IndexBatchSize;
            }

            // Get topN forum posts
            var q = ForumPostInfoProvider.GetForumPosts()
                .Source(s => s
                    .LeftJoin<ForumInfo>("PostForumID", "ForumID")
                    .LeftJoin<ForumGroupInfo>("ForumGroupID", "GroupID")
                )
                .TopN(batchSize)
                .Where(where)
                .Where("PostID", QueryOperator.LargerThan, lastId)
                .OrderBy("PostID");

            DataSet ds = q;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get iDocuments from dataset
                var iDocs = new List<SearchDocument>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ForumPostInfo fpi = new ForumPostInfo(dr);
                    var searchDocument = fpi.GetSearchDocument(sii);

                    iDocs.Add(searchDocument);
                }

                // Dispose dataset object
                ds.Dispose();

                // Return iDocument collection
                return iDocs;
            }

            return null;
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


        #region "Internal methods"

        /// <summary>
        /// Returns the number of message boards related to the specified document.
        /// </summary>
        /// <param name="documentId">Document ID to return number of boards for</param> 
        protected int GetForumsCountInternal(int documentId)
        {
            if (documentId <= 0)
            {
                return 0;
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@DocumentID", documentId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("forums.forum.documentforumscount", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0);
            }

            return 0;
        }


        /// <summary>
        /// Returns the ForumInfo structure for the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        protected ForumInfo GetForumInfoByDocumentInternal(int documentId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", documentId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.Forum.selectByDocumentId", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ForumInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ForumInfo info)
        {
            if (info != null)
            {
                bool isUpdated = false;
                int originalAnswerLimit = 0;
                bool originalModerated = false;
                info.SetValue("ForumSettings", info.ForumSettings.GetData());

                // Update the database
                if (info.ForumID > 0)
                {
                    originalAnswerLimit = ValidationHelper.GetInteger(info.GetOriginalValue("ForumIsAnswerLimit"), -1);
                    if (originalAnswerLimit == -1)
                    {
                        ForumGroupInfo fg = ForumGroupInfoProvider.GetForumGroupInfo(info.ForumGroupID);
                        if (fg != null)
                        {
                            originalAnswerLimit = fg.GroupIsAnswerLimit;
                        }
                    }
                    originalModerated = ValidationHelper.GetBoolean(info.GetOriginalValue("ForumModerated"), false);

                    isUpdated = true;
                }
                else
                {
                    // Creating new object, pre-fill default values
                    info.ForumPosts = 0;
                    info.ForumThreads = 0;
                }

                // Update in database
                base.SetInfo(info);

                if (isUpdated)
                {
                    // Remove forum attachments from cache
                    if (info.Generalized.TouchCacheDependencies)
                    {
                        CacheHelper.TouchKey("forumattachment|" + info.ForumID);
                    }

                    if (info.ForumIsAnswerLimit != originalAnswerLimit)
                    {
                        // If IsAnswerLimit is changed, update QuestionSolved fields
                        UpdateQuestionSolved(info.ForumID);
                    }

                    if (info.ForumModerated != originalModerated)
                    {
                        // If moderating is changed update shown post and thread numbers
                        UpdateForumCounts(info.ForumID);

                        DataSet ds = ForumPostInfoProvider.SelectForumPosts(info.ForumID, "/%", null, null, 0, false, 0, "PostIDPath");
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                string path = ValidationHelper.GetString(dr["PostIDPath"], String.Empty);
                                if (!String.IsNullOrEmpty(path))
                                {
                                    if (path.EndsWith("/", StringComparison.Ordinal))
                                    {
                                        path = path.Remove(path.LastIndexOf("/", StringComparison.Ordinal));
                                    }

                                    if (path.EndsWith("/%", StringComparison.Ordinal))
                                    {
                                        path = path.Remove(path.LastIndexOf("/%", StringComparison.Ordinal));
                                    }

                                    ForumPostInfoProvider.UpdateThreadCounts(path);
                                }
                            }
                        }
                    }
                }
                else if (!RepositoryActionContext.CurrentIsRestoreOperationRunning)
                {
                    // Init forum groups after insert if forum groups are not restored from the CI repository
                    info.Generalized.InitObjectsOrder();
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(info));
            }

            ClearLicCheck();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ForumInfo info)
        {
            if (info != null)
            {
                DeleteAttachments(info);

                // Delete the object
                base.DeleteInfo(info);

                ClearLicCheck();

                // Update user counts
                UserInfoProvider.UpdateUserCounts(ActivityPointsEnum.ForumPost, 0, 0);

                // Update search index
                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                    TreeNode node = tree.SelectSingleDocument(info.ForumDocumentID);

                    // Update search index for given document
                    if (node != null && DocumentHelper.IsSearchTaskCreationAllowed(node))
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, PredefinedObjectType.DOCUMENT, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                    }

                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, ForumInfo.OBJECT_TYPE, "postforumid", info.ForumID.ToString(), 0);
                }
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        protected override void ProcessWebFarmTaskInternal(string actionName, string data, byte[] binary)
        {
            if (String.Equals(actionName, CLEAR_CACHED_FORUMS_COUNT_ACTION_NAME, StringComparison.OrdinalIgnoreCase))
            {
                licForums.Clear();
            }
            else
            {
                base.ProcessWebFarmTaskInternal(actionName, data, binary);
            }
        }


        /// <summary>
        /// Delete attachments related to the current forum
        /// </summary>
        internal static void DeleteAttachments(ForumInfo info)
        {
            var whereCondiotion = new WhereCondition().WhereIn("AttachmentPostID", new ObjectQuery<ForumPostInfo>().WhereEquals("PostForumID", info.ForumID).Column("PostID"));
            ForumAttachmentInfoProvider.DeleteFiles(whereCondiotion.ToString(true), GetForumSiteName(info.ForumID));
        }


        /// <summary>
        /// Moves forum up in the order sequence (up = smaller ForumOrder = sooner in the navigation).
        /// </summary>
        /// <param name="forumId">Forum ID to move</param>
        protected void MoveForumUpInternal(int forumId)
        {
            ForumInfo fi = GetForumInfo(forumId);
            fi?.Generalized.MoveObjectUp();
        }


        /// <summary>
        /// Moves forum down in the order sequence (down = larger ForumpOrder = later in the navigation).
        /// </summary>
        /// <param name="forumId">Forum ID to move</param>
        protected void MoveForumDownInternal(int forumId)
        {
            ForumInfo fi = GetForumInfo(forumId);
            fi?.Generalized.MoveObjectDown();
        }


        /// <summary>
        /// Returns RoleNames in format rolename1;rolename2;rolename3....
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        protected string GetRoleNamesForForumInternal(int forumId)
        {
            if (forumId <= 0)
            {
                return "";
            }

            string toReturn = "";


            // Init the node orders to the proper sequence
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", forumId);

            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.Forum.GetRoleNameForForum", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    toReturn += ValidationHelper.GetString(dr["RoleName"], "") + ";";
                }
            }

            return toReturn;
        }


        /// <summary>
        /// Update threads, posts, last user and last time values.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        protected void UpdateForumCountsInternal(int forumId)
        {
            ForumInfo fi = GetForumInfo(forumId);
            if (fi != null)
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ID", forumId);
                parameters.Add("@ThreadLength", ForumPostInfoProvider.ForumPostIdLength + 2);

                ConnectionHelper.ExecuteQuery("Forums.Forum.UpdateForumCounts", parameters);
                fi.Generalized.Invalidate(false);
            }
        }


        /// <summary>
        /// Updates PostQuestionSolved field in threads. Sets true in threads where
        /// at least one post is marked as is answer (according to the forum AnswerLimit settings).
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="postIdPath">Post id path</param>
        protected void UpdateQuestionSolvedInternal(int forumId, string postIdPath)
        {
            ForumInfo fi = GetForumInfo(forumId);
            if (fi != null)
            {
                // Get thread path
                if (!String.IsNullOrEmpty(postIdPath))
                {
                    int slashIndex = postIdPath.IndexOf('/', 1);
                    if (slashIndex > 0)
                    {
                        postIdPath = postIdPath.Substring(0, slashIndex);
                    }
                }
                else
                {
                    postIdPath = String.Empty;
                }

                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ForumID", forumId);
                parameters.Add("@Limit", fi.ForumIsAnswerLimit);
                parameters.Add("@PostIDPath", postIdPath);

                ConnectionHelper.ExecuteQuery("Forums.Forum.UpdateQuestionSolved", parameters);

                fi.Generalized.Invalidate(false);
            }
        }


        /// <summary>
        /// Returns true if role is allowed for given forum.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="forumId">Forum ID</param>
        protected bool IsRoleAllowedForForumInternal(int roleId, int forumId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoleID", roleId);
            parameters.Add("@ForumID", forumId);

            DataSet ds = ConnectionHelper.ExecuteQuery("forums.forum.isroleallowedforforum", parameters);

            return (!DataHelper.DataSourceIsEmpty(ds));
        }


        /// <summary>
        /// Returns DataSet with moderators(User) to specified forum.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        protected InfoDataSet<UserInfo> GetModeratorsInternal(int forumId)
        {
            if (forumId <= 0)
            {
                return null;
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ForumID", forumId);
            parameters.EnsureDataSet<UserInfo>();

            // Get the data
            return ConnectionHelper.ExecuteQuery("forums.forum.getmoderators", parameters).As<UserInfo>();
        }


        /// <summary>
        /// Returns forum id's to moderator assigned by user id.
        /// </summary>
        /// <param name="userId">User ID</param>
        protected DataSet GetModeratorInternal(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", userId);

            return ConnectionHelper.ExecuteQuery("forums.forum.getmoderator", parameters);
        }


        /// <summary>
        /// Add security where condition to the existing where condition.
        /// </summary>
        /// <param name="where">Existing where condition</param>
        /// <param name="communityGroupId">Community group ID</param>
        /// <returns>Returns where condition</returns>
        protected string CombineSecurityWhereConditionInternal(string where, int communityGroupId)
        {
            // public < 100000
            // authenticated >99999    < 200000
            // access denied> 399999    500000 <
            // authenticated roles  >199999 < 300000   
            // group members > 299999   < 400000

            // If current user doesn't exist, return nothing
            if (MembershipContext.AuthenticatedUser == null)
            {
                return "(1=2) AND (" + where + ")";
            }

            if (!AuthenticationHelper.IsAuthenticated())
            {
                if (!String.IsNullOrEmpty(where))
                {
                    where = "(" + where + ")  AND ";
                }

                where += "(ForumAccess < 100000)";
            }
            else
            {
                // Global admin can see all posts
                if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    return where;
                }

                // Group administrator can see all posts too
                if (MembershipContext.AuthenticatedUser.IsGroupAdministrator(communityGroupId))
                {
                    return where;
                }

                // Get user roles
                string roleIds = MembershipContext.AuthenticatedUser.GetRoleIdList(true, true, SiteContext.CurrentSiteName);
                if (!String.IsNullOrEmpty(roleIds))
                {
                    roleIds = " AND RoleID IN(" + roleIds + ")";
                }

                if (!String.IsNullOrEmpty(where))
                {
                    where = "(" + where + ") AND ";
                }

                // Access denied, All users Authenticated
                where += " (ForumAccess < 500000) AND ((ForumAccess < 100000) OR (ForumAccess > 99999 AND ForumAccess < 200000)";

                // Group members
                if (communityGroupId > 0)
                {
                    // Is group member, admin or community admin
                    if (MembershipContext.AuthenticatedUser.IsGroupAdministrator(communityGroupId) || MembershipContext.AuthenticatedUser.IsGroupMember(communityGroupId))
                    {
                        where += "OR ((ForumAccess > 299999 AND ForumAccess < 400000))";
                    }
                }

                // Authorized roles
                where += " OR (ForumAccess > 199999 AND ForumAccess < 300000) AND ((SELECT Count(RoleID) FROM (SELECT RoleID, PermissionID, ForumID AS RoleForumID FROM Forums_ForumRoles) AS Forums_ForumRoles WHERE Forums_ForumRoles.RoleForumID = ForumID AND Forums_ForumRoles.PermissionID = (SELECT TOP 1 PermissionID FROM CMS_Permission WHERE PermissionName = 'AccessToForum') " + roleIds + ") > 0))";
            }

            return where;
        }


        /// <summary>
        /// Indicates whether specified user has assigned specific permission for the specified forum.
        /// </summary>
        /// <param name="forumId">ID of the forum to check permission for</param>
        /// <param name="forumGroupId">ID of the group to check permission for</param>
        /// <param name="permissionName">Name of the permission</param>
        /// <param name="permissionScope">User level</param>
        /// <param name="user">User info to check permission for</param>
        /// <returns></returns>
        protected bool IsAuthorizedPerForumInternal(int forumId, int forumGroupId, string permissionName, SecurityAccessEnum permissionScope, UserInfo user)
        {
            // Check whether user info is defined
            if (user == null)
            {
                return false;
            }

            // Try convert user object to current user info
            CurrentUserInfo cUser = user as CurrentUserInfo ?? new CurrentUserInfo(user, true);

            switch (permissionScope)
            {
                // Nobody
                case SecurityAccessEnum.Nobody:
                    return cUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);

                // All users
                case SecurityAccessEnum.AllUsers:
                    return true;

                // Authenticated users
                case SecurityAccessEnum.AuthenticatedUsers:
                    return !cUser.IsPublic();

                // Authorized roles
                case SecurityAccessEnum.AuthorizedRoles:

                    int siteId = SiteContext.CurrentSiteID;
                    ForumGroupInfo forumGroupInfo = ForumGroupInfoProvider.GetForumGroupInfo(forumGroupId);
                    if (forumGroupInfo != null)
                    {
                        siteId = forumGroupInfo.GroupSiteID;
                    }
                    return IsUserAuthorizedPerForum(forumId, permissionName, cUser, siteId);

                // Group members
                case SecurityAccessEnum.GroupMembers:
                    // Get the Community group ID from the Forum Group ID
                    forumGroupInfo = ForumGroupInfoProvider.GetForumGroupInfo(forumGroupId);
                    return (forumGroupInfo != null) && (cUser.IsGroupMember(forumGroupInfo.GroupGroupID));
            }

            return false;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Indicates whether specified user has assigned specific permission for the specified forum.
        /// </summary>
        /// <param name="forumId">ID of the forum to check permission for</param>
        /// <param name="permissionName">Name of the permission</param>
        /// <param name="userInfo">User info to check permission for</param>      
        /// <param name="siteId">ID of the site role should belongs to</param>
        private bool IsUserAuthorizedPerForum(int forumId, string permissionName, UserInfo userInfo, int siteId)
        {
            // Check whether user info is defined
            if (userInfo == null)
            {
                return false;
            }

            // Global administrator is always authorized
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            // Look for the permissions in cache
            Hashtable userForumsPerm = (Hashtable)RequestStockHelper.GetItem("ForumPermission_" + userInfo.UserID);
            if (userForumsPerm?[forumId] == null)
            {
                // Prepare parameters
                var parameters = new QueryDataParameters
                {
                    { "@SiteID", siteId },
                    { "@UserID", userInfo.UserID },
                    { "@ForumID", forumId },
                    { "@Now", DateTime.Now }
                };

                // Get user generic roles
                var genericRolesWhere = UserInfoProvider.GetGenericRoles(userInfo);

                parameters.AddMacro("##GENERICROLES##", genericRolesWhere);

                // Get the data from DB
                DataSet permissions = ConnectionHelper.ExecuteQuery("forums.forum.isauthorizedperforum", parameters);
                if (!DataHelper.DataSourceIsEmpty(permissions))
                {
                    // Go through the permissions and generate data to be cached
                    Hashtable userPermissions = new Hashtable();
                    foreach (DataRow permission in permissions.Tables[0].Rows)
                    {
                        userPermissions[permission["PermissionName"].ToString()] = true;
                    }

                    if (userForumsPerm == null)
                    {
                        userForumsPerm = new Hashtable();
                    }
                    userForumsPerm[forumId] = userPermissions;
                }

                // Save the data on forum permissions to cache
                if ((userForumsPerm != null) && (userForumsPerm.Count > 0))
                {
                    RequestStockHelper.Add("ForumPermission_" + userInfo.UserID, userForumsPerm);
                }
                else
                {
                    return false;
                }
            }

            // Get the info on requested permission
            if (userForumsPerm.Count > 0)
            {
                Hashtable forumPerm = (userForumsPerm[forumId] as Hashtable);
                if ((forumPerm != null) && (forumPerm.Count > 0))
                {
                    if (forumPerm.Contains(permissionName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}