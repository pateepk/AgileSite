using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

using CMS.IO;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents an e-mail message.
    /// </summary>
    public class EmailMessage
    {
        #region "Variables"

        private EmailFormatEnum mEmailFormat = EmailFormatEnum.Default;

        private EmailPriorityEnum mPriority = EmailPriorityEnum.Normal;

        private AttachmentCollection mAttachments;

        private NameValueCollection mHeaders;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the From address.       
        /// </summary>
        /// <remarks>
        /// This address specifies on whose behalf the message is sent and is visible in the client's email software.
        /// </remarks>
        public string From
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ReplyTo address.
        /// </summary>
        /// <remarks>
        /// This address specifies where the replies to this message should be sent.
        /// </remarks>
        public string ReplyTo
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the Recipients.
        /// </summary>
        public string Recipients
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CcRecipients.
        /// </summary>
        public string CcRecipients
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the BccRecipients.
        /// </summary>
        public string BccRecipients
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the Subject.
        /// </summary>
        public string Subject
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the Body.
        /// </summary>
        public string Body
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets plain text body.
        /// </summary>
        public string PlainTextBody
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the e-mail format.
        /// </summary>
        public EmailFormatEnum EmailFormat
        {
            get
            {
                return mEmailFormat;
            }
            set
            {
                mEmailFormat = value;
            }
        }


        /// <summary>
        /// Gets or sets e-mail priority.
        /// </summary>
        public EmailPriorityEnum Priority
        {
            get
            {
                return mPriority;
            }
            set
            {
                mPriority = value;
            }
        }


        /// <summary>
        /// Gets the collection of e-mail attachments.
        /// </summary>
        public AttachmentCollection Attachments
        {
            get
            {
                return mAttachments ?? (mAttachments = new MailMessage().Attachments);
            }
        }


        /// <summary>
        /// Gets the collection of extra e-mail header fields (apart from the standard set).
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                return mHeaders ?? (mHeaders = new NameValueCollection());
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessage"/> class.
        /// </summary>
        public EmailMessage()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessage"/> class form the specified message.
        /// </summary>
        /// <param name="message">The mail message to convert from</param>
        public EmailMessage(MailMessage message)
        {
            From = (message.From != null ? message.From.ToString() : string.Empty);
            ReplyTo = EmailHelper.GetReplyToEmailAddress(message);

            Recipients = EmailHelper.Merge(message.To);
            CcRecipients = EmailHelper.Merge(message.CC);
            BccRecipients = EmailHelper.Merge(message.Bcc);

            EmailFormat = GetEmailFormat(message);
            Priority = EmailHelper.ToEmailPriority(message.Priority);

            Subject = message.Subject;

            SetEmailContent(this, message);

            foreach (Attachment attachment in message.Attachments)
            {
                Attachments.Add(attachment);
            }

            foreach (string header in message.Headers.AllKeys)
            {
                Headers.Add(header, message.Headers[header]);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Converts the <see cref="EmailMessage" /> to <see cref="MailMessage" />.
        /// </summary>
        /// <param name="siteName">Site name (determines e-mail format depending on the site settings)</param>
        /// <returns>A <see cref="MailMessage" /> object</returns>
        public MailMessage ToMailMessage(string siteName)
        {
            // Set email format
            EmailFormatEnum format = EmailHelper.ResolveEmailFormat(EmailFormat, siteName);

            // Initialize mail message
            MailMessage email = new MailMessage
            {
                From = new MailAddress(From),
                Subject = Subject,
                IsBodyHtml = (format == EmailFormatEnum.Html || format == EmailFormatEnum.Both)
            };

            // Set reply to address
            if (!string.IsNullOrEmpty(ReplyTo))
            {
                EmailHelper.SetReplyToEmailAddress(email, ReplyTo);
            }

            // Set recipients
            EmailHelper.Fill(email.To, Recipients);
            EmailHelper.Fill(email.CC, CcRecipients);
            EmailHelper.Fill(email.Bcc, BccRecipients);

            // Set mail body (html and/or plain text)
            EmailHelper.SetEmailBody(email, format, Body, PlainTextBody, siteName, Attachments);

            // Add custom headers
            foreach (string header in Headers)
            {
                email.Headers.Add(header, Headers[header]);
            }

            // Set priority
            email.Priority = EmailHelper.ToMailPriority(Priority);

            return email;
        }


        /// <summary>
        /// Gets individual email header fields from header text.
        /// </summary>
        /// <param name="emailHeader">Email header text</param>
        /// <returns>Name-value collection containing individual field names and bodies</returns>
        /// <remarks>
        /// This approximates how the email header fields should be parsed, see RFC 5322 for complete specification.
        /// </remarks>
        public static NameValueCollection GetHeaderFields(string emailHeader)
        {
            if (string.IsNullOrEmpty(emailHeader))
            {
                return new NameValueCollection();
            }

            // Chop email headers into lines that can be processed sequentially
            string[] lines = emailHeader.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            string name = string.Empty;
            string body = string.Empty;
            NameValueCollection fields = new NameValueCollection();

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                int colonIndex = line.IndexOf(":", StringComparison.OrdinalIgnoreCase);

                // Line contains a new field, save old values if they exist and get new values
                if (char.IsLetter(line[0]) && (colonIndex > 0))
                {
                    if (!string.IsNullOrEmpty(body))
                    {
                        fields.Add(name, body);
                    }

                    name = line.Substring(0, colonIndex);
                    body = line.Substring(colonIndex + 1);
                }

                // Continuing previous value if starting with whitespace and field name exists
                if (char.IsWhiteSpace(line[0]) && !string.IsNullOrEmpty(name))
                {
                    body += line;
                }
            }

            // This will handle the last field, which was not processed in cycle
            if (!string.IsNullOrEmpty(name))
            {
                fields.Add(name, body);
            }

            return fields;
        }


        /// <summary>
        /// Gets email header fields joined into a single string.
        /// </summary>
        /// <param name="emailHeader">Collection of email header fields</param>
        /// <returns>Single string with joined email header fields</returns>
        public static string GetHeaderFields(NameValueCollection emailHeader)
        {
            if (emailHeader == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (string name in emailHeader.AllKeys)
            {
                var values = emailHeader.GetValues(name);
                if (values == null)
                {
                    continue;
                }

                foreach (string value in values)
                {
                    builder.AppendFormat("{0}: {1}", name, value);
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }


        private static EmailFormatEnum GetEmailFormat(MailMessage message)
        {
            // Does the message contain both alt views?
            if ((message.AlternateViews.Count == 2) &&
                ((message.AlternateViews[0].ContentType.MediaType == MediaTypeNames.Text.Html &&
                  message.AlternateViews[1].ContentType.MediaType == MediaTypeNames.Text.Plain) ||
                 (message.AlternateViews[0].ContentType.MediaType == MediaTypeNames.Text.Plain &&
                  message.AlternateViews[1].ContentType.MediaType == MediaTypeNames.Text.Html)))
            {
                return EmailFormatEnum.Both;
            }
            
            return message.IsBodyHtml ? EmailFormatEnum.Html : EmailFormatEnum.PlainText;
        }


        private static void SetEmailContent(EmailMessage emessage, MailMessage message)
        {
            // HTML & plain-text
            if (message.AlternateViews.Count > 0)
            {
                foreach (AlternateView view in message.AlternateViews)
                {
                    if (view.ContentType.MediaType == MediaTypeNames.Text.Html)
                    {
                        emessage.Body = Read(view);

                        // Attach linked resources from HTML view to the email message
                        AttachLinkedResources(emessage, view.LinkedResources);
                    }
                    else if (view.ContentType.MediaType == MediaTypeNames.Text.Plain)
                    {
                        emessage.PlainTextBody = Read(view);
                    }
                }

                return;
            }

            // HTML
            if (message.IsBodyHtml)
            {
                emessage.Body = message.Body;
                return;
            }

            // Plain-text
            emessage.PlainTextBody = message.Body;
        }


        private static string Read(AlternateView view)
        {
            // Position must be reset first 
            // (working with a memory stream here, but some precautions are necessary)
            if (view.ContentStream.CanSeek)
            {
                view.ContentStream.Position = 0;
            }

            using (StreamReader reader = StreamReader.New(view.ContentStream))
            {
                return reader.ReadToEnd();
            }
        }


        private static void AttachLinkedResources(EmailMessage message, IEnumerable<LinkedResource> linkedResources)
        {
            // Transform linked resources to attachments
            foreach (var resource in linkedResources)
            {
                var attachment = new Attachment(resource.ContentStream, resource.ContentType)
                {
                    ContentId = resource.ContentId,
                    TransferEncoding = resource.TransferEncoding
                };
                attachment.ContentDisposition.Inline = true;
                attachment.ContentDisposition.DispositionType = DispositionTypeNames.Inline;

                message.Attachments.Add(attachment);
            }
        }

        #endregion
    }
}