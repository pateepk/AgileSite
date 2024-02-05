using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Class providing AttachmentForEmailInfo management.
    /// </summary>
    public class AttachmentForEmailInfoProvider : AbstractInfoProvider<AttachmentForEmailInfo, AttachmentForEmailInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the AttachmentForEmailInfo structure for the specified attachmentForEmail.
        /// </summary>
        /// <param name="emailId">EmailID</param>
        /// <param name="attachmentId">AttachmentID</param>
        public static AttachmentForEmailInfo GetAttachmentForEmailInfo(int emailId, int attachmentId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@EmailID", emailId);
            parameters.Add("@AttachmentID", attachmentId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.attachmentforemail.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new AttachmentForEmailInfo(ds.Tables[0].Rows[0]);
            }
            
            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified attachmentForEmail.
        /// </summary>
        /// <param name="attachmentForEmail">AttachmentForEmail to set</param>
        public static void SetAttachmentForEmailInfo(AttachmentForEmailInfo attachmentForEmail)
        {
            if (attachmentForEmail != null)
            {
                // Check IDs
                if ((attachmentForEmail.EmailID <= 0) || (attachmentForEmail.AttachmentID <= 0))
                {
                    throw new Exception("[AttachmentForEmailInfoProvider.SetAttachmentForEmailInfo]: Object IDs not set.");
                }

                // Get existing
                AttachmentForEmailInfo existing = GetAttachmentForEmailInfo(attachmentForEmail.EmailID, attachmentForEmail.AttachmentID);
                if (existing != null)
                {
                    attachmentForEmail.Generalized.UpdateData();
                }
                else
                {
                    attachmentForEmail.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[AttachmentForEmailInfoProvider.SetAttachmentForEmailInfo]: No AttachmentForEmailInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified attachmentForEmail.
        /// </summary>
        /// <param name="infoObj">AttachmentForEmail object</param>
        public static void DeleteAttachmentForEmailInfo(AttachmentForEmailInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified attachmentForEmail.
        /// </summary>
        /// <param name="emailId">EmailID</param>
        /// <param name="attachmentId">AttachmentID</param>
        public static void DeleteAttachmentForEmailInfo(int emailId, int attachmentId)
        {
            AttachmentForEmailInfo infoObj = GetAttachmentForEmailInfo(emailId, attachmentId);
            DeleteAttachmentForEmailInfo(infoObj);
        }


        /// <summary>
        /// Returns a query for all the AttachmentForEmailInfo objects.
        /// </summary>
        public static ObjectQuery<AttachmentForEmailInfo> GetAttachmentForEmailInfos()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion
    }
}