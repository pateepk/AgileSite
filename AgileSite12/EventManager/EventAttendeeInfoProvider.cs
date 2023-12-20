using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.EventManager
{
    /// <summary>
    /// Class providing EventAttendeeInfo management.
    /// </summary>
    public class EventAttendeeInfoProvider : AbstractInfoProvider<EventAttendeeInfo, EventAttendeeInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static EventAttendeeInfo GetEventAttendeeInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the EventAttendeeInfo structure for the specified eventAttendee.
        /// </summary>
        /// <param name="eventAttendeeId">EventAttendee id</param>
        public static EventAttendeeInfo GetEventAttendeeInfo(int eventAttendeeId)
        {
            return ProviderObject.GetInfoById(eventAttendeeId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified eventAttendee.
        /// </summary>
        /// <param name="eventAttendee">EventAttendee to set</param>
        public static void SetEventAttendeeInfo(EventAttendeeInfo eventAttendee)
        {
            ProviderObject.SetInfo(eventAttendee);
        }


        /// <summary>
        /// Deletes specified eventAttendee.
        /// </summary>
        /// <param name="eventAttendeeObj">EventAttendee object</param>
        public static void DeleteEventAttendeeInfo(EventAttendeeInfo eventAttendeeObj)
        {
            ProviderObject.DeleteInfo(eventAttendeeObj);
        }


        /// <summary>
        /// Deletes specified eventAttendee.
        /// </summary>
        /// <param name="eventAttendeeId">EventAttendee id</param>
        public static void DeleteEventAttendeeInfo(int eventAttendeeId)
        {
            EventAttendeeInfo eventAttendeeObj = GetEventAttendeeInfo(eventAttendeeId);
            DeleteEventAttendeeInfo(eventAttendeeObj);
        }


        /// <summary>
        /// Gets attendee info by event node ID and attendee's e-mail.
        /// </summary>
        /// <param name="eventNodeId">Event node id</param>
        /// <param name="attendeeEmail">Attendee email</param>
        public static EventAttendeeInfo GetEventAttendeeInfo(int eventNodeId, string attendeeEmail)
        {
            if (String.IsNullOrEmpty(attendeeEmail))
            {
                return null;
            }

            var attendee = GetEventAttendees(eventNodeId)
                .TopN(1)
                .WhereEquals("AttendeeEmail", attendeeEmail)
                .FirstOrDefault();

            return attendee;
        }


        /// <summary>
        /// Provides ObjectQuery access to EventAttendees
        /// </summary>
        public static ObjectQuery<EventAttendeeInfo> GetEventAttendees()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets the ObjectQuery of event attendees of the specific event.
        /// </summary>
        /// <param name="eventNodeId">Event node ID</param>
        public static ObjectQuery<EventAttendeeInfo> GetEventAttendees(int eventNodeId)
        {
            return GetEventAttendees()
                            .WhereEquals("AttendeeEventNodeID", eventNodeId);
        }


        /// <summary>
        /// Returns true if attendee is registered for the event.
        /// </summary>
        /// <param name="eventNodeId">Event node</param>
        /// <param name="attendeeEmail">Attendee's e-mail</param>
        public static bool IsRegisteredForEvent(int eventNodeId, string attendeeEmail)
        {
            return GetEventAttendeeInfo(eventNodeId, attendeeEmail) != null;
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(EventAttendeeInfo info)
        {
            if (info == null)
            {
                return;
            }

            base.DeleteInfo(info);
        }

        #endregion
    }
}