using CMS.DataEngine;

namespace CMS.EmailEngine
{
    using TypedDataSet = InfoDataSet<EmailTemplateInfo>;

    /// <summary>
    /// Provides access to email templates.
    /// </summary>
    public class EmailTemplateProvider : AbstractInfoProvider<EmailTemplateInfo, EmailTemplateProvider>
    {
        #region "Constants"

        /// <summary>
        /// Used by GetEmailTemplates(int siteID) to compare with siteID. 
        /// ALL_TEMPLATES represents templates from all sites.
        /// </summary>
        public const int ALL_TEMPLATES = -1;


        /// <summary>
        /// Used by GetEmailTemplates(int siteID) to compare with siteID. 
        /// GLOBAL_TEMPLATES represents global templates, ie. templates with siteID = NULL.
        /// </summary>
        public const int GLOBAL_TEMPLATES = 0;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns e-mail template given by template name and site ID.
        /// Returns global e-mail template if site template was not found.
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <param name="siteId">Template site ID</param>
        public static EmailTemplateInfo GetEmailTemplate(string templateName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(templateName, siteId, true, true);
        }


        /// <summary>
        /// Returns e-mail template given by template name and site name (or just template name for global templates).
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <param name="siteName">Site name or null for global templates</param>
        public static EmailTemplateInfo GetEmailTemplate(string templateName, string siteName)
        {
            return ProviderObject.GetEmailTemplateInternal(templateName, siteName);
        }


        /// <summary>
        /// Returns e-mail template given by template ID.
        /// </summary>
        /// <param name="templateId">Template ID</param>
        public static EmailTemplateInfo GetEmailTemplate(int templateId)
        {
            return ProviderObject.GetInfoById(templateId);
        }


        /// <summary>
        /// Updates e-mail template into DB or inserts new one.
        /// </summary>
        /// <param name="template">Template object</param>
        public static void SetEmailTemplate(EmailTemplateInfo template)
        {
            ProviderObject.SetInfo(template);
        }


        /// <summary>
        /// Deletes e-mail template.
        /// </summary>
        /// <param name="template">Template object</param>
        public static void DeleteEmailTemplate(EmailTemplateInfo template)
        {
            ProviderObject.DeleteInfo(template);
        }


        /// <summary>
        /// Deletes e-mail template given by template name and site name.
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <param name="siteName">Site name</param>
        public static void DeleteEmailTemplate(string templateName, string siteName)
        {
            EmailTemplateInfo emailTemplateInfo = GetEmailTemplate(templateName, siteName);
            DeleteEmailTemplate(emailTemplateInfo);
        }


        /// <summary>
        /// Deletes e-mail template given by the ID.
        /// </summary>
        /// <param name="templateId">Template ID</param>
        public static void DeleteEmailTemplate(int templateId)
        {
            EmailTemplateInfo emailTemplateInfo = GetEmailTemplate(templateId);
            DeleteEmailTemplate(emailTemplateInfo);
        }


        /// <summary>
        /// Returns all e-mail templates.
        /// </summary>
        public static ObjectQuery<EmailTemplateInfo> GetEmailTemplates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all e-mail templates for specific site or all global templates or all templates sorted by template display name ascending.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static TypedDataSet GetEmailTemplates(int siteId)
        {
            return ProviderObject.GetEmailTemplatesInternal(siteId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns e-mail template given by template name and site name (or just template name for global templates).
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <param name="siteName">Site name or null for global templates</param>
        protected virtual EmailTemplateInfo GetEmailTemplateInternal(string templateName, string siteName)
        {
            return GetInfoByCodeName(templateName, EmailHelper.GetSiteId(siteName), true, true);
        }


        /// <summary>
        /// Returns all e-mail templates for specific site or all global templates or all templates sorted by template display name ascending.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual TypedDataSet GetEmailTemplatesInternal(int siteId)
        {
            var includeGlobal = (siteId <= GLOBAL_TEMPLATES);

            return GetObjectQuery().OnSite(siteId, includeGlobal).OrderByAscending("EmailTemplateDisplayName").TypedResult;
        }

        #endregion
    }
}