using System.Linq;

using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Protection;

namespace CMS.Chat
{
    /// <summary>
    /// Base class of all chat web services.
    /// </summary>
    public class ChatServiceBase
    {
        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ChatServiceBase()
        {
            CMSApplication.Init();
        }

        #endregion


        #region "Private static fields"

        private static GlobalSites mSites;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Shortcut to ChatGlobalData.Instance.Sites (contains cached chat objects (rooms, etc.)).
        /// </summary>
        protected GlobalSites Sites
        {
            get
            {
                return mSites ?? (mSites = ChatGlobalData.Instance.Sites);
            }
        }


        /// <summary>
        /// Gets ChatUserState of currently logged in chat user.
        /// </summary>
        protected ChatUserStateData CurrentChatUserState
        {
            get
            {
                bool isCurrentUserHidden;

                ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser(out isCurrentUserHidden);

                // Chat user is logged in (either hidden or not hidden)
                if (currentChatUser != null)
                {
                    return new ChatUserStateData()
                    {
                        IsLoggedIn = !isCurrentUserHidden, // If user is hidden, act as if he was not logged in (but send his ID, Nickname, etc.)
                        Nickname = currentChatUser.ChatUserNickname,
                        ChatUserID = currentChatUser.ChatUserID,
                        IsAnonymous = currentChatUser.IsAnonymous,
                    };
                }
                else
                {
                    // Chat user is not logged in.

                    CurrentUserInfo cmsUser = MembershipContext.AuthenticatedUser;

                    // CMS User is also not logged in - return nothing
                    if (cmsUser.IsPublic())
                    {
                        return new ChatUserStateData()
                        {
                            IsLoggedIn = false,
                            Nickname = "",
                            ChatUserID = 0,
                            IsAnonymous = true,
                        };
                    }
                    else // Chat user is not logged in, but CMS User is - return information about CMS User
                    {
                        ChatUserInfo associatedChatUser = ChatUserHelper.GetChatUserFromCMSUser(cmsUser);

                        return new ChatUserStateData()
                        {
                            IsLoggedIn = false,
                            Nickname = associatedChatUser.ChatUserNickname,
                            ChatUserID = associatedChatUser.ChatUserID,
                            IsAnonymous = associatedChatUser.IsAnonymous, // always false
                        };
                    }
                }
            }
        }

        #endregion


        #region "Verifying methods"

        /// <summary>
        /// Verifies that currently logged in chat user has admin rights for a room.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        protected void VerifyChatUserHasAdminRoomRights(int roomID)
        {
            if (!ChatUserHelper.CheckAdminRoomRights(roomID))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
            }
        }


        /// <summary>
        /// Verifies that IP addres which was used to make this request is not banned.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        protected void VerifyIPIsNotBanned()
        {
            if (IsIPBanned)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BannedIP);
            }
        }


        /// <summary>
        /// Verifies that chat user is logged in.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        protected void VerifyChatUserIsLoggedIn()
        {
            if (!ChatOnlineUserHelper.IsChatUserLoggedIn())
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NotLoggedIn);
            }
        }
        

        /// <summary>
        /// Verifies that currently logged in chat user is online in a room.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        protected void VerifyChatUserIsOnlineInARoom(int roomID)
        {
            RoomState roomState;

            if (!Sites.Current.Rooms.ForceTryGetRoom(roomID, out roomState) || !roomState.ForceIsUserOnline(ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NotJoinedInARoom);
            }
        }


        /// <summary>
        /// Verifies that currently logged in chat user has specified permission.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        /// <param name="permission">Chat permission to check</param>
        protected void VerifyChatUserHasPermission(ChatPermissionEnum permission)
        {
            if (!ChatProtectionHelper.HasCurrentUserPermission(permission))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
            }
        }


        /// <summary>
        /// Verifies that currently logged in chat user has at least one of the passed permissions.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        /// <param name="permissions">Chat permissions to check</param>
        protected void VerifyChatUserHasAnyPermission(params ChatPermissionEnum[] permissions)
        {
            if (!permissions.Any(perm => ChatProtectionHelper.HasCurrentUserPermission(perm)))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
            }
        }


        /// <summary>
        /// Verifies that currently logged in chat user has all of the passed permissions.
        /// 
        /// Throws exception in case of failure and does nothing in case of success.
        /// </summary>
        /// <param name="permissions">Chat permissions to check</param>
        protected void VerifyChatUserHasAllPermissions(params ChatPermissionEnum[] permissions)
        {
            if (!permissions.All(perm => ChatProtectionHelper.HasCurrentUserPermission(perm)))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AccessDenied);
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Builds general chat response with OK code.
        /// </summary>
        /// <returns>General response with OK code and default string</returns>
        protected ChatGeneralResponse GetOkChatResponse()
        {
            return GetChatResponse(ChatResponseStatusEnum.OK, ChatResponseStatusEnum.OK.ToStringValue());
        }


        /// <summary>
        /// Builds general chat response with OK code and Data set to <paramref name="data"/>.
        /// </summary>
        /// <typeparam name="TData">Type of inner Data property.</typeparam>
        /// <param name="data">Payload of this response</param>
        /// <returns>General response with data</returns>
        protected ChatGeneralResponse<TData> GetOkChatResponse<TData>(TData data)
        {
            return GetChatResponse(ChatResponseStatusEnum.OK, ChatResponseStatusEnum.OK.ToStringValue(), data);
        }


        /// <summary>
        /// Builds general chat response with error set to <paramref name="statusCode"/> and message set to default value associated with <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">Status code of the response</param>
        /// <returns>General response</returns>
        protected ChatGeneralResponse GetChatResponse(ChatResponseStatusEnum statusCode)
        {
            return GetChatResponse(statusCode, statusCode.ToStringValue());
        }


        /// <summary>
        /// Builds general chat response with error set to <paramref name="statusCode"/> and message set to <paramref name="statusMessage"/>.
        /// </summary>
        /// <param name="statusCode">Response code</param>
        /// <param name="statusMessage">Response message</param>
        /// <returns>General error response</returns>
        protected ChatGeneralResponse GetChatResponse(ChatResponseStatusEnum statusCode, string statusMessage)
        {
            return new ChatGeneralResponse()
            {
                StatusCode = statusCode,
                StatusMessage = statusMessage
            };
        }


        /// <summary>
        /// Builds chat response with code set to <paramref name="statusCode"/>, message set to default value associated with <paramref name="statusCode"/> and data set to default(TData).
        /// </summary>
        /// <typeparam name="TData">Type of Data property of the response</typeparam>
        /// <param name="statusCode">Status code</param>
        /// <returns>General response</returns>
        protected ChatGeneralResponse<TData> GetChatResponse<TData>(ChatResponseStatusEnum statusCode)
        {
            return GetChatResponse<TData>(statusCode, statusCode.ToStringValue());
        }


        /// <summary>
        /// Builds chat response with code set to <paramref name="statusCode"/>, message set to <paramref name="statusMessage"/> and data set to default(TData).
        /// </summary>
        /// <typeparam name="TData">Type of data property</typeparam>
        /// <param name="statusCode">Status code</param>
        /// <param name="statusMessage">Status message</param>
        /// <returns>General response with Data of type TData</returns>
        protected ChatGeneralResponse<TData> GetChatResponse<TData>(ChatResponseStatusEnum statusCode, string statusMessage)
        {
            return GetChatResponse(statusCode, statusMessage, default(TData));
        }


        /// <summary>
        /// Builds generic ChatGeneral response with properties set to passed params.
        /// </summary>
        /// <typeparam name="TData">Type of payload</typeparam>
        /// <param name="statusCode">Status code</param>
        /// <param name="statusMessage">Status message</param>
        /// <param name="data">Payload</param>
        /// <returns>ChatGeneralResponse</returns>
        protected ChatGeneralResponse<TData> GetChatResponse<TData>(ChatResponseStatusEnum statusCode, string statusMessage, TData data)
        {
            return new ChatGeneralResponse<TData>()
            {
                Data = data,
                StatusCode = statusCode,
                StatusMessage = statusMessage
            };
        }


        /// <summary>
        /// Gets true if current user's IP address is banned and he can not do anything on the chat.
        /// </summary>
        protected bool IsIPBanned
        {
            get
            {
                return !BannedIPInfoProvider.IsAllowed(SiteContext.CurrentSiteName, BanControlEnum.AllNonComplete);
            }
        }

        #endregion
    }
}
