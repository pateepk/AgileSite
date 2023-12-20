using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// This class stores cache of SupportRoom (rooms needed support). Those rooms are stored in cache under the key which is time of last change.
    /// </summary>
    public class SupportRooms
    {
        #region "Nested classes"

        /// <summary>
        /// Wraps one argument (DateTime?) in class implementing IChatCacheableParam.
        /// </summary>
        private class SinceWhenCacheParam : IChatCacheableParam
        {
            /// <summary>
            /// Since when data should be loaded.
            /// </summary>
            public DateTime? SinceWhen { get; set; }


            /// <summary>
            /// Cache key
            /// </summary>
            public string CacheKey
            {
                get
                {
                    return SinceWhen?.Ticks.ToString() ?? "(null)";
                }
            }

            /// <summary>
            /// Constructs SinceWhenCacheParam.
            /// </summary>
            /// <param name="sinceWhen">since when</param>
            public SinceWhenCacheParam(DateTime? sinceWhen)
            {
                SinceWhen = sinceWhen;
            }
        }

        #endregion


        #region "Private fields"

        private readonly ChatParametrizedCacheWrapper<SupportRoom, SinceWhenCacheParam> supportRoomsCache;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructs SupportRooms. Initializes cache.
        /// </summary>
        /// <param name="parentName">This string is used as a cache key - it has to be unique for every created instance of this class</param>
        /// <param name="siteID">Taken rooms on this site (and global rooms) will be stored in this cache</param>
        public SupportRooms(string parentName, int siteID)
        {
            supportRoomsCache = new ChatParametrizedCacheWrapper<SupportRoom, SinceWhenCacheParam>(
                parentName + "|SupRooms",
                (param) => ChatRoomInfoProvider.GetSupportRoomsWithNewMessages(param.SinceWhen, siteID),
                TimeSpan.FromSeconds(10));
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets room which needs support.
        /// </summary>
        /// <param name="supportChatUserID">ID of chat user who is online on support right now. Rooms will be filtered based on this param (if room is taken by somebody else, it won't be send, etc.)</param>
        /// <param name="sinceWhen">Rooms changed since this time will be send. All pending support request will be send if null.</param>
        public SupportRoomsData GetChangedSupportRooms(int supportChatUserID, DateTime? sinceWhen)
        {
            var cacheResult = supportRoomsCache.GetData(new SinceWhenCacheParam(sinceWhen));

            // If nothing was found, return null
            if (cacheResult == null)
            {
                return null;
            }

            List<SupportRoomData> resultRooms = (from room in cacheResult.Items
                                                 let isRemoved = (room.TakenByChatUserID.HasValue && room.TakenByChatUserID.Value != supportChatUserID) || // room should be removed from the list on client if it is taken (TakenByChatUserID.HasValue) by another user
                                                    (!room.TakenByChatUserID.HasValue && room.UnreadMessagesCount == 0) // also remove it if there are no new messages and room is not taken (it means that room was resolved)
                                                 where !isRemoved || (sinceWhen.HasValue && room.TakenStateLastChange.HasValue && (room.TakenStateLastChange.Value > sinceWhen.Value)) // filter out rooms which has been removed before sinceWhen (those are already removed on client and it is not neccesary to send them again)
                                                 select new SupportRoomData // convert SupportRoom to SupportRoomData
                                                {
                                                    ChatRoomID = room.ChatRoomID,
                                                    DisplayName = room.DisplayName,
                                                    UnreadMessagesCount = room.UnreadMessagesCount,
                                                    IsTaken = room.TakenByChatUserID.HasValue && room.TakenByChatUserID.Value == supportChatUserID,
                                                    IsRemoved = isRemoved,
                                                }).ToList();

            // If everything was filtered out, return null
            if (resultRooms.Count == 0)
            {
                return null;
            }

            return new SupportRoomsData
            {
                List = resultRooms,
                LastChange = cacheResult.LastChange.Ticks, // take last change from the cacheResult. By doing this, it will more likely happen for clients to have the same LastChange, thus hitting cache more often
            };
        }

        #endregion
    }
}
