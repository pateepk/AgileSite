using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Threading;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Extensible provider for basic e-mail capabilities that is used internally in the e-mail engine.
    /// </summary>
    /// <remarks>
    /// This provider does not offer any methods that can be called directly.
    /// To send e-mails using API, use methods from <see cref="EmailSender" /> class.
    /// Asynchronous send methods of this provider cannot be called from the main UI thread.
    /// </remarks>
    public class EmailProvider : AbstractBaseProvider<EmailProvider>
    {
        #region "Events"

        /// <summary>
        /// Occurs when the send operation finishes.
        /// </summary>
        public static event SendCompletedEventHandler SendCompleted;

        #endregion


        #region "Constants"

        /// <summary>
        /// Specifies amount of seconds for sending time-out (5minutes by default).
        /// </summary>
        private const int SEND_TIMEOUT = 300000;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sends an e-mail through the SMTP server.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="message">E-mail message</param>
        /// <param name="smtpServer">SMTP server</param>        
        /// <remarks>
        /// This method is used to test if the SMTP server is available and should not be used
        /// for any other purpose.
        /// </remarks>
        internal static void SendEmail(string siteName, MailMessage message, SMTPServerInfo smtpServer)
        {
            ProviderObject.SendEmailInternal(siteName, message, smtpServer);
        }


        /// <summary>
        /// Asynchronously sends an e-mail through the SMTP server.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="message">E-mail message</param>
        /// <param name="smtpServer">SMTP server</param>
        /// <param name="emailToken">E-mail token that represents the message being sent</param>
        /// <remarks>
        /// This method is used to send individual messages from the e-mail queue.
        /// Asynchronous send method cannot be called from the main UI thread.
        /// </remarks>
        internal static void SendEmailAsync(string siteName, MailMessage message, SMTPServerInfo smtpServer, EmailToken emailToken)
        {
            ProviderObject.SendEmailAsyncInternal(siteName, message, smtpServer, emailToken);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Internal method that sends an e-mail through the SMTP server.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="message">E-mail message</param>
        /// <param name="smtpServer">SMTP server</param>        
        protected virtual void SendEmailInternal(string siteName, MailMessage message, SMTPServerInfo smtpServer)
        {
            SetEncoding(siteName, message);

            using (SmtpClient client = GetSMTPClient(smtpServer))
            {
                client.Send(message);
            }
        }


        /// <summary>
        /// Internal method that asynchronously sends an e-mail through the SMTP server.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="message">E-mail message</param>
        /// <param name="smtpServer">SMTP server</param>
        /// <param name="emailToken">E-mail token that represents the message being sent</param>
        protected virtual void SendEmailAsyncInternal(string siteName, MailMessage message, SMTPServerInfo smtpServer, EmailToken emailToken)
        {
            SetEncoding(siteName, message);

            SmtpClient client = GetSMTPClient(smtpServer);
            client.SendCompleted += new SendCompletedEventHandler(CMSThread.Wrap<object, AsyncCompletedEventArgs>(OnSendCompleted));

            try
            {
                // Prepare the sending action
                Action action = () =>
                {
                    try
                    {
                        client.SendAsync(message, emailToken);
                    }
                    catch (Exception ex)
                    {
                        ThreadPool.QueueUserWorkItem(userData =>
                        {
                            // Allow empty context because when called this we are in anonymous thread
                            CMSThread.AllowEmptyContext();
                            OnSendCompleted(new AsyncCompletedEventArgs(ex, false, emailToken));
                        });
                    }
                };

                // Try to send the message, finish the sending after 5minutes in case of no response from SMTP server
                CallWithTimeout(action);
            }
            catch (TimeoutException ex)
            {
                EventLogProvider.LogException("EmailEngine", "EmailProvider", ex);

                // Cancel async sending (OnSendCompleted is called automatically)
                client.SendAsyncCancel();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("EmailEngine", "EmailProvider", ex);
                
                // Call OnSendCompleted to proceed sending process
                ThreadPool.QueueUserWorkItem(userData => OnSendCompleted(new AsyncCompletedEventArgs(ex, false, emailToken)));
            }
        }


        /// <summary>
        /// Sends mail and waits for callback. Throws timeout exception if sending does not finish in 5minutes.
        /// </summary>
        /// <param name="action">Action to be called</param>
        private static void CallWithTimeout(Action action)
        {
            EventWaitHandle waitHandle = new ManualResetEvent(false);
            // Main blocked thread is released in the action callback
            AsyncCallback callback = ar => waitHandle.Set();
            // Start the sending action
            action.BeginInvoke(callback, null);

            // Block the current thread until it's allowed to proceed from callback (successful sending) or by time-out after 5minutes (no response from smtp server)
            if (!waitHandle.WaitOne(SEND_TIMEOUT))
            {
                throw new TimeoutException("Failed to complete in the timeout specified.");
            }
        }


        /// <summary>
        /// Raises the SendCompleted event after the send is completed.
        /// </summary>
        private void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SmtpClient client = ((SmtpClient)sender);
            if ((EmailHelper.SendLimit > 0) && (client.DeliveryMethod == SmtpDeliveryMethod.Network))
            {
                // Get email token and update send attempts of the server
                EmailToken sentItem = (EmailToken)e.UserState;
                sentItem.SMTPServer.SendAttempts++;

                if (sentItem.SMTPServer.SendAttempts == EmailHelper.SendLimit)
                {
                    // Close connection to the SMTP server and reset counter
                    client.Dispose();

                    sentItem.SMTPServer.SendAttempts = 0;
                }
            }

            OnSendCompleted(e);
        }


        /// <summary>
        /// Raises the SendCompleted event after the send is completed.
        /// </summary>
        /// <param name="e">Provides data for async SendCompleted event</param>
        protected virtual void OnSendCompleted(AsyncCompletedEventArgs e)
        {
            if (SendCompleted != null)
            {
                SendCompleted(this, e);
            }
        }


        /// <summary>
        /// Sets message's body and subject encoding.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <param name="message">The message</param>
        private static void SetEncoding(string siteName, MailMessage message)
        {
            if (message != null)
            {
                // Set e-mail encoding
                message.BodyEncoding = message.SubjectEncoding = EmailHelper.GetEncoding(siteName);
            }
        }


        /// <summary>
        /// Gets a new SMTP client that will be used to send the e-mail message.
        /// </summary>
        /// <param name="smtpServer">SMTP server to send an e-mail message through</param>
        /// <returns>An instance of SMTP client</returns>
        private static SmtpClient GetSMTPClient(SMTPServerInfo smtpServer)
        {
            SmtpClient client;
            string serverName = smtpServer.ServerName;

            // Try to parse the server name into host and port
            int port = 0;
            int portIndex = serverName.LastIndexOf(":", StringComparison.Ordinal);
            if (portIndex >= 0)
            {
                port = ValidationHelper.GetInteger(serverName.Substring(portIndex + 1), 0);
            }

            if (port > 0)
            {
                serverName = serverName.Substring(0, portIndex);
                client = new SmtpClient(serverName, port);
            }
            else
            {
                client = new SmtpClient(serverName);
            }

            client.EnableSsl = smtpServer.ServerUseSSL;

            if (!string.IsNullOrEmpty(smtpServer.ServerUserName))
            {
                NetworkCredential credentials = new NetworkCredential(smtpServer.ServerUserName, EncryptionHelper.DecryptData(smtpServer.ServerPassword));
                client.Credentials = credentials;
            }
            else
            {
                // Use default credentials of the currently logged on user if no username was provided                
                client.UseDefaultCredentials = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEmailUseDefaultCredentials"], false);
            }

            client.DeliveryMethod = GetDeliveryMethod(smtpServer.ServerDeliveryMethod);
            client.PickupDirectoryLocation = StorageHelper.GetFullFilePhysicalPath(smtpServer.ServerPickupDirectory);

            return client;
        }


        /// <summary>
        /// Returns <see cref="SmtpDeliveryMethod"/> based on <see cref="SMTPServerDeliveryEnum"/> parameter.
        /// </summary>
        /// <param name="deliveryEnum">SMTP server delivery method</param>
        private static SmtpDeliveryMethod GetDeliveryMethod(SMTPServerDeliveryEnum deliveryEnum)
        {
            switch (deliveryEnum)
            {
                // Pickup directory
                case SMTPServerDeliveryEnum.SpecifiedPickupDirectory:
                    return SmtpDeliveryMethod.SpecifiedPickupDirectory;

                // Pickup directory from IIS
                case SMTPServerDeliveryEnum.PickupDirectoryFromIis:
                    return SmtpDeliveryMethod.PickupDirectoryFromIis;

                // Network
                default:
                    return SmtpDeliveryMethod.Network;
            }
        }

        #endregion
    }
}