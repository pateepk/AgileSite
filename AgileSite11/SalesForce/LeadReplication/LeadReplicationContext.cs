namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce lead replication context.
    /// </summary>
    public sealed class LeadReplicationContext
    {

        #region "Public properties"

        /// <summary>
        /// Gets or sets the identifier of the score that provides the value to determine whether the contact will be replicated.
        /// </summary>
        public int ScoreId { get; set; }

        /// <summary>
        /// Gets or sets the minimum score value that the contact has to reach to be replicated.
        /// </summary>
        public int MinScoreValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of SalesForce operations in one integration API request.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Gets or sets the macro that provides the description of the replicated contact.
        /// </summary>
        public string DescriptionMacro { get; set; }

        /// <summary>
        /// Gets or sets the company name to use when there is no account associated with the replicated contact.
        /// </summary>
        public string DefaultCompanyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the replicated leads should be updated when the bound contact has changed.
        /// </summary>
        public bool UpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mapping of contacts to SalesForce leads.
        /// </summary>
        public Mapping Mapping { get; set; }

        /// <summary>
        /// Gets or sets an optional white list of contact identifiers.
        /// </summary>
        public int[] ContactIdentifiers { get; set; }

        #endregion

    }

}