using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CMS.Chat
{
    /// <summary>
    /// Class holds four helper classes (rooms, online users, online supporters and messages in support rooms) and provide accessors for them.
    /// </summary>
    public class SiteState
    {
        #region "Private fields"

        private SiteRooms rooms;
        private SiteOnlineUsers onlineUsers;
        private OnlineSupport onlineSupport;
        private SupportRooms supportRooms;

        private int siteID;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets instance of SiteRooms.
        /// </summary>
        public SiteRooms Rooms
        {
            get
            {
                return rooms;
            }
        }


        /// <summary>
        /// Gets instance of SiteOnlineUsers.
        /// </summary>
        public SiteOnlineUsers OnlineUsers
        {
            get
            {
                return onlineUsers;
            }
        }


        /// <summary>
        /// Gets instance of OnlineSupport (cache of supporters online on this site).
        /// </summary>
        public OnlineSupport OnlineSupport
        {
            get
            {
                return onlineSupport;
            }
        }
        

        /// <summary>
        /// Gets instance of SupportRooms (cache of messages in support rooms).
        /// </summary>
        public SupportRooms SupportRooms
        {
            get
            {
                return supportRooms;
            }
        }


        /// <summary>
        /// Gets SiteID of site which is represented by this SiteState.
        /// </summary>
        public int SiteID
        {
            get
            {
                return siteID;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentName">Unique name of a parent class.</param>
        /// <param name="siteID">Site ID</param>
        public SiteState(string parentName, int siteID)
        {
            this.siteID = siteID;

            string uniqueName = parentName + "|SiteState|" + siteID;

            rooms = new SiteRooms(uniqueName, siteID, TimeSpan.FromSeconds(10));

            onlineUsers = new SiteOnlineUsers(uniqueName, siteID);

            onlineSupport = new OnlineSupport(uniqueName, siteID);

            supportRooms = new SupportRooms(uniqueName, siteID);
        }

        #endregion
    }
}