using System;
using System.Runtime.Serialization;

using CMS.Helpers;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// Class for user information stored in token in JSON format
    /// </summary>
    [DataContract]
    internal class JsonWebTokenClaims
    {
        #region "Variables"

        private DateTime? expiration;

        #endregion


        #region "Properties"

        /// <summary>
        /// Expiration time. The unit is given in seconds since midnight of 1/1/1970 in UTC.
        /// </summary>
        [DataMember(Name = "exp")]
        private int expUnixTime
        {
            get;
            set;
        }


        /// <summary>
        /// Expiration time - DateTime object.
        /// </summary>
        public DateTime Expiration
        {
            get
            {
                if (expiration == null)
                {
                    expiration = DateTimeHelper.UNIX_TIME_START.AddSeconds(expUnixTime);
                }

                return (DateTime)expiration;
            }
        }


        /// <summary>
        /// Identifies the principal who issued the token.
        /// </summary>
        [DataMember(Name = "iss")]
        public string Issuer
        {
            get;
            private set;
        }


        /// <summary>
        /// Domain name for the application.
        /// </summary>
        [DataMember(Name = "aud")]
        public string Audience
        {
            get;
            private set;
        }


        /// <summary>
        /// An identifier for the user which is unique to the application. 
        /// </summary>
        [DataMember(Name = "uid")]
        public string UserId
        {
            get;
            private set;
        }


        /// <summary>
        /// Version identifier for the token.
        /// </summary>
        [DataMember(Name = "ver")]
        public int Version
        {
            get;
            private set;
        }


        /// <summary>
        /// The Windows client identifier of the application, if there is one.
        /// </summary>
        [DataMember(Name = "urn:microsoft:appuri")]
        public string ClientIdentifier
        {
            get;
            private set;
        }

        #endregion
    }
}
