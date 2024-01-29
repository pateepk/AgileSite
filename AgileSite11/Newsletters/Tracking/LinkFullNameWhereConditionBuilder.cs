using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Builds where condition to get link based on its full name.
    /// </summary>
    internal class LinkFullNameWhereConditionBuilder
    {
        private readonly string fullName;


        /// <summary>
        /// Creates instance of <see cref="LinkFullNameWhereConditionBuilder"/> class.
        /// </summary>
        /// <param name="fullName">Full name to use for building the where condition.</param>
        public LinkFullNameWhereConditionBuilder(string fullName)
        {
            this.fullName = fullName;
        }


        /// <summary>
        /// Builds the full name where condition for the given full name of the link.
        /// </summary>
        public IWhereCondition Build()
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return new WhereCondition();
            }

            var delimiterIndex = GetDelimiterIndex();
            if (delimiterIndex < 0)
            {
                return new WhereCondition();
            }

            var issueId = GetIssueId(delimiterIndex);
            var target = GetLinkTarget(delimiterIndex);

            return GetWhereCondition(issueId, target);
        }


        private static IWhereCondition GetWhereCondition(int issueId, string target)
        {
            return new WhereCondition()
                .WhereEquals("LinkIssueID", issueId)
                .WhereEquals("LinkTarget", target);
        }


        private string GetLinkTarget(int delimiterIndex)
        {
            return fullName.Substring(delimiterIndex + 1);
        }


        private int GetIssueId(int delimiterIndex)
        {
            return ValidationHelper.GetInteger(fullName.Substring(0, delimiterIndex), 0);
        }


        private int GetDelimiterIndex()
        {
            const string FULLNAME_DELIMITER = ".";

            return fullName.IndexOf(FULLNAME_DELIMITER, StringComparison.Ordinal);
        }
    }

}