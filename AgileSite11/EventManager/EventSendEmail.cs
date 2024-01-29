using System;
using System.Data;
using System.Threading;

using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.EventManager
{
    /// <summary>
    /// Thread e-mail sender for event manager.
    /// </summary>
    public class EventSendEmail
    {
        private int eventId = 0;
        private string siteName = String.Empty;
        private string emailText = String.Empty;
        private string subject = String.Empty;
        private string senderName = String.Empty;
        private string senderEmail = String.Empty;


        /// <summary>
        /// Sends email to all attandees.
        /// </summary>
        /// <param name="eventId">Event id</param>
        /// <param name="siteName">Site name</param>
        /// <param name="subject">Subject</param>
        /// <param name="emailText">E-mail body</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender e-mail</param>
        public EventSendEmail(int eventId, string siteName, string subject, string emailText, string senderName, string senderEmail)
        {
            this.eventId = eventId;
            this.siteName = siteName;
            this.emailText = emailText;
            this.senderName = senderName;
            this.senderEmail = senderEmail;
            this.subject = subject;

            ThreadStart threadStartObj = new ThreadStart(Run);
            Thread emailSend = new Thread(threadStartObj);
            emailSend.Start();
        }


        /// <summary>
        /// Sends emails to all attendees.
        /// </summary>
        public void Run()
        {
            try
            {
                // Get attendees info
                DataSet dsAtendees = EventAttendeeInfoProvider.GetEventAttendees(eventId);
                if (!DataHelper.DataSourceIsEmpty(dsAtendees))
                {
                    // Init resolver
                    MacroResolver resolver = MacroResolver.GetInstance();
                    // Data rows for macro resolver
                    DataRow[] rows = new DataRow[1];

                    // Send e-mail one by one to all attendees
                    foreach (DataRow dr in dsAtendees.Tables[0].Rows)
                    {
                        // Initialize resolver
                        rows[0] = dr;
                        resolver.SetAnonymousSourceData(rows);
                        resolver.SetNamedSourceData("Attendee", new EventAttendeeInfo(dr));

                        // Set e-mail properties and send e-mail
                        EmailMessage em = new EmailMessage();
                        em.EmailFormat = EmailFormatEnum.Html;

                        if (!String.IsNullOrEmpty(senderName) && !String.IsNullOrEmpty(senderEmail) && (senderEmail.Length + 3 + senderName.Length <= 250))
                        {
                            em.From = senderName + " <" + senderEmail + ">";
                        }
                        else
                        {
                            em.From = senderEmail;
                        }

                        em.Recipients = ValidationHelper.GetString(dr["AttendeeEmail"], null);
                        em.Subject = resolver.ResolveMacros(subject);
                        em.Body = resolver.ResolveMacros(emailText);

                        EmailSender.SendEmail(siteName, em);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("E", "SENDEMAIL", ex);
            }
        }
    }
}