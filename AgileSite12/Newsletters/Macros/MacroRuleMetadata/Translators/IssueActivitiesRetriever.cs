using System;

using CMS.Activities;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Encapsulates methods to retrieve data of activities made on newsletter issues.
    /// </summary>
    internal class IssueActivitiesRetriever
    {
        /// <summary>
        /// Returns activities of given <paramref name="activityType"/> which contacts have made on a specified <see cref="IssueInfo"/>.
        /// </summary>
        /// <param name="issueGuid">Issue GUID.</param>
        /// <param name="activityType">Activity type.</param>
        public static ObjectQuery<ActivityInfo> GetActivitiesQuery(Guid issueGuid, string activityType)
        {
            var issueIds = IssueInfoProvider.GetIssues()
                                            .WhereEquals("IssueGUID", issueGuid)
                                            .Or().WhereIn("IssueVariantOfIssueID", IssueInfoProvider.GetIssues()
                                                .Column("IssueID")
                                                .WhereEquals("IssueGUID", issueGuid)
                                            )
                                            .Column("IssueID");

            return ActivityInfoProvider.GetActivities()
                                       .WhereEquals("ActivityType", activityType)
                                       .WhereIn("ActivityItemDetailID", issueIds);
                      
        }
    }
}
