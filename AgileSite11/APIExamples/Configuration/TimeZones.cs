using System;

using CMS.Globalization;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds time zone API examples.
    /// </summary>
    /// <pageTitle>Time zones</pageTitle>
    internal class TimeZones
    {
        /// <heading>Creating a new time zone</heading>
        private void CreateTimezone()
        {
            // Creates a new time zone object
            CMS.Globalization.TimeZoneInfo newTimezone = new CMS.Globalization.TimeZoneInfo();

            // Sets the time zone properties
            newTimezone.TimeZoneDisplayName = "New time zone";
            newTimezone.TimeZoneName = "NewTimezone";
            newTimezone.TimeZoneGMT = -12;
            newTimezone.TimeZoneDaylight = true;
            newTimezone.TimeZoneRuleStartRule = "MAR|SUN|1|LAST|3|0|1";
            newTimezone.TimeZoneRuleEndRule = "OCT|SUN|1|LAST|3|0|0";

            // Saves the new time zone into the database
            TimeZoneInfoProvider.SetTimeZoneInfo(newTimezone);
        }

        
        /// <heading>Updating a time zone</heading>
        private void GetAndUpdateTimezone()
        {
            // Gets the time zone
            CMS.Globalization.TimeZoneInfo updateTimezone = TimeZoneInfoProvider.GetTimeZoneInfo("NewTimezone");
            if (updateTimezone != null)
            {
                // Updates the time zone properties
                updateTimezone.TimeZoneDisplayName = updateTimezone.TimeZoneDisplayName.ToLower();

                // Saves the changes to the database
                TimeZoneInfoProvider.SetTimeZoneInfo(updateTimezone);
            }
        }

        
        /// <heading>Updating multiple time zones</heading>
        private void GetAndBulkUpdateTimezones()
        {
            // Prepares the where condition
            string where = "TimeZoneName LIKE N'NewTimezone%'";

            // Gets all time zones whose name starts with 'NewTimezone'
            var timezones = TimeZoneInfoProvider.GetTimeZones().Where(where);            
            
            // Loops through the individual time zones
            foreach (CMS.Globalization.TimeZoneInfo timezone in timezones)
            {                
                // Updates the time zone's properties
                timezone.TimeZoneDisplayName = timezone.TimeZoneDisplayName.ToUpper();

                // Saves the changes to the database
                TimeZoneInfoProvider.SetTimeZoneInfo(timezone);
            }            
        }

        
        /// <heading>Deleting a time zone</heading>
        private void DeleteTimezone()
        {
            // Gets the time zone
            CMS.Globalization.TimeZoneInfo deleteTimezone = TimeZoneInfoProvider.GetTimeZoneInfo("NewTimezone");

            if (deleteTimezone != null)
            {
                // Deletes the time zone
                TimeZoneInfoProvider.DeleteTimeZoneInfo(deleteTimezone);
            }
        }

        
        /// <heading>Converting time to the current user's time zone</heading>
        private void ConvertTime()
        {
            // Gets the current user
            UserInfo user = UserInfoProvider.GetFullUserInfo(MembershipContext.AuthenticatedUser.UserID);

            // Checks that the user exists
            if (user != null)
            {
                // Gets the current time converted to the user's time zone
                DateTime convertedTime = TimeZoneHelper.ConvertToUserDateTime(DateTime.Now, user);
            }            
        }
    }
}
