using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net.Mail;
    using System.Net;
    using WTE.Communication;

    /// <summary>
    /// Description: 
    /// </summary>
    public class EmailManager
    {

        private SmtpClient _mail = null;

        private int _port = 25;
        private bool _isLog = false;
        private string _emailFromGeneric = String.Empty;
        private string _emailFromGenericDisplayName = String.Empty;
        private List<int> _attachmentFileID = new List<int>();
        private List<string> _attachmentFilePath = new List<string>();
        private List<string> _emailAddressesCC = new List<string>();
        private List<string> _emailAddressesBCC = new List<string>();

        public EmailManager()
        {
            _port = Utils.getAppSettings(SV.AppSettings.EmailServerPort).ToInt();
            _isLog = Utils.getAppSettings(SV.AppSettings.IsOutgoingEmailLog).ToBool();
            _emailFromGeneric = Utils.getAppSettings(SV.AppSettings.OutgoingEmailFrom).ToString();
            _emailFromGenericDisplayName = Utils.getAppSettings(SV.AppSettings.OutgoingEmailFromDisplayName).ToString();
            if (_emailFromGenericDisplayName.Length > 0)
            {
                _emailFromGeneric = String.Format("\"{0}\" <{1}>", _emailFromGenericDisplayName, _emailFromGeneric);
            }
        }

        public void AddAttachmentFileID(int UploadFileID)
        {
            _attachmentFileID.Add(UploadFileID);
        }

        public void AddAttachmentFilePath(string attachmentFilePath)
        {
            _attachmentFilePath.Add(attachmentFilePath);
        }

        public void AddEmailAddressCC(string emailAddressCC)
        {
            _emailAddressesCC.Add(emailAddressCC);
        }

        public void AddEmailAddressBCC(string emailAddressBCC)
        {
            _emailAddressesBCC.Add(emailAddressBCC);
        }

        public void TestEmailToDeveloper()
        {
            string comURL = Utils.getAppSettings(SV.AppSettings.CommunicationWSURL).ToString();
            string comUser = Utils.getAppSettings(SV.AppSettings.CommunicationWSUser).ToString();
            string comPassword = Utils.getAppSettings(SV.AppSettings.CommunicationWSPassword).ToString();
            string devEmail = Utils.getAppSettings(SV.AppSettings.DevelopmentEmail).ToString();

            WTE.Communication.ConnectionInfo ci = new ConnectionInfo(comURL, comUser, comPassword);
            WTE.Communication.Email.Addresses address = new Email.Addresses();
            address.FromAddress = new WTE.Communication.Address(devEmail, "Developer");
            List<Address> la = new List<Address>();
            la.Add(new Address(devEmail, "Developer"));
            address.ToAddresses = la.ToArray();
            string body = String.Format("From Machine: {0}\r\nURL:{1}", Environment.MachineName, comURL);
            WTE.Communication.Communication.SendAsynchronousEmail(ci, address, "Subject: Test Email To Developer", body, false);
        }

        // -1 - email was not successfully 
        public Int64 SendEmail(MailMessage message)
        {
            Int64 emailLogID = 0;
            bool result = true;
            bool IsEmailManagerUsingWTECommunication = Utils.getAppSettings(SV.AppSettings.IsEmailManagerUsingWTECommunication).ToBool();

            if (IsEmailManagerUsingWTECommunication)
            {
                string comURL = Utils.getAppSettings(SV.AppSettings.CommunicationWSURL).ToString();
                string comUser = Utils.getAppSettings(SV.AppSettings.CommunicationWSUser).ToString();
                string comPassword = Utils.getAppSettings(SV.AppSettings.CommunicationWSPassword).ToString();
                string devEmail = Utils.getAppSettings(SV.AppSettings.DevelopmentEmail).ToString();
                 
                WTE.Communication.ConnectionInfo ci = new ConnectionInfo(comURL, comUser, comPassword);
                WTE.Communication.Email.Addresses address = new Email.Addresses();
                address.FromAddress = new WTE.Communication.Address(message.From.Address, message.From.DisplayName);
                List<Address> la = new List<Address>();
                foreach (var item in message.To)
                {
                    la.Add(new Address(item.Address, item.DisplayName));
                }

                address.ToAddresses = la.ToArray();
                WTE.Communication.Communication.SendAsynchronousEmail(ci, address, message.Subject, message.Body, false);

            }
            else
            {

                _mail = new SmtpClient(Utils.getAppSettings(SV.AppSettings.EmailServer).ToString());
                _mail.Port = Utils.getAppSettings(SV.AppSettings.EmailServerPort).ToInt();
                try
                {
                    _mail = new SmtpClient(Utils.getAppSettings(SV.AppSettings.EmailServer).ToString());
                    NetworkCredential Credentials = new NetworkCredential(Utils.getAppSettings(SV.AppSettings.Email_NetworkCredential_Username).ToString(), Utils.getAppSettings(SV.AppSettings.Email_NetworkCredential_Password).ToString());
                    _mail.Credentials = Credentials;
                    _mail.Send(message);
                }
                catch (Exception ex)
                {
                    ErrorManager.logError(String.Format(SV.ErrorMessages.ErrorSendingEmail, message.To), ex, false);
                }

            }

            if (_isLog)
            {
                try
                {
                    DRspEmailLog_Insert log = SQLData.spEmailLog_Insert(
                          message.From.ToString()
                        , message.To.ToString()
                        , message.Subject
                        , message.Body
                        , result
                        );

                    emailLogID = log.EmailLogID(0);
                }
                catch (Exception ex)
                {
                    ErrorManager.logError(String.Format(SV.ErrorMessages.ErrorLoggingEmail, message.To), ex);
                }
            }
            return result ? emailLogID : 0;
        }

        /// <summary>
        /// Sending email 
        /// </summary>
        /// <param name="fromEmail"></param>
        /// <param name="toEmail"></param>
        /// <param name="subjectEmail"></param>
        /// <param name="bodyEmail"></param>
        /// <returns></returns>
        public Int64 SendEmail(string fromEmail, string toEmail, string subjectEmail, string bodyEmail)
        {
            EmailContentModification(ref fromEmail, ref toEmail, ref subjectEmail, ref bodyEmail);
            MailMessage message = new MailMessage(fromEmail, toEmail, subjectEmail, bodyEmail);

            if (_emailAddressesCC.Count > 0)
            {
                foreach (string emailAddressCC in _emailAddressesCC)
                {
                    message.CC.Add(emailAddressCC);
                }
            }

            if (_emailAddressesBCC.Count > 0)
            {
                foreach (string emailAddressBCC in _emailAddressesBCC)
                {
                    message.Bcc.Add(emailAddressBCC);
                }
            }

            if (_attachmentFilePath.Count > 0)
            {
                for (int i = 0; i < _attachmentFilePath.Count; i++)
                {
                    message.Attachments.Add(new System.Net.Mail.Attachment(Utils.getMapFile(_attachmentFilePath[i])));
                }
            }

            return SendEmail(message);
        }

        private void EmailContentModification(ref string fromEmail, ref string toEmail, ref string subjectEmail, ref string bodyEmail)
        {
            // do nothing on production
            if (!(AppSettings.EnvironmentCode == environmentCodes.Production))
            {
                // modifiy the subject
                subjectEmail = String.Format("[{0} - {1}] {2}",
                     Utils.getAppSettings(SV.AppSettings.ApplicationName).ToString()
                    , AppSettings.EnvironmentCode.ToString()
                    , subjectEmail);

                // modify to address and put original in the subject
                subjectEmail = String.Format("To {0} {1}", toEmail, subjectEmail);
                toEmail = Utils.getAppSettings(SV.AppSettings.DevelopmentEmail).ToString();

                // show CC and BCC at bottom of email
                if (_emailAddressesCC.Count > 0)
                {
                    bodyEmail += "\r\n\r\n";
                    foreach (string emailAddressCC in _emailAddressesCC)
                    {
                        bodyEmail += "\r\n CC:" + emailAddressCC;
                    }
                }
                if (_emailAddressesBCC.Count > 0)
                {
                    bodyEmail += "\r\n";
                    foreach (string emailAddressBCC in _emailAddressesBCC)
                    {
                        bodyEmail += "\r\n BCC:" + emailAddressBCC;
                    }
                }
                _emailAddressesCC.Clear();
                _emailAddressesBCC.Clear();
            }
        }

        /// <summary>
        /// Sending email using generic email from address
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="subjectEmail"></param>
        /// <param name="bodyEmail"></param>
        /// <returns></returns>
        public Int64 SendEmail(string toEmail, string subjectEmail, string bodyEmail)
        {
            return SendEmail(_emailFromGeneric, toEmail, subjectEmail, bodyEmail);
        }

        public Int64 SendEmailToWebsiteAdministrator(string subjectEmail, string bodyEmail)
        {
            return SendEmail(_emailFromGeneric, _emailFromGeneric, subjectEmail, bodyEmail);
        }

        public Int64 SendEmail(List<roleIDs> roleid, string subjectEmail, string bodyEmail)
        {
            Int64 emailLogID = 0;
            string roleids = String.Empty;
            for (int i = 0; i < roleid.Count; i++)
            {
                roleids += ((int)roleid[i]).ToString() + ",";
            }

            DRspUsers_GetByRole users = SQLData.spUsers_GetByRole(roleids);
            if (!users.isError)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (!String.IsNullOrEmpty(users.Email(i)))
                    {
                        emailLogID = SendEmail(users.Email(i), subjectEmail, bodyEmail);
                    }
                }
            }
            return emailLogID;
        }

    }

}
