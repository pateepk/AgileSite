using System;
using System.Linq;

namespace CMS.Newsletters.Web.UI
{
    internal abstract class EmailContactDemographicsDataRetrieverBase
    {
        protected int[] GetIssueIDs(int issueID)
        {
            var issue = IssueInfoProvider.GetIssueInfo(issueID);
            if (issue == null)
            {
                throw new InvalidOperationException($"Issue was not found for given ID ({issueID})");
            }

            var parentAndVariantsIds = IssueInfoProvider.GetIssues()
                                                        .Column("IssueID")
                                                        .WhereEquals("IssueVariantOfIssueID", issue.IssueID)
                                                        .GetListResult<int>();

            parentAndVariantsIds.Add(issue.IssueID);
            return parentAndVariantsIds.ToArray();
        }
    }
}