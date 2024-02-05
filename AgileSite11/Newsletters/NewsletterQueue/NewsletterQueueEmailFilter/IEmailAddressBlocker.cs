namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods which decide whether email to certain email address should or should not be generated to the newsletter queue.
    /// </summary>
    public interface IEmailAddressBlocker
    {
        /// <summary>
        /// Checks whether emails with the specified email address as recipients should be generated to the newsletter queue.
        /// </summary>
        /// <param name="emailAddress">Email address which will be checked</param>
        /// <returns>False if emails shouldn't be generated, otherwise true</returns>
        bool IsBlocked(string emailAddress);
    }
}