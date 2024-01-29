using System;
using System.Text;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Encapsulates identity option for macros.
    /// </summary>
    /// <remarks>
    /// When both <see cref="IdentityName"/> and <see cref="UserName"/> are specified, the identity name has precedence.
    /// </remarks>
    [Serializable]
    public class MacroIdentityOption
    {
        /// <summary>
        /// Name of macro identity to be used when signing by macro identity is desired.
        /// </summary>
        /// <seealso cref="MacroIdentityInfo"/>
        public string IdentityName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of user to be used when signing by user is desired.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether <paramref name="identityOption"/> is null or both its <see cref="IdentityName"/> and <see cref="UserName"/> properties are null or empty.
        /// </summary>
        /// <param name="identityOption">Identity option to be checked for null or emptiness.</param>
        /// <returns>Returns true when <paramref name="identityOption"/> is null or when both its identity name and user name are null or empty strings.</returns>
        public static bool IsNullOrEmpty(MacroIdentityOption identityOption)
        {
            return identityOption == null || (String.IsNullOrEmpty(identityOption.IdentityName) && String.IsNullOrEmpty(identityOption.UserName));
        }
        

        /// <summary>
        /// Gets effective user of macro identity identified by <see cref="IdentityName"/> when its value is provided.
        /// Otherwise gets user identified by <see cref="UserName"/>.
        /// </summary>
        /// <returns>Returns user to be used when evaluating a macro, or null.</returns>
        /// <remarks>
        /// This method returns null when <see cref="IdentityName"/> is provided, but the corresponding signature either does not exists or its <see cref="MacroIdentityInfo.MacroIdentityEffectiveUserID"/>
        /// is not set.
        /// </remarks>
        public IUserInfo GetEffectiveUser()
        {
            if (!String.IsNullOrEmpty(IdentityName))
            {
                var identityInfo = MacroIdentityInfoProvider.GetMacroIdentityInfo(IdentityName);

                return identityInfo != null ? (IUserInfo)ProviderHelper.GetInfoById(PredefinedObjectType.USER, identityInfo.MacroIdentityEffectiveUserID) : null;
            }

            return (IUserInfo)ProviderHelper.GetInfoByName(PredefinedObjectType.USER, UserName);
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            if (!String.IsNullOrEmpty(IdentityName))
            {
                res.Append("(identity)").Append(IdentityName);
            }
            if (!String.IsNullOrEmpty(UserName))
            {
                res.Append("(user)").Append(UserName);
            }

            return res.ToString();
        }


        /// <summary>
        /// Creates a new identity option from user info.
        /// </summary>
        public static MacroIdentityOption FromUserInfo(IUserInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new ArgumentNullException(nameof(userInfo));
            }

            var userMacroIdentityInfo = UserMacroIdentityInfoProvider.GetUserMacroIdentityInfo(userInfo);
            var macroIdentityInfo = MacroIdentityInfoProvider.GetMacroIdentityInfo(userMacroIdentityInfo?.UserMacroIdentityMacroIdentityID ?? 0);

            return new MacroIdentityOption
            {
                IdentityName = macroIdentityInfo?.MacroIdentityName,
                UserName = userInfo.UserName
            };
        }
    }
}
