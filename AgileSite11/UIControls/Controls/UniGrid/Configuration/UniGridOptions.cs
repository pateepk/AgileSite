using System;
using System.Xml.Linq;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid options.
    /// </summary>
    public class UniGridOptions : AbstractConfiguration
    {
        #region "Properties"

        /// <summary>
        /// Indicates whether a filter should be displayed above the UniGrid. If the amount of displayed rows is lower than the value of the FilterLimit key, the filter will be hidden despite this setting.
        /// </summary>
        public bool DisplayFilter
        {
            get;
            set;
        }


        /// <summary>
        /// Determines the minimum amount of rows that must be displayed in the UniGrid before a filter is shown. The default value is read from the CMSDefaultListingFilterLimit web.config key.
        /// </summary>
        public int FilterLimit
        {
            get;
            set;
        }


        /// <summary>
        /// Path to the custom filter that will be displayed instead of default filter.
        /// </summary>
        public string FilterPath
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether a column allowing the selection of rows should be displayed on the left of the UniGrid. This can be used to perform mass actions affecting multiple rows.
        /// The selected rows can be accessed through the SelectedItems property of the UniGrid.
        /// </summary>
        public bool ShowSelection
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the column used as an item in the array of selected rows which can be accessed through the SelectedItems property of the UniGrid. By default the first column in the data source is used.
        /// </summary>
        public string SelectionColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the UniGrid supports column sorting.
        /// </summary>
        public bool AllowSorting
        {
            get;
            set;
        }


        /// <summary>
        /// Determines if an arrow showing the sorting direction should be displayed next to the header of the column used for sorting.
        /// </summary>
        public bool ShowSortDirection
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public UniGridOptions()
        {
            FilterLimit = -1;
            ShowSortDirection = true;
            AllowSorting = true;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Option XML element</param>
        public UniGridOptions(XElement options)
            : this()
        {
            DisplayFilter = GetOptionKeyValue(options, "DisplayFilter", DisplayFilter);
            FilterLimit = GetOptionKeyValue(options, "FilterLimit", FilterLimit);
            AllowSorting = GetOptionKeyValue(options, "AllowSorting", AllowSorting);
            ShowSortDirection = GetOptionKeyValue(options, "ShowSortDirection", ShowSortDirection);
            ShowSelection = GetOptionKeyValue(options, "ShowSelection", false);
            SelectionColumn = GetOptionKeyValue(options, "SelectionColumn", String.Empty);
            FilterPath = GetOptionKeyValue(options, "FilterPath", String.Empty);
        }

        #endregion
    }
}