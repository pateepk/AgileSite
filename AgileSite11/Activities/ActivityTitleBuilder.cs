using System;
using System.Linq;

using CMS.Helpers;

namespace CMS.Activities
{
    /// <summary>
    /// Provides methods to create activity title.
    /// </summary>
    public class ActivityTitleBuilder
    {
        /// <summary>
        /// Creates title for given <paramref name="activityType"/> and user information in <paramref name="visitorData"/>. Title is read from resources.
        /// </summary>
        /// <param name="activityType">Activity type</param>
        /// <param name="visitorData">Visitor data to create title from; if null default resource string is used</param>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="activityType"/> is null or empty</exception>
        /// <returns>Title with included user name.</returns>
        public string CreateTitleWithUserName(string activityType, VisitorData visitorData)
        {
            if (string.IsNullOrEmpty(activityType))
            {
                throw new ArgumentNullException("activityType");
            }

            string visitorName = visitorData == null ? null : BuildVisitorName(visitorData);
            return CreateTitle(activityType, visitorName);
        }


        /// <summary>
        /// Creates title from title data and <paramref name="activityType"/>. Title is read from resources.
        /// </summary>
        /// <param name="activityType">Activity type</param>
        /// <param name="titleData">Data that should be displayed in title; if null default resource string is used</param>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="activityType"/> is null or empty</exception>
        /// <returns>Returns activity title.</returns>
        public string CreateTitle(string activityType, string titleData = null)
        {
            if (string.IsNullOrEmpty(activityType))
            {
                throw new ArgumentNullException("activityType");
            }

            var data = titleData ?? ResHelper.GetString("general.unknown", CultureHelper.DefaultUICultureCode);
            return string.Format(ResHelper.GetString("om.acttitle." + activityType, CultureHelper.DefaultUICultureCode), data);
        }


        /// <summary>
        /// Returns formatted visitor name.
        /// </summary>
        /// <param name="visitorData">Visitor data used to obtain visitor name.</param>
        /// <returns>Created visitor name.</returns>
        private string BuildVisitorName(VisitorData visitorData)
        {
            string result = new[]
            {
                visitorData.FirstName,
                visitorData.MiddleName,
                visitorData.LastName,
                string.IsNullOrEmpty(visitorData.Email) ? null : string.Format("({0})", visitorData.Email)
            }.Where(c => !string.IsNullOrEmpty(c)).Join(" ");

            return string.IsNullOrEmpty(result) ? visitorData.UserName : result;
        }
    }
}