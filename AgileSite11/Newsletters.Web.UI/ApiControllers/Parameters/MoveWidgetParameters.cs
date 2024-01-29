using System;
using System.ComponentModel;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents parameters required to move an existing widget.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MoveWidgetParameters
    {
        /// <summary>
        /// Identifier of the issue.
        /// </summary>
        public int IssueIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Identifier of the new widget zone.
        /// </summary>
        public string ZoneIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Identifier of the widget instance.
        /// </summary>
        public Guid WidgetIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// The zero-based index specifying the new location of the widget within the widget zone.
        /// </summary>
        public int Index
        {
            get;
            set;
        }
    }
}
