using System;
using System.Web;
using System.Web.Security;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Provides support for safely storing and restoring user information in cookies
    /// </summary>
    internal static class ImpersonationHelper
    {
        /// <summary>
        /// Cookie key for impersonated user
        /// </summary>
        internal const string IMPERSONATION_KEY = CookieName.Impersonation;

        /// <summary>
        /// MachineKey protection purpose
        /// </summary>
        private const string PURPOSE = "User impersonation";


        /// <summary>
        /// Returns data object from cookie with given name
        /// </summary>
        public static ImpersonationCookieData GetDataFromCookie()
        {
            var cookieData = CookieHelper.GetValue(IMPERSONATION_KEY);
            return DecodeData(cookieData);
        }


        /// <summary>
        /// Encodes given data object to string representation
        /// </summary>
        internal static string EncodeData(ImpersonationCookieData data)
        {
            var bytes = Serialize(data);
            var protectedBytes = MachineKey.Protect(bytes, PURPOSE);
            return Convert.ToBase64String(protectedBytes);
        }


        /// <summary>
        /// Decodes data object from string representation
        /// </summary>
        internal static ImpersonationCookieData DecodeData(string encoded)
        {
            if (!string.IsNullOrEmpty(encoded))
            {
                try
                {
                    var protectedBytes = Convert.FromBase64String(encoded);
                    var bytes = MachineKey.Unprotect(protectedBytes, PURPOSE);
                    return Deserialize(bytes);
                }
                catch (Exception e)
                {
                    EventLogProvider.LogEvent(EventType.WARNING, IMPERSONATION_KEY, "CookieDecode", e.Message);
                }
            }

            return new ImpersonationCookieData(Guid.Empty, Guid.Empty);
        }


        /// <summary>
        /// Serializes <see cref="ImpersonationCookieData.ImpersonatedUserId"/> and <see cref="ImpersonationCookieData.OriginalUserId"/> of <paramref name="data"/> to byte array.
        /// </summary>
        private static byte[] Serialize(ImpersonationCookieData data)
        {
            byte[] result = new byte[32];
            Buffer.BlockCopy(data.ImpersonatedUserId.ToByteArray(), 0, result, 0, 16);
            Buffer.BlockCopy(data.OriginalUserId.ToByteArray(), 0, result, 16, 16);

            return result;
        }


        /// <summary>
        /// De-serializes <see cref="ImpersonationCookieData.ImpersonatedUserId"/> and <see cref="ImpersonationCookieData.OriginalUserId"/> from <paramref name="bytes"/>.
        /// </summary>
        private static ImpersonationCookieData Deserialize(byte[] bytes)
        {
            byte[] impersonatedUserIdBytes = new byte[16];
            byte[] originalUserIdBytes = new byte[16];
            Buffer.BlockCopy(bytes, 0, impersonatedUserIdBytes, 0, 16);
            Buffer.BlockCopy(bytes, 16, originalUserIdBytes, 0, 16);

            return new ImpersonationCookieData(new Guid(originalUserIdBytes), new Guid(impersonatedUserIdBytes));
        }


        /// <summary>
        /// Returns impersonation cookie
        /// </summary>
        /// <returns></returns>
        public static HttpCookie GetCookie()
        {
            return CookieHelper.GetExistingCookie(IMPERSONATION_KEY);
        }


        /// <summary>
        /// Sets impersonation cookie data for given users
        /// </summary>
        public static void SetCookie(IUserInfo impersonatedUser, IUserInfo currentUser)
        {
            if (impersonatedUser == null)
            {
                throw new ArgumentNullException(nameof(impersonatedUser));
            }

            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            var data = new ImpersonationCookieData(currentUser.UserGUID, impersonatedUser.UserGUID);
            CookieHelper.SetValue(IMPERSONATION_KEY, EncodeData(data), DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Removes impersonation cookie data
        /// </summary>
        public static void RemoveCookie()
        {
            CookieHelper.Remove(IMPERSONATION_KEY);
        }
    }
}
