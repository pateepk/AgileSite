using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Chat
{
    /// <summary>
    /// This class holds rooms for one site. Rooms are stored in classic private Dictionary field, so if Cache behavior is desired (ability to clear it, etc.) instance of this class has to be put into Cache.
    /// </summary>
    internal class RoomsContainer
    {
        #region "Private fields"

        private Dictionary<int, RoomState> publicRooms = new Dictionary<int, RoomState>();
        private Dictionary<int, RoomState> privateRooms = new Dictionary<int, RoomState>();
        private ChatCacheDictionaryWrapper<int, RoomState> oneToOneRoomsCache;

        private object roomsLock = new object();

        private string uniqueName;

        private int siteID;

        private DateTime? lastRoomsUpdate = null;
        private DateTime? lastPublicRoomsChange = null;
        private DateTime? lastPrivateRoomsChange = null;
        private DateTime? totalLastRoomsChange = null;
        private DateTime lastRoomsUpdateSQLServerTime = DateTime.MinValue;

        private TimeSpan maxDelay;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Collection of public rooms.
        /// </summary>
        public ICollection<RoomState> PublicRooms
        {
            get
            {
                lock (roomsLock)
                {
                    return publicRooms.Values;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor. <paramref name="parentName"/> is used as cache key.
        /// </summary>
        /// <param name="parentName">Unique name of parent object</param>
        /// <param name="siteID">Site ID</param>
        /// <param name="maxDelay">Max delay</param>
        public RoomsContainer(string parentName, int siteID, TimeSpan maxDelay)
        {
            uniqueName = parentName + "|R";
            this.siteID = siteID;
            this.maxDelay = maxDelay;

            oneToOneRoomsCache = new ChatCacheDictionaryWrapper<int, RoomState>(
                parentName + "|OO",
                (roomID) =>
                {
                    ChatRoomInfo roomInfo = ChatRoomInfoProvider.GetOneToOneChatRoomInfo(roomID);

                    if (roomInfo == null)
                    {
                        return null;
                    }
                    return new RoomState(uniqueName, roomInfo);
                },
                null,
                TimeSpan.FromMinutes(5));

            UpdateRoomsIfNeeded();
        }

        #endregion


        #region "Private properties"

        private DateTime LastUpdateSQLServerTime
        {
            get
            {
                if (!totalLastRoomsChange.HasValue)
                {
                    return lastRoomsUpdateSQLServerTime;
                }
                return lastRoomsUpdateSQLServerTime > totalLastRoomsChange.Value ? lastRoomsUpdateSQLServerTime : totalLastRoomsChange.Value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets RoomState. Room is specified by <paramref name="chatRoomID"/>.
        /// 
        /// Rooms are updated before retrieving if needed.
        /// </summary>
        /// <param name="chatRoomID">ID of a chat room</param>
        /// <returns>RoomState or null if room was not found</returns>
        public RoomState GetRoom(int chatRoomID)
        {
            return GetRoom(chatRoomID, true);
        }


        /// <summary>
        /// Gets changed rooms by specified chat user.
        /// </summary>
        /// <param name="sinceWhen">Rooms changed since this time will be returned. If null, only accessible rooms will be returned</param>
        /// <param name="chatUser">Chat user whose accessible rooms should be returned. If null, only public rooms will be returned.</param>
        public ChatRoomsData GetChangedRooms(DateTime? sinceWhen, ChatUserInfo chatUser)
        {
            lock (roomsLock)
            {
                UpdateRoomsIfNeeded();

                // If chat user is not logged in - return only public rooms
                if (chatUser == null)
                {
                    return GetRoomsForLoggedOut(sinceWhen);
                }

                // If sinceWhen is null, return all accessible rooms
                if (sinceWhen == null)
                {
                    return GetRoomsFirstRequest(chatUser);
                }
                else
                {
                    return GetRoomsNotFirstRequest(sinceWhen.Value, chatUser);
                }
            }
        }


        /// <summary>
        /// Gets room from cache. If room is not found, new rooms from database are retrieved and then the finding is made again.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="roomState">Output RoomState</param>
        public bool ForceTryGetRoom(int roomID, out RoomState roomState)
        {
            lock (roomsLock)
            {
                roomState = GetRoom(roomID);

                if (roomState == null)
                {
                    ForceUpdate();

                    roomState = GetRoom(roomID);
                }

                return roomState != null;
            }
        }


        /// <summary>
        /// Updates rooms.
        /// </summary>
        public void ForceUpdate()
        {
            Update();
        }


        /// <summary>
        /// Updates (takes changes from DB) if time since last update is greater than maxDelay.
        /// </summary>
        public void UpdateRoomsIfNeeded()
        {
            DateTime now = DateTime.Now;

            if (!lastRoomsUpdate.HasValue || (lastRoomsUpdate.Value.Add(maxDelay) < now))
            {
                lock (roomsLock)
                {
                    if (!lastRoomsUpdate.HasValue || (lastRoomsUpdate.Value.Add(maxDelay) < now))
                    {
                        Update();
                    }
                }
            }
        }


        /// <summary>
        /// Tries to return private room. If room is not found, returns null.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <returns>RoomState or null</returns>
        public RoomState GetPrivateRoom(int roomID)
        {
            lock (roomsLock)
            {
                RoomState result;
                if (!privateRooms.TryGetValue(roomID, out result))
                {
                    result = null;
                }

                return result;
            }
        }

        #endregion


        #region "Private methods"

        private RoomState GetRoom(int chatRoomID, bool update)
        {
            lock (roomsLock)
            {
                if (update)
                {
                    UpdateRoomsIfNeeded();
                }

                RoomState roomState;

                // Try public room
                if (!publicRooms.TryGetValue(chatRoomID, out roomState))
                {
                    // Try private room
                    if (!privateRooms.TryGetValue(chatRoomID, out roomState))
                    {
                        // Try one to one room (returns null if does not exist)
                        roomState = oneToOneRoomsCache.GetItem(chatRoomID);
                    }
                }

                return roomState;
            }
        }


        private void Update()
        {
            lock (roomsLock)
            {
                // Sets time of last rooms update. The time has to be taken from SQL Server because the time at web servers can differ from each other if you are using web farms.
                lastRoomsUpdateSQLServerTime = ChatRoomInfoProvider.GetCurrentDateTime();

                IEnumerable<ChatRoomInfo> changedRooms = ChatRoomInfoProvider.GetChangedChatRooms(siteID, totalLastRoomsChange);

                foreach (ChatRoomInfo changedRoom in changedRooms)
                {
                    Dictionary<int, RoomState> rooms = changedRoom.ChatRoomPrivate ? privateRooms : publicRooms;
                    Dictionary<int, RoomState> oppositeRooms = changedRoom.ChatRoomPrivate ? publicRooms : privateRooms;

                    RoomState roomState;
                    if (rooms.TryGetValue(changedRoom.ChatRoomID, out roomState))
                    {
                        roomState.ReloadChatRoomInfo(changedRoom);
                    }
                    else
                    {
                        rooms[changedRoom.ChatRoomID] = new RoomState(uniqueName, changedRoom);
                    }

                    // If private/public state was changed, we have to move room from one list to another
                    if (oppositeRooms.ContainsKey(changedRoom.ChatRoomID))
                    {
                        oppositeRooms.Remove(changedRoom.ChatRoomID);
                    }

                    if (changedRoom.ChatRoomPrivate)
                    {
                        if (!lastPrivateRoomsChange.HasValue || (changedRoom.ChatRoomLastModification > lastPrivateRoomsChange.Value))
                        {
                            lastPrivateRoomsChange = changedRoom.ChatRoomLastModification;
                        }
                    }
                    else
                    {
                        if (!lastPublicRoomsChange.HasValue || (changedRoom.ChatRoomLastModification > lastPublicRoomsChange.Value))
                        {
                            lastPublicRoomsChange = changedRoom.ChatRoomLastModification;
                        }
                    }

                    if (!totalLastRoomsChange.HasValue || (changedRoom.ChatRoomLastModification > totalLastRoomsChange.Value))
                    {
                        totalLastRoomsChange = changedRoom.ChatRoomLastModification;
                    }
                }

                lastRoomsUpdate = DateTime.Now;
            }
        }


        private RoomState GetPublicOrPrivateRoom(int roomID)
        {
            lock (roomsLock)
            {
                RoomState result;
                if (!publicRooms.TryGetValue(roomID, out result))
                {
                    if (!privateRooms.TryGetValue(roomID, out result))
                    {
                        result = null;
                    }
                }

                return result;
            }
        }


        #region "Getting rooms"

        private ChatRoomsData GetRoomsForLoggedOut(DateTime? sinceWhen)
        {
            // Return null if nothing has changed
            if (!lastPublicRoomsChange.HasValue || (sinceWhen.HasValue && (lastPublicRoomsChange.Value <= sinceWhen.Value)))
            {
                return null;
            }

            IEnumerable<RoomState> rooms = publicRooms.Values;

            // It is not the first request
            if (sinceWhen.HasValue)
            {
                rooms = rooms.Where(rs => rs.RoomInfo.ChatRoomLastModification > sinceWhen.Value);
            }
            // First request - return only enabled rooms and rooms where anonymous users are allowed
            else
            {
                rooms.Where(rs => rs.RoomInfo.ChatRoomEnabled && rs.RoomInfo.ChatRoomAllowAnonym);
            }

            List<RoomState> list = rooms.ToList();

            if (list.Count == 0)
            {
                return null;
            }

            return new ChatRoomsData()
            {
                LastChange = LastUpdateSQLServerTime.Ticks,
                List = list.Select(rs => ChatRoomHelper.ConvertRoomToData(rs, true))
            };
        }


        private ChatRoomsData GetRoomsFirstRequest(ChatUserInfo chatUser)
        {
            // Begin with public rooms
            IEnumerable<RoomState> rooms = publicRooms.Values;


            UsersRoomAdminStates roomAdminStates = ChatGlobalData.Instance.UsersRoomAdminStates;

            // IDs of rooms where user can enter
            IEnumerable<int> accessibleRooms = roomAdminStates.GetRoomsWithJoinRights(chatUser.ChatUserID);

            // Add accessible rooms (only private) to the list
            rooms = rooms.Concat(accessibleRooms.Select(chatRoomID => GetPrivateRoom(chatRoomID)).Where(roomState => roomState != null));

            // Filter to enabled rooms
            rooms = rooms.Where(rs => rs.RoomInfo.ChatRoomEnabled);

            // If user is anonymous, filter to rooms which allows anonymous users
            if (chatUser.IsAnonymous)
            {
                rooms = rooms.Where(rs => rs.RoomInfo.ChatRoomAllowAnonym);
            }

            List<RoomState> list = rooms.ToList();

            if (list.Count == 0)
            {
                return null;
            }

            return new ChatRoomsData()
            {
                LastChange = LastUpdateSQLServerTime.Ticks,
                List = list.Select(rs => ChatRoomHelper.ConvertRoomToData(rs, chatUser.IsAnonymous))
            };
        }


        private ChatRoomsData GetRoomsNotFirstRequest(DateTime sinceWhen, ChatUserInfo chatUser)
        {
            // Begin with empty list
            IEnumerable<RoomState> rooms = Enumerable.Empty<RoomState>();

            // If any public room was changed (lastPublicRoomsChange.Value > sinceWhen.Value), include public rooms the list (they will be filtered later)
            if (lastPublicRoomsChange.HasValue && (lastPublicRoomsChange.Value > sinceWhen))
            {
                rooms = publicRooms.Values;
            }

            UsersRoomAdminStates roomAdminStates = ChatGlobalData.Instance.UsersRoomAdminStates;

            // Rooms where user has more then None rights
            IEnumerable<int> accessibleRooms = roomAdminStates.GetRoomsWithJoinRights(chatUser.ChatUserID);


            // Add accessible rooms (only private) to the list
            rooms = rooms.Concat(accessibleRooms.Select(chatRoomID => GetPrivateRoom(chatRoomID)).Where(roomState => roomState != null));

            // Filter rooms (public and accessible private) to changed ones
            rooms = rooms.Where(rs => rs.RoomInfo.ChatRoomLastModification > sinceWhen);


            // Rooms converted from RoomState to ChatRoomData
            IEnumerable<ChatRoomData> finalRooms = rooms.Select(rs => ChatRoomHelper.ConvertRoomToData(rs, chatUser.IsAnonymous));


            // Rooms where rights of this user were changed since last time
            IEnumerable<RoomAdminState> changedRights = roomAdminStates.GetRoomsWithChangedRights(chatUser.ChatUserID, sinceWhen);

            // Rooms where rights of current user changed since 'sinceWhen'
            IEnumerable<ChatRoomData> roomsWithChangedRights = changedRights.Select(ras => ChatRoomHelper.ConvertRoomToDataAdminLevel(GetPublicOrPrivateRoom(ras.RoomID), ras.AdminLevel)).Where(r => r != null);

            finalRooms = finalRooms.Concat(roomsWithChangedRights);


            // Those rooms were recently (since 'sinceWhen') changed from public to private
            IEnumerable<RoomState> migratedToPrivateRooms = privateRooms.Values.Where(rs => rs.RoomInfo.ChatRoomPrivateStateLastModification > sinceWhen);

            // Migrated rooms where current user does not have access rights
            IEnumerable<RoomState> migratedRoomsWithoutRights = migratedToPrivateRooms.Where(migratedRoom => roomAdminStates.GetAdminLevelInRoom(chatUser.ChatUserID, migratedRoom.RoomInfo.ChatRoomID) < AdminLevelEnum.Join);

            // Add those rooms to the final list
            finalRooms = finalRooms.Concat(migratedRoomsWithoutRights.Select(roomState => new ChatRoomData() { IsRemoved = true, ChatRoomID = roomState.RoomInfo.ChatRoomID }));



            List<ChatRoomData> list = finalRooms.ToList();

            if (list.Count == 0)
            {
                return null;
            }

            return new ChatRoomsData()
            {
                LastChange = LastUpdateSQLServerTime.Ticks,
                List = list
            };
        }

        #endregion

        #endregion
    }
}
