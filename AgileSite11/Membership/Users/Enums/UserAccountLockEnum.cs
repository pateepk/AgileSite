using CMS.Base;

namespace CMS.Membership
{
    #region "User account lock enum"

    /// <summary>
    /// Enum describing reason for locking user account
    /// </summary>
    public enum UserAccountLockEnum : int
    {
        /// <summary>
        /// User account was disabled manually.
        /// </summary>
        DisabledManually = 0,

        /// <summary>
        /// User account was disabled because his password expired.
        /// </summary>
        PasswordExpired = 1,

        /// <summary>
        /// User account was disabled because user reach maximum invalid logon attempts.
        /// </summary>
        MaximumInvalidLogonAttemptsReached = 2
    }

    #endregion


    #region  "User account lock code"

    /// <summary>
    /// Page mode code.
    /// </summary>
    public static class UserAccountLockCode
    {
        #region "Constants"

        /// <summary>
        /// User account was disabled manually.
        /// </summary>
        public const int DisabledManually = 0;

        /// <summary>
        /// User account was disabled because his password expired.
        /// </summary>
        public const int PasswordExpired = 1;

        /// <summary>
        /// User account was disabled because user reach maximum invalid logon attempts.
        /// </summary>
        public const int MaximumInvalidLogonAttemptsReached = 2;

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the user account lock enumeration from the string value.
        /// </summary>
        /// <param name="userAccountLock">User account lock string</param>
        public static UserAccountLockEnum GetLockEnumFromString(string userAccountLock)
        {
            switch (userAccountLock.ToLowerCSafe())
            {
                case "disabledmanually":
                    return UserAccountLockEnum.DisabledManually;

                case "passwordexpired":
                    return UserAccountLockEnum.PasswordExpired;

                case "maximuminvalidlogonattemptsreached":
                    return UserAccountLockEnum.MaximumInvalidLogonAttemptsReached;
            }

            return UserAccountLockEnum.DisabledManually;
        }


        /// <summary>
        /// Returns the enumeration representation of the user account lock code.
        /// </summary>
        /// <param name="code">User account lock code</param>
        public static UserAccountLockEnum ToEnum(int code)
        {
            switch (code)
            {
                case DisabledManually:
                    return UserAccountLockEnum.DisabledManually;

                case PasswordExpired:
                    return UserAccountLockEnum.PasswordExpired;

                case MaximumInvalidLogonAttemptsReached:
                    return UserAccountLockEnum.MaximumInvalidLogonAttemptsReached;

                default:
                    return UserAccountLockEnum.DisabledManually;
            }
        }


        /// <summary>
        /// Returns the user account lock code from the enumeration value.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static int FromEnum(UserAccountLockEnum value)
        {
            switch (value)
            {
                case UserAccountLockEnum.DisabledManually:
                    return DisabledManually;

                case UserAccountLockEnum.PasswordExpired:
                    return PasswordExpired;

                case UserAccountLockEnum.MaximumInvalidLogonAttemptsReached:
                    return MaximumInvalidLogonAttemptsReached;

                default:
                    return DisabledManually;
            }
        }

        #endregion
    }

    #endregion
}
