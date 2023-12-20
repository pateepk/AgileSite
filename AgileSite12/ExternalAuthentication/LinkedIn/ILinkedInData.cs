using System.Collections.Generic;

namespace CMS.ExternalAuthentication.LinkedIn
{
    /// <summary>
    /// Represents data required to get LinkedIn access token.
    /// </summary>
    public interface ILinkedInData
    {
        /// <summary>
        /// Gets the editor id in order to set its value.
        /// </summary>
        string EditorId
        {
            get;
        }


        /// <summary>
        /// Gets LinkedIn account api key.
        /// </summary>
        string ApiKey
        {
            get;
        }


        /// <summary>
        /// Gets LinkedIn account api secret.
        /// </summary>
        string ApiSecret
        {
            get;
        }


        /// <summary>
        /// Gets LinkedIn code required to generate access token.
        /// </summary>
        string Code
        {
            get;
        }


        /// <summary>
        /// Gets the error message when access token was not retrieved.
        /// </summary>
        string Error
        {
            get;
        }


        /// <summary>
        /// Returns true when all of <see cref="ApiKey"/>, <see cref="ApiSecret"/>, <see cref="Code"/> are empty.
        /// </summary>
        bool IsEmpty
        {
            get;
        }


        /// <summary>
        /// Returns true when user does not allow access using LinkedIn page.
        /// </summary>
        bool UserDeniedAccess
        {
            get;
        }


        /// <summary>
        /// Returns true when one of <see cref="ApiKey"/> and <see cref="ApiSecret"/> is empty or if <see cref="Code"/> or <see cref="Error"/> are empty.
        /// </summary>
        bool SettingsMissing
        {
            get;
        }


        /// <summary>
        /// Contains additional query parameters used when opening authorization page.
        /// </summary>
        IDictionary<string, string> AdditionalQueryParameters
        {
            get;
        }
    }
}