using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Represents the object manager control interface.
    /// </summary>
    public interface ICMSObjectManager
    {
        /// <summary>
        /// Indicates if the object locking panel should be showed.
        /// </summary>
        bool ShowPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates if the manager should register for common events (save, etc.).
        /// </summary>
        bool RegisterEvents
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates if the manager should render the client scripts.
        /// </summary>
        bool RenderScript
        {
            get;
            set;
        }


        /// <summary>
        /// On after action event handler.
        /// </summary>
        event EventHandler<SimpleObjectManagerEventArgs> OnAfterAction;
    }
}
