using System;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Handler user related data in event log.
    /// </summary>
    internal static class EventLogDataHandler
    {
        /// <summary>
        /// Indicates whether original user name should be added to event.
        /// </summary>
        internal static bool SkipOriginalUserName { get; set; }


        internal static void Init()
        {
            EventLogEvents.PrepareData.Execute += PrepareData;
        }


        private static void PrepareData(object sender, LogEventArgs e)
        {
            var eventObject = e.Event;

            if (eventObject.UserID <= 0 && String.IsNullOrEmpty(eventObject.UserName) && CMSHttpContext.Current?.User != null)
            {
                var user = CMSActionContext.CurrentUser;
                if (user != null)
                {
                    eventObject.UserID = user.UserID;
                    eventObject.UserName = user.UserName;
                }
            }

            // Fallback to obtain user name from request context
            if (String.IsNullOrEmpty(eventObject.UserName))
            {
                eventObject.UserName = GetUserNameFromRequestContext();
            }

            // Add original user name if it's available and should not be skipped
            if (!SkipOriginalUserName && !String.IsNullOrEmpty(MembershipContext.OriginalUserName) && !eventObject.UserName.Contains(MembershipContext.OriginalUserName))
            {
                eventObject.UserName += $" ({MembershipContext.OriginalUserName})";
            }
        }


        private static string GetUserNameFromRequestContext()
        {
            return ValidationHelper.UseSafeUserName ? ValidationHelper.GetSafeUserName(RequestContext.UserName, null) : RequestContext.UserName;
        }
    }
}
