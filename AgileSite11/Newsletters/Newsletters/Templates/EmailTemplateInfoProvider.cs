using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Newsletters
{
    using TypedDataSet = InfoDataSet<EmailTemplateInfo>;

    /// <summary>
    /// Class providing EmailTemplate management.
    /// </summary>
    public class EmailTemplateInfoProvider : AbstractInfoProvider<EmailTemplateInfo, EmailTemplateInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public EmailTemplateInfoProvider()
            : base(EmailTemplateInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns a query for all the EmailTemplateInfo objects.
        /// </summary>
        public static ObjectQuery<EmailTemplateInfo> GetEmailTemplates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets the e-mail template.
        /// </summary>
        /// <param name="templateId">Template ID</param>
        public static EmailTemplateInfo GetEmailTemplateInfo(int templateId)
        {
            return ProviderObject.GetInfoById(templateId);
        }


        /// <summary>
        /// Gets the e-mail template.
        /// </summary>
        /// <param name="templateName">Template code name</param>
        /// <param name="siteId">Template site ID</param>
        public static EmailTemplateInfo GetEmailTemplateInfo(string templateName, int siteId)
        {
            return ProviderObject.GetInfoByCodeName(templateName, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified e-mail template.
        /// </summary>
        /// <param name="emailTemplate">Template object</param>
        public static void SetEmailTemplateInfo(EmailTemplateInfo emailTemplate)
        {
            if (emailTemplate != null)
            {
                ProviderObject.SetInfo(emailTemplate);
            }
            else
            {
                throw new Exception("[EmailTemplateInfoProvider.SetEmailTemplate]: No EmailTemplate object set.");
            }
        }


        /// <summary>
        /// Deletes specified e-mail template.
        /// </summary>
        /// <param name="emailTemplate">Template object</param>
        public static void DeleteEmailTemplateInfo(EmailTemplateInfo emailTemplate)
        {
            ProviderObject.DeleteInfo(emailTemplate);
        }


        /// <summary>
        /// Deletes specified e-mail template.
        /// </summary>
        /// <param name="emailTemplateId">Template ID</param>
        public static void DeleteEmailTemplateInfo(int emailTemplateId)
        {
            EmailTemplateInfo emailTemplateObj = GetEmailTemplateInfo(emailTemplateId);
            DeleteEmailTemplateInfo(emailTemplateObj);
        }


        /// <summary>
        /// Returns dataset with all subscription templates.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static TypedDataSet GetAllSubscriptionTemplates(int siteId)
        {
            return ProviderObject
                    .GetObjectQuery()
                    .WhereEquals("TemplateType", "S")
                    .WhereEquals("TemplateSiteID", siteId)
                    .TypedResult;
        }


        /// <summary>
        /// Returns dataset with all unsubscription templates.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static TypedDataSet GetAllUnsubscriptionTemplates(int siteId)
        {
            return ProviderObject
                    .GetObjectQuery()
                    .WhereEquals("TemplateType", "U")
                    .WhereEquals("TemplateSiteID", siteId)
                    .TypedResult;
        }


        /// <summary>
        /// Returns dataset with all double opt-in templates.
        /// </summary>
        /// <param name="siteId">ID of a site</param>
        /// <returns>DataSet with double opt-in templates</returns>
        public static TypedDataSet GetAllDoubleOptInTemplates(int siteId)
        {
            return ProviderObject
                    .GetObjectQuery()
                    .WhereEquals("TemplateType", "D")
                    .WhereEquals("TemplateSiteID", siteId)
                    .TypedResult;
        }


        /// <summary>
        /// Returns dataset with all e-mail templates.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static TypedDataSet GetAllIssueTemplates(int siteId)
        {
            return ProviderObject
                    .GetObjectQuery()
                    .WhereEquals("TemplateType", "I")
                    .WhereEquals("TemplateSiteID", siteId)
                    .TypedResult;
        }


        /// <summary>
        /// Returns virtual path to the template's stylesheet.
        /// </summary>
        /// <param name="codeName">CSS stylesheet code name</param>
        public static string GetStylesheetUrl(string codeName)
        {
            string querystring = $"?newslettertemplatename={codeName}";
            return CssLinkHelper.GetCssUrl(querystring);
        }

        #endregion
    }
}