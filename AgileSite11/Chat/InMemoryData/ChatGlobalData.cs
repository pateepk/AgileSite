using System;

namespace CMS.Chat
{
    /// <summary>
    /// Singleton class which holds data needed by chat. It contains instances of other classes to split responsibilities between classes.
    /// </summary>
    public sealed class ChatGlobalData
    {
        #region "Singleton members"

        /// <summary>
        /// Instance.
        /// </summary>
        private static volatile ChatGlobalData mInstance;
        private static readonly object chatGlobalDataLock = new object();


        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ChatGlobalData Instance
        {
            get
            {
                // Lazy thread-safe instantiation
                if (mInstance == null)
                {
                    lock (chatGlobalDataLock)
                    {
                        if (mInstance == null)
                        {
                            mInstance = new ChatGlobalData();
                        }
                    }
                }
                return mInstance;
            }
        }


        /// <summary>
        /// Private constructor
        /// </summary>
        private ChatGlobalData() { }

        #endregion


        #region "Private fields"

        /// <summary>
        /// Name of this class.
        /// </summary>
        private const string UNIQUE_NAME = "ChatGlobalData";

        #endregion


        #region "Sites"

        private GlobalSites globalSites = new GlobalSites(UNIQUE_NAME);


        /// <summary>
        /// Gets instance of GlobalOnlineUsers.
        /// </summary>
        public GlobalSites Sites
        {
            get
            {
                return globalSites;
            }
        }

        #endregion


        #region "Flood protector"

        private ChatFloodProtector floodProtector;


        /// <summary>
        /// Gets instance of ChatFloodProtector.
        /// </summary>
        public ChatFloodProtector FloodProtector
        {
            get
            {
                if (floodProtector == null)
                {
                    floodProtector = new ChatFloodProtector();
                }

                return floodProtector;
            }
        }

        #endregion


        #region "User admin states"

        private UsersRoomAdminStates usersRoomAdminStates = new UsersRoomAdminStates(UNIQUE_NAME, TimeSpan.FromMinutes(10));


        /// <summary>
        /// Gets instance of ChatFloodProtector.
        /// </summary>
        public UsersRoomAdminStates UsersRoomAdminStates
        {
            get
            {
                return usersRoomAdminStates;
            }
        }

        #endregion


        #region "Initiated chats"

        private InitiatedChats initiatedChats = new InitiatedChats(UNIQUE_NAME);


        /// <summary>
        /// Gets instance of GlobalOnlineUsers.
        /// </summary>
        public InitiatedChats InitiatedChats
        {
            get
            {
                return initiatedChats;
            }
        }

        #endregion
    }
}
