namespace CMS.SalesForce.Automation
{

    /// <summary>
    /// Represent the result of "Replicate lead" command.
    /// </summary>
    public enum ReplicateLeadCommandResultEnum
    {

        /// <summary>
        /// The action completed without errors.
        /// </summary>
        Success,

        /// <summary>
        /// Salesforce.com integration is not available.
        /// </summary>
        FeatureNotAvailable,

        /// <summary>
        /// Salesforce organization access is not authorized.
        /// </summary>
        NoCredentials

    }

}