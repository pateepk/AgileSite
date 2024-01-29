using CMS.Helpers;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Enumeration describes status code returned by Strands portal in API setup calls.
    /// </summary>
    public enum StrandsSetupCallStatusCodeEnum
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
        /// Wrong api ID in setup call request.
        /// </summary>
        [EnumStringRepresentation("Error validating customer’s APIID")]
        WrongApiId = 900,


        /// <summary>
        /// Missing required parameter in setup call request.
        /// </summary>
        [EnumStringRepresentation("Error in the request parameters")]
        MissingParameter = 901,


        /// <summary>
        /// Unexpected error on Strands Recommender end.
        /// </summary>
        [EnumStringRepresentation("An unexpected error has occurred")]
        UnexpectedError = 902,


        /// <summary>
        /// Wrong validation token in setup call request.
        /// </summary>
        [EnumStringRepresentation("Error validating customer’s validation token")]
        WrongValidationToken = 903
    }
}
