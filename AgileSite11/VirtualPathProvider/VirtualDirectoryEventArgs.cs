using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.VirtualPathProvider
{
    /// <summary>
    /// Virtual directory event data
    /// </summary>
    public class VirtualDirectoryEventArgs : EventArgs
    {
        /// <summary>
        /// Required virtual path
        /// </summary>
        public string VirtualPath
        {
            get;
            set;
        }


        /// <summary>
        /// Result virtual directory 
        /// </summary>
        public DbVirtualDirectory VirtualDirectory
        {
            get;
            set;
        }
    }
}
