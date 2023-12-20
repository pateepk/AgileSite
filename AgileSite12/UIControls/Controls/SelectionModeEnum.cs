using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Selection modes enumeration.
    /// </summary>
    public enum SelectionModeEnum
    {
        /// <summary>
        /// Textbox for single selection.
        /// </summary>
        SingleTextBox = 0,

        /// <summary>
        /// Dropdown list for single selection.
        /// </summary>
        SingleDropDownList = 1,

        /// <summary>
        /// Unigrid for multiple selection.
        /// </summary>
        Multiple = 2,

        /// <summary>
        /// Multiple selection with textbox.
        /// </summary>
        MultipleTextBox = 3,

        /// <summary>
        /// Single button.
        /// </summary>
        SingleButton = 4,

        /// <summary>
        /// Multiple button.
        /// </summary>
        MultipleButton = 5,

        /// <summary>
        /// Single selection with result displayed via object transformation.
        /// </summary>
        SingleTransformation = 6
    }
}