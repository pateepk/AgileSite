using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.VirtualPathProvider
{
    /// <summary>
    /// Virtual file event data
    /// </summary>
    public class VirtualFileEventArgs : EventArgs
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
        /// Result virtual file
        /// </summary>
        public DbVirtualFile VirtualFile
        {
            get;
            set;
        }
    }
}
