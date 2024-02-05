using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatInitiatedChatRequestInfo management.
    /// </summary>
    public class ChatInitiatedChatRequestInfoProvider : AbstractInfoProvider<ChatInitiatedChatRequestInfo, ChatInitiatedChatRequestInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat initiated chat requests.
        /// </summary>
        public static ObjectQuery<ChatInitiatedChatRequestInfo> GetInitiatedChatRequests()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns initiated chat request with specified ID.
        /// </summary>
        /// <param name="requestId">Initiated chat request ID.</param>        
        public static ChatInitiatedChatRequestInfo GetChatInitiatedChatRequestInfo(int requestId)
        {
            return ProviderObject.GetInfoById(requestId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified initiated chat request.
        /// </summary>
        /// <param name="requestObj">Initiated chat request to be set.</param>
        public static void SetChatInitiatedChatRequestInfo(ChatInitiatedChatRequestInfo requestObj)
        {
            ProviderObject.SetInfo(requestObj);
        }


        /// <summary>
        /// Deletes specified initiated chat request.
        /// </summary>
        /// <param name="requestObj">Initiated chat request to be deleted.</param>
        public static void DeleteChatInitiatedChatRequestInfo(ChatInitiatedChatRequestInfo requestObj)
        {
            ProviderObject.DeleteInfo(requestObj);
        }


        /// <summary>
        /// Deletes initiated chat request with specified ID.
        /// </summary>
        /// <param name="requestId">Initiated chat request ID.</param>
        public static void DeleteChatInitiatedChatRequestInfo(int requestId)
        {
            ChatInitiatedChatRequestInfo requestObj = GetChatInitiatedChatRequestInfo(requestId);
            DeleteChatInitiatedChatRequestInfo(requestObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets all active initiated chat request.
        /// </summary>
        /// <param name="byUser">If true, requests will be grouped by user (PK will be UserID). Otherwise it will be ContactID.</param>
        public static IEnumerable<InitiateChatRequestData> GetAllInitiateRequests(bool byUser)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ByUser", byUser);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.InitiatedChatRequest.selectallrequests", parameters);

            return GetInitiateRoomDataFromDataSet(ds);
        }


        /// <summary>
        /// Gets requests which has changed (change of state or new message) since changedSince.
        /// </summary>
        /// <param name="byUser">If true, requests will be grouped by user (PK will be UserID). Otherwise it will be ContactID.</param>
        /// <param name="changedSince">Request changed since this time will be returned</param>
        public static IEnumerable<InitiateChatRequestData> GetChangedInitiateRequests(bool byUser, DateTime changedSince)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ByUser", byUser);
            parameters.Add("@ChangedSince", changedSince);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.InitiatedChatRequest.selectchangedrequests", parameters);

            return GetInitiateRoomDataFromDataSet(ds);
        }


        /// <summary>
        /// Finds initiated chat request by user id or contact id. One of these has to be not null.
        /// </summary>
        /// <param name="userID">If not null, finds initiate request by user id</param>
        /// <param name="contactID">If not null, finds initiate request by contact id</param>
        /// <returns>ChatInitiatedChatRequestInfo or null if the request was not found</returns>
        public static ChatInitiatedChatRequestInfo GetInitiateRequest(int? userID, int? contactID)
        {
            if (((userID == null) && (contactID == null)) || ((userID != null) && (contactID != null)))
            {
                return null;
            }

            var whereCondition = new WhereCondition();
            whereCondition = (userID == null) ? whereCondition.WhereEquals("InitiatedChatRequestContactID", contactID.Value): whereCondition.WhereEquals("InitiatedChatRequestUserID", userID.Value);
            
            return GetInitiatedChatRequests()
                    .Where(whereCondition)
                    .SingleOrDefault();
        }


        /// <summary>
        /// Gets initiated chat request by room ID.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public static ChatInitiatedChatRequestInfo GetInitiateRequest(int roomID)
        {
            return GetInitiatedChatRequests()
                    .WhereEquals("InitiatedChatRequestRoomID", roomID)
                    .SingleOrDefault();
        }


        /// <summary>
        /// Cleans old initiated chat requests. First, requests are changed to the 'deleted' state, co they can be removed from cache and on the second pass, they are completely deleted.
        /// </summary>
        /// <returns>Number of completely deleted requests.</returns>
        public static int CleanOldRequests()
        {
            return ConnectionHelper.ExecuteQuery("Chat.InitiatedChatRequest.cleanoldinitiatedchatrequests", null).Tables[0].Rows[0].Field<int>(0);
        }

        #endregion


        #region "Internal methods - Advanced"
        #endregion		


        #region "Private methods"

        private static IEnumerable<InitiateChatRequestData> GetInitiateRoomDataFromDataSet(DataSet ds)
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row =>
                    new InitiateChatRequestData
                    {
                        ChangeTime = ValidationHelper.GetDateTime(row["LastChange"], DateTime.MinValue),
                        RequestState = (InitiatedChatRequestStateEnum)ValidationHelper.GetInteger(row["RequestState"], 1),
                        InitiatorName = ValidationHelper.GetString(row["InitiatorName"], ""),
                        PK = ValidationHelper.GetInteger(row["UserContactID"], 0),
                        RoomID = ValidationHelper.GetInteger(row["RoomID"], 0),
                    });
            }

            return Enumerable.Empty<InitiateChatRequestData>();
        }

        #endregion
    }
}
