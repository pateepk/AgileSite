using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Core;
using CMS.Helpers;
using CMS.Newsletters.Web.UI.Internal;
using CMS.SiteProvider;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(NewslettersController))]

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Handles various operations for newsletter module.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    [IsAuthorizedPerResource(ModuleName.NEWSLETTER, "Read")]
    [IsAuthorizedPerResource(ModuleName.NEWSLETTER, "Configure")]
    public sealed class NewslettersController : CMSApiController
    {
        /// <summary>
        /// Return all available email campaigns for current site.
        /// </summary>
        [HttpGet]
        public IEnumerable<NewsletterViewModel> GetAllEmailCampaigns()
        {
            return NewsletterInfoProvider.GetNewsletters()
                                         .OnSite(SiteContext.CurrentSiteID)
                                         .WhereEquals("NewsletterType", (int)EmailCommunicationTypeEnum.EmailCampaign).TypedResult
                                         .Select(newsletter => ConvertNewsletterInfoToModel(newsletter, GetNewsletterTemplates(newsletter.NewsletterID)));
        }

        private IEnumerable<TemplateViewModel> GetNewsletterTemplates(int newsletterID)
        {
            var templateIds = EmailTemplateNewsletterInfoProvider.GetEmailTemplateNewsletters()
                                               .WhereEquals("NewsletterID", newsletterID)
                                               .Column("TemplateID");

            return EmailTemplateInfoProvider.GetEmailTemplates()
                                            .WhereIn("TemplateID", templateIds)
                                            .WhereEquals("TemplateType", EmailTemplateTypeEnum.Issue.ToStringRepresentation())
                                            .Columns("TemplateID", "TemplateType", "TemplateDisplayName")
                                            .ToList()
                                            .Select(template => new TemplateViewModel
                                            {
                                                Id = template.TemplateID,
                                                DisplayName = template.TemplateDisplayName,
                                                Type = GetTemplateType(template.TemplateType),
                                            });

        }


        /// <summary>
        /// Return list of <see cref="IssueViewModel"/> for given issueIdList.
        /// </summary>
        /// <remarks>
        /// If no issue can be found then empty <see cref="IssueViewModel"/> is returned (only issue id is set).
        /// </remarks>
        [HttpGet]
        public IEnumerable<IssueViewModel> GetIssues([FromUri] List<int> issueIds)
        {
            var issues = IssueInfoProvider.GetIssues()
                                          .OnSite(SiteContext.CurrentSiteID)
                                          .WhereIn("IssueID", issueIds)
                                          .ToList();

            Func<int, IssueInfo, IssueViewModel> buildIssueViewModel = (issueId, issue) => new IssueViewModel
            {
                Id = issueId,
                NewsletterId = issue != null ? issue.IssueNewsletterID : 0,
                Subject = issue != null ? issue.IssueSubject : null,
            };

            foreach (var issue in issues)
            {
                yield return buildIssueViewModel(issue.IssueID, issue);
            }

            foreach (var issueId in issueIds.Except(issues.Select(issue => issue.IssueID)))
            {
                yield return buildIssueViewModel(issueId, null);
            }
        }


        /// <summary>
        /// Creates issue for given newsletter.
        /// </summary>
        /// <returns>New <see cref="IssueViewModel"/></returns>
        /// <exception cref="ArgumentException">When <paramref name="newsletterId"/> is not set, when <paramref name="templateId"/> is not assigned to specified newsletter or when <paramref name="subject"/> is null or empty.</exception>
        [HttpPost]
        public IssueViewModel CreateNewIssue([FromUri] int newsletterId, [FromUri] int templateId, [FromBody] string subject)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("[NewslettersController.CreateNewIssue] Subject should not be empty.");
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterId);
            if (newsletter == null || newsletter.NewsletterSiteID != SiteContext.CurrentSiteID)
            {
                throw new ArgumentException(string.Format("[NewslettersController.CreateNewIssue] Newsletter with id {0} cannot be found", newsletterId));
            }

            var emailTemplateNewsletter = EmailTemplateNewsletterInfoProvider.GetEmailTemplateNewsletterInfo(templateId, newsletterId);
            if (emailTemplateNewsletter == null)
            {
                throw new ArgumentException(string.Format("[NewslettersController.CreateNewIssue] Template with id {0} cannot be found or is not assigned to specified newsletter.", templateId));
            }
            
            var issue = new IssueInfo
            {
                IssueNewsletterID = newsletterId,
                IssueDisplayName = TextHelper.LimitLength(subject, 200, wholeWords: true),
                IssueSubject = subject,
                IssueText = "",
                IssueUnsubscribed = 0,
                IssueSentEmails = 0,
                IssueSiteID = newsletter.NewsletterSiteID,
                IssueUTMSource = "",
                IssueUTMCampaign = "",
                IssueUseUTM = false,
                IssueTemplateID = templateId,
            };
            IssueInfoProvider.SetIssueInfo(issue);

            return new IssueViewModel
            {
                Id = issue.IssueID,
                NewsletterId = newsletterId,
                Subject = subject,
            };
        }


        /// <summary>
        /// Creates static (template based) newsletter.
        /// </summary>
        /// <returns>New <see cref="NewsletterViewModel"/></returns>
        /// <exception cref="HttpResponseException">When <see cref="CreateNewsletterModel"/> invalid.</exception>
        [HttpPost]
        public NewsletterViewModel CreateEmailCampaign(CreateNewsletterModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var newsletter = new NewsletterInfo
            {
                NewsletterDisplayName = model.DisplayName,
                NewsletterSenderName = model.SenderName,
                NewsletterSenderEmail = model.SenderEmail,
                NewsletterUnsubscriptionTemplateID = model.UnsubscriptionTemplateId,
                NewsletterSiteID = SiteContext.CurrentSiteID,
                NewsletterSource = NewsletterSource.TemplateBased,
                NewsletterTrackOpenEmails = true,
                NewsletterTrackClickedLinks = true,
                NewsletterLogActivity = true,
                NewsletterType = EmailCommunicationTypeEnum.EmailCampaign
            };
            newsletter.Generalized.EnsureCodeName();
            NewsletterInfoProvider.SetNewsletterInfo(newsletter);
            EmailTemplateNewsletterInfoProvider.AddNewsletterToTemplate(model.IssueTemplateId, newsletter.NewsletterID);

            return ConvertNewsletterInfoToModel(newsletter, GetNewsletterTemplates(newsletter.NewsletterID));
        }


        /// <summary>
        /// Returns available templates for current site.
        /// </summary>
        [HttpGet]
        public IEnumerable<TemplateViewModel> GetTemplates()
        {
            return EmailTemplateInfoProvider.GetEmailTemplates()
                                            .OnSite(SiteContext.CurrentSiteID)
                                            .Columns("TemplateID", "TemplateType", "TemplateDisplayName")
                                            .ToList()
                                            .Select(template => new TemplateViewModel
                                            {
                                                Id = template.TemplateID,
                                                DisplayName = template.TemplateDisplayName,
                                                Type = GetTemplateType(template.TemplateType),
                                            });
        }


        private NewsletterViewModel ConvertNewsletterInfoToModel(NewsletterInfo newsletter, IEnumerable<TemplateViewModel> templateIds)
        {
            return new NewsletterViewModel
            {
                Id = newsletter.NewsletterID,
                DisplayName = newsletter.NewsletterDisplayName,
                SenderName = newsletter.NewsletterSenderName,
                SenderEmail = newsletter.NewsletterSenderEmail,
                Templates = new Dictionary<string, int>
                {
                    {"unsubscription", newsletter.NewsletterUnsubscriptionTemplateID},
                },
                IssueTemplates = templateIds.ToList()
            };
        }


        private string GetTemplateType(EmailTemplateTypeEnum type)
        {
            switch (type)
            {
                case EmailTemplateTypeEnum.Subscription: return "subscription";
                case EmailTemplateTypeEnum.Unsubscription: return "unsubscription";
                case EmailTemplateTypeEnum.Issue: return "issue";
                case EmailTemplateTypeEnum.DoubleOptIn: return "doubleOptIn";
                default: throw new ArgumentException(string.Format("[NewslettersController.GetTemplateType]: Unknown email template type {0}", type));
            }
        }
    }
}
