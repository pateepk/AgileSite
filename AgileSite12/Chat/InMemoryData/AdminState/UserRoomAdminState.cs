using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// This class holds information about admin states of one user.
    /// </summary>
    public class UserRoomAdminState
    {
        #region "Private fields"

        /// <summary>
        /// Last change of admin states.
        /// </summary>
        private DateTime lastChange;

        private Dictionary<int, RoomAdminState> roomAdminStates;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="roomAdminStates">Admin states of one user</param>
        public UserRoomAdminState(IEnumerable<RoomAdminState> roomAdminStates)
        {
            this.roomAdminStates = roomAdminStates.ToDictionary(ras => ras.RoomID);

            lastChange = roomAdminStates.Max(ras => ras.LastChange);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets admin states which has changed since specified date.
        /// </summary>
        /// <param name="sinceWhen">Admin states changes later than this date</param>
        public IEnumerable<RoomAdminState> GetChangedRooms(DateTime sinceWhen)
        {
            if (lastChange > sinceWhen)
            {
                return roomAdminStates.Values.Where(ras => (ras.IsOneOnOne == false) && (ras.LastChange > sinceWhen));
            }

            return Enumerable.Empty<RoomAdminState>();
        }


        /// <summary>
        /// Returns rooms IDs with rights are higher than None and are not one to one.
        /// </summary>
        public IEnumerable<int> GetRoomsWithJoinRights()
        {
            return roomAdminStates.Values.Where(ras => (ras.IsOneOnOne == false) && (ras.AdminLevel != AdminLevelEnum.None)).Select(ras => ras.RoomID);
        }


        /// <summary>
        /// Gets admin level in specified room. Returns None if this room was not found.
        /// </summary>
        /// <param name="chatRoomID">Room ID</param>
        public AdminLevelEnum GetAdminLevelInRoom(int chatRoomID)
        {
            if (roomAdminStates.ContainsKey(chatRoomID))
            {
                return roomAdminStates[chatRoomID].AdminLevel;
            }

            return AdminLevelEnum.None;
        }

        #endregion
    }
}
