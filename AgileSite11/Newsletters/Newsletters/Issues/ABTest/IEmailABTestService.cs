namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for email A/B testing service.
    /// </summary>
    public interface IEmailABTestService
    {
        /// <summary>
        /// Creates a new A/B test issue variant.
        /// </summary>
        /// <remarks>The new variant is always created from the original issue variant.</remarks>
        /// <param name="name">Name of the new variant.</param>
        /// <param name="issueId">ID of the source issue.</param>
        /// <returns>Returns the newly created variant.</returns>
        IssueInfo CreateVariant(string name, int issueId);


        /// <summary>
        /// Deletes email variant given by <paramref name="deleteIssueId"/>. If single variant remains after the variant is deleted,
        /// then the remaining variant and related A/B test is deleted as well.
        /// </summary>
        /// <param name="deleteIssueId">Email variant ID</param>
        void DeleteVariant(int deleteIssueId);


        /// <summary>
        /// Returns email variant which is considered as 'original'. Original variant is a clone of email which is A/B tested.
        /// </summary>
        /// <param name="issueId">ID of an issue which is A/B tested.</param>
        IssueInfo GetOriginalVariant(int issueId);
    }
}
