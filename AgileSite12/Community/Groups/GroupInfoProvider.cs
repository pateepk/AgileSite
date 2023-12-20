using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Taxonomy;

namespace CMS.Community
{
    using TypedDataSet = InfoDataSet<GroupInfo>;

    /// <summary>
    /// Class providing GroupInfo management.
    /// </summary>
    public class GroupInfoProvider : AbstractInfoProvider<GroupInfo, GroupInfoProvider>
    {
        #region "Constants and variables"

        /// <summary>
        /// Current group.
        /// </summary>
        public const string CURRENT_GROUP = PredefinedObjectType.COMMUNITY_CURRENT_GROUP;

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the GroupInfo structure for the specified group.
        /// </summary>
        /// <param name="groupId">Group ID</param>
        public static GroupInfo GetGroupInfo(int groupId)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Groups);
            }

            return ProviderObject.GetInfoById(groupId);
        }


        /// <summary>
        /// Returns the GroupInfo structure for the specified group.
        /// </summary>
        /// <param name="groupGuid">Group GUID</param>
        public static GroupInfo GetGroupInfo(Guid groupGuid)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Groups);
            }

            return ProviderObject.GetInfoByGuid(groupGuid);
        }


        /// <summary>
        /// Returns the GroupInfo structure for the specified group.
        /// </summary>
        /// <param name="groupCodeName">Code name of the group to get</param>     
        /// <param name="siteName">Group site name</param>
        public static GroupInfo GetGroupInfo(string groupCodeName, string siteName)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Groups);
            }

            return ProviderObject.GetGroupInfoInternal(groupCodeName, siteName);
        }


        /// <summary>
        /// Returns object query for groups.
        /// </summary>
        public static ObjectQuery<GroupInfo> GetGroups()
        {
            // Check license
            if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                if (!LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.Groups))
                {
                    return new ObjectQuery<GroupInfo>().NoResults();
                }
            }

            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the dataset of GroupInfo structure managed by user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="orderBy">Order by statement used to sort the data</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty select all columns</param>
        /// <returns>Data set with records.</returns>
        public static TypedDataSet GetGroupsManagedByUser(int userId, string siteName, string orderBy, int topN, string columns)
        {
            // Check license
            if (LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Groups))
            {
                return ProviderObject.GetGroupsManagedByUserInternal(userId, siteName, orderBy, topN, columns);
            }

            return null;
        }


        /// <summary>
        /// Returns first N records on groups according to the specified conditions from groups view.
        /// </summary>
        /// <param name="where">Where condition to apply</param>
        /// <param name="orderBy">Order by statement used to sort the data</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty select all columns</param>
        /// <returns>Data set with records.</returns>
        public static TypedDataSet GetGroupsView(string where, string orderBy, int topN, string columns)
        {
            // Check license
            if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                if (!LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.Groups))
                {
                    return null;
                }
            }

            return ProviderObject.GetGroupsViewInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified group.
        /// </summary>
        /// <param name="group">Group to set</param>
        public static void SetGroupInfo(GroupInfo group)
        {
            ProviderObject.SetInfo(group);
        }


        /// <summary>
        /// Deletes specified group including related document.
        /// </summary>
        /// <param name="infoObj">Group object</param>
        public static void DeleteGroupInfo(GroupInfo infoObj)
        {
            DeleteGroupInfo(infoObj, true);
        }


        /// <summary>
        /// Deletes specified group including related document if needed.
        /// </summary>
        /// <param name="infoObj">Group object</param>
        /// <param name="deleteRelatedDocument">If true related document is deleted too</param>
        public static void DeleteGroupInfo(GroupInfo infoObj, bool deleteRelatedDocument)
        {
            ProviderObject.DeleteGroupInfoInternal(infoObj, deleteRelatedDocument);
        }


        /// <summary>
        /// Deletes specified group.
        /// </summary>
        /// <param name="groupId">Group id</param>
        public static void DeleteGroupInfo(int groupId)
        {
            GroupInfo infoObj = GetGroupInfo(groupId);
            DeleteGroupInfo(infoObj);
        }


        /// <summary>
        /// Returns group profile path with resolved macros from settings.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="siteName">Site name</param>
        public static string GetGroupProfilePath(string groupName, string siteName)
        {
            string path = SettingsKeyInfoProvider.GetValue(siteName + ".CMSGroupProfilePath");
            return MacroResolver.Resolve(path.Replace("{GroupName}", groupName));
        }


        /// <summary>
        /// Returns group management path with resolved macros from settings.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="siteName">Site name</param>
        public static string GetGroupManagementPath(string groupName, string siteName)
        {
            string path = SettingsKeyInfoProvider.GetValue(siteName + ".CMSGroupManagementPath");
            return MacroResolver.Resolve(path.Replace("{GroupName}", groupName));
        }


        /// <summary>
        /// Returns group security access path with resolved macros from settings.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="siteName">Site name</param>
        public static string GetGroupSecurityAccessPath(string groupName, string siteName)
        {
            string path = SettingsKeyInfoProvider.GetValue(siteName + ".CMSGroupsSecurityAccessPath");
            return MacroResolver.Resolve(path.Replace("{GroupName}", groupName));
        }


        /// <summary>
        /// Gets number of user groups.
        /// </summary>
        /// <param name="userInfo">User info</param>
        /// <param name="siteInfo">Site info</param>
        public static int GetUserGroupsCount(UserInfo userInfo, SiteInfo siteInfo)
        {
            return ProviderObject.GetUserGroupsCountInternal(userInfo, siteInfo);
        }


        /// <summary>
        /// Returns true if CurrentUser is authorized for the specified action in the group.
        /// </summary>
        /// <param name="permissionName">Name of the permission to check (createpages, editpages, deletepages)</param>
        /// <param name="groupId">Group ID</param>
        public static bool CheckPermission(string permissionName, int groupId)
        {
            // Get group object
            GroupInfo group = GetGroupInfo(groupId);
            if (group != null)
            {
                // Get group's security access
                SecurityAccessEnum securityGroup = SecurityAccessEnum.Nobody;
                switch (permissionName.ToLowerCSafe())
                {
                    case "createpages":
                        securityGroup = group.AllowCreate;
                        break;

                    case "editpages":
                        securityGroup = group.AllowModify;
                        break;

                    case "deletepages":
                        securityGroup = group.AllowDelete;
                        break;
                }

                switch (securityGroup)
                {
                    case SecurityAccessEnum.Nobody:
                        // Nobody has permission
                        return false;

                    case SecurityAccessEnum.AllUsers:
                        // Everybody has permission
                        return true;

                    case SecurityAccessEnum.AuthenticatedUsers:
                        // Authenticated users have permission
                        return ((MembershipContext.AuthenticatedUser != null) && (AuthenticationHelper.IsAuthenticated()));

                    case SecurityAccessEnum.AuthorizedRoles:
                        // Get group permission object
                        PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo(permissionName, "cms.groups", null);
                        // Authorized roles have permission
                        return ((permission != null) && (MembershipContext.AuthenticatedUser != null) && MembershipContext.AuthenticatedUser.IsGroupMember(groupId)
                                && (IsAuthorizedPerGroup(groupId, MembershipContext.AuthenticatedUser.UserID, permission.PermissionId)));

                    case SecurityAccessEnum.GroupMembers:
                        // Group members have permission
                        return ((MembershipContext.AuthenticatedUser != null) && (MembershipContext.AuthenticatedUser.IsGroupMember(groupId)));
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if the user as a role member has permission for the specified group.
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="permissionId">Permission ID</param>
        private static bool IsAuthorizedPerGroup(int groupId, int userId, int permissionId)
        {
            return ProviderObject.IsAuthorizedPerGroupInternal(groupId, userId, permissionId);
        }


        /// <summary>
        /// Creates group pages for specified group.
        /// </summary>
        /// <param name="gi">Group info object</param>
        /// <param name="sourcePath">Source template path</param>
        /// <param name="targetPath">Target template path</param>
        /// <param name="groupProfileURLPath">URL path to group profile. Macro {groupname} is automatically resolved to current group</param>
        /// <param name="culture">Document culture</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture</param>
        /// <param name="user">User info</param>
        /// <param name="ri">Role info</param>
        /// <returns>Returns error message if this method failed. Otherwise return empty string.</returns>
        public static string CopyGroupDocument(GroupInfo gi, string sourcePath, string targetPath, string groupProfileURLPath, string culture, bool combineWithDefaultCulture, UserInfo user, RoleInfo ri)
        {
            return ProviderObject.CopyGroupDocumentInternal(gi, sourcePath, targetPath, groupProfileURLPath, culture, combineWithDefaultCulture, user, ri);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the GroupInfo structure for the specified group.
        /// </summary>
        /// <param name="groupCodeName">Code name of the group to get</param>
        /// <param name="siteName">Group site name</param>
        protected GroupInfo GetGroupInfoInternal(string groupCodeName, string siteName)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                return GetInfoByCodeName(groupCodeName, si.SiteID);
            }

            return null;
        }


        /// <summary>
        /// Returns the dataset of GroupInfo structure managed by user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="orderBy">Order by statement used to sort the data</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty select all columns</param>
        /// <returns>Data set with records.</returns>
        protected TypedDataSet GetGroupsManagedByUserInternal(int userId, string siteName, string orderBy, int topN, string columns)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@UserID", userId);
                parameters.Add("@SiteID", si.SiteID);
                parameters.EnsureDataSet<GroupInfo>();

                // Get the data
                return ConnectionHelper.ExecuteQuery("community.group.selectmanagedbyuser", parameters, null, orderBy, topN, columns).As<GroupInfo>();
            }

            return null;
        }


        /// <summary>
        /// Returns first N records on groups according to the specified conditions from groups view.
        /// </summary>
        /// <param name="where">Where condition to apply</param>
        /// <param name="orderBy">Order by statement used to sort the data</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty select all columns</param>
        /// <returns>Data set with records.</returns>
        protected TypedDataSet GetGroupsViewInternal(string where, string orderBy, int topN, string columns)
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<GroupInfo>();

            return ConnectionHelper.ExecuteQuery("community.group.selectallview", parameters, where, orderBy, topN, columns).As<GroupInfo>();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(GroupInfo info)
        {
            if ((info != null) && (info.GroupID <= 0))
            {
                info.GroupCreatedWhen = DateTime.Now;
            }

            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes specified group including related document if needed.
        /// The related document is only the one specified in GroupNodeGUID.
        /// All documents that are owned by group are kept - their group ownership is set to null.
        /// </summary>
        /// <param name="infoObj">Group object</param>
        /// <param name="deleteRelatedDocument">If true related document is deleted too</param>
        protected void DeleteGroupInfoInternal(GroupInfo infoObj, bool deleteRelatedDocument)
        {
            if (infoObj != null)
            {
                SiteInfo site = SiteInfoProvider.GetSiteInfo(infoObj.GroupSiteID);
                if (site != null)
                {
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Update all nodes owned by group - set their owner to null
                    // They cannot be deleted because they could have non-group children
                    var nodesOwnedByGroup = tree.SelectNodes(site.SiteName, null, TreeProvider.ALL_CULTURES, false, null, "[NodeGroupID] = " + infoObj.GroupID, null, -1, false);
                    foreach (TreeNode node in nodesOwnedByGroup)
                    {
                        using (new CMSActionContext { LogEvents = false })
                        {
                            // Update the document node
                            node.SetIntegerValue("NodeGroupID", 0, false);
                            node.Update();
                        }

                        // Log synchronization
                        DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.UpdateDocument, tree);
                    }

                    // Delete document related to the group if requested
                    if (deleteRelatedDocument && (infoObj.GroupNodeGUID != Guid.Empty))
                    {
                        TreeNode node = tree.SelectSingleNode(infoObj.GroupNodeGUID, TreeProvider.ALL_CULTURES, site.SiteName);
                        if (node != null)
                        {
                            DocumentHelper.DeleteDocument(node, tree, true, true);
                        }
                    }

                    // Delete group's avatar if custom
                    AvatarInfo ai = AvatarInfoProvider.GetAvatarInfo(infoObj.GroupAvatarID);
                    if ((ai != null) && ai.AvatarIsCustom)
                    {
                        AvatarInfoProvider.DeleteAvatarInfo(ai);
                    }

                    DeleteInfo(infoObj);
                }
            }
        }


        /// <summary>
        /// Gets number of user groups.
        /// </summary>
        /// <param name="userInfo">User info</param>
        /// <param name="siteInfo">Site info</param>
        protected int GetUserGroupsCountInternal(UserInfo userInfo, SiteInfo siteInfo)
        {
            if ((userInfo == null) || (siteInfo == null))
            {
                return 0;
            }

            string where = "GroupSiteID =" + siteInfo.SiteID;

            // Get only groups in which user is the member
            if (!userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !UserInfoProvider.IsAuthorizedPerResource("CMS.Groups", "Manage", siteInfo.SiteName, userInfo))
            {
                where += " AND GroupID IN (SELECT MemberGroupID FROM Community_GroupMember WHERE MemberUserID = " + userInfo.UserID + " )";
            }

            return GetGroups().Where(where).Count;
        }


        /// <summary>
        /// Returns true if the user as a role member has permission for the specified group.
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="permissionId">Permission ID</param>
        protected bool IsAuthorizedPerGroupInternal(int groupId, int userId, int permissionId)
        {
            if ((groupId > 0) && (userId > 0) && (permissionId > 0))
            {
                // Prepare where condition
                string where = "GroupID=" + groupId + " AND PermissionID = " + permissionId +
                               " AND RoleID IN (SELECT RoleID FROM CMS_UserRole WHERE UserID = " + userId + ")";

                // Get GroupRolePermissionInfo objects which fit the where condition
                DataSet ds = GroupRolePermissionInfoProvider.GetGroupRolePermissionInfos(where, null);
                return !DataHelper.DataSourceIsEmpty(ds);
            }

            return false;
        }


        /// <summary>
        /// Creates group pages for specified group.
        /// </summary>
        /// <param name="gi">Group info object</param>
        /// <param name="sourcePath">Source template path</param>
        /// <param name="targetPath">Target template path</param>
        /// <param name="groupProfileURLPath">URL path to group profile. Macro {groupname} is automatically resolved to current group</param>
        /// <param name="culture">Document culture</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture</param>
        /// <param name="user">User info</param>
        /// <param name="ri">Role info</param>
        /// <returns>Returns error message if this method failed. Otherwise return empty string.</returns>
        protected string CopyGroupDocumentInternal(GroupInfo gi, string sourcePath, string targetPath, string groupProfileURLPath, string culture, bool combineWithDefaultCulture, UserInfo user, RoleInfo ri)
        {
            if ((gi != null) && (gi.GroupID > 0))
            {
                // For empty source path try load from settings
                if (String.IsNullOrEmpty(sourcePath))
                {
                    sourcePath = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSGroupTemplatePath");
                }

                if (String.IsNullOrEmpty(sourcePath))
                {
                    return ResHelper.GetString("group.registration.nosourcepath");
                }

                if (String.IsNullOrEmpty(targetPath))
                {
                    return ResHelper.GetString("group.registration.notargetpath");
                }

                sourcePath = MacroResolver.ResolveCurrentPath(sourcePath);
                targetPath = MacroResolver.ResolveCurrentPath(targetPath);

                // Ensure user object
                if (user == null)
                {
                    user = MembershipContext.AuthenticatedUser;
                }

                int groupId = gi.GroupID;
                string codeName = gi.GroupName;

                TreeProvider tp = new TreeProvider(user);

                TreeNode nodeSource = tp.SelectSingleNode(SiteContext.CurrentSiteName, sourcePath, culture, combineWithDefaultCulture);
                TreeNode nodeTarget = tp.SelectSingleNode(SiteContext.CurrentSiteName, targetPath, culture, combineWithDefaultCulture);

                if ((nodeSource != null) && (nodeTarget != null))
                {
                    // Check allowed child class
                    if (!DocumentHelper.IsDocumentTypeAllowed(nodeTarget, nodeSource.GetValue("NodeClassID", 0)))
                    {
                        EventLogProvider.LogEvent(EventType.ERROR, "Community group", "Register", "Target page type is not allowed child type.", RequestContext.CurrentURL);

                        return ResHelper.GetString("group.registration.templatecopyfailed");
                    }

                    // Check cyclic copying (copying of the node to some of its child nodes)
                    if (nodeTarget.NodeAliasPath.StartsWithCSafe(nodeSource.NodeAliasPath + "/", true))
                    {
                        EventLogProvider.LogEvent(EventType.ERROR, "Community group", "Register", "Target page cannot be copied into itself.", RequestContext.CurrentURL);

                        return ResHelper.GetString("group.registration.templatecopyfailed");
                    }

                    var settings = new CopyDocumentSettings(nodeSource, nodeTarget, tp)
                    {
                        IncludeChildNodes = true,
                        NewDocumentsOwner = user.UserID,
                        NewDocumentsGroup = groupId
                    };

                    TreeNode node = DocumentHelper.CopyDocument(settings);

                    node.DocumentName = codeName;
                    node.NodeAlias = codeName;

                    // Ensure user friendly menu caption
                    node.DocumentMenuCaption = gi.GroupDisplayName;

                    // Set group profile page url
                    node.DocumentUrlPath = groupProfileURLPath.ToLowerCSafe().Replace("{groupname}", codeName);

                    // Update copied menu item name
                    node.SetDocumentNameSource(codeName);

                    // Create new tag group info
                    string tagGroupName = gi.GroupName;
                    int counter = 0;
                    while (TagGroupInfoProvider.GetTagGroupInfo(tagGroupName, SiteContext.CurrentSiteID) != null)
                    {
                        // Get unique TagGroupName
                        counter++;
                        tagGroupName = gi.GroupName + "_" + counter;
                    }
                    TagGroupInfo tgi = new TagGroupInfo();
                    tgi.TagGroupDisplayName = gi.GroupDisplayName;
                    tgi.TagGroupName = tagGroupName;
                    tgi.TagGroupDescription = "";
                    tgi.TagGroupSiteID = SiteContext.CurrentSiteID;
                    tgi.TagGroupIsAdHoc = false;

                    // Store tag group info to DB
                    TagGroupInfoProvider.SetTagGroupInfo(tgi);

                    // Update document Tag group ID
                    node.DocumentTagGroupID = tgi.TagGroupID;

                    // Update original values => Document alias for source document is not created
                    using (new DocumentActionContext { GenerateDocumentAliases = false })
                    {
                        DocumentHelper.UpdateDocument(node, tp, "NodeName;NodeAlias;DocumentName;DocumentMenuCaption;DocumentUrlPath");
                    }

                    // Set full control for group admin role
                    if (ri != null)
                    {
                        AclItemInfoProvider.SetRolePermissions(node, 127, 0, ri);
                    }

                    // Update search index for node
                    if (DocumentHelper.IsSearchTaskCreationAllowed(node))
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, PredefinedObjectType.DOCUMENT, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                    }

                    // Check document culture, set current culture properties
                    if (node.DocumentCulture != nodeSource.DocumentCulture)
                    {
                        node = DocumentHelper.GetDocument(node.NodeID, nodeSource.DocumentCulture, tp);
                        if (node != null)
                        {
                            node.DocumentName = codeName;
                            node.NodeAlias = codeName;

                            // Update copied menu item name
                            node.SetDocumentNameSource(codeName);

                            DocumentHelper.UpdateDocument(node, tp, "NodeName;NodeAlias;DocumentName;DocumentMenuCaption;DocumentUrlPath");

                            // Update search index for node
                            if (DocumentHelper.IsSearchTaskCreationAllowed(node))
                            {
                                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, PredefinedObjectType.DOCUMENT, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                            }
                        }
                    }

                    // Set group node guid
                    gi.GroupNodeGUID = node.NodeGUID;
                    SetGroupInfo(gi);

                    // Update path under workflow
                    if (node.PublishedVersionExists)
                    {
                        VersionManager vm = VersionManager.GetInstance(tp);
                        WorkflowManager wm = vm.WorkflowManager;
                        wm.MoveToPublishedStep(node, "Groups auto-publish");
                    }
                }
                else
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "Community group", "Register", " Source or target page not found.", RequestContext.CurrentURL);

                    return ResHelper.GetString("group.registration.templatecopyfailed");
                }
            }
            else
            {
                EventLogProvider.LogEvent(EventType.ERROR, "Community group", "Register", "Community group is not defined.", RequestContext.CurrentURL);

                return ResHelper.GetString("group.registration.templatecopyfailed");
            }

            return String.Empty;
        }


        /// <summary>
        /// Removes the given node from all group page locations which are linked to the node.
        /// </summary>
        /// <param name="siteId">Site which the node belongs to.</param>
        /// <param name="nodeGuid">Node unique identifier.</param>
        internal static void ClearGroupPageLocation(int siteId, Guid nodeGuid)
        {
            ProviderObject.ClearGroupPageLocationInternal(siteId, nodeGuid);
        }


        private void ClearGroupPageLocationInternal(int siteId, Guid nodeGuid)
        {
            if ((siteId == 0) || (nodeGuid == Guid.Empty))
            {
                return;
            }

            UpdateData(
                new WhereCondition()
                    .WhereEquals("GroupSiteID", siteId)
                    .WhereEquals("GroupNodeGUID", nodeGuid),
                new Dictionary<string, object>
                {
                    { "GroupNodeGUID", null }
                }
            );
        }

        #endregion
    }
}