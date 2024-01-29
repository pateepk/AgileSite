using System;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSMapProperties class.
    /// </summary>
    public class CMSMapProperties
    {
        #region "Variables"

        private bool mEnableKeyboardShortcuts = true;
        private bool mEnableMapDragging = true;
        private bool mShowNavigationControl = true;
        private bool mShowStreetViewControl = true;
        private bool mShowZoomControl = true;
        private bool mShowScaleControl = true;
        private bool mShowMapTypeControl = true;
        private string mHeight = "400";
        private string mWidth = "400";
        private int mScale = 1;
        private int mZoomScale = 1;
        private string mMapId = "";
        private string mLatitudeField = "";
        private string mLocation = "";
        private string mToolTipField = "";
        private string mToolTip = "";
        private string mContent = "";
        private string mLongitudeField = "";
        private string mLocationField = "";
        private string mIconField = "";
        private string mMapKey = "";
        private string mMapType = "";
        private string mIconURL = "";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether the keyboard shortcuts are enabled.
        /// </summary>
        public bool EnableKeyboardShortcuts
        {
            get
            {
                return mEnableKeyboardShortcuts;
            }
            set
            {
                mEnableKeyboardShortcuts = value;
            }
        }


        /// <summary>
        /// Gets Map name.
        /// </summary>
        public string MapName
        {
            get
            {
                return MapId + "_map";
            }
        }


        /// <summary>
        /// Gets or sets Map ID.
        /// </summary>
        public string MapId
        {
            get
            {
                return mMapId;
            }
            set
            {
                mMapId = value;
            }
        }


        /// <summary>
        /// Gets or sets Page.
        /// </summary>
        public Page Page
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the user can drag the map with the mouse.
        /// </summary>
        public bool EnableMapDragging
        {
            get
            {
                return mEnableMapDragging;
            }
            set
            {
                mEnableMapDragging = value;
            }
        }


        /// <summary>
        /// Gets or sets the height of the map.
        /// </summary>
        public string Height
        {
            get
            {
                return mHeight;
            }
            set
            {
                mHeight = value;
            }
        }


        /// <summary>
        /// Gets or sets the width of the map.
        /// </summary>
        public string Width
        {
            get
            {
                return mWidth;
            }
            set
            {
                mWidth = value;
            }
        }


        /// <summary>
        /// Gets or sets the source latitude field.
        /// </summary>
        public string LatitudeField
        {
            get
            {
                return mLatitudeField;
            }
            set
            {
                mLatitudeField = value;
            }
        }


        /// <summary>
        /// Gets or sets the source longitude field.
        /// </summary>
        public string LongitudeField
        {
            get
            {
                return mLongitudeField;
            }
            set
            {
                mLongitudeField = value;
            }
        }


        /// <summary>
        /// Gets or sets the source icon field.
        /// </summary>
        public string IconField
        {
            get
            {
                return mIconField;
            }
            set
            {
                mIconField = value;
            }
        }


        /// <summary>
        /// Gets or sets the address field.
        /// </summary>
        public string LocationField
        {
            get
            {
                return mLocationField;
            }
            set
            {
                mLocationField = value;
            }
        }


        /// <summary>
        /// Gets or sets the default location of the center of the map or location of single marker in detail mode.
        /// </summary>
        public string Location
        {
            get
            {
                return mLocation;
            }
            set
            {
                mLocation = value;
            }
        }


        /// <summary>
        /// Gets or sets the latitude of of the center of the map or latitude of single marker in detail mode.
        /// </summary>
        public double? Latitude
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the longitude of of the center of the map or longitude of single marker in detail mode.
        /// </summary>
        public double? Longitude
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the map key.
        /// </summary>
        public string MapKey
        {
            get
            {
                return mMapKey;
            }
            set
            {
                mMapKey = value;
            }
        }


        /// <summary>
        /// Gets or sets the initial map type.
        /// </summary>
        public string MapType
        {
            get
            {
                return mMapType;
            }
            set
            {
                mMapType = value;
            }
        }


        /// <summary>
        /// The Zoom control type.
        /// </summary>
        public int ZoomControlType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the scale of the map.
        /// </summary>
        public int Scale
        {
            get
            {
                return mScale;
            }
            set
            {
                mScale = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether NavigationControl is displayed.
        /// </summary>
        public bool ShowNavigationControl
        {
            get
            {
                return mShowNavigationControl;
            }
            set
            {
                mShowNavigationControl = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether Zoom control is displayed.
        /// </summary>
        public bool ShowZoomControl
        {
            get
            {
                return mShowZoomControl;
            }
            set
            {
                mShowZoomControl = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether Street view control is displayed.
        /// </summary>
        public bool ShowStreetViewControl
        {
            get
            {
                return mShowStreetViewControl;
            }
            set
            {
                mShowStreetViewControl = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether ScaleControl is displayed.
        /// </summary>
        public bool ShowScaleControl
        {
            get
            {
                return mShowScaleControl;
            }
            set
            {
                mShowScaleControl = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether MapTypeControl is displayed.
        /// </summary>
        public bool ShowMapTypeControl
        {
            get
            {
                return mShowMapTypeControl;
            }
            set
            {
                mShowMapTypeControl = value;
            }
        }


        /// <summary>
        /// Gets or sets the tool tip text field (filed for markers tool tip text).
        /// </summary>
        public string ToolTipField
        {
            get
            {
                return mToolTipField;
            }
            set
            {
                mToolTipField = value;
            }
        }


        /// <summary>
        /// Gets or sets the tool tip text for single marker in detail mode.
        /// </summary>
        public string ToolTip
        {
            get
            {
                return mToolTip;
            }
            set
            {
                mToolTip = value;
            }
        }


        /// <summary>
        /// Gets or sets the content text for single marker in detail mode.
        /// </summary>
        public string Content
        {
            get
            {
                if (mContent.Contains("~/"))
                {
                    mContent = HTMLHelper.ResolveUrls(mContent, null);
                }
                return mContent;
            }
            set
            {
                mContent = value;
            }
        }


        /// <summary>
        /// Gets or sets the icon URL.
        /// </summary>
        public string IconURL
        {
            get
            {
                return mIconURL;
            }
            set
            {
                mIconURL = value;
            }
        }


        /// <summary>
        /// Gets or sets the scale of the map when zoomed (after marker click event).
        /// </summary>
        public int ZoomScale
        {
            get
            {
                return mZoomScale;
            }
            set
            {
                mZoomScale = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether server processing is enabled.
        /// </summary>
        public bool EnableServerProcessing
        {
            get;
            set;
        }

        #endregion
    }
}