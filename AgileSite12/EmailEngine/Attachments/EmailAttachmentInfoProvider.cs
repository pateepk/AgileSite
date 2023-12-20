using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Class providing EmailAttachmentInfo management.
    /// </summary>
    public class EmailAttachmentInfoProvider : AbstractInfoProvider<EmailAttachmentInfo, EmailAttachmentInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the EmailAttachmentInfo structure for the specified emailAttachment.
        /// </summary>
        /// <param name="emailAttachmentId">EmailAttachment ID</param>
        public static EmailAttachmentInfo GetEmailAttachmentInfo(int emailAttachmentId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", emailAttachmentId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.EmailAttachment.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new EmailAttachmentInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Gets EmailAttachments that correspond to specified criteria.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N expression</param>
        /// <param name="columns">Columns expression</param>
        public static DataSet GetEmailAttachmentInfos(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("cms.emailattachment.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all attachments attached to specified email.
        /// </summary>
        /// <param name="emailId">Email ID</param>
        public static DataSet GetEmailAttachmentInfos(int emailId)
        {
            return GetEmailAttachmentInfos(emailId, null, -1, null);
        }


        /// <summary>
        /// Gets attachments attached to specified email and that correspond to specified criteria.
        /// </summary>
        /// <param name="emailId">Email ID</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N expression</param>
        /// <param name="columns">Columns expression</param>
        public static DataSet GetEmailAttachmentInfos(int emailId, string orderBy, int topN, string columns)
        {
            return GetEmailAttachmentInfos("[AttachmentID] IN (SELECT [AttachmentID] FROM CMS_AttachmentForEmail WHERE [EmailID] = " + emailId.ToString() + ")", orderBy, topN, columns);
        }


        /// <summary>
        /// Sets (updates or inserts) specified emailAttachment.
        /// </summary>
        /// <param name="emailAttachment">EmailAttachment to set</param>
        public static void SetEmailAttachmentInfo(EmailAttachmentInfo emailAttachment)
        {
            if (emailAttachment != null)
            {
                if (emailAttachment.AttachmentID > 0)
                {
                    emailAttachment.Generalized.UpdateData();
                }
                else
                {
                    emailAttachment.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[EmailAttachmentInfoProvider.SetEmailAttachmentInfo]: No EmailAttachmentInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified emailAttachment.
        /// </summary>
        /// <param name="infoObj">EmailAttachment object</param>
        public static void DeleteEmailAttachmentInfo(EmailAttachmentInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified emailAttachment.
        /// </summary>
        /// <param name="emailAttachmentId">EmailAttachment ID</param>
        public static void DeleteEmailAttachmentInfo(int emailAttachmentId)
        {
            EmailAttachmentInfo infoObj = GetEmailAttachmentInfo(emailAttachmentId);
            DeleteEmailAttachmentInfo(infoObj);
        }

        #endregion
    }
}