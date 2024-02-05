using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Container class for CMS file system dialog parameters which determine the behavior of the dialogs.
    /// </summary>
    public class FileSystemDialogConfiguration
    {
        #region "Variables"

        private string mSelectedPath = "";
        private string mStartingPath = "";
        private string mDefaultPath = "";
        private string mExcludedFolders = "";
        private string mExcludedExtensions = "";
        private string mAllowedFolders = "";
        private string mAllowedExtensions = "";
        private bool mAllowNonApplicationPath = true;

        // Dialog dimensions
        private int mDialogWidth = 95;
        private int mDialogHeight = 86;
        private bool mUseRelativeDimensions = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// If true, the dialog allows managing of the files.
        /// </summary>
        public bool AllowManage
        {
            get;
            set;
        }


        /// <summary>
        /// Starting path of file system tree. Virtual path with slashes, e.g. 
        /// </summary>
        public string StartingPath
        {
            get
            {
                return mStartingPath;
            }
            set
            {
                mStartingPath = value;
            }
        }


        /// <summary>
        /// Value of selected file or folder. 
        /// </summary>
        public string SelectedPath
        {
            get
            {
                return mSelectedPath;
            }
            set
            {
                mSelectedPath = value;
            }
        }


        /// <summary>
        /// Path in filesystem tree selected by default.
        /// </summary>
        public string DefaultPath
        {
            get
            {
                return mDefaultPath;
            }
            set
            {
                mDefaultPath = value;
            }
        }


        /// <summary>
        /// String containing list of allowed folders.
        /// </summary>
        public string AllowedFolders
        {
            get
            {
                return mAllowedFolders;
            }
            set
            {
                mAllowedFolders = value;
            }
        }


        /// <summary>
        /// String containing list of excluded folders.
        /// </summary>
        public string ExcludedFolders
        {
            get
            {
                return mExcludedFolders;
            }
            set
            {
                mExcludedFolders = value;
            }
        }


        /// <summary>
        /// String containing list of allowed extensions separated by semicolon.
        /// </summary>
        public string AllowedExtensions
        {
            get
            {
                return mAllowedExtensions;
            }
            set
            {
                mAllowedExtensions = value;
            }
        }


        /// <summary>
        /// Extension allowed for creation of a new text file
        /// </summary>
        public string NewTextFileExtension
        {
            get;
            set;
        }


        /// <summary>
        /// String containing list of excluded extensions.
        /// </summary>
        public string ExcludedExtensions
        {
            get
            {
                return mExcludedExtensions;
            }
            set
            {
                mExcludedExtensions = value;
            }
        }


        /// <summary>
        /// Determines if folder mode is turned on or if file should be selected.
        /// </summary>
        public bool ShowFolders
        {
            get;
            set;
        }


        /// <summary>
        /// Determines if the selector should display zip folders.
        /// </summary>
        public bool AllowZipFolders
        {
            get;
            set;
        }


        /// <summary>
        /// Client ID of the textbox to which the value will be set.
        /// </summary>
        public string EditorClientID
        {
            get;
            set;
        }


        /// <summary>
        /// Width of the dialog.
        /// </summary>
        public int DialogWidth
        {
            get
            {
                return mDialogWidth;
            }
            set
            {
                mDialogWidth = value;
            }
        }


        /// <summary>
        /// Height of the dialog.
        /// </summary>
        public int DialogHeight
        {
            get
            {
                return mDialogHeight;
            }
            set
            {
                mDialogHeight = value;
            }
        }


        /// <summary>
        /// Indicates if dialog width/height are set as relative to the total width/height of the screen.
        /// </summary>
        public bool UseRelativeDimensions
        {
            get
            {
                return mUseRelativeDimensions;
            }
            set
            {
                mUseRelativeDimensions = value;
            }
        }


        /// <summary>
        /// Indicates if starting path can be located out of application directory.
        /// </summary>
        public bool AllowNonApplicationPath
        {
            get
            {
                return mAllowNonApplicationPath;
            }
            set
            {
                mAllowNonApplicationPath = value;
            }
        }
        
        #endregion
    }
}