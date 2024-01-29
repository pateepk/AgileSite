using CMS.Helpers;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Enumeration describes status code returned by Strands portal in API template calls.
    /// </summary>
    public enum StrandsTemplateCallStatusCodeEnum
    {
        /// <summary>
        /// Occurs when no status code is present in document.
        /// </summary>
        [EnumStringRepresentation("No status code present in document")]
        UnknownStatus = -1,


        /// <summary>
        /// Success response.
        /// </summary>
        [EnumStringRepresentation("Success")]
        Success = 0,


        /// <summary>
        /// Error in request parameter response.
        /// </summary>
        [EnumStringRepresentation("Error in the request parameters")]
        ErrorInTheRequestParameters = 1,


        /// <summary>
        /// Error in provided api ID response.
        /// </summary>
        [EnumStringRepresentation("Error validating customer’s APIID")]
        ErrorValidatingCustomersApiID = 2,


        /// <summary>
        /// Unexpected error response.
        /// </summary>
        [EnumStringRepresentation("Unexpected error")]
        UnexpectedError = 3,


        /// <summary>
        /// Email recommendations are not enabled for requested account due to licensing reasons.
        /// </summary>
        [EnumStringRepresentation("Email recommendations unavailable")]
        EmailRecommendationsUnavailable = 6,
    }
}