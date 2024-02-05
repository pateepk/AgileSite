using System.Linq;

using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing EmailTemplateNewsletterInfo management.
    /// </summary>
    public class EmailTemplateNewsletterInfoProvider : AbstractInfoProvider<EmailTemplateNewsletterInfo, EmailTemplateNewsletterInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates a new instance of EmailTemplateNewsletterInfo.
        /// </summary>        
        public EmailTemplateNewsletterInfoProvider()
            : base(EmailTemplateNewsletterInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns a query for all the EmailTemplateNewsletterInfo objects.
        /// </summary>
        public static ObjectQuery<EmailTemplateNewsletterInfo> GetEmailTemplateNewsletters()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified template and newsletter.
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        public static EmailTemplateNewsletterInfo GetEmailTemplateNewsletterInfo(int templateId, int newsletterId)
        {
            return ProviderObject.GetEmailTemplateNewsletterInfoInternal(templateId, newsletterId);
        }


        /// <summary>
        /// Sets relationship between specified template and newsletter.
        /// </summary>
        /// <param name="tempNewsInfo">Template-newsletter relationship to be set</param>
        public static void SetEmailTemplateNewsletterInfo(EmailTemplateNewsletterInfo tempNewsInfo)
        {
            ProviderObject.SetInfo(tempNewsInfo);
        }


        /// <summary>
        /// Sets relationship between specified server and site.
        /// </summary>	
        /// <param name="templateId">Template ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        public static void AddNewsletterToTemplate(int templateId, int newsletterId)
        {
            EmailTemplateNewsletterInfo infoObj = ProviderObject.CreateInfo();

            infoObj.TemplateID = templateId;
            infoObj.NewsletterID = newsletterId;

            SetEmailTemplateNewsletterInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship specified template and newsletter.
        /// </summary>
        /// <param name="tempNewsInfo">Template-newsletter relationship to be set</param>
        public static void DeleteEmailTemplateNewsletterInfo(EmailTemplateNewsletterInfo tempNewsInfo)
        {
            ProviderObject.DeleteInfo(tempNewsInfo);
        }


        /// <summary>
        /// Deletes relationship between specified template and specified newsletter.
        /// </summary>
        /// <param name="templateId">Email template ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        public static void RemoveNewsletterFromTemplate(int templateId, int newsletterId)
        {
            EmailTemplateNewsletterInfo infoObj = GetEmailTemplateNewsletterInfo(templateId, newsletterId);
            DeleteEmailTemplateNewsletterInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns relationship between specified template and newsletter.
        /// </summary>
        /// <param name="templateId">E-mail template ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        protected virtual EmailTemplateNewsletterInfo GetEmailTemplateNewsletterInfoInternal(int templateId, int newsletterId)
        {
            return GetObjectQuery().TopN(1)
                    .WhereEquals("TemplateID", templateId)
                    .WhereEquals("NewsletterID", newsletterId).FirstOrDefault();
        }

        #endregion
    }
}