using System.Linq;

using CMS.DataEngine;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatPopupWindowSettings management.
    /// </summary>
    public class ChatPopupWindowSettingsInfoProvider : AbstractInfoProvider<ChatPopupWindowSettingsInfo, ChatPopupWindowSettingsInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat popup window settings.
        /// </summary>
        public static ObjectQuery<ChatPopupWindowSettingsInfo> GetChatPopupWindowSettings()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns popup window settings with specified ID.
        /// </summary>
        /// <param name="settingsId">Popup window settings ID.</param>        
        public static ChatPopupWindowSettingsInfo GetChatPopupWindowSettings(int settingsId)
        {
            return ProviderObject.GetInfoById(settingsId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified popup window settings.
        /// </summary>
        /// <param name="settingsObj">Popup window settings to be set.</param>
        public static void SetChatPopupWindowSettings(ChatPopupWindowSettingsInfo settingsObj)
        {
            ProviderObject.SetInfo(settingsObj);
        }


        /// <summary>
        /// Deletes specified popup window settings.
        /// </summary>
        /// <param name="settingsObj">Popup window settings to be deleted.</param>
        public static void DeleteChatPopupWindowSettings(ChatPopupWindowSettingsInfo settingsObj)
        {
            ProviderObject.DeleteInfo(settingsObj);
        }


        /// <summary>
        /// Deletes popup window settings with specified ID.
        /// </summary>
        /// <param name="settingsId">Popup window settings ID.</param>
        public static void DeleteChatPopupWindowSettings(int settingsId)
        {
            var settingsObj = GetChatPopupWindowSettings(settingsId);
            DeleteChatPopupWindowSettings(settingsObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets popup window settings by hash code.
        /// </summary>
        /// <param name="hashCode">Hash code</param>
        public static ChatPopupWindowSettingsInfo GetByHashCode(int hashCode)
        {
            return GetChatPopupWindowSettings().WhereEquals("ChatPopupWindowSettingsHashCode", hashCode).FirstOrDefault();
        }


        /// <summary>
        /// Stores popup window settings under specified hash code.
        /// </summary>
        /// <param name="hashCode">Hash code of this settings.</param>
        /// <param name="messageTrans">Message transformation setting</param>
        /// <param name="userTrans">User transformation setting</param>
        /// <param name="errorTrans">Error transformation setting</param>
        /// <param name="errorClearTrans">Error clear transformation setting</param>
        /// <returns>Stored settings</returns>
        public static ChatPopupWindowSettingsInfo Store(int hashCode, string messageTrans, string userTrans, string errorTrans, string errorClearTrans)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@HashCode", hashCode);
            parameters.Add("@MessageTransformationName", messageTrans);
            parameters.Add("@ErrorTransformationName", errorTrans);
            parameters.Add("@ErrorClearTransformationName", errorClearTrans);
            parameters.Add("@UserTransformationName", userTrans);

            return new ChatPopupWindowSettingsInfo(ConnectionHelper.ExecuteQuery("Chat.PopupWindowSettings.Store", parameters).Tables[0].Rows[0]);
        }

        #endregion
    }
}
