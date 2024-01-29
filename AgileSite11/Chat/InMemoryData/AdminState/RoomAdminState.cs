using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Admin state of one user in one room.
    /// </summary>
    public class RoomAdminState
    {
        #region "Public properties"

        /// <summary>
        /// Room ID
        /// </summary>
        public int RoomID { get; set; }

        
        /// <summary>
        /// Last change of admin state
        /// </summary>
        public DateTime LastChange { get; set; }


        /// <summary>
        /// Current admin level
        /// </summary>
        public AdminLevelEnum AdminLevel { get; set; }


        /// <summary>
        /// Is room one to one
        /// </summary>
        public bool IsOneOnOne { get; set; }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="lastChange">Last change of admin state</param>
        /// <param name="adminLevel">Admin level</param>
        /// <param name="isOneOnOne">Is room one to one</param>
        public RoomAdminState(int roomID, DateTime lastChange, AdminLevelEnum adminLevel, bool isOneOnOne)
        {
            RoomID = roomID;
            LastChange = lastChange;
            AdminLevel = adminLevel;
            IsOneOnOne = isOneOnOne;
        }

        #endregion
    }
}
