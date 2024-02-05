using System.Xml.Linq;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid pager configuration.
    /// </summary>
    public class UniGridPagerConfig : AbstractConfiguration
    {
        #region "Properties"

        /// <summary>
        /// True if the pager should be displayed.
        /// </summary>
        public bool? DisplayPager
        {
            get;
            set;
        }


        /// <summary>
        /// Defines the default amount of rows displayed on one UniGrid page. 
        /// The value must be one of the options offered by the page size selection drop-down list. These values are defined by the PageSizeOptions key.
        /// </summary>
        public int DefaultPageSize
        {
            get;
            set;
        }


        /// <summary>
        /// This setting can be used to override the default values offered by the page size selection drop-down list.
        /// Values must be separated by commas.
        /// The ##ALL## macro can be used as a value to indicate that all rows should be displayed.
        /// The default value is “25,50,100,##ALL##”.
        /// </summary>
        public string PageSizeOptions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether a drop-down list used for direct page selection should be displayed.
        /// </summary>
        public bool? ShowDirectPageControl
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the buttons that link to the first and last page should be displayed.
        /// </summary>
        public bool? ShowFirstLastButtons
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the page size selection drop-down list should be displayed.
        /// </summary>
        public bool? ShowPageSize
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the buttons that link to the previous and next page page should be displayed.
        /// </summary>
        public bool? ShowPreviousNextButtons
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the buttons that link to the next group of page links should be displayed.
        /// </summary>
        public bool? ShowPreviousNextPageGroup
        {
            get;
            set;
        }


        /// <summary>
        /// Determines the amount of displayed page links in one group.
        /// </summary>
        public int GroupSize
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public UniGridPagerConfig()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">Pager configuration XML element</param>
        public UniGridPagerConfig(XElement element)
            : this()
        {
            DisplayPager = GetOptionKeyBoolValue(element, "DisplayPager", null);
            PageSizeOptions = GetOptionKeyValue<string>(element, "PageSizeOptions", null);
            ShowDirectPageControl = GetOptionKeyBoolValue(element, "ShowDirectPageControl", null);
            ShowFirstLastButtons = GetOptionKeyBoolValue(element, "ShowFirstLastButtons", null);
            ShowPageSize = GetOptionKeyBoolValue(element, "ShowPageSize", null);
            ShowPreviousNextButtons = GetOptionKeyBoolValue(element, "ShowPreviousNextButtons", null);
            ShowPreviousNextPageGroup = GetOptionKeyBoolValue(element, "ShowPreviousNextPageGroup", null);
            GroupSize = GetOptionKeyValue(element, "VisiblePages", 0);
            DefaultPageSize = GetOptionKeyValue(element, "DefaultPageSize", 0);
        }
        
        #endregion
    }
}