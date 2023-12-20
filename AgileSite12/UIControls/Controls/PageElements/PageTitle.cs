using System;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// PageTitle control base class.
    /// </summary>
    public class PageTitle : CMSUserControl
    {
        #region "Private variables"

        private bool mShowFullScreenButton = true;
        private bool mShowCloseButton = true;
        private string mTitleText = string.Empty;
        private string mTitleInformation = string.Empty;
        private string mAlternateText = string.Empty;
        private int mHeadingLevel = 2;
        private bool? mHideBreadcrumbs;

        #endregion


        #region "Properties"
        
        /// <summary>
        /// Indicates if breadcrumbs should be visible.
        /// </summary>
        public bool HideBreadcrumbs
        {
            get
            {
                if (mHideBreadcrumbs == null)
                {
                    // Display breadcrumbs only in dialog mode or on live site
                    var isDialog = IsDialog || QueryHelper.GetBoolean("dialogmode", false);
                    mHideBreadcrumbs = (!isDialog || !HideTitle) && !IsLiveSite;
                }

                return mHideBreadcrumbs.Value;
            }
            set
            {
                mHideBreadcrumbs = value;
            }
        }


        /// <summary>
        /// Text of title label.
        /// </summary>
        public virtual string TitleText
        {
            get
            {
                return mTitleText;
            }
            set
            {
                mTitleText = value;
            }
        }


        /// <summary>
        /// Title information label.
        /// </summary>
        public virtual string TitleInformation
        {
            get
            {
                return mTitleInformation;
            }
            set
            {
                mTitleInformation = value;
            }
        }


        /// <summary>
        /// Alternative text of image.
        /// </summary>
        public virtual string AlternateText
        {
            get
            {
                return mAlternateText;
            }
            set
            {
                mAlternateText = value;
            }
        }


        /// <summary>
        /// Title CSS class.
        /// </summary>
        public virtual string TitleCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Help topic name.
        /// </summary>
        public virtual string HelpTopicName
        {
            get;
            set;
        }


        /// <summary>
        /// Help name to identify the help within the JavaScript.
        /// </summary>
        public virtual string HelpName
        {
            get;
            set;
        }


        /// <summary>
        /// Help icon name.
        /// </summary>
        public virtual string HelpIconName
        {
            get;
            set;
        }


        /// <summary>
        /// Help icon URL.
        /// </summary>
        public virtual string HelpIconUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the control is used in a dialog.
        /// </summary>
        public virtual bool IsDialog
        {
            get;
            set;
        }


        /// <summary>
        /// Hide title 
        /// </summary>
        public virtual bool HideTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets heading element level. By default generates h2 title.
        /// </summary>
        public int HeadingLevel
        {
            get
            {
                return mHeadingLevel;
            }
            set
            {
                mHeadingLevel = value;
            }
        }
        

        /// <summary>
        /// Placeholder after image and title text.
        /// </summary>
        public virtual PlaceHolder RightPlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Breadcrumbs control
        /// </summary>
        public virtual Breadcrumbs Breadcrumbs
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Determines whether to show dialog fullscreen button. By default is true.
        /// </summary>
        public bool ShowFullScreenButton
        {
            get
            {
                return mShowFullScreenButton;
            }
            set
            {
                mShowFullScreenButton = value;
            }
        }


        /// <summary>
        /// Determines whether to show dialog close button. By default is true.
        /// </summary>
        public bool ShowCloseButton
        {
            get
            {
                return mShowCloseButton;
            }
            set
            {
                mShowCloseButton = value;
            }
        }


        /// <summary>
        /// Indicates if text in title should be wrapped.
        /// </summary>
        public bool Wrap
        {
            get;
            set;
        }

        #endregion
    }
}