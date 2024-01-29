using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Actions for buttons in button group
    /// </summary>
    public class CMSButtonGroupAction : CMSButtonAction
    {
        /// <summary>
        /// Gets or sets the CSS class that serves as icon for the button.
        /// Icon is by default rendered before text. Position of the icon can be overriden by
        /// using expression <see cref="CMSButton.ICON_PLACEMENT_MACRO"/> inside the text property.
        /// </summary>
        public string IconCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the icon button (without text) should be used.
        /// </summary>
        public bool UseIconButton
        {
            get;
            set;
        }
    }
}
