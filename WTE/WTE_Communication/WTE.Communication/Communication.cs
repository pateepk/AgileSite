using System;
using System.Collections.Concurrent;
using System.Threading;
using WTE.Communication.CommunicationWebReference;

/*! \mainpage WTE.Communication API
 *
 * \section intro_sec Introduction
 *
 * This API is used by applications to communicate with the WTE communication web service
 *
 * \section install_sec Installation
 *
 * add a reference to the WTE.Communication.dll to your project
 *
 * \section example_sec Examples
 *
 * The CommunicationTest.zip file contains an example of using the API
 */

namespace WTE.Communication
{
    /// <summary>
    /// Main class for initiating Communication Library interaction
    /// </summary>
    public class Communication
    {
        public enum FaxFormat { HTML, Text };

        private static Thread m_emailQueueThread = null;

        /// <summary>
        /// Manually start queue handling. This is automatically called when SendAsynchronousEmail method is called.
        /// </summary>
        public static void StartQueueHandling()
        {
            if (m_emailQueueThread == null || !m_emailQueueThread.IsAlive)
            {
                CommunicationQueueHandler eqh = new CommunicationQueueHandler();
                CommunicationQueueHandler.Enabled = true;
                m_emailQueueThread = new Thread(new ThreadStart(eqh.ProcessQueue));
                m_emailQueueThread.Name = "EmailQueueHandler";
                m_emailQueueThread.IsBackground = true;
                m_emailQueueThread.Start();
            }
        }

        private static CommunicationWebReference.CommunicationAppWebService
            GetCommunicationWebReference(ConnectionInfo p_connectionInfo)
        {
            //get web service
            CommunicationWebReference.CommunicationAppWebService cwr = new CommunicationWebReference.CommunicationAppWebService();

            //set gateway URL
            cwr.Url = p_connectionInfo.URL;

            //set auth information
            ServiceAuthHeader m_serviceAuthHeader = new ServiceAuthHeader();
            m_serviceAuthHeader.Username = p_connectionInfo.login;
            m_serviceAuthHeader.Password = p_connectionInfo.password;
            cwr.ServiceAuthHeaderValue = m_serviceAuthHeader;

            return cwr;
        }

        /// <summary>
        /// Verify that the Communication Service is running
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <returns>health check response</returns>
        public static HealthCheckResponse HealthCheck(ConnectionInfo p_connectionInfo)
        {
            HealthCheckResponse hcr = new HealthCheckResponse();
            try
            {
                //get web service
                CommunicationWebReference.CommunicationAppWebService cwr =
                    GetCommunicationWebReference(p_connectionInfo);

                CommunicationWebReference.ResponseInformation ri =
                    cwr.HealthCheck();
                //0 - success
                //1 - success with warnings
                //2 - error
                hcr.Code = (HealthCheckResponse.ResponseCodeType)ri.ResponseCode;
                hcr.Message = ri.ResponseMessage;
            }
            catch (Exception ex)
            {
                hcr.Code = HealthCheckResponse.ResponseCodeType.Error; //error
                hcr.Message = "Unable to check communication service. Reason: " + ex.Message;
            }
            return hcr;
        }

        #region communication queue

        public class CommunicationQueueItem
        {
            public enum CommunicationQueueItemType { None, Email, Fax, SMS };

            public CommunicationQueueItemType m_type = CommunicationQueueItemType.None;

            public CommunicationQueueItemType ItemType
            {
                get { return m_type; }
                set { m_type = value; }
            }
        }

        public class CommunicationQueueEmailItem : CommunicationQueueItem
        {
            private Email m_email = null;

            public Email EmailItem
            {
                get { return m_email; }
                set { m_email = value; }
            }

            public CommunicationQueueEmailItem()
            {
                ItemType = CommunicationQueueItemType.Email;
            }
        }

        public class CommunicationQueueFaxItem : CommunicationQueueItem
        {
            private Fax m_fax = null;

            public Fax FaxItem
            {
                get { return m_fax; }
                set { m_fax = value; }
            }

            public CommunicationQueueFaxItem()
            {
                ItemType = CommunicationQueueItemType.Fax;
            }
        }

        public class CommunicationQueueSMSItem : CommunicationQueueItem
        {
            private SMS m_sms = null;

            public SMS SMSItem
            {
                get { return m_sms; }
                set { m_sms = value; }
            }

            public CommunicationQueueSMSItem()
            {
                ItemType = CommunicationQueueItemType.SMS;
            }
        }

        /// <summary>
        /// Email processing queue
        /// </summary>
        public class CommunicationQueueHandler
        {
            private static bool m_enabled = true;

            private static ConcurrentQueue<CommunicationQueueItem> m_emailQueue = new ConcurrentQueue<CommunicationQueueItem>();

            /// <summary>
            /// The queue
            /// </summary>
            public static ConcurrentQueue<CommunicationQueueItem> CommunicationQueue
            {
                get
                {
                    return m_emailQueue;
                }

                set
                {
                    m_emailQueue = value;
                }
            }

            /// <summary>
            /// Add email to queue
            /// </summary>
            /// <param name="p_email"></param>
            public static void Enqueue(CommunicationQueueItem p_communicationQueueItem)
            {
                CommunicationQueue.Enqueue(p_communicationQueueItem);
                lock (CommunicationQueue)
                {
                    Monitor.Pulse(CommunicationQueue);
                }
            }

            /// <summary>
            /// Inidicates if queue should be processing
            /// </summary>
            public static bool Enabled
            {
                get { return m_enabled; }
                set
                {
                    lock (CommunicationQueue)
                    {
                        m_enabled = value;
                        Monitor.Pulse(CommunicationQueue);
                    }
                }
            }

            /// <summary>
            /// start process of checking queue for emails and sending them to communication server
            /// after email is sent to communication server the AsyncResponseHandler is called if attached to email
            /// </summary>
            public void ProcessQueue()
            {
                while (Enabled)
                {
                    CommunicationQueueItem communicationQueueItem = null;
                    while (CommunicationQueue.TryDequeue(out communicationQueueItem))
                    {
                        switch (communicationQueueItem.ItemType)
                        {
                            case CommunicationQueueItem.CommunicationQueueItemType.Email:
                                Email email = ((CommunicationQueueEmailItem)communicationQueueItem).EmailItem;
                                EmailResponse ret = SendEmail(email.ConnectionInfo, email.EmailAddresses, email.Subject, email.Body, email.IsHtmlEmail, email.Attachments);
                                ret.AdditionalData = email.AdditionalData;
                                if (email.AsyncResponseHandler != null)
                                {
                                    email.AsyncResponseHandler.HandleResponse(ret);
                                }
                                break;

                            case CommunicationQueueItem.CommunicationQueueItemType.Fax:
                                Fax fax = ((CommunicationQueueFaxItem)communicationQueueItem).FaxItem;
                                FaxResponse faxResponse = new FaxResponse();
                                try
                                {
                                    CommunicationWebReference.CommunicationAppWebService cwr = GetCommunicationWebReference(fax.ConnectionInfo);
                                    ResponseInformation faxRet = cwr.SendFormattedFaxMessage(fax.FaxNumber, fax.FaxFrom, fax.Message, fax.Format);
                                    faxResponse.Code = (FaxResponse.ResponseCodeType)faxRet.ResponseCode;
                                    faxResponse.Message = faxRet.ResponseMessage;
                                }
                                catch (Exception e)
                                {
                                    faxResponse.Code = CommunicationResponse.ResponseCodeType.Error;
                                    faxResponse.Message = e.Message;
                                }

                                if (fax.AsyncResponseHandler != null)
                                {
                                    fax.AsyncResponseHandler.HandleResponse(faxResponse);
                                }
                                break;

                            case CommunicationQueueItem.CommunicationQueueItemType.SMS:
                                SMS sms = ((CommunicationQueueSMSItem)communicationQueueItem).SMSItem;
                                SMSResponse smsResponse = new SMSResponse();
                                try
                                {
                                    CommunicationWebReference.CommunicationAppWebService cwr = GetCommunicationWebReference(sms.ConnectionInfo);
                                    ResponseInformation smsRet = cwr.SendSMSMessage(sms.ToPhoneNumber, sms.Message);
                                    smsResponse.Code = (SMSResponse.ResponseCodeType)smsRet.ResponseCode;
                                    smsResponse.Message = smsRet.ResponseMessage;
                                }
                                catch (Exception e)
                                {
                                    smsResponse.Code = CommunicationResponse.ResponseCodeType.Error;
                                    smsResponse.Message = e.Message;
                                }

                                if (sms.AsyncResponseHandler != null)
                                {
                                    sms.AsyncResponseHandler.HandleResponse(smsResponse);
                                }
                                break;

                            default:
                                //log
                                break;
                        }
                    }

                    lock (CommunicationQueue)
                    {
                        try
                        {
                            // Waits for the Monitor.Pulse in Enqueue
                            Monitor.Wait(CommunicationQueue);
                        }
                        catch (SynchronizationLockException e)
                        {
                            //Console.WriteLine(e);
                        }
                        catch (ThreadInterruptedException e)
                        {
                            //Console.WriteLine(e);
                        }
                    }
                }
            }
        }

        #endregion communication queue

        #region e-mail

        private class AddressConversion
        {
            public static CommunicationWebReference.Address Convert(Address p_address)
            {
                WTE.Communication.CommunicationWebReference.Address ret =
                    new WTE.Communication.CommunicationWebReference.Address();
                ret.Email = p_address.EMail;
                ret.Name = p_address.Name;
                return ret;
            }

            public static CommunicationWebReference.Address[] Convert(Address[] p_address)
            {
                CommunicationWebReference.Address[] ret = new WTE.Communication.CommunicationWebReference.Address[] { };
                if (p_address != null && p_address.Length > 0)
                {
                    ret = new WTE.Communication.CommunicationWebReference.Address[p_address.Length];
                    for (int i = 0; i < p_address.Length; i++)
                    {
                        ret[i] = AddressConversion.Convert(p_address[i]);
                    }
                }

                return ret;
            }
        }

        private class AttachmentConversion
        {
            public static CommunicationWebReference.AttachmentType Convert(Attachment p_attachment)
            {
                WTE.Communication.CommunicationWebReference.AttachmentType ret =
                    new WTE.Communication.CommunicationWebReference.AttachmentType();
                ret.Filename = p_attachment.Filename;
                ret.Name = p_attachment.Name;
                ret.Encoding = (int)p_attachment.Encoding;
                ret.MimeFlags = (int)p_attachment.MimeFlags;
                ret.Data = p_attachment.Data;
                return ret;
            }

            public static CommunicationWebReference.AttachmentType[] Convert(Attachment[] p_attachments)
            {
                CommunicationWebReference.AttachmentType[] ret = new WTE.Communication.CommunicationWebReference.AttachmentType[] { };
                if (p_attachments != null && p_attachments.Length > 0)
                {
                    ret = new WTE.Communication.CommunicationWebReference.AttachmentType[p_attachments.Length];
                    for (int i = 0; i < p_attachments.Length; i++)
                    {
                        ret[i] = AttachmentConversion.Convert(p_attachments[i]);
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// send an email using the portal defined gateway
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <param name="p_addresses">the e-mail addresses</param>
        /// <param name="p_subject">the email subject</param>
        /// <param name="p_body">the email body</param>
        /// <param name="p_isHTMLEmail">true if p_body is an HTML formatted email</param>
        /// <returns>empty string if no error, else error</returns>
        public static EmailResponse SendEmail(ConnectionInfo p_connectionInfo, WTE.Communication.Email.Addresses p_addresses, String p_subject, String p_body, bool p_isHTMLEmail)
        {
            return SendEmail(p_connectionInfo, p_addresses, p_subject, p_body, p_isHTMLEmail, null);
        }

        /// <summary>
        /// send an email using the portal defined gateway
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <param name="p_addresses">the e-mail addresses</param>
        /// <param name="p_subject">the email subject</param>
        /// <param name="p_body">the email body</param>
        /// <param name="p_isHTMLEmail">true if p_body is an HTML formatted email</param>
        /// <param name="p_attachments">any attachments</param>
        /// <returns></returns>
        public static EmailResponse SendEmail(ConnectionInfo p_connectionInfo, WTE.Communication.Email.Addresses p_addresses,
            String p_subject, String p_body, bool p_isHTMLEmail, Attachment[] p_attachments)
        {
            EmailResponse ret = new EmailResponse();

            CommunicationWebReference.Addresses emailAddresses = new CommunicationWebReference.Addresses();

            emailAddresses.FromAddress = AddressConversion.Convert(p_addresses.FromAddress);

            emailAddresses.ToAddresses = new CommunicationWebReference.Address[] { };
            emailAddresses.CCAddresses = new CommunicationWebReference.Address[] { };
            emailAddresses.BCCAddresses = new CommunicationWebReference.Address[] { };
            CommunicationWebReference.AttachmentType[] attachments = new CommunicationWebReference.AttachmentType[] { };

            if (p_addresses.ToAddresses != null)
            {
                emailAddresses.ToAddresses = AddressConversion.Convert(p_addresses.ToAddresses);
            }
            if (p_addresses.CCAddresses != null)
            {
                emailAddresses.CCAddresses = AddressConversion.Convert(p_addresses.CCAddresses);
            }
            if (p_addresses.BCCAddresses != null)
            {
                emailAddresses.BCCAddresses = AddressConversion.Convert(p_addresses.BCCAddresses);
            }
            if (p_addresses.ReplyToAddress != null)
            {
                emailAddresses.ReplyToAddress = AddressConversion.Convert(p_addresses.ReplyToAddress);
            }

            if (p_attachments != null && p_attachments.Length > 0)
            {
                attachments = AttachmentConversion.Convert(p_attachments);
            }

            //get web service
            CommunicationWebReference.CommunicationAppWebService cwr =
                GetCommunicationWebReference(p_connectionInfo);

            try
            {
                //send email
                CommunicationWebReference.ResponseInformation ri =
                    cwr.SendEmailWithAttachment(emailAddresses, p_subject, p_body, p_isHTMLEmail, attachments);
                //0 - success
                //1 - success with warnings
                //2 - error
                ret.Code = (EmailResponse.ResponseCodeType)ri.ResponseCode;
                ret.Message = ri.ResponseMessage;
            }
            catch (Exception ex)
            {
                ret.Code = EmailResponse.ResponseCodeType.Error; //error
                ret.Message = "Unable to send e-mail. Reason: " + ex.Message;
            }

            return ret;
        }

        /// <summary>
        /// send an email using the portal defined gateway
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <param name="p_addresses">the e-mail addresses</param>
        /// <param name="p_subject">the email subject</param>
        /// <param name="p_body">the email body</param>
        /// <param name="p_isHTMLEmail">true if p_body is an HTML formatted email</param>
        public static void SendAsynchronousEmail(ConnectionInfo p_connectionInfo, WTE.Communication.Email.Addresses p_addresses,
            String p_subject, String p_body, bool p_isHTMLEmail)
        {
            SendAsynchronousEmail(p_connectionInfo, p_addresses, p_subject, p_body, p_isHTMLEmail, null, null);
        }

        /// <summary>
        /// send an email using the portal defined gateway
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <param name="p_toEmail">string array of each TO Email address to send to. can be null</param>
        /// <param name="p_fromEmail">the from email address</param>
        /// <param name="p_CC">array of CC email addresses. can be null</param>
        /// <param name="p_BCC">array of BCC email addresses. can be null</param>
        /// <param name="p_subject">the email subject</param>
        /// <param name="p_body">the email body</param>
        /// <param name="p_isHTMLEmail">true if p_body is an HTML formatted email</param>
        /// <param name="p_asynchronousResponseHandler">optional response handler. can be null</param>
        public static void SendAsynchronousEmail(ConnectionInfo p_connectionInfo, WTE.Communication.Email.Addresses p_addresses,
            String p_subject, String p_body, bool p_isHTMLEmail, Attachment[] p_attachments)
        {
            SendAsynchronousEmail(p_connectionInfo, p_addresses, p_subject, p_body, p_isHTMLEmail, p_attachments, null);
        }

        /// <summary>
        /// send an email using the portal defined gateway
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <param name="p_toEmail">string array of each TO Email address to send to. can be null</param>
        /// <param name="p_fromEmail">the from email address</param>
        /// <param name="p_CC">array of CC email addresses. can be null</param>
        /// <param name="p_BCC">array of BCC email addresses. can be null</param>
        /// <param name="p_subject">the email subject</param>
        /// <param name="p_body">the email body</param>
        /// <param name="p_isHTMLEmail">true if p_body is an HTML formatted email</param>
        /// <param name="p_asynchronousResponseHandler">optional response handler. can be null</param>
        public static void SendAsynchronousEmail(ConnectionInfo p_connectionInfo, WTE.Communication.Email.Addresses p_addresses,
            String p_subject, String p_body, bool p_isHTMLEmail, Attachment[] p_attachments, AsynchronousResponseHandler p_asynchronousResponseHandler)
        {
            SendAsynchronousEmail(p_connectionInfo, p_addresses, p_subject, p_body, p_isHTMLEmail, p_attachments, p_asynchronousResponseHandler, null);
        }

        /// <summary>
        /// send an email using the portal defined gateway
        /// </summary>
        /// <param name="p_connectionInfo">Connection Information</param>
        /// <param name="p_toEmail">string array of each TO Email address to send to. can be null</param>
        /// <param name="p_fromEmail">the from email address</param>
        /// <param name="p_CC">array of CC email addresses. can be null</param>
        /// <param name="p_BCC">array of BCC email addresses. can be null</param>
        /// <param name="p_subject">the email subject</param>
        /// <param name="p_body">the email body</param>
        /// <param name="p_isHTMLEmail">true if p_body is an HTML formatted email</param>
        /// <param name="p_asynchronousResponseHandler">optional response handler. can be null</param>
        public static void SendAsynchronousEmail(ConnectionInfo p_connectionInfo, WTE.Communication.Email.Addresses p_addresses,
            String p_subject, String p_body, bool p_isHTMLEmail, Attachment[] p_attachments, AsynchronousResponseHandler p_asynchronousResponseHandler, Object p_additionalData)
        {
            //make sure queue handler is running
            StartQueueHandling();

            CommunicationQueueEmailItem queueEmailItem = new CommunicationQueueEmailItem();
            Email emailItem = new Email(p_connectionInfo, p_addresses, p_subject, p_body, p_isHTMLEmail, p_attachments, p_asynchronousResponseHandler);
            emailItem.AdditionalData = p_additionalData;
            queueEmailItem.EmailItem = emailItem;
            CommunicationQueueHandler.Enqueue(queueEmailItem);
        }

        #endregion e-mail

        #region fax

        /// <summary>
        /// Send a Fax
        /// </summary>
        /// <param name="p_connectionInfo"></param>
        /// <param name="p_faxNumber"></param>
        /// <param name="p_faxFrom"></param>
        /// <param name="p_message"></param>
        /// <param name="p_format"></param>
        /// <returns></returns>
        public static ResponseInformation SendFormattedFaxMessage(ConnectionInfo p_connectionInfo, string p_faxNumber, string p_faxFrom, string p_message, string p_format)
        {
            //get web service
            CommunicationWebReference.CommunicationAppWebService cwr = GetCommunicationWebReference(p_connectionInfo);

            ResponseInformation resp = new ResponseInformation();
            try
            {
                resp = cwr.SendFormattedFaxMessage(p_faxNumber, p_faxFrom, p_message, p_format);
            }
            catch (Exception e)
            {
                resp.ResponseCode = (int)CommunicationResponse.ResponseCodeType.Error;
                resp.ResponseMessage = e.Message;
            }
            return resp;
        }

        public static void SendAsynchronousFax(ConnectionInfo p_connectionInfo, string p_faxNumber, string p_faxFrom, string p_message, FaxFormat p_format, AsynchronousResponseHandler p_asynchronousResponseHandler)
        {
            string format = "HTML";
            if (p_format == FaxFormat.Text)
            {
                format = "text";
            }
            SendAsynchronousFax(p_connectionInfo, p_faxNumber, p_faxFrom, p_message, format, p_asynchronousResponseHandler);
        }

        public static void SendAsynchronousFax(ConnectionInfo p_connectionInfo, string p_faxNumber, string p_faxFrom, string p_message, string p_format, AsynchronousResponseHandler p_asynchronousResponseHandler)
        {
            //make sure queue handler is running
            StartQueueHandling();

            CommunicationQueueFaxItem queueFaxItem = new CommunicationQueueFaxItem();
            queueFaxItem.FaxItem = new Fax(p_connectionInfo, p_faxNumber, p_faxFrom, p_message, p_format, p_asynchronousResponseHandler);
            CommunicationQueueHandler.Enqueue(queueFaxItem);
        }

        #endregion fax

        #region sms

        /// <summary>
        /// Send a SMS/Text message
        /// </summary>
        /// <param name="p_connectionInfo"></param>
        /// <param name="p_toPhoneNumber"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public static ResponseInformation SendSMSMessage(ConnectionInfo p_connectionInfo, string p_toPhoneNumber, string p_message)
        {
            //get web service
            CommunicationWebReference.CommunicationAppWebService cwr = GetCommunicationWebReference(p_connectionInfo);

            ResponseInformation resp = new ResponseInformation();
            try
            {
                resp = cwr.SendSMSMessage(p_toPhoneNumber, p_message);
            }
            catch (Exception e)
            {
                resp.ResponseCode = (int)CommunicationResponse.ResponseCodeType.Error;
                resp.ResponseMessage = e.Message;
            }
            return resp;
        }

        public static void SendAsynchronousSMS(ConnectionInfo p_connectionInfo, string p_toPhoneNumber, string p_message, AsynchronousResponseHandler p_asynchronousResponseHandler)
        {
            //make sure queue handler is running
            StartQueueHandling();

            CommunicationQueueSMSItem queueSMSItem = new CommunicationQueueSMSItem();
            queueSMSItem.SMSItem = new SMS(p_connectionInfo, p_toPhoneNumber, p_message, p_asynchronousResponseHandler);
            CommunicationQueueHandler.Enqueue(queueSMSItem);
        }

        #endregion sms
    }
}