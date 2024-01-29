namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods for building full name of subscribers of all existing types.
    /// </summary>
    public interface ISubscriberFullNameFormater
    {
        /// <summary>
        /// Returns name for user subscriber.
        /// </summary>
        /// <param name="fullName">Full name of user subscriber</param>
        /// <returns>Name of user subscriber</returns>
        string GetUserSubscriberName(string fullName);


        /// <summary>
        /// Returns name for role subscriber.
        /// </summary>
        /// <param name="roleName">Name of role</param>
        /// <returns>Name of role subscriber</returns>
        string GetRoleSubscriberName(string roleName);


        /// <summary>
        /// Returns name of contact subscriber. Adds spaces where necessary.
        /// </summary>
        /// <param name="firstName">First name of contact</param>
        /// <param name="middleName">Middle name of contact</param>
        /// <param name="lastName">Last name of contact</param>
        /// <returns>Name of contact subscriber</returns>
        string GetContactSubscriberName(string firstName, string middleName, string lastName);


        /// <summary>
        /// Returns name for contact group subscriber.
        /// </summary>
        /// <param name="contactGroupName">Name of contact group</param>
        /// <returns>Name of contact group subscriber</returns>
        string GetContactGroupSubscriberName(string contactGroupName);


        /// <summary>
        /// Returns name for persona subscriber.
        /// </summary>
        /// <param name="personaName">Name of persona</param>
        /// <returns>Name of persona subscriber</returns>
        string GetPersonaSubscriberName(string personaName);
    }
}