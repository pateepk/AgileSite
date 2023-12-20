using System;
using System.Linq;
using System.Security.Cryptography;

namespace CMS.Membership
{
    /// <summary>
    /// TOTP implementation providing setup codes, OTP verification and OTP generation.
    /// </summary>
    /// <remarks>
    /// Setup codes are encoded using base32 without padding. 
    /// Time to start counting time steps is 0 (Unix epoch) and the time step is 30 seconds. 
    /// </remarks>
    internal static class Totp
    {
        internal static TotpStepCounter StepCounter
        {
            get;
            set;
        } = new TotpStepCounter();


        /// <summary>
        /// Returns a manual setup code for the user. This code is used to synchronize the second factor (Google Authenticator, Authy, ...) with the web application instance.
        /// </summary>
        /// <param name="accountSecretKey">The secret used in TOTP generation.</param>
        public static string GetSetupCode(byte[] accountSecretKey)
        {
            if (accountSecretKey == null || !accountSecretKey.Any())
            {
                throw new ArgumentException(nameof(accountSecretKey));
            }

            return Base32.Encode(accountSecretKey);
        }


        /// <summary>
        /// Checks whether the given OTP is valid for the given secret and time tolerance. 
        /// </summary>
        /// <param name="accountSecretKey">The secret used in TOTP generation.</param>
        /// <param name="twoFactorCodeFromClient">The code that should be validated.</param>
        /// <param name="timeTolerance">Defines a time interval to address clock drift issues.</param>
        /// <param name="hmac">Authenticated hash function used in TOTP generation.</param>
        /// <param name="digits">Length of the TOTP.</param>
        /// <param name="timeStep">Time step of valid OTP. Can be used to prevent OTP reuse.</param>
        public static bool ValidateOtp(byte[] accountSecretKey, string twoFactorCodeFromClient, TimeSpan timeTolerance, out long timeStep, Func<HMAC> hmac = null, int digits = 6)
        {
            timeStep = -1;

            if (accountSecretKey == null || !accountSecretKey.Any() || String.IsNullOrEmpty(twoFactorCodeFromClient))
            {
                return false;
            }

            timeStep = FindMatchingOtpTimeStep(accountSecretKey, twoFactorCodeFromClient, timeTolerance, hmac, digits);

            return timeStep != -1;
        }


        /// <summary>
        /// Returns the current OTP for the current time and the given secret. 
        /// </summary>
        /// <param name="accountSecretKey">The secret used in TOTP generation.</param>
        /// <param name="hmac">Authenticated hash function used in TOTP generation.</param>
        /// <param name="digits">Length of the TOTP.</param>
        public static string GetCurrentOtp(byte[] accountSecretKey, Func<HMAC> hmac = null, int digits = 6)
        {
            if (accountSecretKey == null || !accountSecretKey.Any())
            {
                throw new ArgumentException(nameof(accountSecretKey));
            }

            return GenerateHashedCode(accountSecretKey, StepCounter.GetUtcNowStepNumber(), hmac, digits);
        }


        /// <summary>
        /// Finds a time step of an OTP matching given <paramref name="twoFactorCodeFromClient"/> with respect to <paramref name="timeTolerance"/>.
        /// </summary>
        /// <returns>Returns the time step of matching OTP if found. Returns -1 otherwise.</returns>
        private static long FindMatchingOtpTimeStep(byte[] accountSecretKey, string twoFactorCodeFromClient, TimeSpan timeTolerance, Func<HMAC> hmac, int digits)
        {
            long iterationCounter = StepCounter.GetUtcNowStepNumber();
            int iterationOffset = 0;

            if (timeTolerance.TotalSeconds > StepCounter.StepLengthSeconds)
            {
                iterationOffset = Convert.ToInt32(timeTolerance.TotalSeconds / (double)StepCounter.StepLengthSeconds);
            }

            long iterationStart = iterationCounter - iterationOffset;
            long iterationEnd = iterationCounter + iterationOffset;

            for (long stepNumber = iterationStart; stepNumber <= iterationEnd; stepNumber++)
            {
                if (GenerateHashedCode(accountSecretKey, stepNumber, hmac, digits).Equals(twoFactorCodeFromClient, StringComparison.OrdinalIgnoreCase))
                {
                    return stepNumber;
                }
            }

            return -1;
        }


        /// <summary>
        /// Generates a OTP according to RFC4226.
        /// </summary>
        internal static string GenerateHashedCode(byte[] key, long iterationNumber, Func<HMAC> hmac, int digits)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }

            byte[] hash;

            if (hmac == null)
            {
                hmac = HmacFactories.HMACSHA1;
            }

            using (var hmacAlgorithm = hmac()) {
                hmacAlgorithm.Key = key;
                hash = hmacAlgorithm.ComputeHash(counter);
            }               

            int offset = hash[hash.Length - 1] & 0xf;

            // Convert the 4 bytes into an integer, ignoring the sign.
            int binary =
                ((hash[offset] & 0x7f) << 24)
                | (hash[offset + 1] << 16)
                | (hash[offset + 2] << 8)
                | (hash[offset + 3]);

            int password = binary % (int)Math.Pow(10, digits);
            return password.ToString(new string('0', digits));
        }
    }
}
