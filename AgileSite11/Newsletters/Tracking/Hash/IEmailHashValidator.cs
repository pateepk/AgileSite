namespace CMS.Newsletters
{
    /// <summary>
    /// Provides method for validating hash generated from given email address.
    /// </summary>
    public interface IEmailHashValidator
    {
        /// <summary>
        /// Validates given <paramref name="hash"/> against <paramref name="emailAddress"/>.
        /// </summary>
        /// <param name="hash">Hash to be validated</param>
        /// <param name="emailAddress">Email address tracked link is sent to</param>
        /// <returns>True, if given <paramref name="hash"/> is valid; otherwise, false</returns>
        bool ValidateEmailHash(string hash, string emailAddress);
    }
}