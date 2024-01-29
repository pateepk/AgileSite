using System;
using System.ComponentModel;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Represents parameters required to insert a new widget.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InsertWidgetParameters
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
        /// Identifier of the widget zone.
        /// </summary>
        public string ZoneIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Identifier of the widget type to insert.
        /// </summary>
        public Guid WidgetTypeIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// The zero-based index within the zone at which widget should be inserted.
        /// </summary>
        public int Index
        {
            get;
            set;
        }
    }
}
