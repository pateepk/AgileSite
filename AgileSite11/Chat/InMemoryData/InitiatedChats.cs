using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// This class holds initiated chat requests in cache. There are two caches grouped by UserID and ContactID, so checking for
    /// request is fast.
    /// 
    /// Cache invalidates itself after certain amount of time (no manual invalidation).
    /// </summary>
    public class InitiatedChats
    {
        #region "Private fields"

        private ChatCurrentStateCacheWrapper<InitiateChatRequestData, int> initiateRequestsByContactCache;
        private ChatCurrentStateCacheWrapper<InitiateChatRequestData, int> initiateRequestsByUserCache;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructs InitiatedChats. Initializes cache wrappers.
        /// </summary>
        /// <param name="parentName">This string is used as a cache key - it has to be unique for every created instance of this class</param>
        public InitiatedChats(string parentName)
        {
            initiateRequestsByContactCache = new ChatCurrentStateCacheWrapper<InitiateChatRequestData, int>(
                parentName + "|IRContact",
                () => ChatInitiatedChatRequestInfoProvider.GetAllInitiateRequests(false),
                (since) => ChatInitiatedChatRequestInfoProvider.GetChangedInitiateRequests(false, since),
                TimeSpan.FromSeconds(9) // Initiation requests will be refreshed after 9 seconds (less than half interval of PingInitiate)
                );


            initiateRequestsByUserCache = new ChatCurrentStateCacheWrapper<InitiateChatRequestData, int>(
                parentName + "|IRUser",
                () => ChatInitiatedChatRequestInfoProvider.GetAllInitiateRequests(true),
                (since) => ChatInitiatedChatRequestInfoProvider.GetChangedInitiateRequests(true, since),
                TimeSpan.FromSeconds(9) // Initiation requests will be refreshed after 9 seconds (less than half interval of PingInitiate)
                );
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets Initiated chat request by ContactID or UserID if exists.
        /// 
        /// First it searches request by contactID, if request is found, it is returned.
        /// 
        /// Then it searches by userID.
        /// 
        /// If request is not found or was created before <paramref name="changedSince"/> null is returned.
        /// </summary>
        /// <param name="contactID">Request for this contact is searched. Ignored if null.</param>
        /// <param name="userID">Request for this user is searched. Ignored if null.</param>
        /// <param name="changedSince">Last change this client received.</param>
        public InitiateChatRequestData GetInitiatedChatRequest(int? contactID, int? userID, DateTime? changedSince)
        {
            InitiateChatRequestData result;

            if (contactID.HasValue && TryGetInitiatedChatRequestFromCache(initiateRequestsByContactCache, contactID.Value, changedSince, out result))
            {
                return result;
            }

            if (userID.HasValue && TryGetInitiatedChatRequestFromCache(initiateRequestsByUserCache, userID.Value, changedSince, out result))
            {
                return result;
            }

            return null;
        }


        /// <summary>
        /// Invalidates cache organized by ContactID. So on the next call to GetInitiatedChatRequest, result will be 100% accurate.
        /// </summary>
        public void InvalidateContactIDCache()
        {
            initiateRequestsByContactCache.Invalidate();
        }


        /// <summary>
        /// Invalidates cache organized by UserID. So on the next call to GetInitiatedChatRequest, result will be 100% accurate.
        /// </summary>
        public void InvalidateUserIDCache()
        {
            initiateRequestsByUserCache.Invalidate();
        }

        #endregion


        #region "Private methods"

        private bool TryGetInitiatedChatRequestFromCache(ChatCurrentStateCacheWrapper<InitiateChatRequestData, int> cache, int key, DateTime? changedSince, out InitiateChatRequestData result)
        {
            if (!cache.CurrentState.TryGetValue(key, out result))
            {
                return false;
            }

            if (changedSince.HasValue && (result.ChangeTime <= changedSince.Value))
            {
                result = null;
                return false;
            }

            if (!changedSince.HasValue)
            {
                if (result.IsRemoved)
                {
                    result = null;
                    return false;
                }
            }

            // If request is not removed, append messages
            if (!result.IsRemoved)
            {
                result.Messages = ChatMessageInfoProvider.GetClassicMessagesText(result.RoomID);
            }

            return true;
        }

        #endregion
    }
}
