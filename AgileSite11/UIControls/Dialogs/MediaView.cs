using System.Data;
using System.Web.UI.WebControls;

using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// View controls base class.
    /// </summary>
    public abstract class MediaView : CMSUserControl
    {
        #region "Private variables"

        private int mTotalRecords = -1;
        private MediaSourceEnum mSourceType = MediaSourceEnum.Content;
        
        private SelectableContentEnum mSelectableContent = SelectableContentEnum.AllContent;
        private SelectablePageTypeEnum mSelectablePageTypes = SelectablePageTypeEnum.All;

        private DialogViewModeEnum mViewMode = DialogViewModeEnum.ListView;

        private int mCurrentSiteId = -1;


        /// <summary>
        /// Event fired when some action takes place.
        /// </summary>
        public CommandEventHandler OnAction;


        /// <summary>
        /// Delegate for the event fired when UniGrid's page changed.
        /// </summary>
        public delegate void OnListReloadRequired();

        /// <summary>
        /// Event fired when UniGrid's page changed.
        /// </summary>
        public event OnListReloadRequired ListReloadRequired;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Path to the displayed media folder
        /// </summary>
        public string FolderPath
        {
            get;
            set;
        }


        /// <summary>
        /// Data source which is set to the inner view control.
        /// </summary>
        public DataSet DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Sets or gets total records count.
        /// </summary>
        public int TotalRecords
        {
            get
            {
                return mTotalRecords;
            }
            set
            {
                mTotalRecords = value;
            }
        }


        /// <summary>
        /// Gets current dialog configuration.
        /// </summary>
        public DialogConfiguration Config
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the files which are displayed.
        /// </summary>
        public MediaSourceEnum SourceType
        {
            get
            {
                return mSourceType;
            }
            set
            {
                mSourceType = value;
            }
        }


        /// <summary>
        /// Type of the content which can be selected.
        /// </summary>
        public SelectableContentEnum SelectableContent
        {
            get
            {
                return mSelectableContent;
            }
            set
            {
                mSelectableContent = value;
            }
        }


        /// <summary>
        /// Page types which can be selected
        /// </summary>
        public SelectablePageTypeEnum SelectablePageTypes
        {
            get
            {
                return mSelectablePageTypes;
            }
            set
            {
                mSelectablePageTypes = value;
            }
        }


        /// <summary>
        /// Selected item.
        /// </summary>
        public MediaItem SelectedItem
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a view mode used to display files.
        /// </summary>
        public virtual DialogViewModeEnum ViewMode
        {
            get
            {
                return mViewMode;
            }
            set
            {
                mViewMode = value;
            }
        }


        /// <summary>
        /// Indicates whether permanent URLs should be used for the currently processed file type.
        /// </summary>
        public bool UsePermanentUrls
        {
            get;
            set;
        }


        /// <summary>
        /// Height of attachment.
        /// </summary>
        public int ResizeToHeight
        {
            get;
            set;
        }


        /// <summary>
        /// Width of attachment.
        /// </summary>
        public int ResizeToWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Max side size of attachment.
        /// </summary>
        public int ResizeToMaxSideSize
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raised when specified action occurs.
        /// </summary>
        public void RaiseOnAction(string action, object argument)
        {
            if (OnAction != null)
            {
                OnAction(this, new CommandEventArgs(action, argument));
            }
        }


        /// <summary>
        /// Raised when list view reload is required.
        /// </summary>
        public void RaiseListReloadRequired()
        {
            if (ListReloadRequired != null)
            {
                ListReloadRequired();
            }
        }


        /// <summary>
        /// Returns current site ID respecting link document option.
        /// </summary>
        protected int GetCurrentSiteId()
        {
            // Get the site ID from the request cache
            if (mCurrentSiteId < 0)
            {
                // No document no site
                if (Config != null)
                {
                    int documentId = Config.AttachmentDocumentID;
                    if (documentId > 0)
                    {
                        // Get the site for the document
                        SiteInfo si = TreePathUtils.GetDocumentSite(documentId);
                        if (si != null)
                        {
                            mCurrentSiteId = si.SiteID;
                        }
                    }
                }

                // Use current site ID if not set
                if (mCurrentSiteId < 0)
                {
                    mCurrentSiteId = SiteContext.CurrentSiteID;
                }
            }

            return mCurrentSiteId;
        }

        #endregion
    }
}