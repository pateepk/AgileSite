using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Describes a cleanup action to be performed on a folder.
    /// </summary>
    internal class CleanupAction
    {
        /// <summary>
        /// Gets or sets the folder path for the cleanup.
        /// </summary>
        public string FolderPath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the folder cleanup is to be performed recursively for all subfolders. False by default.
        /// </summary>
        public bool Recursive
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the callback to be invoked for each file being cleaned up. The return value
        /// indicates whether to delete the file.
        /// </summary>
        public Func<string, string, bool> DeleteFileCallback
        {
            get;
            set;
        }
    }
}
