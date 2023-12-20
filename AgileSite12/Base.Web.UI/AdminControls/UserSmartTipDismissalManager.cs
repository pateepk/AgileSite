using System;
using System.Collections.Generic;

using CMS.Membership;

using Newtonsoft.Json;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class that helps managing user's dismissed smart tips.
    /// </summary>
    public class UserSmartTipDismissalManager
    {
        private readonly UserInfo mUser;
        private HashSet<string> mUserDismissedSmartTips;


        private HashSet<string> UserDismissedSmartTips
        {
            get
            {
                return mUserDismissedSmartTips ?? (mUserDismissedSmartTips = LoadUserDismissedSmartTips());
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">User</param>
        public UserSmartTipDismissalManager(UserInfo user)
        {
            mUser = user ?? throw new ArgumentNullException(nameof(user));
        }


        /// <summary>
        /// Toggles specified smart tip's state. 
        /// If smart tip is dismissed, removes it from dismissed list.
        /// If not, smart tip is added to dismissed list.
        /// </summary>
        /// <param name="uniqueIdentifier">Unique identifier of the smart tip</param>
        public void ToggleSmartTipState(string uniqueIdentifier)
        {
            if (String.IsNullOrEmpty(uniqueIdentifier))
            {
                throw new ArgumentException("[UserSmartTipDismissalManager.ToggleSmartTipState]: Unique identifier of the smart tip must be defined", "uniqueIdentifier");
            }

            if (UserDismissedSmartTips.Contains(uniqueIdentifier))
            {
                UserDismissedSmartTips.Remove(uniqueIdentifier);
            }
            else
            {
                UserDismissedSmartTips.Add(uniqueIdentifier);
            }

            SaveUserDismissedSmartTips();
        }


        /// <summary>
        /// Indicates whether is specified smart tip dismissed. 
        /// </summary>
        /// <param name="uniqueIdentifier">Unique identifier of the smart tip</param>
        public bool IsSmartTipDismissed(string uniqueIdentifier)
        {
            if (String.IsNullOrEmpty(uniqueIdentifier))
            {
                throw new ArgumentException("[UserSmartTipDismissalManager.IsSmartTipDismissed]: Unique identifier of the smart tip must be defined", "uniqueIdentifier");
            }

            return UserDismissedSmartTips.Contains(uniqueIdentifier);
        }


        /// <summary>
        /// Loads user dismissed smart tips from persistent storage.
        /// </summary>
        private HashSet<string> LoadUserDismissedSmartTips()
        {
            try
            {
                return JsonConvert.DeserializeObject<HashSet<string>>(mUser.UserSettings.UserDismissedSmartTips) ?? new HashSet<string>();
            }
            catch (JsonException)
            {
                return new HashSet<string>();
            }
        }


        /// <summary>
        /// Saves user dismissed smart tips to persistent storage.
        /// </summary>
        private void SaveUserDismissedSmartTips()
        {
            var userInfo = UserInfoProvider.GetUserInfo(mUser.UserID);
            if (userInfo != null)
            {
                userInfo.UserSettings.UserDismissedSmartTips = JsonConvert.SerializeObject(UserDismissedSmartTips, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
                UserInfoProvider.SetUserInfo(userInfo);
            }
        }
    }
}