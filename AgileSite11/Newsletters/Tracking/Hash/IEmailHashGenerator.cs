namespace CMS.Newsletters
{
    /// <summary>
    /// Provides method for generating hash from given email address.
    /// </summary>
    internal interface IEmailHashGenerator
    {
        /// <summary>
        /// Gets hash for given <paramref name="emailAddress"/>.
        /// </summary>
        /// <param name="emailAddress">Email address tracked link is sent to</param>
        /// <returns>Hash obtained from given input parameters</returns>
        string GetEmailHash(string emailAddress);
    }
}