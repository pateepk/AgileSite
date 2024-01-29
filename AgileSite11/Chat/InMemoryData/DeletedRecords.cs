using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Class holding information about number of records deleted by scheduled task 'Delete old chat records'
    /// </summary>
    public class DeletedRecords
    {
        /// <summary>
        /// Messages deleted
        /// </summary>
        public int MessagesDeleted { get; set; }


        /// <summary>
        /// Rooms deleted
        /// </summary>
        public int RoomsDeleted { get; set; }


        /// <summary>
        /// Users deleted
        /// </summary>
        public int UsersDeleted { get; set; }
    }
}
