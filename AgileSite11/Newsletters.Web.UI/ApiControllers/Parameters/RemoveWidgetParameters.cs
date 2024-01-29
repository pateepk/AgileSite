using System;
using System.ComponentModel;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents parameters required to remove a widget.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RemoveWidgetParameters
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
        /// Identifier of the widget instance.
        /// </summary>
        public Guid WidgetIdentifier
        {
            get;
            set;
        }
    }
}
