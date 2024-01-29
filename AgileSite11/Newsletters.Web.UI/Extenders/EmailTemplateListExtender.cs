using System;
using System.Linq;

using CMS;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Newsletters.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("NewsletterEmailTemplateListExtender", typeof(EmailTemplateListExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Email marketing template UIForm extender
    /// </summary>
    public class EmailTemplateListExtender : ControlExtender<UniGrid>
    {
        private CMSPage Page
        {
            get
            {
                return (CMSPage)Control.Page;
            }
        }


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.OnAction += Control_OnAction;
            Control.OnExternalDataBound += Control_OnExternalDataBound;
        }


        /// <summary>
        /// Handles the UniGrid's OnAction event.
        /// </summary>
        /// <param name="actionName">Name of item (button) that throws event</param>
        /// <param name="actionArgument">ID (value of Primary key) of corresponding data row</param>
        private void Control_OnAction(string actionName, object actionArgument)
        {
            if (actionName.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                DeleteTemplate(ValidationHelper.GetInteger(actionArgument, 0));
            }
        }


        private void DeleteTemplate(int templateId)
        {
            var isInUse = IsTemplateInNewsletterUse(templateId) || IsTemplateInEmailUse(templateId);

            if (!isInUse)
            {
                // Delete EmailTemplate object from database
                EmailTemplateInfoProvider.DeleteEmailTemplateInfo(templateId);
            }
        }


        private bool IsTemplateInEmailUse(int templateId)
        {
            // Check if the template is used in an issue
            var newsletterIssuesIDs = IssueInfoProvider.GetIssues().WhereEquals("IssueTemplateID", templateId).TopN(1).Column("IssueID");
            if (!newsletterIssuesIDs.Any())
            {
                return false;
            }

            Page.ShowError(ResHelper.GetString("NewsletterTemplate_List.TemplateInUseByNewsletterIssue"));
            return true;
        }


        private bool IsTemplateInNewsletterUse(int templateId)
        {
            // Check if the template is used in a newsletter
            var usedInNewsletter = NewsletterInfoProvider
                .GetNewsletters()
                .WhereEquals("NewsletterSubscriptionTemplateID", templateId)
                .Or()
                .WhereEquals("NewsletterUnsubscriptionTemplateID", templateId)
                .Or()
                .WhereEquals("NewsletterOptInTemplateID", templateId)
                .Column("NewsletterID")
                .TopN(1);

            // Check if the template is used in a template to newsletter binding
            var usedInNewsletterBinding = EmailTemplateNewsletterInfoProvider.GetEmailTemplateNewsletters()
                .WhereEquals("TemplateID", templateId)
                .TopN(1);

            if (!usedInNewsletter.Any() && !usedInNewsletterBinding.Any())
            {
                return false;
            }

            Page.ShowError(ResHelper.GetString("NewsletterTemplate_List.TemplateInUseByNewsletter"));
            return true;
        }


        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (sourceName.Equals("templatetype", StringComparison.OrdinalIgnoreCase))
            {
                EmailTemplateTypeEnum templateType = parameter.ToString().ToEnum<EmailTemplateTypeEnum>();
                return templateType.ToLocalizedString("EmailTemplateTypeEnum");
            }

            return parameter;
        }
    }
}