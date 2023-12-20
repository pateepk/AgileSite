using System;

using CMS.EmailEngine;
using CMS.Newsletters.Internal;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides support for building the <see cref="EmailMessage"/>.
    /// </summary>
    internal sealed class EmailMessageBuilder : IEmailMessageBuilder
    {
        private readonly EmailParts emailParts;
        private readonly IEmailMessageModifier messageModifier;
        private readonly ISenderRetriever customSenderRetriever;


        /// <summary>
        /// Creates an instance of <see cref="EmailMessageBuilder"/> class.
        /// </summary>
        /// <param name="emailParts">Parts of an email to build the email message from.</param>
        /// <param name="messageModifier">Email message modifier to be applied to the message.</param>
        /// <param name="customSenderRetriever">Custom sender email and name retriever.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="emailParts"/> is null.</exception>
        public EmailMessageBuilder(EmailParts emailParts, IEmailMessageModifier messageModifier = null, ISenderRetriever customSenderRetriever = null)
        {
            this.emailParts = emailParts;
            if (emailParts == null)
            {
                throw new ArgumentNullException(nameof(emailParts));
            }
            this.messageModifier = messageModifier;
            this.customSenderRetriever = customSenderRetriever;
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
            var senderRetriever = GetSenderRetriever();

            return new EmailMessage
            {
                EmailFormat = EmailFormatEnum.Both,
                Priority = EmailPriorityEnum.Low,
                Body = emailParts.GetBody(),
                Subject = emailParts.GetSubject(),
                From = senderRetriever.GetFrom(),
                ReplyTo = senderRetriever.GetReplyTo(),
                PlainTextBody = emailParts.GetPlainText()
            };
        }


        private ISenderRetriever GetSenderRetriever()
        {
            return customSenderRetriever ?? new SenderRetriever(emailParts.Issue, emailParts.Newsletter);
        }


        private void ApplyModifier(EmailMessage message)
        {
            messageModifier?.Apply(message);
        }
    }
}