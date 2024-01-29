using System;

using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Class providing basic variables and properties.
    /// </summary>
    public class FolderTree : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        /// <summary>
        /// Root folder path in file system.
        /// </summary>
        public string RootFolderPath
        {
            get;
            set;
        }


        /// <summary>
        /// Path to the trees images.
        /// </summary>
        public string ImageFolderPath
        {
            get;
            set;
        }


        /// <summary>
        /// Selected path in treeview.
        /// </summary>
        public virtual string SelectedPath
        {
            get;
            set;
        }


        /// <summary>
        /// Expand path in treeview.
        /// </summary>
        public string ExpandPath
        {
            get;
            set;
        }


        /// <summary>
        /// Media library folder in root of treeview.
        /// </summary>
        public string MediaLibraryFolder
        {
            get;
            set;
        }


        /// <summary>
        /// Media library path for root of tree within library.
        /// </summary>
        public string MediaLibraryPath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the file id querystring parameter.
        /// </summary>
        public string FileIDQueryStringKey
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the path querystring parameter.
        /// </summary>
        public string PathQueryStringKey
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the filter method.
        /// </summary>
        public int FilterMethod
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if subfolders content should be displayed.
        /// </summary>
        public bool ShowSubfoldersContent
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if file count in directory should be displayed in folder tree.
        /// </summary>
        public bool DisplayFileCount
        {
            get;
            set;
        }


        /// <summary>
        /// Current folder in library.
        /// </summary>
        public string CurrentFolder
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition.
        /// </summary>
        public string Where
        {
            get
            {
                return ValidationHelper.GetString(ViewState["Where"], String.Empty);
            }
            set
            {
                ViewState["Where"] = value;
            }
        }

        #endregion
    }
}