using System;
using System.Security.Cryptography;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Helper for multi-factor authentication 
    /// </summary>
    public class MFAuthenticationHelper : AbstractHelper<MFAuthenticationHelper>
    {
        private TimeSpan mClockDriftTolerance = TimeSpan.FromMinutes(5);
        private int mPasscodeLength = 6;
        private Func<HMAC> mHashAlgorithm = HmacFactories.HMACSHA1;

        /// <summary>
        /// Indicates whether MFA is forced. 
        /// </summary>
        public static bool IsMultiFactorAuthRequired
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(".CMSMFRequired");
            }
        }


        /// <summary>
        /// Indicates whether MFA is enabled.
        /// </summary>
        public static bool IsMultiFactorAuthEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(".CMSMFEnabled");
            }
        }


        /// <summary>
        /// Indicates whether the setup code is showed to the user after enabling MFA.
        /// </summary>
        public static bool DisplaySetupCode
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(".CMSMFDisplayInitToken");
            }
        }


        /// <summary>
        /// Tolerated clock drift between client and server. 
        /// </summary>
        /// <remarks>The default value is 5 minutes.</remarks>
        protected virtual TimeSpan ClockDriftTolerance
        {
            get
            {
                return mClockDriftTolerance;
            }
        }


        /// <summary>
        /// Length of the passcode.
        /// </summary>
        /// <remarks>The default value is 6.</remarks>
        protected virtual int PasscodeLength
        {
            get
            {
                return mPasscodeLength;
            }
        }


        /// <summary>
        /// Underlying hash algorithm used by the passcode generator.
        /// </summary>
        /// <remarks>
        /// Default algorithm used is <see cref="HMACSHA1"/>.
        /// As per RFC 6238 <see cref="HMACSHA256"/> and <see cref="HMACSHA512"/> may be used.
        /// </remarks>
        protected virtual Func<HMAC> HashAlgorithm
        {
            get
            {
                return mHashAlgorithm;
            }
        }


        /// <summary>
        /// Resets the user's current MFA information. 
        /// </summary>
        /// <remarks>The user's authenticator has to be resynchronized when logging in again.</remarks>
        /// <param name="user">UserInfo whose MFA secret will be reset.</param>
        public static void ResetSecretForUser(UserInfo user)
        {
            HelperObject.ResetSecretForUserInternal(user);
        }


        /// <summary>
        /// Checks whether the entered passcode is valid.
        /// </summary>
        /// <param name="user">User who is being authenticated.</param>
        /// <param name="passcode">Passcode entered by the user.</param>
        /// <returns>True if passcode is valid. False otherwise.</returns>
        public static bool IsPasscodeValid(UserInfo user, string passcode)
        {
            return HelperObject.IsPasscodeValidInternal(user, passcode);
        }


        /// <summary>
        /// Fires the <see cref="SecurityEvents.MultiFactorAuthenticate"/> event.
        /// Generates a new secret in case the user has none. 
        /// </summary>
        /// <param name="username">User who is being authenticated.</param>
        public static void IssuePasscode(string username)
        {
            HelperObject.IssuePasscodeInternal(username);
        }


        /// <summary>
        /// Returns the setup code that should be used to synchronize with the user's authenticator.
        /// </summary>
        /// <param name="username">Username of user who is being authenticated.</param>
        public static string GetSetupCodeForUser(string username)
        {
            return HelperObject.GetSetupCodeForUserInternal(username);
        }


        /// <summary>
        /// Indicates whether MFA is required for the given user.
        /// </summary>
        /// <param name="username">Username of user to check.</param>
        public static bool IsMultiFactorRequiredForUser(string username)
        {
            return HelperObject.IsMultiFactorRequiredForUserInternal(username);
        }


        /// <summary>
        /// Resets the user's current MFA information. 
        /// </summary>
        /// <remarks>The user's authenticator has to be resynchronized when logging in again.</remarks>
        /// <param name="user">UserInfo whose MFA secret will be reset.</param>
        protected virtual void ResetSecretForUserInternal(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserMFSecret = null;
            user.UserMFTimestep = 0;
            UserInfoProvider.SetUserInfo(user);
        }


        /// <summary>
        /// Checks whether the entered passcode is valid.
        /// </summary>
        /// <param name="user">User who is being authenticated.</param>
        /// <param name="passcode">Passcode entered by the user.</param>
        /// <returns>True if passcode is valid. False otherwise.</returns>
        protected virtual bool IsPasscodeValidInternal(UserInfo user, string passcode)
        {
            if ((user == null) || !user.Enabled || string.IsNullOrEmpty(passcode))
            {
                return false;
            }

            byte[] secret;

            if (!TryGetUserSecret(user, out secret) || secret == null)
            {
                return false;
            }

            long timestep;

            var valid = Totp.ValidateOtp(secret, passcode, ClockDriftTolerance, out timestep, HashAlgorithm, PasscodeLength);
            if (valid && timestep > user.UserMFTimestep)
            {
                user.UserMFTimestep = timestep;
                UserInfoProvider.SetUserInfo(user);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Fires the <see cref="SecurityEvents.MultiFactorAuthenticate"/> event.
        /// Generates a new secret in case the user has none. 
        /// </summary>
        /// <param name="username">User who is being authenticated.</param>
        protected virtual void IssuePasscodeInternal(string username)
        {
            var userInfo = UserInfoProvider.GetUserInfo(username);
            if (userInfo == null)
            {
                return;
            }

            byte[] secret;

            if (!TryGetUserSecret(userInfo, out secret))
            {
                return;
            }

            if (secret == null)
            {
                secret = GenerateSecret();
                userInfo.UserMFSecret = secret;
                UserInfoProvider.SetUserInfo(userInfo);
                MembershipContext.MFAuthenticationTokenNotInitialized = true;
            }

            // Initiate the MultiFactorAuthentication event
            string validPasscode = Totp.GetCurrentOtp(secret, HashAlgorithm, PasscodeLength);
            SecurityEvents.MultiFactorAuthenticate.StartEvent(ref userInfo, username, passcode: validPasscode);
        }


        /// <summary>
        /// Returns the setup code that should be used to synchronize with the user's authenticator.
        /// </summary>
        /// <param name="username">Username of user who is being authenticated.</param>
        protected virtual string GetSetupCodeForUserInternal(string username)
        {
            var ui = UserInfoProvider.GetUserInfo(username);
            if (ui != null)
            {
                byte[] secret;

                return (TryGetUserSecret(ui, out secret) && secret != null) ? Totp.GetSetupCode(secret) : string.Empty;
            }

            return null;
        }


        /// <summary>
        /// Indicates whether MFA is required for the given user.
        /// </summary>
        /// <param name="username">Username of user to check.</param>
        protected virtual bool IsMultiFactorRequiredForUserInternal(string username)
        {
            bool userRequireMFa = false;
            var ui = UserInfoProvider.GetUserInfo(username);
            if (ui != null)
            {
                userRequireMFa = ui.UserMFRequired;
            }
            return (userRequireMFa || IsMultiFactorAuthRequired) && IsMultiFactorAuthEnabled;
        }


        /// <summary>
        /// Generates a secret to use in passcode generation. 
        /// </summary>
        internal static byte[] GenerateSecret()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                // 15 bytes get encoded to 24 base32 characters -> indicates no padding
                var buffer = new byte[15];
                rng.GetBytes(buffer);

                return buffer;
            }
        }


        // Returns true if the user has no token or a valid token, false in case the token is malformed.
        private static bool TryGetUserSecret(UserInfo user, out byte[] secret)
        {
            secret = null;
            
            if (user == null)
            {
                return false;
            }           

            try
            {
                secret = user.UserMFSecret;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MF Authentication", "MALFORMEDSECRET", ex, SiteContext.CurrentSiteID, $"User '{user.UserName}' has a malformed MF secret.");

                return false;    
            }

            return true;
        }
    }
}