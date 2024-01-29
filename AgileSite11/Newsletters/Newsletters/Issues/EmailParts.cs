using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.Newsletters.Filters;
using CMS.Newsletters.Issues.Widgets.Configuration;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides support for building an email from issue and newsletter.
    /// </summary>
    internal class EmailParts
    {
        /// <summary>
        /// Issue.
        /// </summary>
        public IssueInfo Issue { get; private set; }


        /// <summary>
        /// Issue newsletter.
        /// </summary>
        public NewsletterInfo Newsletter { get; private set; }


        /// <summary>
        /// Newsletter base url.
        /// </summary>
        public string BaseUrl { get; private set; }

        private string subject;
        private string plainText;
        private string text;
        private string templateCode;

        private EmailTemplateInfo template;
        private IZonesConfigurationService configurationService;
        private HashSet<WidgetCode> widgets;
        private ICssInlinerService cssInlinerService;


        private class WidgetCode
        {
            public string ZoneIdentifier;
            public Guid WidgetIdentifier;
            public string Html;
        }


        /// <summary>
        /// Creates an instance of <see cref="EmailParts"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="customCssInlinerService">Custom CSS inlining service.</param>
        public EmailParts(IssueInfo issue, NewsletterInfo newsletter, ICssInlinerService customCssInlinerService = null)
        {
            Issue = issue;
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            Newsletter = newsletter;
            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            BaseUrl = Service.Resolve<IIssueUrlService>().GetBaseUrl(Newsletter);
            if (newsletter.NewsletterSource == NewsletterSource.TemplateBased)
            {
                configurationService = Service.Resolve<IZonesConfigurationServiceFactory>().Create(Issue.IssueID);
                templateCode = GetTemplateCode(issue);
                widgets = GetWidgets(issue);
            }
            else
            {
                text = issue.IssueText;
                widgets = new HashSet<WidgetCode>();
            }

            subject = issue.IssueSubject;
            plainText = issue.IssuePlainText;
            cssInlinerService = customCssInlinerService ?? Service.Resolve<ICssInlinerService>();
        }


        /// <summary>
        /// Private constructor to support faster object clone.
        /// </summary>
        private EmailParts()
        {
        }


        /// <summary>
        /// Creates clone of the current instance of <see cref="EmailParts"/>.
        /// </summary>
        public EmailParts Clone()
        {
            var clone = new EmailParts
            {
                Issue = Issue,
                Newsletter = Newsletter,
                BaseUrl = BaseUrl,

                subject = subject,
                plainText = plainText,
                text = text,
                templateCode = templateCode,

                template = template,
                configurationService = configurationService,
                cssInlinerService = cssInlinerService,

                widgets = CloneWidgets()
            };

            return clone;
        }


        private HashSet<WidgetCode> CloneWidgets()
        {
            var widgetsClone = new HashSet<WidgetCode>();
            foreach (var widgetCode in widgets)
            {
                widgetsClone.Add(new WidgetCode
                {
                    ZoneIdentifier = widgetCode.ZoneIdentifier,
                    WidgetIdentifier = widgetCode.WidgetIdentifier,
                    Html = widgetCode.Html
                });
            }

            return widgetsClone;
        }


        private HashSet<WidgetCode> GetWidgets(IssueInfo issue)
        {
            var codes = new HashSet<WidgetCode>();
            var zonesConfiguration = Service.Resolve<IZonesConfigurationServiceFactory>().Create(issue);
            var content = zonesConfiguration.GetEmailContent(null);
            foreach (var zone in content.Zones)
            {
                foreach (var widget in zone.Widgets)
                {
                    codes.Add(new WidgetCode
                    {
                        ZoneIdentifier = zone.Identifier,
                        WidgetIdentifier = widget.Identifier,
                        Html = widget.Html
                    });
                }
            }

            return codes;
        }


        private string GetTemplateCode(IssueInfo issue)
        {
            template = EmailTemplateInfoProvider.GetEmailTemplateInfo(issue.IssueTemplateID);
            if (template == null)
            {
                throw new InvalidOperationException("Missing newsletter template.");
            }

            return template.TemplateCode;
        }


        /// <summary>
        /// Applies filters to email parts (subject, body and plain text).
        /// </summary>
        /// <param name="bodyFilter">Filter applied to the email body content.</param>
        /// <param name="subjectFilter">Filter applied to the email subject content.</param>
        /// <param name="plainTextFilter">Filter applied to the email plain text.</param>
        /// <remarks>This method should be called before Get methods in order to return correctly resolved values.</remarks>
        public void ApplyFilters(IWidgetContentFilter bodyFilter, IEmailContentFilter subjectFilter, IEmailContentFilter plainTextFilter)
        {
            subject = subjectFilter.Apply(subject);
            plainText = plainTextFilter.Apply(plainText);

            if (Newsletter.NewsletterSource == NewsletterSource.TemplateBased)
            {
                templateCode = bodyFilter.Apply(templateCode);
                ApplyFilterToWidgets(bodyFilter);
            }
            else
            {
                text = bodyFilter.Apply(text);
            }
        }


        private void ApplyFilterToWidgets(IWidgetContentFilter bodyFilter)
        {
            foreach (var widget in widgets)
            {
                var configuration = configurationService.GetWidgetConfiguration(widget.WidgetIdentifier);
                if (configuration.WidgetDefinitionNotFound)
                {
                    continue;
                }

                widget.Html = bodyFilter.Apply(widget.Html, configuration);
            }
        }


        /// <summary>
        /// Gets email subject.
        /// </summary>
        public string GetSubject()
        {
            return subject;
        }


        /// <summary>
        /// Gets plain text version of email.
        /// </summary>
        public string GetPlainText()
        {
            return plainText;
        }


        /// <summary>
        /// Gets body of an email.
        /// </summary>
        public string GetBody()
        {
            if (Newsletter.NewsletterSource != NewsletterSource.TemplateBased)
            {
                return text;
            }

            var body = WidgetZonePlaceholderHelper.ReplacePlaceholders(templateCode, GetZoneContent);
            if (template.TemplateInlineCSS)
            {
                body = cssInlinerService.InlineCss(body, new Uri(BaseUrl));
            }

            return body;
        }


        private string GetZoneContent(string zoneIdentifier)
        {
            var html = widgets.Where(w => w.ZoneIdentifier == zoneIdentifier).Select(w => w.Html);
            return string.Join(string.Empty, html);
        }
    }
}
