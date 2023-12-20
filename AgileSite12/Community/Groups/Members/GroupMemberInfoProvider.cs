using System;
using System.Collections;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.Community
{
    using TypedDataSet = InfoDataSet<GroupMemberInfo>;

    /// <summary>
    /// Class providing GroupMemberInfo management.
    /// </summary>
    public class GroupMemberInfoProvider : AbstractInfoProvider<GroupMemberInfo, GroupMemberInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the GroupMemberInfo structure for the specified groupMember.
        /// </summary>
        /// <param name="groupMemberId">GroupMember id</param>
        public static GroupMemberInfo GetGroupMemberInfo(int groupMemberId)
        {
            return ProviderObject.GetInfoById(groupMemberId);
        }


        /// <summary>
        /// Returns a query for all the <see cref="GroupMemberInfo"/> objects.
        /// </summary>
        public static ObjectQuery<GroupMemberInfo> GetGroupMembers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns records with information about members with complete user and site info.
        /// </summary>
        /// <param name="where">Where condition to apply</param>
        /// <param name="orderBy">Order by statement used to sort the data</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        public static TypedDataSet GetCompleteSiteMembers(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetCompleteSiteMembersInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Indicates whether the specified user is member of the specified group.
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="groupId">ID of the group to check</param>
        public static bool IsMemberOfGroup(int userId, int groupId)
        {
            return ProviderObject.IsMemberOfGroupInternal(userId, groupId);
        }


        /// <summary>
        /// Returns GroupInfo structure specified by userdId and groupId.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="groupId">ID of group</param>
        public static GroupMemberInfo GetGroupMemberInfo(int userId, int groupId)
        {
            return ProviderObject.GetGroupMemberInfoInternal(userId, groupId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified groupMember.
        /// </summary>
        /// <param name="groupMember">GroupMember to set</param>
        public static void SetGroupMemberInfo(GroupMemberInfo groupMember)
        {
            ProviderObject.SetInfo(groupMember);
        }


        /// <summary>
        /// Deletes specified groupMember.
        /// </summary>
        /// <param name="infoObj">GroupMember object</param>
        public static void DeleteGroupMemberInfo(GroupMemberInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified groupMember.
        /// </summary>
        /// <param name="groupMemberId">GroupMember id</param>
        public static void DeleteGroupMemberInfo(int groupMemberId)
        {
            GroupMemberInfo infoObj = GetGroupMemberInfo(groupMemberId);
            DeleteGroupMemberInfo(infoObj);
        }


        /// <summary>
        /// Sends group notification.
        /// </summary>
        /// <param name="templateName">Email template name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="memberInfo">Group member object</param>
        /// <param name="sendToAdmin">True if send to admin, false if send to user</param>
        public static void SendNotificationMail(string templateName, string siteName, GroupMemberInfo memberInfo, bool sendToAdmin)
        {
            EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate(templateName, siteName);

            if ((emailTemplate != null) && (memberInfo != null))
            {
                string subject;
                // Get subject from template or from resources
                if (!String.IsNullOrEmpty(emailTemplate.TemplateSubject))
                {
                    subject = emailTemplate.TemplateSubject;
                }
                else
                {
                    subject = ResHelper.GetString(templateName + ".subject");
                }
                // Get body
                string body = URLHelper.MakeLinksAbsolute(emailTemplate.TemplateText);
                string plainTextBody = URLHelper.MakeLinksAbsolute(emailTemplate.TemplatePlainText);

                // Resolve macros
                MacroResolver resolver = CreateMacroResolver(memberInfo);

                // Message to send
                EmailMessage message = new EmailMessage();

                message.EmailFormat = EmailFormatEnum.Default;
                message.Subject = resolver.ResolveMacros(subject);
                message.CcRecipients = emailTemplate.TemplateCc;
                message.BccRecipients = emailTemplate.TemplateBcc;
                message.ReplyTo = emailTemplate.TemplateReplyTo;

                // Enable macro encoding for body
                resolver.Settings.EncodeResolvedValues = true;
                message.Body = resolver.ResolveMacros(body);
                resolver.Settings.EncodeResolvedValues = false;
                message.PlainTextBody = resolver.ResolveMacros(plainTextBody);

                // Get sender email from template or from settings
                string fromAddress;
                if (!String.IsNullOrEmpty(emailTemplate.TemplateFrom))
                {
                    fromAddress = emailTemplate.TemplateFrom;
                }
                else
                {
                    fromAddress = SettingsKeyInfoProvider.GetValue(siteName + ".CMSNoreplyEmailAddress");
                }
                if (!String.IsNullOrEmpty(fromAddress))
                {
                    message.From = fromAddress;

                    // If send to admins
                    if (sendToAdmin)
                    {
                        // Get admins mails
                        string admins = GetAdminsMails(memberInfo.MemberGroupID);
                        if (admins != null)
                        {
                            message.Recipients = admins;
                        }
                    }
                    else
                    {
                        // Get user info
                        UserInfo user = UserInfoProvider.GetUserInfo(memberInfo.MemberUserID);
                        if (user != null)
                        {
                            message.Recipients = user.Email;
                        }
                    }

                    // If from and to emails are set try to send message
                    if (!(String.IsNullOrEmpty(message.From) || String.IsNullOrEmpty(message.Recipients)))
                    {
                        try
                        {
                            // Attach template metafiles to e-mail
                            EmailHelper.ResolveMetaFileImages(message, emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                            EmailSender.SendEmail(siteName, message);
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Send notification", "GROUP", ex);
                        }
                    }
                }
                else
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "GroupMember", "EmailSenderNotSpecified");
                }
            }
        }


        /// <summary>
        /// Returns administrators emails.
        /// </summary>
        /// <param name="groupID">Group ID</param>
        private static string GetAdminsMails(int groupID)
        {
            if (groupID > 0)
            {
                // Get admin roles
                var roles = RoleInfoProvider.GetRoles().Where("(RoleIsGroupAdministrator = 1) AND (RoleGroupID = " + groupID + ")");
                ArrayList list = new ArrayList();
                // Foreach role
                foreach (var role in roles)
                {
                    // Get role users
                    var tableUsers = RoleInfoProvider.GetRoleUsers(role.RoleID);
                    if (!DataHelper.DataSourceIsEmpty(tableUsers))
                    {
                        // Foreach user
                        foreach (DataRow rowUser in tableUsers.Rows)
                        {
                            // Add email into list if not in
                            if (!String.IsNullOrEmpty(rowUser["Email"].ToString()))
                            {
                                if (!list.Contains(rowUser["Email"]))
                                {
                                    list.Add(rowUser["Email"]);
                                }
                            }
                        }
                    }
                }
                // Create admins string
                string[] mails = new string[list.Count];
                list.CopyTo(mails);
                return string.Join(";", mails);
            }
            return null;
        }


        /// <summary>
        /// Creates macro resolver.
        /// </summary>
        /// <param name="memberInfo">Group member object</param>
        private static MacroResolver CreateMacroResolver(GroupMemberInfo memberInfo)
        {
            // Create resolver
            MacroResolver resolver = MacroResolver.GetInstance();
            if (memberInfo != null)
            {
                // Get data
                GroupInfo gi = GroupInfoProvider.GetGroupInfo(memberInfo.MemberGroupID);
                UserInfo ui = UserInfoProvider.GetUserInfo(memberInfo.MemberUserID);

                // Fill the data
                resolver.SetAnonymousSourceData(ui, gi);
                resolver.SetNamedSourceData("Group", gi);
                resolver.SetNamedSourceData("MemberUser", ui);
            }
            return resolver;
        }


        /// <summary>
        /// Returns member profile path with resolved macros from settings.
        /// </summary>
        /// <param name="memberName">Member name</param>
        /// <param name="siteName">Site name</param>
        public static string GetMemberProfilePath(string memberName, string siteName)
        {
            string path = SettingsKeyInfoProvider.GetValue(siteName + ".CMSMemberProfilePath");

            // Resolve well-known wildcards for UserName and CultureCode
            path = path.ToLowerInvariant().Replace("{username}", UserInfoProvider.TrimSitePrefix(memberName));
            var currentCulture = LocalizationContext.CurrentCulture;
            path = path.Replace("{culturecode}", (currentCulture != null) ? currentCulture.CultureCode : String.Empty);

            return MacroResolver.Resolve(path);
        }


        /// <summary>
        /// Returns member management path with resolved macros from settings.
        /// </summary>
        /// <param name="memberName">Member name</param>
        /// <param name="siteName">Site name</param>
        public static string GetMemberManagementPath(string memberName, string siteName)
        {
            string path = SettingsKeyInfoProvider.GetValue(siteName + ".CMSMemberManagementPath");

            // Resolve well-known wildcards for UserName and CultureCode
            path = path.ToLowerInvariant().Replace("{username}", UserInfoProvider.TrimSitePrefix(memberName));
            var currentCulture = LocalizationContext.CurrentCulture;
            path = path.Replace("{culturecode}", (currentCulture != null) ? currentCulture.CultureCode : String.Empty);

            return MacroResolver.Resolve(path);
        }


        internal static void RemoveUsersFromSiteGroups(int userId, int siteId)
        {
            new ObjectQuery(PredefinedObjectType.GROUPMEMBER)
                .WhereID("MemberUserID", userId)
                .WhereIn("MemberGroupID", new ObjectQuery(PredefinedObjectType.GROUP)
                    .Columns("GroupID")
                    .WhereID("GroupSiteID", siteId))
                .ForEachObject(groupMemberInfo => groupMemberInfo.Delete());
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns records with information about members with complete user and site info.
        /// </summary>
        /// <param name="where">Where condition to apply</param>
        /// <param name="orderBy">Order by statement used to sort the data</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual TypedDataSet GetCompleteSiteMembersInternal(string where, string orderBy, int topN, string columns)
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<GroupMemberInfo>();

            return ConnectionHelper.ExecuteQuery("community.groupmember.selectallview", parameters, where, orderBy, topN, columns).As<GroupMemberInfo>();
        }


        /// <summary>
        /// Indicates whether the specified user is member of the specified group.
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="groupId">ID of the group to check</param>
        protected virtual bool IsMemberOfGroupInternal(int userId, int groupId)
        {
            if ((userId > 0) && (groupId > 0))
            {
                return GetGroupMemberInfoInternal(userId, groupId) != null;
            }

            return false;
        }


        /// <summary>
        /// Returns GroupInfo structure specified by userdId and groupId.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="groupId">ID of group</param>
        protected virtual GroupMemberInfo GetGroupMemberInfoInternal(int userId, int groupId)
        {
            if ((userId > 0) && (groupId > 0))
            {
                var where = new WhereCondition().WhereEquals("MemberUserID", userId).WhereEquals("MemberGroupID", groupId);

                return GetGroupMembers().Where(where).TopN(1).FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(GroupMemberInfo info)
        {
            if (info != null)
            {
                // Set default member status if not set
                info.MemberStatus = info.MemberStatus;

                base.SetInfo(info);

                // Invalidate user info
                UserInfoProvider.InvalidateUser(info.MemberUserID);
            }
            else
            {
                throw new ArgumentNullException(nameof(info));
            }
        }

        #endregion
    }
}