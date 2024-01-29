using CMS.DataEngine;
using System.Linq;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing EmailWidgetTemplateInfo management.
    /// </summary>
    public class EmailWidgetTemplateInfoProvider : AbstractInfoProvider<EmailWidgetTemplateInfo, EmailWidgetTemplateInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all EmailWidgetTemplateInfo bindings.
        /// </summary>
        public static ObjectQuery<EmailWidgetTemplateInfo> GetEmailWidgetTemplates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns EmailWidgetTemplateInfo binding structure.
        /// </summary>
        /// <param name="emailwidgetId">Email widget ID</param>
        /// <param name="emailtemplateId">Email template ID</param>  
        public static EmailWidgetTemplateInfo GetEmailWidgetTemplateInfo(int emailwidgetId, int emailtemplateId)
        {
            return ProviderObject.GetEmailWidgetTemplateInfoInternal(emailwidgetId, emailtemplateId);
        }


        /// <summary>
        /// Sets specified EmailWidgetTemplateInfo.
        /// </summary>
        /// <param name="infoObj">EmailWidgetTemplateInfo to set</param>
        public static void SetEmailWidgetTemplateInfo(EmailWidgetTemplateInfo infoObj)
        {
            ProviderObject.SetEmailWidgetTemplateInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified EmailWidgetTemplateInfo binding.
        /// </summary>
        /// <param name="infoObj">EmailWidgetTemplateInfo object</param>
        public static void DeleteEmailWidgetTemplateInfo(EmailWidgetTemplateInfo infoObj)
        {
            ProviderObject.DeleteEmailWidgetTemplateInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes EmailWidgetTemplateInfo binding.
        /// </summary>
        /// <param name="emailwidgetId">Email widget ID</param>
        /// <param name="emailtemplateId">Email template ID</param>  
        public static void RemoveEmailWidgetFromEmailTemplate(int emailwidgetId, int emailtemplateId)
        {
            ProviderObject.RemoveEmailWidgetFromEmailTemplateInternal(emailwidgetId, emailtemplateId);
        }


        /// <summary>
        /// Creates EmailWidgetTemplateInfo binding. 
        /// </summary>
        /// <param name="emailwidgetId">Email widget ID</param>
        /// <param name="emailtemplateId">Email template ID</param>   
        public static void AddEmailWidgetToEmailTemplate(int emailwidgetId, int emailtemplateId)
        {
            ProviderObject.AddEmailWidgetToEmailTemplateInternal(emailwidgetId, emailtemplateId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the EmailWidgetTemplateInfo structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="emailwidgetId">Email widget ID</param>
        /// <param name="emailtemplateId">Email template ID</param>  
        protected virtual EmailWidgetTemplateInfo GetEmailWidgetTemplateInfoInternal(int emailwidgetId, int emailtemplateId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("EmailWidgetID", emailwidgetId)
                .WhereEquals("TemplateID", emailtemplateId).FirstOrDefault();
        }


        /// <summary>
        /// Sets specified EmailWidgetTemplateInfo binding.
        /// </summary>
        /// <param name="infoObj">EmailWidgetTemplateInfo object</param>
        protected virtual void SetEmailWidgetTemplateInfoInternal(EmailWidgetTemplateInfo infoObj)
        {
            SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified EmailWidgetTemplateInfo.
        /// </summary>
        /// <param name="infoObj">EmailWidgetTemplateInfo object</param>
        protected virtual void DeleteEmailWidgetTemplateInfoInternal(EmailWidgetTemplateInfo infoObj)
        {
            DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes EmailWidgetTemplateInfo binding.
        /// </summary>
        /// <param name="emailwidgetId">Email widget ID</param>
        /// <param name="emailtemplateId">Email template ID</param>  
        protected virtual void RemoveEmailWidgetFromEmailTemplateInternal(int emailwidgetId, int emailtemplateId)
        {
            var infoObj = GetEmailWidgetTemplateInfo(emailwidgetId, emailtemplateId);
            if (infoObj != null)
            {
                DeleteEmailWidgetTemplateInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates EmailWidgetTemplateInfo binding. 
        /// </summary>
        /// <param name="emailwidgetId">Email widget ID</param>
        /// <param name="emailtemplateId">Email template ID</param>   
        protected virtual void AddEmailWidgetToEmailTemplateInternal(int emailwidgetId, int emailtemplateId)
        {
            // Create new binding
            var infoObj = new EmailWidgetTemplateInfo();
            infoObj.EmailWidgetID = emailwidgetId;
            infoObj.TemplateID = emailtemplateId;

            // Save to the database
            SetEmailWidgetTemplateInfo(infoObj);
        }

        #endregion
    }
}