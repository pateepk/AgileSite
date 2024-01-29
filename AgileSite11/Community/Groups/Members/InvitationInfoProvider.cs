using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.MacroEngine;

namespace CMS.Community
{
    /// <summary>
    /// Class providing InvitationInfo management.
    /// </summary>
    public class InvitationInfoProvider : AbstractInfoProvider<InvitationInfo, InvitationInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the InvitationInfo structure for the specified Invitation.
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        public static InvitationInfo GetInvitationInfo(int invitationId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", invitationId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Community.invitation.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new InvitationInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns the InvitationInfo structure specified by GUID.
        /// </summary>
        /// <param name="guid">GUID of InvitationInfo</param>
        public static InvitationInfo GetInvitationInfo(Guid guid)
        {
            string where = "InvitationGUID = '" + guid + "'";
            DataSet ds = ConnectionHelper.ExecuteQuery("community.invitation.selectall", null, where);
            return !DataHelper.DataSourceIsEmpty(ds) ? new InvitationInfo(ds.Tables[0].Rows[0]) : null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified invitation.
        /// </summary>
        /// <param name="invitation">Invitation to set</param>
        public static void SetInvitationInfo(InvitationInfo invitation)
        {
            if (invitation != null)
            {
                if (invitation.InvitationID > 0)
                {
                    invitation.Generalized.UpdateData();
                }
                else
                {
                    invitation.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[InvitationInfoProvider.SetInvitationInfo]: No InvitationInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified invitation.
        /// </summary>
        /// <param name="infoObj">Invitation object</param>
        public static void DeleteInvitationInfo(InvitationInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified invitation.
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        public static void DeleteInvitationInfo(int invitationId)
        {
            InvitationInfo infoObj = GetInvitationInfo(invitationId);
            DeleteInvitationInfo(infoObj);
        }


        /// <summary>
        /// Deletes invitations based on where condition.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        public static void DeleteInvitations(string whereCondition)
        {
            ConnectionHelper.ExecuteQuery("community.invitation.deletewhere", null, whereCondition);
        }


        /// <summary>
        /// Gets invitations specified by parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Get top N rows</param>
        /// <returns>DataSet with invitations</returns>
        public static DataSet GetInvitations(string where, string orderBy, int topN)
        {
            return ConnectionHelper.ExecuteQuery("community.invitation.selectall", null, where, orderBy, topN);
        }


        /// <summary>
        /// Gets my invitations with group and user data.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <returns>DataSet with invitations</returns>
        public static DataSet GetMyInvitations(string where, string orderBy)
        {
            return ConnectionHelper.ExecuteQuery("community.invitation.selectformyinvitations", null, where, orderBy);
        }


        /// <summary>
        /// Gets sent invitations with group and user data.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <returns>DataSet with sent invitations</returns>
        public static DataSet GetMySentInvitations(string where, string orderBy)
        {
            return ConnectionHelper.ExecuteQuery("community.invitation.selectformysentinvitations", null, where, orderBy);
        }


        /// <summary>
        /// Determines whether invitation exists.
        /// </summary>
        /// <param name="invitedUserId">ID of invited user</param>
        /// <param name="groupId">ID of group</param>
        /// <returns>True if invitation exists.</returns>
        public static bool InvitationExists(int invitedUserId, int groupId)
        {
            // Prepare parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Date", DateTime.Now);

            // Delete invalid invitations
            ConnectionHelper.ExecuteQuery("community.invitation.deletewhere", parameters, "NOT InvitationValidTo IS NULL AND InvitationValidTo < @Date");

            // Check invitations
            DataSet ds = ConnectionHelper.ExecuteQuery("community.invitation.selectall", null, "InvitedUserID=" + invitedUserId + " AND InvitationGroupID=" + groupId, null, 1, "InvitationID");
            return !DataHelper.DataSourceIsEmpty(ds);
        }


        /// <summary>
        /// Send information email to the invited user.
        /// One of recipient or recipientEmail must be specified.
        /// </summary>
        /// <param name="invitation">Infitation info</param>
        /// <param name="siteName">Name of site</param>
        public static void SendInvitationEmail(InvitationInfo invitation, string siteName)
        {
            if (invitation != null)
            {
                // Get group object
                GroupInfo group = GroupInfoProvider.GetGroupInfo(invitation.InvitationGroupID);
                if (group != null)
                {
                    // Get recipient e-mail address
                    string recipientEmail = invitation.InvitationUserEmail;
                    if (string.IsNullOrEmpty(recipientEmail))
                    {
                        UserInfo user = UserInfoProvider.GetUserInfo(invitation.InvitedUserID);
                        if (user != null)
                        {
                            recipientEmail = user.Email;
                        }
                    }

                    // Send e-mail
                    if (!string.IsNullOrEmpty(recipientEmail))
                    {
                        MacroResolver resolver = MacroContext.CurrentResolver;
                        resolver.SetAnonymousSourceData(invitation, group);
                        resolver.SetNamedSourceData("Invitation", invitation);
                        resolver.SetNamedSourceData("Group", group);

                        // Get URL for accepting invitation
                        string acceptationUrl = GetAcceptationUrl(invitation.InvitationGUID, siteName);

                        UserInfo ui = UserInfoProvider.GetUserInfo(invitation.InvitedByUserID);

                        resolver.SetNamedSourceData(new Dictionary<string, object>
                        {
                            { "ACCEPTIONURL", acceptationUrl },
                            { "InvitedBy", (ui != null) ? ui.UserName : String.Empty }
                        }, isPrioritized: false);

                        // Get e-mail template
                        EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate("Groups.Invitation", siteName);
                        if (template != null)
                        {
                            string from = DataHelper.GetNotEmpty(template.TemplateFrom, SettingsKeyInfoProvider.GetValue(siteName + ".CMSNoreplyEmailAddress"));
                            if (!String.IsNullOrEmpty(from))
                            {
                                // Send e-mail
                                EmailMessage message = new EmailMessage();
                                message.Recipients = recipientEmail;
                                message.From = from;

                                EmailSender.SendEmailWithTemplateText(siteName, message, template, resolver, false);
                            }
                            else
                            {
                                EventLogProvider.LogEvent(EventType.ERROR, "GroupInvitation", "EmailSenderNotSpecified");
                            }
                        }
                    }
                }
            }
        }


        ///<summary>
        /// Gets absolute URL for accepting invitation
        ///</summary>
        ///<param name="invitationGUID">GUID of invitation to accept</param>
        ///<param name="siteName">Name of site</param>
        public static string GetAcceptationUrl(Guid invitationGUID, string siteName)
        {
            string acceptationPath = SettingsKeyInfoProvider.GetValue(siteName + ".CMSInvitationAcceptationPath");

            if (!string.IsNullOrEmpty(acceptationPath))
            {
                acceptationPath = URLHelper.GetAbsoluteUrl(DocumentURLProvider.GetUrl(acceptationPath.Trim())) + "?invitationguid=" + invitationGUID;
            }
            return acceptationPath;
        }

        #endregion
    }
}