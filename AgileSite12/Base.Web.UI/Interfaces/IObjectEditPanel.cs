namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Represents the object edit panel control interface.
    /// </summary>
    public interface IObjectEditPanel
    {
        /// <summary>
        /// Gets the object manager.
        /// </summary>
        ICMSObjectManager AbstractObjectManager
        {
            get;
        }


        /// <summary>
        /// Gets the object edit menu.
        /// </summary>
        IObjectEditMenu AbstractObjectEditMenu
        {
            get;
        }


        /// <summary>
        /// Gets or sets the value that indicates if the control is used in the preview mode.
        /// </summary>
        bool PreviewMode
        {
            get;
            set;
        }
    }
}
