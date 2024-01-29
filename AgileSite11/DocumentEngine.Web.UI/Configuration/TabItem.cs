using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI.Configuration
{
    /// <summary>
    /// Tab control item.
    /// </summary>
    public class TabItem : NavigationItem
    {
        #region "Properties"

        /// <summary>
        /// Position of a tab.
        /// </summary>
        public int Index
        {
            get;
            set;
        }


        /// <summary>
        /// The left image of the item.
        /// </summary>
        public string LeftItemImage
        {
            get;
            set;
        }


        /// <summary>
        /// The middle image of the item.
        /// </summary>
        public string MiddleItemImage
        {
            get;
            set;
        }


        /// <summary>
        /// The right image of the item.
        /// </summary>
        public string RightItemImage
        {
            get;
            set;
        }


        /// <summary>
        /// Style attribute of the tab.
        /// </summary>
        public string ItemStyle
        {
            get;
            set;
        }


        /// <summary>
        /// Name of tab.
        /// </summary>
        public string TabName
        {
            get;
            set;
        }


        /// <summary>
        /// Style attribute of the tab.
        /// </summary>
        public string AlternatingCssSuffix
        {
            get;
            set;
        }


        /// <summary>
        /// Alternative text of the image.
        /// </summary>
        public string ImageAlternativeText
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true, the tab is set as selected if displayed
        /// </summary>
        public bool Selected
        {
            get;
            set;
        }


        /// <summary>
        /// Suppresses default behavior when onclick event is raised.
        /// </summary>
        public bool SuppressDefaultOnClientClick
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TabItem()
        {
            Index = -1;
        }


        /// <summary>
        /// Legacy constructor for compatibility.
        /// </summary>
        /// <param name="tabArray">Array to initialize object from.</param>
        private TabItem(string[] tabArray)
            : this()
        {
            if (tabArray.Length > 0)
            {
                Text = tabArray[0];
            }
            if (tabArray.Length > 1)
            {
                OnClientClick = tabArray[1];
            }
            if (tabArray.Length > 2)
            {
                RedirectUrl = tabArray[2];
            }
            if (tabArray.Length > 3)
            {
                Tooltip = tabArray[3];
            }
            if (tabArray.Length > 4)
            {
                LeftItemImage = tabArray[4];
            }
            if (tabArray.Length > 5)
            {
                MiddleItemImage = tabArray[5];
            }
            if (tabArray.Length > 6)
            {
                RightItemImage = tabArray[6];
            }
            if (tabArray.Length > 7)
            {
                ItemStyle = tabArray[7];
            }
            if (tabArray.Length > 8)
            {
                AlternatingCssSuffix = tabArray[8];
            }
            if (tabArray.Length > 9)
            {
                CssClass = tabArray[9];
            }
            if (tabArray.Length > 10)
            {
                ImageAlternativeText = tabArray[10];
            }
            if (tabArray.Length > 11)
            {
                SuppressDefaultOnClientClick = ValidationHelper.GetBoolean(tabArray[11], false);
            }
        }

        #endregion


        #region "Compatibility methods"

        /// <summary>
        /// Creates new tab item object from array values. 
        /// </summary>
        /// <param name="tabArray">Array</param>
        public static TabItem New(string[] tabArray)
        {
            return (tabArray != null) ? new TabItem(tabArray) : null;
        }


        /// <summary>
        /// Converts object to legacy array.
        /// </summary>
        public string[] ToArray()
        {
            string[] tabArray = new string[12];
            tabArray[0] = Text;
            tabArray[1] = OnClientClick;
            tabArray[2] = RedirectUrl;
            tabArray[3] = Tooltip;
            tabArray[4] = LeftItemImage;
            tabArray[5] = MiddleItemImage;
            tabArray[6] = RightItemImage;
            tabArray[7] = ItemStyle;
            tabArray[8] = AlternatingCssSuffix;
            tabArray[9] = CssClass;
            tabArray[10] = ImageAlternativeText;
            tabArray[11] = SuppressDefaultOnClientClick.ToString();
            return tabArray;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks TabItem for emptiness.
        /// </summary>
        /// <returns>TRUE if all fields are equal to null.</returns>
        public bool IsEmpty()
        {
            return (Text == null) && (OnClientClick == null) && (RedirectUrl == null) && (Tooltip == null) && 
                   (LeftItemImage == null) && (MiddleItemImage == null) && (RightItemImage == null) &&
                   (ItemStyle == null) && (AlternatingCssSuffix == null) &&
                   (CssClass == null) && (ImageAlternativeText == null);
        }

        #endregion
    }
}
