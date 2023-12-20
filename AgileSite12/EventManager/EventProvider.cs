using System;
using System.Data;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Globalization;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Base;
using CMS.SiteProvider;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.EventManager
{
    /// <summary>
    /// Class providing sending of invitation in eventmanager.
    /// </summary>
    public static class EventProvider
    {
        /// <summary>
        /// Sends invitation e-mail to new attendee.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="eventData">Event data for text merging</param>
        /// <param name="sendTo">Send to e-mail address</param>
        public static void SendInvitation(string siteName, IDataContainer eventData, string sendTo)
        {
            // Get event attendee info
            EventAttendeeInfo attendee = EventAttendeeInfoProvider.GetEventAttendeeInfo(ValidationHelper.GetInteger(eventData.GetValue("NodeID"), 0), sendTo);

            if (attendee != null)
            {
                SendInvitation(siteName, eventData, attendee);
            }
        }


        /// <summary>
        /// Sends invitation e-mail to new attendee.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="eventData">Event data for text merging</param>
        /// <param name="attendeeData">Attendee data for text merging</param>
        public static void SendInvitation(string siteName, IDataContainer eventData, IDataContainer attendeeData)
        {
            SendInvitation(siteName, eventData, attendeeData, null);
        }


        /// <summary>
        /// Sends invitation e-mail to new attendee.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="eventData">Event data for text merging</param>
        /// <param name="attendeeData">Attendee data for text merging</param>
        /// <param name="tzi">Time zone for shifting datetime values</param>
        public static void SendInvitation(string siteName, IDataContainer eventData, IDataContainer attendeeData, TimeZoneInfo tzi)
        {
            // Check license
            if (!string.IsNullOrEmpty(DataHelper.GetNotEmpty(RequestContext.CurrentDomain, string.Empty)))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.EventManager);
            }

            if ((eventData != null) && (attendeeData != null) && !string.IsNullOrEmpty(siteName))
            {
                // Get invitation email template
                EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate("BookingEvent.Invitation", siteName);

                if (emailTemplate != null)
                {
                    // Initialize email message
                    EmailMessage message = new EmailMessage();

                    if (!string.IsNullOrEmpty(emailTemplate.TemplateFrom))
                    {
                        // Get sender e-mail address from template
                        message.From = emailTemplate.TemplateFrom;
                    }
                    else
                    {
                        // Get sender e-mail address from settings
                        message.From = TextHelper.LimitLength(SettingsKeyInfoProvider.GetValue(siteName + ".CMSEventManagerInvitationFrom"), 250);
                        // Get sender name from settings
                        string senderName = SettingsKeyInfoProvider.GetValue(siteName + ".CMSEventManagerSenderName");
                        // Check if "from" and "sender" length together do not exceed database limit
                        if (!String.IsNullOrEmpty(senderName) && !String.IsNullOrEmpty(message.From) && (message.From.Length + 3 + senderName.Length <= 250))
                        {
                            message.From = senderName + " <" + message.From + ">";
                        }
                    }

                    // Set recipient (new attendee)
                    message.Recipients = ValidationHelper.GetString(attendeeData.GetValue("AttendeeEmail"), "");

                    // Check sender and recipients for emptiness
                    if (string.IsNullOrEmpty(message.From) || string.IsNullOrEmpty(message.Recipients))
                    {
                        string warning = (string.IsNullOrEmpty(message.From) ? "Sender e-mail address is not set." : "Attendee e-mail address is not set.");

                        // Log warning to the event log
                        EventLogProvider.LogEvent(EventType.WARNING, "EventProvider", "SendInvitation", warning, RequestContext.CurrentURL);

                        return;
                    }

                    // Initialize macro resolver
                    MacroResolver resolver = MacroResolver.GetInstance();

                    // Prepare data
                    resolver.SetAnonymousSourceData(eventData, attendeeData);

                    // Add named source data
                    resolver.SetNamedSourceData("Event", eventData);
                    resolver.SetNamedSourceData("Attendee", attendeeData);

                    // Event date string macro
                    DateTime eventDate = ValidationHelper.GetDateTime(eventData.GetValue("EventDate"), DateTimeHelper.ZERO_TIME);
                    DateTime eventEndDate = ValidationHelper.GetDateTime(eventData.GetValue("EventEndDate"), DateTimeHelper.ZERO_TIME);
                    bool isAllDay = ValidationHelper.GetBoolean(eventData.GetValue("EventAllDay"), false);

                    resolver.SetNamedSourceData("eventdatestring", GetEventDateString(eventDate, eventEndDate, isAllDay, tzi, siteName), false);

                    resolver.Settings.EncodeResolvedValues = false;
                    if (!string.IsNullOrEmpty(emailTemplate.TemplateSubject))
                    {
                        // Get subject from template
                        message.Subject = resolver.ResolveMacros(emailTemplate.TemplateSubject);
                    }
                    else
                    {
                        // Get subject from settings
                        message.Subject = TextHelper.LimitLength(resolver.ResolveMacros(SettingsKeyInfoProvider.GetValue(siteName + ".CMSEventManagerInvitationSubject")), 450);
                    }

                    // Set Cc recipients
                    message.CcRecipients = emailTemplate.TemplateCc;
                    // Set Bcc recipients
                    message.BccRecipients = emailTemplate.TemplateBcc;

                    // E-mail format will be set according to site's settings
                    message.EmailFormat = EmailFormatEnum.Default;
                    message.ReplyTo = emailTemplate.TemplateReplyTo;

                    // Resolve e-mail plain text body
                    message.PlainTextBody = resolver.ResolveMacros(emailTemplate.TemplatePlainText);
                    message.PlainTextBody = URLHelper.MakeLinksAbsolute(message.PlainTextBody);

                    // Resolve e-mail body
                    resolver.Settings.EncodeResolvedValues = true;
                    message.Body = resolver.ResolveMacros(emailTemplate.TemplateText);
                    message.Body = URLHelper.MakeLinksAbsolute(message.Body);

                    // Add attachments
                    EmailHelper.ResolveMetaFileImages(message, emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

                    // Send the message
                    EmailSender.SendEmail(siteName, message);
                }
            }
        }


        /// <summary>
        /// Gets event data.
        /// </summary>
        /// <param name="eventNodeId">ID of event node</param>
        /// <param name="siteName">Site name</param>
        /// <param name="columns">Columns</param>
        /// <returns>Returns dataset with result</returns>
        public static DataSet GetEvent(int eventNodeId, string siteName, string columns)
        {
            var query = GetBookingEventQuery(columns);

            if (eventNodeId > 0)
            {
                var condition = new WhereCondition()
                    .WhereEquals("NodeID", eventNodeId)
                    .WhereNull("NodeLinkedNodeID");

                query.Where(condition);
            }


            // Use site name to validate the node ID for given site
            if (!String.IsNullOrEmpty(siteName))
            {
                query.OnSite(siteName);
            }

            return query;
        }


        /// <summary>
        /// Returns string representation of event time with dependence on current ITimeZone manager
        /// time zone settings.
        /// </summary>
        /// <param name="start">Event start time</param>
        /// <param name="end">Event end time</param>
        /// <param name="isAllDayEvent">Indicates if it is all day event - if yes, result does not contain times</param>
        /// <param name="tzi">Time zone for shifting datetime values</param>
        /// <param name="siteName">Site name</param>
        public static string GetEventDateString(DateTime start, DateTime end, bool isAllDayEvent, TimeZoneInfo tzi, string siteName)
        {
            string result = string.Empty;
            string gmtShift = string.Empty;

            if (TimeZoneHelper.TimeZonesEnabled)
            {
                if (tzi == null)
                {
                    // Get time zone for specified site
                    SiteInfo site = SiteInfoProvider.GetSiteInfo(siteName);
                    tzi = TimeZoneHelper.GetTimeZoneInfo(site);
                }

                // Convert times
                start = TimeZoneHelper.ConvertTimeZoneDateTime(start, TimeZoneHelper.ServerTimeZone, tzi);
                end = TimeZoneHelper.ConvertTimeZoneDateTime(end, TimeZoneHelper.ServerTimeZone, tzi);

                if (tzi != null)
                {
                    // Get string representation of time zone shift
                    gmtShift = TimeZoneHelper.GetUTCStringOffset(tzi);
                }
            }

            if ((start != DateTimeHelper.ZERO_TIME) || (end != DateTimeHelper.ZERO_TIME))
            {
                if ((start != DateTimeHelper.ZERO_TIME) && (end != DateTimeHelper.ZERO_TIME))
                {
                    // Get date string combined from start time and end time
                    if (isAllDayEvent)
                    {
                        if (start.Date.CompareTo(end.Date) == 0)
                        {
                            // All day event with same start and end dates
                            result = start.ToShortDateString();
                        }
                        else
                        {
                            // All day event through multiple days
                            result = string.Format("{0} - {1}", start.ToShortDateString(), end.ToShortDateString());
                        }
                    }
                    else
                    {
                        if (start.Date.CompareTo(end.Date) == 0)
                        {
                            // One-day event with times
                            result = string.Format("{0}, {1} - {2}{3}", start.ToShortDateString(), start.ToShortTimeString(), end.ToShortTimeString(), gmtShift);
                        }
                        else
                        {
                            // Multi-day event with times
                            result = string.Format("{0}{2} - {1}{2}", start.ToString("g"), end.ToString("g"), gmtShift);
                        }
                    }
                }
                else if (start == DateTimeHelper.ZERO_TIME)
                {
                    // Get date string from end time
                    if (isAllDayEvent)
                    {
                        result = end.ToShortDateString();
                    }
                    else
                    {
                        result = end.ToString("g") + gmtShift;
                    }
                }
                else if (end == DateTimeHelper.ZERO_TIME)
                {
                    // Get date string from start time
                    if (isAllDayEvent)
                    {
                        result = start.ToShortDateString();
                    }
                    else
                    {
                        result = start.ToString("g") + gmtShift;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the booking event query for selecting events with number of event attendees.
        /// </summary>
        /// <param name="columns">Columns to be selected</param>
        internal static DocumentQuery GetBookingEventQuery(string columns)
        {
            // Attendees count column
            var attendeesQueryColumn = EventAttendeeInfoProvider.GetEventAttendees()
                                                                .Column(new CountColumn())
                                                                .WhereEquals("AttendeeEventNodeID", "[NodeID]".AsExpression())
                                                                .AsColumn("AttendeesCount");

            // Create query
            var query = DocumentHelper.GetDocuments("cms.bookingevent")
                                      .All()
                                      .PublishedVersion()
                                      .AddColumn(attendeesQueryColumn)
                                      .AsNested<DocumentQuery>();

            // Add specific columns if specified
            if (!String.IsNullOrEmpty(columns))
            {
                query.Columns(columns);
            }

            return query;
        }
    }
}