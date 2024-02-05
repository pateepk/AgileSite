using System;

using CMS.EmailEngine;
using CMS.Newsletters.Filters;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides support for building the <see cref="EmailMessage"/> for confirmation.
    /// </summary>
    internal sealed class ConfirmationMessageBuilder : IEmailMessageBuilder
    {
        private readonly EmailTemplateInfo template;
        private readonly NewsletterInfo newsletter;
        private readonly IEmailContentFilter bodyFilter;
        private readonly IEmailContentFilter subjectFilter;
        private readonly IEmailMessageModifier messageModifier;


        /// <summary>
        /// Creates an instance of <see cref="ConfirmationMessageBuilder"/> class.
        /// </summary>
        /// <param name="template">Template.</param>
        /// <param name="newsletter">Issue newsletter.</param>
        /// <param name="bodyFilter">Filter applied to the email body content.</param>
        /// <param name="subjectFilter">Filter applied to the email subject content.</param>
        /// <param name="messageModifier">Email message modifier to be applied to the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newsletter"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bodyFilter"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subjectFilter"/> is null.</exception>
        public ConfirmationMessageBuilder(EmailTemplateInfo template, NewsletterInfo newsletter, IEmailContentFilter bodyFilter, IEmailContentFilter subjectFilter,
            IEmailMessageModifier messageModifier = null)
        {
            this.template = template;
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            this.newsletter = newsletter;
            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            if (bodyFilter == null)
            {
                throw new ArgumentNullException(nameof(bodyFilter));
            }
            this.bodyFilter = bodyFilter;

            if (subjectFilter == null)
            {
                throw new ArgumentNullException(nameof(subjectFilter));
            }
            this.subjectFilter = subjectFilter;

            this.messageModifier = messageModifier;
        }


        /// <summary>
        /// Builds the email message.
        /// </summary>
        public EmailMessage Build()
        {
            var message = GetMessage();

            ApplyModifier(message);

            return message;
        }


        private EmailMessage GetMessage()
        {
            return new EmailMessage
            {
                EmailFormat = EmailFormatEnum.Html,
                Priority = EmailPriorityEnum.Low,
                Body = GetBody(),
                Subject = GetSubject(),
                From = GetSender()
            };
        }


        private string GetSender()
        {
            return SenderFormatter.GetSender(newsletter.NewsletterSenderName, newsletter.NewsletterSenderEmail);
        }


        private void ApplyModifier(EmailMessage message)
        {
            messageModifier?.Apply(message);
        }


        private string GetSubject()
        {
            return subjectFilter.Apply(template.TemplateSubject);
        }


        private string GetBody()
        {
            return bodyFilter.Apply(template.TemplateCode);
        }
    }
}