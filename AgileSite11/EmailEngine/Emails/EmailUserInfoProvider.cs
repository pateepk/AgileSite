using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Class providing EmailUserInfo management.
    /// </summary>
    public class EmailUserInfoProvider : AbstractInfoProvider<EmailUserInfo, EmailUserInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns the EmailUserInfo structure for the specified email and user IDs.
        /// </summary>
        /// <param name="emailId">Email ID</param>
        /// <param name="userId">User ID</param>
        public static EmailUserInfo GetEmailUserInfo(int emailId, int userId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@EmailID", emailId);
            parameters.Add("@UserID", userId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.emailuser.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new EmailUserInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns dataset of EmailUserInfo objects specified by where condition.
        /// </summary>
        /// <param name="where">Criteria to filter data against</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">How many records to retrieve</param>
        /// <param name="columns">Which columns to retrieve</param>
        /// <returns>DataSet containg EmailUserInfo objects</returns>
        public static DataSet GetEmailUserInfos(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("cms.emailuser.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified EmailUserInfo.
        /// </summary>
        /// <param name="emailUser">EmailUserInfo object to set</param>
        public static void SetEmailUserInfo(EmailUserInfo emailUser)
        {
            if (emailUser != null)
            {
                // Check IDs
                if ((emailUser.EmailID <= 0) || (emailUser.UserID <= 0))
                {
                    throw new Exception("[EmailUserInfoProvider.SetEmailUserInfo]: Object IDs not set.");
                }

                // Get existing
                EmailUserInfo existing = GetEmailUserInfo(emailUser.EmailID, emailUser.UserID);
                if (existing != null)
                {
                    emailUser.Generalized.UpdateData();
                }
                else
                {
                    emailUser.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[EmailUserInfoProvider.SetEmailUserInfo]: No EmailUserInfo object set.");
            }
        }


        /// <summary>
        /// Deletes EmailUserInfo specified by EmailID and UserID.
        /// </summary>
        /// <param name="emailId">E-mail ID</param>
        /// <param name="userId">User ID</param>
        public static void DeleteEmailUserInfo(int emailId, int userId)
        {
            EmailUserInfo eui = GetEmailUserInfo(emailId, userId);
            DeleteEmailUserInfo(eui);
        }


        /// <summary>
        /// Deletes specified EmailUserInfo.
        /// </summary>
        /// <param name="infoObj">EmailUserInfo object</param>
        public static void DeleteEmailUserInfo(EmailUserInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes all EmailUserInfo objects that satisfy the specified criteria.
        /// </summary>
        /// <param name="where">The criteria to filter against</param>
        public static void DeleteEmailUserInfos(string where)
        {
            DataSet emailUsersSet = ConnectionHelper.ExecuteQuery("cms.emailuser.deleteselected", null, where);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Determines whether there are unsent messages to recipients of the specified mass e-mail.
        /// </summary>
        /// <param name="emailId">ID of the mass e-mail</param>
        /// <returns><c>true</c> if there are unsent messages for the specified mass e-mail, otherwise, <c>false</c></returns>
        internal static bool HasFailedEmailUsers(int emailId)
        {
            string where = string.Format("(EmailID = {0} AND Status = {1:D})", emailId, EmailStatusEnum.Waiting);
            DataSet emailUsersSet = GetEmailUserInfos(where, null, 1, "UserID");
            return (!DataHelper.DataSourceIsEmpty(emailUsersSet));
        }


        /// <summary>
        /// Archives the e-mail message for the user specified in the binding object (for mass e-mails).
        /// </summary>
        /// <param name="emailUser">The e-mail user binding</param>  
        internal static void ArchiveEmailUser(EmailUserInfo emailUser)
        {
            emailUser.Status = EmailStatusEnum.Archived;
            emailUser.LastSendAttempt = DateTime.Now;
            emailUser.LastSendResult = null;
            SetEmailUserInfo(emailUser);
        }


        /// <summary>
        /// Saves the failed e-mail message for the user specified in the binding object along 
        /// with the failure notification (for mass e-mails).
        /// </summary>
        /// <param name="emailUser">The e-mail user binding object</param>
        /// <param name="lastSendResult">Reason for the delivery failure</param>
        internal static void SaveFailedEmailUser(EmailUserInfo emailUser, string lastSendResult)
        {
            emailUser.Status = EmailStatusEnum.Waiting;
            emailUser.LastSendAttempt = DateTime.Now;
            emailUser.LastSendResult = lastSendResult;
            SetEmailUserInfo(emailUser);
        }


        /// <summary>
        /// Saves the e-mail message that was not sent for the user specified in the binding 
        /// object along with the failure notification (for mass e-mails)
        /// </summary>
        /// <param name="emailUser">The e-mail user binding object</param>        
        internal static void SaveNotSentEmailUser(EmailUserInfo emailUser)
        {
            emailUser.Status = EmailStatusEnum.Waiting;
            emailUser.LastSendAttempt = DateTime.Now;
            SetEmailUserInfo(emailUser);
        }

        #endregion
    }
}