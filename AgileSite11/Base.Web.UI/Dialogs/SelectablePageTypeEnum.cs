using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Defines which page types can be selected in the given dialog.
    /// </summary>
    public enum SelectablePageTypeEnum
    {
        /// <summary>
        /// All page types can be selected.
        /// </summary>
        All,

        /// <summary>
        /// Only standard pages can be selected. Content-only pages cannot be selected.
        /// </summary>
        Standard,

        /// <summary>
        /// Only content only pages can be selected. Standard pages cannot be selected.
        /// </summary>
        ContentOnly
    }
}
