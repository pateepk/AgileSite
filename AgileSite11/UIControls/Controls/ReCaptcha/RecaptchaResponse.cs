using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

using Newtonsoft.Json;

namespace CMS.UIControls
{
    /// <summary>
    /// Encapsulates a response from reCAPTCHA web service.
    /// </summary>
    public class RecaptchaResponse
    {
        #region "Variables"

        /// <summary>
        /// Valid.
        /// </summary>
        [Obsolete("This field is not used and will be removed in the next version without replacement.")]
        public static readonly RecaptchaResponse Valid = null;

        /// <summary>
        /// Invalid challenge.
        /// </summary>
        [Obsolete("This field is not used and will be removed in the next version without replacement.")]
        public static readonly RecaptchaResponse InvalidChallenge = null;

        /// <summary>
        /// Invalid response.
        /// </summary>
        [Obsolete("This field is not used and will be removed in the next version without replacement.")]
        public static readonly RecaptchaResponse InvalidResponse = null;


        /// <summary>
        /// Invalid solution.
        /// </summary>
        [Obsolete("This field is not used and will be removed in the next version without replacement.")]
        public static readonly RecaptchaResponse InvalidSolution = null;

        /// <summary>
        /// Recaptcha is not reachable.
        /// </summary>
        [Obsolete("This field is not used and will be removed in the next version without replacement.")]
        public static readonly RecaptchaResponse RecaptchaNotReachable = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether the reCAPTCHA validation was successful.
        /// </summary>
        [JsonProperty("success")]
        public bool IsValid
        {
            get;
            set;
        }


        /// <summary>
        /// The hostname of the site where the reCAPTCHA was solved
        /// </summary>
        [JsonProperty("hostname")]
        public string HostName
        {
            get;
            set;
        }


        /// <summary>
        /// Timestamp of the challenge load.
        /// </summary>
        [JsonProperty("challenge_ts")]
        public DateTime TimeStamp
        {
            get;
            set;
        }


        /// <summary>
        /// Error codes explaining why reCAPTCHA validation failed.
        /// </summary>
        [JsonProperty("error-codes")]
        public IEnumerable<string> ErrorCodes
        {
            get;
            set;
        } = Enumerable.Empty<string>();


        /// <summary>
        /// Aggregated error message from all the error codes.
        /// </summary>
        [JsonIgnore]
        public string ErrorMessage
        {
            get
            {
                return String.Join(" ", ErrorCodes.Select(x => ResHelper.GetString("recaptcha.error." + x)));
            }
            set
            {
            }
        }

        #endregion
    }
}