using System;

using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds Event log API examples
    /// </summary>
    /// <pageTitle>Event log</pageTitle>
    internal class EventLog
    {
        /// <heading>Logging events</heading>
        private void LogEvent()
        {
            // Logs an information event into the event log
            EventLogProvider.LogEvent(EventType.INFORMATION, "API Example", "APIEXAMPLE", eventDescription: "Testing event logging.");
        }


        /// <heading>Working with events (sending events by email)</heading>
        private void SendEventsByEmail()
        {
            // Gets all error events logged in the past day 
            var errors = EventLogProvider.GetEvents()
                                            .WhereEquals("EventType", "E")                                            
                                            .WhereGreaterThan("EventTime", DateTime.Now.Subtract(TimeSpan.FromDays(1)));

            if (errors.Count > 0)
            {
                // Creates the email message
                EmailMessage msg = new EmailMessage();

                msg.From = "system@localhost.local";
                msg.Recipients = "admin@localhost.local";
                msg.Subject = "Kentico Errors (" + errors.Count + ")";
                msg.Body = "<html><body><ul>";
                
                // Creates a list of the errors
                foreach (EventLogInfo errorEvent in errors)
                {
                    msg.Body += String.Format("<li>{0} - {1} - {2}</li>", errorEvent.EventType, errorEvent.EventCode, errorEvent.EventDescription.Substring(0, 100));
                }

                msg.Body += "</ul></body></html>";

                // Sends out the email message
                EmailSender.SendEmail(msg);
            }
        }


        /// <heading>Clearing the event log</heading>
        private void ClearLog()
        {
            // Clears the event log for the current site
            EventLogProvider.ClearEventLog(MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName, RequestContext.UserHostAddress, SiteContext.CurrentSiteID);
        }
    }
}