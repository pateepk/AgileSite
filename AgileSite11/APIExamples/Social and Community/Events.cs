using System;

using CMS.Membership;
using CMS.DocumentEngine;
using CMS.Localization;
using CMS.EventManager;
using CMS.SiteProvider;
using CMS.PortalEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds event-related API examples.
    /// </summary>
    /// <pageTitle>Events</pageTitle>
    internal class EventsMain
    {
        /// <summary>
        /// Holds event API examples.
        /// </summary>
        /// <groupHeading>Events</groupHeading>
        private class Events
        {
            /// <heading>Creating an event</heading>
            private void CreateEvent()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the root page
                TreeNode root = tree.SelectNodes()
                    .Path("/")
                    .OnCurrentSite()
                    .FirstObject;

                // Creates a new "Page (menu item)" page
                TreeNode node = TreeNode.New(SystemDocumentTypes.MenuItem, tree);

                // Sets the basic properties of the page
                node.DocumentName = "NewPage";
                node.DocumentCulture = LocalizationContext.PreferredCultureCode;

                // Gets a page template
                PageTemplateInfo template = PageTemplateInfoProvider.GetPageTemplateInfo("cms.empty");

                if (template != null)
                {
                    // Sets the page template for the page
                    node.SetDefaultPageTemplateID(template.PageTemplateId);
                }

                // Inserts the page under the root
                node.Insert(root);

                // Creates a new "Event (booking system)" page
                TreeNode eventNode = TreeNode.New("CMS.BookingEvent", tree);

                // Sets field values for the event
                eventNode.DocumentName = "NewEvent";
                eventNode.DocumentCulture = LocalizationContext.PreferredCultureCode;
                eventNode.SetValue("EventSummary", "Event summary");
                eventNode.SetValue("EventDetails", "Event details");
                eventNode.SetValue("EventLocation", "Event location");
                eventNode.SetValue("EventDate", DateTime.Now);
                eventNode.SetValue("EventCapacity", 100);

                // Gets a page template for the event page
                PageTemplateInfo eventTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("cms.empty");

                if (eventTemplate != null)
                {
                    // Sets the page template for the event page
                    eventNode.SetDefaultPageTemplateID(eventTemplate.PageTemplateId);
                }

                // Inserts the "Event (booking system)" under the specified page
                eventNode.Insert(node);
            }


            /// <heading>Updating an event</heading>
            private void GetAndUpdateEvent()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the event page
                TreeNode node = tree.SelectNodes()
                    .Path("/NewPage/NewEvent")
                    .OnCurrentSite()
                    .FirstObject;

                if (node != null)
                {
                    // Updates a value
                    node.SetValue("EventDetails", "My event details were updated.");
                    node.SetValue("EventCapacity", 200);
                    node.Update();
                }
            }


            /// <heading>Deleting an event</heading>
            private void DeleteEvent()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the event page
                TreeNode eventNode = tree.SelectNodes()
                    .Path("/NewPage/NewEvent")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                // Gets the parent page of the event
                TreeNode node = tree.SelectNodes()
                    .Path("/NewPage")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (eventNode != null && node != null)
                {
                    // Deletes the event page and its parent page (including all culture versions and version history)
                    eventNode.Delete(true, true);
                    node.Delete(true, true);
                }
            }
        }

        /// <summary>
        /// Holds event attendee API examples.
        /// </summary>
        /// <groupHeading>Event attendees</groupHeading>
        private class Attendees
        {
            /// <heading>Creating an attendee</heading>
            private void CreateAttendee()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the event page
                TreeNode eventNode = tree.SelectNodes()
                    .Path("/NewPage/NewEvent")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (eventNode != null)
                {
                    // Creates a new attendee object
                    EventAttendeeInfo newAttendee = new EventAttendeeInfo();

                    // Sets the properties of the attendee
                    newAttendee.AttendeeEmail = "NewAttendee@localhost.local";
                    newAttendee.AttendeeEventNodeID = eventNode.NodeID;
                    newAttendee.AttendeeFirstName = "John";
                    newAttendee.AttendeeLastName = "Doe";

                    // Saves the attendee to the database
                    EventAttendeeInfoProvider.SetEventAttendeeInfo(newAttendee);
                }
            }


            /// <heading>Updating an attendee</heading>
            private void GetAndUpdateAttendee()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the event page
                TreeNode eventNode = tree.SelectNodes()
                    .Path("/NewPage/NewEvent")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (eventNode != null)
                {
                    // Gets the attendee based on an email address
                    EventAttendeeInfo updateAttendee = EventAttendeeInfoProvider.GetEventAttendeeInfo(eventNode.NodeID, "NewAttendee@localhost.local");

                    if (updateAttendee != null)
                    {
                        // Updates the properties of the attendee
                        updateAttendee.AttendeeEmail = updateAttendee.AttendeeEmail.ToLower();

                        // Saves the changes to the database
                        EventAttendeeInfoProvider.SetEventAttendeeInfo(updateAttendee);
                    }
                }
            }


            /// <heading>Updating multiple attendees</heading>
            private void GetAndBulkUpdateAttendees()
            {
                // Gets the tree structure
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the event page
                TreeNode eventNode = tree.SelectNodes()
                    .Path("/NewPage/NewEvent")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (eventNode != null)
                {
                    // Gets the attendees of the selected event whose emails start with 'NewAttendee' 
                    var attendees = EventAttendeeInfoProvider.GetEventAttendees()
                                                             .WhereEquals("AttendeeEventNodeID", eventNode.NodeID)
                                                             .WhereStartsWith("AttendeeEmail", "NewAttendee");

                    // Loops through individual attendees
                    foreach (EventAttendeeInfo attendee in attendees)
                    {
                        // Updates the properties of the attendee
                        attendee.AttendeeEmail = attendee.AttendeeEmail.ToUpper();

                        // Saves the changes to the database
                        EventAttendeeInfoProvider.SetEventAttendeeInfo(attendee);
                    }
                }
            }


            /// <heading>Deleting an attendee</heading>
            private void DeleteAttendee()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the event page
                TreeNode eventNode = tree.SelectNodes()
                    .Path("/NewPage/NewEvent")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                if (eventNode != null)
                {
                    // Gets the attendee based on an email address
                    EventAttendeeInfo deleteAttendee = EventAttendeeInfoProvider.GetEventAttendeeInfo(eventNode.NodeID, "NewAttendee@localhost.local");

                    if (deleteAttendee != null)
                    {
                        // Deletes the attendee
                        EventAttendeeInfoProvider.DeleteEventAttendeeInfo(deleteAttendee);
                    }
                }
            }
        }
    }
}
