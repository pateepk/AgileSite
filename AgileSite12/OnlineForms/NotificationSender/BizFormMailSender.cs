using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Represents default implementation for sending on-line form notification and autoresponder emails.
    /// </summary>
    public class BizFormMailSender : IBizFormMailSender
    {
        private static Regex mRegExEmailMacro;
        private string mSiteName;
        private CultureInfo mCulture;
        private MacroResolver mResolver;


        /// <summary>
        /// Regular expression for macros in e-mail body, macros are in form $$type:fieldname$$ where type is "label" or "value".
        /// </summary>
        private static Regex RegExEmailMacro
        {
            get
            {
                return mRegExEmailMacro ?? (mRegExEmailMacro = RegexHelper.GetRegex("\\$\\$\\w+:(?:[A-Za-z]|_[A-Za-z])\\w*\\$\\$"));
            }
        }


        /// <summary>
        /// Configuration of the form.
        /// </summary>
        protected BizFormInfo FormConfiguration
        {
            get;
            set;
        }


        /// <summary>
        /// Data collected from the form.
        /// </summary>
        protected IDataContainer FormData
        {
            get;
            set;
        }


        /// <summary>
        /// Form structure definition.
        /// </summary>
        protected FormInfo FormDefinition
        {
            get;
            set;
        }


        /// <summary>
        /// Names of file input fields where some files were recently uploaded.
        /// </summary>
        protected IEnumerable<string> Uploads
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if email content should be encoded.
        /// </summary>
        protected bool EncodeEmails
        {
            get;
            set;
        }


        private string SiteName
        {
            get
            {
                if ((mSiteName == null) && (FormConfiguration != null))
                {
                    mSiteName = SiteInfoProvider.GetSiteName(FormConfiguration.FormSiteID);
                }

                return mSiteName;
            }
        }


        private CultureInfo Culture
        {
            get
            {
                return mCulture ?? (mCulture = Thread.CurrentThread.CurrentCulture);
            }
        }


        private MacroResolver Resolver
        {
            get
            {
                if (mResolver == null)
                {
                    mResolver = MacroResolver.GetInstance();
                    mResolver.Culture = Culture.Name;
                    mResolver.Settings.EncodeResolvedValues = EncodeEmails;
                    if (FormData != null)
                    {
                        mResolver.SetAnonymousSourceData(FormData);
                    }
                }

                return mResolver;
            }
        }


        /// <summary>
        /// Initiates the mail sender service with required data.
        /// </summary>
        /// <param name="formConfiguration">Configuration of the online form</param>
        /// <param name="formData">Data collected from the form</param>
        /// <param name="formDefinition">Form structure definition, may include changes from alternative form if alt.form is used; if empty, default definition from <paramref name="formConfiguration"/> is used</param>
        /// <param name="uploads">Names of file input fields where some files were recently uploaded</param>
        /// <param name="encodeEmails">Indicates if email content should be encoded; false by default</param>
        public BizFormMailSender(BizFormInfo formConfiguration, IDataContainer formData, FormInfo formDefinition = null, IEnumerable<string> uploads = null, bool encodeEmails = false)
        {
            FormConfiguration = formConfiguration;
            FormData = formData;
            FormDefinition = formDefinition ?? formConfiguration?.Form;
            Uploads = uploads;
            EncodeEmails = encodeEmails;
        }


        /// <summary>
        /// Sends notification email to specified person based on on-line form configuration and collected data.
        /// </summary>
        public void SendNotificationEmail()
        {
            if ((FormData == null) || (FormConfiguration == null) || (FormDefinition == null))
            {
                return;
            }

            var fromEmail = FormConfiguration.FormSendFromEmail;
            var recipients = FormConfiguration.FormSendToEmail;
            if (String.IsNullOrEmpty(fromEmail) || String.IsNullOrEmpty(recipients))
            {
                return;
            }

            // Merge email addresses and subject with entered data
            fromEmail = Resolver.ResolveMacros(fromEmail);
            recipients = Resolver.ResolveMacros(recipients);

            if (!ValidationHelper.AreEmails(recipients))
            {
                throw new ArgumentException("At least one of the specified recipient's email addresses is not valid.");
            }

            // Set default email subject
            var subject = ResHelper.GetString("BizForm.MessageSubject") + " - " + FormConfiguration.FormDisplayName;
            if (!DataHelper.IsEmpty(FormConfiguration.FormEmailSubject))
            {
                // Set predefined email subject
                subject = Resolver.ResolveMacros(FormConfiguration.FormEmailSubject);
            }

            var template = FormConfiguration.FormEmailTemplate;
            if (String.IsNullOrEmpty(template))
            {
                // Prepare template with default layout
                var columnNames = FormDefinition.GetColumnNames().Where(col => FormData.ContainsColumn(col));
                template = CreateTemplateWithDefaultLayout(columnNames);
            }

            var body = ResolveEmailMessageText(template);

            // Set and send email message
            var em = new EmailMessage
            {
                EmailFormat = EmailFormatEnum.Html,
                From = fromEmail,
                Recipients = recipients,
                Subject = subject,
                Body = URLHelper.MakeLinksAbsolute(body)
            };

            // Attach uploaded documents if allowed
            if (FormConfiguration.FormEmailAttachUploadedDocs && (Uploads != null))
            {
                string folderPath = FormHelper.GetBizFormFilesFolderPath(SiteName);

                foreach (string file in Uploads)
                {
                    var fileNameString = Convert.ToString(FormData.GetValue(file));
                    if (!String.IsNullOrEmpty(fileNameString))
                    {
                        var filePath = folderPath + FormHelper.GetGuidFileName(fileNameString);
                        if (File.Exists(filePath))
                        {
                            var stream = FileStream.New(filePath, FileMode.Open, FileAccess.Read);

                            // Add attachment
                            var attachment = new Attachment(stream, FormHelper.GetOriginalFileName(fileNameString));
                            em.Attachments.Add(attachment);
                        }
                    }
                }
            }

            // Encode email body if required
            if (EncodeEmails)
            {
                em.Body = HTMLHelper.HTMLEncode(em.Body);
            }

            // Send email
            EmailSender.SendEmail(SiteName, em);
        }


        /// <summary>
        /// Sends confirmation email (autoresponder) based on on-line form configuration and collected data.
        /// </summary>
        public void SendConfirmationEmail()
        {
            if ((FormData == null) || (FormConfiguration == null) || (FormDefinition == null))
            {
                return;
            }

            var fromEmail = FormConfiguration.FormConfirmationSendFromEmail;
            var email = FormData.GetValue(FormConfiguration.FormConfirmationEmailField);
            var toEmail = ValidationHelper.GetString(email, "");
            if (String.IsNullOrEmpty(fromEmail) || String.IsNullOrEmpty(toEmail))
            {
                return;
            }

            // Merge email addresses and subject with entered data
            fromEmail = Resolver.ResolveMacros(fromEmail);
            toEmail = Resolver.ResolveMacros(toEmail);

            if (!ValidationHelper.IsEmail(toEmail))
            {
                throw new ArgumentException("Recipient's email address is not valid.");
            }

            // Set default email subject
            var subject = String.Format("{0} - {1}", ResHelper.GetString("BizForm.ConfirmationMessageSubject"), FormConfiguration.FormDisplayName);
            if (!DataHelper.IsEmpty(FormConfiguration.FormConfirmationEmailSubject))
            {
                // Set predefined email subject
                subject = Resolver.ResolveMacros(FormConfiguration.FormConfirmationEmailSubject);
            }

            // Prepare email body
            var body = ResolveEmailMessageText(FormConfiguration.FormConfirmationTemplate);

            // Set and send email message
            var em = new EmailMessage
            {
                EmailFormat = EmailFormatEnum.Html,
                From = fromEmail,
                Recipients = toEmail,
                Subject = subject,
                Body = URLHelper.MakeLinksAbsolute(body)
            };

            // Append attachments
            EmailHelper.ResolveMetaFileImages(em, FormConfiguration.FormID, BizFormInfo.OBJECT_TYPE, ObjectAttachmentsCategories.FORMLAYOUT);

            // Encode email body if required
            if (EncodeEmails)
            {
                em.Body = HTMLHelper.HTMLEncode(em.Body);
            }

            // Send the message
            EmailSender.SendEmail(SiteName, em);
        }


        /// <summary>
        /// Resolve possible field macros in email message text.
        /// </summary>
        /// <param name="emailLayout">Email layout template with macros to resolve</param>
        internal string ResolveEmailMessageText(string emailLayout)
        {
            if (String.IsNullOrEmpty(emailLayout))
            {
                return String.Empty;
            }

            emailLayout = Resolver.ResolveMacros(emailLayout);

            // Read form layout definition from the beginning to the end
            var matchColl = RegExEmailMacro.Matches(emailLayout);

            int actualPos = 0;
            var result = new StringBuilder();

            foreach (Match match in matchColl)
            {
                int newPos = match.Index;
                if (actualPos < newPos)
                {
                    // Append non-macro text
                    result.Append(emailLayout.Substring(actualPos, newPos - actualPos));
                }
                actualPos = newPos + match.Length;

                // Read macro value
                string macro = match.Value.Replace("$$", string.Empty);
                var colonIndex = macro.IndexOf(":", StringComparison.Ordinal);
                string ffType = macro.Substring(0, colonIndex);
                string ffName = macro.Substring(colonIndex + 1, macro.Length - colonIndex - 1);

                var ffi = FormDefinition.GetFormField(ffName);
                if (ffi != null)
                {
                    switch (ffType.ToLowerInvariant())
                    {
                        case "label":
                            // Add field caption
                            result.Append(ResHelper.LocalizeString(ffi.GetDisplayName(Resolver)));
                            break;

                        case "value":
                            // Add field value
                            if (FormData.ContainsColumn(ffName))
                            {
                                var value = GetFieldValueForMail(ffi);
                                result.Append(value);
                            }
                            break;
                    }
                }
            }

            if (actualPos < emailLayout.Length)
            {
                // Append non-macro text
                result.Append(emailLayout.Substring(actualPos, emailLayout.Length - actualPos));
            }

            return result.ToString();
        }


        /// <summary>
        /// Returns field value for mail message. Value is transformed according to field type.
        /// </summary>
        /// <param name="formFieldInfo">Form field info</param>
        private string GetFieldValueForMail(FormFieldInfo formFieldInfo)
        {
            var rawValue = FormData.GetValue(formFieldInfo.Name);
            var value = DataTypeManager.GetStringValue(TypeEnum.Field, formFieldInfo.DataType, rawValue, Culture);

            if (FormHelper.IsFieldOfType(formFieldInfo, FormFieldControlTypeEnum.UploadControl) || formFieldInfo.DataType == BizFormUploadFile.DATATYPE_FORMFILE)
            {
                if (SiteContext.CurrentSite.SiteIsContentOnly)
                {
                    value = FormHelper.GetOriginalFileName(value);
                }
                else
                {
                    value = GetBizFormAttachmentLink(value);
                }
            }

            return value;
        }


        /// <summary>
        /// Returns html code of link to bizform attached file.
        /// </summary>
        /// <param name="fileNameString">BizForm file name - guid + extension</param>
        private string GetBizFormAttachmentLink(string fileNameString)
        {
            if (!String.IsNullOrEmpty(fileNameString))
            {
                return $"<a href=\"~/CMSPages/GetBizFormFile.aspx?filename={FormHelper.GetGuidFileName(fileNameString)}&sitename={SiteName}\">{FormHelper.GetOriginalFileName(fileNameString)}</a>";
            }

            return String.Empty;
        }


        /// <summary>
        /// Creates email body template with default layout based on form fields.
        /// </summary>
        /// <param name="columnNames">Form fields</param>
        private string CreateTemplateWithDefaultLayout(IEnumerable<string> columnNames)
        {
            var template = new StringBuilder();

            foreach (string column in columnNames)
            {
                template.AppendFormat("$$label:{0}$$:      $$value:{0}$$<br /><br />", column);
            }

            return template.ToString();
        }
    }
}
