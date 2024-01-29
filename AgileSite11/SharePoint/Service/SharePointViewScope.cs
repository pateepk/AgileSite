using System;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// Contains well-known constants specifying the recursive scope of a SharePoint library.
    /// </summary>
    public static class SharePointViewScope
    {
        /// <summary>
        /// Show only the files and subfolders of a specific folder.
        /// </summary>
        public const string DEFAULT = "Default";


        /// <summary>
        /// Show only the files of a specific folder.
        /// </summary>
        public const string FILES_ONLY = "FilesOnly";


        /// <summary>
        /// Show all files and all subfolders of all folders.
        /// </summary>
        public const string RECURSIVE_ALL = "RecursiveAll";


        /// <summary>
        /// Show only the files of a specific folder.
        /// </summary>
        public const string RECURSIVE = "Recursive";
    }
}
