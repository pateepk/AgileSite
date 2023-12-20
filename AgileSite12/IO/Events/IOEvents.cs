using System;
using System.Linq;
using System.Text;

namespace CMS.IO
{
    /// <summary>
    /// IO events
    /// </summary>
    public class IOEvents
    {
        /// <summary>
        /// Fires when a file is deleted
        /// </summary>
        public static IOHandler DeleteFile = new IOHandler { Name = "IOEvents.DeleteFile" };
        
        /// <summary>
        /// Fires when a directory is deleted
        /// </summary>
        public static IOHandler DeleteDirectory = new IOHandler { Name = "IOEvents.DeleteDirectory" };
    }
}
