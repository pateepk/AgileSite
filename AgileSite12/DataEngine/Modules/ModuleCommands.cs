using System;
using System.Collections.Generic;
using System.Data;

using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Module commands.
    /// </summary>
    public static class ModuleCommands
    {
        private static object ProcessModuleCommand(string moduleName, string commandName, object[] parameters)
        {
            return ModuleManager.GetModule(moduleName)?.ProcessCommand(commandName, parameters);
        }


        // ALL COMMANDS REGISTERED IN THIS CLASS MUST BE APPROVED

        #region "Community"

        /// <summary>
        /// Gets the group by GUID.
        /// </summary>
        /// <param name="groupGuid">Group GUID</param>
        public static BaseInfo CommunityGetGroupInfoByGuid(Guid groupGuid)
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.COMMUNITY, "GetGroupInfoByGuid", new object[] { groupGuid });
        }


        /// <summary>
        /// Gets the Group info from Community module.
        /// </summary>
        /// <param name="groupId">Group ID</param>
        public static BaseInfo CommunityGetGroupInfo(int groupId)
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.COMMUNITY, "GetGroupInfo", new object[] { groupId });
        }


        /// <summary>
        /// Gets the Group info by group name and site name from Community module.
        /// </summary>
        /// <param name="groupName">Group code name</param>
        /// <param name="siteName">Site name</param>
        public static BaseInfo CommunityGetGroupInfoByName(string groupName, string siteName)
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.COMMUNITY, "getgroupinfobyname", new object[] { groupName, siteName });
        }


        /// <summary>
        /// Gets the groups for the given user.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static DataSet CommunityGetUserGroups(int userId)
        {
            return (DataSet)ProcessModuleCommand(ModuleName.COMMUNITY, "GetUserGroups", new object[] { userId });
        }


        /// <summary>
        /// Gets the groups for the given user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="columns">Columns to select</param>
        public static DataSet CommunityGetUserGroups(int userId, string columns)
        {
            return (DataSet)ProcessModuleCommand(ModuleName.COMMUNITY, "GetUserGroups", new object[] { userId, columns });
        }


        /// <summary>
        /// Gets the current group's ID. Returns 0 if current group does not exist.
        /// </summary>
        public static int CommunityGetCurrentGroupID()
        {
            // Get current group object from community context
            BaseInfo currentGroup = (BaseInfo)ModuleManager.GetContextProperty("CommunityContext", "CurrentGroup");
            if (currentGroup != null)
            {
                return currentGroup.Generalized.ObjectID;
            }

            return 0;
        }


        /// <summary>
        /// Gets group profile path.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="siteName">Site name</param>
        public static string CommunityGetGroupProfilePath(string groupName, string siteName)
        {
            return (string)ProcessModuleCommand(ModuleName.COMMUNITY, "GetGroupProfilePath", new object[] { groupName, siteName });
        }


        /// <summary>
        /// Gets group management path.
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="siteName">Site name</param>
        public static string CommunityGetGroupManagementPath(string groupName, string siteName)
        {
            return (string)ProcessModuleCommand(ModuleName.COMMUNITY, "GetGroupManagementPath", new object[] { groupName, siteName });
        }


        /// <summary>
        /// Gets member profile path.
        /// </summary>
        /// <param name="memberName">Member name</param>
        /// <param name="siteName">Site name</param>
        public static string CommunityGetMemberProfilePath(string memberName, string siteName)
        {
            return (string)ProcessModuleCommand(ModuleName.COMMUNITY, "GetMemberProfilePath", new object[] { memberName, siteName });
        }


        /// <summary>
        /// Gets member management path.
        /// </summary>
        /// <param name="memberName">Member name</param>
        /// <param name="siteName">Site name</param>
        public static string CommunityGetMemberManagementPath(string memberName, string siteName)
        {
            return (string)ProcessModuleCommand(ModuleName.COMMUNITY, "GetMemberManagementPath", new object[] { memberName, siteName });
        }


        /// <summary>
        /// Returns true if selected site contains at least one group.
        /// </summary>
        /// <param name="siteId">Site id</param>
        public static bool CommunitySiteHasGroup(int siteId)
        {
            object hasGroupObj = ProcessModuleCommand(ModuleName.COMMUNITY, "SiteHasGroup", new object[] { siteId });
            if (hasGroupObj != null)
            {
                return (bool)hasGroupObj;
            }
            return false;
        }


        /// <summary>
        /// Returns true if CurrentUser is authorized for the specified action in the group.
        /// </summary>
        /// <param name="permissionName">Permission to check</param>
        /// <param name="groupId">Group ID</param>
        public static bool CommunityCheckGroupPermission(string permissionName, int groupId)
        {
            object checkGroupPermissionObj = ProcessModuleCommand(ModuleName.COMMUNITY, "CheckGroupPermission", new object[] { permissionName, groupId });
            if (checkGroupPermissionObj != null)
            {
                return (bool)checkGroupPermissionObj;
            }
            return false;
        }


        /// <summary>
        /// Indicates whether the specified user is member of the specified group.
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="groupId">ID of the group to check</param>
        public static bool CommunityIsMemberOfGroup(int userId, int groupId)
        {
            return (bool)ProcessModuleCommand(ModuleName.COMMUNITY, "IsMemberOfGroup", new object[] { userId, groupId });
        }

        #endregion


        #region "Booking events"

        /// <summary>
        /// Returns Booking event by ID for specified site.
        /// </summary>
        /// <param name="eventId">Booking event ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="columns">Columns of booking event to be returned in DataSet</param>
        public static DataSet EventsGetSiteEvent(int eventId, string siteName, string columns)
        {
            return (DataSet)ProcessModuleCommand(ModuleName.EVENTMANAGER, "GetSiteEvent", new object[] { eventId, siteName, columns });
        }

        #endregion


        #region "Forums"

        /// <summary>
        /// Gets the number of forums for current document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static int ForumsGetDocumentForumsCount(int documentId)
        {
            return ValidationHelper.GetInteger(ProcessModuleCommand(ModuleName.FORUMS, "GetDocumentForumsCount", new object[] { documentId }), 0);
        }


        /// <summary>
        /// Add moderator to forum.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        public static void ForumsAddForumModerator(int userId, int forumId)
        {
            ProcessModuleCommand(ModuleName.FORUMS, "AddForumModerator", new object[] { userId, forumId });
        }


        /// <summary>
        /// Removes moderator to forum.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="forumId">Forum ID</param>
        public static void ForumsRemoveForumModerator(int userId, int forumId)
        {
            ProcessModuleCommand(ModuleName.FORUMS, "RemoveForumModerator", new object[] { userId, forumId });
        }


        /// <summary>
        /// Returns forum post info (BaseInfo) of specified ID.
        /// </summary>
        /// <param name="forumPostId">ID of the forum</param>        
        public static BaseInfo ForumsGetForumPostInfo(int forumPostId)
        {
            return ProcessModuleCommand(ModuleName.FORUMS, "getforumpostinfo", new object[1] { forumPostId }) as BaseInfo;
        }


        /// <summary>
        /// Returns ForumInfo(BaseInfo) of specified id.
        /// </summary>
        /// <param name="forumId">ID of the forum</param>        
        public static BaseInfo ForumsGetForumInfo(int forumId)
        {
            return ProcessModuleCommand(ModuleName.FORUMS, "getforuminfo", new object[1] { forumId }) as BaseInfo;
        }


        /// <summary>
        /// Returns URL of the specified forum post.
        /// <param name="forumId">Forum id</param>
        /// <param name="postIdPath">Post id path</param>
        /// <param name="encodeQueryString">Indicates if the query string should be encoded</param>
        /// </summary>
        public static string ForumsGetPostUrl(string postIdPath, int forumId, bool encodeQueryString)
        {
            return ProcessModuleCommand(ModuleName.FORUMS, "getforumposturl", new object[] { postIdPath, forumId, encodeQueryString }) as string;
        }

        #endregion


        #region "Reporting"

        /// <summary>
        /// Item's default connection string 
        /// </summary>
        public static String GetDefaultReportConnectionString()
        {
            return (String)ProcessModuleCommand(ModuleName.REPORTING, "GetDefaultReportConnectionString", null);
        }


        /// <summary>
        /// Refresh children count for single category.
        /// </summary>
        /// <param name="infoObj">Category to update</param>
        public static void ReportingRefreshCategoryDataCount(BaseInfo infoObj)
        {
            ProcessModuleCommand(ModuleName.REPORTING, "refreshcategorydatacount", new object[] { infoObj });
        }

        #endregion


        #region "E-mails"

        /// <summary>
        /// Sends the e-mail.
        /// </summary>
        /// <param name="emailAddress">E-mail address(es) of recipient(s). Use semicolon as a separator.</param>
        /// <param name="fromEmailAddress">E-mail address from which is the email sent</param>
        /// <param name="emailSubject">E-mail subject</param>
        /// <param name="emailBody">E-mail body</param>
        /// <param name="emailPlainTextBody">E-mail plain text body (optional)</param>
        /// <param name="siteName">Name of the site (optional)</param>
        public static void SendEmail(string emailAddress, string fromEmailAddress, string emailSubject, string emailBody, string emailPlainTextBody, string siteName)
        {
            // Call send command from e-mail module
            object[] parameters = new object[] { emailAddress, emailSubject, emailBody, siteName, fromEmailAddress, emailPlainTextBody };
            ProcessModuleCommand(ModuleName.EMAILENGINE, "SendEmail", parameters);
        }

        #endregion


        #region "Media library"

        /// <summary>
        /// Gets media library info object.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        public static BaseInfo MediaLibraryGetMediaLibraryInfo(int libraryId)
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.MEDIALIBRARY, "GetMediaLibraryInfo", new object[] { libraryId });
        }


        /// <summary>
        /// Returns media url according to site settings.
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="siteName">Site name</param>
        public static string MediaLibraryGetMediaFileUrl(string fileGuid, string siteName)
        {
            return (string)ProcessModuleCommand(ModuleName.MEDIALIBRARY, "GetMediaFileUrl", new object[] { fileGuid, siteName });
        }


        /// <summary>
        /// Returns media url according to GUID and file name.
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        public static string MediaLibraryGetMediaFileUrl(Guid fileGuid, string fileName)
        {
            return (string)ProcessModuleCommand(ModuleName.MEDIALIBRARY, "GetMediaFileUrlByName", new object[] { fileGuid, fileName });
        }


        /// <summary>
        /// Deletes media file from file system.
        /// </summary>
        /// <param name="siteID">Site id</param>
        /// <param name="libraryID">Library id</param>
        /// <param name="filePath">Sub path to file</param>
        /// <param name="onlyFile">Indicates if only file should be deleted</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        public static void MediaLibraryDeleteMediaFile(int siteID, int libraryID, string filePath, bool onlyFile, bool synchronization)
        {
            ProcessModuleCommand(ModuleName.MEDIALIBRARY, "DeleteMediaFile", new object[] { siteID, libraryID, filePath, onlyFile, synchronization });
        }


        /// <summary>
        /// Deletes media file preview from file system.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="filePath">File path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        public static void MediaLibraryDeleteMediaFilePreview(string siteName, int libraryID, string filePath, bool synchronization)
        {
            ProcessModuleCommand(ModuleName.MEDIALIBRARY, "DeleteMediaFilePreview", new object[] { siteName, libraryID, filePath, synchronization });
        }

        #endregion


        #region "Notifications"

        /// <summary>
        /// Raises notification events according to the specified parameters.
        /// </summary>
        /// <param name="eventSource">Subscription event source</param>
        /// <param name="eventCode">Subscription event code</param>
        /// <param name="eventObjectId">Subscription event object ID</param>
        /// <param name="eventData1">Subscription event data 1</param>
        /// <param name="eventData2">Subscription event data 2</param>
        /// <param name="siteId">ID of the site where the event belongs</param>
        /// <param name="where">Additional WHERE condition</param>
        /// <param name="resolverData">Custom data for macro resolver (DataRow or DataClass object)</param>
        /// <param name="resolverSpecialMacros">Special macros</param>
        public static void NotificationsRaiseEvent(string eventSource, string eventCode, int eventObjectId, string eventData1, string eventData2, int siteId, string where, object resolverData, IDictionary<string, object> resolverSpecialMacros)
        {
            object[] parameters = { eventSource, eventCode, eventObjectId, eventData1, eventData2, siteId, where, resolverData, resolverSpecialMacros };
            ProcessModuleCommand(ModuleName.NOTIFICATIONS, "raiseevent", parameters);
        }

        #endregion


        #region "Message board"

        /// <summary>
        /// Gets the number of message boards for current document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static int MessageBoardGetDocumentBoardsCount(int documentId)
        {
            return ValidationHelper.GetInteger(ProcessModuleCommand(ModuleName.MESSAGEBOARD, "GetDocumentBoardsCount", new object[] { documentId }), 0);
        }


        /// <summary>
        /// Adds specified role to the board.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="boardId">BoardID</param>
        public static void MessageBoardAddRoleToBoard(int roleId, int boardId)
        {
            ProcessModuleCommand(ModuleName.MESSAGEBOARD, "AddRoleToBoard", new object[] { roleId, boardId });
        }


        /// <summary>
        /// Removes specified role from the board.
        /// </summary>
        /// <param name="roleId">RoleID</param>
        /// <param name="boardId">BoardID</param>
        public static void MessageBoardRemoveRoleFromBoard(int roleId, int boardId)
        {
            ProcessModuleCommand(ModuleName.MESSAGEBOARD, "RemoveRoleFromBoard", new object[] { roleId, boardId });
        }


        /// <summary>
        /// Adds new BoardModeratorInfo object.
        /// </summary>        
        /// <param name="boardId">ID of the board moderator is being added to</param>
        /// <param name="userId">ID of the user representing board moderator</param>
        public static void MessageBoardAddModeratorToBoard(int userId, int boardId)
        {
            ProcessModuleCommand(ModuleName.MESSAGEBOARD, "AddModeratorToBoard", new object[] { userId, boardId });
        }


        /// <summary>
        /// Removes specified user from the board.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="boardId">BoardID</param>
        public static void MessageBoardRemoveModeratorFromBoard(int roleId, int boardId)
        {
            ProcessModuleCommand(ModuleName.MESSAGEBOARD, "RemoveModeratorFromBoard", new object[] { roleId, boardId });
        }


        /// <summary>
        /// Gets message board info object with dependence on specified board id.
        /// </summary>
        /// <param name="messageBoardId">Message board id</param>
        public static BaseInfo MessageBoardGetMessageBoardInfo(int messageBoardId)
        {
            return ProcessModuleCommand(ModuleName.MESSAGEBOARD, "getmessageboardinfo", new object[] { messageBoardId }) as BaseInfo;
        }


        /// <summary>
        /// Gets board message info object specified by its ID.
        /// </summary>
        /// <param name="boardMessageId">Message board id</param>
        public static BaseInfo MessageBoardGetBoardMessageInfo(int boardMessageId)
        {
            return ProcessModuleCommand(ModuleName.MESSAGEBOARD, "getboardmessageinfo", new object[] { boardMessageId }) as BaseInfo;
        }

        #endregion


        #region "Polls"

        /// <summary>
        /// Assigns the role to poll.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="pollId">Poll ID</param>
        public static void PollsAddRoleToPoll(int roleId, int pollId)
        {
            ProcessModuleCommand(ModuleName.POLLS, "AddRoleToPoll", new object[] { roleId, pollId });
        }


        /// <summary>
        /// Removes role from poll.
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="pollId">Poll ID</param>
        public static void PollsRemoveRoleFromPoll(int roleId, int pollId)
        {
            ProcessModuleCommand(ModuleName.POLLS, "RemoveRoleFromPoll", new object[] { roleId, pollId });
        }


        /// <summary>
        /// Returns TRUE if poll belongs to specified group.
        /// </summary>
        /// <param name="pollId">Poll ID</param>
        /// <param name="groupId">Group ID</param>
        public static bool PollsPollBelongsToGroup(int pollId, int groupId)
        {
            return ValidationHelper.GetBoolean(ProcessModuleCommand(ModuleName.POLLS, "BelongsToGroup", new object[] { pollId, groupId }), false);
        }


        /// <summary>
        /// Returns poll info object (BaseInfo) specified by ID.
        /// </summary>
        /// <param name="pollId">Poll info ID</param>
        public static BaseInfo PollsGetPollInfo(int pollId)
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.POLLS, "GetPollInfo", new object[] { pollId });
        }


        /// <summary>
        /// Returns answer info specified by ID.
        /// </summary>
        /// <param name="answerId">Answer info ID</param>
        public static BaseInfo PollsGetPollAnswerInfo(int answerId)
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.POLLS, "GetPollAnswerInfo", new object[] { answerId });
        }

        #endregion


        #region "On-line marketing"

        /// <summary>
        /// Returns SiteID of given multivariate test id.
        /// </summary>
        /// <param name="testId">The multivariate test id</param>
        public static object OnlineMarketingGetMVTestSiteID(int testId)
        {
            return ProcessModuleCommand(ModuleName.ONLINEMARKETING, "getmvtestsiteid", new object[1] { testId });
        }


        /// <summary>
        /// Creates the default combination for a given page template (The default combination contains original versions of the page template webparts).
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        public static void OnlineMarketingEnsureDefaultCombination(int pageTemplateId)
        {
            ProcessModuleCommand(ModuleName.ONLINEMARKETING, "ensuredefaultcombination", new object[1] { pageTemplateId });
        }


        /// <summary>
        /// Returns current contact ID.
        /// </summary>
        public static int OnlineMarketingGetCurrentContactID()
        {
            return ValidationHelper.GetInteger(ProcessModuleCommand(ModuleName.ONLINEMARKETING, "getcurrentcontactid", null), 0);
        }


        /// <summary>
        /// Returns current contact if such exists.
        /// </summary>
        public static BaseInfo OnlineMarketingGetExistingContact()
        {
            return (BaseInfo)ProcessModuleCommand(ModuleName.ONLINEMARKETING, "getexistingcontact", null);
        }


        /// <summary>
        /// Creates new contact and assigns given member.
        /// </summary>
        /// <param name="firstName">Contact first name</param>
        /// <param name="lastName">Contact last name</param>
        /// <param name="email">E-mail address</param>
        /// <param name="relatedId">Related ID</param>
        /// <param name="type">Type of member (user, subscriber, customer)</param>
        public static int OnlineMarketingCreateNewContact(string firstName, string lastName, string email, int relatedId, int type)
        {
            return ValidationHelper.GetInteger(ProcessModuleCommand(ModuleName.ONLINEMARKETING, "createnewcontact", new object[] { firstName, lastName, email, relatedId, type }), 0);
        }


        /// <summary>
        /// Returns contact ID for specified user info.
        /// </summary>
        /// <param name="userInfo">User info</param>
        public static int OnlineMarketingGetUserLoginContactID(object userInfo)
        {
            return ValidationHelper.GetInteger(ProcessModuleCommand(ModuleName.ONLINEMARKETING, "getuserlogincontactid", new object[] { userInfo }), 0);
        }


        /// <summary>
        /// Removes customer references from all contact management objects.
        /// </summary>
        /// <param name="customerID">Customer being deleted</param>
        public static void OnlineMarketingRemoveCustomer(int customerID)
        {
            ProcessModuleCommand(ModuleName.ONLINEMARKETING, "removecustomer", new object[] { customerID });
        }


        /// <summary>
        /// Returns current contact ID.
        /// </summary>
        /// <param name="relatedId">RelatedID (subscriber ID, customer ID)</param>
        /// <param name="memberType">Member type converted to int</param>
        /// <param name="contactId">Contact ID</param>
        public static void OnlineMarketingCreateRelation(int relatedId, int memberType, int contactId)
        {
            ProcessModuleCommand(ModuleName.ONLINEMARKETING, "createrelation", new object[] { relatedId, memberType, contactId });
        }


        /// <summary>
        /// Returns TRUE if contact is monitored.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        public static bool OnlineMarketingContactIsMonitored(int contactId)
        {
            if (contactId <= 0)
            {
                return false;
            }
            return ValidationHelper.GetBoolean(ProcessModuleCommand(ModuleName.ONLINEMARKETING, "contactismonitored", new object[] { contactId }), false);
        }


        /// <summary>
        /// Moves all MVTests from the document under the oldAlias path to the document under the newAlias path.
        /// </summary>
        /// <param name="newAlias">Document's new alias path</param>
        /// <param name="oldAlias">Document's old alias path</param>
        /// <param name="siteID">Document's site ID</param>
        public static void OnlineMarketingMoveMVTests(String newAlias, String oldAlias, int siteID)
        {
            ProcessModuleCommand(ModuleName.ONLINEMARKETING, "movemvtests", new object[] { newAlias, oldAlias, siteID });
        }

        #endregion


        #region "Synchronization"

        /// <summary>
        /// Processes the given task.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="taskType">Type of the task</param>
        /// <param name="taskObjectType">Task object type</param>
        /// <param name="taskData">Document data for the synchronization task</param>
        /// <param name="taskBinaryData">Binary data for the synchronization task</param>
        /// <param name="processChilds">Process child objects</param>
        /// <param name="siteName">Site name</param>
        /// <param name="userName">User name</param>
        /// <param name="taskGroups">Staging task groups to which task is associated</param>
        public static object SynchronizationProcessTask(OperationTypeEnum operationType, TaskTypeEnum taskType, string taskObjectType, string taskData, string taskBinaryData, bool processChilds, string siteName, string userName = null, IEnumerable<string> taskGroups = null)
        {
            return ProcessModuleCommand(ModuleName.SYNCHRONIZATIONENGINE, "ProcessTask", new object[] { operationType, taskType, taskObjectType, taskData, taskBinaryData, processChilds, siteName, userName, taskGroups });
        }

        #endregion


        #region "Web analytics"

        /// <summary>
        /// Logs onsite search keywords.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture</param>
        /// <param name="keywords">Keywords to log</param>
        public static void LogOnSiteKeywords(string siteName, string culture, string keywords)
        {
            ProcessModuleCommand(ModuleName.WEBANALYTICS, "LogOnSiteKeywords", new object[] { siteName, culture, keywords });
        }

        #endregion
    }
}