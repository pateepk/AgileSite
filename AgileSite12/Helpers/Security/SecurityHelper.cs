using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Core;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Contains methods for ensuring security
    /// </summary>
    public static class SecurityHelper
    {
        #region "Constants"

        /// <summary>
        /// HTTP header which can contain session token.
        /// </summary>
        public const string SESSION_TOKEN_HEADER = "Cms-Session-Token";


        /// <summary>
        /// Length of sub-key derived from user password. 
        /// <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes"/> uses internally HMACSHA1 which returns 20 bytes long output, so a longer derived sub-key adds no additional security.
        /// </summary>
        private const int PBKDF2_SUBKEY_BYTES = 20; 


        /// <summary>
        /// Length of salt generated within <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes"/>.
        /// </summary>
        private const int PBKDF2_SALT_BYTES = 16;

        #endregion


        #region "Variables"

        /// <summary>
        /// Indicates whether security callback was set.
        /// </summary>
        private static bool securityCallbackSet;


        /// <summary>
        /// Regular expression for replacing square brackets from like expressions.
        /// </summary>
        public static CMSRegex RegSquerBrackets = new CMSRegex("(\\[|\\])");


        /// <summary>
        /// Indicates whether scheduler accepts all certificates (including invalid certificates).
        /// </summary>
        private static bool? mSchedulerAcceptAllCertificates;


        /// <summary>
        /// Indicates whether staging accepts all certificates (including invalid certificates).
        /// </summary>
        private static bool? mStagingAcceptAllCertificates;


        /// <summary>
        /// Indicates whether application requests should accept all certificates (including invalid certificates).
        /// </summary>
        private static bool? mAcceptAllCertificates;


        /// <summary>
        /// Iterations count used within <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes"/>. 
        /// More iterations will generate more resistant hashes to brute-force attacks, however performance issues might arise when set to high. 
        /// </summary>
        private static int mPbkdf2IterationsCount = 10000;

        #endregion


        #region "Properties"
        

        /// <summary>
        /// Indicates whether scheduler accepts all certificates (including invalid certificates).
        /// </summary>
        private static bool SchedulerAcceptAllCertificates
        {
            get
            {
                if (mSchedulerAcceptAllCertificates == null)
                {
                    mSchedulerAcceptAllCertificates = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSchedulerAcceptAllCertificates"], false);
                }

                return mSchedulerAcceptAllCertificates.Value;
            }
        }


        /// <summary>
        /// Indicates whether staging accepts all certificates (including invalid certificates).
        /// </summary>
        private static bool StagingAcceptAllCertificates
        {
            get
            {
                if (mStagingAcceptAllCertificates == null)
                {
                    mStagingAcceptAllCertificates = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStagingAcceptAllCertificates"], false);
                }

                return mStagingAcceptAllCertificates.Value;
            }
        }


        /// <summary>
        /// Indicates whether application requests should accept all certificates (including invalid certificates).
        /// </summary>
        private static bool AcceptAllCertificates
        {
            get
            {
                if (mAcceptAllCertificates == null)
                {
                    mAcceptAllCertificates = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAcceptAllCertificates"], false);
                }

                return mAcceptAllCertificates.Value;
            }
        }


        /// <summary>
        /// Iterations count used within <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes"/>. 
        /// More iterations will generate more resistant hashes to brute-force attacks, however performance issues might arise when set to high. 
        /// </summary>
        public static int Pbkdf2IterationsCount
        {
            get
            {
                return mPbkdf2IterationsCount;
            }
            set
            {
                mPbkdf2IterationsCount = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// If some module allows acceptation of untrusted or expired certificate, this method registers certificate validation callback.
        /// </summary>
        public static void EnsureCertificateSecurity()
        {
            if ((StagingAcceptAllCertificates || SchedulerAcceptAllCertificates || AcceptAllCertificates) && !securityCallbackSet)
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;
                securityCallbackSet = true;
            }
        }


        /// <summary>
        /// Returns true if current request should be excluded from checking
        /// Otherwise return real value
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="certificate">Certificate</param>
        /// <param name="chain">Chain</param>
        /// <param name="errors">Errors</param>
        public static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // Get current request
            HttpWebRequest request = sender as HttpWebRequest;
            if (request != null)
            {
                // Scheduler request
                if ((AcceptAllCertificates || SchedulerAcceptAllCertificates) && (request.Address.AbsolutePath.EndsWith("/Scheduler.ashx", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // Staging request 
                if ((AcceptAllCertificates || StagingAcceptAllCertificates) && (request.Address.AbsolutePath.EndsWith("/syncserver.asmx", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            // By default return real status
            return (errors == SslPolicyErrors.None);
        }


        /// <summary>
        /// Gets the SecurityAccessEnum equivalent of the permission information from the given forum access encoded info.
        /// </summary>
        /// <param name="access">Encoded information on access parameters of permissions</param>
        /// <param name="position">Position of the permission within the access parameter</param> 
        public static SecurityAccessEnum GetSecurityAccessEnum(int access, int position)
        {
            if (access > -1)
            {
                return ((SecurityAccessEnum)(GetCurrentAccessValue(access, position)));
            }

            // Action is denied by default
            return SecurityAccessEnum.Nobody;
        }


        /// <summary>
        /// Gets the integer equivalent of the permission information specified by the SecurityAccessEnum.
        /// </summary>
        /// <param name="access">SecurityAccessEnum information of access permission</param>
        /// <param name="securityAccess">Encoded information on access parameters of permissions</param>
        /// <param name="position">Position of the permission within the access parameter</param>
        public static int SetSecurityAccessEnum(int access, SecurityAccessEnum securityAccess, int position)
        {
            int digitPlace = (int)Math.Pow(10, position - 1);
            int currValue = GetCurrentAccessValue(access, position);

            return ((access - (currValue * digitPlace)) + ((int)securityAccess * digitPlace));
        }


        /// <summary>
        /// Returns the value of the permission from the forum access info at specified position.
        /// </summary>
        /// <param name="access">Encoded information on access parameters of permissions</param> 
        /// <param name="position">Position of the permission within the access parameter</param>  
        private static int GetCurrentAccessValue(int access, int position)
        {
            string result = access.ToString();

            if (result.Length >= position)
            {
                string value = result[result.Length - position].ToString();
                if (ValidationHelper.IsInteger(value))
                {
                    return Convert.ToInt32(value);
                }
            }

            return 0;
        }


        /// <summary>
        /// Generates hash for confirmation email which approves certain action (subscription to forum, password change, ...).  
        /// </summary>
        /// <param name="identifier">Request identifier.</param>
        /// <param name="time">Time when request was sent.</param>
        public static string GenerateConfirmationEmailHash(string identifier, DateTime time)
        {
            string dataToHash = identifier + ValidationHelper.HashStringSalt + DateTimeUrlFormatter.Format(time);
            return GetSHA2Hash(dataToHash);
        }


        /// <summary>
        /// Returns whether hash for confirmation email is valid.
        /// </summary>
        /// <param name="hash">Validated hash.</param>
        /// <param name="identifier">Request identifier.</param>
        /// <param name="time">Time in format "ddMMyyyyhhmmss" when request was sent.</param>        
        public static bool ValidateConfirmationEmailHash(string hash, string identifier, DateTime time)
        {
            // Count new hash
            string countedHash = GenerateConfirmationEmailHash(identifier, time);

            // Check whether hashes are the same
            return (hash == countedHash);
        }


        /// <summary>
        /// Returns PBKDF2 hash for password.
        /// <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes"/> implements PBKDF2 with HMACSHA1 (hard coded in the class).
        /// </summary>
        /// <remarks>
        /// Use this method for generating password hashes only. Because this method is by design CPU consuming it generally is not suitable for hashing 
        /// arbitrary input data. The performance/security relation can be adjusted by modifying the <see cref="T:CMS.Helpers.SecurityHelper.Pbkdf2IterationsCount"/> property.
        /// </remarks>
        /// <param name="password">Password to be hashed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
        /// <returns>Base64 encoded hash containing iteration count, salt and derived sub-key.</returns>
        public static string GetPBKDF2Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] subkey;
            byte[] salt;

            using (var deriveBytes = new Rfc2898DeriveBytes(password, PBKDF2_SALT_BYTES, mPbkdf2IterationsCount))
            {
                subkey = deriveBytes.GetBytes(PBKDF2_SUBKEY_BYTES);
                salt = deriveBytes.Salt;
            }

            byte[] iterationCount = BitConverter.GetBytes(mPbkdf2IterationsCount);

            // Output is version header (0x00) + iterations + salt + sub-key
            // Version header is used for potential backwards compatibility 
            byte[] outputBytes = new byte[1 + sizeof(int) + salt.Length + PBKDF2_SUBKEY_BYTES];
            Buffer.BlockCopy(iterationCount, 0, outputBytes, 1, iterationCount.Length);
            Buffer.BlockCopy(salt, 0, outputBytes, 1 + sizeof(int), salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + sizeof(int) + salt.Length, PBKDF2_SUBKEY_BYTES);

            return Convert.ToBase64String(outputBytes);
        }


        /// <summary>
        /// Returns true in case given password matches given PBKDF2 hash. 
        /// </summary>
        /// <param name="password">Password to be hashed and compared with <paramref name="hash"/>.</param>
        /// <param name="hash">Hash to compare hashed <paramref name="password"/> with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> or <paramref name="hash"/> are null.</exception>
        /// <returns>True for hashes generated using <see cref="T:CMS.Helpers.SecurityHelper.GetPBKDF2Hash"/> method (for given password).</returns>
        public static bool VerifyPBKDF2Hash(string password, string hash)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            byte[] hashBytes;

            try
            {
                hashBytes = Convert.FromBase64String(hash);
            }
            catch (FormatException)
            {
                // Given hash was not a base64 encoded string 
                return false;
            }

            if ((hashBytes.Length != (1 + sizeof(int) + PBKDF2_SALT_BYTES + PBKDF2_SUBKEY_BYTES)) || (hashBytes[0] != 0x00))
            {
                // Wrong length or version header
                return false;
            }

            // Get hashing parameters
            byte[] salt = new byte[PBKDF2_SALT_BYTES];
            byte[] iterations = new byte[sizeof(int)];

            Buffer.BlockCopy(hashBytes, 1, iterations, 0, sizeof(int));
            Buffer.BlockCopy(hashBytes, 1 + sizeof(int), salt, 0, PBKDF2_SALT_BYTES);

            // Get sub-key stored in hash
            byte[] storedSubkey = new byte[PBKDF2_SUBKEY_BYTES];
            Buffer.BlockCopy(hashBytes, 1 + sizeof(int) + PBKDF2_SALT_BYTES, storedSubkey, 0, PBKDF2_SUBKEY_BYTES);

            // Generate sub-key using given password and parameters stored in hash 
            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, BitConverter.ToInt32(iterations, 0)))
            {
                generatedSubkey = deriveBytes.GetBytes(PBKDF2_SUBKEY_BYTES);
            }

            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }


        /// <summary>
        /// Returns SHA2 hash for input data.
        /// </summary>
        /// <param name="inputData">Data to by hashed.</param>
        public static string GetSHA2Hash(string inputData)
        {
            using (var algorithm = new SHA256Managed())
            {
                return GetHash(inputData, algorithm);
            }
        }

        
        /// <summary>
        /// Returns HMAC SHA256 hash for input data with key.
        /// </summary>
        /// <param name="inputData">Data to be hashed.</param>
        /// <param name="key">Key on which data will be hashed.</param>
        public static string GetHMACSHA2Hash(string inputData, byte[] key)
        {
            using (var algorithm = new HMACSHA256(key))
            {
                return GetHash(inputData, algorithm);
            }
        }

        
        /// <summary>
        /// Returns the SHA1 hash byte array for given password string.
        /// </summary>
        /// <param name="inputData">Data to be hashed.</param>
        public static string GetSHA1Hash(string inputData)
        {
            using (var algorithm = new SHA1CryptoServiceProvider())
            {
                return GetHash(inputData, algorithm);
            }
        }


        /// <summary>
        /// Returns SHA2 hash for input data.
        /// </summary>
        /// <param name="inputData">Data to by hashed.</param>
        public static string GetMD5Hash(string inputData)
        {
            using (var algorithm = new MD5CryptoServiceProvider())
            {
                return GetHash(inputData, algorithm);
            }
        }


        /// <summary>
        /// Check password policy for specified password
        /// </summary>
        /// <param name="password">Password to check for policy fulfillment</param>
        /// <param name="siteName">Name of site containing policy to be met</param>
        /// <returns>True if policy met, false otherwise</returns>
        public static bool CheckPasswordPolicy(string password, string siteName)
        {
            return CheckPasswordPolicy(password, siteName, CoreServices.Settings[siteName + ".CMSPolicyMinimalLength"].ToInteger(0), CoreServices.Settings[siteName + ".CMSPolicyNumberOfNonAlphaNumChars"].ToInteger(0), CoreServices.Settings[siteName + ".CMSPolicyRegularExpression"].ToString(""));
        }


        /// <summary>
        /// Check password policy for specified password
        /// </summary>
        /// <param name="password">Password to check for policy fulfillment</param>
        /// <param name="siteName">Name of site containing policy to be met</param>
        /// <param name="minLength">Minimum password length</param>
        /// <param name="minNonAlphaNum">Minimal number of non-alpha numeric characters</param>
        /// <param name="regularExpression">Regular expression to be met</param>
        /// <returns>True if policy met, false otherwise</returns>
        public static bool CheckPasswordPolicy(string password, string siteName, int minLength, int minNonAlphaNum, string regularExpression)
        {
            if (CoreServices.Settings[siteName + ".CMSUsePasswordPolicy"].ToBoolean(false))
            {
                // Check minimal length
                if (password.Length < minLength)
                {
                    return false;
                }

                // Check number of non alphanum characters
                int counter = 0;
                foreach (char c in password)
                {
                    if (!Char.IsLetterOrDigit(c))
                    {
                        counter++;
                    }
                }

                if (counter < minNonAlphaNum)
                {
                    return false;
                }

                // Check regular expression
                if (!String.IsNullOrEmpty(regularExpression))
                {
                    Regex regex = RegexHelper.GetRegex(regularExpression);
                    if (!regex.IsMatch(password))
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Returns whether given path is excluded from adding X-Frame-Options HTTP header.
        /// </summary>
        /// <param name="path">Rewritten path or path prefix starting with /.</param>
        /// <param name="url">Original URL including query string.</param>
        public static bool IsXFrameOptionsExcluded(string path, Uri url)
        {
            return new XFrameOptionsExcludedPathDetector().IsExcluded(path, url);
        }


        /// <summary>
        /// Validates if session token is equal given token. In case that session doesn't contain token returns true.
        /// </summary>
        /// <param name="token">Token to validate.</param>
        /// <param name="sessionTokenName">Name of the session token to compare the token value with. If no token name is defined, SESSION_TOKEN_HEADER is used instead.</param>
        public static bool ValidateSessionToken(string token, string sessionTokenName = null)
        {
            // Validate session token
            var sessionToken = SessionHelper.GetValue(string.IsNullOrEmpty(sessionTokenName) ? SESSION_TOKEN_HEADER : sessionTokenName) as string;
            if (!String.IsNullOrEmpty(sessionToken))
            {
                return (token == sessionToken);
            }

            return true;
        }


        /// <summary>
        /// Gets the time period when client should contact server to check ScreenLock state.
        /// </summary>
        /// <param name="sitename">Site name.</param>
        public static int GetSecondsToShowScreenLockAction(string sitename)
        {
            TimeSpan secondsToLock = TimeSpan.FromMinutes(CoreServices.Settings[sitename + ".CMSScreenLockInterval"].ToInteger(0));
            TimeSpan secondsToWarning = TimeSpan.FromSeconds(CoreServices.Settings[sitename + ".CMSScreenLockWarningInterval"].ToInteger(0));

            int secs = (int)(secondsToLock - secondsToWarning).TotalSeconds;

            return (secs > 0) ? secs : 0;
        }


        /// <summary>
        /// Logs the call of this method as request for ScreenLock feature.
        /// </summary>
        public static void LogScreenLockAction()
        {
            bool locked = (SessionHelper.GetValue("ScreenLockIsLocked") != null) && (bool)SessionHelper.GetValue("ScreenLockIsLocked");

            // Log ScreenLock action if screen is not locked
            if (!locked)
            {
                SessionHelper.SetValue("ScreenLockLastRequest", DateTime.Now);
            }
        }


        /// <summary>
        /// Tries to parse Authorization header (Basic Authentication). Retrieves both username and password from header if parsing succeeded.
        /// </summary>
        /// <param name="authorizationHeader">HTTP Authorization header</param>
        /// <param name="username">Is set to username from the header if parsing was successful</param>
        /// <param name="password">Is set to password from the header if parsing was successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParseBasicAuthorizationHeader(string authorizationHeader, out string username, out string password)
        {
            username = null;
            password = null;

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return false;
            }

            authorizationHeader = authorizationHeader.Trim();

            var authorizationParts = authorizationHeader.Split(' ');
            if (authorizationParts.Length == 2 && authorizationParts[0].Equals("basic", StringComparison.OrdinalIgnoreCase))
            {
                var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(authorizationParts[1]));
                var firstColonIndex = decoded.IndexOf(':');
                if (firstColonIndex > 0)
                {
                    username = decoded.Substring(0, firstColonIndex);
                    password = decoded.Substring(firstColonIndex + 1, decoded.Length - firstColonIndex - 1);

                    return true;
                }
            }

            return false;
        }

        #endregion


        #region "Methods for retrieving settings"

        /// <summary>
        /// Returns whether Autocomplete is enabled for login usernames.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static bool IsAutoCompleteEnabledForLogin(string siteName)
        {
            return CoreServices.Settings[siteName + ".CMSAutocompleteEnableForLogin"].ToBoolean(false);
        }


        /// <summary>
        /// Returns whether ScreenLock feature is enabled for given site.
        /// </summary>
        /// <param name="sitename">Site name.</param>
        public static bool IsScreenLockEnabled(string sitename)
        {
            var enabled = CoreServices.Settings[sitename + ".CMSScreenLockEnabled"].ToBoolean(false);
            var wifEnabled = CoreServices.Settings[sitename + ".CMSWIFEnabled"].ToBoolean(false);

            return enabled && !wifEnabled;
        }

        #endregion


        #region "SQL query methods"

        /// <summary>
        /// Adds protection against clickjacking.
        /// </summary>
        public static void HandleClickjacking()
        {
            if (!IsXFrameOptionsExcluded(RequestContext.CurrentRelativePath, RequestContext.URL))
            {
                var context = CMSHttpContext.Current;
                if (context != null)
                {
                    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns hash of given data in given type.
        /// </summary>
        /// <param name="inputData">Input data.</param>
        /// <param name="algorithm">Algorithm to use.</param>
        private static string GetHash(string inputData, HashAlgorithm algorithm)
        {
            // Convert to bytes
            byte[] dataToHash = Encoding.UTF8.GetBytes(inputData);

            // Compute
            byte[] hashedData = algorithm.ComputeHash(dataToHash);

            // Convert hash back to a string value.
            return ValidationHelper.GetStringFromHash(hashedData);
        }


        /// <summary>
        /// Compares two byte arrays.
        /// </summary>
        /// <param name="first">First byte array.</param>
        /// <param name="second">Second byte array.</param>
        private static bool ByteArraysEqual(byte[] first, byte[] second)
        {
            if ((first == null) || (second == null) || (first.Length != second.Length))
            {
                return false;
            }

            return first.SequenceEqual(second);
        }

        #endregion
    }
}