using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Chat
{
    /// <summary>
    /// Chat flood protector. Holds info about last actions in memory -> works only in one-server solution.
    /// </summary>
    public class ChatFloodProtector
    {
        #region "Private fields"

        private Dictionary<int, ChatFloodProtectorUser> floodProtectorUsers = new Dictionary<int, ChatFloodProtectorUser>();

        #endregion


        #region "Private properties"

        private ChatFloodProtectorUser this[int chatUserId]
        {
            get
            {
                if (!floodProtectorUsers.ContainsKey(chatUserId))
                {
                    floodProtectorUsers.Add(chatUserId, new ChatFloodProtectorUser());
                }
                return floodProtectorUsers[chatUserId];
            }
        }

        #endregion


        #region "Private static methods"

        private static double GetOperationProtectionInterval(FloodOperationEnum operation)
        {
            switch (operation)
            {
                case FloodOperationEnum.CreateRoom:
                    return ChatSettingsProvider.CreateRoomFloodProtectionIntervalSetting;
                case FloodOperationEnum.JoinRoom:
                    return ChatSettingsProvider.JoinRoomFloodProtectionIntervalSetting;
                case FloodOperationEnum.PostMessage:
                    return ChatSettingsProvider.PostMessageFloodProtectionIntervalSetting;
                case FloodOperationEnum.ChangeNickname:
                    return ChatSettingsProvider.ChangeNicknameFloodProtectionIntervalSetting;
            }
            return 0;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks specified operation for flooding. One user can perform an operation only once in the interval specified in settings.
        /// </summary>
        /// <param name="chatUserID">Unique identifier of the chat user.</param>
        /// <param name="operation">Operation performed.</param>
        /// <returns>True if user can perform the operation. False if it is called to early.</returns>
        public bool CheckOperation(int chatUserID, FloodOperationEnum operation)
        {
            return this[chatUserID].CheckOperation(operation);
        }

        #endregion


        #region "Nested classes"

        private class ChatFloodProtectorUser
        {
            #region "Private fields"

            private Dictionary<FloodOperationEnum, DateTime> lastOperationExecutions = new Dictionary<FloodOperationEnum, DateTime>();

            #endregion


            #region "Private properties"

            private DateTime this[FloodOperationEnum operation]
            {
                get
                {
                    if (!lastOperationExecutions.ContainsKey(operation))
                    {
                        lastOperationExecutions.Add(operation, DateTime.MinValue);
                    }
                    return lastOperationExecutions[operation];
                }
            }

            #endregion


            #region "Public methods"

            /// <summary>
            /// Checks if user has performed this operation too early.
            /// </summary>
            /// <param name="operation">Type of operation</param>
            /// <returns>True if everything is ok, false if flooding has occured.</returns>
            public bool CheckOperation(FloodOperationEnum operation)
            {
                double operationProtectionInterval = GetOperationProtectionInterval(operation);

                // If protection interval is 0 flood protection is disabled
                if (operationProtectionInterval <= 0)
                {
                    return true;
                }

                // Check if last operation was "recently"
                if (this[operation].AddSeconds(operationProtectionInterval) > DateTime.Now)
                {
                    lastOperationExecutions[operation] = DateTime.Now;
                    return false;
                }

                // Log last operation execution time
                lastOperationExecutions[operation] = DateTime.Now;

                return true;
            }

            #endregion
        }

        #endregion
    }
}
